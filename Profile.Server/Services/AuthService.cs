using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Profile.Server.Services;

public class AuthService(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<JWT> jwt) : IAuthService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly JWT _jwt = jwt.Value;

    public async Task<AuthDto> RegisterAsync(RegisterDto model)
    {
        if (await _userManager.FindByEmailAsync(model.Email) is not null)
            return new AuthDto { Message = "Email Is Already registered!;" };
        
        if (await _userManager.FindByNameAsync(model.Email) is not null)
            return new AuthDto { Message = "UserName Is Already registered!;" };

        User user = new()
        {
            UserName = model.UserName,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            var errors = string.Empty;

            foreach (var err in result.Errors)
            {
                errors += $"{err.Description},";
            }
            return new AuthDto { Message = errors };
        }
        var jwtSecurityToken = await CreateJwtToken(user);

        if (jwtSecurityToken is null)
            return new() { Message = "error jwtSecurityToken"};
        var addRoleUser = await _userManager.AddToRoleAsync(user, RoleNames.User);

        
        if (!addRoleUser.Succeeded)
            return new AuthDto { Message="The User Is Registered But Something went wrong in adding to role user" };

        return new()
        {
            Email = user.Email,
            //ExpiresOn = jwtSecurityToken.ValidTo,
            IsAuthenticated = true,
            Roles = new() { RoleNames.User},
            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            UserName = user.UserName,
        };

    }

    private async Task<JwtSecurityToken?> CreateJwtToken(User user)
    {
        if (user == null)
            return null;
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var role in roles)
            roleClaims.Add(new Claim("roles", role));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user?.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user?.Email ?? ""),
            new Claim("uid", user?.Id ?? "")
        }
        .Union(userClaims)
        .Union(roleClaims);

        SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes(_jwt.Key));
        SigningCredentials signingCredentials = new(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwtSecurityToken = new(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwt.DeurationInMin),
            signingCredentials: signingCredentials
            );

        return jwtSecurityToken;
    }
    
    public async Task<AuthDto> GetTokenAsync(TokenRequestDto model)
    {
        AuthDto authModel = new();

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            authModel.Message = "Email or password is incorrect";
            return authModel;
        }

        var jwtSecurityToken = await CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);

        
        authModel.IsAuthenticated = true;
        authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        //authModel.ExpiresOn = jwtSecurityToken.ValidTo;
        authModel.Email = user?.Email ?? "NON";
        authModel.UserName = user?.UserName ?? "NON";
        authModel.Roles = [.. rolesList];

        if (user.RefreshTokens.Any(e => e.IsActive))
        {
            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(e => e.IsActive);

            authModel.RefreshToken = activeRefreshToken?.Token;
            authModel.RefreshTokenExpiretion = activeRefreshToken.ExpiresOn;
        }
        else
        {
            var refreshToken = GenerateRefreshToken();
            authModel.RefreshToken = refreshToken.Token;
            authModel.RefreshTokenExpiretion = refreshToken.ExpiresOn;

            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);
        }


        return authModel;
    }
    public async Task<string> AddRoleAsync(AddRoleDto model)
    {
        var user = await _userManager.FindByIdAsync(model.UsrId);
        
        if (user == null || !await _roleManager.RoleExistsAsync(model.Role))
            return "Invalid Role or User Id";

        if (await _userManager.IsInRoleAsync(user, model.Role))
            return "User already assign to this role";

        var result = await _userManager.AddToRoleAsync(user, model.Role);
        if (result.Succeeded)
            return string.Empty;

        return "Some Went Wrong";
    }
    

    public async Task<AuthDto> RefreshTokenAsync(string token)
    {
        AuthDto authModel = new();

        var user = await _userManager.Users.SingleOrDefaultAsync(e => e.RefreshTokens.Any(u => u.Token == token));
        
        if (user == null)
        {
            authModel.Message = "Invalid token";
            return authModel;
        }
        var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshToken.IsActive)
        {
            authModel.Message = "Inactive token";
            return authModel;
        }

        // revoke to refresh toke 
        refreshToken.RevokeOn = DateTime.UtcNow;
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        var jwtToken = await CreateJwtToken(user);
        authModel.IsAuthenticated = true;
        authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        authModel.Email = user.Email;
        authModel.UserName = user.UserName;
        authModel.Roles = [.. (await _userManager.GetRolesAsync(user))];
        authModel.RefreshToken = newRefreshToken.Token;
        authModel.RefreshTokenExpiretion = newRefreshToken.ExpiresOn;

        return authModel;
    }
    private RefreshToken GenerateRefreshToken()
    {
        var rundomNumber = new byte[32];
        using RNGCryptoServiceProvider generator = new(rundomNumber);
        generator.GetBytes(rundomNumber);
        return new()
        {
            CreateOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(1),
            Token = Convert.ToBase64String(rundomNumber)
        };
    }
}

using Profile.Server.DTO;

namespace Profile.Server.IServices;
public interface IAuthService
{
    Task<AuthDto> RegisterAsync(RegisterDto model);
    Task<AuthDto> GetTokenAsync(TokenRequestDto model);
    Task<string> AddRoleAsync(AddRoleDto model);
    Task<AuthDto> RefreshTokenAsync(string token);
}

using System.Threading;

namespace Profile.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IUnitOfWork context) : ControllerBase
{
    private readonly IUnitOfWork _context = context;

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _context._authService.RegisterAsync(model);
        if (result is null || !string.IsNullOrEmpty(result.Message) )
            return BadRequest(result);

        return Ok(new { result.Token});
    }
    [HttpPost("Token")]
    public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestDto model)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);

        var result = await _context._authService.GetTokenAsync(model);
        if (result is null || !string.IsNullOrEmpty(result.Message))
            return BadRequest(result);

        if (!string.IsNullOrEmpty(result.RefreshToken)) 
            SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExpiretion);

        return Ok(result);
        
    }
    [HttpPost("AddToRole")]
    public async Task<IActionResult> AddToRoleAsync([FromBody] AddRoleDto model)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);

        var result = await _context._authService.AddRoleAsync(model); 
        if (!string.IsNullOrEmpty(result))
            return BadRequest(result);

        return Ok();
    }
    [HttpGet("GetRefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        
        var result = await _context._authService.RefreshTokenAsync(refreshToken);

        if (!result.IsAuthenticated)
            return BadRequest(result);


        SetRefreshTokenInCookies(result.RefreshToken, result.RefreshTokenExpiretion);
        
        return Ok(result);
    }
    private void SetRefreshTokenInCookies(string refreshToken, DateTime expires)
    {
        CookieOptions cookieOptions = new()
        {
            HttpOnly = true,
            Expires = expires.ToLocalTime()
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}

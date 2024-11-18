using System.Text.Json.Serialization;

namespace Profile.Server.DTO;

public class AuthDto
{
    public string? Message { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = default!;
    public string Token { get; set; } = string.Empty;
    //public DateTime ExpiresOn { get; set; }

    [JsonIgnore]
    public string? RefreshToken {  get; set; }  
    public DateTime RefreshTokenExpiretion { get; set; }

}

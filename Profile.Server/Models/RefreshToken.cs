namespace Profile.Server.Models;

[Owned]
public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public bool IsExpired => DateTime.UtcNow > ExpiresOn;
    public DateTime CreateOn { get; set; }
    public DateTime? RevokeOn { get; set; }
    public bool IsActive => RevokeOn == null && !IsExpired;

}

namespace Profile.Server.Models;

public class User : IdentityUser
{
    [MaxLength(250)]
    public string FirstName { get; set; } = default!;
    [MaxLength(250)]
    public string LastName { get; set; } = default!;
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = default!;
}

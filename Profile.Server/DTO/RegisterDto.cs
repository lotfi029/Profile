namespace Profile.Server.DTO;
public class RegisterDto
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;
    [StringLength(128)]
    public string Email { get; set; } = string.Empty;
    [StringLength(256)]
    public string Password { get; set; } = string.Empty;
}

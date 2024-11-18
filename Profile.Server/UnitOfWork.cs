
using Microsoft.Extensions.Options;

namespace Profile.Server;
public class UnitOfWork : IUnitOfWork
{
    public IAuthService _authService { get; }




    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IOptions<JWT> _jwt;
    private readonly ApplicationDbContext context;


    public UnitOfWork(
        ApplicationDbContext _context,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<JWT> jwt
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwt = jwt;
        context = _context;
    
        _authService = new AuthService(_userManager,_roleManager, _jwt);
    }

    public async Task<int> CommitChangeAsync()
        => await context.SaveChangesAsync();

    public void Dispose()
        => context.Dispose();
}

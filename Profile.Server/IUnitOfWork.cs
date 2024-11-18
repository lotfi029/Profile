namespace Profile.Server;

public interface IUnitOfWork : IDisposable
{
    IAuthService _authService { get; }
    Task<int> CommitChangeAsync();
}

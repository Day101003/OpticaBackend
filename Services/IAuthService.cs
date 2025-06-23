using System.Threading.Tasks;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto registerDto);
    Task<string> LoginAsync(LoginDto loginDto);
}

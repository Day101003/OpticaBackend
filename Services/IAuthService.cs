using ProyectoFinal.Models;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterDto registerDto);
    Task<LoginResponseDto> LoginAsync(LoginDto loginDto);  // Devuelve LoginResponseDto
}

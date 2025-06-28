using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Services;
using ProyectoFinal.Models;
using Microsoft.AspNetCore.Authorization;


namespace ProyectoFinal.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // El método LoginAsync devuelve un LoginResponseDto, no solo el token
                var loginResponse = await _authService.LoginAsync(loginDto);
                return Ok(loginResponse);  // Devuelve el objeto completo (Token y Role)
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("Invalid credentials");
            }
        }

        // Ruta para registro de usuario
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var token = await _authService.RegisterAsync(registerDto);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // Retorna el error si ya existe el email o algún otro error
            }
        }

        [Route("api/[controller]")]
        [ApiController]
        public class AdminController : ControllerBase
        {
            // Esta ruta solo será accesible para administradores
            [Authorize(Roles = "Admin")]
            [HttpGet("dashboard")]
            public IActionResult GetAdminDashboard()
            {
                return Ok("Bienvenido al panel de administración");
            }
        }
    }
}

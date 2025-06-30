using Microsoft.AspNetCore.Mvc;
using ProyectoFinal.Services;
using ProyectoFinal.Models;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoFinal.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IStringLocalizer<Messages> _localizer;

        public AuthController(IAuthService authService, IStringLocalizer<Messages> localizer)
        {
            _authService = authService;
            _localizer = localizer;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // ?? Llama al servicio que genera el token y devuelve rol
                var loginResponse = await _authService.LoginAsync(loginDto);

                // ? Devolver directamente el DTO para facilitar acceso en frontend
                return Ok(loginResponse); // { "token": "...", "role": "..." }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = _localizer["InvalidCredentials"] });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var token = await _authService.RegisterAsync(registerDto);
                return Ok(new
                {
                    message = _localizer["RegisterSuccess"],
                    token = token
                });
            }
            catch (Exception)
            {
                return BadRequest(new { message = _localizer["RegisterError"] });
            }
        }
    }

    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public AdminController(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("dashboard")]
        public IActionResult GetAdminDashboard()
        {
            return Ok(new { message = _localizer["LoginSuccess"] });
        }
    }
}
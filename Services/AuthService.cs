using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models;
using ProyectoFinal.Data;
using ProyectoFinal.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ProyectoFinal.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
                throw new Exception("El email ya está registrado.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var newUser = new Users
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = hashedPassword,
                Phone = registerDto.Phone,
                DateRegister = DateTime.Now,
                ImagePath = "assets/img/FotoPerfil/default.jpg",
                Role = Role.User  // Asignar rol de "User" por defecto
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return GenerateJwtToken(newUser);
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                throw new UnauthorizedAccessException("Credenciales inválidas");

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                Token = token,
                Role = user.Role.ToString()  // Retorna el rol del usuario
            };
        }

        private string GenerateJwtToken(Users user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())  // Agregar el rol al token
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? throw new Exception("JWT Secret no configurado"))
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

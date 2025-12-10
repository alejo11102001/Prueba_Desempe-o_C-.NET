using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TalentoPlus.API.Dtos;
using TalentoPlus.Core.Entities;
using TalentoPlus.Core.Interfaces;

namespace TalentoPlus.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IEmailService _emailService;

        public AuthController(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            IEmpleadoRepository empleadoRepository,
            IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _empleadoRepository = empleadoRepository;
            _emailService = emailService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Verificar si el usuario ya existe
            var userExists = await _userManager.FindByEmailAsync(dto.Email);
            if (userExists != null)
                return BadRequest(new { Message = "El correo ya está registrado." });

            // 2. Crear usuario de Identity (Login)
            var user = new IdentityUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // 3. Vincular con Empleado (Negocio)
            // Buscamos si el empleado ya existía (cargado por Excel)
            var empleado = await _empleadoRepository.GetByDocumentoAsync(dto.Documento);

            if (empleado == null)
            {
                // Si no existe, lo creamos (Autoregistro completo)
                empleado = new Empleado
                {
                    Documento = dto.Documento,
                    Nombres = dto.Nombres,
                    Apellidos = dto.Apellidos,
                    Email = dto.Email,
                    
                    Direccion = "Dirección pendiente", 
                    Telefono = "0000000",             
                    NivelEducativo = "No registrado", 
                    PerfilProfesional = "Perfil pendiente de completar", 
                    Salario = 0,
                    
                    Cargo = "Sin Asignar", // Valores por defecto
                    Departamento = "General",
                    UsuarioId = user.Id, // VINCULACIÓN IMPORTANTE
                    FechaIngreso = DateTime.UtcNow,
                    Estado = EstadoEmpleado.Activo
                };
                await _empleadoRepository.AddAsync(empleado);
            }
            else
            {
                // Si ya existía por el Excel, solo actualizamos el UsuarioId para que pueda entrar
                empleado.UsuarioId = user.Id;
                empleado.Email = dto.Email; // Aseguramos que el email coincida
                await _empleadoRepository.UpdateAsync(empleado);
            }

            // 4. Enviar Correo Real
            try 
            {
                string mensaje = $"Hola {dto.Nombres},<br>Tu registro en TalentoPlus fue exitoso. Ya puedes acceder a la App.";
                await _emailService.SendEmailAsync(dto.Email, "Bienvenido a TalentoPlus", mensaje);
            }
            catch
            {
                // No detenemos el registro si falla el correo, pero podríamos loguearlo
                // Para la prueba, basta con que funcione la lógica principal
            }

            return Ok(new { Message = "Usuario registrado exitosamente" });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Validar credenciales
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Unauthorized(new { Message = "Credenciales inválidas" });
            }

            // 2. Generar el Token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2), // Dura 2 horas
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok(new { Token = jwtToken });
        }
    }
}
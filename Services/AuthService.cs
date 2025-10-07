using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechSolutionsAPI.Models.DTOs.User;
using TechSolutionsAPI.Services.Interfaces;

namespace TechSolutionsAPI.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación.
    /// Maneja login y generación de tokens JWT.
    /// </summary>
    public class AuthService : IAuthService
    {
        // ═══════════════════════════════════════════════════════════
        // INYECCIÓN DE DEPENDENCIAS
        // ═══════════════════════════════════════════════════════════

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor: Recibe las dependencias que necesita este servicio.
        /// </summary>
        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;       
            _configuration = configuration; 
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO 1: LOGIN (AUTENTICACIÓN)
        // ═══════════════════════════════════════════════════════════

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            try
            {
                // PASO 1: Buscar usuario
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

                // DEBUG: Imprimir en consola
                Console.WriteLine($"[DEBUG] Email buscado: {request.Email}");
                Console.WriteLine($"[DEBUG] Usuario encontrado: {user != null}");

                if (user == null)
                {
                    Console.WriteLine("[DEBUG] Usuario NO encontrado en BD");
                    return new LoginResponseDTO
                    {
                        Success = false,
                        Message = "Email o contraseña incorrectos",
                        Token = null,
                        User = null
                    };
                }

                // DEBUG: Ver el hash almacenado
                Console.WriteLine($"[DEBUG] PasswordHash en BD: {user.PasswordHash}");
                Console.WriteLine($"[DEBUG] Password recibido: {request.Password}");

                // PASO 2: Verificar contraseña
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

                Console.WriteLine($"[DEBUG] Contraseña válida: {isPasswordValid}");

                if (!isPasswordValid)
                {
                    return new LoginResponseDTO
                    {
                        Success = false,
                        Message = "Email o contraseña incorrectos",
                        Token = null,
                        User = null
                    };
                }

                // Si llegó aquí, todo OK
                string token = GenerateJwtToken(user.UserId, user.Email, user.Role);

                return new LoginResponseDTO
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = token,
                    User = new UserDTO
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role
                    }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] ERROR: {ex.Message}");
                return new LoginResponseDTO
                {
                    Success = false,
                    Message = $"Error al iniciar sesión: {ex.Message}",
                    Token = null,
                    User = null
                };
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO 2: GENERAR TOKEN JWT
        // ═══════════════════════════════════════════════════════════

        public string GenerateJwtToken(int userId, string email, string role)
        {
            // ─────────────────────────────────────────────────────
            // PASO 1: Leer la configuración JWT de appsettings.json
            // ─────────────────────────────────────────────────────
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]);

            // ─────────────────────────────────────────────────────
            // PASO 2: Crear los CLAIMS (datos que irán dentro del token)
            // ─────────────────────────────────────────────────────
            var claims = new List<Claim>
            {
                // NameIdentifier: El ID del usuario (muy importante)
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                
                // Email: El email del usuario
                new Claim(ClaimTypes.Email, email),
                
                // Role: El rol del usuario (Admin, User, etc.)
                new Claim(ClaimTypes.Role, role),
                
                // Name: Nombre para identificar al usuario
                new Claim(ClaimTypes.Name, email),
                
                // JTI: ID único del token (para identificarlo)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // ¿QUÉ ES UN CLAIM?
            // - Es una "afirmación" sobre el usuario
            // - "Este usuario tiene ID 5"
            // - "Este usuario es Admin"
            // - Van DENTRO del token, el frontend puede leerlos

            // ─────────────────────────────────────────────────────
            // PASO 3: Crear la clave de seguridad
            // ─────────────────────────────────────────────────────
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // SymmetricSecurityKey:
            // - Convierte tu SecretKey (string) en bytes
            // - Se usa para FIRMAR el token
            // - Solo quien tiene esta clave puede crear/validar tokens

            // ─────────────────────────────────────────────────────
            // PASO 4: Crear las credenciales de firma
            // ─────────────────────────────────────────────────────
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // SigningCredentials:
            // - Define CÓMO se firma el token
            // - HmacSha256: Algoritmo de encriptación
            // - Garantiza que el token no ha sido alterado

            // ─────────────────────────────────────────────────────
            // PASO 5: Crear el token JWT
            // ─────────────────────────────────────────────────────
            var token = new JwtSecurityToken(
                issuer: issuer,                           // Quién emite el token
                audience: audience,                       // Para quién es el token
                claims: claims,                           // Los datos que lleva dentro
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes), // Cuándo expira
                signingCredentials: credentials           // Cómo se firma
            );

            // ─────────────────────────────────────────────────────
            // PASO 6: Convertir el token a string
            // ─────────────────────────────────────────────────────
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);

            // WriteToken:
            // - Convierte el objeto JwtSecurityToken en un string
            // - Ese string es lo que envías al frontend
            // - Ejemplo: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        }
    }
}

/* 
════════════════════════════════════════════════════════════════════════
RESUMEN DE AUTHSERVICE:
════════════════════════════════════════════════════════════════════════

MÉTODO LoginAsync:
1. Busca usuario por email
2. Verifica que exista y esté activo
3. Compara contraseña con BCrypt
4. Si todo OK, genera token JWT
5. Devuelve respuesta con token y datos del usuario

MÉTODO GenerateJwtToken:
1. Lee configuración de appsettings.json
2. Crea claims (datos del usuario)
3. Crea clave de seguridad
4. Firma el token con esa clave
5. Devuelve el token como string

¿CÓMO FUNCIONA JWT?
- Es como una "tarjeta de identificación digital"
- El servidor la firma (nadie más puede crear una válida)
- El frontend la guarda y la envía en cada petición
- El servidor verifica la firma para confirmar que es válida
- NO necesitas guardar sesiones en el servidor (stateless)

════════════════════════════════════════════════════════════════════════
*/
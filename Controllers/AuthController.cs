using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechSolutionsAPI.Models.DTOs;
using TechSolutionsAPI.Models.DTOs.User;
using TechSolutionsAPI.Services.Interfaces;

namespace TechSolutionsAPI.Controllers
{
    /// <summary>
    /// Controlador de autenticación.
    /// Maneja el login de usuarios administradores.
    /// </summary>
    [ApiController]                          // Indica que es un controlador de API
    [Route("api/[controller]")]              // Ruta: /api/auth
    public class AuthController : ControllerBase
    {
        // ═══════════════════════════════════════════════════════════
        // INYECCIÓN DE DEPENDENCIAS
        // ═══════════════════════════════════════════════════════════

        private readonly IAuthService _authService;

        /// <summary>
        /// Constructor: Recibe el servicio de autenticación.
        /// .NET Core lo inyecta automáticamente.
        /// </summary>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ═══════════════════════════════════════════════════════════
        // ENDPOINT: POST /api/auth/login
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Endpoint para iniciar sesión.
        /// Recibe email y password, devuelve token JWT si es válido.
        /// </summary>
        /// <param name="request">Objeto con email y password</param>
        /// <returns>Respuesta con token JWT y datos del usuario</returns>
        [HttpPost("login")]                 
        [AllowAnonymous]                    
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            // [FromBody]:
            // - Indica que los datos vienen en el BODY de la petición HTTP
            // - En formato JSON
            // - ASP.NET Core lo deserializa automáticamente a LoginRequestDTO

            // ─────────────────────────────────────────────────────
            //  modelo (datos correctos)
            // ─────────────────────────────────────────────────────
            if (!ModelState.IsValid)
            {
                // ModelState.IsValid:
                // - Verifica las Data Annotations del DTO
                // - [Required], [EmailAddress], [MinLength], etc.
                // - Si algo está mal, devuelve false

                return BadRequest(new
                {
                    success = false,
                    message = "Datos inválidos",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            try
            {
                // ─────────────────────────────────────────────────────
                //  servicio de autenticación
                // ─────────────────────────────────────────────────────
                var response = await _authService.LoginAsync(request);

                // El servicio:
                // 1. Busca el usuario en la BD
                // 2. Verifica la contraseña
                // 3. Genera el token JWT
                // 4. Devuelve LoginResponseDTO

                // ─────────────────────────────────────────────────────
                //  Verificar si el login fue exitoso
                // ─────────────────────────────────────────────────────
                if (!response.Success)
                {
                    // Login falló (email o password incorrectos)
                    return Unauthorized(new
                    {
                        success = false,
                        message = response.Message
                    });

                    // Unauthorized:
                    // - Código HTTP 401 (Unauthorized)
                    // - Indica que las credenciales son inválidas
                }

                // ─────────────────────────────────────────────────────
                // Login exitoso, devolver token
                // ─────────────────────────────────────────────────────
                return Ok(new
                {
                    success = true,
                    message = response.Message,
                    token = response.Token,
                    user = response.User
                });

            }
            catch (Exception ex)
            {
                // ─────────────────────────────────────────────────────
                // MANEJO DE ERRORES INESPERADOS
                // ─────────────────────────────────────────────────────
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = ex.Message
                });

                // StatusCode(500):
                // - Código HTTP 500 (Internal Server Error)
                // - Indica que hubo un error en el servidor
                // - Puede ser error de BD, red, etc.
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ENDPOINT OPCIONAL: Validar token (si lo necesitas)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Verifica si el token JWT es válido.
        /// Útil para que el frontend verifique si la sesión sigue activa.
        /// </summary>
        [HttpGet("validate")]
        [Authorize]  
        public IActionResult ValidateToken()
        {
            // Si llegó hasta aquí, el token es válido
            // (el middleware de autenticación ya lo verificó)

            return Ok(new
            {
                success = true,
                message = "Token válido",
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            });

            // User:
            // - Es una propiedad del controlador
            // - Contiene los Claims del token JWT
            // - Puedes extraer información del usuario autenticado
        }
    }
}

/* 
════════════════════════════════════════════════════════════════════════
EXPLICACIÓN DETALLADA - AUTHCONTROLLER:
════════════════════════════════════════════════════════════════════════

ESTRUCTURA DE UN CONTROLADOR:
1. Atributos de clase: [ApiController], [Route]
2. Inyección de dependencias en el constructor
3. Métodos = Endpoints HTTP

ENDPOINT POST /api/auth/login:

Flujo completo:
1. Frontend envía: { "email": "admin@...", "password": "..." }
2. ASP.NET deserializa a LoginRequestDTO
3. Valida ModelState (Data Annotations)
4. Llama a _authService.LoginAsync()
5. Si OK: Devuelve 200 con token
6. Si falla: Devuelve 401 Unauthorized

CÓDIGOS HTTP MÁS COMUNES:
- 200 OK: Todo bien
- 201 Created: Recurso creado exitosamente
- 400 Bad Request: Datos inválidos
- 401 Unauthorized: Sin autenticación o credenciales incorrectas
- 403 Forbidden: Sin permisos (autenticado pero no autorizado)
- 404 Not Found: Recurso no existe
- 500 Internal Server Error: Error en el servidor

ATRIBUTOS:
- [HttpPost("login")]: Define método HTTP y ruta adicional
- [FromBody]: Los datos vienen en el body JSON
- [Authorize]: Requiere token JWT válido
- [AllowAnonymous]: NO requiere token

IActionResult:
- Tipo de retorno flexible para controladores
- Permite devolver diferentes códigos HTTP
- Ok(), BadRequest(), NotFound(), etc.

════════════════════════════════════════════════════════════════════════
*/
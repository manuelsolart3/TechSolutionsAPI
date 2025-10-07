using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechSolutionsAPI.Models.DTOs;
using TechSolutionsAPI.Models.DTOs.Service;
using TechSolutionsAPI.Services.Interfaces;

namespace TechSolutionsAPI.Controllers
{
    /// <summary>
    /// Controlador de servicios (CRUD completo).
    /// Maneja todas las operaciones sobre el catálogo de servicios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] 
    public class ServicesController : ControllerBase
    {
        // ═══════════════════════════════════════════════════════════
        // INYECCIÓN DE DEPENDENCIAS
        // ═══════════════════════════════════════════════════════════

        private readonly IServiceManagementService _serviceManagement;

        public ServicesController(IServiceManagementService serviceManagement)
        {
            _serviceManagement = serviceManagement;
        }

        // ═══════════════════════════════════════════════════════════
        // ENDPOINT 1: GET /api/services (Obtener todos)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene todos los servicios activos del catálogo.
        /// Endpoint público (no requiere autenticación).
        /// </summary>
        [HttpGet]
        [AllowAnonymous]  
        public async Task<IActionResult> GetAllServices()
        {
            try
            {
                var services = await _serviceManagement.GetAllServicesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Servicios obtenidos exitosamente",
                    data = services,
                    count = services.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener servicios",
                    error = ex.Message
                });
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ENDPOINT 2: GET /api/services/{id} (Obtener uno)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Obtiene un servicio específico por su ID.
        /// Endpoint público.
        /// </summary>
        /// <param name="id">ID del servicio</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceById(int id)
        {
            // {id}:
            // - Es un parámetro de ruta
            // - Ejemplo: GET /api/services/5
            // - ASP.NET lo mapea automáticamente al parámetro 'id'

            try
            {
                var service = await _serviceManagement.GetServiceByIdAsync(id);

                if (service == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Servicio con ID {id} no encontrado"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Servicio encontrado",
                    data = service
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al obtener el servicio",
                    error = ex.Message
                });
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ENDPOINT 3: POST /api/services (Crear nuevo)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un nuevo servicio en el catálogo.
        /// REQUIERE AUTENTICACIÓN (solo admin).
        /// </summary>
        /// <param name="dto">Datos del servicio a crear</param>
        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> CreateService([FromBody] ServiceDTO dto)
        {

            if (!ModelState.IsValid)
            {
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
                // Obtener el ID del usuario del token JWT
                // ─────────────────────────────────────────────────────
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

                // User:
                // - Es una propiedad del controlador
                // - Contiene los Claims del token JWT
                // - FindFirst busca el claim específico

                if (userIdClaim == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "No se pudo identificar al usuario"
                    });
                }

                int userId = int.Parse(userIdClaim.Value);

                // ─────────────────────────────────────────────────────
                //  Crear el servicio
                // ─────────────────────────────────────────────────────
                var createdService = await _serviceManagement.CreateServiceAsync(dto, userId);

                // ─────────────────────────────────────────────────────
                // Devolver respuesta 201 Created
                // ─────────────────────────────────────────────────────
                return CreatedAtAction(
                    nameof(GetServiceById),          
                    new { id = createdService.ServiceId }, 
                    new
                    {
                        success = true,
                        message = "Servicio creado exitosamente",
                        data = createdService
                    }
                );

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al crear el servicio",
                    error = ex.Message
                });
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ENDPOINT 4: PUT /api/services/{id} (Actualizar)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Actualiza un servicio existente.
        /// REQUIERE AUTENTICACIÓN (solo admin).
        /// </summary>
        /// <param name="id">ID del servicio a actualizar</param>
        /// <param name="dto">Nuevos datos del servicio</param>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServiceDTO dto)
        {

            if (!ModelState.IsValid)
            {
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
                // Actualizar el servicio
                // ─────────────────────────────────────────────────────
                var updatedService = await _serviceManagement.UpdateServiceAsync(id, dto);

                if (updatedService == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Servicio con ID {id} no encontrado"
                    });
                }

                // ─────────────────────────────────────────────────────
                // Devolver servicio actualizado
                // ─────────────────────────────────────────────────────
                return Ok(new
                {
                    success = true,
                    message = "Servicio actualizado exitosamente",
                    data = updatedService
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al actualizar el servicio",
                    error = ex.Message
                });
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ENDPOINT 5: DELETE /api/services/{id} (Eliminar)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Elimina lógicamente un servicio (soft delete).
        /// REQUIERE AUTENTICACIÓN (solo admin).
        /// </summary>
        /// <param name="id">ID del servicio a eliminar</param>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                // ─────────────────────────────────────────────────────
                // Eliminar el servicio (soft delete)
                // ─────────────────────────────────────────────────────
                bool deleted = await _serviceManagement.DeleteServiceAsync(id);

                if (!deleted)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Servicio con ID {id} no encontrado"
                    });
                }

                // ─────────────────────────────────────────────────────
                // Devolver confirmación
                // ─────────────────────────────────────────────────────
                return Ok(new
                {
                    success = true,
                    message = "Servicio eliminado exitosamente"
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al eliminar el servicio",
                    error = ex.Message
                });
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ENDPOINT 6: GET /api/services/search 
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Busca servicios por nombre o categoría.
        /// Endpoint público.
        /// </summary>
        /// <param name="term">Término de búsqueda</param>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchServices([FromQuery] string term)
        {
            // [FromQuery]:
            // - Los datos vienen en la query string
            // - Ejemplo: /api/services/search?term=desarrollo

            try
            {
                var services = await _serviceManagement.SearchServicesAsync(term);

                return Ok(new
                {
                    success = true,
                    message = "Búsqueda completada",
                    data = services,
                    count = services.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al buscar servicios",
                    error = ex.Message
                });
            }
        }
    }
}

/* 
════════════════════════════════════════════════════════════════════════
RESUMEN - SERVICESCONTROLLER:
════════════════════════════════════════════════════════════════════════

ENDPOINTS CREADOS:

1. GET /api/services
   - Público (sin token)
   - Devuelve todos los servicios activos

2. GET /api/services/{id}
   - Público
   - Devuelve un servicio específico

3. POST /api/services
   - PROTEGIDO (requiere token)
   - Crea un nuevo servicio
   - Devuelve 201 Created

4. PUT /api/services/{id}
   - PROTEGIDO
   - Actualiza un servicio existente
   - Devuelve 200 OK

5. DELETE /api/services/{id}
   - PROTEGIDO
   - Elimina lógicamente (soft delete)
   - Devuelve 200 OK

6. GET /api/services/search?term=...
   - Público
   - Busca servicios por nombre o categoría

CÓDIGOS HTTP USADOS:
- 200 OK: Operación exitosa
- 201 Created: Recurso creado
- 400 Bad Request: Datos inválidos
- 401 Unauthorized: Sin token o token inválido
- 404 Not Found: Recurso no existe
- 500 Internal Server Error: Error del servidor

SEGURIDAD:
- [Authorize]: Solo usuarios con token válido
- User.FindFirst(): Extrae datos del token JWT
- ModelState.IsValid: Valida Data Annotations

CONVENCIONES REST:
- GET: Leer datos (no modifica)
- POST: Crear nuevos recursos
- PUT: Actualizar recursos existentes
- DELETE: Eliminar recursos

════════════════════════════════════════════════════════════════════════
*/
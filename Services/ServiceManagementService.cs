using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TechSolutionsAPI.Models.DTOs;
using TechSolutionsAPI.Models.DTOs.Service;
using TechSolutionsAPI.Models.Entities;
using TechSolutionsAPI.Services.Interfaces;

namespace TechSolutionsAPI.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de servicios (CRUD completo).
    /// Maneja todas las operaciones relacionadas con los servicios del catálogo.
    /// </summary>
    public class ServiceManagementService : IServiceManagementService
    {
        // ═══════════════════════════════════════════════════════════
        // INYECCIÓN DE DEPENDENCIAS
        // ═══════════════════════════════════════════════════════════

        private readonly ApplicationDbContext _context;

          public ServiceManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO 1: OBTENER TODOS LOS SERVICIOS
        // ═══════════════════════════════════════════════════════════

        public async Task<List<ServiceDTO>> GetAllServicesAsync()
        {
            try
            {
                // ─────────────────────────────────────────────────────
                // PASO 1: Consultar servicios activos de la BD
                // ─────────────────────────────────────────────────────
                var services = await _context.Services
                    .Where(s => s.IsActive)             
                    .OrderByDescending(s => s.CreatedAt) 
                    .ToListAsync();                     

                // LINQ (Language Integrated Query):
                // - Where: Filtra registros (como SQL WHERE)
                // - OrderByDescending: Ordena de mayor a menor
                // - ToListAsync: Ejecuta la consulta y devuelve lista

                // ─────────────────────────────────────────────────────
                // PASO 2: Convertir entidades a DTOs
                // ─────────────────────────────────────────────────────
                var serviceDTOs = new List<ServiceDTO>();

                foreach (var service in services)
                {
                    serviceDTOs.Add(MapToDTO(service));
                }

                // ¿Por qué convertir a DTO?
                // - Las entidades tienen relaciones y propiedades internas
                // - Los DTOs son "objetos limpios" para enviar al frontend
                // - Features se convierte de JSON string a List<string>

                return serviceDTOs;
            }
            catch (Exception ex)
            {
                // Manejar errores de BD
                throw new Exception($"Error al obtener servicios: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO 2: OBTENER UN SERVICIO POR ID
        // ═══════════════════════════════════════════════════════════

        public async Task<ServiceDTO> GetServiceByIdAsync(int id)
        {
            try
            {
                // ─────────────────────────────────────────────────────
                // Buscar servicio por ID
                // ─────────────────────────────────────────────────────
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == id && s.IsActive);

                // FirstOrDefaultAsync:
                // - Busca el primer registro que cumple la condición
                // - Si no existe, devuelve NULL

                // ─────────────────────────────────────────────────────
                // Si no existe, devolver null
                // ─────────────────────────────────────────────────────
                if (service == null)
                    return null;

                // ─────────────────────────────────────────────────────
                // Convertir a DTO y devolver
                // ─────────────────────────────────────────────────────
                return MapToDTO(service);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener servicio: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO 3: CREAR UN NUEVO SERVICIO
        // ═══════════════════════════════════════════════════════════

        public async Task<ServiceDTO> CreateServiceAsync(ServiceDTO dto, int createdBy)
        {
            try
            {
                // ─────────────────────────────────────────────────────
                // PASO 1: Convertir Features de List a JSON string
                // ─────────────────────────────────────────────────────
                string featuresJson = JsonSerializer.Serialize(dto.Features);

                // ¿Por qué JSON?
                // - En la BD, Features es un string (no puede ser lista directamente)
                // - JsonSerializer.Serialize convierte List<string> a JSON
                // - Ejemplo: ["Feature 1", "Feature 2"] → '["Feature 1","Feature 2"]'

                // ─────────────────────────────────────────────────────
                // PASO 2: Crear la entidad Service
                // ─────────────────────────────────────────────────────
                var service = new Service
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Category = dto.Category,
                    Stock = dto.Stock,
                    InPromotion = dto.InPromotion,
                    DiscountPercent = dto.DiscountPercent,
                    ImageUrl = dto.ImageUrl,
                    Features = featuresJson,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = createdBy    
                };

                // ─────────────────────────────────────────────────────
                // PASO 3: Agregar a la BD
                // ─────────────────────────────────────────────────────
                _context.Services.Add(service);

                // Add:
                // - Marca el objeto para ser insertado
                // - NO lo guarda todavía en la BD
                // - Es como "preparar" la operación

                // ─────────────────────────────────────────────────────
                // PASO 4: Guardar cambios en la BD
                // ─────────────────────────────────────────────────────
                await _context.SaveChangesAsync();

                // SaveChangesAsync:
                // - AQUÍ se ejecuta el INSERT en la BD
                // - Es async porque la BD puede tardar
                // - Devuelve el número de registros afectados

                // ─────────────────────────────────────────────────────
                // PASO 5: Convertir a DTO y devolver
                // ─────────────────────────────────────────────────────
                return MapToDTO(service);

                // Ahora 'service' tiene su ServiceId asignado por la BD
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear servicio: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO 4: ACTUALIZAR UN SERVICIO EXISTENTE
        // ═══════════════════════════════════════════════════════════

        public async Task<ServiceDTO> UpdateServiceAsync(int id, ServiceDTO dto)
        {
            try
            {
                // ─────────────────────────────────────────────────────
                // PASO 1: Buscar el servicio existente
                // ─────────────────────────────────────────────────────
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == id && s.IsActive);

                if (service == null)
                    return null; // No existe, devolver null

                // ─────────────────────────────────────────────────────
                // PASO 2: Actualizar las propiedades
                // ─────────────────────────────────────────────────────
                service.Name = dto.Name;
                service.Description = dto.Description;
                service.Price = dto.Price;
                service.Category = dto.Category;
                service.Stock = dto.Stock;
                service.InPromotion = dto.InPromotion;
                service.DiscountPercent = dto.DiscountPercent;
                service.ImageUrl = dto.ImageUrl;
                service.Features = JsonSerializer.Serialize(dto.Features);

                // Entity Framework detecta automáticamente los cambios
                // No necesitas hacer _context.Services.Update(service)

                // ─────────────────────────────────────────────────────
                // PASO 3: Guardar cambios
                // ─────────────────────────────────────────────────────
                await _context.SaveChangesAsync();

                // SaveChangesAsync:
                // - Ejecuta UPDATE en la BD
                // - Solo actualiza las columnas que cambiaron

                // ─────────────────────────────────────────────────────
                // PASO 4: Devolver el servicio actualizado
                // ─────────────────────────────────────────────────────
                return MapToDTO(service);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar servicio: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO 5: ELIMINAR UN SERVICIO (SOFT DELETE)
        // ═══════════════════════════════════════════════════════════

        public async Task<bool> DeleteServiceAsync(int id)
        {
            try
            {
                // ─────────────────────────────────────────────────────
                // PASO 1: Buscar el servicio
                // ─────────────────────────────────────────────────────
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == id);

                if (service == null)
                    return false; 

                // ─────────────────────────────────────────────────────
                // PASO 2: Marcar como inactivo (NO eliminar físicamente)
                // ─────────────────────────────────────────────────────
                service.IsActive = false;
                
                // SOFT DELETE:
                // - NO ejecutamos _context.Services.Remove(service)
                // - Solo cambiamos IsActive a false
                // - El registro sigue en la BD, pero no se muestra

                // ¿Por qué Soft Delete?
                // - Mantener historial
                // - Poder recuperar si fue un error
                // - Auditoría y trazabilidad

                // ─────────────────────────────────────────────────────
                // PASO 3: Guardar cambios
                // ─────────────────────────────────────────────────────
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar servicio: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO 6: BUSCAR SERVICIOS (OPCIONAL)
        // ═══════════════════════════════════════════════════════════

        public async Task<List<ServiceDTO>> SearchServicesAsync(string searchTerm)
        {
            try
            {
                // ─────────────────────────────────────────────────────
                // Si el término está vacío, devolver todos
                // ─────────────────────────────────────────────────────
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllServicesAsync();

                // ─────────────────────────────────────────────────────
                // Buscar en nombre o categoría
                // ─────────────────────────────────────────────────────
                var services = await _context.Services
                    .Where(s => s.IsActive &&
                           (s.Name.Contains(searchTerm) ||
                            s.Category.Contains(searchTerm)))
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                // Contains:
                // - Busca el término DENTRO del texto (como SQL LIKE)
                // - Ejemplo: searchTerm="Web" encontrará "Desarrollo Web"

                // ─────────────────────────────────────────────────────
                // Convertir a DTOs
                // ─────────────────────────────────────────────────────
                var serviceDTOs = new List<ServiceDTO>();
                foreach (var service in services)
                {
                    serviceDTOs.Add(MapToDTO(service));
                }

                return serviceDTOs;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar servicios: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MÉTODO HELPER: MAPEAR ENTITY A DTO
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Convierte una entidad Service a ServiceDTO.
        /// Extrae Features de JSON string a List.
        /// </summary>
        private ServiceDTO MapToDTO(Service service)
        {
            // ─────────────────────────────────────────────────────
            // Deserializar Features de JSON a List
            // ─────────────────────────────────────────────────────
            List<string> featuresList = new List<string>();

            if (!string.IsNullOrEmpty(service.Features))
            {
                try
                {
                    featuresList = JsonSerializer.Deserialize<List<string>>(service.Features);
                }
                catch
                {
                    // Si el JSON está mal formado, dejar lista vacía
                    featuresList = new List<string>();
                }
            }

            // JsonSerializer.Deserialize:
            // - Convierte JSON string a objeto C#
            // - '["Feature 1","Feature 2"]' → List<string>

            // ─────────────────────────────────────────────────────
            // Crear y devolver DTO
            // ─────────────────────────────────────────────────────
            return new ServiceDTO
            {
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Category = service.Category,
                Stock = service.Stock,
                InPromotion = service.InPromotion,
                DiscountPercent = service.DiscountPercent,
                ImageUrl = service.ImageUrl,
            };

            // ¿Por qué no incluir fechas ni CreatedBy?
            // - El frontend normalmente no los necesita
            // - Puedes agregarlos si quieres
        }
    }
}

/* 
════════════════════════════════════════════════════════════════════════
RESUMEN DE SERVICEMANAGEMENTSEVICE:
════════════════════════════════════════════════════════════════════════

OPERACIONES CRUD:

1. GetAllServicesAsync:
   - Trae todos los servicios activos
   - Los ordena por fecha (más recientes primero)
   - Convierte Features de JSON a List

2. GetServiceByIdAsync:
   - Busca UN servicio por ID
   - Devuelve null si no existe
   - Útil para mostrar detalles

3. CreateServiceAsync:
   - Crea un nuevo servicio
   - Convierte Features de List a JSON
   - Asigna CreatedBy (trazabilidad)
   - Devuelve el servicio con su nuevo ID

4. UpdateServiceAsync:
   - Actualiza un servicio existente
   - Solo modifica los campos que cambien
   - Actualiza UpdatedAt

5. DeleteServiceAsync:
   - NO borra físicamente
   - Solo marca IsActive = false
   - Permite recuperar datos

6. SearchServicesAsync:
   - Busca en nombre o categoría
   - Útil para filtros en el frontend

MÉTODO HELPER:
- MapToDTO: Convierte Entity → DTO
  * Deserializa Features de JSON
  * Limpia datos para el frontend

CONCEPTOS CLAVE:
- Entity vs DTO: Entity es para BD, DTO para frontend
- Async/Await: Operaciones de BD no bloquean el servidor
- LINQ: Lenguaje de consultas integrado en C#
- JsonSerializer: Convierte entre JSON y objetos C#
- Soft Delete: No borrar, solo marcar como inactivo

════════════════════════════════════════════════════════════════════════
*/
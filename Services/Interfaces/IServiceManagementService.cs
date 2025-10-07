using TechSolutionsAPI.Models.DTOs;
using TechSolutionsAPI.Models.DTOs.Service;

namespace TechSolutionsAPI.Services.Interfaces
{
    /// <summary>
    /// Interfaz que define el contrato para gestionar servicios (CRUD completo).
    /// Create, Read, Update, Delete + búsqueda.
    /// </summary>
    public interface IServiceManagementService
    {
        /// <summary>
        /// Obtiene todos los servicios activos del catálogo.
        /// </summary>
        /// <returns>Lista de servicios en formato DTO</returns>
        Task<List<ServiceDTO>> GetAllServicesAsync();

        /// <summary>
        /// Obtiene UN servicio específico por su ID.
        /// </summary>
        /// <param name="id">ID del servicio a buscar</param>
        /// <returns>Servicio encontrado o null si no existe</returns>
        Task<ServiceDTO> GetServiceByIdAsync(int id);

        /// <summary>
        /// Crea un nuevo servicio en el catálogo.
        /// </summary>
        /// <param name="dto">Datos del servicio a crear</param>
        /// <param name="createdBy">ID del usuario admin que lo crea</param>
        /// <returns>Servicio creado con su nuevo ID</returns>
        Task<ServiceDTO> CreateServiceAsync(ServiceDTO dto, int createdBy);

        /// <summary>
        /// Actualiza un servicio existente.
        /// </summary>
        /// <param name="id">ID del servicio a actualizar</param>
        /// <param name="dto">Nuevos datos del servicio</param>
        /// <returns>Servicio actualizado</returns>
        Task<ServiceDTO> UpdateServiceAsync(int id, ServiceDTO dto);

        /// <summary>
        /// Elimina lógicamente un servicio (soft delete).
        /// NO lo borra físicamente, solo marca IsActive = false.
        /// </summary>
        /// <param name="id">ID del servicio a eliminar</param>
        /// <returns>True si se eliminó correctamente, False si no existe</returns>
        Task<bool> DeleteServiceAsync(int id);

        /// <summary>
        /// Busca servicios por nombre o categoría (opcional).
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda</param>
        /// <returns>Lista de servicios que coinciden con la búsqueda</returns>
        Task<List<ServiceDTO>> SearchServicesAsync(string searchTerm);
    }
}


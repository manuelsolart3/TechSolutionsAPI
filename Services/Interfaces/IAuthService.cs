using TechSolutionsAPI.Models.DTOs;
using TechSolutionsAPI.Models.DTOs.User;

namespace TechSolutionsAPI.Services.Interfaces
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de autenticación.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Método para iniciar sesión.
        /// Recibe credenciales (email y password) y devuelve una respuesta con el token JWT.
        /// </summary>
        /// <param name="request">DTO con email y password del usuario</param>
        /// <returns>Respuesta con éxito/error, token JWT y datos del usuario</returns>
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);

        /// <summary>
        /// Método para generar un token JWT.
        /// Toma los datos del usuario y crea un token firmado que el frontend usará para autenticarse.
        /// </summary>
        /// <param name="userId">ID único del usuario</param>
        /// <param name="email">Email del usuario</param>
        /// <param name="role">Rol del usuario (ej: Admin)</param>
        /// <returns>String con el token JWT</returns>
        string GenerateJwtToken(int userId, string email, string role);
    }
}

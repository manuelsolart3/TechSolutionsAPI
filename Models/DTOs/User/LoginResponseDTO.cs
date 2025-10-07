using System.ComponentModel.DataAnnotations;
using TechSolutionsAPI.Models.Entities;

namespace TechSolutionsAPI.Models.DTOs.User;

public class LoginResponseDTO
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public UserDTO User { get; set; }
}

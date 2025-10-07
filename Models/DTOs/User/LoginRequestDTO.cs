using System.ComponentModel.DataAnnotations;

namespace TechSolutionsAPI.Models.DTOs.User;

public class LoginRequestDTO
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}

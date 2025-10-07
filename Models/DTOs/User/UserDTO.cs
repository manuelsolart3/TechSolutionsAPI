using TechSolutionsAPI.Models.Entities;

namespace TechSolutionsAPI.Models.DTOs.User;

public class UserDTO
{
    public int UserId { get; set; }
    public string Email{ get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
}

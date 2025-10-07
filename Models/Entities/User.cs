﻿using System.ComponentModel.DataAnnotations;

namespace TechSolutionsAPI.Models.Entities;

public class Users
{
    public int UserId { get; set; }
    public string Email { get; set; }
    
    public string PasswordHash { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
    public bool IsActive{ get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual ICollection<Service> Services { get; set; }
}

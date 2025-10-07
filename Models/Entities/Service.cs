using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechSolutionsAPI.Models.Entities;

public class Service
{
    public int ServiceId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public int Stock { get; set; }
    public bool InPromotion { get; set; } = false;
    public int DiscountPercent { get; set; } = 0;
    public string ImageUrl { get; set; }
    public string Features { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdateAt { get; set; }
    public int? CreatedBy { get; set; }

    public virtual Users Creator { get; set; }
}

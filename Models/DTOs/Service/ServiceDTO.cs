namespace TechSolutionsAPI.Models.DTOs.Service;

public class ServiceDTO
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
}

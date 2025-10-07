using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSolutionsAPI.Models.Entities;

namespace TechSolutionsAPI.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable(nameof(Service));
        builder.HasKey(s => s.ServiceId);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(255);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(s => s.Category).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Stock).HasDefaultValue(0);
        builder.Property(s => s.InPromotion).HasDefaultValue(false);
        builder.Property(s => s.DiscountPercent).HasDefaultValue(0);
        builder.Property(s => s.ImageUrl).HasMaxLength(500);
        builder.Property(s => s.Features).HasMaxLength(1000);
        builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("GETDATE()");
        builder.Property(s => s.UpdateAt).HasDefaultValueSql("GETDATE()");

        builder.HasOne(x => x.Creator)
             .WithMany()
             .HasForeignKey(x => x.CreatedBy);
    }
}

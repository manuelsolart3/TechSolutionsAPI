using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechSolutionsAPI.Models.Entities;

namespace TechSolutionsAPI.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.ToTable(nameof(Users));
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(255);
        builder.Property(u => u.Role).IsRequired().HasMaxLength(50).HasDefaultValue("Admin");
        builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
    }
}

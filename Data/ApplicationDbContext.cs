using Microsoft.EntityFrameworkCore;
using TechSolutionsAPI.Abstractions;
using TechSolutionsAPI.Models;
using TechSolutionsAPI.Models.Entities;

public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Users> Users { get; set; }
    public DbSet<Service> Services { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Users>().ToTable("Users");
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

       
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}

using LernDotnet.Models;
using Microsoft.EntityFrameworkCore;
 
namespace LernDotnet.Data;
 
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
 
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<AppUser> Users => Set<AppUser>();
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
 
    public override int SaveChanges()
    {
        TouchTimestamps();
        return base.SaveChanges();
    }
 
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        TouchTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
 
    private void TouchTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
 
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not LernDotnet.Models.IHasTimestamps entity)
                continue;
                
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = now;
                entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = now;
            }
        }
    }
}
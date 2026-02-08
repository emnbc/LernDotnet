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
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(x => x.Id);
 
            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);
 
            entity.Property(x => x.Content)
                .IsRequired();
 
            entity.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");
 
            entity.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");
        });
 
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
 
        foreach (var entry in ChangeTracker.Entries<Note>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
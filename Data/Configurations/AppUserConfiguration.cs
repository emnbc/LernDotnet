using LernDotnet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
 
namespace LernDotnet.Data.Configurations;
 
public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> entity)
    {
        entity.HasKey(x => x.Id);
 
        entity.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(320);
 
        entity.HasIndex(x => x.Email)
            .IsUnique();
 
        entity.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);
 
        entity.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);
 
        entity.Property(x => x.PasswordHash)
            .IsRequired();
 
        entity.Property(x => x.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");
 
        entity.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}
using LernDotnet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
 
namespace LernDotnet.Data.Configurations;
 
public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> entity)
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
    }
}
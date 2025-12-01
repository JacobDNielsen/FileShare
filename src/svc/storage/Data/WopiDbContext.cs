using Microsoft.EntityFrameworkCore;
using Storage.Models;

namespace Storage.Data;

public class WopiDbContext : DbContext
{
    public WopiDbContext(DbContextOptions<WopiDbContext> options) : base(options)
    {
    }

    public DbSet<FileMetadata> Files => Set<FileMetadata>(); //vi kan i resten af koden referere til Files.NavnPåTabel
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(e =>
        {
            e.ToTable("metadata");
            e.HasKey(f => f.Id);
            e.HasIndex(f => f.FileId).IsUnique(); //sørger for at FileId er unik og laver et index på den
            e.Property(f => f.BaseFileName).HasMaxLength(255).IsRequired();
            e.Property(f => f.FileId).HasMaxLength(255).IsRequired();
            e.Property(f => f.OwnerId).IsRequired();
            
        });

     
    }
}
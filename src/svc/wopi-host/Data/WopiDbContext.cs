using Microsoft.EntityFrameworkCore;
using WopiHost.Models;

namespace WopiHost.Data;

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
            e.Property(f => f.FileName).HasMaxLength(255).IsRequired();
        });
    }
}
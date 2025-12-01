using Microsoft.EntityFrameworkCore;
using Lock.Models;

namespace Lock.Data;

public class WopiDbContext : DbContext
{
    public WopiDbContext(DbContextOptions<WopiDbContext> options) : base(options)
    {
    }

   
    public DbSet<FileLock> FileLocks => Set<FileLock>();
  
    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<FileLock>(e =>
        {
            e.ToTable("filelocks");
            e.HasKey(fl => fl.Id);
            e.HasIndex(fl => fl.FileId).IsUnique(); //sørger for at FileId er unik og laver et index på den

            e.Property(fl => fl.LockId).HasMaxLength(255).IsRequired();
            e.Property(fl => fl.FileId).HasMaxLength(255).IsRequired();
            e.Property(fl => fl.CreatedAt).HasMaxLength(255).IsRequired();
            e.Property(fl => fl.ExpiresAt).HasMaxLength(255).IsRequired();
            e.Property(fl => fl.UpdatedAt).HasMaxLength(255).IsRequired();

        });

    }
}
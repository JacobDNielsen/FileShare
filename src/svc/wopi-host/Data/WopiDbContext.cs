using Microsoft.EntityFrameworkCore;
using WopiHost.Models;

namespace WopiHost.Data;

public class WopiDbContext : DbContext
{
    public WopiDbContext(DbContextOptions<WopiDbContext> options) : base(options)
    {
    }

    public DbSet<FileMetadata> Files => Set<FileMetadata>(); //vi kan i resten af koden referere til Files.NavnPåTabel
    public DbSet<FileLock> FileLocks => Set<FileLock>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(e =>
        {
            e.ToTable("metadata");
            e.HasKey(f => f.Id);
            e.HasIndex(f => f.FileId).IsUnique(); //sørger for at FileId er unik og laver et index på den
            e.Property(f => f.BaseFileName).HasMaxLength(255).IsRequired();
            e.Property(f => f.FileId).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<FileLock>(e =>
        {
            e.ToTable("filelocks");
            e.HasKey(fl => fl.Id);
            e.HasIndex(fl => fl.FileId).IsUnique(); //sørger for at FileId er unik og laver et index på den

            e.Property(fl => fl.LockId).HasMaxLength(255).IsRequired();
            e.Property(fl => fl.FileId).HasMaxLength(255).IsRequired();

            e.HasOne<FileMetadata>() //en filelock har en filmetadata
             .WithMany() //en filmetadata kan have mange filelocks
             .HasForeignKey(fl => fl.FileId) //foreign key i filelock er FileId
             .HasPrincipalKey(f => f.FileId) //primary key i filmetadata er File
             .OnDelete(DeleteBehavior.Cascade); //hvis en filmetadata slettes, så slettes alle dens filelocks også
        });

        modelBuilder.Entity<UserAccount>(e =>
        {
            e.ToTable("useraccounts");
            e.HasKey(ua => ua.Id);
            e.Property(ua => ua.UserName).HasMaxLength(64).IsRequired();
            e.Property(ua => ua.PasswordHash).HasMaxLength(512).IsRequired(); //currently plain text, but should be hashed
            e.Property(ua => ua.Email).HasMaxLength(256).IsRequired();

            e.HasIndex(ua => ua.UserName).IsUnique();
            e.HasIndex(ua => ua.Email).IsUnique();

        });
    }
}
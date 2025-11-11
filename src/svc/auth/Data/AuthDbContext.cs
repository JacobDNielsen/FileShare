using Microsoft.EntityFrameworkCore;
using Auth.Models;

namespace Auth.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(e =>
        {
            e.ToTable("useraccounts");
            e.HasKey(ua => ua.Id);
            e.Property(ua => ua.UserName).HasMaxLength(64).IsRequired();
            e.Property(ua => ua.PasswordHash).HasMaxLength(512).IsRequired();
            e.Property(ua => ua.Email).HasMaxLength(256).IsRequired();

            e.HasIndex(ua => ua.UserName).IsUnique();
            e.HasIndex(ua => ua.Email).IsUnique();

        });
    }
}
using DotnetAuthentication.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetAuthentication.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.EnsureCreated();
        SeedRoles();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "User" },
            new Role { Id = 2, Name = "Admin" }
        );
    }

    private void SeedRoles()
    {
        // Check if roles already exist to avoid duplicates
        if (!Roles.Any())
        {
            Roles.AddRange(
                new Role { Id = 1, Name = "User" },
                new Role { Id = 2, Name = "Admin" }
            );
            SaveChanges();
        }
    }
}
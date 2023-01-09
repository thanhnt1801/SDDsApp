using Microsoft.EntityFrameworkCore;
using System;
using UserService.Models;

namespace UserService.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolesHasPermissions> RolesHasPermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "ADMIN", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now},
                new Role { Id = 2, Name = "MEMBER", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now},
                new Role { Id = 3, Name = "EXPERT", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now}
                );

            modelBuilder.Entity<RolesHasPermissions>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });
        }
    }
}

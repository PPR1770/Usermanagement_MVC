using ChatApp.Models;
using ChatApp.Models.ChatModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ChatApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ FIX

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ FIX

            builder.Entity<Permission>().HasData(
                new Permission { Id = 1,  Name = "Users.View",         Module = "Users",       Action = "View" },
                new Permission { Id = 2,  Name = "Users.Create",       Module = "Users",       Action = "Create" },
                new Permission { Id = 3,  Name = "Users.Edit",         Module = "Users",       Action = "Edit" },
                new Permission { Id = 4,  Name = "Users.Delete",       Module = "Users",       Action = "Delete" },
                new Permission { Id = 5,  Name = "Roles.View",         Module = "Roles",       Action = "View" },
                new Permission { Id = 6,  Name = "Roles.Create",       Module = "Roles",       Action = "Create" },
                new Permission { Id = 7,  Name = "Roles.Edit",         Module = "Roles",       Action = "Edit" },
                new Permission { Id = 8,  Name = "Roles.Delete",       Module = "Roles",       Action = "Delete" },
                new Permission { Id = 9,  Name = "Permissions.View",   Module = "Permissions", Action = "View" },
                new Permission { Id = 10, Name = "Permissions.Manage", Module = "Permissions", Action = "Manage" }
            );
        }
    }
}

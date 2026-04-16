using Microsoft.AspNetCore.Identity;
using ChatApp.Models;

namespace ChatApp.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            string[] roles = { "Admin", "Manager", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role,
                        Description = $"{role} role"
                    });
            }

            if (await userManager.FindByEmailAsync("admin@app.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@app.com",
                    Email = "admin@app.com",
                    FirstName = "System",
                    LastName = "Admin",
                    IsActive = true,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}

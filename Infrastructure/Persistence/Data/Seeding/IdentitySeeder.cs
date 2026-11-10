using System;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Shared.Constants;

namespace Persistence.Data.Seeding;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            Roles.Admin,
            Roles.Teacher,
            Roles.Student
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedAdminAsync(UserManager<AppUser> userManager, string adminEmail, string adminPassword)
    {
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            await userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }
}

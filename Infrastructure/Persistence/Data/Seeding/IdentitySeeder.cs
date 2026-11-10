using System;
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
}

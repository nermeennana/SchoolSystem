using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DomainLayer.Contracts;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Constants;

namespace Persistence.Data.Seeding;

public class DataSeeding : IDataSeeding
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly SchoolDbContext _schoolDbContext;
    public DataSeeding(SchoolDbContext schoolDbContext,UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _schoolDbContext = schoolDbContext;
    }
    private async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            Roles.Admin,
            Roles.Teacher,
            Roles.Student
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private async Task SeedAdminAsync()
    {
        var adminEmail = _configuration["AdminUser:Email"];
        var adminPassword = _configuration["AdminUser:Password"];

        if(string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            throw new Exception("Admin user email or password is not configured.");
        }

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
        }
    }

    public async Task DataSeedAsync()
    {
        try
        {
            if ((await _schoolDbContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await _schoolDbContext.Database.MigrateAsync();
            }
            // if (!(await _schoolDbContext.Schools.AnyAsync()))
            // {
            //     // var todos = File.OpenRead(@"..\Infrastructure\perisistence\DataSeed\todos.json");
            //     var seedFilePath = Path.Combine(
            //         "..",
            //         "Infrastructure",
            //         "perisistence",
            //         "DataSeed",
            //         "schools.json");
            //     using var schoolData = File.OpenRead(seedFilePath);
                
            //     var options = new JsonSerializerOptions
            //     {
            //         Converters = { new JsonStringEnumConverter() }
            //     };
            //     var todosList = await JsonSerializer.DeserializeAsync<List<School>>(schoolData, options);
            //     if (todosList != null && todosList.Any())
            //     {
            //         await _schoolDbContext.Schools.AddRangeAsync(todosList);
            //     }
            // }
            await SeedRolesAsync();
            await SeedAdminAsync();
            await _schoolDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Data seeding failed: {ex.Message}");
            throw;
        }
    }
}

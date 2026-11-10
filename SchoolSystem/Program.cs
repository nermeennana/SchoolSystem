using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Data.Seeding;

namespace SchoolSystem;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<SchoolDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;

            options.User.RequireUniqueEmail = true;
        }).AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<SchoolDbContext>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await IdentitySeeder.SeedRolesAsync(roleManager);

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var adminEmail = builder.Configuration["AdminUser:Email"];
            var adminPassword = builder.Configuration["AdminUser:Password"];
            if(string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                throw new Exception("Admin user email or password is not configured.");
            }
            await IdentitySeeder.SeedAdminAsync(userManager, adminEmail, adminPassword);
        }

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
using System;
using DomainLayer.Contracts;

namespace SchoolSystem.Extensions;

public static class WebApplicationRegistration
{
    public static async Task SeedDataBaseAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dataSeeding = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
            await dataSeeding.DataSeedAsync();
        }
    }
}

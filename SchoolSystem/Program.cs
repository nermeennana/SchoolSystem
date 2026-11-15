using Persistence;
using ServiceLayer;
using SchoolSystem.Extensions;

namespace SchoolSystem;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

        var app = builder.Build();
        
        await app.SeedDataBaseAsync();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
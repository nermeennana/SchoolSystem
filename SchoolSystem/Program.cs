using Persistence;
using SchoolSystem.Extensions;

namespace SchoolSystem;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructureServices(builder.Configuration);

        var app = builder.Build();
        
        await app.SeedDataBaseAsync();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
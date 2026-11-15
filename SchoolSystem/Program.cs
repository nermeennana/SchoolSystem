using Persistence;
using ServiceLayer;
using SchoolSystem.Extensions;
using SchoolSystem.CustomMiddlewares;

namespace SchoolSystem;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

        var app = builder.Build();
        app.UseMiddleware<CustomExceptionHandlerMiddleware>();
        
        await app.SeedDataBaseAsync();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        app.Run();
    }
}
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace SchoolSystem;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<SchoolDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using specmatic_uuid_api.Data;
using specmatic_uuid_api.Models;
using specmatic_uuid_api.Models.Entity;

public class Program
{
    public static async Task Main(string[] args)
    {
        var app = BuildApp();
        await app.RunAsync();
    }

    public static WebApplication BuildApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(8080));

        // Add services
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                o => o.MapEnum<UuidType>("UuidType")
            )
        );

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new BadRequestObjectResult(new ErrorResponse
                {
                    TimeStamp = DateTime.UtcNow.ToString("o"),
                    Error = "Bad Request",
                    Message = string.Join(", ", errors)
                });
            };
        });

        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
             var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
             db.Database.EnsureCreated();
        }

        // Middleware
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}

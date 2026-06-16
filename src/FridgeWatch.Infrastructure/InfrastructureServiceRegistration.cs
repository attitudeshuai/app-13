using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Infrastructure.Data;
using FridgeWatch.Infrastructure.Repositories;
using FridgeWatch.Infrastructure.Services;
using FridgeWatch.Application.Interfaces;
using MySqlConnector;

namespace FridgeWatch.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var dbHost = configuration["DB_HOST"];
        var dbPort = configuration["DB_PORT"];
        var dbName = configuration["DB_NAME"];
        var dbUser = configuration["DB_USER"];
        var dbPassword = configuration["DB_PASSWORD"];

        string? connectionString;
        
        if (!string.IsNullOrEmpty(dbHost))
        {
            connectionString = $"Server={dbHost};Port={dbPort ?? "3306"};Database={dbName ?? "fridgewatch"};User={dbUser ?? "app_user"};Password={dbPassword ?? "app_pass"};";
        }
        else
        {
            connectionString = configuration.GetConnectionString("DefaultConnection") ??
                "Server=localhost;Port=13308;Database=fridgewatch;User=app_user;Password=app_pass;";
        }

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
        services.AddDbContext<FridgeWatchDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IHouseholdRepository, HouseholdRepository>();
        services.AddScoped<IHouseholdMemberRepository, HouseholdMemberRepository>();
        services.AddScoped<IFoodItemRepository, FoodItemRepository>();
        services.AddScoped<IExpiryAlertRepository, ExpiryAlertRepository>();
        services.AddScoped<IConsumptionRecordRepository, ConsumptionRecordRepository>();

        services.AddScoped<IJwtService, JwtService>();

        var jwtKey = configuration["Jwt:Key"] ?? "your-super-secret-key-for-jwt-token-must-be-at-least-32-chars";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "FridgeWatch";
        var jwtAudience = configuration["Jwt:Audience"] ?? "FridgeWatchUsers";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });

        return services;
    }
}

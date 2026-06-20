using Microsoft.Extensions.DependencyInjection;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Application.Services;
using FridgeWatch.Application.Mappings;
using FluentValidation;
using System.Reflection;

namespace FridgeWatch.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHouseholdService, HouseholdService>();
        services.AddScoped<IHouseholdMemberService, HouseholdMemberService>();
        services.AddScoped<IFoodItemService, FoodItemService>();
        services.AddScoped<IExpiryAlertService, ExpiryAlertService>();
        services.AddScoped<IConsumptionRecordService, ConsumptionRecordService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IShoppingListService, ShoppingListService>();
        services.AddScoped<IShareService, ShareService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IExpiryAlertSyncService, ExpiryAlertSyncService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}

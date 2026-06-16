using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace FridgeWatch.API.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FridgeWatch API",
                Version = "v1",
                Description = "家庭冰箱食材保质期预警系统 API 文档",
                Contact = new OpenApiContact
                {
                    Name = "FridgeWatch Team",
                    Email = "support@fridgewatch.com"
                }
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "请输入 JWT Token",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, new string[] { } }
            });

            var xmlFile = "FridgeWatch.API.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FridgeWatch API v1");
            c.RoutePrefix = string.Empty;
        });

        return app;
    }
}

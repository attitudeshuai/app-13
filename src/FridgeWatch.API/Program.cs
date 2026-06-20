using Microsoft.EntityFrameworkCore;
using FridgeWatch.API.Middleware;
using FridgeWatch.API.Extensions;
using FridgeWatch.Application;
using FridgeWatch.Infrastructure;
using FridgeWatch.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var retryCount = 5;
    var retryDelay = TimeSpan.FromSeconds(10);
    
    for (int i = 0; i < retryCount; i++)
    {
        try
        {
            var context = services.GetRequiredService<FridgeWatchDbContext>();
            context.Database.EnsureCreated();
            await DbSeeder.SeedDataAsync(context);
            Log.Information("数据库初始化完成");
            break;
        }
        catch (Exception ex)
        {
            if (i < retryCount - 1)
            {
                Log.Warning(ex, "数据库初始化失败，{RetryDelay}秒后进行第{RetryNumber}次重试...", retryDelay.TotalSeconds, i + 2);
                await Task.Delay(retryDelay);
            }
            else
            {
                Log.Error(ex, "数据库初始化失败，已重试{RetryCount}次", retryCount);
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

var uploadsPath = builder.Configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("FridgeWatch API 启动中...");
app.Run();

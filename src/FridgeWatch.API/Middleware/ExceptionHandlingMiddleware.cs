using System.Net;
using System.Text.Json;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Domain.Common;

namespace FridgeWatch.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发生未处理的异常");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>();

        switch (exception)
        {
            case BusinessException businessEx:
                context.Response.StatusCode = businessEx.Code;
                response.Code = businessEx.Code;
                response.Message = businessEx.Message;
                break;
            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Code = (int)HttpStatusCode.Forbidden;
                response.Message = exception.Message;
                break;
            case KeyNotFoundException:
            case FileNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Code = (int)HttpStatusCode.NotFound;
                response.Message = exception.Message;
                break;
            case ArgumentException:
            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Code = (int)HttpStatusCode.BadRequest;
                response.Message = exception.Message;
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Code = (int)HttpStatusCode.InternalServerError;
                response.Message = "服务器内部错误，请稍后重试";
                break;
        }

        var result = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(result);
    }
}

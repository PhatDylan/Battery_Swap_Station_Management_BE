using System.Net;
using System.Text.Json;
using BusinessObject.DTOs;
using Service.Exceptions;

namespace EV_Driver.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InvalidModelStateException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest, "400", ex.Errors);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.Unauthorized, "401");
        }
        catch (ValidationException ex)
        {
            await HandleExceptionAsync(context, ex, ex.StatusCode, ex.Code);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError, "500");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, HttpStatusCode statusCode,
        string code, object? content = null)
    {
        var response = new ResponseObject<object>
        {
            Content = content,
            Message = ex.Message,
            Code = code,
            Success = false
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
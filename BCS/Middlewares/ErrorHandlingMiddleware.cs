using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HMTSolution.BCS.Middlewares
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger _logger;

        public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var message = "Internal Server Error";
            var statusCode = System.Net.HttpStatusCode.InternalServerError;

            if (ex is UnauthorizedAccessException)
            {
                message = "Unauthorized Access. " + ex.Message;
                statusCode = System.Net.HttpStatusCode.Unauthorized;
            }
            else if (ex is ValidationException)
            {
                message = ex.Message;
                statusCode = System.Net.HttpStatusCode.BadRequest;
            }

            _logger?.LogError(ex, $"Message:{message} - StatusCode:{statusCode} - [Detail]{ex.Message}");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(message);
        }
    }

    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlingMiddleware(this IApplicationBuilder app)
        {
            return app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder => appBuilder.UseMiddleware<ErrorHandlingMiddleware>());
        }
        public static IServiceCollection AddErrorHandlingMiddleware(this IServiceCollection services)
        {
            return services.AddTransient<ErrorHandlingMiddleware>();
        }
    }
}

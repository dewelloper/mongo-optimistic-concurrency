using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HMTSolution.BCS.Middlewares
{
    public class RequestLoggerMiddleware : IMiddleware
    {
        private readonly ILogger<RequestLoggerMiddleware> _logger;

        public RequestLoggerMiddleware(ILogger<RequestLoggerMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var traceId = context.TraceIdentifier;

            var requestBodyStream = new MemoryStream();
            var originalRequestBody = context.Request.Body;
            var message = $"TraceId: {traceId} - {context.Request.Method}{Environment.NewLine}";

            try
            {
                await context.Request.Body.CopyToAsync(requestBodyStream);
            }
            catch (TaskCanceledException e)
            {
                message += "Task Cancelled";
                _logger.LogWarning(e, message);
                return;
            }

            requestBodyStream.Seek(0L, SeekOrigin.Begin);

            var requestBodyText = await new StreamReader(requestBodyStream).ReadToEndAsync();

            message += $"Request: {context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString}{Environment.NewLine}{requestBodyText}";

            _logger.LogInformation(message);

            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body = requestBodyStream;

            await next(context);

            context.Request.Body = originalRequestBody;
        }
    }

    public static class RequestLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLoggerMiddleware(this IApplicationBuilder app)
        {
            return app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder => appBuilder.UseMiddleware<RequestLoggerMiddleware>());
        }
        public static IServiceCollection AddRequestLoggerMiddleware(this IServiceCollection services)
        {
            return services.AddTransient<RequestLoggerMiddleware>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace HMTSolution.BCS.Middlewares
{
    public class ResponseLoggerMiddleware : IMiddleware
    {
        private readonly ILogger<ResponseLoggerMiddleware> _logger;
        private readonly ResponseLoggerMiddlewareOptions _middlewareOptions;

        public ResponseLoggerMiddleware(ILogger<ResponseLoggerMiddleware> logger, IOptions<ResponseLoggerMiddlewareOptions> options = null)
        {
            _logger = logger;
            _middlewareOptions = options?.Value ?? new ResponseLoggerMiddlewareOptions();
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var traceId = context.TraceIdentifier;

            var bodyStream = context.Response.Body;

            var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            await next(context);

            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();

            var message = $"TraceId: {traceId}{Environment.NewLine}Response: {context.Response.StatusCode} - {responseBody}";

            if (_middlewareOptions.UnwritableResponses.Contains(context.Response.StatusCode))
            {
                if (_middlewareOptions.UnloggerResponses.Any(p => context.Request.Path.StartsWithSegments(p)) == false)
                {
                    _logger.LogInformation(message);
                }
            }
            else
            {
                if (context.Response.StatusCode >= 400)
                {
                    _logger.LogError(message);
                }
                else if (context.Response.StatusCode >= 300)
                {
                    _logger.LogWarning(message);
                }
                else if (_middlewareOptions.UnloggerResponses.Any(p => context.Request.Path.StartsWithSegments(p)) == false)
                {
                    _logger.LogInformation(message);
                }

                var destination = new Memory<byte>(new byte[responseBodyStream.Length]);
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.ReadAsync(destination);
                await bodyStream.WriteAsync(destination);
            }
        }
    }

    public class ResponseLoggerMiddlewareOptions
    {
        public List<int> UnwritableResponses { get; set; } = new List<int>
                                                             {
                                                                 (int) HttpStatusCode.NoContent,
                                                                 (int) HttpStatusCode.NotModified,
                                                                 (int) HttpStatusCode.Redirect,
                                                                 (int) HttpStatusCode.RedirectKeepVerb,
                                                                 (int) HttpStatusCode.RedirectMethod,
                                                                 (int) HttpStatusCode.PermanentRedirect,
                                                                 (int) HttpStatusCode.TemporaryRedirect
                                                             };

        public List<string> UnloggerResponses = new List<string>()
        {
            "/api/Stock"
        };
    }

    public static class ResponseLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseLoggerMiddleware(this IApplicationBuilder app)
        {
            return app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder => appBuilder.UseMiddleware<ResponseLoggerMiddleware>());
        }
        public static IServiceCollection AddResponseLoggerMiddleware(this IServiceCollection services)
        {
            return services.AddTransient<ResponseLoggerMiddleware>();
        }
    }
}

using HMTSolution.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMTSolution.BCS.Extensions
{
    public static class StartupMiddlewareExtension
    {
        public static IServiceCollection AddMongoSettings(this IServiceCollection services,
           IConfiguration configuration)
        {
            return services.Configure<MongoSettings>(options =>
            {
                options.ConnectionString = configuration
                    .GetSection(nameof(MongoSettings) + ":" + MongoSettings.ConnectionStringValue).Value;
                options.Database = configuration
                    .GetSection(nameof(MongoSettings) + ":" + MongoSettings.DatabaseValue).Value;
            });
        }
    }
}

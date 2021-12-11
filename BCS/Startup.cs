using HMTSolution.BCS.Extensions;
using HMTSolution.MongoRepo.Repositories;
using HMTSolution.MongoRepo.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HMTSolution.BCS.Middlewares;
using Newtonsoft.Json.Converters;

namespace HMTSolution.BCS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors()
            .AddMemoryCache()
            .AddMongoSettings(Configuration)
            .AddValidations()
            .AddScoped<IStockRepository, StockRepository>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Stock API",
                    Version = "v1",
                    Description = "Case Study",
                    Contact = new OpenApiContact
                    {
                        Name = "Hamit Yıldırım",
                        Email = "dewelloper@gmail.com"
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddSwaggerGenNewtonsoftSupport();
            services.AddHttpContextAccessor();
            services.ConfigureAuthentication(Configuration);

            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateAudience = false,
            //        ValidAudience = "audience.Stock.com",
            //        ValidateIssuer = false,
            //        ValidIssuer = "west-world.Stock.com",
            //        ValidateLifetime = false,
            //        ValidateIssuerSigningKey = false,
            //        IssuerSigningKey = new SymmetricSecurityKey(
            //            Encoding.UTF8.GetBytes("MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAIpMKSJbPFt3htd9edicBOvTLkwoRQMveYrPRdOpaoU23E7J2D+e4qXTqQMVkRGgj7zaFAXSvHZiSYuWZ5uFzU7hn5OJmB8wvsm27eoKboU/zaHxVDGGbBOvITVK/jE+ZEdfKZMgo1FvpGI3XUyD/hubrQanMrktOJumDYHPtwepAgMBAAECgYAgzangHVX2uCZCzN9u8qr0KPZNWCvucn9Y3otIhmHe0UF2asghZxWJkef/9Eihrr0JZYzkSLUtO2kIdBeFOzqUQ6XqZf1dX47HPBE+hwXBVwnTVLmq57DBxENFRiaiGAXzPNXdtmzp+cw2EY7zoZP0+laOKjPNSpEEZ/UW5ed45QJBAPez/reupeVaOnGaELXqOozQUvURiABeKZstlqoWgz11FKosmSYp5RQjG7q6eCnUEPI4y2EgJUUSJ3dn4Sups6sCQQCO7gjKmp4WBDkxhlS9RF9N7+npBoJwPUlDtYqtr5neWaUdrPKpY/VctD86Zm/jAZpPabdqd9VUU1SYraZjbJ37AkEAhdunUuv2irLv0mRHk4c4jNAnhHgs3sYEBe/k85Wm5pdWy3++Y3lQakluusH6HeCUJ9G5VotgmKru2QAyFr5mcQJASDHFsQi+VyKU/QX8IYm6lfRb8z89fZIHQrMdNDPhhaVEOKQWAieiVMwar9X0J/a0Se59HcMftzNMJL55r/i6JwJAPJWkGYqefrV2TklXmEBO0yHskNgzTg+uVkwRXuGd+039V0sTPV8cFGt6Jo3jo3MamdxfxN3V2QUJ9Xxfy3QFdg=="))
            //    };
            //    options.Events = new JwtBearerEvents
            //    {
            //        OnTokenValidated = ctx =>
            //        {
            //            //Gerekirse burada gelen token içerisindeki çeşitli bilgilere göre doğrulam yapılabilir.
            //            return Task.CompletedTask;
            //        },
            //        OnAuthenticationFailed = ctx =>
            //        {
            //            Console.WriteLine("Exception:{0}", ctx.Exception.Message);
            //            return Task.CompletedTask;
            //        }
            //    };
            //});
            services.AddControllers().AddNewtonsoftJson(p => p.SerializerSettings.Converters.Add(new StringEnumConverter()));
            services.AddHealthChecks();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("CorsPolicy");

            //app.UseRequestLoggerMiddleware()
            //   .UseResponseLoggerMiddleware()
            //   .UseErrorHandlingMiddleware();

            app.UseSwagger(c => c.SerializeAsV2 = true);
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "StockManagement Swagger");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            Console.WriteLine($"{typeof(Startup).Namespace} running on {env.EnvironmentName} environment.");
        }
    }
}

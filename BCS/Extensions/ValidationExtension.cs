using FluentValidation;
using HMTSolution.BCS.Validations.CommonValidation;
using HMTSolution.BCS.Validations.Resolver;
using Microsoft.Extensions.DependencyInjection;

namespace HMTSolution.BCS.Extensions
{
    public static class ValidationExtension
    {
        public static IServiceCollection AddValidations(this IServiceCollection services)
        {
            services.AddScoped<IValidatorResolver, ValidatorResolver>();

            services.AddSingleton<IValidator, StringValidator>();
            //Add other validators if needs

            return services;
        }
    }
}

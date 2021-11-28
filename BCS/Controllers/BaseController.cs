using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using FluentValidation;
using HMTSolution.BCS.Validations;
using HMTSolution.BCS.Validations.Resolver;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace HMTSolution.BCS.Controllers
{
    public class BaseController : ControllerBase
    {
        internal readonly ILogger _logger;
        internal readonly IValidatorResolver _validatorResolver;

        public BaseController(ILogger logger, IValidatorResolver validatorResolver)
        {
            _logger = logger;
            _validatorResolver = validatorResolver;
        }

        // full feature not implemented yet
        internal void Validate<T>(T model, Type validatorType, string structName = "")
        {
            var validator = _validatorResolver.Resolve(validatorType);
            if (validator is IStructValidator structValidator)
            {
                structValidator.PropertyName = structName;
            }

            var context = new ValidationContext<T>(model);
            var validationResult = validator.Validate(context);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.ToString());
            }
        }

        //Feature:: If needs for external sites integration
        public static async Task<TResult> PostAsync<TResult>(string url, IEnumerable<KeyValuePair<string, string>> postData, string token = "", string contentType = "application/x-www-form-urlencoded")
        {
            using (var httpClient = new HttpClient())
            {
                if (token != "")
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                using (var content = new FormUrlEncodedContent(postData))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", contentType);

                    var response = await httpClient.PostAsync(url, content);

                    return await System.Text.Json.JsonSerializer.DeserializeAsync<TResult>(await response.Content.ReadAsStreamAsync());
                }
            }
        }

    }

    
}

using Maggsoft.Core.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Exceptions
{
    public static class CustomErrorHandlerHelper
    {
        public static void UseCustomErrors(this IApplicationBuilder app, IHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.Use(WriteDevelopmentResponse);
            }
            else
            {
                app.Use(WriteProductionResponse);
            }
        }

        private static Task WriteDevelopmentResponse(HttpContext httpContext, Func<Task> next)
            => WriteResponse(httpContext, includeDetails: true);

        private static Task WriteProductionResponse(HttpContext httpContext, Func<Task> next)
            => WriteResponse(httpContext, includeDetails: false);

        private static async Task WriteResponse(HttpContext httpContext, bool includeDetails)
        {
            // Try and retrieve the error from the ExceptionHandler middleware
            var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            // Should always exist, but best to be safe!
            if (ex != null)
            {
                // ProblemDetails has it's own content type
                httpContext.Response.ContentType = "application/problem+json";

                // Get the details to display, depending on whether we want to expose the raw exception
                //var title = includeDetails ? "An error occured: " + ex.Message : "An error occured";
                var details = includeDetails ? ex.ToString() : null;

                var response = new Response<object>();
                var configuration = httpContext.RequestServices.GetService<IConfiguration>();
                var majorVersionConfig = configuration.GetSection("ApiVersion:MajorVersion")?.Value;
                var minorVersionConfig = configuration.GetSection("ApiVersion:MinorVersion")?.Value;

                response.ApiVersion = $"{majorVersionConfig}.{minorVersionConfig}";
                response.SystemError = details;
                response.IsError = true;
                response.StatusCode = StatusCodes.Status500InternalServerError;

                var stream = httpContext.Response.Body;
                await JsonSerializer.SerializeAsync(stream, response);
            }
        }
    }
}

using Maggsoft.Core.Base;
using Maggsoft.Core.Exceptions;
using Maggsoft.Framework.Exceptions;
using Maggsoft.Framework.Helper.ResponseRewind;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Middleware
{
    [AttributeUsage(AttributeTargets.All)]
    public class IgnoreResponseRewindMiddlewareAttribute : Attribute { }

    public class ResponseRewindMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ResponseRewindMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public virtual async Task Invoke(HttpContext context)
        {
            var originalBody = context.Response.Body;

            var response = new Response<object>();

            var majorVersionConfig = _configuration.GetSection("ApiVersion:MajorVersion")?.Value;
            var minorVersionConfig = _configuration.GetSection("ApiVersion:MinorVersion")?.Value;

            response.ApiVersion = $"{majorVersionConfig}.{minorVersionConfig}";
            response.StatusCode = StatusCodes.Status200OK;

            try
            {
                var json = await FormatResponse(context);
                response.Result = JsonConvert.DeserializeObject(json);
                response.Success = true;
                await new BaseResponseHelper(context).Bind(originalBody, response);
            }
            catch (ArgumentException ex)
            {
                await new ArgumentExceptionHelper(context).Bind(originalBody, response, ex);
            }
            catch (ModelStateException ex)
            {
                await new ModelStateExceptionHelper(context).Bind(originalBody, response, ex);
            }
            catch (ApiVersioningException ex)
            {
                await new ApiVersioningExceptionHelper(context).Bind(originalBody, response, ex);
            }
            catch (NotFoundException ex)
            {
                await new NotFoundExceptionHelper(context).Bind(originalBody, response, ex);
            }
            catch (FileLoadException ex)
            {
                await new ExceptionHelper(context).Bind(originalBody, response, ex);
            }
            catch (MaggsoftException ex)
            {
                await new ExceptionHelper(context).Bind(originalBody, response, ex);
            }
            catch (ForbiddenExtension ex)
            {
                await new ForbiddenExtensionHelper(context).Bind(originalBody, response, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                await new UnauthorizedAccessExceptionHelper(context).Bind(originalBody, response, ex);
            }
            catch (Exception ex)
            {
                await new ExceptionHelper(context).Bind(originalBody, response, ex);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }

        private async Task<string> FormatResponse(HttpContext context)
        {
            string responseBody;
            await using (var memStream = new MemoryStream())
            {
                context.Response.Body = memStream;
                await _next(context);
                memStream.Position = 0;
                responseBody = await new StreamReader(memStream).ReadToEndAsync();
            }

            return responseBody;
        }
    }
}

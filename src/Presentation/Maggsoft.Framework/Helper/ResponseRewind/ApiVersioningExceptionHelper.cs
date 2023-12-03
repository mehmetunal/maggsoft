using System;
using System.IO;
using Maggsoft.Core.Base;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Maggsoft.Core.Extensions;

namespace Maggsoft.Framework.Helper.ResponseRewind
{
    public class ApiVersioningExceptionHelper : BaseResponseHelper, IExtensionsHelper
    {
        private HttpContext _context;
        public ApiVersioningExceptionHelper(HttpContext context) : base(context)
        {
            _context = context;
        }

        public async Task Bind(Stream body, Response<object> response, Exception ex)
        {
            response.AddMessage(ex.Message);
            response.StatusCode = StatusCodes.Status400BadRequest;
            _context.Response.StatusCode = response.StatusCode;
            await base.Bind(body, response);
        }
    }
}

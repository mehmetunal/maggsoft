using System;
using System.IO;
using Maggsoft.Core.Base;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            response.SystemError = ex.Message;
            response.IsError = true;
            response.StatusCode = StatusCodes.Status400BadRequest;
            await base.Bind(body, response);
        }
    }
}

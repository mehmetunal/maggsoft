using System;
using System.IO;
using Maggsoft.Core.Base;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Maggsoft.Framework.Helper.ResponseRewind
{
    public class UnauthorizedAccessExceptionHelper : BaseResponseHelper, IExtensionsHelper
    {
        private HttpContext _context;

        public UnauthorizedAccessExceptionHelper(HttpContext context) : base(context)
        {
            _context = context;
        }

        public async Task Bind(Stream body, Response<object> response, Exception ex)
        {
            response.SystemError = ex.Message;
            response.IsError = true;
            response.StatusCode = StatusCodes.Status401Unauthorized;
            _context.Response.StatusCode = response.StatusCode;
            await base.Bind(body, response);
        }
    } 
    public class ForbiddenExtensionHelper : BaseResponseHelper, IExtensionsHelper
    {
        private HttpContext _context;

        public ForbiddenExtensionHelper(HttpContext context) : base(context)
        {
            _context = context;
        }

        public async Task Bind(Stream body, Response<object> response, Exception ex)
        {
            response.SystemError = ex.Message;
            response.IsError = true;
            response.StatusCode = StatusCodes.Status403Forbidden;
            _context.Response.StatusCode = response.StatusCode;
            await base.Bind(body, response);
        }
    }
}

using Maggsoft.Core.Base;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Helper.ResponseRewind
{
    public class ExceptionHelper : BaseResponseHelper, IExtensionsHelper
    {
        private HttpContext _context;
        public ExceptionHelper(HttpContext context) : base(context)
        {
            _context = context;
        }
        public async Task Bind(Stream body, Response<object> response, Exception ex)
        {
            response.SystemError = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            response.IsError = true;
            response.StatusCode = StatusCodes.Status500InternalServerError;
            _context.Response.StatusCode = response.StatusCode;
            await base.Bind(body, response);
        }
    }
}

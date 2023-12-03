using Maggsoft.Core.Base;
using Maggsoft.Core.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Helper.ResponseRewind
{
    public class NotFoundExceptionHelper : BaseResponseHelper, IExtensionsHelper
    {
        private HttpContext _context;
        public NotFoundExceptionHelper(HttpContext context) : base(context)
        {
            _context = context;
        }
        public async Task Bind(Stream body, Response<object> response, Exception ex)
        {
            response.AddMessage(ex.Message);
            response.IsError = true;
            response.StatusCode = StatusCodes.Status404NotFound;
            await base.Bind(body, response);
        }
    }
}

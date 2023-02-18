using Maggsoft.Core.Base;
using Maggsoft.Core.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Helper.ResponseRewind
{
    public class ArgumentExceptionHelper : BaseResponseHelper, IExtensionsHelper
    {
        private HttpContext _context;
        public ArgumentExceptionHelper(HttpContext context) : base(context)
        {
            _context = context;
        }
        public async Task Bind(Stream body, Response<object> response, Exception ex)
        {
            response.StatusCode = StatusCodes.Status404NotFound;
            response.AddMessage(ex.Message);
            await base.Bind(body, response);
        }
    }
}

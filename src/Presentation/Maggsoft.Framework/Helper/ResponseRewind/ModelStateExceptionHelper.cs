using Maggsoft.Core.Base;
using Maggsoft.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Helper.ResponseRewind
{
    public class ModelStateExceptionHelper : BaseResponseHelper, IExtensionsHelper
    {
        private HttpContext _context;
        public ModelStateExceptionHelper(HttpContext context) : base(context)
        {
            _context = context;
        }
        public async Task Bind(Stream body, Response<object> response, Exception ex)
        {
            if (!string.IsNullOrEmpty(ex.Message))
            {
                try
                {
                    foreach (var item in JsonConvert.DeserializeObject<List<string>>(ex.Message))
                        response.AddValidationMessages(item);
                }
                catch
                {
                    response.AddValidationMessages(ex.Message);
                }
            }

            response.StatusCode = StatusCodes.Status400BadRequest;
            await base.Bind(body, response);
        }
    }
}

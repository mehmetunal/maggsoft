using Maggsoft.Framework.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace Maggsoft.Framework.Helper.ApiVersioning
{
    public class ApiVersioningErrorResponseProvider : DefaultErrorResponseProvider
    {
        public override IActionResult CreateResponse(ErrorResponseContext context)
        {
            throw new ApiVersioningException(context.Message);
        }
    }
}

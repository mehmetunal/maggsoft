using Microsoft.AspNetCore.Mvc;

namespace Maggsoft.Framework.Api
{
    [Produces("application/json")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        //[ApiExplorerSettings(IgnoreApi = true)]
    }
}

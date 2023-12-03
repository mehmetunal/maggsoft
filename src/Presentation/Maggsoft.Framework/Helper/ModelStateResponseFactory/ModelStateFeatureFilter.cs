using Newtonsoft.Json;
using System.Threading.Tasks;
using Maggsoft.Framework.Exceptions;
using Maggsoft.Framework.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Maggsoft.Framework.Helper.ModelStateResponseFactory;

public class ModelStateFeatureFilter : IActionResult
{
    public Task ExecuteResultAsync(ActionContext context)
    {
        var modelState = context.ModelState.GetErrorMessages();
        throw new ModelStateException(JsonConvert.SerializeObject(modelState));
        // var modelStateDictionary = new Dictionary<string, string[]>();
        // for (var b = 0; b < context.ModelState.Values.Count(); b++)
        // {
        //     if (context.ModelState.Values.ElementAt(b).Errors.Count() > 0)
        //         modelStateDictionary.Add(context.ModelState.Keys.ElementAt(b), context.ModelState.Values.ElementAt(b).Errors.Select(c => c.ErrorMessage).ToArray());
        // }
        //
        // throw new ModelStateException(JsonConvert.SerializeObject(modelStateDictionary));
        //return Task.CompletedTask;
    }
}

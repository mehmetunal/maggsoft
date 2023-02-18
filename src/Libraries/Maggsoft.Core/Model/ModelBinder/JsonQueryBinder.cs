using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Maggsoft.Core.Model.ModelBinder
{
    internal class JsonQueryBinder : IModelBinder
    {
        private readonly ILogger<JsonQueryBinder> _logger;

        public JsonQueryBinder(ILogger<JsonQueryBinder> logger)
        {
            _logger = logger;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;
            if (value == null)
            {
                return Task.CompletedTask;
            }

            try
            {
                var parsed = JsonSerializer.Deserialize(
                    value,
                    bindingContext.ModelType,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web)
                );
                bindingContext.Result = ModelBindingResult.Success(parsed);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to bind '{FieldName}'", bindingContext.FieldName);
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}

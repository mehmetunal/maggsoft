using Microsoft.AspNetCore.Mvc;

namespace Maggsoft.Core.Model.ModelBinder;

public class FromJsonQueryAttribute : ModelBinderAttribute
{
    public FromJsonQueryAttribute()
    {
        BinderType = typeof(JsonQueryBinder);
    }
}

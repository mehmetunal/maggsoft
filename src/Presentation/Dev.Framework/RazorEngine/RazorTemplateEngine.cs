using RazorEngineCore;
using System.Collections.Concurrent;

namespace Dev.Framework.RazorEngine
{
    internal class RazorTemplateEngine : ITemplateEngine
    {
        private static ConcurrentDictionary<int, IRazorEngineCompiledTemplate> TemplateCache;

        public RazorTemplateEngine()
        {
            //.net 6 ArgumentNullException.ThrowIfNull(TemplateCache);
            TemplateCache ??= new ConcurrentDictionary<int, IRazorEngineCompiledTemplate>();
        }

        public string Render(string template, object model)
        {
            int hashCode = template.GetHashCode();

            IRazorEngineCompiledTemplate compiledTemplate = TemplateCache.GetOrAdd(hashCode, i =>
            {
                RazorEngineCore.RazorEngine razorEngine = new();
                return razorEngine.Compile(template);
            });

            return compiledTemplate.Run(model);
        }
    }
}

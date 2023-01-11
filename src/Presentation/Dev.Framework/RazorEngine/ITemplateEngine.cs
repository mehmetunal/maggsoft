namespace Dev.Framework.RazorEngine
{
    internal interface ITemplateEngine
    {
        string Render(string template, object model);
    }
}

namespace Maggsoft.Services.RazorEngine
{
    internal interface ITemplateEngine
    {
        string Render(string template, object model);
    }
}

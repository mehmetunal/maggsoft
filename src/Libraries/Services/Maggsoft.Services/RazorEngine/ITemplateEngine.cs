namespace Maggsoft.Services.RazorEngine
{
    public interface ITemplateEngine
    {
        string Render(string template, object model);
    }
}

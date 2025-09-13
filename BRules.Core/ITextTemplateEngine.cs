
namespace BRules;

public interface ITextTemplateEngine
{
    string Render(string templateText, IDictionary<string, object?> parameters);
}
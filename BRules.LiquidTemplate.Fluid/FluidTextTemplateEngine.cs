using Fluid;

namespace BRules;

public class FluidTextTemplateEngine: ITextTemplateEngine
{
    private static FluidParser parser = new ();
    private FluidParser _parser;

    public FluidTextTemplateEngine(FluidParser? parser = null)
    {
        _parser = parser ?? FluidTextTemplateEngine.parser;
    }

    public string Render(string templateText, IDictionary<string, object?> parameters)
    {
        var template = _parser.Parse(templateText);

        var context = new TemplateContext();
        foreach (var parameter in parameters)
        {
            context.SetValue(parameter.Key, parameter.Value);
        }

        return template.Render(context);
    }
}

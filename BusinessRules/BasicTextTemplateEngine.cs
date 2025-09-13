using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRules;

class BasicTextTemplateEngine : ITextTemplateEngine
{
    public string Render(string templateText, IDictionary<string, object?> parameters)
    {
        var outputText = templateText;
        foreach (var parameter in parameters)
        {
            outputText = outputText.Replace($"{{{{{parameter.Key}}}}}", parameter.Value?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        return outputText;
    }
}

public static class BasicTemplateEngineExtensions
{
    public static IRulesEngineBuilder UseBasicTemplateEngine(this IRulesEngineBuilder self)
    {
        return self.UseTextTemplateEngine(new BasicTextTemplateEngine());
    }
}

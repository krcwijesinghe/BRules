using Jint;

namespace BRules;

public static class JintEvaluationEngineExtensions
{
    public static IRulesEngineBuilder UseJintEvaluationEngine(this IRulesEngineBuilder self, Options? options = null)
    {
        var engine = new JintEvaluationEngine();
        if (options != null)
        {
            engine.Options = options;
        }
        return self.UseEvaluationEngine(engine);
    }

    public static IRulesEngineBuilder UseJintEvaluationEngine(this IRulesEngineBuilder self, Action<Options> setOptionsAction)
    {
        var engine = new JintEvaluationEngine();
        var options = new Options();
        setOptionsAction(options);
        engine.Options = options;
        return self.UseEvaluationEngine(engine);
    }
}



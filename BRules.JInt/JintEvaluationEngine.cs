using Jint;

namespace BRules;

public class JintEvaluationEngine : IEvaluationEngine
{
    public Func<string, object[], object?>? FunctionEvaluator { get; set; } = null;
    public Options? Options { get; set; } = null;
    
    public object? EvaluateExpression(string expression, IDictionary<string, object?> parameters, IList<string>? functions = null)
    {
        var engine = Options != null? new Engine(Options): new Engine();
        RegisterCustomFunctions(functions, engine);

        foreach (var parameter in parameters)
        {
            engine.SetValue(parameter.Key, parameter.Value);
        }

        return engine.Evaluate(expression).ToObject();
    }

    private void RegisterCustomFunctions(IList<string>? functions, Engine engine)
    {
        if (functions == null || FunctionEvaluator == null) return;

        engine.SetValue("__internalFunc", (string name, object[] args) => FunctionEvaluator(name, args));
        foreach (var functionName in functions)
        {
            engine.Execute($"function {functionName}(...args) {{ return __internalFunc('{functionName}', args); }}");
        }
    }
}



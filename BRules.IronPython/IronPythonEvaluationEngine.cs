
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace BRules;

public class IronPythonEvaluationEngine : IEvaluationEngine
{
    private ScriptEngine engine = Python.CreateEngine();

    public Func<string, object[], IRulesExecutionContext, object?>? FunctionEvaluator { get; set; } = null;

    public object? EvaluateExpression(string expression, IDictionary<string, object?> parameters, IRulesExecutionContext context, IList<string>? functions = null)
    {
        ScriptScope scope = engine.CreateScope();

        RegisterCustomFunctions(functions, scope, context);

        foreach (var parameter in parameters)
        {
            scope.SetVariable(parameter.Key, parameter.Value);
        }

        return engine.Execute(expression, scope);
    }

    private void RegisterCustomFunctions(IList<string>? functions, ScriptScope scope, IRulesExecutionContext context)
    {
        if (functions == null || FunctionEvaluator == null) return;

        scope.SetVariable("__internalFunc", (string name, object[] args) => FunctionEvaluator(name, args, context));
        foreach (var functionName in functions)
        {
            engine.Execute($"""
                def {functionName}(*args):
                    return __internalFunc('{functionName}', args)
                """, scope);
        }
    }
}

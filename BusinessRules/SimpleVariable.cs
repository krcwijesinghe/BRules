namespace BRules;

internal class SimpleVariable
{
    public required string Expression { get; set; }

    public Task<object?> GetValue(RuleExecutionContext context)
    {
        return Task.FromResult(context.EvaludationEngine.EvaluateExpression(Expression, context.LocalVariables, context.FunctionNames));
    }
}




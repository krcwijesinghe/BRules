namespace BRules;

internal class RuleVariable
{
    public required RuleVariableType Type { get; set; } = RuleVariableType.DefaultValue;
    public object? DefaultValue { get; set; }
    public Delegate? ValueProvider { get; set; }
    public IList<string>? ValueProviderParameters { get; set; }
    public AggregateVariable? Aggregate { get; set; }
    public SimpleVariable? SimpleVariable { get; set; }
    public FieldValueVariable? FieldValue { get; set; }
    public bool OutputVariable { get; set; }
    public Type? DataType { get; set; }
    public IList<string>? VariablesToPreload { get; set; }


    public async Task<object?> GetValue(RuleExecutionContext context)
    {
        if (VariablesToPreload != null)
        {
            foreach (var varName in VariablesToPreload)
            {
                await context.Preload(varName);
            }
        }

        var value = Type switch
        {
            RuleVariableType.DefaultValue => DefaultValue,
            RuleVariableType.ValueProvider => await EvaluateValueProvider(context),
            RuleVariableType.Aggregate => await Aggregate!.GetValue(context),
            RuleVariableType.SimpleVariable => await SimpleVariable!.GetValue(context),
            RuleVariableType.FieldValue => await FieldValue!.GetValue(context),
            _ => throw new InvalidOperationException("Unknown variable type")
        };

        if (DataType != null)
        {
            value = Convert.ChangeType(value, DataType);
        }

        return value;
    }

    public async Task<object?> EvaluateValueProvider(RuleExecutionContext context)
    {
        if (ValueProvider == null) throw new InvalidOperationException("ValueProvider is not set.");
        
        var parameters = new List<object?>();
        if (ValueProviderParameters != null)
        {
            foreach (var parameter in ValueProviderParameters)
            {
                if (IsMutable(parameter, context))
                {
                    throw new InvalidOperationException($"The mutable value '{parameter}' cannot be used as an input to a ValueProvider.");
                }
                parameters.Add(await context.GetValue(parameter));
            }
        }    
        
        var result = ValueProvider.DynamicInvoke(parameters.ToArray());
        if (result is Task task)
        {
            await task.ConfigureAwait(false);
            var type = task.GetType();
            if (type.IsGenericType)
            {
                // Use reflection to get the Result property value
                var resultProperty = type.GetProperty("Result");
                return resultProperty?.GetValue(task);
            }
            return null;
        }
        return result;
    }

    private bool IsMutable(string parameterName, RuleExecutionContext context)
    {
        if (context.ParameterNames.Contains(parameterName))
        {
            return false;
        }

        var variable = context.Variables.TryGetValue(parameterName, out var v) ? v : null;
        if (variable != null)
        {
            if (variable.Type == RuleVariableType.ValueProvider ||
                variable.Type == RuleVariableType.Aggregate)
            {
                return false;
            }
        }

        return true;
    }
}

internal enum RuleVariableType
{
    DefaultValue = 0,
    ValueProvider = 1,
    Aggregate = 2,
    FieldValue = 3,
    SimpleVariable = 4,
}






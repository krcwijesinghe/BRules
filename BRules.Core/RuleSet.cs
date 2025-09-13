namespace BRules;

public class RuleSet
{
    public string Name { get; set; }
    public string Version { get; set; } = "1.0";
    public IList<RuleDefinition> Rules { get; set; }
    public IList<ParameterDefinition> Parameters { get; set; }
    public IList<string>? PreloadVariables { get; set; }
    public IList<AggregateVariableDefinition>? AggregateVariables { get; set; }

    public RuleSet()
    {       
    }

    public RuleSet(
        string name, 
        string version,
        IList<RuleDefinition> rules,
        IList<ParameterDefinition> parameters,
        IList<string>? preloadVariables = null,
        IList<AggregateVariableDefinition>? aggregateVariable = null)
    {
        Name = name;
        Version = version;
        Rules = rules;
        Parameters = parameters;
        PreloadVariables = preloadVariables;
        AggregateVariables = aggregateVariable;
    }
}

public class RuleDefinition
{
    public required string Name { get; set; }
    public required string Type { get; set; }

    public string? ConditionType { get; set; }
    public string? Condition { get; set; }

    public IList<string>? VariablesToPreload { get; set; }

    public string? Variable { get; set; }
    public string? Expression { get; set; }

    public bool? MarkAsInvalid { get; set; }
    public string? ValidationMessageTemplate { get; set; }
    public bool TerminateIfInvalid { get; set; }

    public IList<RuleDefinition>? SubRules { get; set; }
}

public class AggregateVariableDefinition
{
    public required string Name { get; set; }
    public required string CollectionVariable { get; set; }
    public required string Expression { get; set; }
    public string? FilterCondition { get; set; }
    public required string AggregateFunction { get; set; }
    public IList<RuleDefinition>? SubRules { get; set; }
}

public class ParameterDefinition
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public bool IsOptional { get; set; }
    public object? DefaultValue { get; set; }
}

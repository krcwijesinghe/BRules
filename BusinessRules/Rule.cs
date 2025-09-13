namespace BRules;

public class Rule
{
    public required string Name { get; set; }
    public required string Type { get; set; }

    public required string RuleSetName { get; set; }
    public required string Version { get; set; }

    public string? ConditionType { get; set; }
    public string? Condition { get; set; }

    public required IList<string> VariablesToPreload { get; set; }

    public IList<Rule>? ChildRules { get; set; }

    public string? Variable { get; set; }
    public string? Expression { get; set; }

    public string? ValidationMessageTemplate { get; set; }
    public bool? TerminateIfInvalid { get; set; }
}



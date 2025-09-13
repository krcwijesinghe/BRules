namespace BRules;

public class RulesEvaluationResult
{
    public bool IsValid { get; set; }
    public required IDictionary<string,object?> OutputParameters { get; set; }
    public required IList<string> EvaluatedRules { get; set; }
    public required IList<string> ValidationMessages { get; set; }
}

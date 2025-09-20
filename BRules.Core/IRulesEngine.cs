namespace BRules;

public interface IRulesEngine
{
    Task<RulesEvaluationResult> ExecuteAsync(IDictionary<string, object?> parameters);
}

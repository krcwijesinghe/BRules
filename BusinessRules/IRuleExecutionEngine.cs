namespace BRules;

internal interface IRuleExecutionEngine
{
    Task<bool> ExecuteRules(IList<Rule> rules, RuleExecutionContext context);
}



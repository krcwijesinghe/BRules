using System.Data;

namespace BRules;

internal class RuleExecutionContext: IRulesExecutionContext
{
    public IEvaluationEngine EvaludationEngine { get; }
    public ITextTemplateEngine TextTemplateEngine { get; }

    public IRuleExecutionEngine RuleExecutionEngine { get; }
    public IDictionary<string, object?> LocalVariables { get; }
    public IList<string> EvaluatedRules { get; } = new List<string>();
    public IDictionary<string, RuleVariable> Variables { get; }
    public IDictionary<string, RuleFunction> Functions { get; }
    public IList<string> ExternalFunctionNames { get; }
    public IList<string> FunctionNames { get; }
    public HashSet<string> ParameterNames { get; } = new();

    public bool IsValid { get; set; }
    public IList<string> ValidationMessages { get; set; } = new List<string>();

    public RuleExecutionContext(IEvaluationEngine evaluationEngine,
                                ITextTemplateEngine textTemplateEngine,
                                IRuleExecutionEngine ruleExecutionEngine,
                                IDictionary<string, object?> parameters,
                                IDictionary<string, RuleVariable> variables,
                                IDictionary<string, RuleFunction> functions,
                                IList<string> externalFunctionNames)
    {
        EvaludationEngine = evaluationEngine ?? throw new ArgumentNullException(nameof(evaluationEngine));
        TextTemplateEngine = textTemplateEngine ?? throw new ArgumentNullException(nameof(textTemplateEngine));
        RuleExecutionEngine = ruleExecutionEngine ?? throw new ArgumentNullException(nameof(ruleExecutionEngine)); ;
        LocalVariables = parameters.ToDictionary(p => p.Key, p => p.Value); //Clone
        Variables = variables.ToDictionary(v => v.Key, v => v.Value); //Clone
        Functions = functions.ToDictionary(f => f.Key, f => f.Value); //Clone
        ExternalFunctionNames = externalFunctionNames.ToList(); //Clone
        FunctionNames = Functions.Keys.Union(ExternalFunctionNames).ToList();
        IsValid = true;

        ParameterNames.UnionWith(parameters.Keys);
    }

    public void SetValue(string name, object? value, bool initialization = false)
    {
        if (ParameterNames.Contains(name) && !initialization)
        {
            throw new InvalidOperationException($"Parameter '{name}' cannot be assigned to");
        }

        var variable = Variables.TryGetValue(name, out var v) ? v : null;
        if (variable != null)
        {
            if (!initialization &&
                (variable.Type == RuleVariableType.ValueProvider ||
                 variable.Type == RuleVariableType.SimpleVariable ||
                 variable.Type == RuleVariableType.Aggregate))
            {
                throw new InvalidOperationException($"Variable '{name}' cannot be assigned to");
            }

            if (variable.DataType != null && value != null)
            {
                value = Convert.ChangeType(value, variable.DataType);
            }
        }

        LocalVariables[name] = value;
    }

    public async Task<object?> GetValue(string name)
    {
        return LocalVariables.TryGetValue(name, out var value) ? value :
               Variables.TryGetValue(name, out var variable) ? await variable.GetValue(this):
               null;
    }

    public async Task Preload(string name)
    {
        if (!LocalVariables.ContainsKey(name))
        {
            SetValue(name, await GetValue(name), initialization: true);
        }
    }

    public void AddRuleEvaluation(Rule rule)
    {
        EvaluatedRules.Add($"{rule.RuleSetName}({rule.Version}).{rule.Name}");
    }

    public RuleExecutionContext Clone()
    {
        return new RuleExecutionContext(EvaludationEngine, TextTemplateEngine, RuleExecutionEngine, LocalVariables, Variables, Functions, ExternalFunctionNames);
    }
}

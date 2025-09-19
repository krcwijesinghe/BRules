using System.Collections.Concurrent;
using System.Data;
using System.Text;

namespace BRules;

internal class RulesEngine : IRulesEngine, IRuleExecutionEngine
{
    private IList<Rule> _rules;
    private IDictionary<string, RuleVariable> _variables;
    private IEvaluationEngine _evaluationEngine;
    private ITextTemplateEngine _textTemplateEngine;
    private IList<ParameterDefinition> _parameterDefs;
    private HashSet<string> _preloadVariables = new();
    private IDictionary<string, RuleFunction> _functions = new Dictionary<string, RuleFunction>();
    private ConcurrentDictionary<string, object?> _functionCache = new();

    internal RulesEngine(
        IList<Rule> rules,
        IDictionary<string, RuleVariable> variables,
        IDictionary<string, RuleFunction> functions,    
        IList<ParameterDefinition> parameterDefs,
        HashSet<string> preloadVariables,
        IEvaluationEngine evaluationEngine,
        ITextTemplateEngine textTemplateEngine)
    {
        _rules = rules;
        _variables = variables;
        _functions = functions;
        _parameterDefs = parameterDefs;
        _preloadVariables = preloadVariables;
        _textTemplateEngine = textTemplateEngine;
        evaluationEngine.FunctionEvaluator = EvaluateFunction;
        _evaluationEngine = evaluationEngine;
    }

    /// <summary>
    /// Execute this rules set
    /// </summary>
    /// <param name="parameters">input parameters</param>
    /// <returns>Returns the rules evaluation result (validation result, output variables and list of executed rules)</returns>
    /// <exception cref="ArgumentNullException">A parameter for one or more rules is null</exception>
    /// <exception cref="ArgumentException">A parameter for one or more rules is invalid</exception>
    /// <exception cref="InvalidOperationException">One of the operations in the rules set is not valid</exception>
    public async Task<RulesEvaluationResult> ExecuteAsync(IDictionary<string, object?> parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        var inputParameters = LoadParameters(parameters);

        RuleExecutionContext context = new RuleExecutionContext(_evaluationEngine, _textTemplateEngine, this, inputParameters, _variables, _functions.Keys.ToList());

        foreach (var variableName in _preloadVariables)
        {
            await context.Preload(variableName);
        }

        await ExecuteRules(_rules, context);

        var outputParameters = new Dictionary<string, object?>();
        foreach (var variable in context.Variables.Where(v => v.Value.OutputVariable))
        {
            outputParameters.Add(variable.Key, await context.GetValue(variable.Key));
        }

        return new RulesEvaluationResult
        {
            IsValid = context.IsValid,
            OutputParameters = outputParameters,
            EvaluatedRules = context.EvaluatedRules,
            ValidationMessages = context.ValidationMessages
        };
    }

    private Dictionary<string, object?> LoadParameters(IDictionary<string, object?> parameters)
    {
        var inputParameters = new Dictionary<string, object?>();
        foreach (var parameterDef in _parameterDefs)
        {
            if (parameters.TryGetValue(parameterDef.Name, out var parameterValue))
            {
                if (parameterValue != null && parameterValue.GetType().FullName != parameterDef.Type)
                {
                    throw new ArgumentException($"Parameter type is invalid for parameter {parameterDef.Name}");
                }

                inputParameters[parameterDef.Name] = parameterValue;
            }
            else
            {
                if (parameterDef.IsOptional)
                {
                    inputParameters[parameterDef.Name] = parameterDef.DefaultValue;
                }
                else
                {
                    throw new ArgumentException($"Missing required parameter: {parameterDef.Name}");
                }
            }
        }
        return inputParameters;
    }

    Task<bool> IRuleExecutionEngine.ExecuteRules(IList<Rule> rules, RuleExecutionContext context)
    {
        return ExecuteRules(rules, context);
    }

    private async Task<bool> ExecuteRules(IList<Rule> rules, RuleExecutionContext context)
    {
        bool? lastConditionValue = null;
        foreach (var rule in rules)
        {
            if (lastConditionValue == true)
            {
                if (rule.ConditionType == "else" || rule.ConditionType == "else if")
                {
                    continue;
                }
            }

            if (rule.ConditionType == "if" || rule.ConditionType == "else if")
            {
                if (string.IsNullOrEmpty(rule.Condition)) throw new ArgumentException("Condition cannot be null for condition type 'if' or 'else if'");
                bool result = _evaluationEngine.EvaluateCondition(context.LocalVariables, rule.Condition);
                lastConditionValue = result;    
                if (!result) continue;
            }
            else
            {
                lastConditionValue = null;
            }

            foreach (var variableName in rule.VariablesToPreload)
            {
                await context.Preload(variableName);
            }

            context.AddRuleEvaluation(rule);    

            var continueExecution = rule.Type.ToLower() switch
            {
                "assign" => Assign(rule, context),
                "execute" => await ExecuteChildRules(rule, context),
                "validate" => Validate(rule, context),
                _ => throw new ArgumentException("Unsupported rule type")
            };
            if (!continueExecution) return false;
        }
        return true;
    }

    private bool Assign(Rule rule, RuleExecutionContext context)
    {
        if (string.IsNullOrEmpty(rule.Variable)) throw new ArgumentException("Variable cannot be null");
        if (string.IsNullOrEmpty(rule.Expression)) throw new ArgumentException("Expression cannot be null");
        
        var value = _evaluationEngine.EvaluateExpression(rule.Expression, context.LocalVariables, context.FunctionNames);
        context.SetValue(rule.Variable, value);
        return true;
    }

    private async Task<bool> ExecuteChildRules(Rule rule, RuleExecutionContext context)
    {
        if (rule.ChildRules == null) throw new ArgumentException("ChildRules cannot be null");
        return await ExecuteRules(rule.ChildRules, context);
    }

    private bool Validate(Rule rule, RuleExecutionContext context)
    {
        if (!string.IsNullOrEmpty(rule.ValidationMessageTemplate))
        {
            var message = _textTemplateEngine.Render(rule.ValidationMessageTemplate, context.LocalVariables);
            context.ValidationMessages.Add(message);
        }

        context.IsValid = false;

        if (rule.TerminateIfInvalid == true)
        {
            return false; // Stop processing further rules if termination is requested
        }

        return true;
    }

    private object? EvaluateFunction(string name, object[] parameters)
    {
        if (_functions == null) throw new ArgumentException($"Invalid function name '{name}'");
        if (_functions.TryGetValue(name, out var function) == false) throw new ArgumentException($"Invalid function name '{name}'");

        if (function.CacheResults)
        {
            var cacheKey = GetCacheKey(name, parameters);
            return _functionCache.GetOrAdd(cacheKey, _ => Invoke(function.Delegate, parameters));
        }

        return Invoke(function.Delegate, parameters);

        string GetCacheKey(string functionName, object[] args)
        {
            var keyBuilder = new StringBuilder("ext_func_" + functionName);
            foreach (var arg in args)
            {
                keyBuilder.Append($"|{arg?.GetHashCode()}");
            }
            return keyBuilder.ToString();
        }
    }

    private static object? Invoke(Delegate del, object[] args)
    {
        if (del == null)
            throw new ArgumentNullException(nameof(del));
        if (args == null)
            throw new ArgumentNullException(nameof(args));

        var expectedNoOfParameters = del.Method.GetParameters().Length;
        if (args.Length != expectedNoOfParameters)
            throw new ArgumentException(
                $"Delegate expects {expectedNoOfParameters} arguments, but you provided {args.Length}.");

        var parameters = new object?[expectedNoOfParameters];
        for (int i = 0; i < expectedNoOfParameters; i++)
        {
            var paramType = del.Method.GetParameters()[i].ParameterType;
            if (args[i] == null && paramType.IsValueType && Nullable.GetUnderlyingType(paramType) == null)
            {
                throw new ArgumentException($"Argument at position {i} is null, but the parameter type is '{paramType}' which is a non-nullable value type.");
            }
            parameters[i] = args[i] == null ? null : Convert.ChangeType(args[i], paramType);
        }

        // DynamicInvoke will automatically convert and call the delegate
        return del.DynamicInvoke(parameters);
    }
}



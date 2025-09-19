using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Xml.Linq;

namespace BRules;

/// <summary>
/// This class is used to build a new instance of a rules engine
/// </summary>
public class RulesEngineBuilder : IRulesEngineBuilder
{
    private IEvaluationEngine? _evaludationEngine = null;
    private ITextTemplateEngine? _textTemplateEngine = null;
    private IDictionary<string, RuleVariable> _variables = new Dictionary<string, RuleVariable>();
    private IDictionary<string, RuleFunction> _functions = new Dictionary<string, RuleFunction>();

    private readonly List<ParameterDef> _parameters = new();

    private RulesEngineBuilder()
    {
    }

    /// <summary>
    /// Creates and returns a new instance of a rules engine builder.
    /// </summary>
    /// <returns>An <see cref="IRulesEngineBuilder"/> instance that can be used to configure and build a rules engine.</returns>
    public static IRulesEngineBuilder Create()
    {
        return new RulesEngineBuilder();
    }

    /// <summary>
    /// Adds a parameter definition to the rules engine builder.
    /// </summary>
    /// <typeparam name="T">The type of the parameter being added.</typeparam>
    /// <param name="name">The name of the parameter. Cannot be <see langword="null"/>.</param>
    /// <param name="isOptional">Indicates whether the parameter is optional. If <see langword="null"/>, the parameter is treated as required.</param>
    /// <param name="defaultValue">The default value for the parameter, used if the parameter is optional and not provided. Can be <see
    /// langword="null"/>.</param>
    /// <returns>The current instance of <see cref="IRulesEngineBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public IRulesEngineBuilder AddParameter<T>(string name, bool? isOptional = false, object? defaultValue = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        _parameters.Add(new ParameterDef(name, typeof(T)));
        return this;
    }

    /// <summary>
    /// Adds a variable to the rules engine with the specified name, value, and optional metadata.
    /// </summary>
    /// <param name="name">The name of the variable. This value cannot be <see langword="null"/>.</param>
    /// <param name="value">The default value of the variable. This can be <see langword="null"/> if the variable does not require an
    /// initial value.</param>
    /// <param name="outputVariable">A value indicating whether the variable should be included in the output of the rules engine. The default is
    /// <see langword="false"/>.</param>
    /// <param name="dataType">The data type of the variable. This can be <see langword="null"/> if the data type is not explicitly specified.</param>
    /// <returns>The current instance of <see cref="IRulesEngineBuilder"/> to allow for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public IRulesEngineBuilder AddVariable(string name, object? value, bool outputVariable = false, Type? dataType = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        _variables.Add(name, new RuleVariable()
        {
            Type = RuleVariableType.DefaultValue,
            DataType = dataType,
            DefaultValue = value,
            OutputVariable = outputVariable
        });
        return this;
    }

    /// <summary>
    /// Add a variable whoes value is loaded at the first time using the provided value provider function.
    /// This value provider function can be asynchronous
    /// This variable can be used by the rule sets associated with the rules engine to be built
    /// This variable must be preloaded before use
    /// Note that the value of this variable must be loaded at the rules set level, if used in if conditions
    /// </summary>
    /// <remarks>This method allows you to define a variable whose value is determined lazily at runtime 
    /// using the specified <paramref name="valueProvider"/> delegate. The variable can optionally  be included in the
    /// output by setting <paramref name="outputVariable"/> to <see langword="true"/>.</remarks>
    /// <param name="name">The name of the variable. This value cannot be <see langword="null"/>.</param>
    /// <param name="valueProviderParameters">A list of parameters to be passed to the value provider delegate.</param>
    /// <param name="valueProvider">A delegate that provides the value for the variable when evaluated.</param>
    /// <param name="outputVariable">A value indicating whether the variable should be included in the output.  The default is <see
    /// langword="false"/>.</param>
    /// <param name="dataType">The expected data type of the variable. This value is optional and can be <see langword="null"/>.</param>
    /// <returns>The current instance of <see cref="IRulesEngineBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public IRulesEngineBuilder AddLazyVariable(string name, IList<string> valueProviderParameters, Delegate valueProvider, Type? dataType = null)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        _variables.Add(name, new RuleVariable()
        {
            Type = RuleVariableType.ValueProvider,
            DataType = dataType,
            ValueProvider = valueProvider,
            ValueProviderParameters = valueProviderParameters,
            OutputVariable = false
        });
        return this;
    }

    /// <summary>
    /// Add a set of variables whoes value is loaded at the first time using the provided value provider function.
    /// The output of the function should be an object with public properties that maches with the provided variable names or an IDictionary<string,object?>
    /// This value provider function can be asynchronous
    /// These variables can be used by the rule sets associated with the rules engine to be built
    /// These variables must be preloaded before use
    /// Note that the value of these variables must be loaded at the rules set level, if used in if conditions
    /// </summary>
    /// <remarks>This method allows you to define a variable whose value is determined lazily at runtime 
    /// using the specified <paramref name="valueProvider"/> delegate. The variable can optionally  be included in the
    /// output by setting <paramref name="outputVariable"/> to <see langword="true"/>.</remarks>
    /// <param name="name">The name of the variable. This value cannot be <see langword="null"/>.</param>
    /// <param name="valueProviderParameters">A list of parameters to be passed to the value provider delegate.</param>
    /// <param name="valueProvider">A delegate that provides the value for the variable when evaluated.</param>
    /// <param name="outputVariable">A value indicating whether the variable should be included in the output.  The default is <see
    /// langword="false"/>.</param>
    /// <param name="dataType">The expected data type of the variable. This value is optional and can be <see langword="null"/>.</param>
    /// <returns>The current instance of <see cref="IRulesEngineBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/>.</exception>
    public IRulesEngineBuilder AddLayerVariables(IList<string> variableNames, IList<string> valueProviderParameters, Delegate valueProvider, Type? dataType = null)
    {
        var variableId = Guid.NewGuid().ToString();
        AddLazyVariable(variableId, valueProviderParameters, valueProvider, dataType);
        foreach (var name in variableNames)
        {
            _variables.Add(name, new RuleVariable()
            {
                Type = RuleVariableType.FieldValue,
                DataType = dataType,
                FieldValue = new FieldValueVariable() { FieldName = name, RecordVariable = variableId },
                OutputVariable = false
            });
        }
        return this;
    }

    /// <summary>
    /// Adds a custom function to the rules engine with the specified name, caching behavior, and delegate
    /// implementation.
    /// </summary>
    /// <param name="name">The unique name of the function. This value cannot be <see langword="null"/>.</param>
    /// <param name="cacheResult">A value indicating whether the result of the function should be cached.  If <see langword="true"/>, the
    /// function's result will be cached for reuse; otherwise, it will be evaluated on each invocation.</param>
    /// <param name="function">The delegate that defines the implementation of the function. This value cannot be <see langword="null"/>.</param>
    /// <returns>The current instance of <see cref="IRulesEngineBuilder"/> to allow for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <see langword="null"/> or if <paramref name="function"/> is <see
    /// langword="null"/>.</exception>
    public IRulesEngineBuilder AddFunction(string name, bool cacheResult, Delegate function)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (function == null) throw new ArgumentNullException(nameof(function));

        _functions.Add(name, new RuleFunction() { Delegate = function, CacheResults = cacheResult });
        return this;
    }

    /// <summary>
    /// Configures the rules engine builder to use the specified evaluation engine.
    /// </summary>
    /// <param name="evaluationEngine">The evaluation engine to be used for evaluating rules. This parameter cannot be <see langword="null"/>.</param>
    /// <returns>The current instance of <see cref="IRulesEngineBuilder"/> to allow for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="evaluationEngine"/> is <see langword="null"/>.</exception>
    public IRulesEngineBuilder UseEvaluationEngine(IEvaluationEngine evaluationEngine)
    {
        if (evaluationEngine == null) throw new ArgumentNullException(nameof(evaluationEngine));

        _evaludationEngine = evaluationEngine;
        return this;
    }

    /// <summary>
    /// Configures the rules engine to use the specified text template engine.
    /// </summary>
    /// <param name="textTemplateEngine">The text template engine to be used for processing templates. Cannot be <see langword="null"/>.</param>
    /// <returns>The current instance of <see cref="IRulesEngineBuilder"/> to allow for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="textTemplateEngine"/> is <see langword="null"/>.</exception>
    public IRulesEngineBuilder UseTextTemplateEngine(ITextTemplateEngine textTemplateEngine)
    {
        if (textTemplateEngine == null) throw new ArgumentNullException(nameof(textTemplateEngine));

        _textTemplateEngine = textTemplateEngine;
        return this;
    }

    /// <summary>
    /// Builds and returns an instance of <see cref="IRulesEngine"/> using the provided rule sets.
    /// </summary>
    /// <param name="ruleSets">An array of <see cref="RuleSet"/> objects to be included in the rules engine. Must contain at least one rule
    /// set.</param>
    /// <returns>An instance of <see cref="IRulesEngine"/> configured with the specified rule sets, parameters, variables, and
    /// associated engines.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="ruleSets"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no expression evaluation engine is associated with the builder.</exception>
    public IRulesEngine Build(params RuleSet[] ruleSets)
    {
        if (ruleSets == null || ruleSets.Length == 0) throw new ArgumentException("Atleast one rule set must be provided");
        var evaluationEngine = _evaludationEngine ?? throw new InvalidOperationException("Cannot build a rules engine without associating an expression evaluation engine");
        var textTemplateEngine = _textTemplateEngine ?? new BasicTextTemplateEngine();

        List<Rule> rules = new();
        List<ParameterDefinition> parameters = new();
        HashSet<string> preloadVariables = new();
        Dictionary<string, RuleVariable> variables = _variables.ToDictionary(v => v.Key, v => v.Value);

        foreach (var ruleSet in ruleSets)
        {
            ValidateParameters(ruleSet);
            ValidateVarialbes(ruleSet);
            LoadRuleSet(ruleSet, rules, parameters, variables, preloadVariables);
        }

        return new RulesEngine(rules, _variables, _functions, parameters, preloadVariables, evaluationEngine, textTemplateEngine);
    }

    /// <summary>
    /// Builds an instance of <see cref="IRulesEngine"/> using the specified JSON-encoded rule sets.
    /// </summary>
    /// <param name="jsonRuleSets">An array of JSON strings, each representing a rule set to be included in the rules engine.</param>
    /// <returns>An <see cref="IRulesEngine"/> instance configured with the provided rule sets.</returns>
    public IRulesEngine Build(params string[] jsonRuleSets)
    {
        var ruleSets = jsonRuleSets
            .Select(json => System.Text.Json.JsonSerializer.Deserialize<RuleSet>(json))
            .ToArray();
        return Build(ruleSets!);
    }

    private void LoadRuleSet(RuleSet ruleSet,
        List<Rule> rules,
        List<ParameterDefinition> parameterDefs,
        Dictionary<string, RuleVariable> variables,
        HashSet<string> preloadVariables)
    {
        if (ruleSet.PreloadVariables != null) preloadVariables.UnionWith(ruleSet.PreloadVariables);

        if (ruleSet.AggregateVariables != null)
        {
            foreach (var aggregateVariable in ruleSet.AggregateVariables)
            {
                var ruleVariable = new RuleVariable()
                {
                    Type = RuleVariableType.Aggregate,
                    Aggregate = new AggregateVariable()
                    {
                        CollectionVariable = aggregateVariable.CollectionVariable,
                        Expression = aggregateVariable.Expression,
                        FilterCondition = aggregateVariable.FilterCondition,
                        AggregateFunction = aggregateVariable.AggregateFunction,
                        SubRules = aggregateVariable.SubRules != null && aggregateVariable.SubRules.Count > 0
                            ? GetRules(ruleSet, aggregateVariable.SubRules)
                            : null,
                    },
                    VariablesToPreload = aggregateVariable.VariablesToPreload,
                    OutputVariable = false
                };
                _variables.Add(aggregateVariable.Name, ruleVariable);
            }
        }

        if (ruleSet.SimpleVariables != null)
        {
            foreach (var simpleVariable in ruleSet.SimpleVariables)
            {
                _variables.Add(simpleVariable.Name, new RuleVariable()
                {
                    Type = RuleVariableType.SimpleVariable,
                    SimpleVariable = new SimpleVariable()
                    {
                        Expression = simpleVariable.Expression
                    },
                    VariablesToPreload = simpleVariable.VariablesToPreload,
                    OutputVariable = false
                });
            }
        }

        foreach (var param in ruleSet.Parameters)
        {
            if (!parameterDefs.Any(p => p.Name == param.Name))
            {
                parameterDefs.Add(new ParameterDefinition()
                {
                    Name = param.Name,
                    Type = param.Type,
                    IsOptional = param.IsOptional,
                    DefaultValue = param.DefaultValue
                });
            }
        }

        SetRules(rules, ruleSet.Rules, ruleSet);
    }

    private void ValidateParameters(RuleSet ruleSet)
    {
        foreach (var parameter in ruleSet.Parameters)
        {
            var environmentParameter = _parameters.FirstOrDefault(p => p.Name == parameter.Name);
            if (environmentParameter == null) throw new ArgumentException($"Parameter {parameter.Name} is not defined in the environment");
            if (parameter.Type != environmentParameter.Type.FullName)
            {
                throw new ArgumentException($"Parameter {parameter.Name} type mismatch. Expected {environmentParameter.Type}, but got {parameter.Type}");
            }
        }
    }

    private void ValidateVarialbes(RuleSet ruleSet)
    {
        foreach (var rule in ruleSet.Rules)
        {
            ValidateVarialbes(rule, ruleSet);
        }
    }

    private void ValidateVarialbes(RuleDefinition rule, RuleSet ruleSet)
    {
        if (rule.VariablesToPreload != null)
        {
            foreach (var variable in rule.VariablesToPreload)
            {
                if (!_variables.ContainsKey(variable) &&
                    ruleSet.AggregateVariables?.Any(v => string.Equals(v.Name, variable, StringComparison.Ordinal)) == false)
                {
                    throw new ArgumentException($"Variable {variable} used in rule {rule.Name} is not defined in the environment");
                }
            }
        }

        if (rule.SubRules != null)
        {
            foreach (var subRule in rule.SubRules)
            {
                ValidateVarialbes(subRule, ruleSet);
            }
        }
    }

    private static void SetRules(List<Rule> rules, IList<RuleDefinition> ruleDefinitions, RuleSet ruleSet)
    {
        foreach (var ruleDef in ruleDefinitions)
        {
            var existingRule = rules.FirstOrDefault(r => r.Name == ruleDef.Name);
            if (existingRule != null)
            {
                Override(existingRule, ruleSet, ruleDef);
            }
            else
            {
                rules.Add(GetRule(ruleSet, ruleDef));
            }
        }
    }

    private static void Override(Rule rule, RuleSet ruleSet, RuleDefinition ruleDef)
    {
        rule.RuleSetName = ruleSet.Name;
        rule.Version = ruleSet.Version;
        rule.ConditionType = ruleDef.ConditionType;
        rule.Condition = ruleDef.Condition;
        rule.Type = ruleDef.Type;
        rule.Variable = ruleDef.Variable;
        rule.VariablesToPreload = ruleDef.VariablesToPreload ?? new List<string>();
        rule.Expression = ruleDef.Expression;
        rule.ValidationMessageTemplate = ruleDef.ValidationMessageTemplate;
        rule.TerminateIfInvalid = ruleDef.TerminateIfInvalid;
        if (ruleDef.SubRules != null && ruleDef.SubRules.Count > 0)
        {
            rule.ChildRules = GetRules(ruleSet, ruleDef.SubRules);
        }
    }

    private static Rule GetRule(RuleSet ruleSet, RuleDefinition ruleDef)
    {
        return new Rule()
        {
            Name = ruleDef.Name,
            Type = ruleDef.Type,
            RuleSetName = ruleSet.Name,
            Version = ruleSet.Version,
            ConditionType = ruleDef.ConditionType,
            Condition = ruleDef.Condition,
            VariablesToPreload = ruleDef.VariablesToPreload ?? new List<string>(),
            Variable = ruleDef.Variable,
            Expression = ruleDef.Expression,
            ValidationMessageTemplate = ruleDef.ValidationMessageTemplate,
            TerminateIfInvalid = ruleDef.TerminateIfInvalid,
            ChildRules = ruleDef.SubRules != null && ruleDef.SubRules.Count > 0
                ? GetRules(ruleSet, ruleDef.SubRules)
                : null,
        };
    }

    private static IList<Rule> GetRules(RuleSet ruleSet, IList<RuleDefinition> ruleDefinitions)
    {
        List<Rule> rules = new List<Rule>();
        foreach (var ruleDef in ruleDefinitions)
        {
            rules.Add(GetRule(ruleSet, ruleDef));
        }
        return rules;
    }

    public class ParameterDef
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public ParameterDef(string name, Type type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }
}





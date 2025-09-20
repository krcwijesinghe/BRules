

namespace BRules;

public interface IRulesEngineBuilder
{
    IRulesEngineBuilder AddFunction(string name, bool cacheResult, Delegate function);
    IRulesEngineBuilder AddLayerVariables(IList<string> variableNames, IList<string> valueProviderParameters, Delegate valueProvider, Type? dataType = null);
    IRulesEngineBuilder AddLazyVariable(string name, IList<string> valueProviderParameters, Delegate valueProvider, Type? dataType = null);
    IRulesEngineBuilder AddParameter<T>(string name, bool? isOptional = false, object? defaultValue = null);
    IRulesEngineBuilder AddVariable(string name, object? value, bool outputVariable = false, Type? dataType = null);
    IRulesEngine Build(params BRules.RuleSet[] ruleSets);
    IRulesEngine Build(params string[] jsonRuleSets);
    IRulesEngineBuilder UseEvaluationEngine(IEvaluationEngine evaluationEngine);
    IRulesEngineBuilder UseTextTemplateEngine(ITextTemplateEngine textTemplateEngine);
}


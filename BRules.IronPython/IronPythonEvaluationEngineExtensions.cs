namespace BRules;

public static class IronPythonEvaluationEngineExtensions
{
    public static IRulesEngineBuilder UseIronPythonEvaluationEngine(this IRulesEngineBuilder self)
    {
        var engine = new IronPythonEvaluationEngine();
        return self.UseEvaluationEngine(engine);
    }
}

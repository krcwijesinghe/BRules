namespace BRules;

public interface IEvaluationEngine
{
    Func<string, object[], object?>? FunctionEvaluator { get; set; }
    object? EvaluateExpression(string expression, IDictionary<string, object?> parameters, IList<string>? functions = null);
}

public static class EvaluationEngineExtensions
{
    public static bool EvaluateCondition(this IEvaluationEngine self, IDictionary<string, object?> parameters, string conditionExpression, IList<string>? functions = null)
    {
        var conditionResult = self.EvaluateExpression(conditionExpression, parameters, functions);
        var result = conditionResult switch
        {
            bool b => b,
            short i => i != 0,
            int i => i != 0,
            long i => i != 0,
            ushort i => i != 0,
            uint i => i != 0,
            ulong i => i != 0,
            float d => d != 0.0,
            double d => d != 0.0,
            decimal d => d != 0.0M,
            string s => !string.IsNullOrEmpty(s),
            null => false,
            _ => true
        };
        return result;
    }
}
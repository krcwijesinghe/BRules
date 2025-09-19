namespace BRules;

internal class AggregateVariable
{
    public required string CollectionVariable { get; set; }
    public required string Expression { get; set; }
    public required string? FilterCondition { get; set; }
    public required  string AggregateFunction { get; set; }
    public IList<Rule>? SubRules { get; set; }

    public async Task<object?> GetValue(RuleExecutionContext context)
    {
        var collection = await context.GetValue(CollectionVariable) as IEnumerable<IDictionary<string, object?>>;
        if (collection == null)
        {
            throw new InvalidOperationException($"Collection variable '{CollectionVariable}' is not found or is not a collection.");
        }

        if (SubRules != null && SubRules.Count > 0)
        {
            var updatedCollection = new List<IDictionary<string, object?>>();
            foreach (var item in collection)
            {
                var subContext = context.Clone();
                foreach (var field in item)
                {
                    subContext.LocalVariables[field.Key] = field.Value;    
                }
                await context.RuleExecutionEngine.ExecuteRules(SubRules, subContext);
                updatedCollection.Add(subContext.LocalVariables);
            }
            collection = updatedCollection;
        }

        if (!string.IsNullOrEmpty(FilterCondition))
        {
            collection = collection.Where(IsFiltered);
        }

        return AggregateFunction switch
        {
            "Sum" => collection.Sum(row => Convert.ToDecimal(GetValue(row))),
            "Count" => collection.Count(),
            "Average" => collection.Average(row => Convert.ToDecimal(GetValue(row))),
            "Min" => collection.Min(row => Convert.ToDecimal(GetValue(row))),
            "Max" => collection.Max(row => Convert.ToDecimal(GetValue(row))),
            "All" => collection.All(row => Convert.ToBoolean(GetValue(row))),
            "Any" => collection.Any(row => Convert.ToBoolean(GetValue(row))),
            _ => throw new NotSupportedException($"Aggregate function '{AggregateFunction}' is not supported.")
        };

        object? GetValue(IDictionary<string, object?> row)
        {
            return context.EvaludationEngine.EvaluateExpression(Expression, row);
        }

        bool IsFiltered(IDictionary<string, object?> row)
        {
            return context.EvaludationEngine.EvaluateCondition(row, FilterCondition);
        }
    }
}




namespace BRules;

internal class FieldValueVariable
{
    public required string RecordVariable { get; set; }
    public required string FieldName { get; set; }

    public async Task<object?> GetValue(RuleExecutionContext context)
    {
        var record = await context.GetValue(RecordVariable);
        if (record is IDictionary<string, object?> dict)
        {
            return dict.TryGetValue(FieldName, out var value) ? value : null;
        }
        else if (record != null)
        {
            var property = record.GetType().GetProperty(FieldName);
            if (property != null)
            {
                return property.GetValue(record);
            }
            throw new InvalidOperationException($"Field '{FieldName}' not found in record variable '{RecordVariable}'.");
        }
        else
        {
            throw new InvalidOperationException($"Record variable '{RecordVariable}' is null.");
        }
    }
}




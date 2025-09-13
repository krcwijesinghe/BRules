namespace SampleWebApiApp.Repositories;

public class RuleSetRepository : IRuleSetRepository
{
    public string? GetRuleSetJsonForAction1(int departmentId)
    {
        // In a real application, this method would retrieve the rule set from a database or other storage.
        // Here, we return a hardcoded JSON string for demonstration purposes.
        if (departmentId == 1)
        {
            return """
            {
                "Name": "Global",
                "Version": "1.0",
                "Rules": [
                    {
                        "Name": "TestRule1",
                        "ConditionType": "if",
                        "Condition": "param1 < 0",
                        "Type": "validate",
                        "ValidationMessageTemplate": "Parameter param1 cannot be negative"
                    },
                    {
                        "Name": "TestRule2",
                        "ConditionType": "if",
                        "Condition": "totalAmount > 2000",
                        "Type": "assign",
                        "Variable": "command",
                        "Expression": "'CommandA'"
                    },
                    {
                        "Name": "TestRule3",
                        "Type": "assign",
                        "Variable": "commandParameter1",
                        "Expression": "param1 * 10"
                    }
                ],
                "Parameters": [
                    {
                        "Name": "param1",
                        "Type": "System.Int32",
                        "IsOptional": false
                    }
                ],
                "PreloadVariables": ["totalAmount"],
                "AggregateVariables": [
                    {
                        "Name": "totalAmount",
                        "CollectionVariable": "collection1",
                        "Expression": "amount",
                        "AggregateFunction": "Sum"
                    }
                ]
            }
            """;
        };
        return null;
    }
}

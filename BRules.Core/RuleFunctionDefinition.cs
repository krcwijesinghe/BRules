

namespace BRules;

public class RuleFunctionDefinition
{
    public required string Name { get; set; }
    public required string Expression { get; set; }
    public required IList<string> Parameters { get; set; }
    public IList<string>? VariablesToPreload { get; set; }
}


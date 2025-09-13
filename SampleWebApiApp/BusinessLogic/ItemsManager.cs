using BRules;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SampleWebApiApp.Repositories;

namespace SampleWebApiApp.BusinessLogic;

public class ItemsManager(
    IRuleSetRepository ruleSetRepository,
    IItemsRepository itemsRepository) : IItemsManager
{
    public async Task<Action1Response> HandleAction1Async(int departmentId, int param1)
    {
        var ruleSetJson = ruleSetRepository.GetRuleSetJsonForAction1(departmentId);
        if (ruleSetJson == null)
        {
            return new Action1Response
            {
                IsValid = false,
                ErrorMessage = "Invalid department id"
            };
        }

        var rulesEngine = RulesEngineBuilder.Create()
                            .UseJintEvaluationEngine()
                            .AddParameter<int>("param1")
                            .AddLazyVariable("collection1", ["param1"], GetCollection1)
                            .AddVariable("command", "none", outputVariable: true)
                            .AddVariable("commandParameter1", 0, outputVariable: true, dataType: typeof(double))
                            .Build(ruleSetJson);
        var result = await rulesEngine.ExecuteAsync(new Dictionary<string, object?> {
            { "param1", param1 }
        });

        if (!result.IsValid)
        {
            return new Action1Response
            {
                IsValid = false,
                ErrorMessage = string.Join("; ", result.ValidationMessages)
            };
        }

        var command = result.OutputParameters["command"]!.ToString();
        var commandParameter1 = (double) result.OutputParameters["commandParameter1"]!;
        var response = PerformAction1Async(command!, (int) commandParameter1);
        return new Action1Response()
        {
            IsValid = true,
            Response = response
        };
    }

    private string PerformAction1Async(string command, int parameter1)
    {
        switch (command)
        {
            case "CommandA":
                return $"Command A performed with parameter {parameter1}";
            case "CommandB":
                return $"Command B performed with parameter {parameter1}";
            default:
                return "No action performed";
        }
    }

    private async Task<IList<IDictionary<string, object?>>> GetCollection1(int param1)
    {
        var items = await itemsRepository.GetItems(param1);
        var rows = items.Select(item => (IDictionary<string, object?>) new Dictionary<string, object?>()
        {
            ["id"] = item.id,
            ["name"] = item.name,
            ["amount"] = item.amount
        }).ToList();
        return rows;
    }
}


using BRules;
using System.Text.Json;

var ruleSet = new RuleSet()
{
    Name = "SalaryCalculation",
    Version = "1.0",
    Parameters = [
        new ParameterDefinition { Name = "Id", Type = "System.Int32", IsOptional = false },
        new ParameterDefinition { Name = "DepartmentId", Type = "System.Int32", IsOptional = false },
        new ParameterDefinition { Name = "CompanyCode", Type = "System.String", IsOptional = false },
        new ParameterDefinition { Name = "BasicSalary", Type = "System.Double", IsOptional = false },
        new ParameterDefinition { Name = "TotalOtHours", Type = "System.Double", IsOptional = false },
        new ParameterDefinition { Name = "TotalNoPayDays", Type = "System.Double", IsOptional = false },
    ],
    SimpleVariables = [
        new SimpleVariableDefinition { Name = "Threshold", Expression = "10000"},
        new SimpleVariableDefinition { Name = "additions", Expression = "0.06"},
        new SimpleVariableDefinition { Name = "Rate", Expression = "0.06"},
    ],
    RuleFunctionDefinitions = [
        new RuleFunctionDefinition 
        { 
            Name = "CalculateTax", 
            Parameters = ["grossSalary"], 
            VariablesToPreload = ["Threshold", "Rate"],
            Expression = "grossSalary > threshold ? (grossSalary - threshold) * rate : 0"
        },
        new RuleFunctionDefinition
        {
            Name = "Min",
            Parameters = ["v1","v2"],
            Expression = "v1 < v2 ? v1 : v2"
        },
    ],
    Rules = [
        new RuleDefinition {
            Name = "TotalOtHours-1",
            Type = "assign",
            VariablesToPreload = ["TotalOtHours"],
            Variable = "EligibleOtHours",
            Expression = "TotalOtHours"
        },
        new RuleDefinition {
            Name = "TotalOtHours-2",
            Type = "execute",
            ConditionType = "if",
            Condition = "CompanyCode == 'ACME'",
            SubRules = [
                new RuleDefinition {
                    Name = "TotalOtHours-2.1",
                    Type = "assign",
                    ConditionType = "if",
                    VariablesToPreload = ["OtHourlyRate", "TotalOtHours"],
                    Condition = "DepartmentId == 101",
                    Variable = "EligibleOtHours",
                    Expression = "TotalOtHours * OtHourlyRate"
                },
                new RuleDefinition {
                    Name = "TotalOtHours-2.2",
                    Type = "assign",
                    ConditionType = "if",
                    VariablesToPreload = ["TotalOtHours"],
                    Condition = "DepartmentId == 102",
                    Variable = "EligibleOtHours",
                    Expression = "Min(TotalOtHours, 20)"
                }
            ]
        }
    ]
};


var rateProvider = new RateProvider();

var rulesEngineBuilder = RulesEngineBuilder.Create()
                    .UseJintEvaluationEngine()
                    .AddParameter<int>("Id")
                    .AddParameter<int>("DepartmentId")
                    .AddParameter<string>("CompanyCode")
                    .AddParameter<double>("TotalNoPayDays")
                    .AddParameter<double>("BasicSalary")
                    .AddParameter<double>("TotalOtHours")
                    .AddLazyVariables(
                        ["OtHourlyRate", "NoPayDailyRate", "EpfEmployeePct", "EpfCompanyPct", "EtfPct"], 
                        ["CompanyCode"],
                        rateProvider.GetPolicy)
                    .AddLazyVariable(
                        "MvplEstateOtHourRatio", 
                        ["DepartmentId"],
                        GetMvplEstateOtHourRatioFromDb)
                    .AddLazyVariable(
                        "TotalPluckedHours",
                        ["Id"],
                        GetTotalPluckedHoursFromDb)
                    .AddLazyVariable(
                        "ToalHoursWorked",
                        ["Id"],
                        GetToalHoursWorkedFromDb)
                    .AddVariable("GrossSalary", 0, outputVariable: true, dataType: typeof(double))
                    .AddVariable("EpfEmployee", 0, outputVariable: true, dataType: typeof(double))
                    .AddVariable("EpfCompany", 0, outputVariable: true, dataType: typeof(double))
                    .AddVariable("Etf", 0, outputVariable: true, dataType: typeof(double))
                    .AddVariable("NetSalary", 0, outputVariable: true, dataType: typeof(double))
                    .AddVariable("NetSalaryAfterTax", 0, outputVariable: true, dataType: typeof(double));

var rulesEngine = rulesEngineBuilder.Build(ruleSet);

const string companyCode = "ACME";
var employess = GetEmployees();
foreach (var employee in employess)
{
    var result = await rulesEngine.ExecuteAsync(new Dictionary<string, object?>()
    {
        ["Id"] = employee.Id,
        ["DepartmentId"] = employee.DepartmentId,
        ["TotalNoPayDays"] = employee.TotalNoPayDays,
        ["BasicSalary"] = employee.BasicSalary,
        ["TotalOtHours"] = employee.TotalOtHours,
        ["CompanyCode"] = companyCode
    });

    if (!result.IsValid)
    {
        Console.WriteLine($"Failed to calculate salary for {employee.Name} due to validation error: ");
        foreach (var message in result.ValidationMessages)
        {
            Console.WriteLine($"\t{message}");
        }
        continue;
    }

    employee.GrossSalary = Round2((double)result.OutputParameters["GrossSalary"]!);
    employee.EpfEmployee = Round2((double)result.OutputParameters["EpfEmployee"]!);
    employee.EpfCompany = Round2((double)result.OutputParameters["EpfCompany"]!);
    employee.Etf = Round2((double)result.OutputParameters["Etf"]!);
    employee.NetSalary = Round2((double)result.OutputParameters["NetSalary"]!);
    employee.NetSalaryAfterTax = Round2((double)result.OutputParameters["NetSalaryAfterTax"]!);
}

var outputJson = JsonSerializer.Serialize(employess, new JsonSerializerOptions() { WriteIndented = true });
Console.WriteLine(outputJson);
Console.WriteLine("Done");

static IList<Employee> GetEmployees()
{
    return [
        new Employee { Id = 1, Name = "John Doe",  DepartmentId = 101, CompanyCode = "HPL", BasicSalary = 5000, TotalOtHours = 10, TotalNoPayDays = 0.5 },
        new Employee { Id = 2, Name = "Jane Smith", DepartmentId = 102, CompanyCode = "MVPL", BasicSalary = 6000, TotalOtHours =  6, TotalNoPayDays = 0   }
    ];
}

static double GetMvplEstateOtHourRatioFromDb(int departmentId)
{
    return 5;
}

static double GetTotalPluckedHoursFromDb(int id)
{
    return Random.Shared.NextDouble();
}

static double GetToalHoursWorkedFromDb(int id)
{
    return Random.Shared.NextDouble();
}

static double Round2(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

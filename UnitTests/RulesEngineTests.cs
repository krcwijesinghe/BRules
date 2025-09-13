using BRules;
using Jint;

namespace UnitTests
{
    [TestClass]
    public sealed class RulesEngineTests
    {
        [TestMethod]
        public async Task Single_rule_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule",
                            Type = "assign",
                            Variable = "result", 
                            Expression = "param1 + param2"}
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(9.0, result.OutputParameters["result"]);
            Assert.AreEqual(1, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule"));
        }

        [TestMethod]
        public async Task Ruleset_as_JSON_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build("""
                {
                    "Name": "Global",
                    "Version": "1.0",
                    "Rules": [
                        {
                            "Name": "TestRule",
                            "Type": "assign",
                            "Variable": "result",
                            "Expression": "param1 + param2"
                        }
                    ],
                    "Parameters": [
                        {
                            "Name": "param1",
                            "Type": "System.Int32"
                        },
                        {
                            "Name": "param2",
                            "Type": "System.Int32"
                        }
                    ]
                }
                """);

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(9.0, result.OutputParameters["result"]);
            Assert.AreEqual(1, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule"));
        }

        [TestMethod]
        public async Task Single_rule_with_positive_condition_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule",
                            ConditionType = "if",
                            Condition = "param1 < 10", 
                            Type = "assign",
                            Variable = "result", 
                            Expression = "param1 + param2"}
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(9.0, result.OutputParameters["result"]);
            Assert.AreEqual(1, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule"));
        }

        [TestMethod]
        public async Task Rule_with_else_condition_type_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            ConditionType = "if",
                            Condition = "param1 < 10",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 1"},
                        new (){
                            Name = "TestRule2",
                            ConditionType = "else",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 10"},
                        new (){
                            Name = "TestRule3",
                            ConditionType = "if",
                            Condition = "param1 > 10",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 100"},
                        new (){
                            Name = "TestRule4",
                            ConditionType = "else",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 1000"},
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(1001.0, result.OutputParameters["result"]);
            Assert.AreEqual(2, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule4"));
        }


        [TestMethod]
        public async Task Rule_with_else_if_condition_type_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            ConditionType = "if",
                            Condition = "param1 < 5",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 1"},
                        new (){
                            Name = "TestRule2",
                            ConditionType = "else if",
                            Condition = "param1 < 10",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 10"},
                        new (){
                            Name = "TestRule3",
                            ConditionType = "else",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 100"},
                        new (){
                            Name = "TestRule4",
                            ConditionType = "if",
                            Condition = "param1 < 4",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 1000"},
                        new (){
                            Name = "TestRule5",
                            ConditionType = "else if",
                            Condition = "param1 < 10",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 10000"},
                        new (){
                            Name = "TestRule6",
                            ConditionType = "else",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 100000"},
                        new (){
                            Name = "TestRule7",
                            ConditionType = "if",
                            Condition = "param1 < 2",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 1000000"},
                        new (){
                            Name = "TestRule8",
                            ConditionType = "else if",
                            Condition = "param1 < 4",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 10000000"},
                        new (){
                            Name = "TestRule9",
                            ConditionType = "else",
                            VariablesToPreload = ["result"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "result + 100000000"},

                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(100010001.0, result.OutputParameters["result"]);
            Assert.AreEqual(3, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule5"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule9"));
        }


        [TestMethod]
        public async Task Children_rules_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            Type = "execute",
                            Condition = "param1 < 10",
                            SubRules = [
                                new RuleDefinition
                                {
                                    Name = "ChildRule1",
                                    Type = "assign",
                                    Variable = "result",
                                    Expression = "param1 + param2"
                                },
                                new RuleDefinition
                                {
                                    Name = "ChildRule2",
                                    Type = "assign",
                                    Variable = "result",
                                    Expression = "result * 2"
                                }
                            ]
                        },
                        new RuleDefinition
                        {
                            Name = "TestRule2",
                            Type =  "assign",
                            Variable = "result",
                            Expression = "result * 3"
                        }
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(54.0, result.OutputParameters["result"]);
            Assert.AreEqual(4, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).ChildRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).ChildRule2"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule2"));
        }

        [TestMethod]
        public async Task Validation_rules_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            Condition = "param1 < 10",
                            Type = "execute",
                            SubRules = [
                                new RuleDefinition
                                {
                                    Name = "ChildRule1",
                                    Type = "assign",
                                    Variable = "result",
                                    Expression = "param1 + param2"
                                },
                                new RuleDefinition
                                {
                                    Name = "ChildValidationRule",
                                    ConditionType = "if",
                                    Condition = "result < 10",
                                    Type = "validate",
                                    ValidationMessageTemplate = "Result ({{result}}) should be less than or equal to 10",
                                    TerminateIfInvalid = true,
                                },
                                new RuleDefinition
                                {
                                    Name = "ChildRule2",
                                    Type = "assign",
                                    Variable = "result",
                                    Expression = "result * 2"
                                }
                            ]
                        },
                        new RuleDefinition
                        {
                            Name = "TestRule2",
                            Type =  "assign",
                            Variable = "result",
                            Expression = "result * 3"
                        }
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.ValidationMessages.Count);
            Assert.IsTrue(result.ValidationMessages.Contains("Result (9) should be less than or equal to 10"));

            Assert.AreEqual(3, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).ChildRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).ChildValidationRule"));
        }


        [TestMethod]
        public async Task Single_rule_with_negative_condition_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(
                    new RuleSet("Global", "1.0", 
                    [
                        new (){
                            Name = "TestRule", 
                            ConditionType = "if",
                            Condition = "param1 > 10", 
                            Type = "assign",
                            Variable = "result", 
                            Expression = "param1 + param2"}
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(0, result.OutputParameters["result"]);
        }

        [TestMethod]
        public async Task Mutiple_rules_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .AddVariable("result1", 0)
                .AddVariable("result2", 0)
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            ConditionType = "if",
                            Condition = "param1 < 5", 
                            Type = "assign",
                            Variable = "result1", 
                            Expression = "param1 + param2"
                        },
                        new (){
                            Name = "TestRule2",
                            ConditionType = "if",
                            Condition = "param2 < 5",
                            Type = "assign",
                            Variable = "result2", 
                            Expression = "param1 * 100"
                        },
                        new (){
                            Name = "TestRule3",
                            Type = "assign",
                            Variable = "result", 
                            Expression = "result1 + result2 + 1"
                        }
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ],
                    preloadVariables: ["result1", "result2"]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 },
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(10.0, result.OutputParameters["result"]);
            Assert.AreEqual(2, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule3"));
        }

        [TestMethod]
        public async Task Rule_overrid_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddVariable("result1", 0)
                .AddVariable("result2", 0)
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(new RuleSet("Global", "1.0", [
                    new (){
                        Name = "TestRule1",
                        ConditionType = "if",
                        Condition = "param1 == 5",
                        Type = "assign",
                        Variable = "result1",
                        Expression = "param1 + param2"},
                    new (){
                        Name = "TestRule3",
                        Type = "assign",
                        Variable = "result",
                        VariablesToPreload = ["result1", "result2"],
                        Expression = "result1 + result2 + 1"}
                ],
                [
                    new () { Name = "param1", Type = typeof(int).FullName! },
                    new () { Name = "param2", Type = typeof(int).FullName! },
                ]),
                new RuleSet("Estate1", "1.0", [
                    new (){
                        Name = "TestRule1",
                        ConditionType = "if",
                        Condition = "param1 < 5",
                        Type = "assign",
                        Variable = "result1",
                        Expression = "param1 + param2"},
                    new (){
                        Name = "TestRule2",
                        ConditionType = "if",
                        Condition = "param2 < 5",
                        Type = "assign",
                        Variable = "result2",
                        Expression = "param1 * 100"},
                ],
                [
                    new () { Name = "param1", Type = typeof(int).FullName! },
                    new () { Name = "param2", Type = typeof(int).FullName! },
                ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 },
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(10.0, result.OutputParameters["result"]);
            Assert.AreEqual(2, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Estate1(1.0).TestRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule3"));
        }

        [TestMethod]
        public async Task Lazy_loading_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddVariable("result1", 0)
                .AddVariable("result2", 0)
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .AddLazyVariable("param3", ["param1"], async (int p1) => p1 * 25) // lazy loading value for param 3
                .AddLazyVariable("param4", [], async () => await Task.FromException<object?>(new Exception("Invalid lazy loading"))) // lazy loading value for param 4
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            ConditionType = "if",
                            Condition = "param1 < 5",
                            Type = "assign",
                            VariablesToPreload = ["param3"],
                            Variable = "result1",
                            Expression = "param3 + param2"
                        },
                        new (){
                            Name = "TestRule2",
                            ConditionType = "if",
                            Condition = "param2 < 5",
                            Type = "assign",
                            Variable = "result2",
                            VariablesToPreload = ["param4"],
                            Expression = "param4 * 100"
                        },
                        new (){
                            Name = "TestRule3",
                            Type = "assign",
                            Variable = "result",
                            VariablesToPreload = ["result1", "result2"],
                            Expression = "result1 + result2"
                        }
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 },
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(105.0, result.OutputParameters["result"]);
            Assert.AreEqual(2, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule3"));
        }

        [TestMethod]
        public async Task Custom_functions_should_be_evaluated_correctly()
        {
            bool func1Executed = false;
            var func2Executed = false;

            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .AddFunction("Func1", true, (int a, int b) => 
                {
                    var result = !func1Executed ? a + b : 100;
                    func1Executed = true;
                    return result;
                })
                .AddFunction("Func2", false, (int a, int b) =>
                {
                    var result = !func2Executed ? a * b : 100;
                    func2Executed = true;
                    return result;
                })
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            Type = "assign",
                            Variable = "result1",
                            Expression = "Func1(param1, param2)"
                        },
                        new (){
                            Name = "TestRule2",
                            Type = "assign",
                            Variable = "result1",
                            Expression = "Func1(param1, param2)"
                        },
                        new (){
                            Name = "TestRule3",
                            Type = "assign",
                            Variable = "result2",
                            Expression = "Func2(param1, param2)"
                        },
                        new (){
                            Name = "TestRule4",
                            Type = "assign",
                            Variable = "result2",
                            Expression = "Func2(param1, param2)"
                        },
                        new (){
                            Name = "TestRule5",
                            Type = "assign",
                            Variable = "result",
                            Expression = "result1 + result2"
                        }
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 },
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual((double) 109, result.OutputParameters["result"]); // 9 + 100
        }

        [TestMethod]
         public async Task Aggregate_variables_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddVariable("result1", 0)
                .AddVariable("result2", 0)
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .AddLazyVariable("employees", [], async () => new List<IDictionary<string, object?>>()
                {
                    new Dictionary<string, object?> {
                        ["Id"] = 1,
                        ["Name"] = "Alice",
                        ["Permenant"] = true,
                        ["Salary"] = 1000
                    },
                    new Dictionary<string, object?> {
                        ["Id"] = 2,
                        ["Name"] = "Bob",
                        ["Permenant"] = false,
                        ["Salary"] = 2000
                    },
                    new Dictionary<string, object?> {
                        ["Id"] = 3,
                        ["Name"] = "Charlie",
                        ["Permenant"] = true,
                        ["Salary"] = 1500
                    }
                })
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            ConditionType = "if",
                            Condition = "param1 > 50", // true
                            VariablesToPreload = ["TotalPermenantEmployeeSalary"], // 1000 + 1500 = 2500
                            Type = "assign",
                            Variable = "result1",
                            Expression = "TotalPermenantEmployeeSalary + param1" // 2500 + 100 = 2600
                        },
                        new (){
                            Name = "TestRule2",
                            ConditionType = "if",
                            Condition = "AllArePermenant", // false
                            Type = "assign",
                            Variable = "result2", //left with default value 0
                            Expression = "param2"
                        },
                        new (){
                            Name = "TestRule3",
                            VariablesToPreload = ["result1", "result2"],
                            Variable = "result",
                            Type = "assign",
                            Expression = "result1 + result2" // 2600 + 0 = 2600
                        }
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ],
                    preloadVariables: ["AllArePermenant"],
                    aggregateVariable: new List<AggregateVariableDefinition>()
                    {
                        new AggregateVariableDefinition
                        {
                            Name = "TotalPermenantEmployeeSalary",
                            CollectionVariable = "employees",
                            Expression = "Salary",
                            FilterCondition = "Permenant == true",
                            AggregateFunction = "Sum"
                        },
                        new AggregateVariableDefinition
                        {
                            Name = "AllArePermenant",
                            CollectionVariable = "employees",
                            Expression = "Permenant == true",
                            AggregateFunction = "All"
                        }
                    }));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 100 },
                { "param2", 200 },
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(2600.0, result.OutputParameters["result"]);
            Assert.AreEqual(2, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule1"));
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule3"));
        }

        [TestMethod]
        public async Task Aggregate_variables_with_subrules_should_be_evaluated_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddVariable("result1", 0)
                .AddVariable("result2", 0)
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .AddLazyVariable("employees", [], async () => new List<IDictionary<string, object?>>()
                {
                    new Dictionary<string, object?> {
                        ["Id"] = 1,
                        ["Name"] = "Alice",
                        ["Permenant"] = true,
                        ["Salary"] = 1000
                    },
                    new Dictionary<string, object?> {
                        ["Id"] = 2,
                        ["Name"] = "Bob",
                        ["Permenant"] = false,
                        ["Salary"] = 2000
                    },
                    new Dictionary<string, object?> {
                        ["Id"] = 3,
                        ["Name"] = "Charlie",
                        ["Permenant"] = true,
                        ["Salary"] = 1500
                    }
                })
                .Build(
                    new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule1",
                            VariablesToPreload = ["TotalTax"],
                            Type = "assign",
                            Variable = "result",
                            Expression = "TotalTax"
                        }
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ],
                    aggregateVariable: new List<AggregateVariableDefinition>()
                    {
                        new AggregateVariableDefinition
                        {
                            Name = "TotalTax",
                            CollectionVariable = "employees",
                            Expression = "Tax",
                            AggregateFunction = "Sum",
                            SubRules = [
                                new (){
                                    Name = "TaxRule",
                                    Type = "assign",
                                    Variable = "Tax",
                                    Expression = "Permenant == true? Salary * 0.2: Salary * 0.1"
                                }
                            ]
                        },
                    }));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 100 },
                { "param2", 200 },
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(700.0, result.OutputParameters["result"]); 
            Assert.AreEqual(1, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule1"));
        }

        [TestMethod]
        public async Task Parameter_loading_should_work_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine()
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule",
                            Type = "assign",
                            Variable = "result",
                            Expression = "param1 + param2"}
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName!, IsOptional = true, DefaultValue = 3},
                    ]));

            var result = await tagert.ExecuteAsync(new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 },
                { "param3", 6 }
            });
            Assert.IsNotNull(result);
            Assert.AreEqual(9.0, result.OutputParameters["result"]);

            result = await tagert.ExecuteAsync(new Dictionary<string, object?>
            {
                { "param1", 4 },
            });
            Assert.IsNotNull(result);
            Assert.AreEqual(7.0, result.OutputParameters["result"]);

            bool noException = false;
            try
            {
                result = await tagert.ExecuteAsync(new Dictionary<string, object?>
                {
                    { "param2", 5 }
                });
                noException = true;
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Missing required parameter: param1", ex.Message);
            }
            Assert.IsFalse(noException);

            bool noException2 = false;
            try
            {
                var tagert2 = RulesEngineBuilder.Create()
                    .UseJintEvaluationEngine()
                    .AddParameter<int>("param1")
                    .AddVariable("result", 0, outputVariable: true)
                    .Build(new RuleSet("Global", "1.0", [
                            new (){
                                Name = "TestRule",
                                Type = "assign",
                                Variable = "result",
                                Expression = "param1 + param2"}
                        ],
                        [
                            new () { Name = "param1", Type = typeof(int).FullName! },
                            new () { Name = "param2", Type = typeof(int).FullName!, IsOptional = true, DefaultValue = 3},
                        ]));
                noException2 = true;
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Parameter param2 is not defined in the environment", ex.Message);
            }
            Assert.IsFalse(noException2);
        }

        [TestMethod]
        public async Task JInt_engine_should_be_configurable_correctly()
        {
            var tagert = RulesEngineBuilder.Create()
                .UseJintEvaluationEngine(options => {
                    options.LimitMemory(4_000_000);
                    options.TimeoutInterval(TimeSpan.FromSeconds(4));
                    options.MaxStatements(1000);
                })
                .AddParameter<int>("param1")
                .AddParameter<int>("param2")
                .AddVariable("result", 0, outputVariable: true)
                .Build(new RuleSet("Global", "1.0", [
                        new (){
                            Name = "TestRule",
                            Type = "assign",
                            Variable = "result",
                            Expression = "param1 + param2"
                        }
                    ],
                    [
                        new () { Name = "param1", Type = typeof(int).FullName! },
                        new () { Name = "param2", Type = typeof(int).FullName! },
                    ]));

            var parameters = new Dictionary<string, object?>
            {
                { "param1", 4 },
                { "param2", 5 }
            };

            var result = await tagert.ExecuteAsync(parameters);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OutputParameters.ContainsKey("result"));
            Assert.AreEqual(9.0, result.OutputParameters["result"]);
            Assert.AreEqual(1, result.EvaluatedRules.Count);
            Assert.IsTrue(result.EvaluatedRules.Contains("Global(1.0).TestRule"));
        }
    }
}

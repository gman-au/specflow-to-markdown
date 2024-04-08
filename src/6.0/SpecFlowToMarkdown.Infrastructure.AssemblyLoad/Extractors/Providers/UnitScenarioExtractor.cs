using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Providers
{
    public class UnitScenarioExtractor : IScenarioExtractor
    {
        private readonly IEnumerable<string> _testCaseAttributes = new[]
        {
            Constants.NUnitTestCaseAttribute,
            Constants.XUnitInlineDataAttribute
        };

        public bool IsApplicable(string attributeName) =>
            attributeName.Equals(Constants.NUnitTestAttribute) ||
            attributeName.Equals(Constants.XUnitFactAttribute) ||
            attributeName.Equals(Constants.XUnitTheoryAttribute);
        
        private readonly ILogger<FeatureExtractor> _logger;

        public UnitScenarioExtractor(ILogger<FeatureExtractor> logger)
        {
            _logger = logger;
        }

        public SpecFlowScenario ExtractScenario(MethodDefinition method)
        {
            // Extract Scenario
            var title = string.Empty;
            var description = string.Empty;
            var tags = new List<string>();
            var scenarioCases = new List<SpecFlowCase>();

            var scenarioArgumentNames = new Dictionary<string, string>();
            var scenarioArgumentValues = new List<IEnumerable<object>>();

            var testCases =
                method
                    .CustomAttributes
                    .Count(o => _testCaseAttributes.Contains(o.AttributeType.FullName));

            foreach (var instruction in
                     method
                         .Body
                         .Instructions
                         .Where(o => o.OpCode == OpCodes.Newobj))
            {
                if (instruction.Operand is MethodReference mr)
                {
                    if (mr.DeclaringType.FullName == Constants.ScenarioInfoTypeName)
                    {
                        var currInstr =
                            instruction
                                .StepPrevious(5);

                        if (currInstr.OpCode == OpCodes.Ldstr)
                        {
                            description =
                                currInstr
                                    .Operand
                                    .ToString();
                        }

                        currInstr =
                            currInstr
                                .Previous;

                        if (currInstr.OpCode == OpCodes.Ldstr)
                        {
                            title =
                                currInstr
                                    .Operand
                                    .ToString();
                                                
                            _logger
                                .LogInformation($"Extracted scenario: [{title}]");
                        }

                        // Extract test cases
                        if (testCases > 0)
                        {
                            _logger
                                .LogInformation($"Scenario [{title}]; found {testCases} test cases");
                            
                            // Get test case argument names
                            currInstr =
                                currInstr
                                    .StepPrevious(3);

                            while (true)
                            {
                                string argKey = null;
                                string argValue = null;

                                if (currInstr.OpCode == OpCodes.Ldarg_S)
                                {
                                    argKey =
                                        currInstr
                                            .Operand
                                            .ToString();
                                }
                                else if (currInstr.OpCode == OpCodes.Ldarg_3)
                                    argKey = "3";
                                else if (currInstr.OpCode == OpCodes.Ldarg_2)
                                    argKey = "2";
                                else if (currInstr.OpCode == OpCodes.Ldarg_1)
                                    argKey = "1";
                                else if (currInstr.OpCode == OpCodes.Ldarg_0)
                                    argKey = "0";

                                var stringLoadInstruction =
                                    currInstr
                                        .Previous;

                                if (stringLoadInstruction.OpCode == OpCodes.Ldstr)
                                {
                                    argValue =
                                        stringLoadInstruction
                                            .Operand
                                            .ToString();
                                }

                                if (!string.IsNullOrEmpty(argKey) && !string.IsNullOrEmpty(argValue))
                                {
                                    scenarioArgumentNames
                                        .Add(
                                            argKey,
                                            argValue
                                        );

                                    currInstr =
                                        currInstr
                                            .StepPrevious(5);
                                }
                                else
                                    break;
                            }

                            currInstr =
                                currInstr
                                    .StepPrevious(15);

                            // Get test case argument values
                            var caseAttributes =
                                method
                                    .CustomAttributes
                                    .Where(o => _testCaseAttributes.Contains(o.AttributeType.FullName))
                                    .ToList();

                            foreach (var caseAttribute in caseAttributes)
                            {
                                var constructorArguments =
                                    caseAttribute
                                        .ConstructorArguments;

                                if (constructorArguments.Count == 1)
                                {
                                    var constructorArgument = constructorArguments[0];
                                    if (constructorArgument.Type.IsArray)
                                    {
                                        if (constructorArgument.Value is CustomAttributeArgument[] args)
                                        {
                                            var values =
                                                args
                                                    .Select(
                                                        o =>
                                                        {
                                                            if (o.Value is CustomAttributeArgument ca)
                                                            {
                                                                return
                                                                    ca
                                                                        .Value?
                                                                        .ToString();
                                                            }

                                                            return null;
                                                        }
                                                    )
                                                    .ToList();

                                            scenarioArgumentValues
                                                .Add(values);
                                        }
                                    }
                                }
                            }

                            scenarioArgumentNames =
                                scenarioArgumentNames
                                    .Reverse()
                                    .ToDictionary(
                                        o => o.Key,
                                        o => o.Value
                                    );

                            for (var i = 0; i < scenarioArgumentValues.Count; i++)
                            {
                                var argumentList =
                                    scenarioArgumentNames
                                        .Select(
                                            o =>
                                                new SpecFlowArgument
                                                {
                                                    ArgumentName = o.Value,
                                                    ArgumentValue =
                                                        scenarioArgumentValues[i]
                                                            .ElementAt(
                                                                scenarioArgumentNames
                                                                    .Keys
                                                                    .ToList()
                                                                    .IndexOf(o.Key)
                                                            )
                                                }
                                        )
                                        .ToList();

                                var scenarioCase = new SpecFlowCase
                                {
                                    Arguments = argumentList
                                };
                                
                                _logger
                                    .LogInformation($"Extracted test case: {{{string.Join(", ", scenarioCase.Arguments.Select(o => $"\"{o.ArgumentName}\":\"{o.ArgumentValue}\""))}}}");

                                scenarioCases
                                    .Add(scenarioCase);
                            }
                        }
                        else
                        {
                            currInstr =
                                currInstr
                                    .StepPrevious(5);
                        }

                        while (currInstr?.OpCode == OpCodes.Ldstr)
                        {
                            if (currInstr.Operand != null)
                            {
                                tags.Add(
                                    currInstr
                                        .Operand.ToString()?
                                        .Replace(
                                            ",",
                                            ""
                                        )
                                );
                            }

                            currInstr =
                                currInstr
                                    .StepPrevious(4);
                        }
                    }
                }
            }

            var scenario = new SpecFlowScenario
            {
                Title = title,
                Description = description,
                Tags = tags
            };

            // Extract Steps
            var scenarioSteps = new List<SpecFlowExecutionStep>();

            foreach (var instruction in
                     method
                         .Body
                         .Instructions
                         .Where(o => o.OpCode == OpCodes.Callvirt))
            {
                if (instruction.Operand is MethodReference methodReference)
                {
                    if (Constants.ScenarioStepFunctions.Any(o => methodReference.Name == o))
                    {
                        var keyword = methodReference.Name;
                        var text = string.Empty;

                        var currInstr =
                            instruction
                                .StepPrevious(4);

                        if (currInstr.OpCode == OpCodes.Ldstr)
                        {
                            text =
                                currInstr
                                    .Operand
                                    .ToString();
                        }
                        else if (currInstr.OpCode == OpCodes.Call)
                        {
                            if (currInstr.Operand is MethodReference mr)
                            {
                                if (mr.Name == Constants.StringFormatFunctionName)
                                {
                                    var stringFormatArguments = new List<string>();
                                    string preFormatText = null;

                                    while (true)
                                    {
                                        string argKey = null;

                                        currInstr =
                                            currInstr
                                                .Previous;

                                        if (currInstr.OpCode == OpCodes.Ldarg_S)
                                        {
                                            argKey =
                                                currInstr
                                                    .Operand
                                                    .ToString();
                                        }
                                        else if (currInstr.OpCode == OpCodes.Ldarg_3)
                                            argKey = "3";
                                        else if (currInstr.OpCode == OpCodes.Ldarg_2)
                                            argKey = "2";
                                        else if (currInstr.OpCode == OpCodes.Ldarg_1)
                                            argKey = "1";
                                        else if (currInstr.OpCode == OpCodes.Ldarg_0)
                                            argKey = "0";
                                        else if (currInstr.OpCode == OpCodes.Ldstr)
                                            preFormatText =
                                                currInstr
                                                    .Operand
                                                    .ToString();

                                        if (string.IsNullOrEmpty(argKey))
                                            break;

                                        if (scenarioArgumentNames.TryGetValue(
                                                argKey,
                                                out var argumentName
                                            ))
                                        {
                                            stringFormatArguments
                                                .Add($"&lt;{argumentName}&gt;");
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(preFormatText) && stringFormatArguments.Any())
                                    {
                                        stringFormatArguments
                                            .Reverse();

                                        text =
                                            string
                                                .Format(
                                                    preFormatText,
                                                    stringFormatArguments
                                                        .ToArray()
                                                );
                                    }
                                }
                                else
                                {
                                    if (currInstr.OpCode == OpCodes.Ldstr)
                                    {
                                        text =
                                            currInstr
                                                .Operand
                                                .ToString();
                                    }
                                }
                            }
                        }

                        var executionStep = new SpecFlowExecutionStep
                        {
                            Keyword = keyword,
                            Text = text
                        };
                        
                        _logger
                            .LogInformation($"Extracted test step [{executionStep.Keyword} {executionStep.Text}]");

                        scenarioSteps
                            .Add(executionStep);
                    }
                }
            }

            scenario.Steps = scenarioSteps;
            scenario.Cases = scenarioCases;

            return scenario;
        }
    }
}
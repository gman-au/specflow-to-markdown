using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Providers
{
    public class MsTestScenarioExtractor : IScenarioExtractor
    {
        private readonly ILogger<FeatureExtractor> _logger;

        private readonly IEnumerable<string> _testCaseAttributes = new[]
        {
            Constants.MsTestTestPropertyAttribute
        };

        private const string ParameterDeclarationSplit = "Parameter\\:.*";

        public MsTestScenarioExtractor(ILogger<FeatureExtractor> logger)
        {
            _logger = logger;
        }

        public bool IsApplicable(MethodDefinition method)
        {
            return
                method.IsVirtual ||
                method
                    .CustomAttributes
                    .Any(
                        o =>
                            o
                                .AttributeType.FullName == Constants.MsTestTestAttribute
                    );
        }

        public SpecFlowScenario ExtractScenario(MethodDefinition method, TypeDefinition type)
        {
            // Extract Scenario
            var title = string.Empty;
            var description = string.Empty;
            var tags = new List<string>();
            var scenarioCases = new List<SpecFlowCase>();

            var scenarioArgumentNames = new Dictionary<string, string>();
            var scenarioArgumentValues = new List<IEnumerable<object>>();

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
                                .StepPrevious(4);

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

                        var testCaseFunctions =
                            type
                                .Methods
                                .Where(
                                    x =>
                                        x.Body
                                            .Instructions
                                            .Any(
                                                o =>
                                                {
                                                    if (o.OpCode == OpCodes.Callvirt && o.Operand is MethodReference methodReference)
                                                    {
                                                        var methodName = methodReference.Name;
                                                        return methodName == method.Name;
                                                    }

                                                    return false;
                                                }
                                            )
                                )
                                .ToList();

                        var testCases =
                            testCaseFunctions
                                .Count;

                        if (testCases > 0)
                        {
                            _logger
                                .LogInformation($"Scenario [{title}]; found {testCases} test cases");

                            var buildConfigurations = new List<Tuple<int, int>>
                            {
                                new(
                                    3,
                                    5
                                ), // debug
                                new(
                                    2,
                                    4
                                ) // release
                            };

                            var startingInstruction = currInstr;

                            foreach (var buildConfiguration in buildConfigurations)
                            {
                                currInstr = startingInstruction;

                                // Get test case argument names
                                currInstr =
                                    currInstr
                                        .StepPrevious(buildConfiguration.Item1);

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
                                                .StepPrevious(buildConfiguration.Item2);
                                    }
                                    else
                                        break;
                                }

                                if (scenarioArgumentNames.Count > 0)
                                    break;
                            }

                            if (scenarioArgumentNames.Count == 0)
                                throw new Exception(
                                    "Could not extract test cases from assembly; please try again with a different build configuration"
                                );

                            currInstr =
                                currInstr
                                    .StepPrevious(15);

                            // Get test case argument values
                            foreach (var testCaseFunction in testCaseFunctions)
                            {
                                var specFlowArguments = new List<SpecFlowArgument>();
                                var caseAttributes =
                                    testCaseFunction
                                        .CustomAttributes
                                        .Where(o => _testCaseAttributes.Contains(o.AttributeType.FullName))
                                        .ToList();

                                foreach (var caseAttribute in caseAttributes)
                                {
                                    var constructorArguments =
                                        caseAttribute
                                            .ConstructorArguments;

                                    if (constructorArguments.Count == 2)
                                    {
                                        var parameterNameArg = 
                                            constructorArguments[0]
                                                .Value
                                                .ToString();
                                        
                                        var fieldMatch =
                                            new Regex(ParameterDeclarationSplit)
                                                .Matches(parameterNameArg);

                                        if (fieldMatch.Count > 0)
                                        {
                                            var argName =
                                                fieldMatch[0]
                                                    .Value
                                                    .Split(":")
                                                    [1];

                                            var argValue = 
                                                constructorArguments[1]
                                                    .Value
                                                    .ToString();
                                            
                                            specFlowArguments
                                                .Add(new SpecFlowArgument
                                                    {
                                                        ArgumentName = argName,
                                                        ArgumentValue = argValue
                                                    }
                                                );
                                        }
                                    }
                                }

                                var scenarioCase = new SpecFlowCase
                                {
                                    Arguments = specFlowArguments
                                };
                                
                                _logger
                                    .LogInformation(
                                        $"Extracted test case: {{{string.Join(", ", scenarioCase.Arguments.Select(o => $"\"{o.ArgumentName}\":\"{o.ArgumentValue}\""))}}}"
                                    );

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
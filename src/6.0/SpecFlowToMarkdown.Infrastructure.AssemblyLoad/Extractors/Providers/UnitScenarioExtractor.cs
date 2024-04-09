using System;
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
        private static readonly string[] XOrNUnitTestAttributeValues =
        {
            Constants.NUnitTestAttribute,
            Constants.XUnitFactAttribute,
            Constants.XUnitTheoryAttribute
        };

        private readonly ILogger<FeatureExtractor> _logger;
        private readonly IStepExtractor _stepExtractor;
        private readonly IScenarioArgumentBuilder _scenarioArgumentBuilder;

        private readonly IEnumerable<string> _testCaseAttributes = new[]
        {
            Constants.NUnitTestCaseAttribute,
            Constants.XUnitInlineDataAttribute
        };

        public UnitScenarioExtractor(
            ILogger<FeatureExtractor> logger,
            IStepExtractor stepExtractor, 
            IScenarioArgumentBuilder scenarioArgumentBuilder
        )
        {
            _logger = logger;
            _stepExtractor = stepExtractor;
            _scenarioArgumentBuilder = scenarioArgumentBuilder;
        }

        public bool IsApplicable(MethodDefinition method)
        {
            return
                method
                    .CustomAttributes
                    .Any(
                        o =>
                            XOrNUnitTestAttributeValues
                                .Contains(
                                    o
                                        .AttributeType.FullName
                                )
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

                            scenarioArgumentNames = 
                                _scenarioArgumentBuilder
                                    .Build(currInstr);

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
            var scenarioSteps =
                _stepExtractor
                    .Perform(
                        method,
                        scenarioArgumentNames
                    );

            scenario.Steps = scenarioSteps;
            scenario.Cases = scenarioCases;

            return scenario;
        }
    }
}
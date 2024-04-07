using System.Collections.Generic;
using System.Linq;
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

        public SpecFlowScenario ExtractScenario(MethodDefinition method)
        {
            // Extract Scenario
            var title = string.Empty;
            var description = string.Empty;
            var tags = new List<string>();
            var scenarioCases = new List<SpecFlowCase>();

            var hasCases =
                method
                    .CustomAttributes
                    .Any(o => _testCaseAttributes.Contains(o.AttributeType.FullName));

            foreach (var instruction in
                     method
                         .Body
                         .Instructions
                         .Where(o => o.OpCode == OpCodes.Newobj))
            {
                if (instruction.Operand is MethodReference mr)
                {
                    if (mr.DeclaringType.FullName == Constants.FeatureInfoTypeName)
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
                        }

                        // Extract test cases
                        var argumentNames = new List<string>();
                        var argumentValues = new List<IEnumerable<object>>();

                        if (hasCases)
                        {
                            // Get test case argument names
                            currInstr =
                                currInstr
                                    .StepPrevious(4);

                            while (currInstr.OpCode == OpCodes.Ldstr)
                            {
                                if (currInstr.Operand != null)
                                {
                                    argumentNames.Add(currInstr.Operand.ToString());
                                }

                                currInstr =
                                    currInstr
                                        .StepPrevious(5);
                            }

                            argumentNames
                                .Reverse();

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
                                                    .Select(o =>
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

                                            argumentValues
                                                .Add(values);
                                        }
                                    }
                                }
                            }

                            for (var i = 0; i < argumentValues.Count; i++)
                            {
                                var scenarioCase = new SpecFlowCase
                                {
                                    Arguments =
                                        argumentNames
                                            .Select(
                                                o =>
                                                    new SpecFlowArgument
                                                    {
                                                        ArgumentName = o,
                                                        ArgumentValue =
                                                            argumentValues[i]
                                                                .ElementAt(argumentNames.IndexOf(o))
                                                    }
                                            )
                                            .ToList()
                                };

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

                        var executionStep = new SpecFlowExecutionStep
                        {
                            Keyword = keyword,
                            Text = text
                        };

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
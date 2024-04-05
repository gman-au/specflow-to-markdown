﻿using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Providers
{
    public class UnitScenarioExtractor : IScenarioExtractor
    {
        public IEnumerable<string> TestCaseAttributes = new[]
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

            var hasCases =
                method
                    .CustomAttributes
                    .Any(o => TestCaseAttributes.Contains(o.AttributeType.FullName));

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
                                .Previous
                                .Previous
                                .Previous
                                .Previous
                                .Previous;

                        if (currInstr.OpCode == OpCodes.Ldstr)
                        {
                            description = currInstr.Operand.ToString();
                        }

                        currInstr = currInstr.Previous;

                        if (currInstr.OpCode == OpCodes.Ldstr)
                        {
                            title = currInstr.Operand.ToString();
                        }

                        // Extract test cases
                        var argumentNames = new List<string>();
                        var argumentValues = new List<IEnumerable<string>>();

                        if (hasCases)
                        {
                            // Get test case argument values
                            var caseAttributes =
                                method
                                    .CustomAttributes
                                    .Where(o => TestCaseAttributes.Contains(o.AttributeType.FullName))
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
                                            foreach(var arg in args)
                                            {
                                                //TODO: load into library
                                            }
                                        }
                                    }
                                }
                            }

                            // Get test case argument names
                            currInstr =
                                currInstr
                                    .Previous
                                    .Previous
                                    .Previous
                                    .Previous;

                            while (currInstr.OpCode == OpCodes.Ldstr)
                            {
                                if (currInstr.Operand != null)
                                {
                                    argumentNames.Add(currInstr.Operand.ToString());
                                }

                                currInstr =
                                    currInstr
                                        .Previous
                                        .Previous
                                        .Previous
                                        .Previous
                                        .Previous;
                            }

                            argumentNames
                                .Reverse();

                            currInstr =
                                currInstr
                                    .StepPrevious(15);
                        }
                        else
                        {
                            currInstr =
                                currInstr
                                    .Previous
                                    .Previous
                                    .Previous
                                    .Previous
                                    .Previous;
                        }

                        while (currInstr.OpCode == OpCodes.Ldstr)
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
                                    .Previous
                                    .Previous
                                    .Previous
                                    .Previous;
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
                                .Previous
                                .Previous
                                .Previous
                                .Previous;

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

            return scenario;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Providers
{
    public class UnitScenarioExtractor : IScenarioExtractor
    {
        public bool IsApplicable(string attributeName) =>
            attributeName.Equals(Constants.NUnitTestAttribute) ||
            attributeName.Equals(Constants.XUnitTestAttribute);

        public SpecFlowScenario ExtractScenario(MethodDefinition method)
        {
            // Extract Scenario
            var title = string.Empty;
            var description = string.Empty;
            var tags = new List<string>();

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

                        currInstr =
                            currInstr
                                .Previous
                                .Previous
                                .Previous
                                .Previous
                                .Previous;

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
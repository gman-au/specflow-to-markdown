using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public class ScenarioExtractor : IScenarioExtractor
    {
        private const string FeatureInfoTypeName = "TechTalk.SpecFlow.ScenarioInfo";
        private static readonly string[] CustomTestAttributeValues = { "NUnit.Framework.TestAttribute" };
        private static readonly string[] ScenarioStepFunctions = { "And", "Given", "When", "Then" };

        public IEnumerable<SpecFlowScenario> ExtractScenarios(TypeDefinition type)
        {
            var results = new List<SpecFlowScenario>();

            foreach (var method in type.Methods)
            {
                if (method.CustomAttributes
                    .Any(
                        o =>
                            CustomTestAttributeValues
                                .Contains(
                                    o
                                        .AttributeType.FullName
                                )
                    ))
                {
                    // Extract Scenario
                    var title = string.Empty;
                    var description = string.Empty;

                    foreach (var instruction in
                             method
                                 .Body
                                 .Instructions
                                 .Where(o => o.OpCode == OpCodes.Newobj))
                    {
                        if (instruction.Operand is MethodReference mr)
                        {
                            if (mr.DeclaringType.FullName == FeatureInfoTypeName)
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
                            }
                        }
                    }

                    var scenario = new SpecFlowScenario
                    {
                        Title = title,
                        Description = description,
                        Tags = null
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
                            if (ScenarioStepFunctions.Any(o => methodReference.Name == o))
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

                    results
                        .Add(scenario);
                }
            }

            return results;
        }
    }
}
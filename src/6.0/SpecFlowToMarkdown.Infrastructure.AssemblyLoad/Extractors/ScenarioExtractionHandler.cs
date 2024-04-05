using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public class ScenarioExtractionHandler : IScenarioExtractionHandler
    {
        private static readonly string[] CustomTestAttributeValues =
        {
            Constants.NUnitTestAttribute,
            Constants.XUnitTestAttribute
        };
        private static readonly string[] ScenarioStepFunctions = { "And", "Given", "When", "Then" };

        private readonly IEnumerable<IScenarioExtractor> _extractors;

        public ScenarioExtractionHandler(IEnumerable<IScenarioExtractor> extractors)
        {
            _extractors = extractors;
        }

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
                    var attribute =
                        method
                            .CustomAttributes
                            .First(
                                o =>
                                    CustomTestAttributeValues
                                        .Contains(
                                            o
                                                .AttributeType.FullName
                                        )
                            );

                    var applicableExtractor =
                        _extractors
                            .FirstOrDefault(o => o.IsApplicable(attribute.AttributeType.FullName));

                    if (applicableExtractor == null)
                        throw new Exception($"No handler has been found for test attribute {attribute.AttributeType.FullName}");

                    var scenario =
                        applicableExtractor
                            .ExtractScenario(method);

                    results
                        .Add(scenario);
                }
            }

            return results;
        }
    }
}
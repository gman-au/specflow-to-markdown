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
            Constants.XUnitFactAttribute,
            Constants.XUnitTheoryAttribute,
            Constants.MsTestTestAttribute
        };

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
                var applicableExtractor =
                    _extractors
                        .FirstOrDefault(o => o.IsApplicable(method));
                // .FirstOrDefault(o => o.IsApplicable(attribute.AttributeType.FullName));

                if (applicableExtractor == null)
                    continue;

                var scenario =
                    applicableExtractor
                        .ExtractScenario(
                            method,
                            type
                        );

                results
                    .Add(scenario);
            }

            return results;
        }
    }
}
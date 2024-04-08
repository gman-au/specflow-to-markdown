using System.Collections.Generic;
using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public interface IScenarioExtractionHandler
    {
        public IEnumerable<SpecFlowScenario> ExtractScenarios(TypeDefinition type);
    }
}
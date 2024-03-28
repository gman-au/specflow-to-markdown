using System.Collections.Generic;
using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public interface IScenarioExtractor
    {
        public IEnumerable<SpecFlowScenario> ExtractScenarios(TypeDefinition type);
    }
}
using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Scenario
{
    public interface IScenarioExtractor
    {
        public bool IsApplicable(MethodDefinition method);

        public SpecFlowScenario Perform(MethodDefinition method, TypeDefinition type, ref string environment);
    }
}
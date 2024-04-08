using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public interface IScenarioExtractor
    {
        public bool IsApplicable(MethodDefinition method);

        public SpecFlowScenario ExtractScenario(MethodDefinition method, TypeDefinition type);
    }
}
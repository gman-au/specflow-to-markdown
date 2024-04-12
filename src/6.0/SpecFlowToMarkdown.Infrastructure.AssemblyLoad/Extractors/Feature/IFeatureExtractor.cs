using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Feature
{
    public interface IFeatureExtractor
    {
        public SpecFlowAssembly Perform(AssemblyDefinition assembly);
    }
}
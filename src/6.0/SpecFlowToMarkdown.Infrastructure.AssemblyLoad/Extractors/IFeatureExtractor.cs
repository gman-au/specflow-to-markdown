using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public interface IFeatureExtractor
    {
        public SpecFlowAssembly ExtractFeatures(AssemblyDefinition assembly);
    }
}
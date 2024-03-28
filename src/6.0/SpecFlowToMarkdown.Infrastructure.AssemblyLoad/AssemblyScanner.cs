using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad
{
    public class AssemblyScanner : IAssemblyScanner
    {
        private readonly IFeatureExtractor _featureExtractor;

        public AssemblyScanner(IFeatureExtractor featureExtractor)
        {
            _featureExtractor = featureExtractor;
        }

        public SpecFlowAssembly Perform(string assemblyPath)
        {
            var assembly =
                AssemblyDefinition
                    .ReadAssembly(assemblyPath);

            var result =
                _featureExtractor
                    .ExtractFeatures(assembly);

            return result;
        }
    }
}
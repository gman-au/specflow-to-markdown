using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad
{
    public static class AssemblyScanner
    {
        public static SpecFlowAssembly Perform(string assemblyPath)
        {
            var assembly =
                AssemblyDefinition
                    .ReadAssembly(assemblyPath);

            var result =
                assembly
                    .ExtractFeatures();

            return result;
        }
    }
}
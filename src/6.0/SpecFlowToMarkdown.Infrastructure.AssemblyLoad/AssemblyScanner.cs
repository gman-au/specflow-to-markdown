using Mono.Cecil;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Feature;
using SpecFlowToMarkdown.Infrastructure.Io;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad
{
    public class AssemblyScanner : IAssemblyScanner
    {
        private readonly IFeatureExtractor _featureExtractor;
        private readonly IFileFinder _fileFinder;

        public AssemblyScanner(
            IFeatureExtractor featureExtractor,
            IFileFinder fileFinder
        )
        {
            _featureExtractor = featureExtractor;
            _fileFinder = fileFinder;
        }

        public SpecFlowAssembly Perform(ProgramArguments arguments)
        {
            var foundFilePath =
                _fileFinder
                    .GetFirstFound(
                        arguments.TestAssemblyFolder,
                        arguments.TestAssemblyFile
                    );

            var assembly =
                AssemblyDefinition
                    .ReadAssembly(foundFilePath);

            var result =
                _featureExtractor
                    .Perform(assembly);

            return result;
        }
    }
}
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad
{
    public interface IAssemblyScanner
    {
        public SpecFlowAssembly Perform(ProgramArguments arguments);
    }
}
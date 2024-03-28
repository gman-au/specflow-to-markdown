using System.Text;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public interface IMarkdownRenderer
    {
        public StringBuilder Perform(
            SpecFlowAssembly assembly,
            TestExecution execution
        );
    }
}
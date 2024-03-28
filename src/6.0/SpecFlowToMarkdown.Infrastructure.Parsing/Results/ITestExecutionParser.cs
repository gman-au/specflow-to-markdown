using SpecFlowToMarkdown.Domain.Result;

namespace SpecFlowToMarkdown.Infrastructure.Parsing.Results
{
    public interface ITestExecutionParser
    {
        public TestExecution Parse(string executionResultsPath);
    }
}
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public interface IResultSummariser
    {
        public TestSummary SummariseAllFeatures(TestExecution execution);

        public TestSummary SummariseAllScenarios(TestExecution execution);

        public TestSummary SummariseAllSteps(TestExecution execution);

        public TestStatusEnum Assess(int successes, int failures, int others);

        public TestStatusEnum Assess(string value);
    }
}
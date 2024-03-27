using System.Linq;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    internal static class Summariser
    {
        private const string StatusOk = "OK";
        private const string StatusError = "TestError";

        public static TestSummary SummariseAllFeatures(TestExecution execution)
        {
            var executionResults =
                execution
                    .ExecutionResults
                    .ToList();

            var featureAggregate =
                executionResults
                    .GroupBy(
                        o => o.FeatureTitle,
                        x => x.Status,
                        (a, b) => new
                        {
                            Feature = a,
                            Successes = b.Count(o => o == StatusOk),
                            Failures = b.Count(o => o == StatusError),
                            Others = b.Count(o => o != StatusOk && o != StatusError)
                        }
                    )
                    .ToList();

            return new TestSummary
            {
                Successes =
                    featureAggregate
                        .Count(
                            o => Assess(
                                o.Successes,
                                o.Failures,
                                o.Others
                            ) == TestStatusEnum.Success
                        ),
                Failures =
                    featureAggregate
                        .Count(
                            o => Assess(
                                o.Successes,
                                o.Failures,
                                o.Others
                            ) == TestStatusEnum.Failure
                        ),
                Others =
                    featureAggregate
                        .Count(
                            o => Assess(
                                o.Successes,
                                o.Failures,
                                o.Others
                            ) == TestStatusEnum.Other
                        )
            };
        }

        public static TestSummary SummariseAllScenarios(TestExecution execution)
        {
            var executionResults =
                execution
                    .ExecutionResults
                    .ToList();

            return new TestSummary
            {
                Successes =
                    executionResults
                        .Count(o => o.Status == StatusOk),
                Failures =
                    executionResults
                        .Count(o => o.Status == StatusError),
                Others =
                    executionResults
                        .Count(o => o.Status != StatusOk && o.Status != StatusError)
            };
        }

        public static TestSummary SummariseAllSteps(TestExecution execution)
        {
            var stepResults =
                execution
                    .ExecutionResults
                    .SelectMany(o => o.StepResults)
                    .ToList();

            return new TestSummary
            {
                Successes =
                    stepResults
                        .Count(o => o.Status == StatusOk),
                Failures =
                    stepResults
                        .Count(o => o.Status == StatusError),
                Others =
                    stepResults
                        .Count(o => o.Status != StatusOk && o.Status != StatusError)
            };
        }

        public static TestStatusEnum Assess(int successes, int failures, int others)
        {
            if (failures > 0) return TestStatusEnum.Failure;
            if (others > 0) return TestStatusEnum.Other;
            if (successes > 0) return TestStatusEnum.Success;
            return TestStatusEnum.Other;
        }

        public static TestStatusEnum Assess(string value)
        {
            switch (value)
            {
                case StatusOk: return TestStatusEnum.Success;
                case StatusError: return TestStatusEnum.Failure;
            }

            return TestStatusEnum.Other;
        }
    }
}
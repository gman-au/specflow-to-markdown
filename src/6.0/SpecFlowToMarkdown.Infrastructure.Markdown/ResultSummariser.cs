using System.Collections.Generic;
using System.Linq;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;
using SpecFlowToMarkdown.Infrastructure.Markdown.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public class ResultSummariser : IResultSummariser
    {
        private const string StatusOk = "OK";
        private const string StatusError = "TestError";

        public TestSummary SummariseAllFeatures(TestExecution execution)
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

            var totalDuration =
                executionResults
                    .SelectMany(o => o.StepResults)
                    .Sum(x => x.Duration.GetValueOrDefault().TotalSeconds);

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
                        ),
                Duration = totalDuration
            };
        }

        public TestSummary SummariseAllScenarios(TestExecution execution)
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

        public TestSummary SummariseAllSteps(TestExecution execution)
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

        public IDictionary<string, TestSummary> SummariseAllTags(TestExecution execution, SpecFlowAssembly assembly)
        {
            var results = new Dictionary<string, TestSummary>();

            var allTags =
                assembly
                    .Features
                    .SelectMany(o => o.Scenarios.SelectMany(x => x.Tags))
                    .Distinct();

            foreach (var tag in allTags)
            {
                var result = new TestSummary
                {
                    Successes = 0,
                    Failures = 0,
                    Others = 0,
                    Duration = 0
                };

                foreach (var feature in assembly.Features)
                {
                    var allTaggedScenarios =
                        feature
                            .Scenarios
                            .Where(x => x.Tags.Contains(tag));

                    foreach (var taggedScenario in allTaggedScenarios)
                    {
                        var executionResult =
                            execution
                                .ExecutionResults
                                .FirstOrDefault(
                                    o => o.FeatureTitle == feature.Title &&
                                        o.ScenarioTitle == taggedScenario.Title
                                );

                        if (executionResult != null)
                        {
                            switch (executionResult.Status.ToStatusEnum())
                            {
                                case TestStatusEnum.Success:
                                    result.Successes++;
                                    break;
                                case TestStatusEnum.Failure:
                                    result.Failures++;
                                    break;
                                default:
                                    result.Others++;
                                    break;
                            }
                        }
                    }
                }

                results
                    .Add(
                        tag,
                        result
                    );
            }

            return
                results
                    .OrderBy(o => o.Key)
                    .ToDictionary(
                        o => o.Key,
                        o => o.Value
                    );
        }

        public TestStatusEnum Assess(int successes, int failures, int others)
        {
            if (failures > 0) return TestStatusEnum.Failure;
            if (others > 0) return TestStatusEnum.Other;
            if (successes > 0) return TestStatusEnum.Success;
            return TestStatusEnum.Other;
        }
    }
}
using System;
using System.Collections.Generic;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Infrastructure.Markdown;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;
using Xunit;

namespace SpecFlowToMarkdown.Tests.Unit
{
    public class ResultSummariserTests
    {
        private readonly TestContext _context = new();

        [Fact]
        public void Test_Summarise_Scenario_One_By_Feature()
        {
            _context.ArrangeScenarioOne();
            _context.ActSummariseResultByFeature();
            _context.AssertScenarioOneSummarisedByFeature();
        }

        [Fact]
        public void Test_Summarise_Scenario_One_By_Scenario()
        {
            _context.ArrangeScenarioOne();
            _context.ActSummariseResultByScenario();
            _context.AssertScenarioOneSummarisedByScenario();
        }

        [Fact]
        public void Test_Summarise_Scenario_One_By_Steps()
        {
            _context.ArrangeScenarioOne();
            _context.ActSummariseResultBySteps();
            _context.AssertScenarioOneSummarisedBySteps();
        }
        
        private class TestContext
        {
            private TestExecution _value;
            private TestSummary _result;

            public void ArrangeScenarioOne()
            {
                _value = SampleExecutionResults.TestRunOne;
            }

            public void ActSummariseResultByFeature() =>
                _result =
                    ResultSummariser
                        .SummariseAllFeatures(_value);

            public void ActSummariseResultByScenario() =>
                _result =
                    ResultSummariser
                        .SummariseAllScenarios(_value);

            public void ActSummariseResultBySteps() =>
                _result =
                    ResultSummariser
                        .SummariseAllSteps(_value);

            public void AssertScenarioOneSummarisedByFeature()
            {
                Assert.Equal(1, _result.Successes);
                Assert.Equal(1, _result.Failures);
                Assert.Equal(0, _result.Others);
            }

            public void AssertScenarioOneSummarisedByScenario()
            {
                Assert.Equal(3, _result.Successes);
                Assert.Equal(1, _result.Failures);
                Assert.Equal(1, _result.Others);
            }

            public void AssertScenarioOneSummarisedBySteps()
            {
                Assert.Equal(6, _result.Successes);
                Assert.Equal(1, _result.Failures);
                Assert.Equal(2, _result.Others);
            }
        }
    }
}
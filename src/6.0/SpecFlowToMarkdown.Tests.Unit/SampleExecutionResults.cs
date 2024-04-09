using System;
using System.Collections.Generic;
using SpecFlowToMarkdown.Domain.Result;

namespace SpecFlowToMarkdown.Tests.Unit
{
    public static class SampleExecutionResults
    {
        private static readonly TimeSpan OneSecond = new(
            0,
            0,
            0,
            0,
            1000
        );
        
        public static readonly TestExecution TestRunOne = new()
        {
            ExecutionResults = new List<ExecutionResult>
            {
                new()
                {
                    FeatureTitle = "Pets",
                    ScenarioTitle = "Pet my cat",
                    ScenarioArguments = new List<string>
                    {
                        "my_argument_1",
                        "my_argument_2",
                        "my_argument_3"
                    },
                    Status = "OK",
                    StepResults = new List<StepResult>
                    {
                        new()
                        {
                            Duration =OneSecond,
                            Status = "OK"
                        }
                    }
                },
                new()
                {
                    FeatureTitle = "Pets",
                    ScenarioTitle = "Wash my cat",
                    Status = "OK",
                    StepResults = new List<StepResult>
                    {
                        new()
                        {
                            Duration = OneSecond,
                            Status = "OK"
                        },
                        new()
                        {
                            Duration = OneSecond,
                            Status = "OK"
                        }
                    }
                },
                new()
                {
                    FeatureTitle = "Cars",
                    ScenarioTitle = "Wash my car",
                    Status = "OK",
                    StepResults = new List<StepResult>
                    {
                        new()
                        {
                            Duration = OneSecond,
                            Status = "OK"
                        },
                        new()
                        {
                            Duration = OneSecond,
                            Status = "OK"
                        }
                    }
                },
                new()
                {
                    FeatureTitle = "Cars",
                    ScenarioTitle = "Pet my car",
                    Status = "TestError",
                    StepResults = new List<StepResult>
                    {
                        new()
                        {
                            Duration = OneSecond,
                            Status = "OK"
                        },
                        new()
                        {
                            Duration = OneSecond,
                            Status = "TestError"
                        }
                    }
                },
                new()
                {
                    FeatureTitle = "Cars",
                    ScenarioTitle = "Repair my car",
                    Status = "Skipped",
                    StepResults = new List<StepResult>
                    {
                        new()
                        {
                            Duration = OneSecond,
                            Status = "Skipped"
                        },
                        new()
                        {
                            Duration = OneSecond,
                            Status = "Skipped"
                        }
                    }
                }
            }
        };
    }
}
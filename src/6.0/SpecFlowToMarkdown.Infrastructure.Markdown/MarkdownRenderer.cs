﻿using System;
using System.Linq;
using System.Text;
using System.Web;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.Markdown.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public class MarkdownRenderer : IMarkdownRenderer
    {
        private readonly IResultSummariser _resultSummariser;

        public MarkdownRenderer(IResultSummariser resultSummariser)
        {
            _resultSummariser = resultSummariser;
        }

        public StringBuilder Perform(
            SpecFlowAssembly assembly,
            TestExecution execution
        )
        {
            var result = new StringBuilder();

            var featureSummary =
                _resultSummariser
                    .SummariseAllFeatures(execution);

            var scenarioSummary =
                _resultSummariser
                    .SummariseAllScenarios(execution);

            var stepSummary =
                _resultSummariser
                    .SummariseAllSteps(execution);

            // Render header
            result
                .AppendLine($"# {assembly.AssemblyName}");

            result
                .AppendLine("<table>")
                .AppendLine("<tr>")
                .AppendLine("<td>")
                .AppendChart(
                    "Features",
                    featureSummary.Successes,
                    featureSummary.Failures,
                    featureSummary.Others
                )
                .AppendLine("</td>")
                .AppendLine("<td>")
                .AppendChart(
                    "Scenarios",
                    scenarioSummary.Successes,
                    scenarioSummary.Failures,
                    scenarioSummary.Others
                )
                .AppendLine("</td>")
                .AppendLine("<td>")
                .AppendChart(
                    "Steps",
                    stepSummary.Successes,
                    stepSummary.Failures,
                    stepSummary.Others
                )
                .AppendLine("</td>")
                .AppendLine("</tr>")
                .AppendLine("<table>")
                .AppendLine();

            // Features
            foreach (var feature in assembly.Features)
            {
                var featureScenarios =
                    execution
                        .ExecutionResults
                        .Where(o => o.FeatureTitle == feature.Title)
                        .ToList();

                var featureSuccesses =
                    featureScenarios
                        .Count(o => _resultSummariser.Assess(o.Status) == TestStatusEnum.Success);

                var featureFails =
                    featureScenarios
                        .Count(o => _resultSummariser.Assess(o.Status) == TestStatusEnum.Failure);

                var featureOthers =
                    featureScenarios
                        .Count(o => _resultSummariser.Assess(o.Status) == TestStatusEnum.Other);

                var status =
                    _resultSummariser
                        .Assess(
                            featureSuccesses,
                            featureFails,
                            featureOthers
                        );

                result
                    .AppendLine($"## {StatusIcon(status)} <i>Feature:</i>\t{feature.Title}");

                // Scenarios
                foreach (var scenario in feature.Scenarios)
                {
                    var scenarioResult =
                        featureScenarios
                            .FirstOrDefault(o => o.ScenarioTitle == scenario.Title);

                    if (scenarioResult != null)
                    {
                        var scenarioStatus =
                            _resultSummariser
                                .Assess(scenarioResult.Status);

                        result
                            .AppendLine($"### {StatusIcon(scenarioStatus)} <i>Scenario:</i>\t{scenario.Title}");

                        if (!string.IsNullOrEmpty(scenario.Description))
                        {
                            result
                                .AppendLine($"`{scenario.Description.Trim()}`");
                        }

                        result
                            .AppendLine("<table>");

                        // Steps
                        for (var i = 0; i < scenario.Steps.Count(); i++)
                        {
                            result
                                .AppendLine("<tr>");

                            var stepDetails =
                                scenario
                                    .Steps
                                    .ElementAt(i);

                            var stepResult =
                                scenarioResult
                                    .StepResults
                                    .ElementAt(i);

                            result
                                .AppendLine(
                                    $"<td>{StatusCircle(stepResult.Status)}</td>" +
                                    $"<td>{FullResultText(stepDetails)}</td>" +
                                    $"<td>{Math.Round(stepResult.Duration.GetValueOrDefault().TotalSeconds, 2)}s</td>"
                                );

                            if (!string.IsNullOrEmpty(stepResult.Error))
                            {
                                result
                                    .AppendLine()
                                    .AppendLine("<td>")
                                    .AppendLine("<details>")
                                    .AppendLine("<summary>Error Details</summary>")
                                    .AppendLine("<code>")
                                    .AppendLine(HttpUtility.HtmlEncode(stepResult.Error.ReplaceLineEndings().Trim()))
                                    .AppendLine("</code>")
                                    .AppendLine("</details>")
                                    .AppendLine("</td>");
                            }
                            else
                            {
                                result
                                    .AppendLine("<td/>");
                            }

                            result
                                .AppendLine("</tr>");
                        }

                        result
                            .AppendLine("</table>")
                            .AppendLine();
                    }
                }
            }

            return result;
        }

        private static string FullResultText(SpecFlowExecutionStep stepDetails)
        {
            var result = $"<strong>{stepDetails.Keyword}</strong> {stepDetails.Text}";

            return result;
        }

        private string StatusCircle(string result) =>
            StatusCircle(_resultSummariser.Assess(result));

        private static string StatusCircle(TestStatusEnum result)
        {
            return result switch
            {
                TestStatusEnum.Success => ":green_circle:",
                TestStatusEnum.Failure => ":red_circle:",
                _ => ":black_circle:"
            };
        }

        private static string StatusIcon(TestStatusEnum result)
        {
            return result switch
            {
                TestStatusEnum.Success => ":heavy_check_mark:",
                TestStatusEnum.Failure => ":x:",
                _ => ":heavy_minus_sign:"
            };
        }
    }
}
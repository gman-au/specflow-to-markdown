using System;
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
        private readonly IAnchorGenerator _anchorGenerator;
        private readonly IColourSorter _colourSorter;
        private readonly IResultSummariser _resultSummariser;

        public MarkdownRenderer(
            IResultSummariser resultSummariser,
            IColourSorter colourSorter,
            IAnchorGenerator anchorGenerator
        )
        {
            _resultSummariser = resultSummariser;
            _colourSorter = colourSorter;
            _anchorGenerator = anchorGenerator;
        }

        public StringBuilder Perform(
            SpecFlowAssembly assembly,
            TestExecution execution
        )
        {
            var result = new StringBuilder();
            var headerBuilder = new StringBuilder();

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
            headerBuilder
                .AppendLine($"# {assembly.AssemblyName}");

            headerBuilder
                .AppendLine("<table>")
                .AppendLine("<tr>")
                .AppendLine("<td>")
                .AppendChart(
                    "Features",
                    _colourSorter
                        .Sort(
                            featureSummary.Successes,
                            featureSummary.Failures,
                            featureSummary.Others
                        )
                )
                .AppendLine("</td>")
                .AppendLine("<td>")
                .AppendChart(
                    "Scenarios",
                    _colourSorter
                        .Sort(
                            scenarioSummary.Successes,
                            scenarioSummary.Failures,
                            scenarioSummary.Others
                        )
                )
                .AppendLine("</td>")
                .AppendLine("<td>")
                .AppendChart(
                    "Steps",
                    _colourSorter
                        .Sort(
                            stepSummary.Successes,
                            stepSummary.Failures,
                            stepSummary.Others
                        )
                )
                .AppendLine("</td>")
                .AppendLine("</tr>")
                .AppendLine("</table>")
                .AppendLine();

            // TOC
            var tocBuilder = new StringBuilder();
            tocBuilder
                .AppendLine()
                .AppendLine("<table>")
                .AppendLine("<tr>");

            foreach (var header in new[] { "Feature", "Scenario", "Passed", "Failed", "Skipped", "Time" })
            {
                tocBuilder
                    .AppendLine($"<th>{header}</th>");
            }

            tocBuilder
                .AppendLine("<tr>");

            // Features
            var featureSectionBuilder = new StringBuilder();
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

                var featureDuration =
                    featureScenarios
                        .SelectMany(o => o.StepResults)
                        .Sum(x => x.Duration.GetValueOrDefault().TotalSeconds);

                var status =
                    _resultSummariser
                        .Assess(
                            featureSuccesses,
                            featureFails,
                            featureOthers
                        );

                var featureStatusIcon = StatusIcon(status);

                var featureAnchor =
                    _anchorGenerator
                        .Build(
                            $"Feature:{feature.Title}",
                            featureStatusIcon
                        );

                tocBuilder
                    .AppendLine($"<td><a href=\"#{featureAnchor}\">Feature:{feature.Title}</a></td>")
                    .AppendLine($"<td/>")
                    .AppendLine($"<td>{featureSuccesses} {(featureSuccesses > 0 ? $":{IconReference.IconSuitePassed}:" : null)}</td>")
                    .AppendLine($"<td>{featureFails} {(featureFails > 0 ? $":{IconReference.IconSuiteFailed}:" : null)}</td>")
                    .AppendLine($"<td>{featureOthers} {(featureOthers > 0 ? $":{IconReference.IconSuiteSkipped}:" : null)}</td>")
                    .AppendLine($"<td>{Math.Round(featureDuration, 2)}s</td>")
                    .AppendLine();

                featureSectionBuilder
                    .AppendLine($"<h2> :{StatusIcon(status)}: <a id=\"{featureAnchor}\"><i>Feature:</i>\t{feature.Title}</a></h2>");

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

                        var scenarioStatusIcon = StatusIcon(scenarioStatus);

                        var scenarioAnchor =
                            _anchorGenerator
                                .Build(
                                    $"Scenario:{scenario.Title}",
                                    scenarioStatusIcon
                                );

                        var scenarioDuration =
                            scenarioResult
                                .StepResults
                                .Sum(o => o.Duration.GetValueOrDefault().TotalSeconds);

                        tocBuilder
                            .AppendLine("<tr>")
                            .AppendLine($"<td/>")
                            .AppendLine($"<td><a href=\"#{scenarioAnchor}\">Scenario:{scenario.Title}</a></td>")
                            .AppendLine(
                                $"<td>{(scenarioStatus == TestStatusEnum.Success ? $":{IconReference.IconStepPassed}:" : null)}</td>"
                            )
                            .AppendLine(
                                $"<td>{(scenarioStatus == TestStatusEnum.Failure ? $":{IconReference.IconStepFailed}:" : null)}</td>"
                            )
                            .AppendLine(
                                $"<td>{(scenarioStatus == TestStatusEnum.Other ? $":{IconReference.IconStepSkipped}:" : null)}</td>"
                            )
                            .AppendLine($"<td>{Math.Round(scenarioDuration, 2)}s</td>")
                            .AppendLine("</tr>");

                        featureSectionBuilder
                            .AppendLine(
                                $"<h3> :{StatusIcon(scenarioStatus)}: <a id=\"{scenarioAnchor}\"><i>Scenario:</i>\t{scenario.Title}</a></h3>"
                            );

                        if (!string.IsNullOrEmpty(scenario.Description))
                        {
                            featureSectionBuilder
                                .AppendLine($"`{scenario.Description.Trim()}`");
                        }

                        featureSectionBuilder
                            .AppendLine("<table>");

                        // Steps
                        for (var i = 0; i < scenario.Steps.Count(); i++)
                        {
                            featureSectionBuilder
                                .AppendLine("<tr>");

                            var stepDetails =
                                scenario
                                    .Steps
                                    .ElementAt(i);

                            var stepResult =
                                scenarioResult
                                    .StepResults
                                    .ElementAt(i);

                            featureSectionBuilder
                                .AppendLine(
                                    $"<td>:{StatusCircle(stepResult.Status)}:</td>" +
                                    $"<td>{FullResultText(stepDetails)}</td>" +
                                    $"<td>{Math.Round(stepResult.Duration.GetValueOrDefault().TotalSeconds, 2)}s</td>"
                                );

                            if (!string.IsNullOrEmpty(stepResult.Error))
                            {
                                featureSectionBuilder
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
                                featureSectionBuilder
                                    .AppendLine("<td/>");
                            }

                            featureSectionBuilder
                                .AppendLine("</tr>");
                        }

                        featureSectionBuilder
                            .AppendLine("</table>")
                            .AppendLine();
                    }
                }
            }

            tocBuilder
                .AppendLine("</table>")
                .AppendLine();

            result
                .Append(headerBuilder)
                .Append(tocBuilder)
                .Append(featureSectionBuilder);

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
                TestStatusEnum.Success => IconReference.IconStepPassed,
                TestStatusEnum.Failure => IconReference.IconStepFailed,
                _ => IconReference.IconStepSkipped
            };
        }

        private static string StatusIcon(TestStatusEnum result)
        {
            return result switch
            {
                TestStatusEnum.Success => IconReference.IconSuitePassed,
                TestStatusEnum.Failure => IconReference.IconSuiteFailed,
                _ => IconReference.IconSuiteSkipped
            };
        }
    }
}
using System;
using System.Linq;
using System.Text;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.Markdown.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public class MarkdownRenderer : IMarkdownRenderer
    {
        private readonly IColourSorter _colourSorter;
        private readonly IResultSummariser _resultSummariser;

        public MarkdownRenderer(
            IResultSummariser resultSummariser,
            IColourSorter colourSorter
        )
        {
            _resultSummariser = resultSummariser;
            _colourSorter = colourSorter;
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

            var tagSummary =
                _resultSummariser
                    .SummariseAllTags(
                        execution,
                        assembly
                    );

            // Render header
            headerBuilder
                .AppendLine($"# {assembly.AssemblyName}");

            headerBuilder
                .AppendLine("<table>")
                .AppendLine("<tr>")
                .AppendLine("<td>")
                .AppendPieChart(
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
                .AppendPieChart(
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
                .AppendPieChart(
                    "Steps",
                    _colourSorter
                        .Sort(
                            stepSummary.Successes,
                            stepSummary.Failures,
                            stepSummary.Others
                        )
                )
                .AppendLine("</td>")
                .AppendLine("<td>")
                .AppendTagChart(
                    "Tags",
                    tagSummary
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

            foreach (var header in new[] { "Feature", "Scenario", "Case", "Passed", "Failed", "Skipped", "Time" })
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
                        .Count(o => o.Status.ToStatusEnum() == TestStatusEnum.Success);

                var featureFails =
                    featureScenarios
                        .Count(o => o.Status.ToStatusEnum() == TestStatusEnum.Failure);

                var featureOthers =
                    featureScenarios
                        .Count(o => o.Status.ToStatusEnum() == TestStatusEnum.Other);

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
                    $"Feature:{feature.Title}"
                        .ToAnchor(featureStatusIcon);

                tocBuilder
                    .AppendLine($"<td><a href=\"#{featureAnchor}\">Feature:\t{feature.Title}</a></td>")
                    .AppendLine($"<td/>")
                    .AppendLine($"<td/>")
                    .AppendLine($"<td>{featureSuccesses} {(featureSuccesses > 0 ? $":{IconReference.IconSuitePassed}:" : null)}</td>")
                    .AppendLine($"<td>{featureFails} {(featureFails > 0 ? $":{IconReference.IconSuiteFailed}:" : null)}</td>")
                    .AppendLine($"<td>{featureOthers} {(featureOthers > 0 ? $":{IconReference.IconSuiteSkipped}:" : null)}</td>")
                    .AppendLine($"<td>{Math.Round(featureDuration, 2)}s</td>")
                    .AppendLine();

                featureSectionBuilder
                    .AppendLine($"<h2> :{StatusIcon(status)}: <a id=\"{featureAnchor}\"><i>Feature:</i>\t{feature.Title}</a></h2>");

                if (!string.IsNullOrEmpty(feature.Description))
                {
                    featureSectionBuilder
                        .AppendLine()
                        .Append(feature.Description)
                        .AppendLine();
                }

                // Scenarios
                foreach (var scenario in feature.Scenarios)
                {
                    var scenarioResult =
                        featureScenarios
                            .FirstOrDefault(o => o.ScenarioTitle == scenario.Title);

                    if (scenarioResult != null)
                    {
                        var scenarioStatus =
                            scenarioResult
                                .Status
                                .ToStatusEnum();

                        var scenarioStatusIcon = StatusIcon(scenarioStatus);

                        var scenarioAnchor =
                            $"Scenario:{scenario.Title}"
                                .ToAnchor(scenarioStatusIcon);

                        var scenarioDuration =
                            scenarioResult
                                .StepResults
                                .Sum(o => o.Duration.GetValueOrDefault().TotalSeconds);

                        tocBuilder
                            .AppendLine("<tr>")
                            .AppendLine($"<td/>")
                            .AppendLine($"<td><a href=\"#{scenarioAnchor}\">Scenario:\t{scenario.Title}</a></td>");

                        featureSectionBuilder
                            .AppendLine(
                                $"<h3> :{StatusIcon(scenarioStatus)}: <a id=\"{scenarioAnchor}\"><i>Scenario:</i>\t{scenario.Title}</a></h3>"
                            );

                        // Test cases
                        if (scenario.Cases.Any())
                        {
                            // Get all matching results with case arguments
                            var caseFeatureResults =
                                featureScenarios
                                    .Where(
                                        o =>
                                            o.ScenarioTitle == scenario.Title &&
                                            o.FeatureTitle == feature.Title
                                    )
                                    .ToList();

                            // Complete the header row
                            tocBuilder
                                .AppendLine($"<td/>")
                                .AppendLine($"<td/>")
                                .AppendLine($"<td/>")
                                .AppendLine($"<td/>")
                                .AppendLine($"<td/>");

                            foreach (var scenarioCase in scenario.Cases)
                            {
                                tocBuilder
                                    .AppendLine("<tr>")
                                    .AppendLine($"<td/>")
                                    .AppendLine($"<td/>")
                                    .AppendLine($"<td>")
                                    .AppendLine();

                                foreach (var argument in scenarioCase.Arguments)
                                {
                                    tocBuilder
                                        .Append($"`{argument.ArgumentName}: {argument.ArgumentValue}`")
                                        .AppendLine();
                                }

                                tocBuilder
                                    .AppendLine($"</td>");

                                var caseFeatureResult =
                                    caseFeatureResults
                                        .FirstOrDefault(
                                            x =>
                                            {
                                                var isMatch = true;
                                                for (var i = 0; i < scenarioCase.Arguments.Count(); i++)
                                                {
                                                    var sourceArg =
                                                        scenarioCase
                                                            .Arguments
                                                            .ElementAt(i)?
                                                            .ArgumentValue?
                                                            .ToString();

                                                    var targetArg =
                                                        x
                                                            .ScenarioArguments
                                                            .ElementAt(i);

                                                    isMatch &=
                                                        sourceArg == targetArg;
                                                }

                                                return isMatch;
                                            }
                                        );

                                if (caseFeatureResult != null)
                                {
                                    var caseStatus =
                                        caseFeatureResult
                                            .Status
                                            .ToStatusEnum();

                                    var caseStatusIcon = StatusIcon(scenarioStatus);

                                    tocBuilder
                                        .AppendLine(
                                            $"<td>{(caseStatus == TestStatusEnum.Success ? $":{IconReference.IconStepPassed}:" : null)}</td>"
                                        )
                                        .AppendLine(
                                            $"<td>{(caseStatus == TestStatusEnum.Failure ? $":{IconReference.IconStepFailed}:" : null)}</td>"
                                        )
                                        .AppendLine(
                                            $"<td>{(caseStatus == TestStatusEnum.Other ? $":{IconReference.IconStepSkipped}:" : null)}</td>"
                                        )
                                        .AppendLine($"<td>{Math.Round(scenarioDuration, 2)}s</td>")
                                        .AppendLine("</tr>");
                                }
                            }
                        }
                        else
                        {
                            tocBuilder
                                .AppendLine($"<td/>")
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
                                .AppendTags(scenario);

                            if (!string.IsNullOrEmpty(scenario.Description))
                            {
                                featureSectionBuilder
                                    .AppendLine()
                                    .AppendLine()
                                    .AppendLine($"`{scenario.Description.Trim()}`");
                            }

                            featureSectionBuilder
                                .AppendStepsTable(
                                    scenario,
                                    scenarioResult
                                );

                            featureSectionBuilder
                                .AppendLine();
                        }
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
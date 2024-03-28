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
                .AppendLine("| Feature | Scenario | Passed | Failed | Skipped | Time |")
                .AppendLine("| --- | --- | --- | --- | --- | --- |");

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
                            featureStatusIcon,
                            feature.Title
                        );

                tocBuilder
                    .Append($"| {featureAnchor} ")
                    .Append($"| ")
                    .Append($"| {featureSuccesses} {(featureSuccesses > 0 ? $":{IconReference.IconSuitePassed}:" : null)} ")
                    .Append($"| {featureFails} {(featureFails > 0 ? $":{IconReference.IconSuiteFailed}:" : null)} ")
                    .Append($"| {featureOthers} {(featureOthers > 0 ? $":{IconReference.IconSuiteSkipped}:" : null)} ")
                    .Append("| ")
                    .AppendLine();

                featureSectionBuilder
                    .AppendLine($"## :{StatusIcon(status)}: <i>Feature:</i>\t{feature.Title}");

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
                                    scenarioStatusIcon,
                                    scenario.Title
                                );
                        
                        tocBuilder
                            .Append($"| ")
                            .Append($"| <small>{scenarioAnchor}</small> ")
                            .Append($"| {(scenarioStatus == TestStatusEnum.Success ? $":{IconReference.IconStepPassed}:" : null)} ")
                            .Append($"| {(scenarioStatus == TestStatusEnum.Failure ? $":{IconReference.IconStepFailed}:" : null)} ")
                            .Append($"| {(scenarioStatus == TestStatusEnum.Other ? $":{IconReference.IconStepSkipped}:" : null)} ")
                            .Append("| ")
                            .AppendLine();

                        featureSectionBuilder
                            .AppendLine($"### :{StatusIcon(scenarioStatus)}: <i>Scenario:</i>\t{scenario.Title}");

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
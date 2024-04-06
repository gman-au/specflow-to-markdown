using System;
using System.Linq;
using System.Text;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.Markdown.Extensions;
using SpecFlowToMarkdown.Infrastructure.Markdown.Renderer;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public class MarkdownRenderer : IMarkdownRenderer
    {
        private readonly IColourSorter _colourSorter;

        public MarkdownRenderer(IColourSorter colourSorter)
        {
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
                ResultSummariser
                    .SummariseAllFeatures(execution);

            var scenarioSummary =
                ResultSummariser
                    .SummariseAllScenarios(execution);

            var stepSummary =
                ResultSummariser
                    .SummariseAllSteps(execution);

            var tagSummary =
                ResultSummariser
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
                ComponentRenderer
                    .RenderFeature(
                        feature,
                        tocBuilder,
                        featureSectionBuilder,
                        execution
                    );
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
    }
}
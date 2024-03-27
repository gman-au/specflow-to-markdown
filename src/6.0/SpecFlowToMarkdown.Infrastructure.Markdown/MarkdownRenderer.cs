using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public static class MarkdownRenderer
    {
        public static StringBuilder Perform(
            SpecFlowAssembly assembly,
            TestExecution execution
        )
        {
            var result = new StringBuilder();

            var featureSummary =
                Summariser
                    .SummariseAllFeatures(execution);

            var scenarioSummary =
                Summariser
                    .SummariseAllScenarios(execution);

            var stepSummary =
                Summariser
                    .SummariseAllSteps(execution);

            result.AppendLine($"# {assembly.AssemblyName}");

            result.AppendLine("<table>");
            result.AppendLine("<tr>");
            result.AppendLine("<td>");
            result.AppendChart(
                "Features",
                featureSummary.Successes,
                featureSummary.Failures,
                featureSummary.Others
            );
            result.AppendLine("</td>");
            result.AppendLine("<td>");
            result.AppendChart(
                "Scenarios",
                scenarioSummary.Successes,
                scenarioSummary.Failures,
                scenarioSummary.Others
            );
            result.AppendLine("</td>");
            result.AppendLine("<td>");
            result.AppendChart(
                "Steps",
                stepSummary.Successes,
                stepSummary.Failures,
                stepSummary.Others
            );
            result.AppendLine("</td>");
            result.AppendLine("</tr>");
            result.AppendLine("<table>");
            result.AppendLine();

            foreach (var feature in assembly.Features)
            {
                var featureScenarios =
                    execution
                        .ExecutionResults
                        .Where(o => o.FeatureTitle == feature.Title)
                        .ToList();

                var featureSuccesses = featureScenarios.Count(o => Summariser.Assess(o.Status) == TestStatusEnum.Success);
                var featureFails = featureScenarios.Count(o => Summariser.Assess(o.Status) == TestStatusEnum.Failure);
                var featureOthers = featureScenarios.Count(o => Summariser.Assess(o.Status) == TestStatusEnum.Other);

                var status =
                    Summariser
                        .Assess(
                            featureSuccesses,
                            featureFails,
                            featureOthers
                        );

                result.AppendLine($"## {StatusIcon(status)} <i>Feature:</i>\t{feature.Title}");

                foreach (var scenario in feature.Scenarios)
                {
                    var scenarioResult =
                        featureScenarios
                            .FirstOrDefault(o => o.ScenarioTitle == scenario.Title);

                    if (scenarioResult != null)
                    {
                        var scenarioStatus = Summariser.Assess(scenarioResult.Status);
                        result.AppendLine($"### {StatusIcon(scenarioStatus)} <i>Scenario:</i>\t{scenario.Title}");
                        if (!string.IsNullOrEmpty(scenario.Description))
                        {
                            result.AppendLine($"`{scenario.Description.Trim()}`");
                        }

                        result.AppendLine("<table>");

                        for (var i = 0; i < scenario.Steps.Count(); i++)
                        {
                            result.AppendLine("<tr>");
                            var stepDetails = scenario.Steps.ElementAt(i);
                            var stepResult = scenarioResult.StepResults.ElementAt(i);

                            result.AppendLine(
                                $"<td>{StatusCircle(stepResult.Status)}</td>" +
                                $"<td>{FullResultText(stepDetails, stepResult)}</td>" +
                                $"<td>{Math.Round(stepResult.Duration.GetValueOrDefault().TotalSeconds, 2)}s</td>"
                            );

                            if (!string.IsNullOrEmpty(stepResult.Error))
                            {
                                result.AppendLine();
                                result.AppendLine("<td>");
                                result.AppendLine("<details>");
                                result.AppendLine("<summary>Error Details</summary>");
                                result.AppendLine("<code>");
                                result.AppendLine(HttpUtility.HtmlEncode(stepResult.Error.ReplaceLineEndings().Trim()));
                                result.AppendLine("</code>");
                                result.AppendLine("</details>");
                                result.AppendLine("</td>");
                            }
                            else
                            {
                                result.AppendLine("<td/>");
                            }

                            result.AppendLine("</tr>");
                        }

                        result.AppendLine("</table>");
                        result.AppendLine();
                    }
                }
            }

            return result;
        }

        private static string FullResultText(SpecFlowExecutionStep stepDetails, StepResult stepResult)
        {
            var result = $"<strong>{stepDetails.Keyword}</strong> {stepDetails.Text}";

            return result;
        }

        private static string StatusCircle(string result) => StatusCircle(Summariser.Assess(result));
        private static string StatusIcon(string result) => StatusIcon(Summariser.Assess(result));


        private static string StatusCircle(TestStatusEnum result)
        {
            switch (result)
            {
                case TestStatusEnum.Success: return ":green_circle:";
                case TestStatusEnum.Failure: return ":red_circle:";
                default: return ":heavy_minus_sign:";
            }
        } 
        
        private static string StatusIcon(TestStatusEnum result)
        {
            switch (result)
            {
                case TestStatusEnum.Success: return ":heavy_check_mark:";
                case TestStatusEnum.Failure: return ":x:";
                default: return ":heavy_minus_sign:";
            }
        }

        private static void AppendChart(
            this StringBuilder stringBuilder,
            string title,
            int passCount,
            int failCount,
            int otherCount
        )
        {
            var positiveValuesList = new List<string>();

            var resultsBuilder = new StringBuilder();

            if (passCount > 0)
            {
                positiveValuesList.Add("#16c60c88");
                resultsBuilder.AppendLine($"\t\"Pass\": {passCount}");
            }

            if (failCount > 0)
            {
                positiveValuesList.Add("#f03a1788");
                resultsBuilder.AppendLine($"\t\"Fail\": {failCount}");
            }

            if (otherCount > 0)
            {
                positiveValuesList.Add("#fff8");
                resultsBuilder.AppendLine($"\t\"Other\": {otherCount}");
            }

            stringBuilder
                .AppendLine()
                .AppendLine("```mermaid")
                .AppendLine("%%{")
                .AppendLine("\tinit: {")
                .AppendLine("\t\t'theme': 'base',")
                .AppendLine("\t\t'themeVariables': {")
                .AppendLine("\t\t\t'primaryTextColor': '#fff',")
                .AppendLine("\t\t\t'pieStrokeColor': '#8888',")
                .AppendLine("\t\t\t'pieOuterStrokeColor': '#8888',");

            foreach (var positiveValue in positiveValuesList)
            {
                stringBuilder
                    .Append($"\t\t\t'pie{positiveValuesList.IndexOf(positiveValue) + 1}': '{positiveValue}'");

                if (positiveValuesList.Last() != positiveValue)
                    stringBuilder.Append(",");

                stringBuilder
                    .AppendLine();
            }

            stringBuilder
                .AppendLine("\t\t}")
                .AppendLine("\t}")
                .AppendLine("}%%")
                .AppendLine($"pie title {title}")
                .Append(resultsBuilder)
                .AppendLine("```")
                .AppendLine();
        }
    }

    internal class TestSummary
    {
        public int Successes { get; set; }
        public int Failures { get; set; }
        public int Others { get; set; }
    }
}
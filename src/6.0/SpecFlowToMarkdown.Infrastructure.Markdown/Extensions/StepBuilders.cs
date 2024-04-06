using System;
using System.Linq;
using System.Text;
using System.Web;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Extensions
{
    internal static partial class StringBuilderEx
    {
        public static StringBuilder AppendStepsTable(
            this StringBuilder stringBuilder,
            SpecFlowScenario specFlowScenario,
            ExecutionResult executionResult
        )
        {
            stringBuilder
                .AppendLine("<table>");
            
            // Steps
            for (var i = 0; i < specFlowScenario.Steps.Count(); i++)
            {
                stringBuilder
                    .AppendLine("<tr>");

                var stepDetails =
                    specFlowScenario
                        .Steps
                        .ElementAt(i);

                var stepResult =
                    executionResult
                        .StepResults
                        .ElementAt(i);

                stringBuilder
                    .AppendLine(
                        $"<td>:{StatusCircle(stepResult.Status)}:</td>" +
                        $"<td>{FullResultText(stepDetails)}</td>" +
                        $"<td>{Math.Round(stepResult.Duration.GetValueOrDefault().TotalSeconds, 2)}s</td>"
                    );

                if (!string.IsNullOrEmpty(stepResult.Error))
                {
                    stringBuilder
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
                    stringBuilder
                        .AppendLine("<td/>");
                }

                stringBuilder
                    .AppendLine("</tr>");
            }
            
            stringBuilder
                .AppendLine("</table>");

            return stringBuilder;
        }

        private static string FullResultText(SpecFlowExecutionStep stepDetails)
        {
            var result = $"<strong>{stepDetails.Keyword}</strong> {stepDetails.Text}";

            return result;
        }

        private static string StatusCircle(string result) =>
            StatusCircle(result.ToStatusEnum());

        private static string StatusCircle(TestStatusEnum result)
        {
            return result switch
            {
                TestStatusEnum.Success => IconReference.IconStepPassed,
                TestStatusEnum.Failure => IconReference.IconStepFailed,
                _ => IconReference.IconStepSkipped
            };
        }
    }
}
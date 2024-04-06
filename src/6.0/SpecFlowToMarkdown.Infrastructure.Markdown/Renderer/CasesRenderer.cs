using System;
using System.Linq;
using System.Text;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.Markdown.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Renderer
{
    internal static partial class ComponentRenderer
    {
        public static void RenderCases(
            SpecFlowFeature specFlowFeature,
            SpecFlowScenario specFlowScenario,
            StringBuilder tocBuilder,
            StringBuilder contentBuilder,
            TestExecution execution
        )
        {
            // Get all matching results with case arguments
            var caseFeatureResults =
                execution
                    .ExecutionResults
                    .Where(
                        o =>
                            o.ScenarioTitle == specFlowScenario.Title &&
                            o.FeatureTitle == specFlowFeature.Title
                    )
                    .ToList();

            // Complete the header row
            tocBuilder
                .AppendLine($"<td/>")
                .AppendLine($"<td/>")
                .AppendLine($"<td/>")
                .AppendLine($"<td/>")
                .AppendLine($"<td/>");

            foreach (var scenarioCase in specFlowScenario.Cases)
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

                    var caseStatusIcon = caseStatus.ToStatusIcon();

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
                        // .AppendLine($"<td>{Math.Round(scenarioDuration, 2)}s</td>")
                        .AppendLine($"<td>{Math.Round(10.0D, 2)}s</td>")
                        .AppendLine("</tr>");
                }
            }
        }
    }
}
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
        private static void RenderScenarios(
            SpecFlowFeature specFlowFeature,
            StringBuilder tocBuilder,
            StringBuilder contentBuilder,
            TestExecution execution
        )
        {
            var featureScenarios =
                execution
                    .ExecutionResults
                    .Where(o => o.FeatureTitle == specFlowFeature.Title)
                    .ToList();

            foreach (var scenario in specFlowFeature.Scenarios)
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

                    var scenarioStatusIcon =
                        scenarioStatus
                            .ToStatusIcon();

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

                    contentBuilder
                        .AppendLine(
                            $"<h3> :{scenarioStatus.ToStatusIcon()}: <a id=\"{scenarioAnchor}\"><i>Scenario:</i>\t{scenario.Title}</a></h3>"
                        );

                    // Test cases
                    if (scenario.Cases.Any())
                    {
                        // Complete the header row
                        tocBuilder
                            .AppendLine($"<td/>")
                            .AppendLine($"<td/>")
                            .AppendLine($"<td/>")
                            .AppendLine($"<td/>")
                            .AppendLine($"<td/>");

                        RenderCases(
                            specFlowFeature,
                            scenario,
                            tocBuilder,
                            contentBuilder,
                            execution
                        );
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

                        contentBuilder
                            .AppendTags(scenario);

                        if (!string.IsNullOrEmpty(scenario.Description))
                        {
                            contentBuilder
                                .AppendLine()
                                .AppendLine()
                                .AppendLine($"`{scenario.Description.Trim()}`");
                        }

                        contentBuilder
                            .AppendStepsTable(
                                scenario,
                                scenarioResult
                            );

                        contentBuilder
                            .AppendLine();
                    }
                }
            }
        }
    }
}
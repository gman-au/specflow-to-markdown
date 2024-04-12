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
        private static void RenderCases(
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

            var cases =
                (specFlowScenario
                    .Cases ?? Enumerable.Empty<SpecFlowCase>())
                .ToList();

            foreach (var scenarioCase in cases)
            {
                var caseIndex =
                    cases
                        .IndexOf(scenarioCase) + 1;

                var caseAnchor =
                    $"{specFlowScenario.Title}Case{caseIndex}"
                        .ToAnchor();

                tocBuilder
                    .AppendLine("<tr>")
                    .AppendLine($"<td/>")
                    .AppendLine($"<td/>")
                    .AppendLine($"<td>");

                tocBuilder
                    .AppendLine("<sub>")
                    .AppendLine($"<a href=\"#{caseAnchor}\">Case #{caseIndex}</a>")
                    .AppendLine();

                var argumentsBuilder = new StringBuilder();

                argumentsBuilder
                    .AppendLine()
                    .AppendLine("```");

                foreach (var argument in scenarioCase.Arguments)
                {
                    argumentsBuilder
                        .AppendLine($"{argument.ArgumentName}: {argument.ArgumentValue}");
                }

                argumentsBuilder
                    .AppendLine("```")
                    .AppendLine();

                tocBuilder
                    .Append(argumentsBuilder)
                    .AppendLine("</sub>")
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

                                    if (i >= x.ScenarioArguments.Count())
                                    {
                                        throw new Exception(
                                            "Mismatch between configured arguments and test execution results; please re-run the tests and try again."
                                        );
                                    }

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
                        .AppendLine($"<td>{Math.Round(caseFeatureResult.StepResults.Sum(o => o.Duration.GetValueOrDefault().TotalSeconds), 2)}s</td>")
                        .AppendLine("</tr>");

                    contentBuilder
                        .AppendLine(
                            $"<h4> :{caseStatus.ToStatusIcon()}: <a id=\"{caseAnchor}\">Case #{caseIndex}</a></h4>"
                        );
                    
                    contentBuilder
                        .AppendTags(specFlowScenario);

                    contentBuilder
                        .Append(argumentsBuilder);

                    contentBuilder
                        .AppendStepsTable(
                            specFlowScenario,
                            caseFeatureResult
                        );

                    contentBuilder
                        .AppendLine();
                }
            }
        }
    }
}
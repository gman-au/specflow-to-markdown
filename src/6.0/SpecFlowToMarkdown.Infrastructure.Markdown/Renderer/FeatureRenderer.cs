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
        public static void RenderFeature(
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
                ResultSummariser
                    .Assess(
                        featureSuccesses,
                        featureFails,
                        featureOthers
                    );

            var featureStatusIcon =
                status
                    .ToStatusIcon();

            var featureAnchor =
                $"Feature:{specFlowFeature.Title}"
                    .ToAnchor(featureStatusIcon);

            tocBuilder
                .AppendLine($"<td><a href=\"#{featureAnchor}\">Feature:\t{specFlowFeature.Title}</a></td>")
                .AppendLine($"<td/>")
                .AppendLine($"<td/>")
                .AppendLine($"<td>{featureSuccesses} {(featureSuccesses > 0 ? $":{IconReference.IconSuitePassed}:" : null)}</td>")
                .AppendLine($"<td>{featureFails} {(featureFails > 0 ? $":{IconReference.IconSuiteFailed}:" : null)}</td>")
                .AppendLine($"<td>{featureOthers} {(featureOthers > 0 ? $":{IconReference.IconSuiteSkipped}:" : null)}</td>")
                .AppendLine($"<td>{Math.Round(featureDuration, 2)}s</td>")
                .AppendLine();

            contentBuilder
                .AppendLine($"<h2> :{status.ToStatusIcon()}: <a id=\"{featureAnchor}\"><i>Feature:</i>\t{specFlowFeature.Title}</a></h2>");

            if (!string.IsNullOrEmpty(specFlowFeature.Description))
            {
                contentBuilder
                    .AppendLine()
                    .Append(specFlowFeature.Description)
                    .AppendLine();
            }

            RenderScenarios(
                specFlowFeature,
                tocBuilder,
                contentBuilder,
                execution
            );
        }
    }
}
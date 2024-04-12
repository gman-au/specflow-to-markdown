using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Extensions
{
    internal static partial class StringBuilderEx
    {
        public static StringBuilder AppendTagChart(
            this StringBuilder stringBuilder,
            string title,
            IDictionary<string, TestSummary> results
        )
        {
            stringBuilder
                .AppendLine()
                .AppendLine("```mermaid")
                .AppendLine("%%{")
                .AppendLine("\tinit: {")
                .AppendLine("\t\t'theme': 'base',")
                .AppendLine("\t\t'themeVariables': {")
                .AppendLine("\t\t\t'xyChart': {")
                .AppendLine("\t\t\t\t'titleColor': \"#fff\",")
                .AppendLine("\t\t\t\t'xAxisLabelColor': \"#fff\",")
                .AppendLine("\t\t\t\t'xAxisTitleColor': \"#fff\",")
                .AppendLine("\t\t\t\t'xAxisTickColor': \"#fff\",")
                .AppendLine("\t\t\t\t'xAxisLineColor': \"#fff\",")
                .AppendLine("\t\t\t\t'yAxisLabelColor': \"#fff\",")
                .AppendLine("\t\t\t\t'yAxisTitleColor': \"#fff\",")
                .AppendLine("\t\t\t\t'yAxisTickColor': \"#fff\",")
                .AppendLine("\t\t\t\t'yAxisLineColor': \"#fff\",")
                .AppendLine($"\t\t\t\t'backgroundColor': \"#0000\",")
                .AppendLine($"\t\t\t\t'plotColorPalette': \"{ColourSorter.PassColourSolid}, {ColourSorter.FailColourSolid}, {ColourSorter.OtherColourSolid}\"");

            stringBuilder
                .AppendLine("\t\t\t}")
                .AppendLine("\t\t}")
                .AppendLine("\t}")
                .AppendLine("}%%");

            stringBuilder
                .AppendLine("xychart-beta")
                .AppendLine($"title {title}");

            var xAxis = string.Join(
                ", ",
                results.Keys
            );

            stringBuilder
                .AppendLine($"x-axis [{xAxis}]")
                .AppendLine($"y-axis \"Tests\"");

            var values = results.Values;
            
            // Reworked slightly to a 'stacked' chart instead of a standard bar chart.
            // The next value in the series will be a concurrent sum so as not to 'overlap'.
            // Successes first, then failures, then other.
            var successValues = string.Join(
                ", ",
                values.Select(o => o.Successes + o.Failures + o.Others)
            );

            var failureValues = string.Join(
                ", ",
                values.Select(o => o.Failures + o.Others)
            );
            
            var otherValues = string.Join(
                ", ",
                values.Select(o => o.Others)
            );

            stringBuilder
                .AppendLine($"bar [{successValues}]")
                .AppendLine($"bar [{failureValues}]")
                .AppendLine($"bar [{otherValues}]");

            stringBuilder
                .AppendLine("```");

            return stringBuilder;
        }

        public static StringBuilder AppendPieChart(
            this StringBuilder stringBuilder,
            string title,
            ICollection<ChartLegendItem> legend
        )
        {
            // pieColor is determined by value, descending

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

            for (var i = 0; i < legend.Count; i++)
            {
                var legendItem = legend.ElementAt(i);

                stringBuilder
                    .Append($"\t\t\t'pie{i + 1}': '{legendItem.Colour}'");

                if (i + 1 < legend.Count)
                    stringBuilder.Append(",");

                stringBuilder
                    .AppendLine();
            }

            stringBuilder
                .AppendLine("\t\t}")
                .AppendLine("\t}")
                .AppendLine("}%%")
                .AppendLine($"pie title {title}");

            foreach (var legendItem in legend)
            {
                stringBuilder
                    .AppendLine($"\t\"{legendItem.Title}\": {legendItem.Value}");
            }

            stringBuilder
                .AppendLine("```")
                .AppendLine();

            return stringBuilder;
        }
    }
}
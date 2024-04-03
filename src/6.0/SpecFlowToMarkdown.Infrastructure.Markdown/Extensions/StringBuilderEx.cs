using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Extensions
{
    public static class StringBuilderEx
    {
        public static StringBuilder AppendTagChart(
            this StringBuilder stringBuilder,
            string title,
            ICollection<ChartLegendItem> tags
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
                .AppendLine($"\t\t\t\t'plotColorPalette': \"{ColourSorter.OtherColour}, {ColourSorter.PassColour}\"");

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
                tags.Select(o => o.Title)
            );

            stringBuilder
                .AppendLine($"x-axis [{xAxis}]")
                .AppendLine($"y-axis \"Tests\"");

            var barValues = string.Join(
                ", ",
                tags.Select(o => o.PrimaryValue)
            );

            var lineValues = string.Join(
                ", ",
                tags.Select(o => o.SecondaryValue)
            );

            stringBuilder
                .AppendLine($"bar [{barValues}]")
                .AppendLine($"line [{lineValues}]");

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
                    .AppendLine($"\t\"{legendItem.Title}\": {legendItem.PrimaryValue}");
            }

            stringBuilder
                .AppendLine("```")
                .AppendLine();

            return stringBuilder;
        }
    }
}
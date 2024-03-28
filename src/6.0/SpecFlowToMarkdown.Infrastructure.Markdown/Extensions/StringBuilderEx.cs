using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Extensions
{
    public static class StringBuilderEx
    {
        public static StringBuilder AppendChart(
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

            for(var i=0; i < legend.Count; i++)
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
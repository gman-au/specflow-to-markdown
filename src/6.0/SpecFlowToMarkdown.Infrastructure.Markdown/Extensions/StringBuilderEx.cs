using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Extensions
{
    public static class StringBuilderEx
    {
        public static void AppendChart(
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
}
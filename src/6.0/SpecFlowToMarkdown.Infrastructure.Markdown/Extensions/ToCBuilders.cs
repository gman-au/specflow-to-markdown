using System.Text;
using SpecFlowToMarkdown.Domain;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Extensions
{
    internal static partial class StringBuilderEx
    {
        public static StringBuilder AppendToCRow(
            this StringBuilder stringBuilder,
            string featureText,
            string scenarioText,
            string caseText,
            TestStatusEnum status,
            double duration
        )
        {
            return
                stringBuilder;
        }
    }
}
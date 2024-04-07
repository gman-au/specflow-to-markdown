using System.Linq;
using System.Text;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.Markdown.Extensions
{
    internal static partial class StringBuilderEx
    {
        public static StringBuilder AppendTags(
            this StringBuilder stringBuilder,
            SpecFlowScenario specFlowScenario
        )
        {
            if ((specFlowScenario.Tags ?? Enumerable.Empty<string>()).Any())
            {
                stringBuilder
                    .AppendLine()
                    .Append($":label:");

                var tagString =
                    string.Join(
                        ", ",
                        specFlowScenario
                            .Tags
                            .Select(
                                o =>
                                    o
                                        .Replace(
                                            ",",
                                            ""
                                        )
                                        .Insert(
                                            0,
                                            "\\@"
                                        )
                            )
                    );

                stringBuilder
                    .Append($" {tagString}")
                    .AppendLine();
            }

            return 
                stringBuilder;
        }
    }
}
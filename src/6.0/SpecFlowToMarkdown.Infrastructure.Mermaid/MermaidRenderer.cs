using System.Text;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;

namespace SpecFlowToMarkdown.Infrastructure.Mermaid
{
    public static class MermaidRenderer
    {
        public static StringBuilder Perform(TestExecution testExecution)
        {
            var result = new StringBuilder();

            // Text in file replace header
            /*result
                .AppendLine(MermaidConstants.SirenAnchorStart);

            // Mermaid header
            result
                .AppendLine(MermaidConstants.MermaidAnchorStart);

            // (optional) neutral theme
            result
                .AppendLine($"\t{MermaidConstants.MermaidNeutralThemeLine}");

            // Header
            result
                .AppendLine($"\t{MermaidConstants.MermaidErDiagramHeader}");

            foreach (var entity in testExecution.Entities)
            {
                // Entity header
                result
                    .AppendLine($"\t{entity.ShortName} {{");

                foreach (var property in entity.Properties)
                {
                    result
                        .Append($"\t\t{property.Type} {property.Name} ");

                    var keys = new List<string>();
                    if (property.IsPrimaryKey)
                        keys.Add("PK");
                    if (property.IsForeignKey)
                        keys.Add("FK");
                    if (property.IsUniqueKey)
                        keys.Add("UK");

                    if (keys.Any())
                    {
                        result
                            .AppendJoin(
                                ',',
                                keys
                            );
                    }

                    result
                        .AppendLine();
                }

                // Entity footer
                result
                    .AppendLine("\t}");
            }

            foreach (var relationship in testExecution.Relationships)
            {
                result
                    .AppendLine(
                        $"{relationship.Source?.ShortName}" +
                        $"{MapCardinalityToString(relationship.SourceCardinality, true)}--" +
                        $"{MapCardinalityToString(relationship.TargetCardinality, false)}" +
                        $"{relationship.Target?.ShortName} " +
                        ": \"\""
                    );
            }

            // Mermaid footer
            result
                .AppendLine(MermaidConstants.MermaidAnchorEnd);

            // Text in file replace footer
            result
                .AppendLine(MermaidConstants.SirenAnchorEnd);
*/
            return result;
        }
    }
}
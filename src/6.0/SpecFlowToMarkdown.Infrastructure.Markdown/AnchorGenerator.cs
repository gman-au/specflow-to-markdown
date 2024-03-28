namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public class AnchorGenerator : IAnchorGenerator
    {
        public string Build(string title, string icon = null, string titleText = null)
        {
            var id =
                title
                    .ToLower()
                    .Trim()
                    .Replace(
                        " ",
                        "-"
                    )
                    .Replace(
                        ":",
                        ""
                    );

            var iconString = string.Empty;

            if (icon != null)
            {
                iconString = $"{icon}-";
            }
            
            return $"<a href=\"#{iconString}{id}\">{titleText ?? title}</a>";
        }
    }
}
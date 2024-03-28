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
            
            return $"[{titleText ?? title}](#{iconString}{id})";
        }
    }
}
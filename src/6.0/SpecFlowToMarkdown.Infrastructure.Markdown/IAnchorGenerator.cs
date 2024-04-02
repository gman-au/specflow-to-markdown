namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public interface IAnchorGenerator
    {
        public string Build(string title, string icon = null);
    }
}
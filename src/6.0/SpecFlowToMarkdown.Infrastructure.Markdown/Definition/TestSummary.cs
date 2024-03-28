namespace SpecFlowToMarkdown.Infrastructure.Markdown.Definition
{
    public class TestSummary
    {
        public int Successes { get; set; }
        
        public int Failures { get; set; }
        
        public int Others { get; set; }
        
        public double Duration { get; set; }
    }
}
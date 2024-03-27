using System.Collections.Generic;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public interface IColourSorter
    {
       ICollection<ChartLegendItem> Sort(int passCount, int failCount, int otherCount);
    }
}
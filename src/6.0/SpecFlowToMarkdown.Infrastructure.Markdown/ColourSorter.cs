using System.Collections.Generic;
using System.Linq;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public class ColourSorter : IColourSorter
    {
        public const string PassColourTransparent = "#16c60c88";
        public const string FailColourTransparent = "#f03a1788";
        public const string OtherColourTransparent = "#fff8";
        public const string PassColourSolid = "#676a6d";
        public const string FailColourSolid = "#105512";
        public const string OtherColourSolid = "#622116";

        public ICollection<ChartLegendItem> Sort(int passCount, int failCount, int otherCount)
        {
            var result = new List<ChartLegendItem>
            {
                new()
                {
                    Title = "Pass",
                    Colour = PassColourTransparent,
                    Value = passCount
                },
                new()
                {
                    Title = "Fail",
                    Colour = FailColourTransparent,
                    Value = failCount
                },
                new()
                {
                    Title = "Other",
                    Colour = OtherColourTransparent,
                    Value = otherCount
                }
            };

            return
                result
                    .OrderByDescending(o => o.Value)
                    .ToList();
        }
    }
}
﻿using System.Collections.Generic;
using System.Linq;
using SpecFlowToMarkdown.Infrastructure.Markdown.Definition;

namespace SpecFlowToMarkdown.Infrastructure.Markdown
{
    public class ColourSorter : IColourSorter
    {
        public const string PassColour = "#16c60c88";
        public const string FailColour = "#f03a1788";
        public const string OtherColour = "#fff8";

        public ICollection<ChartLegendItem> Sort(int passCount, int failCount, int otherCount)
        {
            var result = new List<ChartLegendItem>
            {
                new()
                {
                    Title = "Pass",
                    Colour = PassColour,
                    PrimaryValue = passCount
                },
                new()
                {
                    Title = "Fail",
                    Colour = FailColour,
                    PrimaryValue = failCount
                },
                new()
                {
                    Title = "Other",
                    Colour = OtherColour,
                    PrimaryValue = otherCount
                }
            };

            return
                result
                    .OrderByDescending(o => o.PrimaryValue)
                    .ToList();
        }
    }
}
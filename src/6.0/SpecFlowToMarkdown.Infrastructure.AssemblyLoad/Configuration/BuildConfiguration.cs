using System;
using System.Collections.Generic;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Configuration
{
    public class BuildConfiguration : IBuildConfiguration
    {
        public IEnumerable<Tuple<string, int, int, int>> Get()
        {
            var buildConfigurations = new List<Tuple<string, int, int, int>>
            {
                new(
                    "Debug",
                    3,
                    5,
                    30
                ), // debug
                new(
                    "Release",
                    2,
                    4,
                    19
                ) // release
            };

            return buildConfigurations;
        }
    }
}
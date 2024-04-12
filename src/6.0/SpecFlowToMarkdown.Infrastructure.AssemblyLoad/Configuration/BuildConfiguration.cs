using System;
using System.Collections.Generic;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Configuration
{
    public class BuildConfiguration : IBuildConfiguration
    {
        public IEnumerable<Tuple<int, int, int>> Get()
        {
            var buildConfigurations = new List<Tuple<int, int, int>>
            {
                new(
                    3,
                    5,
                    30
                ), // debug
                new(
                    2,
                    4,
                    19
                ) // release
            };

            return buildConfigurations;
        }
    }
}
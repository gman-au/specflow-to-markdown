using System;
using System.Collections.Generic;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Configuration
{
    public class BuildConfiguration : IBuildConfiguration
    {
        public IEnumerable<Tuple<int, int>> Get()
        {
            var buildConfigurations = new List<Tuple<int, int>>
            {
                new(
                    3,
                    5
                ), // debug
                new(
                    2,
                    4
                ) // release
            };

            return buildConfigurations;
        }
    }
}
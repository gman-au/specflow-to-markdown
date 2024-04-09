using System;
using System.Collections.Generic;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Configuration
{
    public interface IBuildConfiguration
    {
        public IEnumerable<Tuple<int, int>> Get();
    }
}
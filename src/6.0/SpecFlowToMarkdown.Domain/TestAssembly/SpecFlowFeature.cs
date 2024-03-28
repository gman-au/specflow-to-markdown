using System.Collections.Generic;

namespace SpecFlowToMarkdown.Domain.TestAssembly
{
    public class SpecFlowFeature
    {
        public string FolderPath { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IEnumerable<SpecFlowScenario> Scenarios { get; set; }
    }
}
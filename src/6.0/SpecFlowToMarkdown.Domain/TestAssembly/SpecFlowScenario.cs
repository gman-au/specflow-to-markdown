using System.Collections.Generic;

namespace SpecFlowToMarkdown.Domain.TestAssembly
{
    public class SpecFlowScenario
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<SpecFlowExecutionStep> Steps { get; set; }
        
        public IEnumerable<SpecFlowCase> Cases { get; set; }
    }
}
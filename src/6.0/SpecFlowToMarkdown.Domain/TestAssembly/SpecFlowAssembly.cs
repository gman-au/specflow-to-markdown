using System.Collections.Generic;

namespace SpecFlowToMarkdown.Domain.TestAssembly
{
    public class SpecFlowAssembly
    {
        public string AssemblyName { get; set; }
        
        public IEnumerable<SpecFlowFeature> Features { get; set; }
    }
}
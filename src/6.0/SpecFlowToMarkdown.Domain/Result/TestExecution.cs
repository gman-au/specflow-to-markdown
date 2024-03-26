using System;
using System.Collections.Generic;

namespace SpecFlowToMarkdown.Domain.Result
{
    public class TestExecution
    {
        public DateTime ExecutionTime { get; set; }
        
        public DateTime GenerationTime { get; set; }
        
        public Guid PluginUserSpecFlowId { get; set; }
        
        public IEnumerable<ExecutionResult> ExecutionResults { get; init; }
    }
}
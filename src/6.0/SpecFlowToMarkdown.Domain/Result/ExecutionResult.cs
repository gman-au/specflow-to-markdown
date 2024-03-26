using System.Collections.Generic;

namespace SpecFlowToMarkdown.Domain.Result
{
    public class ExecutionResult
    {
        public string ContextType { get; set; }
        
        public string FeatureFolderPath { get; set; }
        
        public string FeatureTitle { get; set; }
        
        public string ScenarioTitle { get; set; }
        
        public string Status { get; set; }
        
        public IEnumerable<StepResult> StepResults { get; set; }
    }
}
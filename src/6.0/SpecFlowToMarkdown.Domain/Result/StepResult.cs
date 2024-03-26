using System;

namespace SpecFlowToMarkdown.Domain.Result
{
    public class StepResult
    {
        public TimeSpan Duration { get; set; }
        
        public string Status { get; set; }
        
        public string Error { get; set; }
    }
}
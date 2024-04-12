using System.Collections.Generic;
using Mono.Cecil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Step
{
    public interface IStepExtractor
    {
        IEnumerable<SpecFlowExecutionStep> Perform(
            MethodDefinition method,
            Dictionary<string, string> argumentNames);
    }
}
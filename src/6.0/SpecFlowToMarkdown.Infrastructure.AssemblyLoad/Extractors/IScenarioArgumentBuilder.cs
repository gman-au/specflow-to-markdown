using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public interface IScenarioArgumentBuilder
    {
        Dictionary<string, string> Build(Instruction currInstr);
    }
}
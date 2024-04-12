using Mono.Cecil.Cil;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extensions
{
    internal static class InstructionEx
    {
        public static Instruction StepPrevious(this Instruction instruction, int times)
        {
            for (var i = 0; i < times && instruction != null; i++)
            {
                instruction =
                    instruction
                        .Previous;
            }

            return instruction;
        }
    }
}
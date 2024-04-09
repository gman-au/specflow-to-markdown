using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Configuration;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public class ScenarioArgumentBuilder : IScenarioArgumentBuilder
    {
        private readonly IBuildConfiguration _buildConfiguration;

        public ScenarioArgumentBuilder(IBuildConfiguration buildConfiguration)
        {
            _buildConfiguration = buildConfiguration;
        }

        public Dictionary<string, string> Build(Instruction currInstr)
        {
            var startingInstruction = currInstr;
            var scenarioArgumentNames = new Dictionary<string, string>();

            foreach (var buildConfiguration in _buildConfiguration.Get())
            {
                currInstr = startingInstruction;

                // Get test case argument names
                currInstr =
                    currInstr
                        .StepPrevious(buildConfiguration.Item1);

                while (true)
                {
                    string argKey = null;
                    string argValue = null;

                    if (currInstr.OpCode == OpCodes.Ldarg_S)
                    {
                        argKey =
                            currInstr
                                .Operand
                                .ToString();
                    }
                    else if (currInstr.OpCode == OpCodes.Ldarg_3)
                        argKey = "3";
                    else if (currInstr.OpCode == OpCodes.Ldarg_2)
                        argKey = "2";
                    else if (currInstr.OpCode == OpCodes.Ldarg_1)
                        argKey = "1";
                    else if (currInstr.OpCode == OpCodes.Ldarg_0)
                        argKey = "0";

                    var stringLoadInstruction =
                        currInstr
                            .Previous;

                    if (stringLoadInstruction.OpCode == OpCodes.Ldstr)
                    {
                        argValue =
                            stringLoadInstruction
                                .Operand
                                .ToString();
                    }

                    if (!string.IsNullOrEmpty(argKey) && !string.IsNullOrEmpty(argValue))
                    {
                        scenarioArgumentNames
                            .Add(
                                argKey,
                                argValue
                            );

                        currInstr =
                            currInstr
                                .StepPrevious(buildConfiguration.Item2);
                    }
                    else
                        break;
                }

                if (scenarioArgumentNames.Count > 0)
                    break;
            }

            if (scenarioArgumentNames.Count == 0)
                throw new Exception(
                    "Could not extract test cases from assembly; please try again with a different build configuration"
                );

            return
                scenarioArgumentNames;
        }
    }
}
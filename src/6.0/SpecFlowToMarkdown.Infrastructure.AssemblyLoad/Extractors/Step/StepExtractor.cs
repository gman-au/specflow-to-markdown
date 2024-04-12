using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extensions;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Utils;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Step
{
    public class StepExtractor : IStepExtractor
    {
        private readonly ILogger<StepExtractor> _logger;

        public StepExtractor(ILogger<StepExtractor> logger)
        {
            _logger = logger;
        }

        public IEnumerable<SpecFlowExecutionStep> Perform(MethodDefinition method, Dictionary<string, string> argumentNames)
        {
            // Extract Steps
            var scenarioSteps = new List<SpecFlowExecutionStep>();

            foreach (var instruction in
                     method
                         .Body
                         .Instructions
                         .Where(o => o.OpCode == OpCodes.Callvirt))
            {
                if (instruction.Operand is MethodReference methodReference)
                {
                    if (Constants.ScenarioStepFunctions.Any(o => methodReference.Name == o))
                    {
                        var keyword = methodReference.Name;
                        var text = string.Empty;

                        var currInstr =
                            instruction
                                .StepPrevious(4);

                        if (currInstr.OpCode == OpCodes.Ldstr)
                        {
                            text =
                                currInstr
                                    .Operand
                                    .ToString();
                        }
                        else if (currInstr.OpCode == OpCodes.Call)
                        {
                            if (currInstr.Operand is MethodReference mr)
                            {
                                if (mr.Name == Constants.StringFormatFunctionName)
                                {
                                    var stringFormatArguments = new List<string>();
                                    string preFormatText = null;

                                    while (true)
                                    {
                                        string argKey = null;

                                        currInstr =
                                            currInstr
                                                .Previous;

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
                                        else if (currInstr.OpCode == OpCodes.Ldstr)
                                            preFormatText =
                                                currInstr
                                                    .Operand
                                                    .ToString();

                                        if (string.IsNullOrEmpty(argKey))
                                            break;

                                        if (argumentNames.TryGetValue(
                                                argKey,
                                                out var argumentName
                                            ))
                                        {
                                            stringFormatArguments
                                                .Add($"&lt;{argumentName}&gt;");
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(preFormatText) && stringFormatArguments.Any())
                                    {
                                        stringFormatArguments
                                            .Reverse();

                                        text =
                                            string
                                                .Format(
                                                    preFormatText,
                                                    stringFormatArguments
                                                        .ToArray()
                                                );
                                    }
                                }
                                else
                                {
                                    if (currInstr.OpCode == OpCodes.Ldstr)
                                    {
                                        text =
                                            currInstr
                                                .Operand
                                                .ToString();
                                    }
                                }
                            }
                        }

                        var executionStep = new SpecFlowExecutionStep
                        {
                            Keyword = keyword,
                            Text = text
                        };

                        _logger
                            .LogInformation($"Extracted test step [{executionStep.Keyword} {executionStep.Text}]");

                        scenarioSteps
                            .Add(executionStep);
                    }
                }
            }

            return 
                scenarioSteps;
        }
    }
}
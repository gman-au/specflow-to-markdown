using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public static class FeatureExtractor
    {
        private const string CustomFeatureAttributeValue = "TechTalk.SpecFlow";
        private const string FeatureSetupMethodName = "FeatureSetup";
        private const string FeatureInfoTypeName = "TechTalk.SpecFlow.FeatureInfo";

        public static SpecFlowAssembly ExtractFeatures(this AssemblyDefinition assembly)
        {
            var assemblyName = assembly.Name.Name;

            var result = new SpecFlowAssembly
            {
                AssemblyName = assemblyName
            };

            var resultFeatures = new List<SpecFlowFeature>();

            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (type.CustomAttributes
                        .Any(
                            o =>
                                o
                                    .ConstructorArguments
                                    .Any(
                                        arg =>
                                            arg.Value != null && arg.Value.ToString() == CustomFeatureAttributeValue
                                    )
                        ))
                    {
                        foreach (var method in type.Methods)
                        {
                            if (method.Name == FeatureSetupMethodName)
                            {
                                foreach (var instruction in
                                         method
                                             .Body
                                             .Instructions
                                             .Where(o => o.OpCode == OpCodes.Newobj))
                                {
                                    if (instruction.Operand is MethodReference methodReference)
                                    {
                                        if (methodReference.DeclaringType.FullName == FeatureInfoTypeName)
                                        {
                                            var description = string.Empty;
                                            var title = string.Empty;
                                            var folderPath = string.Empty;

                                            var currInstr =
                                                instruction
                                                    .Previous
                                                    .Previous
                                                    .Previous;

                                            if (currInstr.OpCode == OpCodes.Ldstr)
                                            {
                                                description = currInstr.Operand.ToString();
                                                currInstr = currInstr.Previous;
                                            }

                                            if (currInstr.OpCode == OpCodes.Ldstr)
                                            {
                                                title = currInstr.Operand.ToString();
                                                currInstr = currInstr.Previous;
                                            }

                                            if (currInstr.OpCode == OpCodes.Ldstr)
                                            {
                                                folderPath = currInstr.Operand.ToString();
                                            }

                                            var feature = new SpecFlowFeature
                                            {
                                                FolderPath = folderPath,
                                                Title = title,
                                                Description = description
                                            };

                                            var scenarios = type.ExtractScenarios();
                                            feature.Scenarios = scenarios;
                                            
                                            resultFeatures.Add(feature);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            result.Features = resultFeatures;

            return result;
        }
    }
}
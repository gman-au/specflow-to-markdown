using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extensions;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Scenario;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Utils;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Feature
{
    public class FeatureExtractor : IFeatureExtractor
    {
        private readonly IEnumerable<IScenarioExtractor> _extractors;
        private readonly ILogger<FeatureExtractor> _logger;

        public FeatureExtractor(
            ILogger<FeatureExtractor> logger, 
            IEnumerable<IScenarioExtractor> extractors
        )
        {
            _logger = logger;
            _extractors = extractors;
        }
        
        public SpecFlowAssembly Perform(AssemblyDefinition assembly)
        {
            var assemblyName = 
                assembly
                    .Name
                    .Name;

            var inferredBuildConfiguration = "Unknown";
            
            _logger
                .LogInformation($"Assembly name: [{assemblyName}]");

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
                                            arg.Value != null && arg.Value.ToString() == Constants.CustomFeatureAttributeValue
                                    )
                        ))
                    {
                        _logger
                            .LogInformation($"Found {type.Methods.Count} feature methods in assembly [{assemblyName}]");
                        
                        foreach (var method in type.Methods)
                        {
                            if (method.Name != Constants.FeatureSetupMethodName) continue;
                            
                            foreach (var instruction in
                                     method
                                         .Body
                                         .Instructions
                                         .Where(o => o.OpCode == OpCodes.Newobj))
                            {
                                if (instruction.Operand is not MethodReference methodReference) continue;

                                if (methodReference.DeclaringType.FullName != Constants.FeatureInfoTypeName) continue;
                                
                                var description = string.Empty;
                                var title = string.Empty;
                                var folderPath = string.Empty;

                                var currInstr =
                                    instruction
                                        .StepPrevious(3);

                                if (currInstr.OpCode == OpCodes.Ldstr)
                                {
                                    description =
                                        currInstr
                                            .Operand
                                            .ToString();

                                    currInstr =
                                        currInstr
                                            .Previous;
                                }

                                if (currInstr.OpCode == OpCodes.Ldstr)
                                {
                                    title =
                                        currInstr
                                            .Operand
                                            .ToString();
                                                
                                    _logger
                                        .LogInformation($"Extracted feature: [{title}]");

                                    currInstr =
                                        currInstr
                                            .Previous;
                                }

                                if (currInstr.OpCode == OpCodes.Ldstr)
                                {
                                    folderPath =
                                        currInstr
                                            .Operand
                                            .ToString();
                                }

                                var feature = new SpecFlowFeature
                                {
                                    FolderPath = folderPath,
                                    Title = title,
                                    Description = description
                                };

                                var scenarios = new List<SpecFlowScenario>();

                                foreach (var typeMethod in type.Methods)
                                {
                                    var applicableExtractor =
                                        _extractors
                                            .FirstOrDefault(o => o.IsApplicable(typeMethod));

                                    if (applicableExtractor == null)
                                    {
                                        _logger
                                            .LogWarning($"Could not find applicable scenario extractor for method type [{typeMethod.Name}]");
                                                    
                                        continue;
                                    }

                                    var scenario =
                                        applicableExtractor
                                            .Perform(
                                                typeMethod,
                                                type,
                                                ref inferredBuildConfiguration
                                            );

                                    scenarios
                                        .Add(scenario);
                                }

                                feature.Scenarios = scenarios;

                                resultFeatures
                                    .Add(feature);
                            }
                        }
                    }
                }
            }

            result.Features = resultFeatures;

            result.BuildConfiguration = inferredBuildConfiguration;
            
            return result;
        }
    }
}
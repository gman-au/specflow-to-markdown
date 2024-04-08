using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Extensions;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    public class FeatureExtractor : IFeatureExtractor
    {
        private const string CustomFeatureAttributeValue = "TechTalk.SpecFlow";
        private const string FeatureSetupMethodName = "FeatureSetup";
        private const string FeatureInfoTypeName = "TechTalk.SpecFlow.FeatureInfo";
        private const string DebuggingModeAttributeName = "System.Diagnostics.DebuggableAttribute/DebuggingModes";

        private readonly IScenarioExtractionHandler _scenarioExtractionHandler;
        private readonly ILogger<FeatureExtractor> _logger;

        public FeatureExtractor(
            IScenarioExtractionHandler scenarioExtractionHandler, 
            ILogger<FeatureExtractor> logger)
        {
            _scenarioExtractionHandler = scenarioExtractionHandler;
            _logger = logger;
        }
        
        public SpecFlowAssembly ExtractFeatures(AssemblyDefinition assembly)
        {
            var assemblyName = 
                assembly
                    .Name
                    .Name;
            
            _logger
                .LogInformation($"Assembly name: [{assemblyName}]");
            
            var isDebug = IsDebugBuild(assembly);
            
            _logger
                .LogInformation($"Debug build: [{isDebug}]");

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
                        _logger
                            .LogInformation($"Found {type.Methods.Count} feature methods in assembly [{assemblyName}]");
                        
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

                                            var scenarios =
                                                _scenarioExtractionHandler
                                                    .ExtractScenarios(type, isDebug);

                                            feature.Scenarios = scenarios;

                                            resultFeatures
                                                .Add(feature);
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
        
        private bool IsDebugBuild(ICustomAttributeProvider assembly)
        {
            _logger
                .LogInformation($"Loading custom attributes for assembly");

            var customAttributes =
                (assembly?
                    .CustomAttributes ?? Enumerable.Empty<CustomAttribute>())
                .ToList();
            
            _logger
                .LogInformation($"Custom attributes loaded [{customAttributes.Count}]");

            var debuggableAttribute =
                customAttributes
                    .FirstOrDefault(o => o.AttributeType.FullName == typeof(DebuggableAttribute).FullName);

            if (debuggableAttribute != null)
            {
                _logger
                    .LogInformation($"DebuggableAttribute found");

                foreach (var constructorArgument in debuggableAttribute.ConstructorArguments ?? Enumerable.Empty<CustomAttributeArgument>())
                {
                    if (constructorArgument.Type.FullName == DebuggingModeAttributeName)
                    {
                        var debuggingMode = constructorArgument;
                        
                        _logger
                            .LogInformation("DebuggableAttribute ConstructorArgument found");
                        
                        if (debuggingMode.Value != null)
                        {
                            _logger
                                .LogInformation($"Debugging attribute value: [{debuggingMode.Value}]");

                            var flagValue = 
                                debuggingMode
                                    .Value?
                                    .ToString();

                            if (string.IsNullOrEmpty(flagValue))
                                return false;
                    
                            var attributes =
                                (DebuggableAttribute.DebuggingModes)Enum.Parse(
                                    typeof(DebuggableAttribute.DebuggingModes),
                                    flagValue
                                );
                    
                            return
                                attributes
                                    .HasFlag(DebuggableAttribute.DebuggingModes.Default);
                        }
                    }
                }
            }

            return false;
        }
    }
}
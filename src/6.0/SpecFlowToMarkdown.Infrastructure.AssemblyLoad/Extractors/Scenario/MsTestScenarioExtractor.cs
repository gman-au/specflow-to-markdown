using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SpecFlowToMarkdown.Domain.TestAssembly;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Configuration;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extensions;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Feature;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Step;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Utils;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors.Scenario
{
    public class MsTestScenarioExtractor : IScenarioExtractor
    {
        private readonly ILogger<FeatureExtractor> _logger;
        private readonly IStepExtractor _stepExtractor;
        private readonly IScenarioArgumentBuilder _scenarioArgumentBuilder;
        private readonly IBuildConfiguration _buildConfiguration;

        private readonly IEnumerable<string> _testCaseAttributes = new[]
        {
            Constants.MsTestTestPropertyAttribute
        };

        private const string ParameterDeclarationSplit = "Parameter\\:.*";

        public MsTestScenarioExtractor(
            ILogger<FeatureExtractor> logger, 
            IStepExtractor stepExtractor, 
            IScenarioArgumentBuilder scenarioArgumentBuilder, 
            IBuildConfiguration buildConfiguration
        )
        {
            _logger = logger;
            _stepExtractor = stepExtractor;
            _scenarioArgumentBuilder = scenarioArgumentBuilder;
            _buildConfiguration = buildConfiguration;
        }

        public bool IsApplicable(MethodDefinition method)
        {
            return
                method.IsVirtual ||
                method
                    .CustomAttributes
                    .Any(
                        o =>
                            o
                                .AttributeType.FullName == Constants.MsTestTestAttribute
                    );
        }

        public SpecFlowScenario Perform(MethodDefinition method, TypeDefinition type)
        {
            // Extract Scenario
            var title = string.Empty;
            var description = string.Empty;
            var tags = new List<string>();
            var scenarioCases = new List<SpecFlowCase>();

            var scenarioArgumentNames = new Dictionary<string, string>();

            foreach (var instruction in
                     method
                         .Body
                         .Instructions
                         .Where(o => o.OpCode == OpCodes.Newobj))
            {
                if (instruction.Operand is MethodReference mr)
                {
                    if (mr.DeclaringType.FullName == Constants.ScenarioInfoTypeName)
                    {
                        var currInstr =
                            instruction
                                .StepPrevious(4);

                        if (currInstr.OpCode == OpCodes.Ldstr)
                        {
                            description =
                                currInstr
                                    .Operand
                                    .ToString();
                        }

                        currInstr =
                            currInstr
                                .Previous;

                        if (currInstr.OpCode == OpCodes.Ldstr)
                        {
                            title =
                                currInstr
                                    .Operand
                                    .ToString();

                            _logger
                                .LogInformation($"Extracted scenario: [{title}]");
                        }

                        var testCaseFunctions =
                            type
                                .Methods
                                .Where(
                                    x =>
                                        x.Body
                                            .Instructions
                                            .Any(
                                                o =>
                                                {
                                                    if (o.OpCode == OpCodes.Callvirt && o.Operand is MethodReference methodReference)
                                                    {
                                                        var methodName = methodReference.Name;
                                                        return methodName == method.Name;
                                                    }

                                                    return false;
                                                }
                                            )
                                )
                                .ToList();

                        var testCases =
                            testCaseFunctions
                                .Count;

                        if (testCases > 0)
                        {
                            _logger
                                .LogInformation($"Scenario [{title}]; found {testCases} test cases");

                            scenarioArgumentNames = 
                                _scenarioArgumentBuilder
                                    .Build(currInstr);

                            currInstr =
                                currInstr
                                    .StepPrevious(15);

                            // Get test case argument values
                            foreach (var testCaseFunction in testCaseFunctions)
                            {
                                var specFlowArguments = new List<SpecFlowArgument>();
                                var caseAttributes =
                                    testCaseFunction
                                        .CustomAttributes
                                        .Where(o => _testCaseAttributes.Contains(o.AttributeType.FullName))
                                        .ToList();

                                foreach (var caseAttribute in caseAttributes)
                                {
                                    var constructorArguments =
                                        caseAttribute
                                            .ConstructorArguments;

                                    if (constructorArguments.Count == 2)
                                    {
                                        var parameterNameArg = 
                                            constructorArguments[0]
                                                .Value
                                                .ToString();
                                        
                                        var fieldMatch =
                                            new Regex(ParameterDeclarationSplit)
                                                .Matches(parameterNameArg);

                                        if (fieldMatch.Count > 0)
                                        {
                                            var argName =
                                                fieldMatch[0]
                                                    .Value
                                                    .Split(":")
                                                    [1];

                                            var argValue = 
                                                constructorArguments[1]
                                                    .Value
                                                    .ToString();
                                            
                                            specFlowArguments
                                                .Add(new SpecFlowArgument
                                                    {
                                                        ArgumentName = argName,
                                                        ArgumentValue = argValue
                                                    }
                                                );
                                        }
                                    }
                                }

                                var scenarioCase = new SpecFlowCase
                                {
                                    Arguments = specFlowArguments
                                };
                                
                                _logger
                                    .LogInformation(
                                        $"Extracted test case: {{{string.Join(", ", scenarioCase.Arguments.Select(o => $"\"{o.ArgumentName}\":\"{o.ArgumentValue}\""))}}}"
                                    );

                                scenarioCases
                                    .Add(scenarioCase);
                            }
                            
                            var startingInstruction = currInstr;
                    
                            foreach (var buildConfiguration in _buildConfiguration.Get())
                            {
                                currInstr = startingInstruction;
                        
                                currInstr =
                                    currInstr
                                        .StepPrevious(buildConfiguration.Item3);

                                if (currInstr?.OpCode == OpCodes.Ldstr)
                                    break;
                            }
                        }
                        else
                        {
                            currInstr =
                                currInstr
                                    .StepPrevious(5);
                        }

                        while (currInstr?.OpCode == OpCodes.Ldstr)
                        {
                            if (currInstr.Operand != null)
                            {
                                tags.Add(
                                    currInstr
                                        .Operand.ToString()?
                                        .Replace(
                                            ",",
                                            ""
                                        )
                                );
                            }

                            currInstr =
                                currInstr
                                    .StepPrevious(4);
                        }
                    }
                }
            }

            var scenario = new SpecFlowScenario
            {
                Title = title,
                Description = description,
                Tags = tags
            };

            // Extract Steps
            var scenarioSteps =
                _stepExtractor
                    .Perform(
                        method,
                        scenarioArgumentNames
                    );

            scenario.Steps = scenarioSteps;
            scenario.Cases = scenarioCases;

            return scenario;
        }
    }
}
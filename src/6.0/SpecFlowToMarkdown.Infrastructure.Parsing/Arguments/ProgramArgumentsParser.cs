using System;
using Microsoft.Extensions.Logging;
using SpecFlowToMarkdown.Domain;

namespace SpecFlowToMarkdown.Infrastructure.Parsing.Arguments
{
    public class ProgramArgumentsParser : IProgramArgumentsParser
    {
        private readonly ILogger<ProgramArgumentsParser> _logger;

        public ProgramArgumentsParser(ILogger<ProgramArgumentsParser> logger)
        {
            _logger = logger;
        }

        public ProgramArguments Parse(string[] args)
        {
            if (args.Length < 3)
                throw new Exception("Expected 3 arguments");

            var assemblyPath = args[0];
            var executionResultsPath = args[1];
            var outputPath = args[2];

            if (string.IsNullOrEmpty(assemblyPath))
                throw new Exception("Assembly path argument invalid");

            if (string.IsNullOrEmpty(executionResultsPath))
                throw new Exception("Results path argument invalid");

            if (string.IsNullOrEmpty(outputPath))
                throw new Exception("Output path argument invalid");

            var result = new ProgramArguments
            {
                TestAssemblyPath = args[0],
                TestResultsPath = args[1],
                OutputFilePath = args[2]
            };
            
            _logger
                .LogInformation(result.ToString());

            return result;
        }
    }
}
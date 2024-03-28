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
            if (args.Length < 5)
                throw new Exception("Expected 5 arguments");

            var testAssemblyFolder = args[0];
            var testAssemblyFile = args[1];
            var testResultsFolder = args[2];
            var testResultsFile = args[3];
            var outputPath = args[4];

            if (string.IsNullOrEmpty(testAssemblyFolder))
                throw new Exception("Assembly path argument invalid");

            if (string.IsNullOrEmpty(testAssemblyFile))
                throw new Exception("Assembly file argument invalid");

            if (string.IsNullOrEmpty(testResultsFolder))
                throw new Exception("Results path argument invalid");

            if (string.IsNullOrEmpty(testResultsFile))
                throw new Exception("Results file argument invalid");

            if (string.IsNullOrEmpty(outputPath))
                throw new Exception("Output path argument invalid");

            var result = new ProgramArguments
            {
                TestAssemblyFolder = testAssemblyFolder,
                TestAssemblyFile = testAssemblyFile,
                TestResultsFolder = testResultsFolder,
                TestResultsFile = testResultsFile,
                OutputFilePath = outputPath
            };
            
            _logger
                .LogInformation(result.ToString());

            return result;
        }
    }
}
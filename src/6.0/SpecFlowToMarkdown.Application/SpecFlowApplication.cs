using System;
using Microsoft.Extensions.Logging;
using SpecFlowToMarkdown.Infrastructure.AssemblyLoad;
using SpecFlowToMarkdown.Infrastructure.Io;
using SpecFlowToMarkdown.Infrastructure.Markdown;
using SpecFlowToMarkdown.Infrastructure.Parsing.Arguments;
using SpecFlowToMarkdown.Infrastructure.Parsing.Results;

namespace SpecFlowToMarkdown.Application
{
    public class SpecFlowApplication : ISpecFlowApplication
    {
        private readonly ILogger<SpecFlowApplication> _logger;
        private readonly IProgramArgumentsParser _programArgumentsParser;
        private readonly IAssemblyScanner _assemblyScanner;
        private readonly ITestExecutionParser _testExecutionParser;
        private readonly IMarkdownRenderer _markdownRenderer;
        private readonly IFileWriter _fileWriter;

        public SpecFlowApplication(
            ILogger<SpecFlowApplication> logger, 
            IProgramArgumentsParser programArgumentsParser, 
            IAssemblyScanner assemblyScanner,
            ITestExecutionParser testExecutionParser, 
            IMarkdownRenderer markdownRenderer, 
            IFileWriter fileWriter
        )
        {
            _logger = logger;
            _programArgumentsParser = programArgumentsParser;
            _assemblyScanner = assemblyScanner;
            _testExecutionParser = testExecutionParser;
            _markdownRenderer = markdownRenderer;
            _fileWriter = fileWriter;
        }

        public void Perform(string[] args)
        {
            try
            {
                _logger
                    .LogInformation("Starting SpecFlow Markdown generation...");

                var arguments =
                    _programArgumentsParser
                        .Parse(args);
                
                var specFlowAssembly =
                    _assemblyScanner
                        .Perform(arguments.TestAssemblyPath);

                var testResults =
                    _testExecutionParser
                        .Parse(arguments.TestResultsPath);
                
                var markdown =
                    _markdownRenderer
                        .Perform(
                            specFlowAssembly,
                            testResults
                        );
                
                _fileWriter
                    .Perform(
                        markdown,
                        arguments.OutputFilePath);

                _logger
                    .LogInformation("Completed SpecFlow Markdown generation");
            }
            catch (Exception ex)
            {
                _logger
                    .LogError($"Error encountered: {ex.Message}");
            }
        }
    }
}
using System.IO;
using Newtonsoft.Json;
using SpecFlowToMarkdown.Domain;
using SpecFlowToMarkdown.Domain.Result;
using SpecFlowToMarkdown.Infrastructure.Io;

namespace SpecFlowToMarkdown.Infrastructure.Parsing.Results
{
    public class JsonTestExecutionParser : ITestExecutionParser
    {
        private readonly IFileFinder _fileFinder;

        public JsonTestExecutionParser(IFileFinder fileFinder)
        {
            _fileFinder = fileFinder;
        }

        public TestExecution Parse(ProgramArguments arguments)
        {
            var foundFilePath =
                _fileFinder
                    .GetFirstFound(
                        arguments.TestResultsFolder,
                        arguments.TestResultsFile
                    );
            
            var jsonString =
                File
                    .ReadAllText(foundFilePath);

            var result =
                JsonConvert
                    .DeserializeObject<TestExecution>(jsonString);

            return result;
        }
    }
}
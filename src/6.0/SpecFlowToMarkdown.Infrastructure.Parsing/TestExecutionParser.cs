using System.IO;
using Newtonsoft.Json;
using SpecFlowToMarkdown.Domain.Result;

namespace SpecFlowToMarkdown.Infrastructure.Parsing
{
    public static class TestExecutionParser
    {
        public static TestExecution Perform(string executionResultsPath)
        {
            var jsonString =
                File
                    .ReadAllText(executionResultsPath);

            var result =
                JsonConvert
                    .DeserializeObject<TestExecution>(jsonString);

            return result;
        }
    }
}
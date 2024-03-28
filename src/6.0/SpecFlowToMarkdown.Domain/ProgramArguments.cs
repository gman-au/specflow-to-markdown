namespace SpecFlowToMarkdown.Domain
{
    public class ProgramArguments
    {
        public string TestAssemblyPath { get; set; }
        
        public string TestResultsPath { get; set; }
        
        public string OutputFilePath { get; set; }

        public override string ToString()
        {
            return
                $"TestAssemblyPath: '{TestAssemblyPath}'\r\n" +
                $"TestResultsPath: '{TestResultsPath}'\r\n" +
                $"OutputFilePath: '{OutputFilePath}'";
        }
    }
}
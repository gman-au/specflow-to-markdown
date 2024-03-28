namespace SpecFlowToMarkdown.Domain
{
    public class ProgramArguments
    {
        public string TestAssemblyFolder { get; set; }
        
        public string TestAssemblyFile { get; set; }
        
        public string TestResultsFolder { get; set; }
        
        public string TestResultsFile { get; set; }
        
        public string OutputFilePath { get; set; }

        public override string ToString()
        {
            return
                $"TestAssemblyFolder: '{TestAssemblyFolder}'\r\n" +
                $"TestAssemblyFile: '{TestAssemblyFile}'\r\n" +
                $"TestResultsFolder: '{TestResultsFolder}'\r\n" +
                $"TestResultsFile: '{TestResultsFile}'\r\n" +
                $"OutputFilePath: '{OutputFilePath}'";
        }
    }
}
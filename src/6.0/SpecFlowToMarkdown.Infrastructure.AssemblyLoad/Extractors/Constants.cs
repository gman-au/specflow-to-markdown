namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    internal static class Constants
    {
        public const string NUnitTestAttribute = "NUnit.Framework.TestAttribute";
        public const string XUnitTestAttribute = "Xunit.SkippableFactAttribute";
        
        public const string FeatureInfoTypeName = "TechTalk.SpecFlow.ScenarioInfo";
        
        public static readonly string[] ScenarioStepFunctions = { "And", "Given", "When", "Then" };
    }
}
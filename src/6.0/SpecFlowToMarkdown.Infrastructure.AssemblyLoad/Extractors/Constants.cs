namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    internal static class Constants
    {
        public const string NUnitTestAttribute = "NUnit.Framework.TestAttribute";
        public const string XUnitFactAttribute = "Xunit.SkippableFactAttribute";
        public const string XUnitTheoryAttribute = "Xunit.SkippableTheoryAttribute";
        
        public const string FeatureInfoTypeName = "TechTalk.SpecFlow.ScenarioInfo";
        
        public const string NUnitTestCaseAttribute = "NUnit.Framework.TestCaseAttribute";
        public const string XUnitInlineDataAttribute = "Xunit.InlineDataAttribute";
        
        public static readonly string[] ScenarioStepFunctions = { "And", "Given", "When", "Then" };
    }
}
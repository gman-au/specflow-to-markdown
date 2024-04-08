﻿namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad.Extractors
{
    internal static class Constants
    {
        public const string MsTestTestAttribute = "Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute";
        public const string NUnitTestAttribute = "NUnit.Framework.TestAttribute";
        public const string XUnitFactAttribute = "Xunit.SkippableFactAttribute";
        public const string XUnitTheoryAttribute = "Xunit.SkippableTheoryAttribute";
        
        public const string ScenarioInfoTypeName = "TechTalk.SpecFlow.ScenarioInfo";
        
        public const string NUnitTestCaseAttribute = "NUnit.Framework.TestCaseAttribute";
        public const string XUnitInlineDataAttribute = "Xunit.InlineDataAttribute";
        public const string MsTestTestPropertyAttribute = "Microsoft.VisualStudio.TestTools.UnitTesting.TestPropertyAttribute";
        
        public static readonly string[] ScenarioStepFunctions = { "And", "Given", "When", "Then" };
        
        public const string StringFormatFunctionName = "Format";
    }
}
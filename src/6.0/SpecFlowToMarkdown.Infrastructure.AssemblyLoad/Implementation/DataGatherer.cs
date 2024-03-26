using TechTalk.SpecFlow;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad
{
    public class DataGatherer : ITestRunner
    {
        public void InitializeTestRunner(int threadId)
        {
            throw new System.NotImplementedException();
        }

        public void OnTestRunStart()
        {
            throw new System.NotImplementedException();
        }

        public void OnTestRunEnd()
        {
            throw new System.NotImplementedException();
        }

        public void OnFeatureStart(FeatureInfo featureInfo)
        {
            throw new System.NotImplementedException();
        }

        public void OnFeatureEnd()
        {
            throw new System.NotImplementedException();
        }

        public void OnScenarioInitialize(ScenarioInfo scenarioInfo)
        {
            throw new System.NotImplementedException();
        }

        public void OnScenarioStart()
        {
            throw new System.NotImplementedException();
        }

        public void CollectScenarioErrors()
        {
            throw new System.NotImplementedException();
        }

        public void OnScenarioEnd()
        {
            throw new System.NotImplementedException();
        }

        public void SkipScenario()
        {
            throw new System.NotImplementedException();
        }

        public void Given(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            throw new System.NotImplementedException();
        }

        public void When(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            throw new System.NotImplementedException();
        }

        public void Then(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            throw new System.NotImplementedException();
        }

        public void And(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            throw new System.NotImplementedException();
        }

        public void But(string text, string multilineTextArg, Table tableArg, string keyword = null)
        {
            throw new System.NotImplementedException();
        }

        public void Pending()
        {
            throw new System.NotImplementedException();
        }

        public int ThreadId { get; }
        public FeatureContext FeatureContext { get; }
        public ScenarioContext ScenarioContext { get; }
    }
}
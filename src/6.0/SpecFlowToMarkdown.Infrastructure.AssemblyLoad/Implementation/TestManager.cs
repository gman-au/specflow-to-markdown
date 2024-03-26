using System.Reflection;
using TechTalk.SpecFlow;

namespace SpecFlowToMarkdown.Infrastructure.AssemblyLoad
{
    public class TestManager : ITestRunnerManager
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public ITestRunner GetTestRunner(int threadId)
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(Assembly testAssembly)
        {
            throw new System.NotImplementedException();
        }

        public void FireTestRunEnd()
        {
            throw new System.NotImplementedException();
        }

        public void FireTestRunStart()
        {
            throw new System.NotImplementedException();
        }

        public Assembly TestAssembly { get; }
        public Assembly[] BindingAssemblies { get; }
        public bool IsMultiThreaded { get; }
    }
}
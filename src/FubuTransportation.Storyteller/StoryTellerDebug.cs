using FubuCore;
using StoryTeller.Execution;
using StoryTeller.Workspace;

namespace StoryTellerTestHarness
{

    public class Debugging
    {
        public void SendAndAwait_Happy_Path()
        {
            var project = new Project
            {
                ProjectFolder = ".".ToFullPath().ParentDirectory().ParentDirectory(),
                TimeoutInSeconds = 240
            };

            using (var runner = new ProjectTestRunner(project))
            {
                runner.RunAndAssertTest("Subscriptions/Simple global subscriptions from one node to another");
            }

            
        }
    }

}
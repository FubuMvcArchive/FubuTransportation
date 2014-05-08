using FubuCore;
using StoryTeller.Execution;
using StoryTeller.ProjectUtils.Loaders;
using FileSystem = FubuCore.FileSystem;

namespace StoryTellerTestHarness
{
    public class Template
    {

        public void Simple_local_subscription()
        {
            var project = new ProjectDirectoryLoader(new FileSystem()).Load(".".ToFullPath().ParentDirectory().ParentDirectory());
            using (var runner = new ProjectTestRunner(project))
            {
                runner.RunAndAssertTest("Subscriptions/Simple local subscription");
            }
            
        }

    }
}
using FubuCore;
using FubuMVC.Core.Packaging;
using StoryTeller.Execution;
using StoryTeller.ProjectUtils.Loaders;
using StoryTeller.Workspace;
using FileSystem = FubuCore.FileSystem;

namespace StoryTellerTestHarness
{
    public class Template
    {
        private ProjectTestRunner runner;


        public void Go()
        {
            var loader = new ProjectDirectoryLoader(new FileSystem());
            IProject project = loader.Load(FubuMvcPackageFacility.GetApplicationPath());
            project.TimeoutInSeconds = 240;
            using (var runner = new ProjectTestRunner(project))
            {
                runner.RunAndAssertTest("HealthMonitoring/An inactive task should get reassigned");
            }
        }

        public void TryFiveTimes()
        {
            for (int i = 0; i < 5; i++)
            {
                Go();
            }
        }

        public void Simple_local_subscription()
        {
            var project =
                new ProjectDirectoryLoader(new FileSystem()).Load(".".ToFullPath().ParentDirectory().ParentDirectory());
            using (var runner = new ProjectTestRunner(project))
            {
                runner.RunAndAssertTest("Subscriptions/Simple local subscription");
            }
        }
    }
}
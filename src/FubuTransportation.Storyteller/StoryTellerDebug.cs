using FubuCore;
using FubuMVC.Core.Packaging;
using StoryTeller.Execution;
using StoryTeller.ProjectUtils.Loaders;
using FileSystem = FubuCore.FileSystem;

namespace StoryTellerTestHarness
{
    public class Template
    {
        private ProjectTestRunner runner;


        public void Go()
        {
            var loader = new ProjectDirectoryLoader(new FileSystem());
            var project = loader.Load(FubuMvcPackageFacility.GetApplicationPath());
            project.TimeoutInSeconds = 240;
            using (var runner = new ProjectTestRunner(project))
            {

                runner.RunAndAssertTest("HealthMonitoring/Simple assignment of dormant tasks");
            }
        }

    }
}
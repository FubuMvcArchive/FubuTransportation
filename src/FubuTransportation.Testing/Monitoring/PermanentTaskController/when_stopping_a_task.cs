using System;
using System.Linq;
using System.Threading.Tasks;
using FubuTestingSupport;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Monitoring;
using NUnit.Framework;

namespace FubuTransportation.Testing.Monitoring.PermanentTaskController
{
    [TestFixture]
    public class when_trying_to_stop_a_task_that_does_not_exist : PersistentTaskControllerContext
    {
        [Test]
        public void returns_a_faulted_task()
        {
            Exception<AggregateException>.ShouldBeThrownBy(() => {
                theController.Deactivate("nonexistent://1".ToUri()).Wait();
            }).InnerException.ShouldBeOfType<ArgumentOutOfRangeException>()
            .Message.ShouldContain("Task 'nonexistent://1/' is not recognized by this node");
        }
    }

    [TestFixture]
    public class when_stopping_a_task_successfully : PersistentTaskControllerContext
    {
        private Task theTask;

        protected override void theContextIs()
        {
            Task("running://1").IsFullyFunctionalAndActive();

            theTask = theController.Deactivate("running://1".ToUri());
            theTask.Wait();
        }

        [Test]
        public void should_stop_the_task()
        {
            Task("running://1").IsActive.ShouldBeFalse();
        }

        [Test]
        public void should_log_the_successful_stop()
        {
            LoggedMessageForSubject<StoppedTask>("running://1");
        }


        [Test]
        public void the_ownership_was_removed_and_persisted()
        {
            theCurrentNode.OwnedTasks.ShouldNotContain("running://1".ToUri());
            
        }
    }

    [TestFixture]
    public class when_stopping_a_task_unsuccessfully : PersistentTaskControllerContext
    {
        private Task theTask;

        protected override void theContextIs()
        {
            Task("running://1").IsFullyFunctionalAndActive();
            Task("running://1").DeactivateException = new DivideByZeroException();

            theTask = theController.Deactivate("running://1".ToUri());
            theTask.Wait();
        }

        [Test]
        public void logged_the_failure_message()
        {
            LoggedMessageForSubject<FailedToStopTask>("running://1");
        }

        [Test]
        public void logged_the_exception()
        {
            theLogger.ErrorMessages.OfType<ErrorReport>()
                .Any(x => x.ExceptionText.Contains("DivideByZeroException"));
        }
    }
}
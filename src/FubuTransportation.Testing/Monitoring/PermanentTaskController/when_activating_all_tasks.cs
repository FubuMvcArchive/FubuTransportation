using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FubuTestingSupport;
using FubuTransportation.Monitoring;
using NUnit.Framework;

namespace FubuTransportation.Testing.Monitoring.PermanentTaskController
{

    [TestFixture]
    public class when_activating_all_tasks_some_fail : PersistentTaskControllerContext
    {
        protected override void theContextIs()
        {
            Task("good://1").IsFullyFunctional();
            Task("good://2").IsFullyFunctional();
        
            Task("bad://1").ActivationException = new DivideByZeroException();
            Task("bad://2").ActivationException = new ExternalException();

            theController.ActivateAllTasks();
        }

        [Test]
        public void should_activate_the_tasks_that_it_can()
        {
            AssertTasksAreActive("good://1", "good://2");
        }

        [Test]
        public void the_current_node_should_own_the_tasks_that_could_be_activated()
        {
            TheOwnedTasksByTheCurrentNodeShouldBe("good://1", "good://2");
        }

        [Test]
        public void should_log_an_activated_slash_ownership_message_for_each_successful_job()
        {
            LoggedMessageForSubject<TookOwnershipOfPersistentTask>("good://1");
            LoggedMessageForSubject<TookOwnershipOfPersistentTask>("good://2");
        }

        [Test]
        public void should_log_exceptions_for_failed_activations()
        {
            ExceptionWasLogged(Task("bad://1").ActivationException);
            ExceptionWasLogged(Task("bad://2").ActivationException);
        }

        [Test]
        public void logged_activation_failures()
        {
            LoggedMessageForSubject<FailedToActivatePersistentTask>("bad://1");
            LoggedMessageForSubject<FailedToActivatePersistentTask>("bad://2");
        }
    }

    [TestFixture]
    public class when_activating_all_tasks_happy_path : PersistentTaskControllerContext
    {
        private readonly string[] taskIds = new[] {"foo://a", "foo://b", "bar://c", "bar://d", "baz://e"};

        protected override void theContextIs()
        {
            taskIds.Each(x => Task(x).IsFullyFunctional());
        
            theController.ActivateAllTasks();
        }

        [Test]
        public void should_activate_all_the_tasks()
        {
            AssertTasksAreActive(taskIds);
        }

        [Test]
        public void the_current_node_should_own_all_the_tasks()
        {
            TheOwnedTasksByTheCurrentNodeShouldBe(taskIds);
        }

        [Test]
        public void should_log_an_activated_slash_ownership_message_for_each()
        {
            taskIds.Each(x => {
                theLogger.InfoMessages.ShouldContain(new TookOwnershipOfPersistentTask(x.ToUri()));
            });
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Logging;
using FubuTestingSupport;
using FubuTransportation.Monitoring;
using FubuTransportation.Subscriptions;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Monitoring
{

    [TestFixture]
    public class when_unsuccessfully_taking_ownership_because_of_exception : TransportPeerContext
    {
        private readonly Uri theSubject = "subject://1".ToUri();
        private OwnershipStatus theStatus;
        private const string theOriginalOwner = "elsewhere";

        protected override void theContextIs()
        {
            var request = new TakeOwnershipRequest
            {
                Subject = theSubject
            };

            theServiceBus.ExpectMessage(request)
            .AtDestination(theNode.Addresses.FirstOrDefault())
            .Returns(new TakeOwnershipResponse
            {
                Status = OwnershipStatus.Exception
            });

            var task = thePeer.TakeOwnership(theSubject);
            task.Wait(1.Minutes()); // for debugging
            theStatus = task.Result;
        }

        [Test]
        public void does_not_persist_the_new_ownership()
        {
            AssertThatTheNodeWas_Not_Persisted();
            theNode.OwnedTasks.ShouldNotContain(theSubject);
        }

        [Test]
        public void the_status_should_be_returned()
        {
            theStatus.ShouldEqual(OwnershipStatus.Exception);
        }
    }

    [TestFixture]
    public class when_unsuccessfully_taking_ownership_because_the_remote_node_fails : TransportPeerContext
    {
        private readonly Uri theSubject = "subject://1".ToUri();
        private OwnershipStatus theStatus;
        private const string theOriginalOwner = "elsewhere";

        protected override void theContextIs()
        {
            var request = new TakeOwnershipRequest
            {
                Subject = theSubject
            };

            theServiceBus.ExpectMessage(request)
            .AtDestination(theNode.Addresses.FirstOrDefault())
            .Throws(new DivideByZeroException());

            var task = thePeer.TakeOwnership(theSubject);
            task.Wait(1.Minutes()); // for debugging
            theStatus = task.Result;
        }

        [Test]
        public void does_not_persist_the_new_ownership()
        {
            AssertThatTheNodeWas_Not_Persisted();
            theNode.OwnedTasks.ShouldNotContain(theSubject);
        }

        [Test]
        public void the_status_should_be_returned()
        {
            theStatus.ShouldEqual(OwnershipStatus.Exception);
        }
    }

    [TestFixture]
    public class when_unsuccessfully_taking_ownership_because_of_the_peer_not_recognizing_the_job : TransportPeerContext
    {
        private readonly Uri theSubject = "subject://1".ToUri();
        private OwnershipStatus theStatus;
        private string theOriginalOwner = "elsewhere";

        protected override void theContextIs()
        {
            var request = new TakeOwnershipRequest
            {
                Subject = theSubject
            };

            theServiceBus.ExpectMessage(request)
            .AtDestination(theNode.Addresses.FirstOrDefault())
            .Returns(new TakeOwnershipResponse
            {
                Status = OwnershipStatus.UnknownSubject
            });

            var task = thePeer.TakeOwnership(theSubject);
            task.Wait(1.Minutes()); // for debugging
            theStatus = task.Result;
        }

        [Test]
        public void does_not_persist_the_new_ownership()
        {
            AssertThatTheNodeWas_Not_Persisted();
            theNode.OwnedTasks.ShouldNotContain(theSubject);
        }

        [Test]
        public void the_status_should_be_returned()
        {
            theStatus.ShouldEqual(OwnershipStatus.UnknownSubject);
        }
    }

    [TestFixture]
    public class when_successfully_taking_ownership_of_a_single_responsibility : TransportPeerContext
    {
        private readonly Uri theSubject = "subject://1".ToUri();
        private OwnershipStatus theStatus;

        protected override void theContextIs()
        {
            var request = new TakeOwnershipRequest
            {
                Subject = theSubject
            };

            theServiceBus.ExpectMessage(request)
            .AtDestination(theNode.Addresses.FirstOrDefault())
            .Returns(new TakeOwnershipResponse
            {
                Status = OwnershipStatus.OwnershipActivated
            });

            var task = thePeer.TakeOwnership(theSubject);
            task.Wait(1.Minutes()); // for debugging
            theStatus = task.Result;
        }

        [Test]
        public void the_status_should_be_returned()
        {
            theStatus.ShouldEqual(OwnershipStatus.OwnershipActivated);
        }
    }

    [TestFixture]
    public class when_successfully_taking_ownership_and_it_was_already_activated : TransportPeerContext
    {
        private readonly Uri theSubject = "subject://1".ToUri();
        private OwnershipStatus theStatus;

        protected override void theContextIs()
        {
            var request = new TakeOwnershipRequest
            {
                Subject = theSubject
            };

            theServiceBus.ExpectMessage(request)
            .AtDestination(theNode.Addresses.FirstOrDefault())
            .Returns(new TakeOwnershipResponse
            {
                Status = OwnershipStatus.AlreadyOwned
            });

            var task = thePeer.TakeOwnership(theSubject);
            task.Wait(1.Minutes()); // for debugging
            theStatus = task.Result;
        }


        [Test]
        public void the_status_should_be_returned()
        {
            theStatus.ShouldEqual(OwnershipStatus.AlreadyOwned);
        }
    }

    [TestFixture]
    public class when_deactivating_successfully : TransportPeerContext
    {
        private readonly Uri theSubject = "foo://1".ToUri();

        protected override void theContextIs()
        {
            theNode.AddOwnership(theSubject);

            theServiceBus.ExpectMessage(new TaskDeactivation(theSubject))
                .AtDestination(theNode.Addresses[0])
                .Returns(new TaskDeactivationResponse
                {
                    Subject = theSubject,
                    Success = true
                });


            thePeer.Deactivate(theSubject).Wait();
        }

        [Test]
        public void should_send_the_deactivation_message_directly_to_the_correct_endpoint()
        {
           theServiceBus.AssertThatAllExpectedMessagesWereReceived();
        }

    }

    [TestFixture]
    public class when_deactivating_unsuccessfully : TransportPeerContext
    {
        private readonly Uri theSubject = "foo://1".ToUri();

        protected override void theContextIs()
        {
            theNode.AddOwnership(theSubject);

            theServiceBus.ExpectMessage(new TaskDeactivation(theSubject))
                .AtDestination(theNode.Addresses[0])
                .Throws(new DivideByZeroException());


            thePeer.Deactivate(theSubject).Wait();
        }

        [Test]
        public void should_send_the_deactivation_message_directly_to_the_correct_endpoint()
        {
            theServiceBus.AssertThatAllExpectedMessagesWereReceived();
        }

        [Test]
        public void should_still_remove_the_ownership_from_the_node_and_persist()
        {
            theSubscriptions.AssertWasCalled(x => x.Persist(theNode));
        }

        [Test]
        public void should_still_have_removed_the_ownership_from_the_node()
        {
            theNode.OwnedTasks.ShouldNotContain(theSubject);
        }
    }

    [TestFixture]
    public class when_requesting_the_task_health_full_happy_path : TransportPeerContext
    {
        private readonly Uri subject1 = "foo://1".ToUri();
        private readonly Uri subject2 = "foo://2".ToUri();
        private readonly Uri subject3 = "foo://3".ToUri();
        private readonly Uri subject4 = "foo://4".ToUri();
        private TaskHealthResponse theResponse;
        private Task<TaskHealthResponse> theReturnedTask;

        protected override void theContextIs()
        {
            var subjects = new Uri[] {subject1, subject2, subject3, subject4};

            theNode.OwnedTasks = subjects;

            theResponse = new TaskHealthResponse
            {
                Tasks = subjects.Select(x => new PersistentTaskStatus(x, HealthStatus.Active)).ToArray()
            };


            theServiceBus.ExpectMessage(new TaskHealthRequest{Subjects = subjects})
                .AtDestination(theNode.Addresses.First())
                .Returns(theResponse);

            theReturnedTask = thePeer.CheckStatusOfOwnedTasks();
            theReturnedTask.Wait();

            
        }

        [Test]
        public void should_just_return_the_response()
        {
            theReturnedTask.Result.ShouldBeTheSameAs(theResponse);
        }
    }

    [TestFixture]
    public class when_requesting_the_task_health_has_to_fill_in_missing_status_from_peer : TransportPeerContext
    {
        private readonly Uri subject1 = "foo://1".ToUri();
        private readonly Uri subject2 = "foo://2".ToUri();
        private readonly Uri subject3 = "foo://3".ToUri();
        private readonly Uri subject4 = "foo://4".ToUri();
        private TaskHealthResponse theResponse;
        private Task<TaskHealthResponse> theReturnedTask;

        protected override void theContextIs()
        {
            var subjects = new Uri[] { subject1, subject2, subject3, subject4 };

            theNode.OwnedTasks = subjects;

            theResponse = new TaskHealthResponse
            {
                Tasks = new PersistentTaskStatus[0]
            };


            theServiceBus.ExpectMessage(new TaskHealthRequest { Subjects = subjects })
                .AtDestination(theNode.Addresses.First())
                .Returns(theResponse);

            theReturnedTask = thePeer.CheckStatusOfOwnedTasks();
            theReturnedTask.Wait();


        }

        [Test]
        public void the_response_should_be_filled_with_inactive_for_the_missing_tasks()
        {
            var response = theReturnedTask.Result;

            response.Tasks.ShouldContain(new PersistentTaskStatus(subject1, HealthStatus.Inactive));
            response.Tasks.ShouldContain(new PersistentTaskStatus(subject2, HealthStatus.Inactive));
            response.Tasks.ShouldContain(new PersistentTaskStatus(subject3, HealthStatus.Inactive));
            response.Tasks.ShouldContain(new PersistentTaskStatus(subject4, HealthStatus.Inactive));
        }
    }

    [TestFixture]
    public class when_requesting_the_task_health_and_the_remote_message_fails : TransportPeerContext
    {
        private readonly Uri subject1 = "foo://1".ToUri();
        private readonly Uri subject2 = "foo://2".ToUri();
        private readonly Uri subject3 = "foo://3".ToUri();
        private readonly Uri subject4 = "foo://4".ToUri();
        private TaskHealthResponse theResponse;
        private Task<TaskHealthResponse> theReturnedTask;

        protected override void theContextIs()
        {
            var subjects = new Uri[] { subject1, subject2, subject3, subject4 };

            theNode.OwnedTasks = subjects;

            theResponse = new TaskHealthResponse
            {
                Tasks = new PersistentTaskStatus[0]
            };


            theServiceBus.ExpectMessage(new TaskHealthRequest { Subjects = subjects })
                .AtDestination(theNode.Addresses.First())
                .Returns(theResponse);

            theReturnedTask = thePeer.CheckStatusOfOwnedTasks();
            theReturnedTask.Wait();


        }

        [Test]
        public void the_response_should_be_filled_with_inactive_for_the_missing_tasks()
        {
            var response = theReturnedTask.Result;

            response.Tasks.ShouldContain(new PersistentTaskStatus(subject1, HealthStatus.Inactive));
            response.Tasks.ShouldContain(new PersistentTaskStatus(subject2, HealthStatus.Inactive));
            response.Tasks.ShouldContain(new PersistentTaskStatus(subject3, HealthStatus.Inactive));
            response.Tasks.ShouldContain(new PersistentTaskStatus(subject4, HealthStatus.Inactive));
        }
    }

    [TestFixture]
    public abstract class TransportPeerContext
    {
        protected readonly TransportNode theNode = new TransportNode { Id = "node1", NodeName = "foo", Addresses = new []{"reply://1".ToUri()}};
        protected RiggedServiceBus theServiceBus;
        protected ISubscriptionRepository theSubscriptions;
        protected TransportPeer thePeer;
        private RecordingLogger theLogger;

        [SetUp]
        public void SetUp()
        {
            theServiceBus = new RiggedServiceBus();
            theSubscriptions = MockRepository.GenerateMock<ISubscriptionRepository>();
            theLogger = new RecordingLogger();
            thePeer = new TransportPeer(theNode, theSubscriptions, theServiceBus, theLogger);

            theSubscriptions.Stub(x => x.FindPeer(theNode.Id))
                .Return(theNode);

            theContextIs();
        }

        protected void AssertThatTheNodeWasPersisted()
        {
            theSubscriptions.AssertWasCalled(x => x.Persist(theNode));
        }

        protected void AssertThatTheNodeWas_Not_Persisted()
        {
            theSubscriptions.AssertWasNotCalled(x => x.Persist(theNode));
        }

        protected virtual void theContextIs()
        {
            
        }
    }
}
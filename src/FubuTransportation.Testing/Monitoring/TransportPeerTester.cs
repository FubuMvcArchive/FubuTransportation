using System;
using System.Linq;
using System.Threading.Tasks;
using FubuCore;
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
        public void should_persist_the_ownership()
        {
            AssertThatTheNodeWasPersisted();
            theNode.OwnedTasks.ShouldContain(theSubject);
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
        public void should_persist_the_ownership()
        {
            AssertThatTheNodeWasPersisted();
            theNode.OwnedTasks.ShouldContain(theSubject);
        }

        [Test]
        public void the_status_should_be_returned()
        {
            theStatus.ShouldEqual(OwnershipStatus.AlreadyOwned);
        }
    }

    [TestFixture]
    public abstract class TransportPeerContext
    {
        protected readonly TransportNode theNode = new TransportNode { Id = "node1", NodeName = "foo", Addresses = new []{"reply://1".ToUri()}};
        protected RiggedServiceBus theServiceBus;
        protected ISubscriptionRepository theSubscriptions;
        protected TransportPeer thePeer;

        [SetUp]
        public void SetUp()
        {
            theServiceBus = new RiggedServiceBus();
            theSubscriptions = MockRepository.GenerateMock<ISubscriptionRepository>();

            thePeer = new TransportPeer(theNode, theSubscriptions, theServiceBus);

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
using System;
using System.Linq;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Subscriptions;
using NUnit.Framework;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class SubscriptionRepositoryTester
    {
        private InMemorySubscriptionPersistence persistence;
        private SubscriptionRepository theRepository;
        private string TheNodeName = "TheNode";
        private ChannelGraph channelGraph;

        [SetUp]
        public void SetUp()
        {
            persistence = new InMemorySubscriptionPersistence();
            channelGraph = new ChannelGraph{Name = TheNodeName};
            channelGraph.AddReplyChannel("foo", "foo://replies".ToUri());
            channelGraph.AddReplyChannel("bar", "bar://replies".ToUri());

            theRepository = new SubscriptionRepository(channelGraph, persistence);
        }

        [Test]
        public void save_the_first_subscriptions()
        {
            var subscription = ObjectMother.NewSubscription();

            theRepository.PersistSubscriptions(subscription);

            var requirements = theRepository.LoadSubscriptions(SubscriptionRole.Subscribes);
            requirements
                .ShouldHaveTheSameElementsAs(subscription);

            requirements.Single().Id.ShouldNotEqual(Guid.Empty);
        }

        [Test]
        public void save_a_new_subscription_that_does_not_match_existing()
        {
            var existing = ObjectMother.ExistingSubscription();
            existing.NodeName = TheNodeName;

            persistence.Persist(existing);

            var subscription = ObjectMother.NewSubscription();
            subscription.NodeName = TheNodeName;

            theRepository.PersistSubscriptions(subscription);
            var requirements = theRepository.LoadSubscriptions(SubscriptionRole.Subscribes);
            requirements.Count().ShouldEqual(2);
            requirements.ShouldContain(existing);
            requirements.ShouldContain(subscription);
        }

        [Test]
        public void save_a_new_subscription_with_a_mix_of_existing_subscriptions()
        {
            var existing = ObjectMother.ExistingSubscription();
            existing.NodeName = TheNodeName;

            persistence.Persist(existing);
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));

            var subscription = ObjectMother.NewSubscription();
            subscription.NodeName = TheNodeName;

            theRepository.PersistSubscriptions(subscription);

            var requirements = theRepository.LoadSubscriptions(SubscriptionRole.Subscribes);
            requirements.Count().ShouldEqual(2);
            requirements.ShouldContain(existing);
            requirements.ShouldContain(subscription);
        }

        [Test]
        public void save_a_subscription_that_already_exists()
        {
            var existing = ObjectMother.ExistingSubscription();
            existing.NodeName = TheNodeName;

            var subscription = existing.Clone();

            theRepository.PersistSubscriptions(subscription);

            theRepository.LoadSubscriptions(SubscriptionRole.Subscribes)
                .Single()
                .ShouldEqual(existing);
        }

        [Test]
        public void save_a_mixed_bag_of_existing_and_new_subscriptions()
        {
            var existing = ObjectMother.ExistingSubscription(TheNodeName);

            var anotherExisting = ObjectMother.ExistingSubscription(TheNodeName);

            persistence.Persist(anotherExisting);
            persistence.Persist(existing);
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));

            var old = existing.Clone();
            var newSubscription = ObjectMother.NewSubscription(TheNodeName);

            theRepository.PersistSubscriptions(old, newSubscription);
            var requirements = theRepository.LoadSubscriptions(SubscriptionRole.Subscribes);

            requirements.Count().ShouldEqual(3);
            requirements.ShouldContain(existing);
            requirements.ShouldContain(newSubscription);
            requirements.ShouldContain(anotherExisting);


        }

        [Test]
        public void save_transport_node_for_the_first_time()
        {
            theRepository.SaveTransportNode();

            var node = persistence.NodesForGroup(channelGraph.Name)
                .Single();

            node.ShouldEqual(new TransportNode(channelGraph));
            node.Id.ShouldNotEqual(Guid.Empty);
        }

        [Test]
        public void saving_the_transport_node_is_idempotent()
        {
            theRepository.SaveTransportNode();

            var id = persistence.NodesForGroup(channelGraph.Name)
                .Single().Id;

            theRepository.SaveTransportNode();
            theRepository.SaveTransportNode();
            theRepository.SaveTransportNode();
            theRepository.SaveTransportNode();
            theRepository.SaveTransportNode();
            theRepository.SaveTransportNode();

            persistence.NodesForGroup(channelGraph.Name)
                .Single().Id.ShouldEqual(id);
        }
    }
}
using System;
using System.Linq;
using FubuTestingSupport;
using FubuTransportation.Subscriptions;
using NUnit.Framework;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class SubscriptionRepositoryTester
    {
        private InMemorySubscriptionPersistence persistence;
        private SubscriptionRepository theRepository;

        [SetUp]
        public void SetUp()
        {
            persistence = new InMemorySubscriptionPersistence();
            theRepository = new SubscriptionRepository(persistence);
        }

        [Test]
        public void save_the_first_subscriptions()
        {
            var subscription = ObjectMother.NewSubscription();

            var requirements = theRepository.PersistRequirements("Foo", subscription);
            requirements
                .ShouldHaveTheSameElementsAs(subscription);

            requirements.Single().Id.ShouldNotEqual(Guid.Empty);
        }

        [Test]
        public void save_a_new_subscription_that_does_not_match_existing()
        {
            var existing = ObjectMother.ExistingSubscription();
            existing.NodeName = "TheNode";

            persistence.Persist(existing);

            var subscription = ObjectMother.NewSubscription();
            subscription.NodeName = "TheNode";

            var requirements = theRepository.PersistRequirements("TheNode", subscription);
            requirements.Count().ShouldEqual(2);
            requirements.ShouldContain(existing);
            requirements.ShouldContain(subscription);
        }

        [Test]
        public void save_a_new_subscription_with_a_mix_of_existing_subscriptions()
        {
            var existing = ObjectMother.ExistingSubscription();
            existing.NodeName = "TheNode";

            persistence.Persist(existing);
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));

            var subscription = ObjectMother.NewSubscription();
            subscription.NodeName = "TheNode";

            var requirements = theRepository.PersistRequirements("TheNode", subscription);
            requirements.Count().ShouldEqual(2);
            requirements.ShouldContain(existing);
            requirements.ShouldContain(subscription);
        }

        [Test]
        public void save_a_subscription_that_already_exists()
        {
            var existing = ObjectMother.ExistingSubscription();
            existing.NodeName = "TheNode";

            var subscription = existing.Clone();

            theRepository.PersistRequirements("TheNode", subscription)
                .Single()
                .ShouldEqual(existing);
        }

        [Test]
        public void save_a_mixed_bag_of_existing_and_new_subscriptions()
        {
            var existing = ObjectMother.ExistingSubscription("TheNode");

            var anotherExisting = ObjectMother.ExistingSubscription("TheNode");

            persistence.Persist(anotherExisting);
            persistence.Persist(existing);
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));
            persistence.Persist(ObjectMother.ExistingSubscription("Different"));

            var old = existing.Clone();
            var newSubscription = ObjectMother.NewSubscription("TheNode");

            var requirements = theRepository.PersistRequirements("TheNode", old, newSubscription);

            requirements.Count().ShouldEqual(3);
            requirements.ShouldContain(existing);
            requirements.ShouldContain(newSubscription);
            requirements.ShouldContain(anotherExisting);


        }
    }
}
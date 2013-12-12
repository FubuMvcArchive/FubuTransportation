using System.Collections.Generic;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.InMemory;
using FubuTransportation.Runtime;
using FubuTransportation.Subscriptions;
using NUnit.Framework;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class SubscriptionCache_replace_subscriptions_tester
    {
        [Test]
        public void can_replace_the_subscriptions()
        {
            var cache = new SubscriptionCache(new ChannelGraph(), new ITransport[]{new InMemoryTransport(), });

            var subscriptions1 = new Subscription[]
            {
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription()
            };

            var subscriptions2 = new Subscription[]
            {
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription()
            };

            cache.LoadSubscriptions(subscriptions1);

            cache.ActiveSubscriptions.ShouldHaveTheSameElementsAs(subscriptions1);

            cache.LoadSubscriptions(subscriptions2);

            cache.ActiveSubscriptions.ShouldHaveTheSameElementsAs(subscriptions2);
        }
    }
}
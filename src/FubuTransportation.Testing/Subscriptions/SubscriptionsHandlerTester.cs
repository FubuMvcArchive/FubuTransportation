using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Subscriptions;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Subscriptions
{
    [TestFixture]
    public class when_handling_subscriptions_changed : InteractionContext<SubscriptionsHandler>
    {
        private Subscription[] theSubscriptions;
        private ChannelGraph theGraph;

        protected override void beforeEach()
        {
            theSubscriptions = new Subscription[]
            {
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription(),
                ObjectMother.ExistingSubscription()
            };

            theGraph = new ChannelGraph {Name = "TheNode"};
            Services.Inject(theGraph);

            MockFor<ISubscriptionRepository>().Stub(x => x.LoadSubscriptions())
                .Return(theSubscriptions);

            ClassUnderTest.Handle(new SubscriptionsChanged());
        }

        [Test]
        public void should_load_the_new_subscriptions_into_the_running_cache()
        {
            MockFor<ISubscriptionCache>()
                .AssertWasCalled(x => x.LoadSubscriptions(theSubscriptions));
        }
    }
}
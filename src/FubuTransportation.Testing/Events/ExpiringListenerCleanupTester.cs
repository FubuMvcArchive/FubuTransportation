using System;
using FubuTestingSupport;
using FubuTransportation.Events;
using NUnit.Framework;
using NUnit.Mocks;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Events
{
    [TestFixture]
    public class ExpiringListenerCleanupTester : InteractionContext<ExpiringListenerCleanup>
    {
        [Test]
        public void execute_prunes_for_the_current_time()
        {
            LocalSystemTime = DateTime.Today.AddHours(8);

            ClassUnderTest.Execute();

            MockFor<IEventAggregator>().AssertWasCalled(x => x.PruneExpiredListeners(UtcSystemTime));
        }
    }
}
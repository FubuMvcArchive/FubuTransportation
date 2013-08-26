using System;
using System.Linq.Expressions;
using System.Security.Cryptography;
using FubuCore.Descriptions;
using FubuTestingSupport;
using FubuTransportation.Polling;
using NUnit.Framework;
using Rhino.Mocks;
using Is = Rhino.Mocks.Constraints.Is;

namespace FubuTransportation.Testing.Polling
{
    [TestFixture]
    public class PollingJobTester : InteractionContext<PollingJob<APollingJob, PollingJobSettings>>
    {
        protected override void beforeEach()
        {
            Expression<Func<PollingJobSettings, double>> intervalSource = x => 350;
            Services.Inject(intervalSource);
        }

        [Test]
        public void run_now_successfully()
        {
            ClassUnderTest.RunNow();

            MockFor<IServiceBus>().AssertWasCalled(x => x.Consume(new JobRequest<APollingJob>()), x => x.IgnoreArguments());
        }

        [Test]
        public void run_now_with_a_failure()
        {
            var ex = new NotImplementedException();
            MockFor<IServiceBus>().Expect(x => x.Consume(new JobRequest<APollingJob>()))
                                  .IgnoreArguments()
                                  .Throw(ex);

            ClassUnderTest.RunNow();

            MockFor<IPollingJobLogger>().AssertWasCalled(x => x.FailedToSchedule(typeof(APollingJob), ex));
        }

        [Test]
        public void smoke_test_describe()
        {
            var description = Description.For(ClassUnderTest);
            description.Properties["Interval"].ShouldEqual("350 ms");
        }
    }

    public class PollingJobSettings
    {
        public PollingJobSettings()
        {
            Polling = 1000;
        }

        public double Polling { get; set; }
    }
}
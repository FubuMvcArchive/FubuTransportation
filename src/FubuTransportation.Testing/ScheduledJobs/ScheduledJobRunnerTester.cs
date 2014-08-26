using System.Threading.Tasks;
using FubuTestingSupport;
using FubuTransportation.Runtime;
using FubuTransportation.ScheduledJobs;
using FubuTransportation.ScheduledJobs.Execution;
using FubuTransportation.ScheduledJobs.Persistence;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.ScheduledJobs
{
    [TestFixture]
    public class ScheduledJobRunnerTester : InteractionContext<ScheduledJobRunner<AJob>>
    {
        private Envelope theEnvelope;
        private IJobRunTracker theTracker;
        private Task<RescheduleRequest<AJob>> theTask;

        protected override void beforeEach()
        {
            theEnvelope = new Envelope
            {
                Attempts = 2
            };

            Services.Inject(theEnvelope);

            var theJob = MockFor<AJob>();
            theTracker = MockFor<IJobRunTracker>();
            MockFor<IScheduleStatusMonitor>().Stub(x => x.TrackJob(theEnvelope.Attempts, theJob))
                .Return(theTracker);

            theTask = Task.FromResult(new RescheduleRequest<AJob>());

            MockFor<IScheduledJob<AJob>>().Stub(x => x.ToTask(theJob, theTracker))
                .Return(theTask);
        }

        [Test]
        public void coordinates_with_the_monitor_and_scheduled_job()
        {
            ClassUnderTest.Execute(new ExecuteScheduledJob<AJob>())
                .ShouldBeTheSameAs(theTask);
        }
    }
}
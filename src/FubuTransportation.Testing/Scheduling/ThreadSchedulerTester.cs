using System.Linq;
using FubuTestingSupport;
using FubuTransportation.Scheduling;
using NUnit.Framework;

namespace FubuTransportation.Testing.Scheduling
{
    [TestFixture]
    public class ThreadSchedulerTester
    {
        [Test]
        public void can_schedule_work()
        {
            var ran = false;
            using(var scheduler = ThreadScheduler.Default())
            {
                scheduler.Start(() => ran = true, false);
                Wait.Until(() => ran = true).ShouldBeTrue();
            }
        }

        [Test]
        public void can_use_multiple_threads()
        {
            using (var scheduler = new ThreadScheduler(5))
            {
                scheduler.Start(() => { }, false);
                scheduler.Threads.ShouldHaveCount(5);
            }
        }

        [Test]
        public void unstarted_threads_should_be_empty()
        {
            using (var scheduler = new ThreadScheduler(5))
            {
                scheduler.Threads.ShouldHaveCount(0);
            }
        }

        [Test]
        public void loops_when_told_to()
        {
            using (var scheduler = new ThreadScheduler(1))
            {
                var count = 0;
                scheduler.Start(() => count++, true);
                Wait.Until(() => count > 1).ShouldBeTrue();
            }
        }

        [Test]
        public void doesnt_loop_when_told_not_to()
        {
            using (var scheduler = new ThreadScheduler(1))
            {
                var count = 0;
                scheduler.Start(() => count++, false);
                Wait.Until(() => count > 1, timeoutInMilliseconds: 1000).ShouldBeFalse();
            }
        }
    }
}
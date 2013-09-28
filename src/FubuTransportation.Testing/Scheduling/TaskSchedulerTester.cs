using FubuTestingSupport;
using FubuTransportation.Scheduling;
using NUnit.Framework;

namespace FubuTransportation.Testing.Scheduling
{
    public class TaskSchedulerTester
    {
        [Test]
        public void can_schedule_work()
        {
            var ran = false;
            using (var scheduler = TaskScheduler.Default())
            {
                scheduler.Start(() => ran = true, false);
                Wait.Until(() => ran = true).ShouldBeTrue();
            }
        }

        [Test]
        public void can_use_multiple_tasks()
        {
            using (var scheduler = new TaskScheduler(5))
            {
                scheduler.Start(() => { }, false);
                scheduler.Tasks.ShouldHaveCount(5);
            }
        }

        [Test]
        public void unstarted_tasks_should_be_empty()
        {
            using (var scheduler = new TaskScheduler(5))
            {
                scheduler.Tasks.ShouldHaveCount(0);
            }
        }

        [Test]
        public void loops_when_told_to()
        {
            using (var scheduler = new TaskScheduler(1))
            {
                var count = 0;
                scheduler.Start(() => count++, true);
                Wait.Until(() => count > 1).ShouldBeTrue();
            }
        }

        [Test]
        public void doesnt_loop_when_told_not_to()
        {
            using (var scheduler = new TaskScheduler(1))
            {
                var count = 0;
                scheduler.Start(() => count++, false);
                Wait.Until(() => count > 1, timeoutInMilliseconds:1000).ShouldBeFalse();
            }
        }
    }
}
using System.Threading;
using FubuTransportation.Polling;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Polling
{
    [TestFixture]
    public class DefaultTimerTester
    {
        [Test]
        public void start_and_callback()
        {
            var reset = new ManualResetEvent(false);

            var timer = new DefaultTimer();

            int i = 0;

            timer.Start(() => {
                i++;
                reset.Set();
                timer.Stop();
            }, 1000);

            reset.WaitOne();

            i.ShouldEqual(1);
        }

        [Test]
        public void polls()
        {
            var reset = new ManualResetEvent(false);

            var timer = new DefaultTimer();

            int i = 0;

            timer.Start(() =>
            {
                i++;
                if (i == 5)
                {
                    reset.Set();
                    timer.Stop();
                }
            }, 100);

            reset.WaitOne(30000);

            i.ShouldEqual(5);
        }
    }
}
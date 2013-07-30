using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FubuCore.Util;
using FubuTransportation.Runtime;

namespace FubuTransportation.RhinoQueues.Testing
{
    public class StubReceiver : IReceiver
    {
        public IList<Envelope> Received = new List<Envelope>(); 

        public void Receive(IChannel channel, Envelope envelope)
        {
            Received.Add(envelope);

            envelope.Callback.MarkSuccessful();
        }
    }

    public static class Wait
    {
        public static void Until(Func<bool> condition, int millisecondPolling = 500, int timeoutInMilliseconds = 5000)
        {
            if (condition()) return;

            var clock = new Stopwatch();
            clock.Start();

            while (clock.ElapsedMilliseconds < timeoutInMilliseconds)
            {
                Debug.WriteLine(clock.ElapsedMilliseconds);

                Thread.Yield();
                Thread.Sleep(500);

                if (condition()) return;
            }

            Debug.WriteLine("Done waiting, I'm out of here");
        }
    }
}
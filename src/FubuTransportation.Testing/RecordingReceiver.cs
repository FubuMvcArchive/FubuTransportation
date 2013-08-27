﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;

namespace FubuTransportation.Testing
{
    public class RecordingReceiver : IReceiver
    {
        public IList<Envelope> Received = new List<Envelope>(); 

        public void Receive(Envelope envelope, IMessageCallback callback)
        {
            Received.Add(envelope);

            callback.MarkSuccessful();
        }
    }

    public static class Wait
    {
        public static bool Until(Func<bool> condition, int millisecondPolling = 500, int timeoutInMilliseconds = 5000)
        {
            if (condition()) return true;

            var clock = new Stopwatch();
            clock.Start();

            while (clock.ElapsedMilliseconds < timeoutInMilliseconds)
            {
                Thread.Yield();
                Thread.Sleep(500);

                if (condition()) return true;
            }

            return false;
        }
    }
}
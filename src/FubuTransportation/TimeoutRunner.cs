using System;
using System.Threading;

namespace FubuTransportation
{
    public enum Completion
    {
        Success,
        Timedout,
        Exception
    }

    public static class TimeoutRunner
    {
        public static Completion Run(TimeSpan timeout, Action action, Action<Exception> onError)
        {
            var returnValue = Completion.Timedout;

            var reset = new ManualResetEvent(false);

            var thread = new Thread(() => {
                try
                {
                    action();
                    reset.Set();
                    returnValue = Completion.Success;
                }
                catch (ThreadAbortException ex)
                {
                    returnValue = Completion.Timedout;
                    onError(ex);
                }
                catch (Exception ex)
                {
                    returnValue = Completion.Exception;
                    onError(ex);
                    reset.Set();
                }
            });

            thread.Start();
            if (!reset.WaitOne(timeout))
            {
                thread.Abort();
                returnValue = Completion.Timedout;
            }

            return returnValue;
        }

        
    }
}
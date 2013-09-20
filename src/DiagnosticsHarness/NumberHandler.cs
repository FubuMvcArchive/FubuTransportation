using System;
using System.Diagnostics;
using System.Linq;
using FubuMVC.Core;
using FubuMVC.Core.Registration;

namespace DiagnosticsHarness
{
    public class NumberHandler
    {
        private readonly INumberCache _cache;

        public NumberHandler(INumberCache cache)
        {
            _cache = cache;
        }

        public void Consume(NumberMessage message)
        {
            if (message.Value > 100)
            {
                throw new ArithmeticException("Too big for me!");
            }

            _cache.Register(message.Value);
        }
    }

    public static class TryThings
    {
        public static void Go()
        {
            using (var runtime = FubuApplication.BootstrapApplication<HarnessApplication>())
            {
                var graph = runtime.Factory.Get<BehaviorGraph>();

                var chain = graph.Behaviors.FirstOrDefault(x => x.InputType() == typeof (NumberMessage));

                Debug.WriteLine(chain);
            }
        }
    }
}
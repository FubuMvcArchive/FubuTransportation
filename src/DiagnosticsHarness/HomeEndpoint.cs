using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuMVC.Core.Continuations;
using FubuTransportation;

namespace DiagnosticsHarness
{
    public class HomeEndpoint
    {
        private readonly IServiceBus _serviceBus;
        private readonly INumberCache _cache;

        public HomeEndpoint(IServiceBus serviceBus, INumberCache cache)
        {
            _serviceBus = serviceBus;
            _cache = cache;
        }

        public FubuContinuation post_numbers(NumberPost input)
        {
            var numbers =
                input.Numbers.ToDelimitedArray().Select(x => { return new NumberMessage {Value = int.Parse(x)}; });

            numbers.Each(x => _serviceBus.Send<NumberMessage>(x));

            return
                FubuContinuation.RedirectTo<FubuMVC.Instrumentation.Features.Instrumentation.InstrumentationEndpoint>(
                    x => x.get_instrumentation(null));
        }

        public string get_received()
        {
            return _cache.Captured.Select(x => x.ToString()).Join("\n");
        }

        public HomeModel Index()
        {
            return new HomeModel();
        }
    }
}
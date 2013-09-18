using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Urls;
using FubuTransportation.Configuration;

namespace DiagnosticsHarness
{
    public class HarnessRegistry : FubuTransportRegistry<HarnessSettings>
    {
        public HarnessRegistry()
        {
        }
    }

    public class HomeEndpoint
    {
        private readonly IUrlRegistry _urls;

        public HomeEndpoint(IUrlRegistry urls)
        {
            _urls = urls;
        }

        public FubuContinuation post_numbers(NumberPost numbers)
        {
            throw new NotImplementedException();
        }
    }


    public class NumberPost
    {
        public string Numbers { get; set; }
    }

    public class HarnessSettings
    {
        public Uri Channel { get; set; }
    }

    public class NumberMessage
    {
        public int Value { get; set; }
    }

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

    public interface INumberCache
    {
        void Register(int number);

        IEnumerable<int> Captured { get; } 
    }

    public class NumberCache : INumberCache
    {
        private readonly IList<int> _numbers = new List<int>(); 

        public void Register(int number)
        {
            _numbers.Add(number);
        }

        public IEnumerable<int> Captured
        {
            get
            {
                return _numbers;
            }
        }
    }
}

using System.Collections.Generic;

namespace FubuTransportation.Runtime
{
    public class DictionaryHeaders : IHeaders
    {
        private readonly IDictionary<string, string> _inner;

        public DictionaryHeaders() : this(new Dictionary<string, string>())
        {
        }

        public DictionaryHeaders(IDictionary<string, string> inner)
        {
            _inner = inner;
        }

        public string this[string key]
        {
            get { return _inner.ContainsKey(key) ? _inner[key] : null; }
            set
            {
                if (_inner.ContainsKey(key))
                {
                    _inner[key] = value;
                }
                else
                {
                    _inner.Add(key, value);
                }
            }
        }

        public IEnumerable<string> Keys()
        {
            return _inner.Keys;
        }
    }
}
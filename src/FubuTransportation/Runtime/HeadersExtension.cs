using System.Collections.Generic;
using System.Collections.Specialized;
using FubuTransportation.Configuration;

namespace FubuTransportation.Runtime
{
    public static class HeadersExtension
    {
        public static IHeaders CloneForSource(this IHeaders headers, ChannelNode node)
        {
            var clone = new NameValueHeaders();
            headers.Keys().Each(key => clone[key] = headers[key]);

            clone[Envelope.SourceKey] = node.Uri.ToString();
            clone[Envelope.ChannelKey] = node.Key;

            return clone;
        }

        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Reflection;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuCore;

namespace FubuTransportation.InMemory
{
    public class InMemoryTransport : ITransport
    {
        public void Dispose()
        {
            // nothing
        }

        public void OpenChannels(ChannelGraph graph)
        {
            graph.Where(x => x.Protocol() == InMemoryChannel.Protocol).Each(x => x.Channel = new InMemoryChannel(x));
        }

        public static T ToInMemory<T>() where T : new()
        {
            var type = typeof (T);
            var settings = ToInMemory<T>(type);

            return (T) settings;
        }

        public static object ToInMemory<T>(Type type) where T : new()
        {
            var settings = Activator.CreateInstance(type);

            type.GetProperties().Where(x => x.CanWrite && x.PropertyType == typeof (Uri)).Each(prop => {
                var accessor = new SingleProperty(prop);
                var uri = "{0}://{1}/{2}".ToFormat(InMemoryChannel.Protocol, accessor.OwnerType.Name.Replace("Settings", ""),
                                                   accessor.Name);

                accessor.SetValue(settings, new Uri(uri));
            });
            return settings;
        }
    }
}
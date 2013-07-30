using System;
using System.Collections.Generic;
using FubuCore;
using FubuCore.Reflection;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Routing;
using System.Linq;

namespace FubuTransportation.Configuration
{
    public class ChannelNode
    {
        // TODO -- blow up if Accessor is not a Uri!!!!
        public Accessor SettingAddress { get; set; }
        public string Key { get; set; }

        public int ThreadCount = 1;
        public bool Incoming = false;

        public IList<IRoutingRule> Rules = new List<IRoutingRule>();

        public Uri Uri { get; set; }
        public IChannel Channel { get; set; }
 
        public bool Publishes(Type type)
        {
            return Rules.Any(x => x.Matches(type));
        }
        
        public void ReadSettings(IServiceLocator services)
        {
            var settings = services.GetInstance(SettingAddress.OwnerType);
            Uri = (Uri) SettingAddress.GetValue(settings);
        }

        public string Protocol()
        {
            throw new NotImplementedException();
        }
    }

    
}
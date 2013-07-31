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
        public Accessor SettingAddress
        {
            get { return _settingAddress; }
            set
            {
                if (value.PropertyType != typeof (Uri))
                {
                    throw new ArgumentOutOfRangeException("SettingAddress", "Can only be a Uri property");
                }
                _settingAddress = value;
            }
        }

        public string Key { get; set; }

        public int ThreadCount = 1;
        public bool Incoming = false;

        public IList<IRoutingRule> Rules = new List<IRoutingRule>();
        private Accessor _settingAddress;

        public Uri Uri { get; set; }
        public IChannel Channel { get; set; }

        public string DefaultContentType { get; set; }

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
            return Uri.Scheme;
        }
    }

    
}
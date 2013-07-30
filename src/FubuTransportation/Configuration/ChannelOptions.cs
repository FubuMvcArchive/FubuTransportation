using System;
using System.Linq.Expressions;
using FubuCore.Reflection;
using FubuCore.Util;

namespace FubuTransportation.Configuration
{
    public class ChannelGraph
    {
        private readonly Cache<string, ChannelOptions> _channels = new Cache<string, ChannelOptions>();

        public static string ToKey(Accessor accessor)
        {
            return accessor.OwnerType.Name.Replace("Settings", "") + ":" + accessor.Name;
        }

        public static string ToKey<T>(Expression<Func<T, object>> property)
        {
            return ToKey(property.ToAccessor());
        }
    }

    public class ChannelOptions
    {



        public int ThreadCount = 1;
        
    }

    
}
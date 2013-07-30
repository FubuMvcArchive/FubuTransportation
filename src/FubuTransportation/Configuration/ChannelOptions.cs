using FubuCore.Reflection;
using FubuCore.Util;

namespace FubuTransportation.Configuration
{
    public class ChannelGraph
    {
        private readonly Cache<string, ChannelOptions> _channels = new Cache<string, ChannelOptions>();
    }

    public class ChannelOptions
    {



        public int ThreadCount = 1;
        
    }

    
}
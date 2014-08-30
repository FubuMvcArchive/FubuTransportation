using System;
using FubuCore;
using FubuCore.Logging;
using FubuTransportation.Configuration;

namespace FubuTransportation.Monitoring
{
    public class PersistentTaskMessageModifier : ILogModifier
    {
        private readonly ChannelGraph _graph;

        public PersistentTaskMessageModifier(ChannelGraph graph)
        {
            _graph = graph;
        }

        public bool Matches(Type logType)
        {
            return logType.CanBeCastTo<PersistentTaskMessage>();
        }

        public void Modify(object log)
        {
            throw new NotImplementedException();
        }
    }
}
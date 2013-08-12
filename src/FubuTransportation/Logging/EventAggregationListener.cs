using System;
using System.Reflection;
using FubuCore.Logging;
using FubuTransportation.Runtime;

namespace FubuTransportation.Logging
{
    public class EventAggregationListener : ILogListener
    {
        private readonly IEventAggregator _events;

        public EventAggregationListener(IEventAggregator events)
        {
            _events = events;
        }

        public bool ListensFor(Type type)
        {
            return type.Assembly.GetName().Name == Assembly.GetExecutingAssembly().GetName().Name;
        }

        public void DebugMessage(object message)
        {
            _events.RouteMessage(message);
        }

        public void InfoMessage(object message)
        {
            _events.RouteMessage(message);
        }

        public void Debug(string message)
        {
            // no-op
        }

        public void Info(string message)
        {
            // no-op
        }

        // TODO -- Do we want to do something here?
        public void Error(string message, Exception ex)
        {
            // no-op
        }

        // TODO -- Do we want to do something here?
        public void Error(object correlationId, string message, Exception ex)
        {
            // no-op
        }

        // TODO -- do we wanna turn this on or off?
        public bool IsDebugEnabled { get { return true; } }
        public bool IsInfoEnabled { get { return true; } }
    }

    public class ChainExecutionStarted : LogRecord
    {
        public Guid ChainId { get; set; }
        public Envelope Envelope { get; set; }
    }

    public class ChainExecutionFinished : LogRecord
    {
        public Guid ChainId { get; set; }
        public Envelope Envelope { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}
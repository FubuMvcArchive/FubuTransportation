﻿using System;
using System.Reflection;
using Bottles;
using FubuCore.Logging;

namespace FubuTransportation.Logging
{
    public class EventAggregationListener : ILogListener
    {
        private readonly IEventAggregator _events;
        private readonly TransportSettings _settings;

        public EventAggregationListener(IEventAggregator events, TransportSettings settings)
        {
            _events = events;
            _settings = settings;
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
        public bool IsDebugEnabled { get { return _settings.DebugEnabled; } }
        public bool IsInfoEnabled { get { return _settings.InfoEnabled; } }
    }
}
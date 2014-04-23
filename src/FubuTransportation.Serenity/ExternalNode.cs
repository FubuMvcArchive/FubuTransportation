using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTransportation.Configuration;
using FubuTransportation.Diagnostics;
using FubuTransportation.Events;
using FubuTransportation.InMemory;
using FubuTransportation.Runtime;
using StructureMap;

namespace FubuTransportation.Serenity
{
    public class ExternalNode : IDisposable
    {
        private readonly string _name;
        private readonly ChannelGraph _systemUnderTest;
        private readonly Type _registryType;
        private FubuRuntime _runtime;
        private bool _isStarted;
        private Type _settingsType;

        public ExternalNode(string name, Type registryType, ChannelGraph systemUnderTest)
        {
            _registryType = registryType;
            _name = name;
            _systemUnderTest = systemUnderTest;

            if (!IsValidRegistryType(_registryType))
            {
                throw new ArgumentException("Registry type must extend FubuTransportRegistry<T>", "registryType");
            }
        }

        private bool IsValidRegistryType(Type type)
        {
            var baseType = type.BaseType;
            bool isTransportRegistry = false;
            while (baseType != null)
            {
                if (baseType.IsConstructedGenericType
                    && baseType.GetGenericTypeDefinition() == typeof(FubuTransportRegistry<>))
                {
                    isTransportRegistry = true;
                    _settingsType = baseType.GetGenericArguments()[0];
                    break;
                }

                baseType = baseType.BaseType;
            }
            return isTransportRegistry && type.IsConcrete();
        }

        public Uri Uri { get; private set; }

        public void Dispose()
        {
            _isStarted = false;
            if (_runtime != null)
            {
                _runtime.Dispose();
                _runtime = null;
            }
        }

        public bool ReceivedMessage<T>(Func<T, bool> predicate = null)
        {
            var recorder = _runtime.Factory.Get<IMessageRecorder>();
            return recorder.ReceivedMessages
                .Any(x => x.GetType().CanBeCastTo<T>()
                          && (predicate == null || predicate(x.As<T>())));
        }

        public IEnumerable<T> ReceivedMessages<T>()
        {
            var recorder = _runtime.Factory.Get<IMessageRecorder>();
            return recorder.ReceivedMessages
                .OfType<T>();
        }

        /// <summary>
        /// Sends a message from this node to the system under test.
        /// </summary>
        public void Send<T>(T message)
        {
            var channelNode = _systemUnderTest.FirstOrDefault(x => x.Publishes(typeof(T)));
            if (channelNode == null)
                throw new ArgumentException("Cannot find destination channel for message type {0}".ToFormat(typeof(T)), "message");

            Uri destination = channelNode.Uri;
            var bus = _runtime.Factory.Get<IServiceBus>();
            bus.Send(destination, message);
        }

        public void Start()
        {
            if (_isStarted)
                return;
            _isStarted = true;

            var registry = Activator.CreateInstance(_registryType).As<FubuTransportRegistry>();
            registry.NodeName = _name;
            registry.EnableInMemoryTransport();
            TestNodes.Alterations.Each(x => x(registry));

            Debug.WriteLine("Starting test node for {0} using registry {1}", _name, _registryType);

            var settings = CreateInMemorySettings();
            var container = new Container(x =>
            {
                x.For(settings.GetType()).Use(settings);
                x.ForSingletonOf<IMessageRecorder>().Use<MessageRecorder>();
                x.Forward<IMessageRecorder, IListener>();
            });

            _runtime = FubuTransport.For(registry).StructureMap(container).Bootstrap();
            Uri = _runtime.Factory.Get<ChannelGraph>().ReplyUriList().First();

            // Wireup the messaging session so the MessageHistory gets notified of messages on this node
            var session = _runtime.Factory.Get<IMessagingSession>();
            Bottles.Services.Messaging.EventAggregator.Messaging.AddListener(session);

            Debug.WriteLine("Started test node with URI: {0}", Uri);
        }

        private object CreateInMemorySettings()
        {
            var settings = InMemoryTransport.ToInMemory(_settingsType);

            // Sync the URIs between the external endpoints and the channels 
            // configured in the system under test.
            _systemUnderTest.Each(channel =>
            {
                string channelName = channel.Key.Split(':')[1];
                var property = _settingsType.GetProperty(channelName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(settings, channel.Uri);
                }
            });
            return settings;
        }
    }
}
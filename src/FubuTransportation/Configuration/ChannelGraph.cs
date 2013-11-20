using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using FubuCore;
using FubuCore.Reflection;
using FubuCore.Util;
using FubuMVC.Core.Registration;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Runtime.Serializers;
using FubuTransportation.Subscriptions;


namespace FubuTransportation.Configuration
{
    [ApplicationLevel]
    public class ChannelGraph : IEnumerable<ChannelNode>, IDisposable
    {
        private readonly Cache<string, ChannelNode> _channels =
            new Cache<string, ChannelNode>(key => new ChannelNode {Key = key});

        private readonly Cache<string, Uri> _replyChannels = new Cache<string, Uri>(name => {
            throw new ArgumentOutOfRangeException("No known reply channel for protocol '{0}'".ToFormat(name));
        });

        public ChannelGraph()
        {
            DefaultContentType = new XmlMessageSerializer().ContentType;
        }


        /// <summary>
        /// Used to identify the instance of the running FT node
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The default content type to use for serialization if none is specified at
        /// either the message or channel level
        /// </summary>
        public string DefaultContentType { get; set; }


        private readonly ReaderWriterLockSlim _subscriptionLock = new ReaderWriterLockSlim();
        // Add some locking here!
        public IEnumerable<Subscription> DynamicSubscriptions
        {
            get
            {
                return _subscriptionLock.Read(() => {
                    return _dynamicSubscriptions ?? new Subscription[0];
                });
            }
            set
            {
                _subscriptionLock.Write(() => {
                    _dynamicSubscriptions = value;
                });
            }
        }

        public ChannelNode ChannelFor<T>(Expression<Func<T, Uri>> property)
        {
            return ChannelFor(ReflectionHelper.GetAccessor(property));
        }

        public ChannelNode ChannelFor(Accessor accessor)
        {
            var key = ToKey(accessor);
            var channel = _channels[key];
            channel.SettingAddress = accessor;

            return channel;
        }

        public Uri ReplyChannelFor(string protocol)
        {
            return _replyChannels[protocol];
        }

        public void AddReplyChannel(string protocol, Uri uri)
        {
            _replyChannels[protocol] = uri;
        }

        public IEnumerable<ChannelNode> NodesForProtocol(string protocol)
        {
            return _channels.Where(x => x.Protocol() != null && x.Protocol().EqualsIgnoreCase(protocol))
                .Distinct()
                .ToArray();
        }


        // leave it virtual for testing
        public virtual void ReadSettings(IServiceLocator services)
        {
            _channels.Each(x => x.ReadSettings(services));
        }

        public virtual void StartReceiving(IHandlerPipeline pipeline)
        {
            _channels.Where(x => x.Incoming).Each(node => node.StartReceiving(pipeline, this));
        }

        public static string ToKey(Accessor accessor)
        {
            return accessor.OwnerType.Name.Replace("Settings", "") + ":" + accessor.Name;
        }

        public static string ToKey<T>(Expression<Func<T, object>> property)
        {
            return ToKey(property.ToAccessor());
        }

        public IEnumerator<ChannelNode> GetEnumerator()
        {
            return _channels.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(ChannelNode replyNode)
        {
            _channels[replyNode.Key] = replyNode;
        }

        private bool _wasDisposed;
        private IEnumerable<Subscription> _dynamicSubscriptions;

        public void Dispose()
        {
            if (_wasDisposed) return;

            _channels.Each(x => x.Dispose());

            _wasDisposed = true;
        }
    }
}
using System;
using System.Collections.Generic;
using FubuCore;
using FubuCore.Reflection;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Headers;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Runtime.Routing;
using System.Linq;
using FubuTransportation.Scheduling;

namespace FubuTransportation.Configuration
{
    public class ChannelNode : IDisposable
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

        public readonly IList<ISettingsAware> SettingsRules = new List<ISettingsAware>(); 

        public string Key { get; set; }

        public IScheduler Scheduler = TaskScheduler.Default();
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

            SettingsRules.Each(x => x.ApplySettings(settings, this));
        }

        public string Protocol()
        {
            return Uri != null ? Uri.Scheme : null;
        }

        public void Describe(IScenarioWriter writer)
        {
            writer.WriteLine(Key);
            using (writer.Indent())
            {
                if (Incoming)
                {
                    writer.WriteLine("Listens to {0} with {1}", Uri, Scheduler);
                }

                Rules.Each(x => x.Describe());
            }
        }

        public override string ToString()
        {
            return string.Format("Channel: {0}", Key);
        }

        public void Dispose()
        {
            // TODO -- going to come back and try to make the scheduler "drain"
            Channel.Dispose();
            Scheduler.Dispose();
        }

        public void StartReceiving(IHandlerPipeline pipeline, ChannelGraph graph)
        {
            if (Channel == null) throw new InvalidOperationException("Cannot receive on node {0} without a matching channel".ToFormat(SettingAddress));
            var receiver = new Receiver(pipeline, graph, this);
            StartReceiving(receiver);
        }

        public void StartReceiving(IReceiver receiver)
        {
            Scheduler.Start(() => {
                var receivingState = ReceivingState.CanContinueReceiving;
                while (receivingState == ReceivingState.CanContinueReceiving)
                {
                    receivingState = Channel.Receive(receiver);
                }
            });
        }

        // virtual for testing of course
        public virtual IHeaders Send(Envelope envelope, Uri replyUri = null)
        {
            var clone = new NameValueHeaders();
            envelope.Headers.Keys().Each(key => clone[key] = envelope.Headers[key]);

            clone[Envelope.DestinationKey] = Uri.ToString();
            clone[Envelope.ChannelKey] = Key;

            if (replyUri != null)
            {
                clone[Envelope.ReplyUriKey] = replyUri.ToString();
            }

            Channel.Send(envelope.Data, clone);

            return clone;
        }
    }

    
}
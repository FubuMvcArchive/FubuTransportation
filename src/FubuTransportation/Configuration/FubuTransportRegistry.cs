using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bottles;
using FubuCore;
using FubuCore.DependencyAnalysis;
using FubuCore.Reflection;
using FubuMVC.Core;
using FubuMVC.Core.Configuration;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Conventions;
using FubuMVC.Core.Registration.Diagnostics;
using FubuTransportation.InMemory;
using FubuTransportation.Polling;
using FubuTransportation.Registration;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Routing;
using FubuTransportation.Runtime.Serializers;
using FubuTransportation.Sagas;
using FubuTransportation.Scheduling;

namespace FubuTransportation.Configuration
{
    public class FubuTransportRegistry : IFubuRegistryExtension
    {
        private readonly IList<IHandlerSource> _sources = new List<IHandlerSource>(); 
        internal readonly PollingJobHandlerSource _pollingJobs = new PollingJobHandlerSource(); // leave it as internal
        private readonly IList<Action<ChannelGraph>> _channelAlterations = new List<Action<ChannelGraph>>(); 
        private readonly IList<Action<FubuRegistry>> _alterations = new List<Action<FubuRegistry>>(); 
        private readonly ConfigurationActionSet _localPolicies = new ConfigurationActionSet(ConfigurationType.Policy);
        private readonly ProvenanceChain _provenance;

        public static FubuTransportRegistry For(Action<FubuTransportRegistry> configure)
        {
            var registry = new FubuTransportRegistry();
            configure(registry);

            return registry;
        }

        public static HandlerGraph HandlerGraphFor(Action<FubuTransportRegistry> configure)
        {
            var behaviors = BehaviorGraphFor(configure);

            return behaviors.Settings.Get<HandlerGraph>();
        }

        public static BehaviorGraph BehaviorGraphFor(Action<FubuTransportRegistry> configure)
        {
            var registry = new FubuRegistry();
            var transportRegistry = new FubuTransportRegistry();

            configure(transportRegistry);

            transportRegistry.As<IFubuRegistryExtension>()
                .Configure(registry);

            new FubuTransportationExtensions().Configure(registry);

            return BehaviorGraph.BuildFrom(registry);
        }

        /// <summary>
        /// Import configuration from an extension
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Import<T>() where T : IFubuTransportRegistryExtension, new()
        {
            new T().Configure(this);
        }

        /// <summary>
        /// Import configuration from an extentension with configured options
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configuration"></param>
        public void Import<T>(Action<T> configuration) where T : IFubuTransportRegistryExtension, new()
        {
            var extension = new T();
            configuration(extension);

            extension.Configure(this);
        }

        /// <summary>
        /// Import configuration from an extension
        /// </summary>
        /// <param name="extension"></param>
        public void Import(IFubuTransportRegistryExtension extension)
        {
            extension.Configure(this);
        }

        public void SagaStorage<T>() where T : ISagaStorage, new()
        {
            AlterSettings<TransportSettings>(x => x.SagaStorageProviders.Add(new T()));
        }

        public static FubuTransportRegistry Empty()
        {
            return new FubuTransportRegistry();
        }

        protected FubuTransportRegistry()
        {
            _provenance = new ProvenanceChain(new Provenance[] {new FubuTransportRegistryProvenance(this)});

            AlterSettings<ChannelGraph>(x => {
                if (x.Name.IsEmpty())
                {
                    x.Name = GetType().Name.Replace("TransportRegistry", "").Replace("Registry", "").ToLower();
                }
            });
        }

        public void AlterSettings<T>(Action<T> alteration) where T : new()
        {
            _alterations.Add(r => r.AlterSettings(alteration));
        }

        public string NodeName
        {
            set
            {
                AlterSettings<ChannelGraph>(x => x.Name = value);
            }
        }

        internal Action<ChannelGraph> channel
        {
            set
            {
                _channelAlterations.Add(value);
            }
        }

        private IEnumerable<IHandlerSource> allSources()
        {

            if (_sources.Any())
            {
                foreach (var handlerSource in _sources)
                {
                    yield return handlerSource;
                }
            }
            else
            {
                var source = new HandlerSource();
                source.UseThisAssembly();
                source.IncludeClassesSuffixedWithConsumer();
                source.IncludeClassesSuffixedWithHandler();
                source.IncludeClassesMatchingSagaConvention();

                yield return source;
            }

            if (_pollingJobs.HasAny())
            {
                yield return _pollingJobs;
            }
        } 

        void IFubuRegistryExtension.Configure(FubuRegistry registry)
        {
            var graph = new HandlerGraph();
            var allCalls = allSources().SelectMany(x => x.FindCalls()).Distinct();
            graph.Add(allCalls);

            graph.ApplyPolicies(_localPolicies);

            registry.AlterSettings<HandlerGraph>(x => x.Import(graph));

            registry.AlterSettings<ChannelGraph>(channels => {
                _channelAlterations.Each(x => x(channels));
            });

            _alterations.Each(x => x(registry));
        }

        /// <summary>
        ///   Finds the currently executing assembly.
        /// </summary>
        /// <returns></returns>
        public static Assembly FindTheCallingAssembly()
        {
            var trace = new StackTrace(false);

            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Assembly fubuCore = typeof(ITypeResolver).Assembly;
            Assembly bottles = typeof(IPackageLoader).Assembly;
            Assembly fubumvc = typeof (FubuRegistry).Assembly;
            

            Assembly callingAssembly = null;
            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                Assembly assembly = frame.GetMethod().DeclaringType.Assembly;
                if (assembly != thisAssembly && assembly != fubuCore && assembly != bottles && assembly != fubumvc && !assembly.GetName().Name.StartsWith("System."))
                {
                    callingAssembly = assembly;
                    break;
                }
            }
            return callingAssembly;
        }

        public HandlersExpression Handlers
        {
            get
            {
                return new HandlersExpression(this);
            }
        }

        public class HandlersExpression
        {
            private readonly FubuTransportRegistry _parent;

            public HandlersExpression(FubuTransportRegistry parent)
            {
                _parent = parent;
            }

            public void Include(params Type[] types)
            {
                _parent._sources.Add(new ExplicitTypeHandlerSource(types));
            }

            public void Include<T>()
            {
                Include(typeof(T));
            }

            public void FindBy(Action<HandlerSource> configuration)
            {
                var source = new HandlerSource();
                configuration(source);

                _parent._sources.Add(source);
            }

            public void FindBy<T>() where T : IHandlerSource, new()
            {
                _parent._sources.Add(new T());
            }

            public void FindBy(IHandlerSource source)
            {
                _parent._sources.Add(source);
            }

            /// <summary>
            /// Just disables handler loading for this registry
            /// </summary>
            public void DisableDefaultHandlerSource()
            {
                Include<NulloHandlerSource>();
            }
        }

        public PollingJobExpression Polling
        {
            get
            {
                return new PollingJobExpression(this);
            }
        }

        public void DefaultSerializer<T>() where T : IMessageSerializer, new()
        {
            channel = graph => graph.DefaultContentType = new T().ContentType;
        }

        public void DefaultContentType(string contentType)
        {
            channel = graph => graph.DefaultContentType = contentType;
        }

        /// <summary>
        /// Applies a Policy to the handler chains created by only this
        /// FubuTransportRegistry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PoliciesExpression Local
        {
            get
            {
                return new PoliciesExpression(x => _localPolicies.Fill(_provenance, x));
            }
        }

        /// <summary>
        /// Applies a Policy to all FubuTransportation Handler chains
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PoliciesExpression Global
        {
            get
            {
                return new PoliciesExpression(policy => {
                    AlterSettings<HandlerPolicies>(x => x.AddGlobal(policy, this));
                });
            }
        }




        /// <summary>
        ///   Configures the <see cref = "IServiceRegistry" /> to specify dependencies. 
        ///   This is an IoC-agnostic method of dependency configuration that will be consumed by the underlying implementation (e.g., StructureMap)
        /// </summary>
        public void Services(Action<ServiceRegistry> configure)
        {
            _alterations.Add(r => r.Services(configure));
        }


        public void Services<T>() where T : ServiceRegistry, new()
        {
            _alterations.Add(r => r.Services<T>());
        }

        /// <summary>
        /// Enable the in memory transport
        /// </summary>
        public void EnableInMemoryTransport()
        {
            AlterSettings<TransportSettings>(x => x.EnableInMemoryTransport = true);
        }
    }

    public class FubuTransportRegistry<T> : FubuTransportRegistry
    {
        protected FubuTransportRegistry()
        {
            AlterSettings<TransportSettings>(x => x.SettingTypes.Fill(typeof (T)));
        }


        public new static FubuTransportRegistry<T> Empty()
        {
            return new FubuTransportRegistry<T>();
        }


        public ChannelExpression Channel(Expression<Func<T, Uri>> expression)
        {
            return new ChannelExpression(this, expression);
        }

        public class ChannelExpression
        {
            private readonly FubuTransportRegistry<T> _parent;
            private readonly Accessor _accessor;

            public ChannelExpression(FubuTransportRegistry<T> parent, Expression<Func<T, Uri>> expression)
            {
                _parent = parent;
                _accessor = ReflectionHelper.GetAccessor(expression);
            }

            private Action<ChannelNode> alter
            {
                set
                {
                    _parent.channel = graph => {
                        var node = graph.ChannelFor(_accessor);
                        value(node);
                    };
                }
            }

            public ChannelExpression DefaultSerializer<TSerializer>() where TSerializer : IMessageSerializer, new()
            {
                alter = node => node.DefaultContentType = new TSerializer().ContentType;
                return this;
            }

            public ChannelExpression DefaultContentType(string contentType)
            {
                alter = node => node.DefaultContentType = contentType;
                return this;
            }

            public ChannelExpression ReadIncoming(IScheduler scheduler = null)
            {
                    alter = node =>
                    {
                        var defaultScheduler = node.Scheduler;
                        node.Incoming = true;
                        node.Scheduler = scheduler ?? defaultScheduler;
                    };
                return this;
            }

            public ChannelExpression ReadIncoming(SchedulerMaker<T> schedulerMaker)
            {
                alter = node => node.SettingsRules.Add(schedulerMaker);
                return this;
            }


            public ChannelExpression AcceptsMessagesInNamespaceContainingType<TMessageType>()
            {
                alter = node => node.Rules.Add(NamespaceRule.For<TMessageType>());
                return this;
            }

            public ChannelExpression AcceptsMessagesInNamespace(string @namespace)
            {
                alter = node => node.Rules.Add(new NamespaceRule(@namespace));
                return this;
            }

            public ChannelExpression AcceptsMessagesInAssemblyContainingType<TMessageType>()
            {
                alter = node => node.Rules.Add(AssemblyRule.For<TMessageType>());
                return this;
            }

            public ChannelExpression AcceptsMessagesInAssembly(string assemblyName)
            {
                var assembly = Assembly.Load(assemblyName);

                alter = node => node.Rules.Add(new AssemblyRule(assembly));
                return this;
            }

            public ChannelExpression AcceptsMessage<TMessage>()
            {
                alter = node => node.Rules.Add(new SingleTypeRoutingRule<TMessage>());
                return this;
            }

            public ChannelExpression AcceptsMessage(Type messageType)
            {
                alter =
                    node => node.Rules.Add(typeof (SingleTypeRoutingRule<>).CloseAndBuildAs<IRoutingRule>(messageType));
                return this;
            }

            public ChannelExpression AcceptsMessages(Expression<Func<Type, bool>> filter)
            {
                alter = node => node.Rules.Add(new LambdaRoutingRule(filter));
                return this;
            }

            public ChannelExpression AcceptsMessagesMatchingRule<TRule>() where TRule : IRoutingRule, new()
            {
                alter = node => node.Rules.Add(new TRule());
                return this;
            }



        }

        public ByThreadScheduleMaker<T> ByThreads(Expression<Func<T, int>> property)
        {
            return new ByThreadScheduleMaker<T>(property);
        }

        public ByTaskScheduleMaker<T> ByTasks(Expression<Func<T, int>> property)
        {
            return new ByTaskScheduleMaker<T>(property);
        }
    }

    public class NulloHandlerSource : IHandlerSource
    {
        public IEnumerable<HandlerCall> FindCalls()
        {
            yield break;
        }
    }
}
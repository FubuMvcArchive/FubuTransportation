using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bottles;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Routing;

namespace FubuTransportation.Configuration
{
    public class FubuTransportRegistry : IFubuRegistryExtension
    {
        private readonly IList<IHandlerSource> _sources = new List<IHandlerSource>(); 
        private readonly IList<Action<ChannelGraph>> _channelAlterations = new List<Action<ChannelGraph>>(); 

        public static FubuTransportRegistry For(Action<FubuTransportRegistry> configure)
        {
            var registry = new FubuTransportRegistry();
            configure(registry);

            return registry;
        }

        public static FubuTransportRegistry Empty()
        {
            return new FubuTransportRegistry();
        }

        protected FubuTransportRegistry()
        {
            
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

                yield return source;
            }
        } 

        void IFubuRegistryExtension.Configure(FubuRegistry registry)
        {
            var graph = new HandlerGraph();
            var allCalls = allSources().SelectMany(x => x.FindCalls());
            graph.Add(allCalls);

            // TODO -- apply policies of some sort

            registry.AlterSettings<HandlerGraph>(x => x.Import(graph));

            registry.AlterSettings<ChannelGraph>(channels => {
                _channelAlterations.Each(x => x(channels));
            });
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

            public void FindBy<T>() where T : IHandlerSource, new()
            {
                _parent._sources.Add(new T());
            }

            public void FindBy(IHandlerSource source)
            {
                _parent._sources.Add(source);
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
    }

    public class FubuTransportRegistry<T> : FubuTransportRegistry
    {
        protected FubuTransportRegistry()
        {
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

            public ChannelExpression ReadIncoming(int threadCount = -1)
            {
                if (threadCount > 0)
                {
                    alter = node => {
                        node.Incoming = true;
                        node.ThreadCount = threadCount;
                    };
                }
                else
                {
                    alter = node => node.Incoming = true;
                }

                return this;
            }

            public ChannelExpression PublishesMessagesInNamespaceContainingType<TMessageType>()
            {
                alter = node => node.Rules.Add(NamespaceRule.For<TMessageType>());
                return this;
            }

            public ChannelExpression PublishesMessagesInNamespace(string @namespace)
            {
                alter = node => node.Rules.Add(new NamespaceRule(@namespace));
                return this;
            }

            public ChannelExpression PublishesMessagesInAssemblyContainingType<TMessageType>()
            {
                alter = node => node.Rules.Add(AssemblyRule.For<TMessageType>());
                return this;
            }

            public ChannelExpression PublishesMessagesInAssembly(string assemblyName)
            {
                var assembly = Assembly.Load(assemblyName);

                alter = node => node.Rules.Add(new AssemblyRule(assembly));
                return this;
            }

            public ChannelExpression PublishesMessage<TMessage>()
            {
                alter = node => node.Rules.Add(new SingleTypeRoutingRule<TMessage>());
                return this;
            }

            public ChannelExpression PublishesMessage(Type messageType)
            {
                alter =
                    node => node.Rules.Add(typeof (SingleTypeRoutingRule<>).CloseAndBuildAs<IRoutingRule>(messageType));
                return this;
            }

            public ChannelExpression PublishesMessages(Expression<Func<Type, bool>> filter)
            {
                alter = node => node.Rules.Add(new LambdaRoutingRule(filter));
                return this;
            }

            public ChannelExpression PublishesMessagesMatchingRule<TRule>() where TRule : IRoutingRule, new()
            {
                alter = node => node.Rules.Add(new TRule());
                return this;
            }
        }
    }

    
}
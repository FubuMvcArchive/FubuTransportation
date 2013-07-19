using System;
using FubuMVC.StructureMap;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using StructureMap;

namespace FubuTransportation.Testing.Runtime
{
    public abstract class InvocationContext
    {
        private FubuTransportRegistry theTransportRegistry;
        private Lazy<IMessageInvoker> _invoker;
            
            
            
        [SetUp]
        public void SetUp()
        {
            theTransportRegistry = FubuTransportRegistry.Empty();
            TestMessageRecorder.Clear();

            _invoker = new Lazy<IMessageInvoker>(() => {
                var container = new Container();
                FubuTransport.For(theTransportRegistry).StructureMap(container).Bootstrap();

                return container.GetInstance<IMessageInvoker>();
            });

            theContextIs();

        }

        protected virtual void theContextIs()
        {
            
        }


        protected void handler<T>()
        {
            handlersAre(typeof(T));
        }

        protected void handler<T, T1, T2, T3>()
        {
            handlersAre(typeof(T), typeof(T1), typeof(T2), typeof(T3));
        }


        protected void handlersAre(params Type[] handlers)
        {
            theTransportRegistry.Handlers.Include(handlers);
        }

        protected StubEnvelope sendMessage(Message message)
        {
            var envelope = new StubEnvelope();
            envelope.Messages = new object[]{message};

            sendEnvelope(envelope);

            return envelope;
        }

        protected void sendEnvelope(StubEnvelope envelope)
        {
            // ????????
        
            _invoker.Value.Invoke(envelope);
        }

        
    }
}
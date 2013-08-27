using System;
using FubuCore.Binding;
using FubuCore.Dates;
using FubuCore.Logging;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Configuration;
using FubuTransportation.Logging;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;
using Rhino.Mocks;
using System.Collections.Generic;
using StructureMap.Pipeline;
using Is = Rhino.Mocks.Constraints.Is;
using FubuCore;

namespace FubuTransportation.Testing.Runtime
{

    [TestFixture]
    public class when_invoking_a_message_right_now_happy_path : InteractionContext<MessageInvoker>
    {
        private OneMessage theMessage;
        private HandlerGraph theGraph;
        private HandlerChain theExpectedChain;
        private StubServiceFactory theFactory;
        private object[] cascadingMessages;

        protected override void beforeEach()
        {
            theMessage = new OneMessage();
            theGraph = FubuTransportRegistry.HandlerGraphFor(x => {
                x.Handlers.Include<OneHandler>();
                x.Handlers.Include<TwoHandler>();
                x.Handlers.Include<ThreeHandler>();
                x.Handlers.Include<FourHandler>();
            });

            Services.Inject<HandlerGraph>(theGraph);

            theExpectedChain = theGraph.ChainFor(typeof (OneMessage));

            cascadingMessages = new object[] { new object(), new object(), new object() };
            theFactory = new StubServiceFactory(theExpectedChain, MockFor<IActionBehavior>(), cascadingMessages);
            Services.Inject<IServiceFactory>(theFactory);

            ClassUnderTest.InvokeNow(theMessage);

        }

        [Test]
        public void executed_the_proper_chain_for_the_input_type()
        {
            MockFor<IActionBehavior>().AssertWasCalled(x => x.Invoke());
        }

        [Test]
        public void cascaded_events_should_be_sent_to_the_bus()
        {
            cascadingMessages.Each(o => {

                // This ugly bit of code is just proving that we have indeed sent an envelope
                // where the inner message is one of our expected cascading messages
                MockFor<IEnvelopeSender>().AssertWasCalled(x => x.Send(null), x => x.Constraints(Is.Matching<Envelope>(e => e.Message == o)));
            });
        }
    }

    [TestFixture]
    public class WhenInvokingWithNoHandlerForMessageType : InteractionContext<MessageInvoker>
    {
        private HandlerGraph theGraph;
 
        protected override void beforeEach()
        {
            theGraph = FubuTransportRegistry.HandlerGraphFor(x =>
            {
                x.Handlers.Include<OneHandler>();
                x.Handlers.Include<TwoHandler>();
                x.Handlers.Include<ThreeHandler>();
                x.Handlers.Include<FourHandler>();
            });

            Services.Inject<HandlerGraph>(theGraph);
        }


        [Test]
        public void should_throw_the_no_handler_exception()
        {
            Exception<NoHandlerException>.ShouldBeThrownBy(() => {
                ClassUnderTest.InvokeNow(new Message1()); // we don't have a handler for this type
            })
            .Message.ShouldContain(typeof(Message1).FullName);
        }
    }

    public class StubServiceFactory : IServiceFactory
    {
        private readonly HandlerChain _chain;
        private readonly IActionBehavior _behavior;
        private readonly object[] _cascadingMessages;
        public HandlerArguments Arguments;

        public StubServiceFactory(HandlerChain chain, IActionBehavior behavior, params object[] cascadingMessages)
        {
            _chain = chain;
            _behavior = behavior;
            _cascadingMessages = cascadingMessages;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IActionBehavior BuildBehavior(ServiceArguments arguments, Guid behaviorId)
        {
            Arguments = arguments.ShouldBeOfType<HandlerArguments>();
            _cascadingMessages.Each(x => Arguments.EnqueueCascading(x));

            _chain.UniqueId.ShouldEqual(behaviorId);

            return _behavior;
        }

        public T Get<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll<T>()
        {
            throw new NotImplementedException();
        }
    }



    [TestFixture]
    public class invocation_of_the_serialization : MessageInvokerContext
    {
        [Test]
        public void do_deserialize_if_the_message_is_not_already_set()
        {
            theChain = null;
            theEnvelope.Message = null;

            ClassUnderTest.Invoke(theEnvelope, theCallback);

            MockFor<IEnvelopeSerializer>().AssertWasCalled(x => x.Deserialize(theEnvelope));
        }

        [Test]
        public void do_not_deserialize_if_the_envelope_message_is_not_null()
        {
            theChain = null;
            theEnvelope.Message = new OneMessage();

            ClassUnderTest.Invoke(theEnvelope, theCallback);

            MockFor<IEnvelopeSerializer>().AssertWasNotCalled(x => x.Deserialize(theEnvelope));
        }
    }

    [TestFixture]
    public class when_receiving_an_envelope_that_is_marked_as_a_response : MessageInvokerContext
    {
        protected override void theContextIs()
        {
            theChain = new HandlerChain();

            theEnvelope.ResponseId = Guid.NewGuid().ToString();

            ClassUnderTest.Expect(x => x.ExecuteChain(theEnvelope, theChain, theCallback))
                          .Repeat.Never();

            ClassUnderTest.Invoke(theEnvelope, theCallback);
        }

        [Test]
        public void should_callback_that_dequeuing_is_successful()
        {
            theCallback.AssertWasCalled(x => x.MarkSuccessful());
        }

        [Test]
        public void should_log_message_successful()
        {
            assertInfoMessageWasLogged(new MessageSuccessful{Envelope = theEnvelope});
        }

        [Test]
        public void should_log_that_the_envelope_was_received()
        {
            assertInfoMessageWasLogged(new EnvelopeReceived
            {
                Envelope = theEnvelope
            });
        }

        [Test]
        public void should_NOT_call_through_to_execute_the_handlers_for_this_message()
        {
            ClassUnderTest.VerifyAllExpectations();
        }
    }

    [TestFixture]
    public class when_invoking_for_an_unrecognized_message_type : MessageInvokerContext
    {
        protected override void theContextIs()
        {
            theChain = null;

            ClassUnderTest.Expect(x => x.ExecuteChain(theEnvelope, theChain, theCallback))
                          .IgnoreArguments().Repeat.Never();

            ClassUnderTest.Invoke(theEnvelope, theCallback);
        }

        [Test]
        public void should_callback_that_dequeuing_is_successful()
        {
            theCallback.AssertWasCalled(x => x.MarkSuccessful());
        }

        [Test]
        public void should_log_that_the_envelope_was_received()
        {
            assertInfoMessageWasLogged(new EnvelopeReceived
            {
                Envelope = theEnvelope
            });
        }

        [Test]
        public void should_log_no_handler()
        {
            assertInfoMessageWasLogged(new NoHandlerForMessage{Envelope = theEnvelope});
        }

        [Test]
        public void should_NOT_call_through_to_execute_the_handlers_for_this_message()
        {
            ClassUnderTest.VerifyAllExpectations();
        }
    }
    

    [TestFixture]
    public class when_invoking_an_normal_envelope_and_the_chain_can_be_found : MessageInvokerContext
    {
        protected override void theContextIs()
        {
            theChain = new HandlerChain();

            ClassUnderTest.Expect(x => x.ExecuteChain(theEnvelope, theChain, theCallback));

            ClassUnderTest.Invoke(theEnvelope, theCallback);
        }

        [Test]
        public void should_log_that_the_envelope_was_received()
        {
            assertInfoMessageWasLogged(new EnvelopeReceived
            {
                Envelope = theEnvelope
            });
        }


        [Test]
        public void should_attempt_to_execute_the_chain()
        {
            ClassUnderTest.VerifyAllExpectations();
        }
    }




    [TestFixture]
    public class find_the_chain_by_envelope_message_type
    {
        [Test]
        public void can_find_chain_for_input_type()
        {
            var graph = FubuTransportRegistry.HandlerGraphFor(x => {
                x.Handlers.Include<OneHandler>();
                x.Handlers.Include<TwoHandler>();
            });

            var invoker = new MessageInvoker(null, graph, null, null, null, SystemTime.Default());

            invoker.FindChain(new Envelope {Message = new OneMessage()})
                   .OfType<HandlerCall>().Single()
                   .HandlerType.ShouldEqual(typeof (OneHandler));
        }

        
    }

    [TestFixture]
    public class when_executing_the_chain_happy_path : MessageInvokerContext
    {
        private IActionBehavior theBehavior;
        private HandlerArguments handlerArguments;

        protected override void theContextIs()
        {
            theBehavior = MockFor<IActionBehavior>();
            theChain = new HandlerChain();
            handlerArguments = new HandlerArguments(theEnvelope);

            MockFor<IServiceFactory>().Stub(x => x.BuildBehavior(handlerArguments, theChain.UniqueId))
                .Return(theBehavior);



            ClassUnderTest.ExecuteChain(theEnvelope, theChain, theCallback);
        }

        [Test]
        public void does_run_the_behavior_found_from_the_factory()
        {
            theBehavior.AssertWasCalled(x => x.Invoke());
        }

        [Test]
        public void should_mark_the_callback_as_successful()
        {
            theCallback.AssertWasCalled(x => x.MarkSuccessful());
        }

        [Test]
        public void should_log_message_successful()
        {
            assertInfoMessageWasLogged(new MessageSuccessful { Envelope = theEnvelope });
        }

    }

    [TestFixture]
    public class when_executing_an_envelope_that_is_delayed : MessageInvokerContext
    {
        protected override void theContextIs()
        {
            LocalSystemTime = DateTime.Today.AddHours(8);

            theEnvelope.ExecutionTime = UtcSystemTime.AddMinutes(10);

            ClassUnderTest.Invoke(theEnvelope, theCallback);
        }

        [Test]
        public void should_happily_redirect_the_callback_to_the_delayed_queue()
        {
            theCallback.AssertWasCalled(x => x.MoveToDelayed());
        }

        [Test]
        public void does_not_try_to_do_anything_else()
        {
            theCallback.AssertWasNotCalled(x => x.MarkFailed());
            theCallback.AssertWasNotCalled(x => x.MarkSuccessful());
        }

        [Test]
        public void does_log_the_envelope_is_delayed()
        {
            theLogger.InfoMessages.Single().ShouldEqual(new DelayedEnvelopeReceived {Envelope = theEnvelope});
        }
    }

    [TestFixture]
    public class when_executing_an_envelope_that_has_an_execution_time_in_the_past : MessageInvokerContext
    {
        protected override void theContextIs()
        {
            LocalSystemTime = DateTime.Today.AddHours(8);

            theEnvelope.ExecutionTime = UtcSystemTime.AddMinutes(-10);

            theChain = new HandlerChain();

            ClassUnderTest.Expect(x => x.ExecuteChain(theEnvelope, theChain, theCallback));

            ClassUnderTest.Invoke(theEnvelope, theCallback);
        }

        [Test]
        public void proceeds_to_the_normal_processing()
        {
            ClassUnderTest.VerifyAllExpectations();
        }
    }

    [TestFixture]
    public class when_executing_the_chain_and_if_fails : MessageInvokerContext
    {
        private IActionBehavior theBehavior;
        private HandlerArguments handlerArguments;
        private NotImplementedException theExceptionThrown;

        protected override void theContextIs()
        {
            theBehavior = MockFor<IActionBehavior>();
            theChain = new HandlerChain();
            handlerArguments = new HandlerArguments(theEnvelope);

            MockFor<IServiceFactory>().Stub(x => x.BuildBehavior(handlerArguments, theChain.UniqueId))
                .Return(theBehavior);

            theExceptionThrown = new NotImplementedException();
            theBehavior.Expect(x => x.Invoke()).Throw(theExceptionThrown);

            ClassUnderTest.ExecuteChain(theEnvelope, theChain, theCallback);
        }

        [Test]
        public void should_mark_the_callback_as_failed()
        {
            theCallback.AssertWasCalled(x => x.MarkFailed());
        }

        [Test]
        public void should_log_failure_message()
        {
            assertInfoMessageWasLogged(new MessageFailed{Envelope = theEnvelope, Exception = theExceptionThrown});
        }

        [Test]
        public void should_log_the_exception()
        {
            var report = theLogger.ErrorMessages.OfType<ExceptionReport>().Single();
            report.ExceptionText.ShouldEqual(theExceptionThrown.ToString());
        }
    }

    
    public abstract class MessageInvokerContext : InteractionContext<MessageInvoker>
    {
        protected Envelope theEnvelope;
        protected RecordingLogger theLogger;
        protected IMessageCallback theCallback;
        private HandlerChain _chain;


        protected sealed override void beforeEach()
        {
            theEnvelope = new Envelope {Data = new byte[] {1, 2, 3, 4}, Message = new OneMessage()};
            theCallback = MockFor<IMessageCallback>();
            theLogger = new RecordingLogger();

            Services.Inject<ILogger>(theLogger);

            Services.PartialMockTheClassUnderTest();
            theContextIs();
        }

        protected virtual void theContextIs()
        {
            
        }

        protected HandlerChain theChain
        {
            set
            {
                ClassUnderTest.Stub(x => x.FindChain(theEnvelope))
                              .Return(value);

                _chain = value;
            }
            get { return _chain; }
        }

        protected void assertInfoMessageWasLogged(object log)
        {
            theLogger.InfoMessages.ShouldContain(log);
        }

        protected void assertInfoMessageWasNotLogged(object log)
        {
            theLogger.InfoMessages.ShouldNotContain(log);
        }


    }
}
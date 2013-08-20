using System;
using FubuCore.Logging;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Runtime.Logging;
using FubuTransportation.Configuration;
using FubuTransportation.Logging;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Runtime;
using FubuTransportation.Testing.ScenarioSupport;
using NUnit.Framework;
using System.Linq;
using FubuTestingSupport;
using Rhino.Mocks;
using System.Collections.Generic;
using Is = Rhino.Mocks.Constraints.Is;

namespace FubuTransportation.Testing.Runtime
{
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

            var invoker = new MessageInvoker(null, graph, null, null, null);

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
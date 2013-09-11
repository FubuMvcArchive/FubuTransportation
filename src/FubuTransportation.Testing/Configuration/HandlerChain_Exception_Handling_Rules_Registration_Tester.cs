using System;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.ErrorHandling;
using NUnit.Framework;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class HandlerChain_Exception_Handling_Rules_Registration_Tester
    {
        [Test]
        public void retry_now()
        {
            var chain = new HandlerChain();

            chain.OnException<NotImplementedException>()
                .Retry();

            var handler = chain.ErrorHandlers.Single().ShouldBeOfType<ErrorHandler>();
            handler.Conditions.Single().ShouldBeOfType<ExceptionTypeMatch<NotImplementedException>>();
            handler.Continuation.ShouldBeOfType<RetryNowContinuation>();
        }

        [Test]
        public void requeue()
        {
            var chain = new HandlerChain();

            chain.OnException<NotSupportedException>()
                .Requeue();

            var handler = chain.ErrorHandlers.Single().ShouldBeOfType<ErrorHandler>();
            handler.Conditions.Single().ShouldBeOfType<ExceptionTypeMatch<NotSupportedException>>();
            handler.Continuation.ShouldBeOfType<RequeueContinuation>();
        }

        [Test]
        public void move_to_error_queue()
        {
            var chain = new HandlerChain();

            chain.OnException<NotSupportedException>()
                .MoveToErrorQueue();

            chain.ErrorHandlers.Single().ShouldBeOfType<MoveToErrorQueueHandler<NotSupportedException>>();
        }

        [Test]
        public void retry_later()
        {
            var chain = new HandlerChain();

            chain.OnException<NotSupportedException>()
                .RetryLater(10.Minutes());

            var handler = chain.ErrorHandlers.Single().ShouldBeOfType<ErrorHandler>();
            handler.Conditions.Single().ShouldBeOfType<ExceptionTypeMatch<NotSupportedException>>();
            handler.Continuation.ShouldBeOfType<DelayedRetryContinuation>()
                .Delay.ShouldEqual(10.Minutes());
        }
    }
}
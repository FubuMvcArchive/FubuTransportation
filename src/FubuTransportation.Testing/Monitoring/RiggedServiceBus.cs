using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FubuTransportation.Testing.Monitoring
{
    public class RiggedServiceBus : IServiceBus
    {
        private readonly IList<RequestReplyExpectation> _expectations = new List<RequestReplyExpectation>();


        public class RequestReplyExpectation : FromExpression, ReturnsExpression
        {
            public Uri Destination;
            public object Message;
            public object Response;

            public ReturnsExpression From(Uri destination)
            {
                Destination = destination;
                return this;
            }

            public void Returns(object response)
            {
                Response = response;
            }
        }

        public FromExpression ExpectMessage(object message)
        {
            var expectation = new RequestReplyExpectation
            {
                Message = message
            };

            _expectations.Add(expectation);

            return expectation;
        }

        public interface FromExpression
        {
            ReturnsExpression From(Uri destination);
        }

        public interface ReturnsExpression
        {
            void Returns(object response);
        }

        public Task<TResponse> Request<TResponse>(object request, RequestOptions options = null)
        {
            var expectation =
                _expectations.FirstOrDefault(x => x.Message == request && x.Destination == options.Destination);

            if (expectation == null)
                Assert.Fail("No expectation for message {0} to destination {1}", request, options.Destination);

            var completion = new TaskCompletionSource<TResponse>();
            completion.SetResult((TResponse) expectation.Response);

            return completion.Task;
        }

        public void Send<T>(T message)
        {
            throw new NotImplementedException();
        }

        public void Send<T>(Uri destination, T message)
        {
            throw new NotImplementedException();
        }

        public void Consume<T>(T message)
        {
            throw new NotImplementedException();
        }

        public void DelaySend<T>(T message, DateTime time)
        {
            throw new NotImplementedException();
        }

        public void DelaySend<T>(T message, TimeSpan delay)
        {
            throw new NotImplementedException();
        }

        public Task SendAndWait<T>(T message)
        {
            throw new NotImplementedException();
        }
    }
}
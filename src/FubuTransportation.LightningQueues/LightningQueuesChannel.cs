using System;
using System.Collections.Generic;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Delayed;
using FubuTransportation.Runtime.Headers;
using LightningQueues;
using LightningQueues.Model;

namespace FubuTransportation.LightningQueues
{
    public class LightningQueuesChannel : IChannel
    {
        private readonly Uri _address;
        private readonly string _queueName;
        private readonly IQueueManager _queueManager;
        private readonly IDelayedMessageCache<MessageId> _delayedMessages;
        private bool _disposed;

        public static LightningQueuesChannel Build(LightningUri uri, IPersistentQueues queues, IDelayedMessageCache<MessageId> delayedMessages, bool incoming)
        {
            var queueManager = queues.ManagerFor(uri.Endpoint, incoming);
            return new LightningQueuesChannel(uri.Address, uri.QueueName, queueManager, delayedMessages);
        }

        public LightningQueuesChannel(Uri address, string queueName, IQueueManager queueManager, IDelayedMessageCache<MessageId> delayedMessages)
        {
            _address = address;
            _queueName = queueName;
            _queueManager = queueManager;
            _delayedMessages = delayedMessages;
        }

        public Uri Address { get { return _address; } }

        public ReceivingState Receive(IReceiver receiver)
        {
            var stream = _queueManager.ReceiveStream(_queueName);
            stream.Each(x =>
            {
                if (_disposed)
                    return;
                receiver.Receive(x.Message.Data, new NameValueHeaders(x.Message.Headers),
                    new TransactionCallback(x.TransactionalScope, x.Message, _delayedMessages));

            });
            return ReceivingState.StopReceiving;
        }

        public void Send(byte[] data, IHeaders headers)
        {
            var messagePayload = new MessagePayload
            {
                Data = data,
                Headers = headers.ToNameValues()
            };

            var sendingScope = _queueManager.BeginTransactionalScope();
            var id = sendingScope.Send(_address, messagePayload);
            
            // TODO -- do we grab this?
            
            //data.CorrelationId = id.MessageIdentifier;
            sendingScope.Commit();
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }

    public static class MessageExtensions
    {
        public static Envelope ToEnvelope(this Message message)
        {
            var envelope = new Envelope(new NameValueHeaders(message.Headers))
            {
                Data = message.Data
            };

            return envelope;
        }

        public static EnvelopeToken ToToken(this Message message)
        {
            return new EnvelopeToken
            {
                Data = message.Data,
                Headers = new NameValueHeaders(message.Headers)
            };
        }

        public static MessagePayload ToPayload(this Message message)
        {
            var payload = new MessagePayload
            {
                Data = message.Data,
                Headers = message.Headers,
            };
            return payload;
        }

        public static MessagePayload ToPayload(this Message message, ErrorReport report)
        {
            var payload = message.ToPayload();
            payload.Headers.Add("ExceptionMessage", report.ExceptionMessage);
            payload.Headers.Add("ExceptionText", report.ExceptionText);
            payload.Headers.Add("ExceptionType", report.ExceptionType);
            return payload;
        }

        public static DateTime ExecutionTime(this Message message)
        {
            return message.ToEnvelope().ExecutionTime.Value;
        }
    }
}
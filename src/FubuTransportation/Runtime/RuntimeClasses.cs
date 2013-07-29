﻿using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FubuTransportation.Runtime
{
    
    public interface IMessageSerializer
    {
        void Serialize(object message, Stream stream);
        object Deserialize(Stream message);
    }

    public interface IMessageCallback
    {
        void MarkSuccessful();
        void MarkFailed();
    }

    
    // Wanna make ITransport as stupid as possible

    // Will use message invoker, but IReceiver will also be responsible for 
    // other coordination with the EventAggregator, sending replies, and logging
    public interface IReceiver
    {
        void Receive(ITransport transport, Envelope envelope);
    }

    // THinking that this thing internally will have a bunch of little IRouterRules
    public interface IRouter
    {
        ITransport SelectTransport(Envelope envelope);
    }


    /// <summary>
    /// Models a queue of outgoing messages as a result of the current message so you don't even try to 
    /// send replies until the original message succeeds
    /// Plus giving you the ability to set the correlation identifiers
    /// </summary>
    public interface IOutgoingMessages : IEnumerable<object>
    {
        void Enqueue(object message);
    }

    public class OutgoingMessages : IOutgoingMessages
    {
        private readonly IList<object> _messages = new List<object>();

        // TODO -- needs to track the originating message in order to do the request/replay semantics

        public void Enqueue(object message)
        {
            var enumerable = message as IEnumerable<object>;
            if (enumerable == null)
            {
                _messages.Add(message);
            }
            else
            {
                _messages.AddRange(enumerable);
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
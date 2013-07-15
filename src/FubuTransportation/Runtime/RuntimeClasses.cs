using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;

namespace FubuTransportation.Runtime
{
    
    // Copied straight from RSB for now
    public interface IMessageSerializer
    {
        void Serialize(object[] messages, Stream message);

        object[] Deserialize(Stream message);
    }

    
    // Wanna make ITransport as stupid as possible
    public interface ITransport : IDisposable
    {
        Uri Id { get; } // Really for identification

        // Envelope might have a reference to its parent
        void Send(Envelope envelope);

        void StartReceiving(IReceiver receiver);
    }

    public interface IMessageInvoker
    {
        void Invoke(Envelope envelope);
    }

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
        void Enqueue(params object[] messages);
        void Enqueue(IEnumerable messages);
    }

    public class OutgoingMessages : IOutgoingMessages
    {
        private readonly IList<object> _messages = new List<object>();

        // TODO -- needs to track the originating message in order to do the request/replay semantics

        public void Enqueue(params object[] messages)
        {
            _messages.AddRange(messages);
        }

        public void Enqueue(IEnumerable messages)
        {
            _messages.Each(x => _messages.Add(x));
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

    public class RequestReplayActionInvoker<TAction, TInput, TOutput> : BasicBehavior
        where TInput : class
        where TOutput : class
    {
        private readonly Func<TAction, TInput, TOutput> _action;
        private readonly TAction _controller;
        private readonly IFubuRequest _request;

        public RequestReplayActionInvoker(IFubuRequest request, TAction controller,
                                        Func<TAction, TInput, TOutput> action)
            : base(PartialBehavior.Executes)
        {
            _request = request;
            _controller = controller;
            _action = action;
        }

        protected override DoNext performInvoke()
        {
            var input = _request.Get<TInput>();
            TOutput output = _action(_controller, input);
            

            throw new NotImplementedException("Need to publish the response right back");

            return DoNext.Continue;
        }
    }

    public class CascadingActionInvoker<TAction, TInput> : BasicBehavior
    {
        private readonly IFubuRequest _request;
        private readonly Func<TAction, TInput, IEnumerable<object>> _action;

        public CascadingActionInvoker(IFubuRequest request, Func<TAction, TInput, IEnumerable<object>> action) : base(PartialBehavior.Executes)
        {
            _request = request;
            _action = action;
        }

        protected override DoNext performInvoke()
        {
            throw new NotImplementedException("get input, run action, publish all");
        }
    }
}
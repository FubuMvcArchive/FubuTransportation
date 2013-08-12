using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FubuCore;
using System.Linq;

namespace FubuTransportation
{

    public class EventAggregator : IEventAggregator
    {
        private readonly List<object> _listeners = new List<object>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public EventAggregator(IEnumerable<IListener> listeners)
        {
            _listeners.AddRange(listeners);
        }

        public void SendMessage<T>(T message)
        {
            // TODO -- Harden this and add logging
            Task.Factory.StartNew(() => {
                var listeners = _lock.Read(() => _listeners.OfType<IListener<T>>().ToArray());
                
                // TODO -- error handling in each so they can fail independently
                listeners.Each(x => x.Handle(message));
            });
        }

        public void SendMessage<T>() where T : new()
        {
            SendMessage(new T());
        }

        public void AddListener(object listener)
        {
            _lock.Write(() => _listeners.Fill(listener));
        }

        public void RemoveListener(object listener)
        {
            _lock.Write(() => _listeners.Remove(listener));
        }

        public IEnumerable<object> Listeners
        {
            get { return _lock.Read(() => _listeners.ToArray()); }
        }

        public void AddListeners(params object[] listeners)
        {
            _lock.Write(() => _listeners.Fill(listeners));
        }

        public bool HasListener(object listener)
        {
            return _lock.Read(() => _listeners.Contains(listener));
        }

        public void RemoveAllListeners()
        {
            _lock.Write(() => _listeners.Clear());
        }
    }
}
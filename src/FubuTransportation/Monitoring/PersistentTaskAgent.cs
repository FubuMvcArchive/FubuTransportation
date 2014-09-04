using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubuTransportation.Monitoring
{
    // Error handling and logging is handled in PersistentTaskController
    public interface IPersistentTaskAgent
    {
        Uri Subject { get; }
        Task<ITransportPeer> AssignOwner(IEnumerable<ITransportPeer> peers);
    }

    public class PersistentTaskAgent : IDisposable, IPersistentTaskAgent
    {
        private readonly IPersistentTask _task;

        private readonly BlockingCollection<Action<IPersistentTask>> _actions
            = new BlockingCollection<Action<IPersistentTask>>(new ConcurrentBag<Action<IPersistentTask>>());

        private readonly Task _background;
        private readonly CancellationTokenSource _cancellation;

        public PersistentTaskAgent(IPersistentTask task)
        {
            _cancellation = new CancellationTokenSource();
            _task = task;
            _background = Task.Factory.StartNew(performActions, _cancellation.Token, TaskCreationOptions.LongRunning);
        }

        public Uri Subject
        {
            get { return _task.Subject; }
        }

        public Task AssertAvailable()
        {
            return Enqueue(t => t.AssertAvailable());
        }

        public Task Activate()
        {
            return Enqueue(t => t.Activate());
        }

        public Task Deactivate()
        {
            return Enqueue(t => t.Deactivate());
        }

        public bool IsActive
        {
            get { return _task.IsActive; }
        }

        public Task<ITransportPeer> AssignOwner(IEnumerable<ITransportPeer> peers)
        {
            return Enqueue(task => task.SelectOwner(peers)).Unwrap();

        }

        public Task Enqueue(Action<IPersistentTask> action)
        {
            var completion = new TaskCompletionSource<object>(TaskCreationOptions.AttachedToParent);
            _actions.Add(t => {
                try
                {
                    action(t);
                    completion.SetResult(new object());
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            });

            return completion.Task;
        }

        public Task<T> Enqueue<T>(Func<IPersistentTask, T> source)
        {
            var completion = new TaskCompletionSource<T>();
            _actions.Add(t => {
                try
                {
                    completion.SetResult(source(t));
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            });

            return completion.Task;
        }

        private void performActions(object state)
        {
            foreach (var action in _actions.GetConsumingEnumerable())
            {
                if (!_cancellation.IsCancellationRequested) action(_task);
            }
        }

        public void Dispose()
        {
            _cancellation.Cancel();
            _actions.CompleteAdding();
            _background.Dispose();
        }
    }
}
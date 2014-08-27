using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FubuCore.Util;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public interface ITransportPeerRepository
    {
        bool HasAnyPeers();

        ITransportPeer FindOwner(Uri subject);
        IEnumerable<ITransportPeer> AllPeers();
        IEnumerable<ITransportPeer> AllOwners(); 
    }

    public interface ITaskOwnershipPersistence
    {
        IEnumerable<TaskOwner> All(string nodeName);
        void PersistOwnership(Uri subject, TransportNode node);
    }

    public interface ITransportPeer
    {
        Task<OwnershipStatus> TakeOwnership(Uri subject);
        Task<TaskHealthResponse> CheckStatusOfOwnedTasks();

        IEnumerable<Uri> CurrentlyOwnedSubjects();
        void RemoveOwnership(IEnumerable<Uri> subjects);
        void RemoveOwnership(Uri subject);

        string NodeId { get; }
        string MachineName { get; }
        IEnumerable<Uri> ReplyAddresses { get; }
    }


    // Need to or queue up requests by subject? ReaderWriterLock by subject?
    // Use an agent for each subject that uses a producer/consumer task
    public class PersistentTaskController
    {
        private readonly ITransportPeerRepository _repository;
        private readonly IEnumerable<IPersistentTaskSource> _sources;

        private readonly ConcurrentCache<Uri, PersistentTaskAgent> _tasks =
            new ConcurrentCache<Uri, PersistentTaskAgent>();

        public PersistentTaskController(ITransportPeerRepository repository, IEnumerable<IPersistentTaskSource> sources)
        {
            _repository = repository;
            _sources = sources;

            _tasks.OnMissing = uri => new PersistentTaskAgent(FindTask(uri));
        }

        public void Activate()
        {
            // find all active startup tasks, and try to take ownership of each
        }

        public IPersistentTask FindTask(Uri subject)
        {
            throw new NotImplementedException();
        }

        public void PerformHealthChecks()
        {
            // hit all owners 
        }

        public Task<OwnershipStatus> TakeOwnership(Uri subject)
        {
            // 1., check if there is an owner already
            // 2., check if this node knows about the task
            // 3., try to start
            //    a.) If successful, log ownership
            //    b.) If failed, send failure


            throw new NotImplementedException();
        }

        public Task<HealthStatus> CheckStatus(Uri subject)
        {
            // active, inactive, error, unknown

            throw new NotImplementedException();
        }

        /*
         * TODO
         * 1.) start up everything, look for immediate tasks, if owned, check the owner.  If not owned, start the election
         * 2.) Try to take ownership
         * 
         */
    }



    public class PersistentTaskAgent : IDisposable
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

        public void Enqueue(Action<IPersistentTask> action)
        {
            _actions.Add(action);
        }

        public Task<T> Enqueue<T>(Func<IPersistentTask, T> source)
        {
            var completion = new TaskCompletionSource<T>();
            Enqueue(t => {
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









    public interface IPersistentTask
    {
        Uri Subject { get; }
        void AssertAvailable();
        void Activate();
        void Deactivate();
        void IsActive();

        ITransportPeer AssignOwner(IEnumerable<ITransportPeer> peers);
    }

    

    public interface IPersistentTaskSource
    {
        string Protocol { get; }
        IEnumerable<Uri> PermanentTasks();

        IPersistentTask CreateTask(Uri uri);
    }

    // Don't think this is quite right
    public interface ITaskAssignmentStrategy
    {
        TransportNode AssignOwner(Uri subject, IServiceBus serviceBus, IEnumerable<TransportNode> node);
    }
}
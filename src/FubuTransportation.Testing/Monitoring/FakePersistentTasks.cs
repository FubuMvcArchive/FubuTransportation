using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FubuCore;
using FubuCore.Util;
using FubuTransportation.Monitoring;
using NUnit.Framework;

namespace FubuTransportation.Testing.Monitoring
{
    public class FakePersistentTaskSource : IPersistentTaskSource
    {
        private readonly Cache<string, FakePersistentTask> _tasks 
            = new Cache<string, FakePersistentTask>(); 

        public FakePersistentTaskSource(string protocol)
        {
            Protocol = protocol;
            _tasks.OnMissing = name => {
                var uri = "{0}://{1}".ToFormat(Protocol, name).ToUri();
                return new FakePersistentTask(uri);
            }; 
        }

        public FakePersistentTask this[string name]
        {
            get
            {
                return _tasks[name];
            }
        }

        public string Protocol { get; private set; }
        public IEnumerable<Uri> PermanentTasks()
        {
            return _tasks.Select(x => x.Subject);
        }

        public IPersistentTask CreateTask(Uri uri)
        {
            var name = uri.Host;

            return _tasks.Has(name) ? _tasks[name] : null;

        }

        public FakePersistentTask AddTask(string key)
        {
            return _tasks[key];
        }
    }

    public class FakePersistentTask : IPersistentTask
    {
        public Exception ActivationException = null;
        public Exception AssertAvailableException = null;
        public Exception DeactivateException = null;

        public FakePersistentTask(Uri subject)
        {
            Subject = subject;
        }

        public void IsFullyFunctional()
        {
            ActivationException = AssertAvailableException = null;
        }

        public Uri Subject { get; private set; }
        public void AssertAvailable()
        {
            Thread.Sleep(10);
            if (AssertAvailableException != null) throw AssertAvailableException;
        
        
        }

        public void Activate()
        {
            Thread.Sleep(10);
            if (ActivationException != null) throw ActivationException;

            IsActive = true;
        }

        public void Deactivate()
        {
            Thread.Sleep(10);
            if (DeactivateException != null) throw DeactivateException;

            IsActive = false;
        }

        public bool IsActive
        {
            get; set;
        }

        public Task<ITransportPeer> SelectOwner(IEnumerable<ITransportPeer> peers)
        {
            throw new NotImplementedException();
        }

        public void IsFullyFunctionalAndActive()
        {
            IsFullyFunctional();
            IsActive = true;
        }

        public void IsActiveButNotFunctional(Exception exception)
        {
            IsActive = true;
            AssertAvailableException = exception;
        }
    }
}
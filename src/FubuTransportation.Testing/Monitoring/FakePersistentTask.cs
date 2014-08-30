using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FubuTransportation.Monitoring;

namespace FubuTransportation.Testing.Monitoring
{
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

        public bool IsActive { get; set; }

        public Task<ITransportPeer> SelectOwner(IEnumerable<ITransportPeer> peers)
        {
            // TODO -- need to make this thing be attached to a parent
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

        public void IsNotActive()
        {
            IsActive = false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FubuTransportation.Monitoring;
using FubuTransportation.Subscriptions;
using StoryTeller.Assertions;

namespace FubuTransportation.Storyteller.Fixtures.Monitoring
{
    public class FakePersistentTask : IPersistentTask
    {
        private IEnumerable<string> _preferredNodes = new string[0];
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

        public IEnumerable<string> PreferredNodes
        {
            get { return _preferredNodes; }
            set { _preferredNodes = value; }
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
            var ordered = _preferredNodes.Select(x => peers.FirstOrDefault(_ => _.NodeId == x))
                .Where(x => x != null);

            StoryTellerAssert.Fail(!ordered.Any(), "No preferred nodes established for this test node");

            var assignment = new OrderedAssignment(Subject, ordered);

            return assignment.SelectOwner();
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

        public void SetState(string state, ISubscriptionPersistence persistence, string nodeId)
        {
            switch (state)
            {
                case MonitoredNode.HealthyAndFunctional:
                    IsFullyFunctionalAndActive();
                    persistence.Alter(nodeId, node => node.AddOwnership(Subject));

                    break;

                case MonitoredNode.ThrowsExceptionOnStartupOrHealthCheck:
                    throw new NotImplementedException();
                    break;

                case MonitoredNode.TimesOutOnStartupOrHealthCheck:
                    throw new NotImplementedException();
                    break;

                case MonitoredNode.IsInactive:
                    throw new NotImplementedException();
                    break;
            }
        }
    }
}
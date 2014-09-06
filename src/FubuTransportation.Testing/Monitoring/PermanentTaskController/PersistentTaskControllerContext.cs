using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Logging;
using FubuCore.Util;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.ErrorHandling;
using FubuTransportation.Monitoring;
using FubuTransportation.Subscriptions;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Monitoring.PermanentTaskController
{
    [TestFixture]
    public abstract class PersistentTaskControllerContext : ITransportPeerRepository
    {
        protected RecordingLogger theLogger;
        private Lazy<PersistentTaskController> _controller; 

        protected readonly Cache<string, ITransportPeer> peers =
            new Cache<string, ITransportPeer>(name => MockRepository.GenerateMock<ITransportPeer>());

        protected readonly Cache<string, FakePersistentTaskSource> sources = 
            new Cache<string, FakePersistentTaskSource>(protocol => new FakePersistentTaskSource(protocol));

        protected TransportNode theCurrentNode;
        protected ChannelGraph theGraph;

        [SetUp]
        public void SetUp()
        {
            theCurrentNode = new TransportNode
            {
                
            };

            peers.ClearAll();
            sources.ClearAll();

            theGraph = new ChannelGraph
            {
                NodeId = "Test@Local"
            };
            theLogger = new RecordingLogger();

            _controller = new Lazy<PersistentTaskController>(() => {
                var controller = new PersistentTaskController(theGraph, theLogger, this, sources, new HealthMonitoringSettings
                {
                    TaskAvailabilityCheckTimeout = 5.Seconds()
                });

                sources.SelectMany(x => x.FakeTasks()).Select(x => x.Subject)
                    .Each(subject => controller.FindAgent(subject));

                return controller;
            });

            theContextIs();
        }

        protected PersistentTaskController theController
        {
            get
            {
                return _controller.Value;
            }
        }

        protected virtual void theContextIs()
        {
            
        }

        IEnumerable<ITransportPeer> ITransportPeerRepository.AllPeers()
        {
            return peers;
        }

        IEnumerable<ITransportPeer> ITransportPeerRepository.AllOwners()
        {
            return peers.Where(x => x.CurrentlyOwnedSubjects().Any());
        }

        public FakePersistentTask Task(string uriString)
        {
            var uri = uriString.ToUri();

            return sources[uri.Scheme][uri.Host];
        }

        protected void LoggedMessageForSubject<T>(string uriString) where T : PersistentTaskMessage
        {
            var hasIt = theLogger.InfoMessages.OfType<T>().Any(x => x.Subject == uriString.ToUri());
            if (!hasIt)
            {
                Assert.Fail("Did not have expected log message of type {0} for subject {1}".ToFormat(typeof(T).Name, uriString));
            }
        }

        protected void AssertTasksAreActive(params string[] uriStrings)
        {
            var inactive = uriStrings.Select(Task).Where(x => !x.IsActive).Select(x => x.Subject.ToString());

            if (inactive.Any())
            {
                Assert.Fail("Tasks {0} have not been activated", inactive.Join(", "));
            }
        }

        protected void TheOwnedTasksByTheCurrentNodeShouldBe(params string[] uriStrings)
        {
            theCurrentNode.OwnedTasks.OrderBy(x => x.ToString())
                .ShouldHaveTheSameElementsAs(uriStrings.OrderBy(x => x).Select(x => x.ToUri()));
        }

        protected void ExceptionWasLogged(Exception ex)
        {
            theLogger.ErrorMessages.OfType<ErrorReport>().Any(x => x.ExceptionText.Contains(ex.ToString()));
        }

        void ITransportPeerRepository.RecordOwnershipToThisNode(Uri subject)
        {
            theCurrentNode.AddOwnership(subject);
        }

        void ITransportPeerRepository.RecordOwnershipToThisNode(IEnumerable<Uri> subjects)
        {
            theCurrentNode.AddOwnership(subjects);
        }

        public TransportNode LocalNode()
        {
            return theCurrentNode;
        }

        void ITransportPeerRepository.RemoveOwnershipFromThisNode(Uri subject)
        {
            theCurrentNode.RemoveOwnership(subject);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Logging;
using FubuCore.Util;
using FubuTestingSupport;
using FubuTransportation.Monitoring;
using NUnit.Framework;

namespace FubuTransportation.Testing.Monitoring.HealthAndAssignmentRouter
{
    [TestFixture]
    public class HealthAndAssignmentRouterTester
    {
        private readonly Uri subject1 = "subject://1".ToUri();
        private readonly Uri subject2 = "subject://2".ToUri();
        private readonly Uri subject3 = "subject://3".ToUri();
        private readonly Uri subject4 = "subject://4".ToUri();

        private readonly Cache<Uri, FakeTransportPeer> ExpectedOwnership
            = new Cache<Uri, FakeTransportPeer>();

        private FakePersistentTasks theTasks;
        private FakeTransportPeer peer1;
        private FakeTransportPeer peer2;
        private FakeTransportPeer peer3;
        private FakeTransportPeer peer4;
        private FubuTransportation.Monitoring.HealthAndAssignmentRouter theRouter;
        private FakeTransportPeer[] peers;

        [SetUp]
        public void SetUp()
        {
            peer1 = new FakeTransportPeer("Peer1", Environment.MachineName, "control://1".ToUri());
            peer2 = new FakeTransportPeer("Peer2", Environment.MachineName, "control://2".ToUri());
            peer3 = new FakeTransportPeer("Peer3", Environment.MachineName, "control://3".ToUri());
            peer4 = new FakeTransportPeer("Peer4", Environment.MachineName, "control://4".ToUri());

            ExpectedOwnership.ClearAll();

            theTasks = new FakePersistentTasks();
            peers = new FakeTransportPeer[] {peer1, peer2, peer3, peer4};
            theRouter = new FubuTransportation.Monitoring.HealthAndAssignmentRouter(new RecordingLogger(), theTasks, peers);
        }

    }
}
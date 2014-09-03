using System;
using StoryTeller;
using StoryTeller.Engine;

namespace FubuTransportation.Storyteller.Fixtures.Monitoring
{
    public class MonitoringFixture : Fixture
    {
        private MonitoredNodeGroup _nodes;

        public MonitoringFixture()
        {
            Title = "Health Monitoring, Failover, and Task Assignment";

            this["Context"] = Embed<MonitoringSetupFixture>("If the nodes and tasks are");


        }

        public override void SetUp(ITestContext context)
        {
            _nodes = new MonitoredNodeGroup();
            context.Store(context);
        }

        public override void TearDown()
        {
            _nodes.Dispose();
        }

        [ExposeAsTable("If the task state is")]
        public void TaskStateIs(
            Uri Task, 
            string Node, 
            [SelectionValues(MonitoredNode.HealthyAndFunctional, MonitoredNode.TimesOutOnStartupOrHealthCheck, MonitoredNode.ThrowsExceptionOnStartupOrHealthCheck, MonitoredNode.IsInactive)]string State)
        {
            _nodes.SetTaskState(Task, Node, State);
        }

        [FormatAs("After the health checks run on all nodes")]
        public void AfterTheHealthChecksRunOnAllNodes()
        {
            _nodes.WaitForAllHealthChecks();
        }

        [FormatAs("Node {Node} drops offline")]
        public void NodeDropsOffline(string Node)
        {
            _nodes.ShutdownNode(Node);
        }

        public IGrammar TheTaskAssignmentsShouldBe()
        {
            return VerifySetOf(() => _nodes.AssignedTasks())
                .Titled("The task assignments should be")
                .MatchOn(x => x.Task, x => x.Node);
        }

        public IGrammar ThePersistedAssignmentsShouldBe()
        {
            return VerifySetOf(() => _nodes.PersistedTasks())
                .Titled("The persisted task assignments should be")
                .MatchOn(x => x.Task, x => x.Node);
        }
    }
}
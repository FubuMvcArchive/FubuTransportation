using System;

namespace FubuTransportation.Testing.TestSupport
{
    public class HandledByStep : IScenarioStep
    {
        public HandledByStep(Message message, NodeConfiguration node)
        {
        }

        public void PreviewAct(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void PreviewAssert(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Act(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Assert(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public bool MatchesMessage(MessageProcessed processed)
        {
            throw new NotImplementedException();
        }
    }
}
namespace FubuTransportation.Testing.TestSupport
{
    public interface IScenarioStep
    {
        void PreviewAct(IScenarioWriter writer);
        void PreviewAssert(IScenarioWriter writer);

        void Act(IScenarioWriter writer);
        void Assert(IScenarioWriter writer);

        bool MatchesMessage(MessageProcessed processed);
    }
}
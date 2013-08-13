using FubuTestingSupport;
using NUnit.Framework;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class HandlerExtensionsTester
    {
        [Test]
        public void matches_saga_convention_when_suffix_is_saga_and_state_property_and_iscompleted_property_exists()
        {
            typeof(SuccessMatchesTestSaga).MatchesSagaConvention().ShouldBeTrue();
        }

        [Test]
        public void does_not_match_if_state_is_missing()
        {
            typeof(MissingStateSaga).MatchesSagaConvention().ShouldBeFalse();
        }

        [Test]
        public void does_not_match_if_iscompleted_is_missing()
        {
            typeof(MissingCompletedSaga).MatchesSagaConvention().ShouldBeFalse();
        }

        [Test]
        public void does_not_match_if_suffix_is_not_Saga()
        {
            typeof(NoSagaSuffix).MatchesSagaConvention().ShouldBeFalse();
        }
    }

    public class MissingCompletedSaga
    {
        public SuccessSagaState State { get; set; }
    }

    public class MissingStateSaga
    {
        public bool IsCompleted { get; set; }
    }

    public class SuccessMatchesTestSaga
    {
        public bool IsCompleted { get; set; }
        public SuccessSagaState State { get; set; }
    }

    public class NoSagaSuffix
    {
        public bool IsCompleted { get; set; }
        public SuccessSagaState State { get; set; }
    }

    public class SuccessSagaState
    {
    }
}
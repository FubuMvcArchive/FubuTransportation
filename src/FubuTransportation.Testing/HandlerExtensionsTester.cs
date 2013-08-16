using FubuTestingSupport;
using NUnit.Framework;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class HandlerExtensionsTester
    {

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
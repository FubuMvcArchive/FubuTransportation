using System;
using System.Collections.Generic;
using FubuTransportation.Sagas;
using NUnit.Framework;

namespace FubuTransportation.Testing.Sagas
{
    [TestFixture]
    public class SagaIntegrationTester
    {

    }

    public class SagaLogger
    {
        public readonly IList<SagaTrace> Traces = new List<SagaTrace>();

        public void Trace(Guid guid, string message)
        {
            Traces.Add(new SagaTrace
            {
                Id = guid,
                Message = message
            });
        }

        public class SagaTrace
        {
            public Guid Id { get; set; }
            public string Message { get; set; }
        }
    }

    public class TestSagaState
    {
        public Guid Id { get; set; }
        public int Accessed { get; set; }

        public override string ToString()
        {
            return string.Format("Accessed {0} times");
        }

    }

    public class TestSagaHandler : IStatefulSaga<TestSagaState>
    {
        private readonly SagaLogger _logger;
        private bool _isCompleted;


        public TestSagaHandler(SagaLogger logger)
        {
            _logger = logger;
        }

        public TestSagaState State { get; set; }
        public bool IsCompleted()
        {
            return _isCompleted;
        }
    }

    public class TestSagaStart
    {
        
    }

    public class TestSagaAction
    {
        public Guid CorrelationId { get; set; }
    }


}

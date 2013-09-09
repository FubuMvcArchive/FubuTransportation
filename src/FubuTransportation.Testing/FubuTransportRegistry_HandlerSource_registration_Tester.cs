using System.Linq;
using FubuTestingSupport;
using FubuTransportation.Configuration;
using FubuTransportation.Registration.Nodes;
using FubuTransportation.Testing.Events;
using NUnit.Framework;

namespace FubuTransportation.Testing
{
    [TestFixture]
    public class FubuTransportRegistry_HandlerSource_registration_Tester
    {
        [Test]
        public void can_register_a_handler_source_by_explicit_config()
        {
            var graph = FubuTransportRegistry.HandlerGraphFor(x => {
                x.Handlers.FindBy(source => {
                    source.UseThisAssembly();
                    source.IncludeTypesNamed(name => name.Contains("FooHandler"));
                });
            });

            graph.SelectMany(x => x.OfType<HandlerCall>()).Select(x => x.HandlerType).OrderBy(x => x.Name)
                .ShouldHaveTheSameElementsAs(typeof(MyFooHandler), typeof(MyOtherFooHandler));

        }
    }

    public class MyFooHandler
    {
        public void Handle(Message1 message1){}
    }

    public class MyOtherFooHandler
    {
        public void Handle(Message2 message2){}
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
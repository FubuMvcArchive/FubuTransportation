using FubuTransportation.Configuration;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class HandlerGraphTester
    {
        private HandlerGraph theGraph;
        private HandlerCall concreteCall;
        private HandlerCall concreteCall2;
        private HandlerCall concreteCall3;
        private HandlerCall concreteCall4;

        [SetUp]
        public void SetUp()
        {
            theGraph = new HandlerGraph();

            concreteCall = HandlerCall.For<ConcreteHandler>(x => x.M1(null));

            concreteCall.Clone().ShouldEqual(concreteCall);


            concreteCall2 = HandlerCall.For<ConcreteHandler>(x => x.M2(null));
            concreteCall3 = HandlerCall.For<ConcreteHandler>(x => x.M3(null));
            concreteCall4 = HandlerCall.For<ConcreteHandler>(x => x.M4(null));
        }

        [Test]
        public void add_a_handler_for_a_concrete_class_creates_a_new_chain()
        {
            theGraph.Add(concreteCall);

            var call = theGraph.ChainFor(typeof (Input)).OfType<HandlerCall>().Single();
            call.ShouldEqual(concreteCall);
            call.ShouldNotBeTheSameAs(concreteCall);
        }

        [Test]
        public void add_a_second_handler_for_a_concrete_class_appends_to_the_chain()
        {
            theGraph.Add(concreteCall);
            theGraph.Add(concreteCall2);
            theGraph.Add(concreteCall3);

            theGraph.ShouldHaveCount(1);

            var calls = theGraph.ChainFor(typeof (Input)).OfType<HandlerCall>().ToArray();
            calls.ShouldHaveCount(3);
            var firstCall = calls.ElementAt(0).ShouldBeOfType<HandlerCall>();

            firstCall.Equals(concreteCall).ShouldBeTrue();

            calls.ElementAt(0).ShouldNotBeTheSameAs(concreteCall);

            calls.ElementAt(1).Equals(concreteCall2).ShouldBeTrue();
            calls.ElementAt(1).ShouldNotBeTheSameAs(concreteCall2);

        }

        [Test]
        public void add_a_different_input_type_adds_a_second_chain()
        {
            theGraph.Add(concreteCall);
            theGraph.Add(concreteCall2);
            theGraph.Add(concreteCall4);
            theGraph.Add(concreteCall3);

            theGraph.ShouldHaveCount(2);

            theGraph.Select(x => x.InputType())
                .ShouldHaveTheSameElementsAs(typeof(Input), typeof(DifferentInput));
        }

        [Test]
        public void interfaces_are_applied_correctly()
        {
            var general = HandlerCall.For<ConcreteHandler>(x => x.General(null));
            var specific1 = HandlerCall.For<ConcreteHandler>(x => x.Specific1(null));
            var specific2 = HandlerCall.For<ConcreteHandler>(x => x.Specific2(null));
        
            theGraph.Add(general);
            theGraph.Add(specific1);
            theGraph.Add(specific2);

            theGraph.ShouldHaveCount(2);
            theGraph.ApplyGeneralizedHandlers();

            theGraph.ShouldHaveCount(2);

            theGraph.ChainFor(typeof (Concrete1)).Last().ShouldBeOfType<HandlerCall>()
                    .InputType().ShouldEqual(typeof (IMessage));

            theGraph.ChainFor(typeof(Concrete2)).Last().ShouldBeOfType<HandlerCall>()
                    .InputType().ShouldEqual(typeof(IMessage));
        }

        [Test]
        public void base_class_handlers_are_applied_correctly()
        {
            var baseHandler = HandlerCall.For<ConcreteHandler>(x => x.Base(null));
            var derivedHandler = HandlerCall.For<ConcreteHandler>(x => x.Derived(null));

            theGraph.Add(baseHandler);
            theGraph.Add(derivedHandler);

            theGraph.ApplyGeneralizedHandlers();

            theGraph.ShouldHaveCount(1);

            theGraph.ChainFor(typeof(DerivedMessage)).Last().ShouldBeOfType<HandlerCall>()
                .Equals(baseHandler).ShouldBeTrue();
        }
    }

    public class ConcreteHandler
    {
        public void M1(Input input){}
        public void M2(Input input){}
        public void M3(Input input){}

        public void M4(DifferentInput input)
        {
        }

        public void General(IMessage input){}
        public void Specific1(Concrete1 input){}
        public void Specific2(Concrete2 input){}

        public void Base(BaseMesage message){}
        public void Derived(DerivedMessage message){}
    }

    public interface IMessage{}
    public class Concrete1 : IMessage{}
    public class Concrete2 : IMessage{}

    public abstract class BaseMesage
    {
    }

    public class DerivedMessage : BaseMesage{}
}
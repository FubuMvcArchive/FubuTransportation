using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class EventAggregatorTester
    {
        private EventAggregator events;
        private StubMessage1Handler handler;

        [SetUp]
        public void SetUp()
        {
            events = new EventAggregator();

            handler = new StubMessage1Handler();
            events.AddListener(handler);
        }

        [Test]
        public void simple_handlers_registered()
        {
            var theMessage = new Message1();
            events.SendMessage(theMessage);

            Wait.Until(() => handler.Message != null);

            handler.Message.ShouldBeTheSameAs(theMessage);
        }

        [Test]
        public void send_message_to_all_listeners_for_a_message_type()
        {
            var listener1 = new StubListener<Message1>();
            var listener2 = new StubListener<Message1>();
            var listener3 = new StubListener<Message1>();
            var listener4 = new StubListener<Message2>();

            events.AddListeners(listener1, listener2, listener3, this, listener4);

            var message1 = new Message1();
            var message2 = new Message2();

            events.SendMessage(message1);
            events.SendMessage(message2);

            Wait.Until(() => listener1.LastMessage != null && listener2.LastMessage != null && listener3.LastMessage != null && listener4.LastMessage != null);

            listener1.LastMessage.ShouldBeTheSameAs(message1);
            listener2.LastMessage.ShouldBeTheSameAs(message1);
            listener3.LastMessage.ShouldBeTheSameAs(message1);

            listener4.LastMessage.ShouldBeTheSameAs(message2);
        }

        [Test]
        public void exposes_all_the_listeners()
        {
            var listener1 = new StubListener<Message1>();
            var listener2 = new StubListener<Message1>();
            var listener3 = new StubListener<Message1>();
            var listener4 = new StubListener<Message2>();

            events.AddListeners(listener1, listener2, listener3, this, listener4);

            events.Listeners.ShouldHaveTheSameElementsAs(handler,listener1, listener2, listener3, this, listener4);

        }

        [Test]
        public void send_message_that_creates_on_the_fly()
        {
            var listener1 = new StubListener<Message1>();
            events.AddListener(listener1);

            events.SendMessage<Message1>();

            Wait.Until(() => listener1.LastMessage != null);
            

            listener1.LastMessage.ShouldBeOfType<Message1>();
        }

        [Test]
        public void remove_listener()
        {
            var listener1 = new StubListener<Message1>();
            var listener2 = new StubListener<Message1>();
            var listener3 = new StubListener<Message1>();
            var listener4 = new StubListener<Message2>();

            var listener5 = MockRepository.GenerateMock<IListener<Message1>>();
            events.AddListeners(listener1, listener2, listener3, this, listener4, listener5);

            var message1 = new Message1();


            events.RemoveListener(listener5);
            events.SendMessage(message1);

            listener5.AssertWasNotCalled(x => x.Handle(message1));
        }
    }

    public class StubListener<T> : IListener<T>
    {
        public T LastMessage { get; set; }

        #region IListener<T> Members

        public void Handle(T message)
        {
            LastMessage = message;
        }

        #endregion
    }

    public class Message1
    {
    }

    public class Message2
    {
    }

    public interface IMessageHandler1
    {
        void HandleMessage(Message1 message);
    }

    public interface IMessageHandler2
    {
        void HandleMessage(Message2 message);
    }


    public class StubMessage1Handler : IListener<Message1>
    {
        public Message1 Message { get; set; }

        #region IListener<Message1> Members

        public void Handle(Message1 message)
        {
            Message = message;
        }

        #endregion
    }
}
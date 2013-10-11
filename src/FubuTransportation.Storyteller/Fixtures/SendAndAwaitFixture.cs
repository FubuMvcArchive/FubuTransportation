using System.Threading.Tasks;
using Serenity.Fixtures;
using ServiceNode;
using StoryTeller.Assertions;
using StoryTeller.Engine;

namespace FubuTransportation.Storyteller.Fixtures
{
    public class SendAndAwaitFixture : Fixture
    {
        private Task _task;

        public SendAndAwaitFixture()
        {
            Title = "Send and Await an Acknowledgement";
        }

        [FormatAs("Send a message that we expect to succeed and wait for the ack")]
        public void SendMessageSuccessfully()
        {
            _task = Retrieve<IServiceBus>().SendAndWait(new SimpleMessage());
        }

        [FormatAs("Send a message that will fail with an AmbiguousMatchException exception")]
        public void SendMessageUnsuccessfully()
        {
            _task = Retrieve<IServiceBus>().SendAndWait(new ServiceNode.ErrorMessage());
        }

        [FormatAs("The acknowledgement was received within {seconds} seconds")]
        public bool AckIsReceived(int seconds)
        {
            return _task.Wait(seconds.Seconds());
        }

        [FormatAs("The acknowledgement was successful")]
        public bool AckWasSuccessful()
        {
            StoryTellerAssert.Fail(_task.Exception != null, _task.Exception.ToString());

            return true;
        }

        [FormatAs("The acknowledgment failed and contained the message {message}")]
        public bool TheAckFailedWithMessage(string message)
        {
            StoryTellerAssert.Fail(_task.Exception == null, "The task exception is null");

            StoryTellerAssert.Fail(!_task.Exception.ToString().Contains(message), "The actual exception text was:\n" + _task.Exception.ToString());

            return true;
        }
    }
}
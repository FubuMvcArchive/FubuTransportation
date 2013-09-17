using FubuCore.Logging;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Invocation;
using FubuTransportation.Testing.Events;
using FubuTransportation.Testing.ScenarioSupport;
using Rhino.Mocks;

namespace FubuTransportation.Testing
{
    public static class ObjectMother
    {
         public static Envelope Envelope()
         {
             return new Envelope
             {
                 Data = new byte[] { 1, 2, 3, 4 },
                 Callback = MockRepository.GenerateMock<IMessageCallback>()
             };
         }

        public static InvocationContext InvocationContext()
        {
            var envelope = Envelope();
            envelope.Message = new Message();

            return new InvocationContext(envelope);
        }
    }
}
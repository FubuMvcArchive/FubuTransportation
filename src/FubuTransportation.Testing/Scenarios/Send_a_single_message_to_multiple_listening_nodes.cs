using FubuTransportation.Testing.TestSupport;

namespace FubuTransportation.Testing.Scenarios
{
    public class Send_a_single_message_to_multiple_listening_nodes : Scenario
    {
        public Send_a_single_message_to_multiple_listening_nodes()
        {
            Website1.Registry.Channel(x => x.Service1)
                    .PublishesMessage<OneMessage>();

            Website1.Registry.Channel(x => x.Service3)
                    .PublishesMessage<OneMessage>();


            Service1.Handles<OneMessage>();
            Service2.Handles<OneMessage>();
            Service3.Handles<OneMessage>();
            Service4.Handles<OneMessage>();

            Send<OneMessage>("first message")
                .ShouldBeReceivedBy(Service1)
                .ShouldBeReceivedBy(Service3);
        }
    }
}
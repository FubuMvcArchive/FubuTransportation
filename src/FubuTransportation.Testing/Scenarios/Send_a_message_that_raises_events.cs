using FubuTransportation.Testing.TestSupport;

namespace FubuTransportation.Testing.Scenarios
{
    public class Send_a_message_that_raises_events : Scenario
    {
        public Send_a_message_that_raises_events()
        {
            Website1.Registry.Channel(x => x.Service1)
                    .PublishesMessage<OneMessage>();

            Service1.Handles<OneMessage>()
                .Raises<TwoMessage>()
                .Raises<ThreeMessage>();

            // Assuming that if the events raised can be handled locally, they are
            // handled here. Corey/Ryan to review
            Website1.Sends<OneMessage>("original message")
                .ShouldBeReceivedBy(Service1)
                .MatchingMessageIsReceivedBy<TwoMessage>(Service1)
                .MatchingMessageIsReceivedBy<ThreeMessage>(Service1);
        }
    }
}
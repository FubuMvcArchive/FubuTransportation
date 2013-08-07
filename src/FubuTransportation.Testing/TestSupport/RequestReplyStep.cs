using System;

namespace FubuTransportation.Testing.TestSupport
{
    public class RequestReplyStep<TRequest, TReply> : IScenarioStep
    {
        private readonly string _description;
        private readonly NodeConfiguration _sender;
        private readonly NodeConfiguration _receiver;

        public RequestReplyStep(string description, NodeConfiguration sender, NodeConfiguration receiver)
        {
            _description = description;
            _sender = sender;
            _receiver = receiver;
        }

        public void PreviewAct(IScenarioWriter writer)
        {
            writer.WriteLine("Node {0} sends request '{1}' ({2}), expecting a matching response {3}", _sender.Name, _description, typeof(TRequest).Name, typeof(TReply).Name);
        }

        public void PreviewAssert(IScenarioWriter writer)
        {
            writer.WriteLine("Expecting a reply of type {0} from node {1}", typeof(TReply).Name, _receiver.Name);
        }

        public void Act(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Assert(IScenarioWriter writer)
        {
            throw new NotImplementedException();
        }

        public bool MatchesMessage(MessageProcessed processed)
        {
            throw new NotImplementedException();
        }
    }
}
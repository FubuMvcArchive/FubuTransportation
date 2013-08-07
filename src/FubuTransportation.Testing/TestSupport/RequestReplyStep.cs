using System;
using System.Threading.Tasks;

namespace FubuTransportation.Testing.TestSupport
{
    public class RequestReplyStep<TRequest, TReply> : IScenarioStep where TRequest : Message, new() where TReply : Message
    {
        private readonly string _description;
        private readonly NodeConfiguration _sender;
        private readonly NodeConfiguration _receiver;
        private Task<TReply> _completion;
        private TRequest _request;

        public RequestReplyStep(string description, NodeConfiguration sender, NodeConfiguration receiver)
        {
            _description = description;
            _sender = sender;
            _receiver = receiver;

            _request = new TRequest();
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
            _completion = _sender.ServiceBus.Request<TRequest, TReply>(_request);
        }

        public void Assert(IScenarioWriter writer)
        {
            var response = _completion.Result;

            if (response == null)
            {
                writer.Failure("Did not get any response!");
            }

            if (response.Id != _request.Id)
            {
                writer.Failure("Response does not match the request");
            }
        }

        public bool MatchesMessage(MessageProcessed processed)
        {
            throw new NotImplementedException();
        }
    }
}
using System.Collections.Generic;
using FubuCore;

namespace FubuTransportation.Testing.TestSupport
{
    public interface IOriginatingMessage
    {
        Message Message { get; }
        string Description { get; }
    }

    public class SendMessageStep<T> : IScenarioStep, IOriginatingMessage where T : Message, new()
    {
        private readonly NodeConfiguration _sender;
        private readonly string _description;
        
        public IList<NodeConfiguration> ReceivingNodes = new List<NodeConfiguration>();

        public SendMessageStep(NodeConfiguration sender, string description = null)
        {
            _sender = sender;
            _description = description;

            Message = new T();
        }

        public void PreviewAct(IScenarioWriter writer)
        {
            if (_description.IsEmpty())
            {
                writer.WriteLine("Node {0} sends new message of type {1}", _sender.Name, typeof (T).Name);
            }
            else
            {
                writer.WriteLine("Node {0} sends new message of type {1} ({2})", _sender.Name, typeof(T).Name, _description);
            }

            
        }

        public void PreviewAssert(IScenarioWriter writer)
        {
            ReceivingNodes.Each(receiver => {
                if (_description.IsEmpty())
                {
                    writer.WriteLine("Message of type {0} should be received by {1}", typeof(T).Name, receiver.Name);
                }
                else
                {
                    writer.WriteLine("Message of type {0} ({1}) should be received by {2}", typeof(T).Name, _description, receiver.Name);
                }
            });



        }

        public void Act(IScenarioWriter writer)
        {
            PreviewAct(writer);
            throw new System.NotImplementedException();
        }

        public void Assert(IScenarioWriter writer)
        {
            throw new System.NotImplementedException();
        }

        public bool MatchesMessage(MessageProcessed processed)
        {
            throw new System.NotImplementedException();
        }

        public Message Message { get; private set; }
        public string Description
        {
            get { return _description; }
        }
    }
}
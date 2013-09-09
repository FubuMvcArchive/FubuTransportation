using System;
using FubuMVC.Core.Registration.Nodes;

namespace FubuTransportation.Publishing
{
    public class PublishEvent : Process
    {
        private readonly Type _eventType;

        public PublishEvent(ActionCall transform) : base(typeof(EventPublisher<>).MakeGenericType(transform.ResourceType()))
        {
            _eventType = transform.ResourceType();
        }

        public Type EventType
        {
            get { return _eventType; }
        }
    }
}
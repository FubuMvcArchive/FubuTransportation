namespace FubuTransportation.Testing.ScenarioSupport
{
    public class MessageProcessed
    {
        public string Description { get; set; }
        public Message Message { get; set; }

        public static MessageProcessed For<T>(Message message)
        {
            return new MessageProcessed
            {
                Description = typeof (T).Name,
                Message = message
            };
        }

        protected bool Equals(MessageProcessed other)
        {
            return string.Equals(Description, other.Description) && Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MessageProcessed) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Description != null ? Description.GetHashCode() : 0)*397) ^
                       (Message != null ? Message.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} / {1}", Message, Description);
        }
    }
}
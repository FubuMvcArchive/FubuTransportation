using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using FubuTransportation.Runtime;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime
{
    public class GenericHandler : SimpleHandler<Message>{}

    public class OneMessage : Message{}

    public class OneHandler : SimpleHandler<OneMessage>{}
    public class AnotherOneHandler : SimpleHandler<OneMessage>{}
    public class DifferentOneHandler : SimpleHandler<OneMessage>{}

    public class TwoMessage : Message{}
    public class TwoHandler : SimpleHandler<TwoMessage> { }

    public class ThreeMessage : Message { }
    public class ThreeHandler : SimpleHandler<ThreeMessage> { }

    public class FourMessage : Message { }
    public class FourHandler : SimpleHandler<FourMessage> { }


    public static class TestMessageRecorder
    {
        private readonly static IList<MessageProcessed> _processed = new List<MessageProcessed>();

        public static void Clear()
        {
            _processed.Clear();
        }

        public static void ShouldMatch<THandler>(this MessageProcessed processed, Message message)
        {
            processed.Description.ShouldEqual(typeof (THandler).Name);
            processed.Message.ShouldEqual(message);
        }

        public static void Processed(string description, Message message)
        {
            _processed.Add(new MessageProcessed
            {
                Description = description,
                Message = message
            });
        }

        public static IEnumerable<MessageProcessed> ProcessedFor<T>() where T : Message
        {
            return _processed.Where(x => x.Message is T);
        } 

        public static MessageProcessed[] AllProcessed
        {
            get
            {
                return _processed.ToArray();
            }
        } 
    }

    public class SimpleHandler<T>  where T : Message
    {
        public void Handle(T message)
        {
            TestMessageRecorder.Processed(GetType().Name, message);
        }
    }

    public class RequestResponseHandler<T> where T : Message
    {
        public MirrorMessage<T> Handle(T message)
        {
            TestMessageRecorder.Processed(GetType().Name, message);
            return new MirrorMessage<T>{Id = message.Id};
        }
    }

    public class ManyResponseHandler<T, TR1, TR2, TR3> where T : Message, new() where TR1 : Message, new() where TR2 : Message, new() where TR3 : Message, new()
    {
        public IEnumerable<object> Handle(T message)
        {
            TestMessageRecorder.Processed(GetType().Name, message);

            yield return new TR1 {Id = message.Id};
            yield return new TR2 {Id = message.Id};
            yield return new TR3 {Id = message.Id};
        }
    }

    public class MessageProcessed
    {
        public string Description { get; set; }
        public Message Message { get; set; }

        public static MessageProcessed For<T>(Message message)
        {
            return new MessageProcessed
            {
                Description = typeof(T).Name, 
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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MessageProcessed)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Description != null ? Description.GetHashCode() : 0) * 397) ^ (Message != null ? Message.GetHashCode() : 0);
            }
        }
    }

    public class Message
    {
        public Message()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        protected bool Equals(Message other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Message)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    public class MirrorMessage<T>
    {
        public Guid Id { get; set; }

        protected bool Equals(MirrorMessage<T> other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MirrorMessage<T>)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }


}
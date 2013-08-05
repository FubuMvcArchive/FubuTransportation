using System.Collections.Generic;
using System.Linq;
using FubuTestingSupport;

namespace FubuTransportation.Testing.TestSupport
{
    public static class TestMessageRecorder
    {
        private static readonly IList<MessageProcessed> _processed = new List<MessageProcessed>();

        public static MessageProcessed[] AllProcessed
        {
            get { return _processed.ToArray(); }
        }

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
    }
}
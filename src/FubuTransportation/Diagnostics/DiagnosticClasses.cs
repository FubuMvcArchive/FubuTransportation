using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using HtmlTags;

namespace FubuTransportation.Diagnostics
{
    public abstract class MessageRecordNode
    {
        public DateTime Timestamp = DateTime.Now; // Yes, use local time because it's meant to be read by humans

        public abstract HtmlTag ToLeafTag();
    }

    public class MessageRecord : MessageRecordNode
    {
        public Guid Id;
        public string Message;
        public string Node;
        public Guid ParentId;
        public string Type;

        public override HtmlTag ToLeafTag()
        {
            return new HtmlTag("li").Text("{0}: {1}".ToFormat(Node, Message));
        }
    }

    public class MessageHistory : MessageRecordNode
    {
        private readonly IList<MessageHistory> _children = new List<MessageHistory>();
        private readonly IList<MessageRecord> _records = new List<MessageRecord>(); 

        public Guid Id { get; set; }
        public Type Type { get; set; }

        public string Description
        {
            get { return "Message {0} ({1})".ToFormat(Id, Type); }
        }

        public IEnumerable<MessageRecord> Records()
        {
            return _records.OrderBy(x => x.Timestamp);
        }

        public void AddChild(MessageHistory child)
        {
            _children.Fill(child);
        }

        protected bool Equals(MessageHistory other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MessageHistory) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        private IEnumerable<MessageRecordNode> allRecordNodes()
        {
            foreach (var child in _children)
            {
                yield return child;
            }

            foreach (var record in _records)
            {
                yield return record;
            }
        } 

        public override HtmlTag ToLeafTag()
        {
            var tag = new HtmlTag("li").Text(Description);
            var root = tag.Add("ol");

            allRecordNodes()
                .OrderBy(x => x.Timestamp)
                .Select(x => x.ToLeafTag())
                .Each(x => root.Append(x));

            return tag;
        }

        public HtmlTag ToNodeTag()
        {
            return new HtmlTag("ol", x => x.Append(ToLeafTag()));
        }
    }

    public class MessageHistoryTableTag : TableTag
    {
        public MessageHistoryTableTag(MessageHistory history)
        {
            AddHeaderRow(tr => { tr.Header().Attr("colspan", "3"); });

            AddHeaderRow(tr => {
                tr.Header("Node");
                tr.Header("Timestamp");
                tr.Header("Message");
            });

            history.Records().Each(rec => {
                AddBodyRow(tr => {
                    tr.Cell(rec.Node);
                    tr.Cell(rec.Timestamp.ToLongTimeString());
                    tr.Cell(rec.Message);
                });
            });
        }
    }


}
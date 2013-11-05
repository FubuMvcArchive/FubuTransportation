using System.Collections.Generic;
using System.Linq;
using FubuCore.Descriptions;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using FubuTransportation.Runtime.Serializers;
using HtmlTags;

namespace FubuTransportation.Diagnostics.Visualization
{
    public class ChannelGraphFubuDiagnostics
    {
        private readonly ChannelGraph _graph;
        private readonly IEnumerable<ITransport> _transports;
        private readonly IEnumerable<IMessageSerializer> _serializers;

        public ChannelGraphFubuDiagnostics(ChannelGraph graph, IEnumerable<ITransport> transports, IEnumerable<IMessageSerializer> serializers)
        {
            _graph = graph;
            _transports = transports;
            _serializers = serializers;
        }

        [System.ComponentModel.Description("Transports and Channels:Visualizes the active model of all the channels and transports in this application")]
        public ChannelVisualization get_channels()
        {
            return new ChannelVisualization
            {
                Graph = _graph,
                Transports = new TransportsTag(_transports),
                Channels = new ChannelsTableTag(_graph),
                Serializers = new SerializersTag(_serializers)
            };            
        }
    }

    public class SerializersTag : TableTag
    {
        public SerializersTag(IEnumerable<IMessageSerializer> serializers)
        {
            AddClass("table");

            serializers.Each(x => {
                var description = Description.For(x);
                AddBodyRow(row =>
                {
                    row.Cell(description.Title).AddClass("title");
                    row.Cell().AddClass("description").Text(description.ShortDescription);
                });
            });
        }
    }
}
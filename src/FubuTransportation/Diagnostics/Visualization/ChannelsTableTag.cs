using System.Collections.Generic;
using System.Linq;
using FubuCore.Descriptions;
using FubuTransportation.Configuration;
using HtmlTags;

namespace FubuTransportation.Diagnostics.Visualization
{
    public class ChannelsTableTag : TableTag
    {
        public ChannelsTableTag(ChannelGraph graph)
        {
            AddClass("table");
            AddClass("channels");

            AddHeaderRow(row => {
                row.Header("Description");
                row.Header("Incoming Scheduler");
                row.Header("Routing Rules");
            });

            graph.OrderBy(x => x.Key).Each(channel => {
                AddBodyRow(row => addRow(row, channel));
            });
        }

        private void addRow(TableRowTag row, ChannelNode channel)
        {
            addDescriptionCell(row, channel);

            addSchedulers(row, channel);

            addRoutingRules(row, channel);
        }

        private static void addRoutingRules(TableRowTag row, ChannelNode channel)
        {
            var cell = row.Cell().AddClass("routing-rules");
            if (channel.Rules.Any())
            {
                cell.Add("ul", ul => { channel.Rules.Each(x => ul.Add("li").Text(x.Describe())); });
            }
            else
            {
                cell.Text("None");
            }
        }

        private static void addSchedulers(TableRowTag row, ChannelNode channel)
        {
            var cell = row.Cell();
            if (channel.Incoming)
            {
                var description = Description.For(channel.Scheduler);
                cell.Append(new DescriptionBodyTag(description));
            }
            else
            {
                cell.Text("None");
            }
        }

        private static void addDescriptionCell(TableRowTag row, ChannelNode channel)
        {
            var cell = row.Cell();
            cell.Add("h5").Text(channel.Key);
            cell.Add("div/i").Text(channel.Uri.ToString());
            if (channel.DefaultContentType != null)
                cell.Add("div").Text("Default Content Type: " + channel.DefaultContentType);
        }
    }
}
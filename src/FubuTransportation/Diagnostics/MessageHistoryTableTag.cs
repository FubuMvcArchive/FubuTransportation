using System.Collections.Generic;
using HtmlTags;

namespace FubuTransportation.Diagnostics
{
    public class MessageHistoryTableTag : TableTag
    {
        public MessageHistoryTableTag(MessageHistory history)
        {
            AddHeaderRow(tr => { tr.Header().Attr("colspan", "4"); });

            AddHeaderRow(tr => {
                tr.Header("Node");
                tr.Header("Timestamp");
                tr.Header("Message");
                tr.Header("Headers");
            });

            history.Records().Each(rec => {
                AddBodyRow(tr => {
                    tr.Cell(rec.Node);
                    tr.Cell(rec.Timestamp.ToLongTimeString());
                    tr.Cell(rec.Message);
                    tr.Cell(rec.Headers);
                });

                if (FubuCore.StringExtensions.IsNotEmpty(rec.ExceptionText))
                {
                    AddBodyRow(tr => {
                        var cell = tr.Cell();
                        cell.Attr("colspan", "4");


                        cell.Add("pre").Text(rec.ExceptionText).Style("background-color", "#FFFFAA");
                    });
                }
            });
        }
    }
}
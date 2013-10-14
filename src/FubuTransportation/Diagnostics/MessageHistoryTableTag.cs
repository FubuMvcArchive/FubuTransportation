using System.Collections.Generic;
using System.Linq;
using HtmlTags;

namespace FubuTransportation.Diagnostics
{
    public class MessageHistoryTableTag : TableTag
    {
        public MessageHistoryTableTag(MessageHistory history)
        {
            Style("width", "inherit");
            Style("margin-top", "50px");

            AddHeaderRow(tr => {
                tr.Header(history.Description).Attr("colspan", "5").Style("text-align", "left");
            });

            AddHeaderRow(tr => {
                tr.Header("Node").Style("text-align", "left");
                tr.Header("Timestamp").Style("text-align", "left");
                tr.Header("Message").Style("text-align", "left").Style("width", "200px");
                tr.Header("Header = ").Style("text-align", "right");
                tr.Header("Value").Style("text-align", "left");
            });

           

            history.Records().Each(rec => {
                var headers = rec.Headers.Split(';').ToArray();
                

                AddBodyRow(tr => {
                    tr.Cell(rec.Node).Attr("valign", "top").Style("padding-right", "30px");
                    tr.Cell(rec.Timestamp.ToLongTimeString()).Style("padding-right", "30px").Attr("valign", "top").Attr("nowrap", "true");
                    tr.Cell(rec.Message).Attr("valign", "top").Style("padding-right", "30px");
                    
                    if (headers.Any())
                    {
                        var count = headers.Count().ToString();
                        tr.Children.Each(x => x.Attr("rowspan", count));

                        string headerValue = headers.First();
                        writeHeaderValue(headerValue, tr);
                    }
                });

                for (int i = 1; i < headers.Count(); i++)
                {
                    AddBodyRow(tr => writeHeaderValue(headers[i], tr));
                }

                if (FubuCore.StringExtensions.IsNotEmpty(rec.ExceptionText))
                {
                    AddBodyRow(tr => {
                        var cell = tr.Cell();
                        cell.Attr("colspan", "5");


                        cell.Add("pre").Text(rec.ExceptionText).Style("background-color", "#FFFFAA");
                    });
                }
            });
        }

        private static void writeHeaderValue(string headerValue, TableRowTag tr)
        {
            var parts = headerValue.Split('=');
            tr.Cell(parts.First() + " = ").Style("text-align", "right");
            tr.Cell(parts.Last());
        }
    }
}
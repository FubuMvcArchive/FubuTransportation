using System.Collections.Generic;
using System.Collections.Specialized;
using FubuTransportation.Configuration;
using FubuTransportation.Runtime;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class Headers_testing
    {
        [Test]
        public void get_and_set_with_name_value_collection()
        {
            var values = new NameValueCollection();
            values["a"] = "1";
            values["b"] = "2";

            var headers = new NameValueHeaders(values);
            headers["a"].ShouldEqual("1");
            headers["b"].ShouldEqual("2");

            headers["c"] = "3";

            values["c"].ShouldEqual("3");
        }

        [Test]
        public void get_keys_from_name_value_collection()
        {
            var values = new NameValueCollection();
            values["a"] = "1";
            values["b"] = "2";
            values["c"] = "3";

            var headers = new NameValueHeaders(values);
        
            headers.Keys().ShouldHaveTheSameElementsAs("a", "b", "c");
        }

        [Test]
        public void get_and_set_with_dictionary()
        {
            var values = new Dictionary<string, string>{{"a", "1"}, {"b", "2"}};

            var headers = new DictionaryHeaders(values);
            headers["a"].ShouldEqual("1");
            headers["b"].ShouldEqual("2");

            headers["c"] = "3";

            values["c"].ShouldEqual("3");
        }

        [Test]
        public void to_name_values_for_dictionary()
        {
            var values = new Dictionary<string, string> { { "a", "1" }, { "b", "2" } };

            var headers = new DictionaryHeaders(values);

            var collection = headers.ToNameValues();
            collection["a"].ShouldEqual("1");
            collection["b"].ShouldEqual("2");
        }

        [Test]
        public void get_keys_from_dictionary()
        {
            var values = new Dictionary<string, string> { { "a", "1" }, { "b", "2" }, {"c", "3"} };

            var headers = new DictionaryHeaders(values);

            headers.Keys().ShouldHaveTheSameElementsAs("a", "b", "c");
        }

        [Test]
        public void copy_headers_for_a_channel_node()
        {
            var headers = new NameValueHeaders();
            headers["a"] = "1";
            headers["b"] = "2";
            headers["c"] = "3";

            var node = new ChannelNode
            {
                Key = "SomeKey",
                Uri = "foo://bar".ToUri()
            };

            var clone = headers.CloneForSource(node);
            clone["a"].ShouldEqual("1");
            clone["b"].ShouldEqual("2");
            clone["c"].ShouldEqual("3");
            clone[Envelope.SourceKey].ShouldEqual(node.Uri.ToString());
            clone[Envelope.ChannelKey].ShouldEqual(node.Key);
        }
    }
}
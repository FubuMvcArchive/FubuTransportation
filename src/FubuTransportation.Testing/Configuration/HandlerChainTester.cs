using FubuTransportation.Configuration;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Configuration
{
    [TestFixture]
    public class HandlerChainTester
    {
        [Test]
        public void url_category_is_handler()
        {
            var chain = new HandlerChain();
            chain.UrlCategory.Category.ShouldEqual("Handler");
        }

        [Test]
        public void sets_itself_as_partial_only()
        {
            var chain = new HandlerChain();
            chain.IsPartialOnly.ShouldBeTrue();
        }
    }
}
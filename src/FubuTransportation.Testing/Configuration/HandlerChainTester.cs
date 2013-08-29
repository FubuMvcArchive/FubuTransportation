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

        [Test]
        public void the_default_number_of_maximum_attempts_is_1()
        {
            new HandlerChain().MaximumAttempts.ShouldEqual(1);
        }
    }
}
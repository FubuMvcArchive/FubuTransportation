using System;

namespace FubuTransportation.Serenity.Samples.Setup
{
    public class MultipleEndpointsSettings
    {
        public Uri AnotherService { get; set; }
        public Uri Client { get; set; }

        public Uri SystemUnderTest
        {
            // A restriction with using the ExternalNodes, we have to hard-code the URI 
            // for the actual system under test in order for subscriptions to work.
            //get { return new Uri("memory://test/systemundertest"); }
            get;
            set;
        }
    }
}
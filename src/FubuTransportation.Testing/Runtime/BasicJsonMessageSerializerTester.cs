using System;
using System.Diagnostics;
using System.IO;
using FubuTransportation.Runtime;
using NUnit.Framework;
using FubuCore;
using FubuTestingSupport;

namespace FubuTransportation.Testing.Runtime
{
    [TestFixture]
    public class BasicJsonMessageSerializerTester
    {
        [Test]
        public void can_round_trip()
        {
            var address1 = new Address
            {
                City = "Austin",
                State = "Texas"
            };

            var stream = new MemoryStream();
            var serializer = new BasicJsonMessageSerializer();
            serializer.Serialize(address1, stream);

            stream.Position = 0;

            var address2 = serializer.Deserialize(stream).ShouldBeOfType<Address>();
            address1.ShouldEqual(address2);
        }
    }

    [Serializable]
    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }

        protected bool Equals(Address other)
        {
            return string.Equals(City, other.City) && string.Equals(State, other.State);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Address) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((City != null ? City.GetHashCode() : 0)*397) ^ (State != null ? State.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("City: {0}, State: {1}", City, State);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Util;
using FubuTransportation.Subscriptions;

namespace FubuTransportation.Monitoring
{
    public class MemoryTaskOwnershipPersistence : ITaskOwnershipPersistence
    {
        private readonly Cache<string, NodeTaskOwner> _owners = new Cache<string, NodeTaskOwner>(name => new NodeTaskOwner(name)); 

        public IEnumerable<TaskOwner> All(string nodeName)
        {
            return _owners[nodeName].Owners();
        }

        public void PersistOwnership(Uri subject, TransportNode node)
        {
            _owners[node.NodeName][subject] = node.Id;
        }

        public class NodeTaskOwner
        {
            private readonly Cache<Uri, string> _ownership = new Cache<Uri, string>();
            private readonly string _node;

            public NodeTaskOwner(string node)
            {
                _node = node;
            }

            public string Node
            {
                get { return _node; }
            }

            public string this[Uri uri]
            {
                get
                {
                    return _ownership[uri];
                }
                set
                {
                    _ownership[uri] = value;
                }
            }

            public IEnumerable<TaskOwner> Owners()
            {
                return _ownership.ToDictionary().Select(x => new TaskOwner
                {
                    Id = x.Key,
                    Owner = x.Value
                });
            } 

            
        }
    }
}
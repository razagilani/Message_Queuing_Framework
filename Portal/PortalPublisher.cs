using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageQueue;

namespace Portal
{
    public class PortalPublisher : Publisher
    {
        public PortalPublisher(Dictionary<string, List<string>> config)
            : base("localhost", config)
        {

        }
    }
}

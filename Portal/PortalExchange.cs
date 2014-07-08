using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MessageQueue;
using System.Windows.Forms;

namespace Portal
{
    class PortalExchange : MessageQueue.Exchange
    {
        public PortalExchange(Dictionary<string, List<Dictionary<string, string>>> config, string name, Form form)
            : base("localhost", config, "Portal", name, (object)form)
        {
            Console.WriteLine("PortalExchange constructor called");
        }
    }
}

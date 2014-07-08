using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MessageQueue;
using System.Windows.Forms;

namespace Altitude
{
    class AltitudeExchange : MessageQueue.Exchange
    {
        public AltitudeExchange(Dictionary<string, List<Dictionary<string, string>>> config, string name, Form form)
            : base("localhost", config, "Altitude", name, (object) form)
        {
            Console.WriteLine("AltitudeExchange constructor called");
        }
    }
}

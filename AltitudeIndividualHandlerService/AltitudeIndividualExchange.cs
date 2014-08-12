using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MessageQueue;

namespace AltitudeIndividualHandlerService
{
    class AltitudeIndividualExchange : MessageQueue.Exchange
    {
        public AltitudeIndividualExchange(Dictionary<string, List<Dictionary<string, string>>> config, string name, object obj)
            : base("localhost", config, "AltitudeIndividualExchange", name, obj)
        {
            Console.WriteLine("IndividualExchange constructor called");
        }
    }
}

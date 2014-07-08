using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageQueue;

namespace Altitude
{
    public class AltitudePublisher : Publisher
    {
        
        public AltitudePublisher(Dictionary<string, List<string>> config)
            : base("localhost", config)
        {

        }
    }
}

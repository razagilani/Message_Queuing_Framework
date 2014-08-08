using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MessageQueue;

namespace AltitudeAccountExchange
{
    class AccountExchange: MessageQueue.Exchange
    {
        public AccountExchange(Dictionary<string,List<Dictionary<string, string>>> config, string name, object obj): base("localhost", config, "AltitudeAccountExchange", name, obj)
        {
            Console.WriteLine("IndividualExchange constructor called");
        }
        static void Main(string[] args)
        {
            try
            {
                RoutingKey r = RoutingKey.Settings;
                Dictionary<string, List<Dictionary<string, string>>> config = new Dictionary<string, List<Dictionary<string, string>>>();
                List<Dictionary<string,string>> inner = new List<Dictionary<string, string>>();
                

                inner.Add(new Dictionary<string,string>(){{"redeliver_attempts",r.Parameters[0].redeliverAttempts}} );
                inner.Add(new Dictionary<string, string>() { { "redeliver_delay", r.Parameters[0].redeliverDelay } });
                inner.Add(new Dictionary<string, string>() { { "redeliver_attempts", r.Parameters[0].manualAck } });
                //inner.Add(r.Parameters[0].redeliverAttempts, r.Parameters[0].redeliverDelay, r.Parameters[0].manualAck);
                config.Add(r.Parameters[0].Name, inner);
                object obj = new Object();
                Exchange e = new AccountExchange(config, "xbill", obj);
            }
            catch (Exception x)
            {
                Console.WriteLine(x);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;
using MessageQueue;
using System.Threading;


namespace ExamplePublisher
{
    public class ExamplePublisher:Publisher
    {
        private Publisher pub;
        public ExamplePublisher(Dictionary<string, List<string>> config): base("localhost", config)
        {

        }

        static void Main(string[] args)
        {
            var exchangeConfig = ConfigurationManager.GetSection("ExchangeConfig") as MyConfigSection;
            Dictionary<string, List<string>> conf = new Dictionary<string, List<string>>();
            List<string> temp = new List<string>();

            foreach(var e in exchangeConfig.Instances.AsEnumerable()) 
            {
                Console.WriteLine(e.Name);
                temp.Add(e.Name);
            }
            conf.Add("exchanges", temp);
            temp = null;
            temp = new List<string>();
            
            var routingConfig = ConfigurationManager.GetSection("RoutingConfig") as MyConfigSection;
            foreach (var e in routingConfig.Instances.AsEnumerable())
            {
                Console.WriteLine(e.Name);
                temp.Add(e.Name);
            }
            conf.Add("routing_keys", temp);
            Publisher pub = new Publisher("localhost",conf);
            while (true)
            {
                Console.WriteLine("Enter Message");
                string text = Console.ReadLine();
                Dictionary <string, string> msg = new Dictionary<string,string>();
                msg.Add("message", text);
                string message = JsonConvert.SerializeObject(msg);
                pub.publish(message);
            }
        }
        public void sendData(int id, string first_name, string last_name, string city, string state, string phone)
        {
            string message = Convert.ToString(id) + " " + first_name + " " + last_name + " " + city + " " + state + " " + phone;
            Dictionary<string, string> msg = new Dictionary<string, string>();
            msg.Add("customer", message);
            pub.publish(JsonConvert.SerializeObject(msg));

        }
    }
}

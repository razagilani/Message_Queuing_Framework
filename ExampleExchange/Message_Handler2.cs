using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using MessageQueue;

namespace AltitudeAccountExchange
{
    public class Message_Handler2: MessageHandler
    {
        public Message_Handler2(IModel channel, string queue, List<Dictionary<string, string>> conf,
           IModel delayChannel, string delayQueue)
            : base(channel, queue, conf, delayChannel, delayQueue)
        {
        }

        public override Dictionary<string, string> handle(MessageQueue.Message message)
        {
            Console.WriteLine("ExampleHandler2 handling message {0}: {1}",
            message.CorrelationId, message.Body);
            Random rnd = new Random();
            Thread.Sleep(rnd.Next(0, 5));
            Console.WriteLine("Return");
            Dictionary<string,string> response = new Dictionary<string, string>();
            response.Add("message", String.Format("This is an Altitude Handler  response to message {0}",
                message.CorrelationId));
            return response;
        }
    }
}

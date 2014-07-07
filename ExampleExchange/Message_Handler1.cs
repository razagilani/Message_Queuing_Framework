using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using MessageQueue;

namespace ExampleExchange
{
    public class Message_Handler1: MessageHandler
    {

        public Message_Handler1(IModel channel, string queue, List<Dictionary<string, string>> conf,
           IModel delayChannel, string delayQueue)
            : base(channel, queue, conf, delayChannel, delayQueue)
        {
        }

        public override Dictionary<string,string> handle(Message message)
        {
            Random rnd = new Random();
            int accept = rnd.Next(0, 9);

            if (accept > 6)
            {
                Console.WriteLine("ExampleHandler1 handling message {0}: {1}",
                    message.CorrelationId, message.Body);
                Thread.Sleep(rnd.Next(0, 5));
                Console.WriteLine("Return");
                message.ack();
                Dictionary<string, string> response = new Dictionary<string, string>();

                response.Add("message", String.Format("This is a Account Handler  response to " +
                   "message {0}", message.CorrelationId));
                return response;
            }
            else
            {
                Console.WriteLine("Got no time for the message, will delay");
                message.ack();
                message.delay();
                return new Dictionary<string, string>() { { "", "" } };

            }
        
        }
    }
}

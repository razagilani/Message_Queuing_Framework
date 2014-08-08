using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using MessageQueue;

namespace PortalExchange
{
    public class PortalMessageHandler: MessageHandler
    {

        public PortalMessageHandler(IModel channel, string queue, List<Dictionary<string, string>> conf,
           IModel delayChannel, string delayQueue, bool manual_ack, Object obj)
            : base(channel, queue, conf, delayChannel, delayQueue)
        {
        }

        public override Dictionary<string,string> handle(Message message)
        {
            
                Console.WriteLine("PortalMessageHandler handling message {0}: {1}",
                    message.CorrelationId, message.Body);
                message.ack();
                Dictionary<string, string> response = new Dictionary<string, string>();

                response.Add("message", String.Format("This is a Account Handler  response to " +
                   "message {0}", message.CorrelationId));
                return response;
           
        
        }
    }
}

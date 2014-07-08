using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageQueue;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace Altitude
{
    public class AltitudeMessageHandler : MessageHandler
    {
        private Form1 form;
        public AltitudeMessageHandler(IModel channel, string queue, List<Dictionary<string, string>> conf,
           IModel delayChannel, string delayQueue, bool manual_ack, Form form)
            : base(channel, queue, conf, delayChannel, delayQueue)
        {
            this.form = (Form1)form;
        }

        public override Dictionary<string, string> handle(MessageQueue.Message message)
        {
            string data = JsonConvert.SerializeObject(message.Body);
            string[] raw_fields = data.Split(':');
            string[] fields = raw_fields[1].Split(',');
            Console.WriteLine(fields[0].Length);
            Console.WriteLine(fields[0]);
            string id = fields[0].Remove(0, 2);
            this.form.add_row(Convert.ToInt16(id), fields[1], fields[2], fields[3],
                fields[4], fields[5]);

            message.ack();
            Dictionary<string, string> response = new Dictionary<string, string>();

            response.Add("message", String.Format("This is a Account Handler  response to " +
                   "message {0}", message.CorrelationId));
            return response;

        }
    }
}

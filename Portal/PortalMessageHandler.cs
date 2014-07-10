using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageQueue;
using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace Portal
{
    public class PortalMessageHandler : MessageHandler
    {
        private Form1 form;
        public PortalMessageHandler(IModel channel, string queue, List<Dictionary<string, string>> conf,
           IModel delayChannel, string delayQueue, bool manual_ack, Form form)
            : base(channel, queue, conf, delayChannel, delayQueue)
        {
            this.form = (Form1)form;
        }

        public override Dictionary<string, string> handle(MessageQueue.Message message)
        {
            Dictionary<string,string> data = JsonConvert.DeserializeObject<Dictionary<string,string>>(message.Body);
            //this.form.add_row(Convert.ToInt16(data["ID"]), data["first_name"], data["last_name"], data["city]", data["state"], data["phone"]);
            this.form.add_row(Convert.ToInt16(data["ID"]), data["first_name"], data["last_name"], data["city"], data["state"], data["phone"]);
            message.ack();
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("message", String.Format("This is a Account Handler  response to " +
                   "message {0}", message.CorrelationId));
            return response;
        }
    }
}

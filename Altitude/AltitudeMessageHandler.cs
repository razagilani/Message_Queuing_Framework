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
            dynamic data = JsonConvert.DeserializeObject(message.Body);
            this.form.add_row(Convert.ToInt16(data.ID.Value), data.first_name.Value, data.last_name.Value,
                data.city.Value, data.state.Value, data.phone.Value);
            message.ack();
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("message", String.Format("This is a Account Handler  response to " +
                   "message {0}", message.CorrelationId));
            return response;

        }
    }
}

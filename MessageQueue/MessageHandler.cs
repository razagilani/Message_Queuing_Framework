using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Threading;

namespace MessageQueue
{
    public class MessageHandler : QueueingBasicConsumer
    {
        private bool manual_ack;
        private IModel channel;
        private string queue;
        private List<Dictionary<string, string>> conf;
        private IModel delayChannel;
        private string delayQueue;
        private int redeliverAttempts;
        private int redeliverDelay;
        private Dictionary<string, int> unacked_msgs;
        public MessageHandler(IModel channel)
            : base(channel)
        {
            this.channel = channel;
        }
        public MessageHandler(IModel channel, string queue, List<Dictionary<string, string>> conf,
            IModel delayChannel, string delayQueue):base(channel)
        {
            
            this.channel = channel;
            this.queue = queue;
            this.conf = conf;
            this.delayChannel = delayChannel;
            this.delayQueue = delayQueue;
            this.manual_ack = Convert.ToBoolean(conf[2].Values.First());
            /*if (!conf.ContainsKey("redeliverAttempts") || !conf.ContainsKey("redeliverDelay"))
                throw new ImproperlyConfiguredException(String.Format("Handler configuration for route {0} " +
                                       "did not specify a redeliver_delay or " +
                                       "redeliver_attempts", this.queue));*/
            this.redeliverAttempts = Convert.ToInt16(conf[0].Values.First());
            this.redeliverDelay = Convert.ToInt16(conf[1].Values.First());
            this.unacked_msgs = new Dictionary<string, int>();
        }

        public override void HandleBasicDeliver(string consumerTag,
                                                ulong deliveryTag,
                                                bool redelivered,
                                                string exchange,
                                                string routingKey,
                                                IBasicProperties properties,
                                                byte[] body)
        {
            Dictionary<string, string> response = new Dictionary<string, string>();
            bool error_occured;
            BasicDeliverEventArgs e = new BasicDeliverEventArgs();
            e.ConsumerTag = consumerTag;
            e.DeliveryTag = deliveryTag;
            e.Redelivered = redelivered;
            e.Exchange = exchange;
            e.RoutingKey = routingKey;
            e.BasicProperties = properties;
            e.Body = body;
            Message msg = Message.createFromChannel(this.channel, redelivered, deliveryTag,
                routingKey, properties, body, this.delayChannel, this.delayQueue);
            
            // If the message was not acknowledged by the handler multiple times
            // then the handler should reject the message
            if (this.manual_ack && this.unacked_msgs.ContainsKey(msg.Id))
            {
                 if (Convert.ToInt16(this.unacked_msgs[msg.Id]) > this.redeliverAttempts)
                 {
                     msg.reject();
                     this.unacked_msgs.Remove(msg.Id);
                     return;
                 }
            }
            try
            {
                response = this.handle(msg);
                error_occured = false;
            }
            catch (Exception ex)
            {
                error_occured = true;
                Dictionary<string, string> resp = new Dictionary<string,string>();
                resp.Add("message", ex.Message);
                resp.Add("classname", ex.Source);
            }
            /* Keep track of the message if we're manually ack'ing and the message
            wasn't acked, so we can reject it later if neccessary */
            if (this.manual_ack)
            {
                if (!msg.Acked)
                {
                    if (this.unacked_msgs.ContainsKey(msg.Id))
                    {
                        this.unacked_msgs[msg.Id] += 1;
                    }
                    else
                    {
                        this.unacked_msgs.Add(msg.Id, 1);
                    }
                }
                else
                {
                    /*The message was acked afterall. We don't have to keep track of
                    it anymore*/
                    if (this.unacked_msgs.ContainsKey(msg.Id))
                        this.unacked_msgs.Remove(msg.Id);
                }
            }
            else
            {
                // Auto acknowledge the message
                msg.ack();
            }
            /* Post to the response exchange if there was a reponse and a response
               was requested by including a reply-to property */
            if (msg.ReplyTo!="" && response.Count > 0)
            {
                if (error_occured)
                    msg.reply(JsonConvert.SerializeObject((response)), "exception");
                else
                    msg.reply(JsonConvert.SerializeObject((response)));
            }
        }

        public virtual Dictionary<string, string> handle(Message msg)
        {
            /*
        Over ride this method to handle messages
        @param message: A Message object
        @return: Depending on the request, this return value will be returned
        to the client
        */
            Dictionary<string, string> response = new Dictionary<string, string>();
            response.Add("", "");
            return response;
        }
    }

}

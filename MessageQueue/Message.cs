using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using Newtonsoft.Json;
using RabbitMQ.Client.Impl;

namespace MessageQueue
{
    public class Message
    {
        
        //private int ack_used;
        private IModel channel;
        private string exchange;
        private string routingKey;
        private string correlationID;
        public string CorrelationId
        {
            get
            {
                return correlationID;
            }
        }
        private string replyTo;
        public string ReplyTo
        {
            get
            {
                return replyTo;
            }
        }
        private Boolean received;
        private dynamic acked;
        public dynamic Acked
        {
            get
            {
                return acked;
            }
        }
        private byte deliveryMode;
        private IModel delayChannel;
        public IModel DelayChannel
        {
            get
            {
                return delayChannel;
            }
            set
            {
                delayChannel = value;
            }
        }
        private string delayQueue;
        public string DelayQueue
        {
            get
            {
                return delayQueue;
            }
            set
            {
                delayQueue = value;
            }
        }
        private string body;
        public string Body
        {
            get
            {
                return body;
            }
        }
        private string messageType;
        private Boolean redelivered;
        private DateTime sent;
        private IDictionary<string, object> headers;
        private ulong deliveryTag;
        private bool msgSent;
        private string contentType;
        private string id;
        public string Id
        {
            get
            {
                return id;
            }
        }
        
        public static Message createFromChannel(IModel channel, bool redelivered, ulong deliveryTag, 
            string routingKey, IBasicProperties prop, byte[] body, IModel delayChannel=null, string delayQueue=null)
        {
            string bodystr = Encoding.UTF8.GetString(body);
            AmqpTimestamp ts = prop.Timestamp;
            string messageType = prop.Type;
            Message msg = new Message(channel, "", routingKey, bodystr,
                redelivered, deliveryTag, prop, delayChannel, delayQueue);
            msg.received = true;
            DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            msg.sent = dtDateTime.AddSeconds( ts.UnixTime ).ToLocalTime();
            return msg;
        }

        public Message(IModel channel, string exchange, string routingKey, string body, 
            bool redelivered, ulong deliveryTag, IBasicProperties props=null,
            IModel delayChannel=null, string delayQueue=null)
        {
            this.channel = channel;
            this.exchange = exchange;
            this.routingKey = routingKey;
            if (props==null)
            {
                this.correlationID = "";
                this.replyTo = "";
                this.messageType = "";
            }
            else
            {
                this.correlationID = props.CorrelationId;
                this.replyTo = props.ReplyTo;
                this.messageType = props.Type;
            }
            this.received = false;
            //this.ack_used = 1;
            this.acked = null;
            this.redelivered = redelivered;
            
            this.deliveryMode = 2;
            this.deliveryTag = deliveryTag;
            this.delayChannel = delayChannel;
            this.delayQueue = delayQueue;
            this.body = body;
            if (props == null)
            {
                msgSent = false;
                this.headers = null;
                this.contentType = "application/json";
                this.id = Guid.NewGuid().ToString();
            }
            else
            {
                this.headers = props.Headers;
                this.contentType = props.ContentType;
                this.id = props.MessageId;
            }
        }
        public void nack()
        {
            /*
            Sends a "Not Acknowledged" message to the server, upon which the
            server will attempt to redeliver the message
            */
            if (this.acked==null)
            {
                if (!this.received)
                {
                    throw new MessageException("Can only acknowledge received messages");
                }
                this.acked = false;
                //this.ack_used = 1;
                this.channel.BasicNack(this.deliveryTag, false, true);
            }
            else
            {
                throw new MessageAcknowledgementAlreadySent();
            }
        }

        public void ack()
        {
            /*    
            Sends a "Acknowledged" message to the server, signaling that the
            message was handled and the job is done
            */
            if (this.acked == null)
            {
                if (!this.received)
                    throw new MessageException("Can only acknowledge received messages");
            this.acked = true;
            //this.ack_used = 2;
            this.channel.BasicAck(this.deliveryTag, false);
            }
            else
                throw new MessageAcknowledgementAlreadySent();
        }
    
        public void delay()
        {
            /*
            Attempts to redeliver the message delayed.
            */
            if (this.acked)
            {
                //this.acked = null;
                if (this.delayQueue == null || this.delayChannel == null)
                    throw new MessageException(
                        "Message did not have a delay channel set " +
                        "up when delay() was called");
                if (!this.received)
                    throw new MessageException("Can only delay received " +
                                       "messages");
                dynamic msg_body = this.body.Clone();
                msg_body = JsonConvert.DeserializeObject(msg_body);
                msg_body._timestamp = this.sent;
                msg_body._type = this.messageType;
                string json_msg_body = JsonConvert.SerializeObject(msg_body);
                IBasicProperties properties = delayChannel.CreateBasicProperties();
                properties.CorrelationId = this.correlationID;
                properties.ReplyTo = this.replyTo;
                properties.ContentType = this.contentType;
                properties.MessageId = this.id;
                properties.DeliveryMode = this.deliveryMode;
                this.delayChannel.BasicPublish("",
                this.delayQueue,
                false,
                false,
                properties,
                System.Text.Encoding.UTF8.GetBytes(json_msg_body));
            }
            else
                throw new MessageException("Cannot delay unacknowledged message");
        }

    public void reject()
    {
        /*
        Sends a "Not Acknowledged" message to the server upon which the
        server will drop the message
        */
        if (this.acked==null)
        {
            if (!this.received)
                throw new MessageException("Can only reject received " +
                                       "messages");
            this.acked = false;
            //this.ack_used = 2;
            this.channel.BasicNack(this.deliveryTag,
                                    false,
                                    false);
        }
        else
            throw new MessageAcknowledgementAlreadySent();
    }

    public void send()
    {
        //TODO: consider using a callback for confirm and turning nowait on
        if (!this.msgSent)
        {
            DateTime now = DateTime.Now;
            dynamic msg_body = this.body.Clone();
            Dictionary<string, string> json_data = JsonConvert.DeserializeObject<Dictionary<string, string>>(msg_body);
            json_data["_timestamp"] = Convert.ToString(now);
            json_data["_type"] = this.messageType;
            IBasicProperties properties = this.channel.CreateBasicProperties();
            properties.CorrelationId = this.correlationID;
            properties.ReplyTo = this.replyTo;
            properties.ContentType = this.contentType;
            properties.MessageId = this.id;
            properties.DeliveryMode = this.deliveryMode;
            string content = JsonConvert.SerializeObject(json_data);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(content);
            this.channel.BasicPublish(this.exchange,
                                       this.routingKey,
                                       properties,
                                       data);
            this.sent = (DateTime)now;
        }
    }

    public Message reply(string content, string message_type="response")
    {
        if (!this.received)
            throw new MessageException("Can only reply to received " +
                                   "messages");
        if (this.acked && this.replyTo!="")
        {
            Message msg = new Message(channel, "", this.replyTo, content,
                redelivered, deliveryTag);
            msg.send();
            return msg;
        }
        else
        {
            if (this.replyTo!="")
                throw new MessageException("Cannot reply to message. A reply was " +
                                       "requested but the message was not " +
                                       "acknowledged.");
            else
                throw new MessageException("Cannot reply to message. The " +
                                       "message did not include a reply-to " +
                                      "property");
        }
    }

    }
}

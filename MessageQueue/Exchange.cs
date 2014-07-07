#define TRACE 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace MessageQueue
{
    /// <summary>
    /// An AMQP exchange. A unique name must be assigned to an exchange.</summary>
    public class Exchange
    {
        /// <summary>
        /// Store for the amqp_host property</summary>
        private string amqp_host;
        /// <summary>
        /// Store for the config property</summary>
        private Dictionary<string, List<Dictionary<string, string>>> config;
        /// <summary>
        /// Store for the handler_module</summary>
        private string handler_module;
        /// <summary>
        /// Store for the name property</summary>
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        /// <summary>
        /// Store for AMQP connection</summary>
        private IConnection connection;
        /// <summary>
        /// Store for AMQP channel</summary>
        private IModel channel;
        /// <summary>
        /// Store for AMQP channel</summary>
        private IModel delay_channel;
        private ArrayList handlers;
        /// <summary>
        /// Description for Exchange Constructor.</summary>
        /// <param name="amqp_host">The ip or hostname of the AMPQ server</param>
        /// <param name="config">An ArrayList of Tuples mapping Queues to MessageHandlers.
        /// Array list is of the following format:
        /// ["<queue_name>", "<message_handler_class>",...]
        /// queue_name is arbitrary and can be named according to it's function
        /// "message_handler_class" should be the name of a class derived from MessageHandler
        /// that handles processing of messages</param>
        ///<param name="handler_module"> the .NET namespace that contains the MessageHandler
        ///derivative classes </param>
        ///<param name="name"> the name of the exchange</param>
        public Exchange(string amqp_host, Dictionary<string, List<Dictionary<string, string>>> config, string handler_module, string name)
        {
            this.config = config;
            this.handler_module = handler_module;
            this.name = name;
            this.amqp_host = amqp_host;
            var factory = new ConnectionFactory() { HostName = amqp_host };
            this.connection = factory.CreateConnection();
            this.channel = connection.CreateModel();
            channel.ConfirmSelect();
            channel.ExchangeDeclare(this.name,"direct");
            this.delay_channel = connection.CreateModel();
            delay_channel.ConfirmSelect();
            this.handlers = new ArrayList();
            foreach(var outerdict in config)
            {
                string route = outerdict.Key;
                List<Dictionary<string,string>> conf = outerdict.Value;
                int redeliver_attempts = Convert.ToInt16(conf[0].Values.First());
                int redeliver_delay = Convert.ToInt16(conf[1].Values.First());
                bool manual_ack = Convert.ToBoolean(conf[2].Values.First());

                    Assembly assembly = Assembly.Load(handler_module);
                    Console.WriteLine(outerdict.Key);
                    Console.WriteLine(outerdict.Value);
                
                    Type classType = assembly.GetType(string.Format("{0}.{1}", handler_module,
                        outerdict.Key), false, true);
                    if (classType == null)
                    {
                        throw new ImproperlyConfiguredException(
                            string.Format("class {0} is not defined in {1}",
                             outerdict.Key, handler_module));
                    }
                    string queue_name = Utils.make_queue_name(this.name, route);
                    string delay_queue_name = Utils.make_queue_name(this.name, route, "delay");
                    // create a dictionary for storing arguments used in declaring a queue
                    Dictionary<string, object> queueArgs = new Dictionary<string, object>
                    {
                        {"x-ha-policy", "all"},
                        {"x-ha-sync-mode", "automatic"},
                    };
                    this.channel.QueueDeclare(queue_name, true, false, false, queueArgs);
                    this.channel.QueueBind(queue_name, this.name, route);
                    
                    
                    
                    // setup arguments for creating MessageHandler instanse
                    object[] args = new object[] { this.channel, route, outerdict.Value,
                                    this.delay_channel,
                                    string.Format("{0}_delay", route, manual_ack)};
                    // create a MessageHandler object
                    MessageHandler handler = (MessageHandler)Activator.CreateInstance(classType, args);
                    
                    /*if (outerdict.Value.ContainsKey("redeliver_delay"))
                    {
                        delay = Convert.ToInt16(outerdict.Value["redeliver_delay"]);
                    }
                    else
                    {
                        throw new ImproperlyConfiguredException(
                            string.Format(
                            "MessaheHandler for route {0} must define a redelivery_delay",
                            route));
                    }*/
                    Dictionary<string, object> delayQueueArgs = new Dictionary<string, object>
                    {
                        {"x-message-ttl", redeliver_delay * 1000},
                        {"x-dead-letter-exchange", this.name},
                        {"x-dead-letter-routing-key", queue_name},
                        { "x-ha-policy", "all"},
                        {"x-ha-sync-mode", "automatic"},
                    };
                    this.delay_channel.QueueDeclare(delay_queue_name, true, false, false,
                        delayQueueArgs);
                    //MessageHandler consumer = new MessageHandler(channel);
                    this.channel.BasicConsume(queue_name, false, handler);
                    this.handlers.Add(handler);
                

            }
            

        }
        
    
    }

    

    
}

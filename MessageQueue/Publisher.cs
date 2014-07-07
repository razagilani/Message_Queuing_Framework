using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace MessageQueue
{
    public class Publisher
    {
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
        private List<string> exchangesList;
        private List<string> routingKeys;
        private Dictionary<string, string> exchanges;
        private IConnection connection;
        /// <summary>
        /// Store for AMQP channel</summary>
        private IModel channel;
        /// <summary>
        /// Store for AMQP channel</summary>

        public Publisher(string amqp_host, Dictionary<string, List<string>> config)
        {
        /*Sets up a very simple Publisher. Publishers are responsible for sending
        messages to the configured exchanges with the configured routing key
        @param amqp_host: The ip or hostname of the AMPQ server
        @param configuration:
        Sample configuration datastructure:
            {'exchanges': ['xbill', 'altitude'],
             'routing_keys': ['account_created', 'some_other_routing_key'],
            }
        exchanges is a list of exchanges that the publisher should publish to
        routing_keys is either a single string which is used as a routing_key
            messages sent to all exchanges, or it is a list of routing_keys
            whereby there must be exactly one routing key for each exchange
        */
        
        this.exchangesList = new List<string>();
        this.routingKeys = new List<string>();

        if (this.name=="")
        {
            this.name = this.GetType().Name;
        }

        List<string> p_exchanges = new List<string>();
        p_exchanges = config["exchanges"];
        List<string> p_routingKeys = new List<string>();
        p_routingKeys = config["routing_keys"];
        this.exchanges = new Dictionary<string, string>();
        Console.WriteLine(routingKeys.Count);

        if (p_routingKeys.Count == 1)
            foreach(string exchange in p_exchanges)
            {
                this.exchanges.Add(exchange, p_routingKeys[0]);
            }
        else if (p_routingKeys.Count == p_exchanges.Count)
        {
            int count = 0;
            foreach(string exchange in p_exchanges)
            {
                this.exchanges.Add(exchange, p_routingKeys[count]);
                count++;
            }
        }
        else
            throw new ImproperlyConfiguredException("The number of routing_keys must " +
                                       "either be one or equal to the number " +
                                       "of exchanges");
        
        var factory = new ConnectionFactory() { HostName = amqp_host };
        this.connection = factory.CreateConnection();
        this.channel = connection.CreateModel();
        // Declare all exchanges in case they haven't been set up already
        foreach(KeyValuePair<string, string> exchange in exchanges)
        {
            channel.ExchangeDeclare(exchange.Key, "direct");
            string queueName = Utils.make_queue_name(exchange.Key,
                                                    exchange.Value);
            channel.QueueDeclare(queueName, true,false,false,
                        new Dictionary<string, object>
                    {
                        {"x-ha-policy", "all"},
                        {"x-ha-sync-mode", "automatic"},
                    });
            channel.QueueBind(queueName, exchange.Key, exchange.Value); 
        }
    }

    public void publish(string message)
    {
        /*
        Publishes a message to configured exchanges with the configured routing
        key
        @param message: The message to be published
        */
        Console.WriteLine(message);
        foreach (KeyValuePair<string, string> exchange in exchanges)
        {
            Message msg = new Message(channel, exchange.Key, exchange.Value,
                message, false, 0);
            msg.send();
        }

    }
}}

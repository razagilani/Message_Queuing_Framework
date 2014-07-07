using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using MessageQueue;
using System.Configuration;
using Newtonsoft.Json;

namespace CLRTrigger
{
    public class CustomerTrigger:Publisher
    {
        private static Publisher pub;
         public CustomerTrigger(Dictionary<string, List<string>> config): base("localhost", config)
        {
            
            
        }
        
        public static void sendInserted()
        {
            var exchangeConfig = ConfigurationManager.GetSection("ExchangeConfig") as MyConfigSection;
            Dictionary<string, List<string>> conf = new Dictionary<string, List<string>>();
            List<string> temp = new List<string>();

            foreach (var e in exchangeConfig.Instances.AsEnumerable())
            {
                Console.WriteLine(e.Name);
                temp.Add(e.Name);
            }
            conf.Add("exchanges", temp);
            temp = null;
            temp = new List<string>();

            var routingConfig = ConfigurationManager.GetSection("RoutingConfig") as MyConfigSection;
            foreach (var e in routingConfig.Instances.AsEnumerable())
            {
                Console.WriteLine(e.Name);
                temp.Add(e.Name);
            }
            conf.Add("routing_keys", temp);
            pub = new Publisher("localhost", conf);
            SqlTriggerContext triggContext = SqlContext.TriggerContext;
            SqlConnection conn = new SqlConnection(" context connection =true ");
            conn.Open();
            SqlCommand sqlComm = conn.CreateCommand();
            SqlPipe sqlP = SqlContext.Pipe;
            SqlDataReader dr;
            sqlComm.CommandText = "SELECT id, first_name, last_name, city, state, phone from inserted";
            dr = sqlComm.ExecuteReader();
            while (dr.Read())
            {
                string message = (string)dr[0] + "," + (string)dr[1] + "," + (string)dr[2] + "," + (string)dr[3] + "," + (string)dr[4] + "," + (string)dr[5];
                Dictionary<string, string> content = new Dictionary<string, string>();
                content.Add("customer", message);
                pub.publish(JsonConvert.SerializeObject(message));
            }
        }

    }
 
    }
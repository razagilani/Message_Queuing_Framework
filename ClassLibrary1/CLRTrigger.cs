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
    public class CLRTrigger : MessageQueue.Publisher
    {
        private static CLRTrigger pub;
        public CLRTrigger(Dictionary<string, List<string>> config)
            : base("localhost", config)
        {


        }

        public static void sendInserted()
        {
            string exchange = "PortalExchange";
            string message_handler = "PortalMessageHandler";
            Dictionary<string, List<string>> conf = new Dictionary<string, List<string>>();
            List<string> temp = new List<string>();
            temp.Add(exchange);
            conf.Add("exchanges", temp);
            temp = null;
            temp = new List<string>();
            temp.Add(message_handler);
            conf.Add("routing_keys", temp);
            pub = new CLRTrigger(conf);
            //SqlContext.Pipe.Send("Publisher Instantiated");
            SqlTriggerContext triggContext = SqlContext.TriggerContext;
            SqlConnection conn = new SqlConnection("context connection=True");
            conn.Open();
            //SqlContext.Pipe.Send("Connection opened");
            SqlCommand sqlComm = conn.CreateCommand();
            SqlPipe sqlP = SqlContext.Pipe;
            SqlDataReader dr;
            sqlComm.CommandText = "SELECT id, first_name, last_name, city, state, phone from inserted";
            dr = sqlComm.ExecuteReader();
            SqlContext.Pipe.Send("Command Executed");
            while (dr.Read())
            {
                Dictionary<string, string> content = new Dictionary<string, string>();
                content.Add("ID", Convert.ToString(dr[0]));
                content.Add("first_name", Convert.ToString(dr[1]));
                content.Add("last_name", Convert.ToString(dr[2]));
                content.Add("city", Convert.ToString(dr[3]));
                content.Add("state", Convert.ToString(dr[4]));
                content.Add("phone", Convert.ToString(dr[5]));
                pub.publish(JsonConvert.SerializeObject(content));
            }
        }
            
    }
    

}
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

namespace AltitudeIndividualCLRTrigger
{
    public class AltitudeIndividualCLRTrigger : MessageQueue.Publisher
    {
        private static AltitudeIndividualCLRTrigger pub;
        public AltitudeIndividualCLRTrigger(Dictionary<string, List<string>> config)
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
            pub = new AltitudeIndividualCLRTrigger(conf);
            //SqlContext.Pipe.Send("Publisher Instantiated");
            SqlTriggerContext triggContext = SqlContext.TriggerContext;
            SqlConnection conn = new SqlConnection("context connection=True");
            conn.Open();
            //SqlContext.Pipe.Send("Connection opened");
            SqlCommand sqlComm = conn.CreateCommand();
            SqlPipe sqlP = SqlContext.Pipe;
            SqlDataReader dr;
            sqlComm.CommandText = "SELECT First, Middle, Last, Work_Email, Create_Date from inserted";
            dr = sqlComm.ExecuteReader();
            SqlContext.Pipe.Send("Command Executed");
            while (dr.Read())
            {
                Dictionary<string, string> content = new Dictionary<string, string>();
                content.Add("First_Name", Convert.ToString(dr[0]));
                content.Add("Middle_name", Convert.ToString(dr[1]));
                content.Add("Last_name", Convert.ToString(dr[2]));
                content.Add("Work EMail", Convert.ToString(dr[3]));
                content.Add("Date_Created", Convert.ToString(dr[4]));
                pub.publish(JsonConvert.SerializeObject(content));
            }
        }
            
    }
    

}
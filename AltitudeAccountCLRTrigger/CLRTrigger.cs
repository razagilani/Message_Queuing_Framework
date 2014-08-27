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

namespace AltitudeAccountCLRTrigger
{
    public class AltitudeAccountCLRTrigger : MessageQueue.Publisher
    {
        private static AltitudeAccountCLRTrigger pub;
        public AltitudeAccountCLRTrigger(Dictionary<string, List<string>> config)
            : base("localhost", config)
        {


        }

        public static void sendInserted()
        {
            string exchange = "altitude";
            string routing_key = "some_other_routing_key";
            Dictionary<string, List<string>> conf = new Dictionary<string, List<string>>();
            List<string> temp = new List<string>();
            temp.Add(exchange);
            conf.Add("exchanges", temp);
            temp = null;
            temp = new List<string>();
            temp.Add(routing_key);
            conf.Add("routing_keys", temp);
            pub = new AltitudeAccountCLRTrigger(conf);
            //SqlContext.Pipe.Send("Publisher Instantiated");
            SqlTriggerContext triggContext = SqlContext.TriggerContext;
            SqlConnection conn = new SqlConnection("context connection=True");
            conn.Open();
            //SqlContext.Pipe.Send("Connection opened");
            SqlCommand sqlComm = conn.CreateCommand();
            SqlPipe sqlP = SqlContext.Pipe;
            SqlDataReader dr;
            sqlComm.CommandText = "SELECT Account_Token, Account_Global_Unique_Identifier, Create_Date, Account_Name from inserted";
            dr = sqlComm.ExecuteReader();                
            SqlContext.Pipe.Send("Command Executed");
            Dictionary<string, Dictionary<string, string>> account = new Dictionary<string, Dictionary<string, string>>();
            if (dr.Read())
            {
                Dictionary<string, string> content = new Dictionary<string, string>();
                content.Add("Token", Convert.ToString(dr[0]));
                content.Add("GUID", Convert.ToString(dr[1]));
                content.Add("Date_Created", Convert.ToString(dr[2]));
                string[] name_tokens = Convert.ToString(dr[3]).Split(' ');
                if (name_tokens.Length > 1)
                {
                    content.Add("First_Name", name_tokens[0]);
                    content.Add("Last_Name", name_tokens[1]);
                }
                else
                {
                    content.Add("First_Name", name_tokens[0]);
                    content.Add("Last_Name", "");
                }
                account.Add("Account", content);
                pub.publish(JsonConvert.SerializeObject(account));
                SqlContext.Pipe.Send("Account object sent: " + account);
            }
        }
            
    }
    

}
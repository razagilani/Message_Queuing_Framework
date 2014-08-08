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

namespace AccountIndividualTrigger
{
    public class AltitudeAccountIndividualCLRTrigger : MessageQueue.Publisher
    {
        private static AltitudeAccountIndividualCLRTrigger pub;
        public AltitudeAccountIndividualCLRTrigger(Dictionary<string, List<string>> config)
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
            pub = new AltitudeAccountIndividualCLRTrigger(conf);
            //SqlContext.Pipe.Send("Publisher Instantiated");
            SqlTriggerContext triggContext = SqlContext.TriggerContext;
            SqlConnection conn = new SqlConnection("context connection=True");
            conn.Open();
            //SqlContext.Pipe.Send("Connection opened");
            SqlCommand sqlComm = conn.CreateCommand();
            SqlPipe sqlP = SqlContext.Pipe;
            SqlDataReader dr;
            sqlComm.CommandText = "SELECT Account, Individual from inserted";
            dr = sqlComm.ExecuteReader();
            SqlContext.Pipe.Send("Command Executed");
            if (dr.Read())
            {
                int account_id = Convert.ToInt16(dr[0]);
                int individual_id = Convert.ToInt16(dr[1]);
                dr.Close();
                SqlCommand accountcmd = conn.CreateCommand();
                accountcmd.CommandText = "SELECT Account_Token, Account_Global_Unique_Identifier, Create_Date, Account_Name from Account where Account_id=" + Convert.ToString(account_id);
                SqlDataReader accountReader = accountcmd.ExecuteReader();
                Dictionary<string, Dictionary<string, string>> content = new Dictionary<string,Dictionary<string,string>>();
                if (accountReader.Read())
                {

                    Dictionary<string, string> account = new Dictionary<string, string>();
                    account.Add("Token", Convert.ToString(accountReader[0]));
                    account.Add("GUID", Convert.ToString(accountReader[1]));
                    account.Add("Date_Created", Convert.ToString(accountReader[2]));
                    string[] name_tokens = Convert.ToString(accountReader[3]).Split(' ');
                    if (name_tokens.Length > 1)
                    {
                        account.Add("First_Name", name_tokens[0]);
                        account.Add("Last_Name", name_tokens[1]);
                    }
                    else
                    {
                        account.Add("First_Name", name_tokens[0]);
                        account.Add("Last_Name", "");
                    }
                    content.Add("Account", account);
                    accountReader.Close();
                }
                SqlCommand individualcmd = conn.CreateCommand();
                individualcmd.CommandText = "SELECT First, Middle, Last, Work_Email, Create_Date, Individual_Global_Unique_Identifier from Individual where individual_id=" + Convert.ToString(individual_id);
                SqlDataReader individualReader = individualcmd.ExecuteReader();
                if (individualReader.Read())
                {
                    Dictionary<string, string> individual = new Dictionary<string, string>();
                    individual.Add("First_Name", Convert.ToString(individualReader[0]));
                    individual.Add("Middle_Name", Convert.ToString(individualReader[1]));
                    individual.Add("Last_Name", Convert.ToString(individualReader[2]));
                    individual.Add("Email", Convert.ToString(individualReader[3]));
                    individual.Add("Date_Created", Convert.ToString(individualReader[4]));
                    individual.Add("GUID", Convert.ToString(individualReader[5]));
                    content.Add("Individual", individual);
                    individualReader.Close();
                }
                
                pub.publish(JsonConvert.SerializeObject(content));
            }
        }

    }


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Sql;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using MessageQueue;

namespace AltitudeAccountExchange
{
    public class AccountHandler : MessageHandler
    {

        public AccountHandler(IModel channel, string queue, List<Dictionary<string, string>> conf,
           IModel delayChannel, string delayQueue, Boolean manual_ack, Object obj)
            : base(channel, queue, conf, delayChannel, delayQueue)
        {
        }

        public override Dictionary<string, string> handle(Message message)
        {


            Console.WriteLine("AccountHandler handling message {0}: {1}",
                message.CorrelationId, message.Body);
            message.ack();
            // add data to it's corresponding table
            RootObject account_data = JsonConvert.DeserializeObject<RootObject>(message.Body);
            //insert data in local database
            insert_data(account_data);
            Dictionary<string, string> response = new Dictionary<string, string>();

            response.Add("message", String.Format("This is a Account Handler  response to " +
               "message {0}", message.CorrelationId));
            return response;
        }

        private void insert_data(RootObject account)
        {
            // setup sql server connection to insert new data
            using (SqlConnection conn = new SqlConnection(@"Persist Security Info=False;Integrated Security=true;Initial Catalog=Altitude_Production;server=(local)"))
            {
                SqlCommand addAccount = new SqlCommand(@"INSERT INTO account (Account_Name, Account_Global_Unique_Identifier, Account_Token, Create_Date, Locked) 
                    VALUES (@account_name, @guid, @token, @created, @locked)", conn);

                addAccount.Parameters.AddWithValue("@account_name", account.Account.First_Name);
                addAccount.Parameters.AddWithValue("@guid", account.Account.GUID);
                addAccount.Parameters.AddWithValue("@token", account.Account.Token);
                addAccount.Parameters.AddWithValue("@created", account.Account.Date_Created);
                addAccount.Parameters.AddWithValue("@locked", account.Account.Locked);

                addAccount.Connection.Open();
                addAccount.ExecuteNonQuery();
                addAccount.Connection.Close();
            }

                
           
        }
        public class Account
        {
            public string Date_Created { get; set; }
            public string First_Name { get; set; }
            public string GUID { get; set; }
            public int Locked { get; set; }
            public string Token { get; set; }
        }

        public class RootObject
        {
            public Account Account { get; set; }
            public string _timestamp { get; set; }
            public string _type { get; set; }
        }
    }

}

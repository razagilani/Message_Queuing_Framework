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

namespace AltitudeIndividualExchange
{
    public class IndividualHandler : MessageHandler
    {

        public IndividualHandler(IModel channel, string queue, List<Dictionary<string, string>> conf,
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
            RootObject individual_data = JsonConvert.DeserializeObject<RootObject>(message.Body);
            //insert data in local database
            insert_data(individual_data);
            Dictionary<string, string> response = new Dictionary<string, string>();

            response.Add("message", String.Format("This is Individual Handler  response to " +
               "message {0}", message.CorrelationId));
            return response;
        }

        private void insert_data(RootObject individual)
        {
            // setup sql server connection to insert new data
            using (SqlConnection conn = new SqlConnection(@"Persist Security Info=False;Integrated Security=true;Initial Catalog=Altitude_Production;server=(local)"))
            {
                SqlCommand addIndividual = new SqlCommand(@"INSERT INTO Individual (First, Last, Individual_Global_Unique_Identifier, Personal_Email, Create_Date, Company, Address_City, Employee_Office, Employee_Department) 
                    VALUES (@first_name, @last_name, @guid, @email, @created, @company, @city, @dept, @office)", conn);

                addIndividual.Parameters.AddWithValue("@first_name", individual.Individual.First_Name);
                addIndividual.Parameters.AddWithValue("@guid", individual.Individual.GUID);
                addIndividual.Parameters.AddWithValue("@email", individual.Individual.Email);
                addIndividual.Parameters.AddWithValue("@created", individual.Individual.Date_Created);
                addIndividual.Parameters.AddWithValue("@last_name", individual.Individual.Last_Name);
                addIndividual.Parameters.AddWithValue("@company", individual.Individual.Company);
                addIndividual.Parameters.AddWithValue("@city", individual.Individual.City);
                addIndividual.Parameters.AddWithValue("@dept", individual.Individual.Emp_Dept);
                addIndividual.Parameters.AddWithValue("@office", individual.Individual.Emp_Office);

                addIndividual.Connection.Open();
                addIndividual.ExecuteNonQuery();
                addIndividual.Connection.Close();
            }

                
           
        }
        public class Individual
        {
            public string Date_Created { get; set; }
            public string First_Name { get; set; }
            public string GUID { get; set; }
            public string Last_Name { get; set; }
            public string Email { get; set; }
            public string Company { get; set; }
            public string City { get; set; }
            public string Emp_Dept { get; set; }
            public string Emp_Office { get; set; }

        }

        public class RootObject
        {
            public Individual Individual { get; set; }
            public string _timestamp { get; set; }
            public string _type { get; set; }
        }
    }

}

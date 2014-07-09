using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using MessageQueue;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Configuration;

namespace Altitude
{
    public partial class Form1 : Form
    {
        private String connectionString = null;
        private SqlConnection sqlConnection = null;
        private SqlDataAdapter sqlDataAdapter = null;
        private SqlCommandBuilder sqlCommandBuilder = null;
        private DataTable dataTable = null;
        private BindingSource bindingSource = null;
        private String selectQueryString = null;
        private AltitudePublisher pub;   

        public Form1()
        {
            InitializeComponent();
            //Load Configuration from config file for instantiating a Publisher
            var exchangeConfig = ConfigurationManager.GetSection("ExchangeConfig") as PublisherConfig;
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

            var routingConfig = ConfigurationManager.GetSection("RoutingConfig") as PublisherConfig;
            foreach (var e in routingConfig.Instances.AsEnumerable())
            {
                Console.WriteLine(e.Name);
                temp.Add(e.Name);
            }
            conf.Add("routing_keys", temp);
            pub = new AltitudePublisher(conf); 
            //Load configuration from config file for instantiating Exchange
            ExchangeConfig r = ExchangeConfig.Settings;
            Dictionary<string, List<Dictionary<string, string>>> config = new Dictionary<string, List<Dictionary<string, string>>>();
            List<Dictionary<string, string>> inner = new List<Dictionary<string, string>>();
            inner.Add(new Dictionary<string, string>() { { "redeliver_attempts", r.Parameters[0].redeliverAttempts } });
            inner.Add(new Dictionary<string, string>() { { "redeliver_delay", r.Parameters[0].redeliverDelay } });
            inner.Add(new Dictionary<string, string>() { { "redeliver_attempts", r.Parameters[0].manualAck } });
            config.Add(r.Parameters[0].Name, inner);
            Exchange exchange = new AltitudeExchange(config, "AltitudeExchange", this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectionString = "Data Source=.\\SQLExpress;Initial Catalog=destination;Integrated Security=True";
            sqlConnection = new SqlConnection(connectionString);
            selectQueryString = "SELECT * FROM customer";

            sqlConnection.Open();

            sqlDataAdapter = new SqlDataAdapter(selectQueryString, sqlConnection);
            sqlCommandBuilder = new SqlCommandBuilder(sqlDataAdapter);

            dataTable = new DataTable();
            sqlDataAdapter.Fill(dataTable);
            bindingSource = new BindingSource();
            bindingSource.DataSource = dataTable;

            dataGridView1.DataSource = bindingSource;

        }

        public void add_row(int id, string fname, string lname, string city, string state, string phone)
        {
            //DataGridViewRow row = (DataGridViewRow)dataGridView1.Rows[0].Clone();
            DataRow row = dataTable.NewRow();
            row["ID"] = id;
            row["first_name"] = fname;
            row["last_name"] = lname;
            row["city"] = city;
            row["state"] = state;
            row["phone"] = phone;
            /*row.Cells[0].Value = id;
            row.Cells[0].Value = fname;
            row.Cells[0].Value = lname;
            row.Cells[0].Value = city;
            row.Cells[0].Value = state;
            row.Cells[0].Value = phone;
            //dataGridView1.Rows.Add(row);*/
            dataTable.Rows.Add(row);
            sqlDataAdapter.Update(dataTable);
           
            //bindingSource.ResetBindings(false);
            /*dataGridView1.DataSource = null;
            dataGridView1.DataSource = bindingSource;*/
            //sqlDataAdapter.Fill(dataTable);  

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = dataTable.GetChanges();
                Dictionary<string, string> rows = new Dictionary<string, string>();
                foreach (DataRow row in dt.Rows)
                {
                    rows.Add("ID", Convert.ToString(row["ID"]));
                    rows.Add("first_name", Convert.ToString(row["first_name"]));
                    rows.Add("last_name", Convert.ToString(row["last_name"]));
                    rows.Add("city", Convert.ToString(row["city"]));
                    rows.Add("state", Convert.ToString(row["state"]));
                    rows.Add("phone", Convert.ToString(row["phone"]));
                    //TextBox1.Text = row["ImagePath"].ToString();

                }
                string message = JsonConvert.SerializeObject(rows);
                pub.publish(message);
                sqlDataAdapter.Update(dataTable);
                MessageBox.Show("The record is added");
            }
            catch (Exception exceptionObj)
            {
                MessageBox.Show(exceptionObj.Message.ToString());
            }
        }
    }
    
    

    
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using Newtonsoft.Json;
using MessageQueue;
using System.Configuration;
using RabbitMQ;
using RabbitMQ.Client;

namespace Portal
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
        private PortalPublisher pub;
 
    
        public Form1()
        {
            InitializeComponent();
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
            pub = new PortalPublisher(conf);
            RoutingKey r = RoutingKey.Settings;
            Dictionary<string, List<Dictionary<string, string>>> config = new Dictionary<string, List<Dictionary<string, string>>>();
            List<Dictionary<string, string>> inner = new List<Dictionary<string, string>>();


            inner.Add(new Dictionary<string, string>() { { "redeliver_attempts", r.Parameters[0].redeliverAttempts } });
            inner.Add(new Dictionary<string, string>() { { "redeliver_delay", r.Parameters[0].redeliverDelay } });
            inner.Add(new Dictionary<string, string>() { { "redeliver_attempts", r.Parameters[0].manualAck } });
            //inner.Add(r.Parameters[0].redeliverAttempts, r.Parameters[0].redeliverDelay, r.Parameters[0].manualAck);
            config.Add(r.Parameters[0].Name, inner);
            Exchange exchange = new PortalExchange(config, "PortalExchange", this);
           
        }

        public void add_row(int id, string fname, string lname, string city, string state, string phone)
        {
            DataRow row = dataTable.NewRow();
            row["ID"] = id;
            row["first_name"] = fname;
            row["last_name"] = lname;
            row["city"] = city;
            row["state"] = state;
            row["phone"] = phone.Remove(phone.Length - 3);
            /*row.Cells[0].Value = id;
            row.Cells[0].Value = fname;
            row.Cells[0].Value = lname;
            row.Cells[0].Value = city;
            row.Cells[0].Value = state;
            row.Cells[0].Value = phone;
            //dataGridView1.Rows.Add(row);*/
            dataTable.Rows.Add(row);
            sqlDataAdapter.Update(dataTable);
        }

        private void Add_row_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = dataTable.GetChanges();
                Dictionary<string, string> rows = new Dictionary<string, string>();
                foreach (DataRow row in dt.Rows)
                {
                    rows.Add("row", row["ID"] + "," + row["first_name"] +
                        "," + row["last_name"] +
                        "," + row["city"] +
                        "," + row["state"] +
                        "," + row["phone"]);
                    //TextBox1.Text = row["ImagePath"].ToString();
                    
                }
                string message = JsonConvert.SerializeObject(rows);
                pub.publish(message);
                sqlDataAdapter.Update(dataTable);
            }
            catch (Exception exceptionObj)
            {
                MessageBox.Show(exceptionObj.Message.ToString());
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connectionString = "Data Source=.\\SQLExpress;Initial Catalog=source;Integrated Security=True";
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
    }
    

   

    
}

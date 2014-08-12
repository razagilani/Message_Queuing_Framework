using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MessageQueue;
using IndividualExchange;

namespace AltitudeIndividualHandlerService
{
    class AltitudeIndividualHandlerService : ServiceBase
    {
        Exchange e = null;
        public AltitudeIndividualHandlerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                RoutingKey r = RoutingKey.Settings;
                Dictionary<string, List<Dictionary<string, string>>> config = new Dictionary<string, List<Dictionary<string, string>>>();
                List<Dictionary<string, string>> inner = new List<Dictionary<string, string>>();


                inner.Add(new Dictionary<string, string>() { { "redeliver_attempts", r.Parameters[0].redeliverAttempts } });
                inner.Add(new Dictionary<string, string>() { { "redeliver_delay", r.Parameters[0].redeliverDelay } });
                inner.Add(new Dictionary<string, string>() { { "redeliver_attempts", r.Parameters[0].manualAck } });
                //inner.Add(r.Parameters[0].redeliverAttempts, r.Parameters[0].redeliverDelay, r.Parameters[0].manualAck);
                config.Add(r.Parameters[0].Name, inner);
                object obj = new Object();
                e = new IndividualExchange.IndividualExchange(config, "xbill", obj, "IndividualExchange");
            }
            catch (Exception x)
            {
                this.EventLog.WriteEntry("Exception starting Altitude Exchange:" + x);
            }
            this.EventLog.WriteEntry("AltitudeIndividualHandlerService has started");
        }

        protected override void OnStop()
        {
            e = null;
            this.EventLog.WriteEntry("AltitudeIndividualHandlerService has stopped");
        }
        private void InitializeComponent()
        {

            this.ServiceName = "Altitude Individual Handler Service";

            this.CanStop = true;

            this.AutoLog = false;

            this.EventLog.Log = "Application";

            this.EventLog.Source = "Altitude Individual Handler Service";

        }
    }
}


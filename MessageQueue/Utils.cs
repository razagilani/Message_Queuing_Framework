using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageQueue
{
    static class Utils 
    {
        public static string make_queue_name(string recipient_exchange,
            string routing_key, string type = "direct")
        {
            string addon = "";
            if (type == "callback")
                addon = "_rpc_callback";
            else if (type == "task")
                addon = "_task_callback";
            else if (type == "delay")
                addon = "_delay";
            return string.Format("{0}_{1}{2}", recipient_exchange, 
                routing_key, addon);
        }
    }
}

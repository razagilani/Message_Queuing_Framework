using System;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AltitudeIndividualHandlerService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ServiceBase.Run(new AltitudeIndividualHandlerService());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageQueue
{
    public class ImproperlyConfiguredException : Exception
    {
        private string p;


        public ImproperlyConfiguredException(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
            Console.WriteLine(p);
        }
    }
    
    class MessageException : Exception
    {
        private string p;
        public MessageException(string p)
        {
            this.p = p;
        }
    }
    
    class MessageAcknowledgementAlreadySent : Exception
    {
    }
}

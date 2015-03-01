using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCC.Model
{
    public class AlertMessage
    {
        public AlertMessage(string message)
        {
            this.message = message;
        }

        public AlertMessage(string message, string color)
        {
            this.color = color;
            this.message = message;
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private string color = "white";
        public string Color
        {
            get { return color; }
            set { color = value; }
        }
    }
}

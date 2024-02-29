using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSDB
{
    public class Ticket
    {
        public string username { get; set; }
        public string productName { get; set; }
        public string payMethod { get; set; }
        public string loginMethod { get; set; }
        public int ticketNum { get; set; }
        public ulong ticketId { get; set; }
        public bool isClosed { get; set; }
        public string reason { get; set; }
    }
}

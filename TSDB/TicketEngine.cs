using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSDB
{
    public class TicketEngine
    {
        public bool StoreTicket(Ticket ticket)
        {
            try
            {
                /*
                string path = @"X:\Practika\TSDB\TSDB\bin\Debug\tickets.json";

                var json = File.ReadAllText(path);
                var jsonObj = JObject.Parse(json);
                    
                var tickets = jsonObj["tickets"].ToObject<List<Ticket>>();
                tickets.Add(ticket);

                jsonObj["tickets"] = JArray.FromObject(tickets);
                File.WriteAllText(path, jsonObj.ToString());

                return true;
                */
                string path = @"X:\Practika\TSDB\TSDB\bin\Debug\tickets.json";

                var json = File.ReadAllText(path);
                var jsonObj = JObject.Parse(json);

                var ticketsArray = jsonObj["tickets"] as JArray;
                if (ticketsArray == null)
                {
                    ticketsArray = new JArray();
                    jsonObj["tickets"] = ticketsArray;
                }

                ticketsArray.Add(JToken.FromObject(ticket));

                File.WriteAllText(path, jsonObj.ToString());
                return true;
            }
            catch(Exception ex) 
            {
                
                Console.WriteLine(ex);
                return false;
            }
            
        }
        /*
        public bool DeleteTicket(Ticket ticket)
        {
            return true;
        }
        */
    }
}

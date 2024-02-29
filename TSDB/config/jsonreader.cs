using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSDB
{
    internal class jsonreader
    {
        public string token { get; set; }
        public string prefix { get; set; }  
        public List<TicketItem> items { get; set; }
        public jsonreader()
        {
            this.items = new List<TicketItem>();
        }

        public async Task ReadJson()
        {
            using (StreamReader sr = new StreamReader("config.json")) 
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);
                this.token = data.token;
                this.prefix = data.prefix;
                List<TicketItem> items = new List<TicketItem>();
                foreach (JSONStructure item in data.items) {
                    this.items.Append(new TicketItem
                    {
                        name = item.name,
                        id = item.id,
                        description = item.description
                    });
                }
            }
        }
    }
    internal sealed class JSONStructure
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public string name { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public List<JSONStructure> items { get; set; }
    }
}

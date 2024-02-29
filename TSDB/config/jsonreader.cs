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
        public List<TicketPayment> payments { get; set; }
        public List<TicketJoin> joins { get; set; }
        public jsonreader()
        {
            this.items = new List<TicketItem>();
            this.payments = new List<TicketPayment>();
            this.joins = new List<TicketJoin>();
        }

        public async Task ReadJson()
        {
            using (StreamReader sr = new StreamReader("config.json")) 
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);
                this.token = data.token;
                this.prefix = data.prefix;
                foreach (var item in data.items)
                {
                    this.items.Add(new TicketItem
                    {
                        name = item.name,
                        id = item.id,
                        description = item.description,
                        emoji = item.emoji,
                    });
                }
                foreach (var payment in data.payments)
                {
                    this.payments.Add(new TicketPayment
                    {
                        name = payment.name,
                        id = payment.id,
                        description = payment.description,
                        emoji = payment.emoji,
                    });
                }
                foreach (var join in data.joins)
                {
                    this.joins.Add(new TicketJoin
                    {
                        name = join.name,
                        id = join.id,
                        description = join.description,
                        emoji = join.emoji,
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
        public ulong emoji { get; set; }
        public List<JSONStructure> items { get; set; }
        public List<JSONStructure> payments { get; set; }
        public List<JSONStructure> joins { get; set; }
    }
}

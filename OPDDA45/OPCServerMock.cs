using Opc.Da;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPDDA45
{
    public class OPCServerMock
    {
        public bool Connect(string serverurl)
        {
            return true;
        }

        public void DisConnect()
        {
            Console.WriteLine("OPCDA server is disconnected.");            
        }

        public bool IsConnected
        {
            get
            {
                return true;
            }
        }

        public ItemValueResult[] ReadTagVal(ICollection<string> tags)
        {


            ItemValueResult[] itemCollection = new ItemValueResult[tags.Count];
            var index = 0;
            Random rnd = new Random();

            foreach (var tag in tags)
            {
                ItemValueResult itemValueResult = new ItemValueResult();
                itemValueResult.ItemName = tag;
                if (tag.Contains("Trend Point.Metered Data.SITE0060B2_EcoMeter#012.Line 1 - Active Energy"))
                    itemValueResult.Value = 2005;
                else 
                    itemValueResult.Value = rnd.Next(1,50);
                itemValueResult.Timestamp = DateTime.UtcNow;
                itemCollection[index++] = itemValueResult;
            }
            return itemCollection;
        }
    }
}

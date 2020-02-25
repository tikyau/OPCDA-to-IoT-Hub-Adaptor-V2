using Opc.Da;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace OPDDA45
{
    /// <summary>
    /// Read-only OPC client
    /// </summary>
    public class OPCServerClient
    {
        private OPCServer _server;

        public OPCServerClient(OPCServer server)
        {
            if (server == null)
                throw new Exception("Server is null");

            _server = server;
        }

         /// <summary>
        /// Read tag value. 
        /// </summary>
        /// <returns>Tag value.</returns>
        public ItemValueResult[] ReadTagVal(ICollection<string> tags)
        {
            if (!_server.IsConnected)
                throw new Exception("Not connected to OPC server");

            Item[] itemCollection = new Item[tags.Count];
            var index = 0;
            foreach (var tag in tags)
            {
                itemCollection[index++] = new Item { ItemName = tag, MaxAge = -1 };
            }
            return _server.Read(itemCollection);
        }

      
    }
}
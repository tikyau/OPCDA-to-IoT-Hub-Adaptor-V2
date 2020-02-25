using Opc.Da;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace OPDDA45
{
    /// <summary>
    /// Summary description for OPCServer
    /// </summary>
    public class OPCServer
    {
        public Opc.Da.Server Server { get; set; }

        private StringBuilder _error = new StringBuilder();

        /// <summary>
        /// Tries to connect to the server.
        /// </summary>
        public bool Connect(string serverurl)
        {
            if (String.IsNullOrEmpty(serverurl))
                throw new Exception(String.Format("Server url '{0}' is not valid", serverurl));

            Opc.URL url = new Opc.URL(serverurl);
            OpcCom.Factory fact = new OpcCom.Factory();
            Server = new Opc.Da.Server(fact, null);
            try
            {
                Server.Connect(url, new Opc.ConnectData(new System.Net.NetworkCredential()));
            }
            catch (Exception ex)
            {
                _error.Append(ex.ToString());
                if (ex.InnerException != null)
                    _error.Append(ex.InnerException.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tries to disconnect to the server.
        /// </summary>
        private void DisConnect()
        {
            try
            {
                if (Server != null && Server.IsConnected)
                    Server.Disconnect();
            }
            catch (Exception ex)
            {
                _error.Append(ex.ToString());
                if (ex.InnerException != null)
                    _error.Append(ex.InnerException.ToString());
            }
        }
        /// <summary>
        /// Validates if the connection to the OPC server is still alive.
        /// </summary>
        /// <returns>Boolean flag.</returns>
        public bool IsConnected
        {
            get
            {
                return Server == null ? false : Server.IsConnected;
            }
        }

        /// <summary>
        /// Read values from OPC tags specified in 'itemCollection' array. 
        /// </summary>
        /// <returns>Array containing the current values of the OPC tags</returns>
        public ItemValueResult[] Read(Item[] itemCollection)
        {
            //_server.Read();
            if (itemCollection == null)
                return null;

            if (!Server.IsConnected)
                throw new Exception("Not connected to OPC server");

            return Server.Read(itemCollection);
        }

        /// <summary>
        /// Disconnect from server.
        /// </summary>
        /// <returns>Boolean flag.</returns>
        public void Disconnect()
        {
            if (IsConnected)
                Server.Disconnect();
        }

        /// <summary>
        /// Last error message.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                return _error.ToString();
            }
        }
    }
}
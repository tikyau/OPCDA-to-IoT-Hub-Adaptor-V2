using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OPDDA45
{
    public class OPCConfigurationManager
    {
        private XmlNode appSettings = null;
        XmlDocument doc = new XmlDocument();
        string xmlFilePath = string.Empty;

        public OPCConfigurationManager(string configFile)
        {
            doc.Load(configFile);
            appSettings = doc.SelectSingleNode("appSettings");
            xmlFilePath = configFile;
        }

        public string GetConfigurationValue(string configNodeName)
        {
            XmlNode gradesNode = appSettings.SelectSingleNode(configNodeName);

            return gradesNode.InnerText.Trim();
        }

        // Commenting this code because not sure about usage of this property.

        // After each device twin update we will check the value saved it
        public void SetConfigurationValue(string configNodeName, string value)
        {
            XmlNode gradesNode = appSettings.SelectSingleNode(configNodeName);

            gradesNode.InnerText = value;
            
            doc.Save(xmlFilePath);
        }

    }
}

using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using OPDDA45;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace simulated_device
{
    class SimulatedDevice
    {
        private static DeviceClient s_deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        static string deviceConnectionString = "";

        // This member will hold all tags and its metadata.
        static IDictionary<string, OPCDATag> tags = new Dictionary<string, OPCDATag>();

        // This member will hold OPC server Connection string.
        static string oPCDAServerUrl = "";

        // This member will hold stream interval to connect to server and read the tag's value.
        static string streamInterval = "";

        // This member will hold OPC server connection timeout error.
        static string connectionTimeout = "";

        const string configFleName = "config.xml";

        const string configFleName_Temp = "config_Temp.xml";

        // Async method to send tags telemetry
        private static async void SendDeviceToCloudMessagesAsync()
        {
            while (true)
            {
                OPCServerMock srv = new OPCServerMock();
                var isConnected = srv.Connect(oPCDAServerUrl);
                Console.WriteLine("Connected to OPCDA Server:" + srv.IsConnected);

                if (!isConnected)
                {
                    await Task.Delay(Convert.ToInt32(connectionTimeout) * 60000);
                    continue;
                }
                // OPCServerClient opcClient = new OPCServerClient(srv);
                // Create JSON message
                try
                {
                    var results = srv.ReadTagVal(tags.Keys);
                    Console.WriteLine("Number of tags data:" + results.Length);

                    List<IOTMessage> lstIOTMessage = new List<IOTMessage>();
                    List<IOTAlarmData> lstIOTAlarmMessage = new List<IOTAlarmData>();

                    foreach (var item in results)
                    {
                        var tagparts = item.ItemName.Split('.');
                        var siteidValue = tagparts[2].Substring(0, 10);
                        var floorNumber = siteidValue.Substring(8);
                        var device = tagparts[2].Substring(11).Split('#');
                        var lineNumber = "0";
                        var parameterName = "";
                        var opcTagObj = tags[item.ItemName];
                        if (tagparts[3].ToLower().Contains("line"))
                        {
                            lineNumber = tagparts[3].Split('-')[0].Trim();
                            parameterName = tagparts[3].Split('-')[1].Trim();
                        }
                        else
                            parameterName = tagparts[3].Trim();
                        var telemetryDataPoint = new IOTMessage
                        {
                            //OPCTag = item.ItemName,
                            id = System.Guid.NewGuid().ToString(),
                            metricCategory = opcTagObj.Description,
                            siteId = siteidValue,
                            FloorNumber = floorNumber,
                            DeviceName = device[0],
                            DeviceAddress = string.Concat("#", device[1]),
                            //ParameterName = parameterName,                         
                            //parameterValue = Convert.ToDouble(item.Value),
                            parameterValue = opcTagObj.Index * Convert.ToDouble(item.Value),
                            unit = opcTagObj.Unit,
                            LineNum = lineNumber,
                            ReceivedTime = item.Timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            messageType = "Metric"

                        };
                        lstIOTMessage.Add(telemetryDataPoint);
                        ThreshholdDetect(s_deviceClient, telemetryDataPoint, opcTagObj.ThresholdUpper, opcTagObj.ThresholdLower, lstIOTAlarmMessage);
                    }
                    if (lstIOTAlarmMessage.Count !=0)
                    {
                        var alarmMessage = new IOTAlarmMessage()
                        {
                            time = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            data = lstIOTAlarmMessage,
                            type = "Threshold",
                            messageType = "Alarm"
                        };
                        var alarmMsgString = JsonConvert.SerializeObject(alarmMessage);
                        var alarmMsg = new Message(Encoding.UTF8.GetBytes(alarmMsgString));
                        await s_deviceClient.SendEventAsync(alarmMsg);
                        Console.WriteLine("{0} > Sending alarm message: {1}", DateTime.Now, alarmMsgString);
                        lstIOTAlarmMessage.Clear();
                    }
                    

                    var messageString = JsonConvert.SerializeObject(lstIOTMessage);
                    var message = new Message(Encoding.UTF8.GetBytes(messageString));

                    // Add a custom application property to the message.
                    // An IoT hub can filter on these properties without access to the message body.
                    // message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                    // Send the telemetry message
                    await s_deviceClient.SendEventAsync(message);
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                    await Task.Delay(Convert.ToInt32(streamInterval));
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Not connected to OPC server")
                    {
                        continue;
                    }
                    throw ex;
                }
                finally
                {
                    srv.DisConnect();
                }
            }
        }

        private static void Main(string[] args)
        {
            // Read the latest config file available on the local server

            // Fixed by Bo - first time start this app there will no config file... with out connection with iot hub
            //ReadConfigFile();

            if (File.Exists(configFleName))
            {
                OPCConfigurationManager config = new OPCConfigurationManager(configFleName);
                deviceConnectionString = config.GetConfigurationValue("IOTHubConnectionString");


                // When file is not present or device connection string is empty then ask user to enter this value
                if (deviceConnectionString.Trim() == string.Empty)
                {
                    Console.WriteLine("Please in put Device Primary Connection String");
                    deviceConnectionString = Console.ReadLine();
                }

                s_deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt_WebSocket_Only);

                if (s_deviceClient == null)
                {
                    Console.WriteLine("Connect to IoT Device Failed Please Try Again!");
                    Console.ReadLine();
                    return;
                }
                else
                {
                    Console.WriteLine("Device Connected！");

                    config.SetConfigurationValue("IOTHubConnectionString", deviceConnectionString);

                    //HostName=bdbiothub.azure-devices.cn;DeviceId=testbo123;SharedAccessKey=Sy8zfhPZ8/91B0aQrVzC9A2WETPK7oL87InkgZVB9b8=

                    DeviceInitialize(s_deviceClient).GetAwaiter().GetResult();
                }

                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine("Sending OPCDA server data to iothub. Ctrl-C to exit.\n");
            }
            else
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine("Please make sure local config file Exist.\n");
            }

            Console.ReadLine();
        }

        private static void ThreshholdDetect(DeviceClient s_deviceClient,IOTMessage msg, string upper,string lower, List<IOTAlarmData> lstIOTAlarmMessage)
        {
            if (!upper.Equals(null) && !upper.Equals("NA"))
            {
                if(msg.parameterValue > Convert.ToDouble(upper))
                {
                    var alarmData = new IOTAlarmData()
                    {
                        id = System.Guid.NewGuid().ToString(),
                        alarmTypeId = "1309",
                        alarmStateId = "0",
                        eventTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        source = msg.metricCategory,
                        note = "System Alarm",
                        userId = "System",
                        siteId = msg.siteId
                    };
                    lstIOTAlarmMessage.Add(alarmData);
                    
                }
            }
            if (!lower.Equals(null) && !lower.Equals("NA"))
            {
                if (msg.parameterValue < Convert.ToDouble(lower))
                {
                    var alarmData = new IOTAlarmData()
                    {
                        id = System.Guid.NewGuid().ToString(),
                        alarmTypeId = "1310",
                        alarmStateId = "0",
                        eventTime = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        source = msg.metricCategory,
                        note = "System Alarm",
                        userId = "System",
                        siteId = msg.siteId
                    };
                    lstIOTAlarmMessage.Add(alarmData);
                }
            }
        }
        /// <summary>
        /// This method is used to parse the tags.
        /// </summary>
        /// <param name="tagsDetails"></param>
        /// <returns></returns>
        private static IDictionary<string, OPCDATag> ParseTags(string tagsDetails)
        {
            var tags = new Dictionary<string, OPCDATag>();
            string[] lines = tagsDetails.Split('\n');
            foreach (string line in lines)
            {
                var items = line.Split(',');
                var opcDATag = new OPCDATag();
                opcDATag.OPCTag = items[0].Trim();
                opcDATag.Description = items[1];
                opcDATag.Index = Convert.ToInt32(items[2]);
                opcDATag.Unit = items[3];
                opcDATag.ThresholdUpper = items[4];
                opcDATag.ThresholdLower = items[5];
                tags.Add(items[0].Trim(), opcDATag);
            }
            return tags;
        }

        private static void DownloadConfigFileAndSyncLocal(string blobSasUri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(blobSasUri);
                request.Method = "GET";
                XmlDocument document = new XmlDocument();
                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream s = resp.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(s, Encoding.UTF8))
                        {
                            document.LoadXml(reader.ReadToEnd());
                        }
                    }
                }

                document.Save(configFleName_Temp);
            }
            catch
            {
                Console.WriteLine("there no config file on the Cloud.");
            }

            if (File.Exists(configFleName_Temp))
            {
                OPCConfigurationManager config_temp = new OPCConfigurationManager(configFleName_Temp);
                OPCConfigurationManager config = new OPCConfigurationManager(configFleName);

                oPCDAServerUrl = config_temp.GetConfigurationValue("OPCDAServerUrl");
                config.SetConfigurationValue("OPCDAServerUrl", oPCDAServerUrl);

                streamInterval = config_temp.GetConfigurationValue("StreamInterval");
                config.SetConfigurationValue("StreamInterval", streamInterval);

                connectionTimeout = config_temp.GetConfigurationValue("ConnectionTimeout");
                config.SetConfigurationValue("ConnectionTimeout", connectionTimeout);

                tags = ParseTags(config_temp.GetConfigurationValue("Tags"));
                config.SetConfigurationValue("Tags", config_temp.GetConfigurationValue("Tags"));

                string temp_deviceConnectionString = config_temp.GetConfigurationValue("IOTHubConnectionString");
                if (temp_deviceConnectionString != deviceConnectionString && temp_deviceConnectionString != string.Empty)
                {
                    config.SetConfigurationValue("IOTHubConnectionString", deviceConnectionString);
                    Console.WriteLine("Please restart your APP for device connection string changed.\n");
                    Console.ReadLine();
                }
            }
            else
            {
                ReadConfigFile();
            }

        }

        private static void ReadConfigFile()
        {
            // Parse tags based on delimiter
            if (File.Exists(configFleName))
            {
                OPCConfigurationManager config = new OPCConfigurationManager(configFleName);
                deviceConnectionString = config.GetConfigurationValue("IOTHubConnectionString");
                oPCDAServerUrl = config.GetConfigurationValue("OPCDAServerUrl");
                streamInterval = config.GetConfigurationValue("StreamInterval");
                connectionTimeout = config.GetConfigurationValue("ConnectionTimeout");
                tags = ParseTags(config.GetConfigurationValue("Tags"));
            }
        }

        public static async Task DeviceInitialize(DeviceClient _deviceClient)
        {
            Console.WriteLine("Device Initialize...");

            await _deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertyChanged, null).ConfigureAwait(false);

            Console.WriteLine("Done");

            Console.WriteLine("Retrieving twin and Download config File...");

            Twin twin = await s_deviceClient.GetTwinAsync().ConfigureAwait(false);
            var desiredCollection = twin.Properties.Desired;
            //string blobSasUri = desiredCollection["tagFileSASUrl"];

            await OnDesiredPropertyChanged(desiredCollection, null);

            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Sending OPCDA server data to iothub. Ctrl-C to exit.\n");
            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async Task OnDesiredPropertyChanged(TwinCollection desiredProperties, object userContext)
        {
            Console.WriteLine("");
            Console.WriteLine("Desired property changed:");
            string blobSasUri = desiredProperties["tagFileSASUrl"];
            DownloadConfigFileAndSyncLocal(blobSasUri);
            Console.WriteLine("Sending current time as reported property");
            TwinCollection reportedProperties = new TwinCollection();
            reportedProperties["DateTimeLastDesiredPropertyChangeReceived"] = DateTime.Now;
            await s_deviceClient.UpdateReportedPropertiesAsync(reportedProperties).ConfigureAwait(false);
        }

    }

}
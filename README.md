# Streaming data from OPCDA Server to Azure IoT Hub

V1 version of this adaptor can be found here: https://github.com/tikyau/OPCDA_to_IoTHub

**Updates:**

1. Local Configuration file for Initialization

Client App initialization will be done according to the local configuration file (Config.xml) which includes device connection string, OPC server IP/ address, tags, message sending interval and threshold data for alarm triggers.  
The file can be modified manually and they will be parsed to the client app as parameters. The client app will automatically connect with the OPC DA server to extract the event messages and send data to IoT Hub. 

2. Device Twin & Azure Function for managements through the cloud 

A replica of the configuration file will be stored in Blob. An azure function will be listening to file changes in Blob and generate a blob SAS token which will be passed to the Device Twin attribute. Client app will receive the device twin notification, download and consume the latest config file without disconnection. 

3. Reliability Measures 

OPC DA server will temporarily stop during a system upgrade or a service downtime. The client app will have a timeout interval for server reconnection during this period to avoid any human intervention. The timeout interval is also configurable in the config file.

4. Cost Efficiency Measures

The client will combine all OPCDA tag data into a single message before sending to IoT Hub to save cost.

**Steps:**

This repo contain the full source code in C# that allows you to 
1. Remotely connect to your OPCDA Server.
3. Extract the relevant data at a regular predefined time interval from your OPCDA server by providing the tag names concerned .
4. Stream the data into your IoT Hub on Azure.

****Prerequisites***
Before you start this tutorial, you should have obtained all your tag names from an OPC Client. You can easily export all the tag names of your OPC server with an OPC Explorer as shown below:
![66d71a321004 1](https://user-images.githubusercontent.com/17831550/65606541-ac1f2700-dfdd-11e9-981f-e2a27a689ac8.gif)

# Installation and Deployment

1. Build and run the full solution (OPDDA45.sln) with Visual Studio 2017 is recommended. 

2. Fill in your OPCDA Server info (Host Name and Server Name), IoT Hub Connection String and define your stream interval in the app.config file.
   ![Capture2](https://user-images.githubusercontent.com/17831550/65575809-3dbd7300-dfa3-11e9-8145-3561c2f2c7f6.PNG)

3. Update the tag names in the Tags.txt file:
   ![tag](https://user-images.githubusercontent.com/17831550/65658916-37d79880-e05c-11e9-9cfb-53eb9bc68716.png)
   
4. Update the schema of your IoT Hub Message as desired in SimulatedDevice.cs as shown below:
   ![Capture4](https://user-images.githubusercontent.com/17831550/65575755-1e264a80-dfa3-11e9-86fa-c647cd6f90cf.PNG)

5. The repo contains all the DLLs that are required to establish the connection and extracting the tag data from your OPCDA Server:

   ![Capture1](https://user-images.githubusercontent.com/17831550/65575841-5168d980-dfa3-11e9-99a3-83da87348f23.PNG)

6. Successful console output:
   ![Screenshot (10)](https://user-images.githubusercontent.com/17831550/65575696-077ff380-dfa3-11e9-875c-072f0ae4a4bc.png)


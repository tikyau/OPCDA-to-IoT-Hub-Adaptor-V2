# Streaming data from OPCDA Server to Azure IoT Hub V2

V1 version of this adaptor can be found here: https://github.com/tikyau/OPCDA_to_IoTHub

**Updates:**

**1. Local Configuration file for Initialization**

Client App initialization will be done according to the local configuration file (Config.xml) which includes device connection string, OPC server IP/ address, tags, message sending interval and threshold data for alarm triggers.  
The file can be modified manually and they will be parsed to the client app as parameters. The client app will automatically connect with the OPC DA server to extract the event messages and send data to IoT Hub. 

**2. Device Twin & Azure Function for managements through the cloud** 

A replica of the configuration file will be stored in Blob. An azure function will be listening to file changes in Blob and generate a blob SAS token which will be passed to the Device Twin attribute. Client app will receive the device twin notification, download and consume the latest config file without disconnection. 

  Link to Function App setup: https://github.com/tikyau/OPCDA-Client-Function-App-Setup_V2

**3. Reliability Measures** 

OPC DA server will temporarily stop during a system upgrade or a service downtime. The client app will have a timeout interval for server reconnection during this period to avoid any human intervention. The timeout interval is also configurable in the config file.

**4. Cost Efficiency Measures**

The client will combine all OPCDA tag data into a single message before sending to IoT Hub to save cost.

**Steps:**

This repo contain the full source code in C# that allows you to 
1. Remotely connect to your OPCDA Server.
3. Extract the relevant data at a regular predefined time interval from your OPCDA server by providing the tag names concerned .
4. Stream the data into your IoT Hub on Azure.

****Prerequisites***
Before you start this tutorial, you should have obtained all your tag names from an OPC Client. You can easily export all the tag names of your OPC server with an OPC Explorer as shown below:
![66d71a321004 1](https://user-images.githubusercontent.com/17831550/65606541-ac1f2700-dfdd-11e9-981f-e2a27a689ac8.gif)

1. Setup IoT Hub and add a new device
2. For config management through the cloud: Setup Azure Function and update the Device Twin Attribute

# Installation and Deployment

1. Build and run the full solution (OPDDA45.sln) with Visual Studio 2017 is recommended.

2. Fill in your OPCDA Server info (Host Name and Server Name), IoT Hub Connection String, define your stream interval and timeout   interval in the config.xml file.

   ![image](https://user-images.githubusercontent.com/17831550/75311192-fb20ff00-5890-11ea-9bce-6be87da846d1.png)

3. Update the tag names in the config.xml file:

   ![image](https://user-images.githubusercontent.com/17831550/75311288-3f140400-5891-11ea-8b4b-221f0cc6e3b6.png)
   
4. Update the schema of your IoT Hub Message as desired in SimulatedDevice.cs as shown below:

   ![image](https://user-images.githubusercontent.com/17831550/75311367-7d112800-5891-11ea-9461-6027569acdc0.png)
   
5. The device twin code for config file update through Azure Cloud:

   ![image](https://user-images.githubusercontent.com/17831550/75311418-a336c800-5891-11ea-81be-224dd17c7d44.png)
   
   Overall Architecture for the device twin and Az Function operation:
   ![Device Twin](https://user-images.githubusercontent.com/17831550/75311009-61f1e880-5890-11ea-9198-b62fe4bc738d.png)

6. A mock up OPCDA server is included for you to test without a real server:

   ![image](https://user-images.githubusercontent.com/17831550/75311501-ec871780-5891-11ea-979c-5844e2d02c26.png)

7. The repo contains all the DLLs that are required to establish the connection and extracting the tag data from your OPCDA Server:

   ![Capture1](https://user-images.githubusercontent.com/17831550/65575841-5168d980-dfa3-11e9-99a3-83da87348f23.PNG)

8. Successful console output:

   ![Screenshot (10)](https://user-images.githubusercontent.com/17831550/65575696-077ff380-dfa3-11e9-875c-072f0ae4a4bc.png)


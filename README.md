# **Unity Mqtt**

It is a light and easy way to connect different devices and bound them throughout the MQTT broker.

## How to import
* **Package Manager**
  
  Open your Unity project and navigate **Windows/Package Manager/+/Add package from Git URL...** and copy `https://github.com/UnityGadgets/Mqtt.git` link to import.

### **Installation**

* Install Mosquitto
  - **Linux**

  `sudo apt-get install mosquitto` to install the package.

  `sudo apt-get install mosquitto-clients` to install the clients package.

  `systemctl status mosquitto` to see the current status.
  
  `sudo systemctl (start|stop) mosquitto` to change the status of the current session.

  `sudo systemctl (enable|disable) mosquitto` to change the startup behaviour.

  You can find the **mosquitto.conf** template file in the **/etc/mosquitto/** folder. Open the file in a text editor and set `listener 1883` and `allow_anonymous true`  

### **How it works**

Unity Mqtt package is using MQTTnet to utilize messaging throughout a broker. For detailed information about how it works, please read [MQTT wiki](https://www.hivemq.com/mqtt-essentials/).

**Menu/Tools/Mqtt/Create Client** to add a client instance object to the scene. Default connection data object will be added.

* **Mqtt Client Connection Data**: To create a new connection data right click in the project folder **Create/Mqtt/New Connection Data**.

  To connect the broker you need to know IP address of the broker.

* **Mqtt Events**

```csharp
event Action OnConnectRequested = delegate { };
event Action OnConnecting = delegate { };
event Action OnDisconnectRequested = delegate { };
event Action OnCancelRequested = delegate { };
event Action<string> OnConnected = delegate { };
event Action<string> OnDisconnected = delegate { };
event Action<string> OnSubscribed = delegate { };
event Action<string> OnSubscribeRequested = delegate { };
event Action<string> OnUnsubscribeRequested = delegate { };
event Action<string> OnRetainCacheRemoved = delegate { };
event Action<string, string> OnMessageReceived = delegate { };
event Action<string, string, bool, byte, string> OnPublishRequested = delegate { };
```

  - **Subscribe/Unsubscribe**

```csharp
public void Subscribe(string topic)
public void Unsubscribe(string topic)
```

  - **Publish**

```csharp
public void Publish(string topic, string message)
public void Publish(string topic, string message, bool retainFlag)
public void Publish(string topic, string message, bool retainFlag, byte QoSLevel)
```

  - **Message Receiving (Listening)**

The most convinient way to listen Mqtt messages is to add the code block below to your script.

```csharp
void OnEnable() {
  Mqtt.OnMessageReceived += MessageReceived;
}

void OnDisable() {
  Mqtt.OnMessageReceived -= MessageReceived;
}

void MessageReceived(string topic, string message) {
  ...
}
```

  - **Editor Events**

It is possible to listen events throughout editor without creating a C# script. Create a new empty GameObject to your hierarchy and **MqttEventListener.cs** script as a component like shown below.

  - **Cancel Connection**

```csharp
public void Cancel()
```

  - **Remove Retain Message**

  The last retain message is always stay throughout the whole mosquitto session. To clean the retain message for any topic, you can call the function below once. After that, you can get rid of it.

  ```csharp
  public void RemoveRetainCache(string topic)
  ```

  - **Built-in Topics**

 `status/offline` when they are disconnected. These messages are not retained.

# **License**
This tool is under MIT license.
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace UnityGadgets.Mqtt {
    [DisallowMultipleComponent, DefaultExecutionOrder(-100)]
    public class MqttClient : MonoBehaviour {
        public static bool IsConnected { get; private set; }
        public static string ClientID { get; private set; }

        [Header("Client Connection Settings")]
        public bool ShowLogs = true;
        public MqttConnectionDataSO ConnectionData;

        string _savePath;
        IMqttClient _client;
        MqttFactory _mqttFactory;
        MqttClientOptions _optionsBuilder;
        SynchronizationContext _synchronization;
        CancellationTokenSource _cts;

        const string disconnect_topic = "status/offline";
        const string publish_log_template = "<color=#00B2D9>[Published]</color> T: <b>[{0}]</b> M: <b>[{1}]</b>";
        const string receive_log_template = "<color=#FAA307>[Received]</color> T: <b>[{0}]</b> M: <b>[{1}]</b>";

        void OnEnable() {
            Mqtt.OnConnectRequested += Connect;
            Mqtt.OnDisconnectRequested += Disconnect;
            Mqtt.OnSubscribeRequested += Subscribe;
            Mqtt.OnUnsubscribeRequested += Unsubscribe;
            Mqtt.OnPublishRequested += Publish;
            Mqtt.OnRetainCacheRemoved += RemoveRetainCache;
            Mqtt.OnCancelRequested += Cancel;
        }

        void OnDisable() {
            Cancel();
            Mqtt.OnConnectRequested -= Connect;
            Mqtt.OnDisconnectRequested -= Disconnect;
            Mqtt.OnSubscribeRequested -= Subscribe;
            Mqtt.OnUnsubscribeRequested -= Unsubscribe;
            Mqtt.OnPublishRequested -= Publish;
            Mqtt.OnRetainCacheRemoved -= RemoveRetainCache;
            Mqtt.OnCancelRequested -= Cancel;
        }

        void Connect() => _ = ConnectAsync(ConnectionData.ReconnectionPeriodInSecond).ConfigureAwait(false);
        void Disconnect() => _ = _client.DisconnectAsync().ConfigureAwait(false);
        void Subscribe(params string[] topics) => _ = SubscribeAsync(topics);
        void Unsubscribe(string topic) => _client.UnsubscribeAsync(topic, _cts.Token).ConfigureAwait(false);
        void Publish(string topic, string message, bool retainFlag, byte QoSLevel, string responseTopic) => _ = PublishAsync(topic, message, retainFlag, QoSLevel, responseTopic).ConfigureAwait(false);
        void RemoveRetainCache(string topic) => _ = PublishAsync(topic, string.Empty, true, 2, string.Empty).ConfigureAwait(false);

        void Awake() {
            IsConnected = false;
            _savePath = Path.Combine(Application.persistentDataPath, $"{ConnectionData.ClientID}_connection.json");
            _synchronization = SynchronizationContext.Current;

            if (ConnectionData.AutoConnect)
                Connect();
        }

        string ConnectionClientId() {
            if (string.IsNullOrWhiteSpace(ConnectionData.ClientID)) {
                return Guid.NewGuid().ToString();
            }

            if (ConnectionData.RandomClientIDSuffix) {
                return $"{ConnectionData.ClientID}-{Guid.NewGuid()}";
            }
            else {
                return ConnectionData.ClientID;
            }
        }

        async Task ConnectAsync(int reconnectionPeriodInSec) {
            _cts = new CancellationTokenSource();

            await LoadDataAsync();
            await Task.Delay(TimeSpan.FromSeconds(ConnectionData.InitialDelayInSecond), _cts.Token);

            _mqttFactory = new MqttFactory();

            using (_client = _mqttFactory.CreateMqttClient()) {
                ClientID = ConnectionClientId();

                _optionsBuilder = new MqttClientOptionsBuilder()
                        .WithClientId(ClientID)
                        .WithTcpServer(ConnectionData.Host, ConnectionData.Port)
                        .WithCredentials(ConnectionData.UserName, ConnectionData.Password)
                        .WithWillTopic(disconnect_topic)
                        .WithWillPayload(ClientID)
                        .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                        .WithWillRetain(false)
                        .WithCleanSession()
                        .Build();

                while (!_cts.IsCancellationRequested) {
                    try {
                        if (!_client.IsConnected) {
                            _client.ConnectedAsync += ConnectedAsync;
                            _client.DisconnectedAsync += DisconnectedAsync;
                            _client.ApplicationMessageReceivedAsync += MessageReceivedAsync;

                            if (ShowLogs)
                                Debug.Log($"Connecting to <b>{ConnectionData.Host}</b>");

                            Mqtt.Connecting();
                            await _client.ConnectAsync(_optionsBuilder, _cts.Token).ConfigureAwait(false);
                            IsConnected = true;
                        }
                    }
                    catch (Exception ex) {
                        switch (ex) {
                            case OperationCanceledException:
                                if (ShowLogs)
                                    Debug.LogWarning($"<b>Canceled!</b>");
                                break;
                            default:
                                if (ShowLogs)
                                    Debug.LogError($"Connection error: {ex.Message}");
                                break;
                        }
                    }
                    finally {
                        await Task.Delay(TimeSpan.FromSeconds(reconnectionPeriodInSec), _cts.Token);

                        if (!_client.IsConnected) {
                            _client.ConnectedAsync -= ConnectedAsync;
                            _client.DisconnectedAsync -= DisconnectedAsync;
                            _client.ApplicationMessageReceivedAsync -= MessageReceivedAsync;
                        }
                    }
                }
            }
        }

        async Task ConnectedAsync(MqttClientConnectedEventArgs e) {
            if (ShowLogs)
                Debug.Log($"<color=#8AC926><b>Connected!</b></color>");

            Mqtt.Connected(ConnectionData.ClientID);

            if (ConnectionData.SaveOnDisk)
                await SaveData();
            else if (File.Exists(_savePath))
                File.Delete(_savePath);

            if (ConnectionData.AutoSubscribe)
                for (int i = 0; i < ConnectionData.Topics.Count; i++)
                    await SubscribeAsync(ConnectionData.Topics[i]).ConfigureAwait(false);
        }

        async Task SubscribeAsync(params string[] topics) {
            for (int i = 0; i < topics.Length; i++) {
                var topicFilter = _mqttFactory.CreateTopicFilterBuilder()
                    .WithTopic(topics[i])
                    .Build();

                var options = _mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(topicFilter)
                    .Build();

                await _client.SubscribeAsync(options, _cts.Token).ConfigureAwait(false);
                Mqtt.Subscribed(topics[i]);
            }
        }

        async Task PublishAsync(string topic, string message, bool retained, byte qualityOfServiceLevel, string responseTopic) {
            if (!_client.IsConnected)
                return;

            var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(message)
                    .WithRetainFlag(retained)
                    .WithResponseTopic(responseTopic)
                    .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qualityOfServiceLevel)
                    .Build();

            await _client.PublishAsync(applicationMessage, _cts.Token).ConfigureAwait(false);

            if (ShowLogs)
                Debug.Log(string.Format(publish_log_template, topic, message));
        }

        Task MessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e) {
            string topic = e.ApplicationMessage.Topic;
            string message = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            _synchronization.Post((_) => Mqtt.ReceiveMessage(topic, message), null);

            if (ShowLogs)
                Debug.Log(string.Format(receive_log_template, topic, message));

            return Task.CompletedTask;
        }

        Task DisconnectedAsync(MqttClientDisconnectedEventArgs e) {
            if (!_client.IsConnected)
                return Task.CompletedTask;

            if (ShowLogs)
                Debug.LogWarning($"<color=#E65C04><b>Disconnected!</b></color>");

            IsConnected = false;
            Mqtt.Disconnected(ConnectionData.ClientID);

            return Task.CompletedTask;
        }

        void Cancel() {
            if (_cts != null) {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        async Task SaveData() {
            string content = JsonUtility.ToJson((ConnectionData.Host, ConnectionData.Port, ConnectionData.AutoConnect));
            await File.WriteAllTextAsync(_savePath, content);
        }

        async Task LoadDataAsync() {
            if (File.Exists(_savePath)) {
                string content = await File.ReadAllTextAsync(_savePath, _cts.Token);
                (string, int, bool) result = JsonUtility.FromJson<(string, int, bool)>(content);

                ConnectionData.Host = result.Item1;
                ConnectionData.Port = result.Item2;
                ConnectionData.AutoConnect = result.Item3;
            }
        }
    }
}
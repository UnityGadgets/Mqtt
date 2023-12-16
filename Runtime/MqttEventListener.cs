using UnityEngine;
using UnityEngine.Events;

namespace UnityGadgets.Mqtt {
    public class MqttEventListener : MonoBehaviour {
        [SerializeField] UnityEvent OnConnect = default;
        [SerializeField] UnityEvent OnCancel = default;
        [SerializeField] UnityEvent OnDisconnect = default;
        [SerializeField] UnityEvent<string, string> OnPublish = default;
        [SerializeField] UnityEvent<string, string> OnReceive = default;

        void OnEnable() {
            Mqtt.OnConnected += HandleConnect;
            Mqtt.OnCancelRequested += HandleCancel;
            Mqtt.OnDisconnected += HandleDisconnect;
            Mqtt.OnPublishRequested += HandlePublish;
            Mqtt.OnMessageReceived += HandleReceive;
        }

        void OnDisable() {
            Mqtt.OnConnected -= HandleConnect;
            Mqtt.OnCancelRequested -= HandleCancel;
            Mqtt.OnDisconnected -= HandleDisconnect;
            Mqtt.OnPublishRequested -= HandlePublish;
            Mqtt.OnMessageReceived -= HandleReceive;
        }

        void HandleDisconnect(string clientID) {
            OnDisconnect?.Invoke();
        }

        void HandleCancel() {
            OnCancel?.Invoke();
        }

        void HandleConnect(string clientID) {
            OnConnect?.Invoke();
        }

        void HandleReceive(string topic, string message) {
            OnReceive?.Invoke(topic, message);
        }

        void HandlePublish(string t, string m, bool retain, byte QoS, string responseT) {
            OnPublish?.Invoke(t, m);
        }
    }
}

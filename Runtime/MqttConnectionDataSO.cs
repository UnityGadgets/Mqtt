using System.Collections.Generic;
using UnityEngine;

namespace UnityGadgets.Mqtt {
    [CreateAssetMenu(fileName = "NewConnectionData", menuName = "Mqtt/New Connection Data", order = 0)]
    public class MqttConnectionDataSO : ScriptableObject {
        [Header("Settings")]
        public string Host;
        public int Port;
        public string ClientID;
        public bool RandomClientIDSuffix;

        [Header("Authentication")]
        public string UserName;
        public string Password;

        [Header("Connect Options")]
        public bool AutoConnect;
        public bool SaveOnDisk;
        public int InitialDelayInSecond = 0;
        public int ReconnectionPeriodInSecond = 5;

        [Header("Topic & Subscribtion")]
        public bool AutoSubscribe;
        public List<string> Topics;

        void OnEnable() {
#if UNITY_STANDALONE
            new CLI().LoadData(this, "mqtt");
#endif
        }
    }
}

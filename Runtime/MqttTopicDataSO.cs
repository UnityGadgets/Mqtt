using UnityEngine;

namespace UnityGadgets.Mqtt {
    [CreateAssetMenu(fileName = "NewTopicData", menuName = "Mqtt/New Topic Data", order = 1)]
    public class MqttTopicDataSO : ScriptableObject {
        public string Topic;
        [TextArea(2, 4)] public string Message;
#if UNITY_EDITOR
        [Header("Editor only")]
        [TextArea(2, 4)] public string Definition;
#endif
    }
}

using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityGadgets.Mqtt.Editor {
    public class MqttEditor {
        const string create_client_path = "Tools/Mqtt/Create Client";

        [MenuItem(create_client_path)]
        static void CreateClient() {
            MqttConnectionDataSO data = null;
            GameObject client = new GameObject("Mqtt", typeof(MqttClient));
            string settingPath = Path.Combine("Assets", "Settings");

            if (!Directory.Exists(settingPath)) {
                Directory.CreateDirectory(settingPath);
            }

            string[] guids = AssetDatabase.FindAssets("t:MqttConnectionDataSO");
            string assetPath = AssetDatabase.GUIDToAssetPath(guids.Length > 0 ? guids[0] : null);

            if (!Directory.Exists(assetPath)) {
                data = ScriptableObject.CreateInstance<MqttConnectionDataSO>();
                AssetDatabase.CreateAsset(data, Path.Combine(settingPath, "DefaultMqttClient.asset"));
            }
            else {
                data = AssetDatabase.LoadAssetAtPath<MqttConnectionDataSO>(assetPath);
            }

            data.Host = "192.168.0.";
            data.Port = 1883;
            data.AutoConnect = true;
            data.AutoSubscribe = true;
            client.GetComponent<MqttClient>().ConnectionData = data;

            EditorUtility.SetDirty(client);
            AssetDatabase.SaveAssets();
        }

        [MenuItem(create_client_path, true)]
        static bool ValidateCreateClient() {
            MqttClient current = Object.FindObjectOfType<MqttClient>();
            return current == null;
        }
    }
}
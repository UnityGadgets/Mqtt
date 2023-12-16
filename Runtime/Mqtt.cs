using System;
using UnityEngine;

namespace UnityGadgets.Mqtt {
    public static class Mqtt {
        public static event Action OnConnectRequested = delegate { };
        public static event Action OnConnecting = delegate { };
        public static event Action OnDisconnectRequested = delegate { };
        public static event Action OnCancelRequested = delegate { };
        public static event Action<string> OnConnected = delegate { };
        public static event Action<string> OnDisconnected = delegate { };
        public static event Action<string> OnSubscribed = delegate { };
        public static event Action<string[]> OnSubscribeRequested = delegate { };
        public static event Action<string> OnUnsubscribeRequested = delegate { };
        public static event Action<string> OnRetainCacheRemoved = delegate { };
        public static event Action<string, string> OnMessageReceived = delegate { };
        public static event Action<string, string, bool, byte, string> OnPublishRequested = delegate { };

        static bool _isCanceling = false;

        public static void Connect() {
            OnConnectRequested?.Invoke();
        }

        public static void Connecting() {
            OnConnecting?.Invoke();
        }

        public static void Connected(string clientID) {
            OnConnected?.Invoke(clientID);
        }

        public static void Disconnect() {
            OnDisconnectRequested?.Invoke();
        }

        public static void Subscribe(params string[] topics) {
            OnSubscribeRequested?.Invoke(topics);
        }

        public static void Subscribed(string topic) {
            OnSubscribed?.Invoke(topic);
        }

        public static void Unsubscribe(string topic) {
            OnUnsubscribeRequested?.Invoke(topic);
        }

        public static void Cancel() {
            _isCanceling = true;
            OnCancelRequested?.Invoke();
        }

        public static void Disconnected(string clientID) {
            if (!_isCanceling)
                OnDisconnected?.Invoke(clientID);

            _isCanceling = false;
        }

        public static void Publish(string topic, string message) {
            OnPublishRequested?.Invoke(topic, message, false, 0, string.Empty);
        }

        public static void Publish(string topic, string message, bool retainFlag) {
            OnPublishRequested?.Invoke(topic, message, retainFlag, 0, string.Empty);
        }

        public static void Publish(string topic, string message, bool retainFlag, byte QoSLevel) {
            OnPublishRequested?.Invoke(topic, message, retainFlag, (byte)Mathf.Clamp(QoSLevel, 0, 2), string.Empty);
        }

        /// <summary>
        /// </summary>
        /// <param name="topic">
        ///     A wildcard can only be used to subscribe to topics, not to publish a message.
        /// </param>
        /// <param name="message">
        ///     Empty or Null value is not going to be received by subscribers.
        /// </param>
        /// <param name="QoSLevel">
        ///     The Quality of Service (QoS) level is an agreement between the sender of a message and the receiver of a message
        ///     that defines the guarantee of delivery for a specific message.
        ///     <list type="bullet">
        ///         <item>
        ///         <description>At most once  (0): Message gets delivered no time, once or multiple times. Fastest.</description>
        ///         </item>
        ///         <item>
        ///         <description>At least once (1): Message gets delivered at least once (one time or more often).</description>
        ///         </item>
        ///         <item>
        ///         <description>Exactly once  (2): Message gets delivered exactly once (It's ensured that the message only comes once). Slowest.</description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <param name="retainFlag">
        ///     Whether this message should be retained.
        /// </param>
        public static void Publish(string topic, string message, bool retainFlag, byte QoSLevel, string responseTopic) {
            OnPublishRequested?.Invoke(topic, message, retainFlag, (byte)Mathf.Clamp(QoSLevel, 0, 2), responseTopic);
        }


        /// <summary>
        /// </summary>
        /// <param name="topic">
        ///     There are two different kinds of wildcards: <c>single-level</c> and multi-level.
        ///     <list type="bullet">
        ///         <item>
        ///         <description>Single-level "+": a single-level wildcard replaces one topic level.</description>
        ///         </item>
        ///         <item>
        ///         <description>Multi-level "#": a multi-level wildcard covers many topic levels.</description>
        ///         </item>
        ///     </list>
        /// </param>
        public static void ReceiveMessage(string topic, string message) {
            OnMessageReceived?.Invoke(topic, message);
        }

        public static void RemoveRetainCache(string topic) {
            OnRetainCacheRemoved?.Invoke(topic);
        }
    }
}

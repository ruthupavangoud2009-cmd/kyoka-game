using UnityEngine;

namespace Kyoka.Networking
{
    public enum KyokaNetworkMode { OfflinePrototype, RelayPrototype, DedicatedServer }

    /// <summary>
    /// Explicit networking boundary. This component does NOT create an online connection.
    /// Antigravity must integrate and test a real networking provider before enabling online claims.
    /// </summary>
    public class KyokaNetworkStatus : MonoBehaviour
    {
        public KyokaNetworkMode configuredMode = KyokaNetworkMode.OfflinePrototype;
        public bool IsActuallyConnected { get; private set; }

        public void SetConnectionResult(bool connected)
        {
            IsActuallyConnected = connected;
        }

        public string GetStatusText()
        {
            return IsActuallyConnected ? "Connected" : "Offline prototype — online service not configured";
        }
    }
}

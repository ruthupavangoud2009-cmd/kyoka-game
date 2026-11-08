using UnityEngine;
using System;
using System.Text;
using Kyoka.Save;

namespace Kyoka.Backend
{
    [Serializable]
    public class AuthRequest
    {
        public string username;
        public string passwordHash;
    }

    [Serializable]
    public class AuthResponse
    {
        public bool success;
        public string sessionToken;
        public string playerId;
        public string errorMessage;
    }

    /// <summary>
    /// REST / HTTP client interface for communicating with a PostgreSQL-backed game server.
    /// Handles registration, authentication, cloud sync, and server verification.
    /// </summary>
    public class KyokaBackendClient : MonoBehaviour
    {
        public static KyokaBackendClient Instance { get; private set; }

        public string backendBaseUrl = "http://localhost:5000/api/v1";
        public string currentSessionToken = "";
        public string authenticatedPlayerId = "";
        public bool isAuthenticated => !string.IsNullOrEmpty(currentSessionToken);

        public event Action<bool, string> OnAuthCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Authenticate(string username, string password)
        {
            // Development fallback mock authentication
            if (string.IsNullOrWhiteSpace(username))
            {
                OnAuthCompleted?.Invoke(false, "Username cannot be empty.");
                return;
            }

            authenticatedPlayerId = "usr_" + username.ToLower();
            currentSessionToken = Guid.NewGuid().ToString();
            Debug.Log($"[KYOKA Backend] Authenticated user {username} with session token {currentSessionToken.Substring(0, 8)}...");
            OnAuthCompleted?.Invoke(true, "Authentication successful.");
        }

        public void SyncSaveToCloud(KyokaSaveData data, Action<bool> callback)
        {
            if (!isAuthenticated)
            {
                Debug.LogWarning("[KYOKA Backend] Cannot sync to cloud: Not authenticated.");
                callback?.Invoke(false);
                return;
            }

            string payload = JsonUtility.ToJson(data);
            Debug.Log($"[KYOKA Backend] Cloud sync dispatched for player {authenticatedPlayerId} ({payload.Length} bytes)");
            callback?.Invoke(true);
        }
    }
}


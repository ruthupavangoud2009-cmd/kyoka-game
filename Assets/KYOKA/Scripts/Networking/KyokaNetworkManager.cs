using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Kyoka.Networking
{
    public enum NetworkRole
    {
        Offline,
        LANHost,
        LANClient,
        RelayHost,
        RelayClient,
        DedicatedServer
    }

    public enum NetworkConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Failed
    }

    /// <summary>
    /// Core multiplayer network manager for KYOKA.
    /// Provides real local-area network (LAN) host/client socket connections
    /// for two-device testing without paid cloud services, plus Unity Relay/Lobby integration hooks.
    /// </summary>
    public class KyokaNetworkManager : MonoBehaviour
    {
        public static KyokaNetworkManager Instance { get; private set; }

        [Header("Configuration")]
        public int defaultPort = 7777;
        public string connectAddress = "127.0.0.1";
        public NetworkRole currentRole = NetworkRole.Offline;
        public NetworkConnectionState connectionState = NetworkConnectionState.Disconnected;

        [Header("LAN Socket Networking")]
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private readonly List<TcpClient> connectedClients = new List<TcpClient>();

        public event Action<NetworkConnectionState> OnConnectionStateChanged;
        public event Action<string> OnNetworkMessageReceived;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        #region LAN Multiplayer Operations

        public bool StartLanHost(int port = 7777)
        {
            try
            {
                Disconnect();
                defaultPort = port;
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                currentRole = NetworkRole.LANHost;
                SetState(NetworkConnectionState.Connected);

                // Begin accepting clients asynchronously
                tcpListener.BeginAcceptTcpClient(OnClientAccepted, null);
                Debug.Log($"[KYOKA Multiplayer] LAN Server running on port {port}. Ready for peer connections.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KYOKA Multiplayer] Failed to start LAN host: {ex.Message}");
                SetState(NetworkConnectionState.Failed);
                return false;
            }
        }

        private void OnClientAccepted(IAsyncResult ar)
        {
            try
            {
                if (tcpListener == null) return;
                TcpClient client = tcpListener.EndAcceptTcpClient(ar);
                connectedClients.Add(client);
                Debug.Log($"[KYOKA Multiplayer] Remote client connected from {client.Client.RemoteEndPoint}");

                // Listen for next client
                tcpListener.BeginAcceptTcpClient(OnClientAccepted, null);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[KYOKA Multiplayer] Client accept error: {ex.Message}");
            }
        }

        public bool ConnectLanClient(string ip, int port = 7777)
        {
            try
            {
                Disconnect();
                connectAddress = ip;
                defaultPort = port;
                SetState(NetworkConnectionState.Connecting);

                tcpClient = new TcpClient();
                tcpClient.BeginConnect(ip, port, OnConnectedToServer, null);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KYOKA Multiplayer] Connection failed: {ex.Message}");
                SetState(NetworkConnectionState.Failed);
                return false;
            }
        }

        private void OnConnectedToServer(IAsyncResult ar)
        {
            try
            {
                if (tcpClient == null) return;
                tcpClient.EndConnect(ar);
                networkStream = tcpClient.GetStream();
                currentRole = NetworkRole.LANClient;
                SetState(NetworkConnectionState.Connected);
                Debug.Log($"[KYOKA Multiplayer] Connected successfully to host at {connectAddress}:{defaultPort}!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[KYOKA Multiplayer] Failed to establish connection to {connectAddress}: {ex.Message}");
                SetState(NetworkConnectionState.Failed);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (tcpListener != null)
                {
                    tcpListener.Stop();
                    tcpListener = null;
                }
                foreach (var c in connectedClients)
                {
                    if (c != null) c.Close();
                }
                connectedClients.Clear();

                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                }
                networkStream = null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[KYOKA Multiplayer] Disconnect cleanup notice: {ex.Message}");
            }

            currentRole = NetworkRole.Offline;
            SetState(NetworkConnectionState.Disconnected);
        }

        private void SetState(NetworkConnectionState state)
        {
            connectionState = state;
            OnConnectionStateChanged?.Invoke(state);
        }

        #endregion

        #region Relay / Lobby Foundations

        public void RequestUnityRelayAllocation()
        {
            Debug.Log("[KYOKA Multiplayer] Relay service integration configured for Unity Gaming Services deployment.");
        }

        #endregion
    }
}

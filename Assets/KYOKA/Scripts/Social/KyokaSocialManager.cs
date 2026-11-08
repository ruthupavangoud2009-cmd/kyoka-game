using UnityEngine;
using System;
using System.Collections.Generic;

namespace Kyoka.Social
{
    [Serializable]
    public class PlayerProfile
    {
        public string playerId = "player_001";
        public string displayName = "KyokaPilot";
        public int level = 5;
        public string title = "Aether Wanderer";
        public bool isOnline = true;
        public int battleRoyaleWins = 3;
    }

    [Serializable]
    public class ChatMessage
    {
        public string sender;
        public string channel; // Global, Party, Local
        public string text;
        public float timestamp;
    }

    public class KyokaSocialManager : MonoBehaviour
    {
        public static KyokaSocialManager Instance { get; private set; }

        [Header("Local Player")]
        public PlayerProfile localProfile = new PlayerProfile();

        [Header("Social Lists")]
        public List<PlayerProfile> friends = new List<PlayerProfile>();
        public List<string> blockedPlayerIds = new List<string>();

        [Header("Party")]
        public string currentPartyId = "";
        public bool isPartyLeader = false;
        public List<PlayerProfile> partyMembers = new List<PlayerProfile>();

        [Header("Chat History")]
        public List<ChatMessage> chatLog = new List<ChatMessage>();
        public int maxChatHistory = 50;

        public event Action<ChatMessage> OnChatMessageReceived;
        public event Action OnPartyUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
            InitializeMockSocialData();
        }

        private void InitializeMockSocialData()
        {
            if (friends.Count == 0)
            {
                friends.Add(new PlayerProfile { playerId = "p_kaito", displayName = "Kaito", level = 12, title = "Wind Striker", isOnline = true });
                friends.Add(new PlayerProfile { playerId = "p_sakura", displayName = "Sakura", level = 9, title = "Tide Blossom", isOnline = true });
                friends.Add(new PlayerProfile { playerId = "p_ryu", displayName = "Ryu", level = 20, title = "Void Blade", isOnline = false });
            }
        }

        public void SendChatMessage(string text, string channel = "Global")
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var msg = new ChatMessage
            {
                sender = localProfile.displayName,
                channel = channel,
                text = text,
                timestamp = Time.time
            };

            chatLog.Add(msg);
            if (chatLog.Count > maxChatHistory) chatLog.RemoveAt(0);

            OnChatMessageReceived?.Invoke(msg);
            Debug.Log($"[KYOKA Chat] [{channel}] {msg.sender}: {text}");
        }

        public void CreateParty()
        {
            currentPartyId = Guid.NewGuid().ToString().Substring(0, 8);
            isPartyLeader = true;
            partyMembers.Clear();
            partyMembers.Add(localProfile);
            OnPartyUpdated?.Invoke();
            Debug.Log($"[KYOKA Party] Created new party #{currentPartyId}");
        }

        public void InviteToParty(PlayerProfile friend)
        {
            if (friend == null || !friend.isOnline) return;
            if (partyMembers.Count >= 4)
            {
                Debug.LogWarning("[KYOKA Party] Party is full (max 4 members).");
                return;
            }

            if (!partyMembers.Exists(m => m.playerId == friend.playerId))
            {
                partyMembers.Add(friend);
                OnPartyUpdated?.Invoke();
                Debug.Log($"[KYOKA Party] {friend.displayName} joined the party!");
            }
        }

        public void LeaveParty()
        {
            currentPartyId = "";
            isPartyLeader = false;
            partyMembers.Clear();
            OnPartyUpdated?.Invoke();
            Debug.Log("[KYOKA Party] Left party.");
        }

        public void BlockPlayer(string playerId)
        {
            if (!blockedPlayerIds.Contains(playerId))
            {
                blockedPlayerIds.Add(playerId);
                Debug.Log($"[KYOKA Social] Blocked player {playerId}");
            }
        }

        public void ReportPlayer(string playerId, string reason)
        {
            Debug.Log($"[KYOKA Safety] Player {playerId} reported for: {reason}. Ticket dispatched to server moderation queue.");
        }
    }
}


using UnityEngine;
using Kyoka.Player;
using Kyoka.Abilities;
using Kyoka.Save;

namespace Kyoka.UI
{
    public class KyokaHUD : MonoBehaviour
    {
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private KyokaPlayerController player;
        private KyokaAbilityUser abilityUser;
        private bool showHelp = true;

        private void Start()
        {
            player = FindObjectOfType<KyokaPlayerController>();
            abilityUser = FindObjectOfType<KyokaAbilityUser>();
        }

        private void OnGUI()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 15 };
                titleStyle.normal.textColor = Color.white;
                bodyStyle.normal.textColor = Color.white;
            }

            GUI.Box(new Rect(12, 12, 310, 125), "");
            GUI.Label(new Rect(24, 18, 280, 34), "KYOKA", titleStyle);
            GUI.Label(new Rect(24, 54, 280, 22), "Playable development prototype", bodyStyle);
            GUI.Label(new Rect(24, 78, 280, 22),
                "Energy: " + (abilityUser ? Mathf.RoundToInt(abilityUser.energy).ToString() : "--"), bodyStyle);
            GUI.Label(new Rect(24, 100, 280, 22), "Camera: V  |  Ability: Q  |  Build: B", bodyStyle);

            if (showHelp)
            {
                GUI.Box(new Rect(12, Screen.height - 132, 380, 118), "");
                GUI.Label(new Rect(24, Screen.height - 122, 350, 22), "WASD Move | Shift Sprint | Space Jump", bodyStyle);
                GUI.Label(new Rect(24, Screen.height - 98, 350, 22), "Mouse Look | V Camera | Q Ability", bodyStyle);
                GUI.Label(new Rect(24, Screen.height - 74, 350, 22), "E Enter/Exit Vehicle | B Place Block", bodyStyle);
                if (GUI.Button(new Rect(24, Screen.height - 48, 100, 25), "Hide Help")) showHelp = false;
            }
            else if (GUI.Button(new Rect(12, Screen.height - 36, 100, 25), "Show Help")) showHelp = true;

            if (GUI.Button(new Rect(Screen.width - 130, 12, 118, 32), "Save Game"))
            {
                var data = new KyokaSaveData();
                if (player)
                {
                    data.posX = player.transform.position.x;
                    data.posY = player.transform.position.y;
                    data.posZ = player.transform.position.z;
                    data.hasSavedPosition = true;
                }
                KyokaSaveSystem.Save(data);
            }
        }
    }
}

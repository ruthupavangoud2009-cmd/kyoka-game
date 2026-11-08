using UnityEngine;
using Kyoka.Player;
using Kyoka.Core;
using Kyoka.Missions;

namespace Kyoka.World
{
    public enum NPCType
    {
        Citizen,
        QuestGiver,
        Shopkeeper,
        PoliceOfficer
    }

    public class KyokaNPC : MonoBehaviour
    {
        public NPCType npcType = NPCType.Citizen;
        public string npcName = "Ren";
        [TextArea] public string greetingDialogue = "Welcome to Kyoka City! Watch out for the storm in the Wildlands.";
        public float interactionDistance = 3.5f;

        [Header("Patrol Waypoints")]
        public Transform[] patrolPoints;
        public float walkSpeed = 2.0f;
        private int currentWaypointIndex = 0;
        private CharacterController controller;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                Patrol();
            }

            CheckPlayerInteraction();
        }

        private void Patrol()
        {
            Transform targetPoint = patrolPoints[currentWaypointIndex];
            if (targetPoint == null) return;

            Vector3 direction = (targetPoint.position - transform.position);
            direction.y = 0;

            if (direction.magnitude < 1.0f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolPoints.Length;
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), 5f * Time.deltaTime);
                if (controller) controller.Move(direction.normalized * walkSpeed * Time.deltaTime);
            }
        }

        private void CheckPlayerInteraction()
        {
            var input = KyokaInputBridge.Instance;
            if (input != null && input.InteractPressed)
            {
                var player = FindObjectOfType<KyokaPlayerController>();
                if (player != null && Vector3.Distance(transform.position, player.transform.position) <= interactionDistance)
                {
                    Interact(player);
                }
            }
        }

        public void Interact(KyokaPlayerController player)
        {
            Debug.Log($"[KYOKA NPC] {npcName}: \"{greetingDialogue}\"");

            if (npcType == NPCType.QuestGiver)
            {
                var mm = KyokaMissionManager.Instance;
                if (mm != null)
                {
                    mm.AddProgress("m_story_01", 1);
                }
            }
        }
    }
}

using UnityEngine;

namespace Kyoka.Character
{
    /// <summary>
    /// Free Fire-style smooth procedural humanoid animator for KYOKA.
    /// Delivers natural idle breathing, weight shifts, tactical sprint with torso lean,
    /// dynamic knee articulation, over-the-shoulder aiming pose, weapon recoil impulse,
    /// and skydiving glider posture without relying on external static animation FBX clips.
    /// </summary>
    [RequireComponent(typeof(KyokaCharacterModel))]
    public class KyokaProceduralAnimator : MonoBehaviour
    {
        private KyokaCharacterModel model;

        [Header("Locomotion State")]
        public float moveSpeed = 0f;
        public bool isAiming = false;
        public bool isGliding = false;
        public bool isGrounded = true;
        public float aimPitch = 0f;

        [Header("Animation Tuning")]
        public float idleFrequency = 1.8f;
        public float runFrequency = 9.2f;
        public float armSwingAngle = 36f;
        public float legStrideAngle = 42f;

        private float animTime = 0f;
        private float recoilTimer = 0f;
        private float currentSpeedSmooth = 0f;

        private void Awake()
        {
            model = GetComponent<KyokaCharacterModel>();
        }

        private void Update()
        {
            if (model == null || model.rootNode == null) return;

            animTime += Time.deltaTime;
            currentSpeedSmooth = Mathf.Lerp(currentSpeedSmooth, moveSpeed, Time.deltaTime * 10f);

            if (recoilTimer > 0f)
            {
                recoilTimer -= Time.deltaTime * 7.5f;
            }

            if (isGliding)
            {
                AnimateGliding();
            }
            else if (currentSpeedSmooth > 0.08f)
            {
                AnimateLocomotion();
            }
            else
            {
                AnimateIdle();
            }
        }

        public void TriggerRecoil()
        {
            recoilTimer = 1.0f;
        }

        private void AnimateIdle()
        {
            float breath = Mathf.Sin(animTime * idleFrequency);
            float weightShift = Mathf.Cos(animTime * idleFrequency * 0.5f) * 1.8f;

            // Hips subtle weight sway
            if (model.hipsNode)
            {
                model.hipsNode.localPosition = new Vector3(0, 0.95f + breath * 0.008f, 0);
                model.hipsNode.localRotation = Quaternion.Euler(0, weightShift * 0.5f, weightShift * 0.8f);
            }

            // Spine & Chest natural respiration
            if (model.spineNode)
            {
                model.spineNode.localRotation = Quaternion.Euler(breath * 2.2f, weightShift, 0);
            }
            if (model.chestNode)
            {
                model.chestNode.localRotation = Quaternion.Euler(breath * 1.5f, 0, 0);
            }

            // Head counter-rotation to look straight ahead
            if (model.headNode)
            {
                model.headNode.localRotation = Quaternion.Euler(-breath * 1.2f, -weightShift * 0.5f, 0);
            }

            if (isAiming)
            {
                ApplyAimPose();
            }
            else
            {
                // Free Fire tactical weapon carry stance
                float recoil = Mathf.Clamp01(recoilTimer) * -16f;

                if (model.leftArm)
                {
                    model.leftArm.localRotation = Quaternion.Euler(22f + breath * 1.5f, 15f, -14f);
                }
                if (model.rightArm)
                {
                    model.rightArm.localRotation = Quaternion.Euler(34f + breath * 2.0f + recoil, -8f, 12f);
                }
            }

            // Legs relaxed athletic stance
            if (model.leftLeg)
            {
                model.leftLeg.localRotation = Quaternion.Euler(1.5f, 0, -3f);
                if (model.leftFoot) model.leftFoot.localRotation = Quaternion.Euler(-1.5f, 0, 0);
            }
            if (model.rightLeg)
            {
                model.rightLeg.localRotation = Quaternion.Euler(-1.5f, 0, 3f);
                if (model.rightFoot) model.rightFoot.localRotation = Quaternion.Euler(1.5f, 0, 0);
            }
        }

        private void AnimateLocomotion()
        {
            float speedFactor = Mathf.Clamp01(currentSpeedSmooth / 6.0f);
            float freq = runFrequency * (0.75f + speedFactor * 0.35f);
            float cycle = Mathf.Sin(animTime * freq);
            float cosCycle = Mathf.Cos(animTime * freq);
            float stride = cycle * legStrideAngle * speedFactor;

            // Torso lean forward during tactical sprint
            float forwardLean = Mathf.Lerp(4f, 12f, speedFactor);

            if (model.hipsNode)
            {
                float bounce = Mathf.Abs(cycle) * 0.05f * speedFactor;
                model.hipsNode.localPosition = new Vector3(0, 0.95f - bounce, 0);
                model.hipsNode.localRotation = Quaternion.Euler(forwardLean * 0.5f, cosCycle * 3.5f * speedFactor, 0);
            }

            if (model.spineNode)
            {
                model.spineNode.localRotation = Quaternion.Euler(forwardLean, -cosCycle * 4f * speedFactor, 0);
            }
            if (model.chestNode)
            {
                model.chestNode.localRotation = Quaternion.Euler(2f, 0, 0);
            }

            // Legs stride with dynamic knee articulation
            if (model.leftLeg)
            {
                float kneeBend = (stride < 0f) ? Mathf.Abs(stride) * 0.85f : 0f;
                model.leftLeg.localRotation = Quaternion.Euler(stride, 0, -2.5f);
                if (model.leftFoot) model.leftFoot.localRotation = Quaternion.Euler(-kneeBend, 0, 0);
            }

            if (model.rightLeg)
            {
                float kneeBend = (-stride < 0f) ? Mathf.Abs(-stride) * 0.85f : 0f;
                model.rightLeg.localRotation = Quaternion.Euler(-stride, 0, 2.5f);
                if (model.rightFoot) model.rightFoot.localRotation = Quaternion.Euler(-kneeBend, 0, 0);
            }

            // Arms counter-swing or aim
            if (isAiming)
            {
                ApplyAimPose();
            }
            else
            {
                float armSwing = -stride * 0.85f;
                float recoil = Mathf.Clamp01(recoilTimer) * -16f;

                if (model.leftArm)
                {
                    model.leftArm.localRotation = Quaternion.Euler(armSwing + 15f, 10f, -12f);
                }
                if (model.rightArm)
                {
                    model.rightArm.localRotation = Quaternion.Euler(-armSwing + 25f + recoil, -10f, 10f);
                }
            }
        }

        private void ApplyAimPose()
        {
            float recoil = Mathf.Clamp01(recoilTimer) * -22f;

            // Upper body tracks camera aiming pitch
            if (model.chestNode)
            {
                model.chestNode.localRotation = Quaternion.Euler(aimPitch * 0.6f, 0, 0);
            }

            // Right arm raises weapon forward aligned with crosshair
            if (model.rightArm)
            {
                model.rightArm.localRotation = Quaternion.Euler(78f + aimPitch * 0.4f + recoil, -18f, 0);
            }

            // Left arm stabilizes rifle foregrip
            if (model.leftArm)
            {
                model.leftArm.localRotation = Quaternion.Euler(64f + aimPitch * 0.4f, 32f, -10f);
            }
        }

        private void AnimateGliding()
        {
            // Glider dive posture: arched spine, arms extended holding glider bars, legs trailing back
            float windSway = Mathf.Sin(animTime * 3.5f) * 2.5f;

            if (model.hipsNode)
            {
                model.hipsNode.localRotation = Quaternion.Euler(28f, windSway, 0);
            }
            if (model.spineNode)
            {
                model.spineNode.localRotation = Quaternion.Euler(14f, 0, 0);
            }
            if (model.headNode)
            {
                // Counteract spine angle so character looks forward towards drop zone
                model.headNode.localRotation = Quaternion.Euler(-32f, windSway * 0.4f, 0);
            }

            // Arms raised up to hold cyber glider grips
            if (model.leftArm)
            {
                model.leftArm.localRotation = Quaternion.Euler(88f, -18f, -28f);
            }
            if (model.rightArm)
            {
                model.rightArm.localRotation = Quaternion.Euler(88f, 18f, 28f);
            }

            // Legs trailing behind in aerodynamic freefall
            if (model.leftLeg)
            {
                model.leftLeg.localRotation = Quaternion.Euler(-32f, 0, -6f);
            }
            if (model.rightLeg)
            {
                model.rightLeg.localRotation = Quaternion.Euler(-32f, 0, 6f);
            }
        }
    }
}

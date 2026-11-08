using UnityEngine;

namespace Kyoka.Character
{
    /// <summary>
    /// Smooth procedural humanoid animator for KYOKA.
    /// Provides natural idle breathing, walk/run cycles, aiming pose, weapon recoil kick,
    /// and parachute gliding posture without requiring external animation FBX clips.
    /// </summary>
    [RequireComponent(typeof(KyokaCharacterModel))]
    public class KyokaProceduralAnimator : MonoBehaviour
    {
        private KyokaCharacterModel model;

        [Header("State")]
        public float moveSpeed = 0f;
        public bool isAiming = false;
        public bool isGliding = false;
        public bool isGrounded = true;
        public float aimPitch = 0f;

        [Header("Tuning")]
        public float idleFrequency = 1.6f;
        public float runFrequency = 9.0f;
        public float armSwingAngle = 32f;
        public float legStrideAngle = 38f;

        private float animTime = 0f;
        private float recoilTimer = 0f;

        private void Awake()
        {
            model = GetComponent<KyokaCharacterModel>();
        }

        private void Update()
        {
            if (model == null || model.rootNode == null) return;

            animTime += Time.deltaTime;
            if (recoilTimer > 0f) recoilTimer -= Time.deltaTime * 6f;

            if (isGliding)
            {
                AnimateGliding();
            }
            else if (moveSpeed > 0.1f)
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
            float subtleSway = Mathf.Cos(animTime * idleFrequency * 0.5f) * 1.5f;

            // Spine & Chest breath
            if (model.spineNode)
            {
                model.spineNode.localRotation = Quaternion.Euler(breath * 1.8f, subtleSway, 0);
            }
            if (model.chestNode)
            {
                model.chestNode.localRotation = Quaternion.Euler(breath * 1.2f, 0, 0);
            }

            // Arms in ready/combat idle or aiming
            if (isAiming)
            {
                ApplyAimPose();
            }
            else
            {
                // Relaxed tactical carry
                if (model.leftArm)
                    model.leftArm.localRotation = Quaternion.Euler(15f + breath * 1.5f, 0, -10f);
                if (model.rightArm)
                {
                    float recoil = Mathf.Clamp01(recoilTimer) * -18f;
                    model.rightArm.localRotation = Quaternion.Euler(30f + breath * 2.0f + recoil, 0, 8f);
                }
            }

            // Legs rest
            if (model.leftLeg) model.leftLeg.localRotation = Quaternion.Euler(0, 0, -2f);
            if (model.rightLeg) model.rightLeg.localRotation = Quaternion.Euler(0, 0, 2f);
        }

        private void AnimateLocomotion()
        {
            float cycle = Mathf.Sin(animTime * runFrequency);
            float cosCycle = Mathf.Cos(animTime * runFrequency);
            float stride = cycle * legStrideAngle;

            // Hips bounce
            if (model.hipsNode)
            {
                float bounce = Mathf.Abs(cycle) * 0.04f;
                model.hipsNode.localPosition = new Vector3(0, 0.95f - bounce, 0);
            }

            // Legs swing
            if (model.leftLeg)
            {
                float kneeBend = (stride < 0f) ? Mathf.Abs(stride) * 0.7f : 0f;
                model.leftLeg.localRotation = Quaternion.Euler(stride, 0, -2f);
                if (model.leftFoot) model.leftFoot.localRotation = Quaternion.Euler(-kneeBend, 0, 0);
            }
            if (model.rightLeg)
            {
                float kneeBend = (-stride < 0f) ? Mathf.Abs(-stride) * 0.7f : 0f;
                model.rightLeg.localRotation = Quaternion.Euler(-stride, 0, 2f);
                if (model.rightFoot) model.rightFoot.localRotation = Quaternion.Euler(-kneeBend, 0, 0);
            }

            // Arms swing oppositely
            if (isAiming)
            {
                ApplyAimPose();
            }
            else
            {
                if (model.leftArm)
                    model.leftArm.localRotation = Quaternion.Euler(-stride * 0.8f, 0, -10f);
                if (model.rightArm)
                {
                    float recoil = Mathf.Clamp01(recoilTimer) * -18f;
                    model.rightArm.localRotation = Quaternion.Euler(stride * 0.8f + recoil, 0, 10f);
                }
            }
        }

        private void ApplyAimPose()
        {
            float recoil = Mathf.Clamp01(recoilTimer) * -22f;

            // Upper body aims up/down with pitch
            if (model.chestNode)
            {
                model.chestNode.localRotation = Quaternion.Euler(aimPitch * 0.5f, 0, 0);
            }

            // Right arm raises weapon forward
            if (model.rightArm)
            {
                model.rightArm.localRotation = Quaternion.Euler(75f + aimPitch * 0.5f + recoil, -15f, 0);
            }
            // Left arm holds rifle foregrip
            if (model.leftArm)
            {
                model.leftArm.localRotation = Quaternion.Euler(60f + aimPitch * 0.5f, 35f, -12f);
            }
        }

        private void AnimateGliding()
        {
            // Glider skydiving posture: arms forward and wide, legs angled back
            if (model.spineNode)
            {
                model.spineNode.localRotation = Quaternion.Euler(35f, 0, 0);
            }
            if (model.leftArm)
            {
                model.leftArm.localRotation = Quaternion.Euler(85f, -25f, -40f);
            }
            if (model.rightArm)
            {
                model.rightArm.localRotation = Quaternion.Euler(85f, 25f, 40f);
            }
            if (model.leftLeg)
            {
                model.leftLeg.localRotation = Quaternion.Euler(-25f, 0, -8f);
            }
            if (model.rightLeg)
            {
                model.rightLeg.localRotation = Quaternion.Euler(-25f, 0, 8f);
            }
        }
    }
}


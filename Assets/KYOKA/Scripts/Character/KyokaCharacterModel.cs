using UnityEngine;
using Kyoka.Art;
using Kyoka.Save;

namespace Kyoka.Character
{
    public enum HairStyleType
    {
        SpikyAnime = 0,
        CyberFade = 1,
        SamuraiPonytail = 2,
        TacticalBuzzcut = 3
    }

    public enum OutfitPreset
    {
        UrbanStalker = 0,
        CyberRonin = 1,
        DesertCommando = 2,
        AetherVanguard = 3
    }

    /// <summary>
    /// Constructs a modular, stylized 3D anime/cyberpunk humanoid character.
    /// Provides customizable hair, skin, face, clothing presets, and modular weapon sockets.
    /// </summary>
    public class KyokaCharacterModel : MonoBehaviour
    {
        [Header("Modular Nodes")]
        public Transform rootNode;
        public Transform hipsNode;
        public Transform spineNode;
        public Transform chestNode;
        public Transform neckNode;
        public Transform headNode;
        public Transform leftShoulder;
        public Transform rightShoulder;
        public Transform leftArm;
        public Transform rightArm;
        public Transform leftHand;
        public Transform rightHand;
        public Transform leftLeg;
        public Transform rightLeg;
        public Transform leftFoot;
        public Transform rightFoot;
        public Transform rightHandWeaponSocket;
        public Transform backWeaponSocket;

        [Header("Renderers for Coloring")]
        public Renderer headRenderer;
        public Renderer hairRenderer;
        public Renderer chestRenderer;
        public Renderer pantsRenderer;
        public Renderer bootsRenderer;
        public Renderer leftArmRenderer;
        public Renderer rightArmRenderer;
        public Renderer leftHandRenderer;
        public Renderer rightHandRenderer;
        public Renderer leftLegRenderer;
        public Renderer rightLegRenderer;
        public Renderer leftFootRenderer;
        public Renderer rightFootRenderer;
        public GameObject currentHairObject;
        public GameObject currentEquippedWeaponObject;

        [Header("Active Customization State")]
        public bool isFemale = false;
        public HairStyleType currentHairStyle = HairStyleType.SpikyAnime;
        public Color currentHairColor = new Color(0.12f, 0.75f, 0.95f); // Cyan
        public Color currentSkinColor = new Color(0.96f, 0.82f, 0.73f); // Fair
        public Color currentEyeColor = new Color(0.1f, 0.85f, 0.7f);   // Jade/Cyan
        public OutfitPreset currentOutfit = OutfitPreset.UrbanStalker;

        private void Awake()
        {
            BuildHumanoidMesh();
        }

        public void BuildHumanoidMesh()
        {
            // Clear existing children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            rootNode = new GameObject("Humanoid_Root").transform;
            rootNode.SetParent(transform, false);

            float scaleW = isFemale ? 0.88f : 1.0f;
            float scaleH = isFemale ? 0.94f : 1.0f;

            // Hips / Pelvis
            hipsNode = CreatePivot("Hips", rootNode, new Vector3(0, 0.95f * scaleH, 0));
            var pelvis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pelvis.name = "Mesh_Pelvis";
            pelvis.transform.SetParent(hipsNode, false);
            pelvis.transform.localScale = new Vector3(0.32f * scaleW, 0.22f, 0.22f);
            Destroy(pelvis.GetComponent<Collider>());
            pantsRenderer = pelvis.GetComponent<Renderer>();

            // Spine & Chest
            spineNode = CreatePivot("Spine", hipsNode, new Vector3(0, 0.2f, 0));
            chestNode = CreatePivot("Chest", spineNode, new Vector3(0, 0.22f, 0));

            var chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = "Mesh_Chest";
            chest.transform.SetParent(chestNode, false);
            chest.transform.localScale = new Vector3(0.38f * scaleW, 0.32f, 0.24f);
            Destroy(chest.GetComponent<Collider>());
            chestRenderer = chest.GetComponent<Renderer>();

            // Tactical Neck & Head
            neckNode = CreatePivot("Neck", chestNode, new Vector3(0, 0.22f, 0));
            headNode = CreatePivot("Head", neckNode, new Vector3(0, 0.12f, 0));

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Mesh_Head";
            head.transform.SetParent(headNode, false);
            head.transform.localRotation = Quaternion.Euler(0, 90f, 0);
            head.transform.localScale = new Vector3(0.24f * scaleW, 0.26f, 0.24f);
            Destroy(head.GetComponent<Collider>());
            headRenderer = head.GetComponent<Renderer>();

            // Left Arm
            leftShoulder = CreatePivot("LeftShoulder", chestNode, new Vector3(-0.24f * scaleW, 0.12f, 0));
            leftArm = CreatePivot("LeftArm", leftShoulder, new Vector3(-0.04f, -0.16f, 0));
            var lArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lArmMesh.transform.SetParent(leftArm, false);
            lArmMesh.transform.localScale = new Vector3(0.11f, 0.22f, 0.11f);
            Destroy(lArmMesh.GetComponent<Collider>());
            leftArmRenderer = lArmMesh.GetComponent<Renderer>();

            leftHand = CreatePivot("LeftHand", leftArm, new Vector3(0, -0.24f, 0));
            var lHandMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lHandMesh.transform.SetParent(leftHand, false);
            lHandMesh.transform.localScale = new Vector3(0.08f, 0.10f, 0.08f);
            Destroy(lHandMesh.GetComponent<Collider>());
            leftHandRenderer = lHandMesh.GetComponent<Renderer>();

            // Right Arm (Weapon Arm)
            rightShoulder = CreatePivot("RightShoulder", chestNode, new Vector3(0.24f * scaleW, 0.12f, 0));
            rightArm = CreatePivot("RightArm", rightShoulder, new Vector3(0.04f, -0.16f, 0));
            var rArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rArmMesh.transform.SetParent(rightArm, false);
            rArmMesh.transform.localScale = new Vector3(0.11f, 0.22f, 0.11f);
            Destroy(rArmMesh.GetComponent<Collider>());
            rightArmRenderer = rArmMesh.GetComponent<Renderer>();

            rightHand = CreatePivot("RightHand", rightArm, new Vector3(0, -0.24f, 0));
            var rHandMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rHandMesh.transform.SetParent(rightHand, false);
            rHandMesh.transform.localScale = new Vector3(0.08f, 0.10f, 0.08f);
            Destroy(rHandMesh.GetComponent<Collider>());
            rightHandRenderer = rHandMesh.GetComponent<Renderer>();

            // Weapon Sockets
            rightHandWeaponSocket = CreatePivot("RightHand_WeaponSocket", rightHand, new Vector3(0, -0.05f, 0.12f));
            backWeaponSocket = CreatePivot("Back_WeaponSocket", chestNode, new Vector3(0.15f, 0, -0.18f));
            backWeaponSocket.localRotation = Quaternion.Euler(25, 45, 135);

            // Left Leg
            leftLeg = CreatePivot("LeftLeg", hipsNode, new Vector3(-0.11f * scaleW, -0.16f, 0));
            var lLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lLegMesh.transform.SetParent(leftLeg, false);
            lLegMesh.transform.localScale = new Vector3(0.13f, 0.36f, 0.13f);
            Destroy(lLegMesh.GetComponent<Collider>());
            leftLegRenderer = lLegMesh.GetComponent<Renderer>();

            leftFoot = CreatePivot("LeftFoot", leftLeg, new Vector3(0, -0.42f, 0.05f));
            var lFootMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lFootMesh.transform.SetParent(leftFoot, false);
            lFootMesh.transform.localScale = new Vector3(0.12f, 0.10f, 0.22f);
            Destroy(lFootMesh.GetComponent<Collider>());
            leftFootRenderer = lFootMesh.GetComponent<Renderer>();
            bootsRenderer = leftFootRenderer;

            // Right Leg
            rightLeg = CreatePivot("RightLeg", hipsNode, new Vector3(0.11f * scaleW, -0.16f, 0));
            var rLegMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rLegMesh.transform.SetParent(rightLeg, false);
            rLegMesh.transform.localScale = new Vector3(0.13f, 0.36f, 0.13f);
            Destroy(rLegMesh.GetComponent<Collider>());
            rightLegRenderer = rLegMesh.GetComponent<Renderer>();

            rightFoot = CreatePivot("RightFoot", rightLeg, new Vector3(0, -0.42f, 0.05f));
            var rFootMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rFootMesh.transform.SetParent(rightFoot, false);
            rFootMesh.transform.localScale = new Vector3(0.12f, 0.10f, 0.22f);
            Destroy(rFootMesh.GetComponent<Collider>());
            rightFootRenderer = rFootMesh.GetComponent<Renderer>();

            // Apply procedural textures and colors
            ApplyCustomization(currentHairStyle, currentHairColor, currentSkinColor, currentEyeColor, currentOutfit);
            EquipWeapon("AssaultRifle");
        }

        public void ApplyCustomization(HairStyleType hair, Color hairColor, Color skinColor, Color eyeColor, OutfitPreset outfit)
        {
            currentHairStyle = hair;
            currentHairColor = hairColor;
            currentSkinColor = skinColor;
            currentEyeColor = eyeColor;
            currentOutfit = outfit;

            // 1. Face Material with Anime Eyes & Expression
            if (headRenderer != null)
            {
                var faceTex = KyokaTextureGenerator.GetFaceTexture(skinColor, eyeColor);
                var faceMat = KyokaMaterialFactory.CreateLit(Color.white, faceTex);
                headRenderer.material = faceMat;
            }

            // Skin Material for hands
            var skinMat = KyokaMaterialFactory.CreateLit(skinColor);
            if (leftHandRenderer) leftHandRenderer.material = skinMat;
            if (rightHandRenderer) rightHandRenderer.material = skinMat;

            // 2. Build Hair
            RebuildHair(hair, hairColor);

            // 3. Outfit Material
            ApplyOutfitStyle(outfit);
        }

        private void RebuildHair(HairStyleType hair, Color color)
        {
            if (currentHairObject != null)
            {
                DestroyImmediate(currentHairObject);
            }

            currentHairObject = new GameObject("HairRoot");
            currentHairObject.transform.SetParent(headNode, false);

            var hairMat = KyokaMaterialFactory.CreateLit(color);

            // Scalp hair base
            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.transform.SetParent(currentHairObject.transform, false);
            cap.transform.localPosition = new Vector3(0, 0.07f, -0.02f);
            cap.transform.localScale = new Vector3(0.25f, 0.18f, 0.25f);
            cap.GetComponent<Renderer>().material = hairMat;
            Destroy(cap.GetComponent<Collider>());

            switch (hair)
            {
                case HairStyleType.SpikyAnime:
                    // Anime bangs and crest
                    for (int s = -2; s <= 2; s++)
                    {
                        var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        spike.transform.SetParent(currentHairObject.transform, false);
                        spike.transform.localPosition = new Vector3(s * 0.045f, 0.14f + (2 - Mathf.Abs(s)) * 0.02f, 0.05f - Mathf.Abs(s) * 0.03f);
                        spike.transform.localRotation = Quaternion.Euler(35f + Mathf.Abs(s) * 10f, s * 15f, -s * 18f);
                        spike.transform.localScale = new Vector3(0.04f, 0.10f, 0.04f);
                        spike.GetComponent<Renderer>().material = hairMat;
                        Destroy(spike.GetComponent<Collider>());
                    }
                    // Forehead framing bangs
                    var lBang = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    lBang.transform.SetParent(currentHairObject.transform, false);
                    lBang.transform.localPosition = new Vector3(-0.09f, 0.06f, 0.11f);
                    lBang.transform.localRotation = Quaternion.Euler(15, -15, -20);
                    lBang.transform.localScale = new Vector3(0.035f, 0.10f, 0.035f);
                    lBang.GetComponent<Renderer>().material = hairMat;
                    Destroy(lBang.GetComponent<Collider>());

                    var rBang = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rBang.transform.SetParent(currentHairObject.transform, false);
                    rBang.transform.localPosition = new Vector3(0.09f, 0.06f, 0.11f);
                    rBang.transform.localRotation = Quaternion.Euler(15, 15, 20);
                    rBang.transform.localScale = new Vector3(0.035f, 0.10f, 0.035f);
                    rBang.GetComponent<Renderer>().material = hairMat;
                    Destroy(rBang.GetComponent<Collider>());
                    break;

                case HairStyleType.CyberFade:
                    var fadeTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    fadeTop.transform.SetParent(currentHairObject.transform, false);
                    fadeTop.transform.localPosition = new Vector3(0, 0.14f, 0.02f);
                    fadeTop.transform.localRotation = Quaternion.Euler(10, 0, 0);
                    fadeTop.transform.localScale = new Vector3(0.18f, 0.08f, 0.20f);
                    fadeTop.GetComponent<Renderer>().material = hairMat;
                    Destroy(fadeTop.GetComponent<Collider>());
                    break;

                case HairStyleType.SamuraiPonytail:
                    var knot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    knot.transform.SetParent(currentHairObject.transform, false);
                    knot.transform.localPosition = new Vector3(0, 0.15f, -0.08f);
                    knot.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
                    knot.GetComponent<Renderer>().material = hairMat;
                    Destroy(knot.GetComponent<Collider>());

                    var tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tail.transform.SetParent(currentHairObject.transform, false);
                    tail.transform.localPosition = new Vector3(0, 0.12f, -0.16f);
                    tail.transform.localRotation = Quaternion.Euler(60, 0, 0);
                    tail.transform.localScale = new Vector3(0.045f, 0.18f, 0.045f);
                    tail.GetComponent<Renderer>().material = hairMat;
                    Destroy(tail.GetComponent<Collider>());
                    break;

                case HairStyleType.TacticalBuzzcut:
                default:
                    // Scalp cap provides clean tactical buzzcut look
                    break;
            }
        }

        private void ApplyOutfitStyle(OutfitPreset outfit)
        {
            var chestMat = KyokaMaterialFactory.CreateLit(Color.white);
            var pantsMat = KyokaMaterialFactory.CreateLit(Color.white);
            var bootsMat = KyokaMaterialFactory.CreateLit(Color.white);

            switch (outfit)
            {
                case OutfitPreset.UrbanStalker:
                    chestMat.color = new Color(0.15f, 0.18f, 0.24f); // Slate charcoal
                    pantsMat.color = new Color(0.10f, 0.12f, 0.16f); // Deep tactical black
                    bootsMat.color = new Color(0.20f, 0.22f, 0.25f);
                    break;

                case OutfitPreset.CyberRonin:
                    chestMat.color = new Color(0.08f, 0.55f, 0.75f); // Neon Cyan plating
                    pantsMat.color = new Color(0.12f, 0.14f, 0.20f);
                    bootsMat.color = new Color(0.15f, 0.70f, 0.90f);
                    break;

                case OutfitPreset.DesertCommando:
                    chestMat.color = new Color(0.65f, 0.55f, 0.38f); // Sand tactical vest
                    pantsMat.color = new Color(0.48f, 0.42f, 0.30f); // Desert camo
                    bootsMat.color = new Color(0.35f, 0.28f, 0.20f);
                    break;

                case OutfitPreset.AetherVanguard:
                    chestMat.color = new Color(0.92f, 0.92f, 0.95f); // White & Gold
                    pantsMat.color = new Color(0.85f, 0.70f, 0.25f); // Gold weave
                    bootsMat.color = new Color(0.95f, 0.80f, 0.30f);
                    break;
            }

            if (chestRenderer) chestRenderer.material = chestMat;
            if (leftArmRenderer) leftArmRenderer.material = chestMat;
            if (rightArmRenderer) rightArmRenderer.material = chestMat;

            if (pantsRenderer) pantsRenderer.material = pantsMat;
            if (leftLegRenderer) leftLegRenderer.material = pantsMat;
            if (rightLegRenderer) rightLegRenderer.material = pantsMat;

            if (bootsRenderer) bootsRenderer.material = bootsMat;
            if (leftFootRenderer) leftFootRenderer.material = bootsMat;
            if (rightFootRenderer) rightFootRenderer.material = bootsMat;
        }

        public void EquipWeapon(string weaponId)
        {
            if (currentEquippedWeaponObject != null)
            {
                DestroyImmediate(currentEquippedWeaponObject);
            }

            if (rightHandWeaponSocket == null) return;

            currentEquippedWeaponObject = new GameObject($"Weapon_{weaponId}");
            currentEquippedWeaponObject.transform.SetParent(rightHandWeaponSocket, false);

            var metalMat = KyokaMaterialFactory.CreateLit(new Color(0.15f, 0.16f, 0.18f));
            var accentMat = KyokaMaterialFactory.CreateLit(new Color(0.12f, 0.75f, 0.95f));

            switch (weaponId)
            {
                case "Katana":
                    var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    blade.transform.SetParent(currentEquippedWeaponObject.transform, false);
                    blade.transform.localPosition = new Vector3(0, 0, 0.45f);
                    blade.transform.localScale = new Vector3(0.02f, 0.06f, 0.85f);
                    blade.GetComponent<Renderer>().material = accentMat;
                    Destroy(blade.GetComponent<Collider>());

                    var hilt = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    hilt.transform.SetParent(currentEquippedWeaponObject.transform, false);
                    hilt.transform.localPosition = new Vector3(0, 0, -0.08f);
                    hilt.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    hilt.transform.localScale = new Vector3(0.035f, 0.12f, 0.035f);
                    hilt.GetComponent<Renderer>().material = metalMat;
                    Destroy(hilt.GetComponent<Collider>());
                    break;

                case "Shotgun":
                    var shotBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    shotBody.transform.SetParent(currentEquippedWeaponObject.transform, false);
                    shotBody.transform.localPosition = new Vector3(0, 0, 0.18f);
                    shotBody.transform.localScale = new Vector3(0.06f, 0.09f, 0.55f);
                    shotBody.GetComponent<Renderer>().material = metalMat;
                    Destroy(shotBody.GetComponent<Collider>());
                    break;

                case "AssaultRifle":
                default:
                    var receiver = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    receiver.transform.SetParent(currentEquippedWeaponObject.transform, false);
                    receiver.transform.localPosition = new Vector3(0, 0, 0.12f);
                    receiver.transform.localScale = new Vector3(0.05f, 0.08f, 0.35f);
                    receiver.GetComponent<Renderer>().material = metalMat;
                    Destroy(receiver.GetComponent<Collider>());

                    var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    barrel.transform.SetParent(currentEquippedWeaponObject.transform, false);
                    barrel.transform.localPosition = new Vector3(0, 0.02f, 0.36f);
                    barrel.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    barrel.transform.localScale = new Vector3(0.022f, 0.16f, 0.022f);
                    barrel.GetComponent<Renderer>().material = accentMat;
                    Destroy(barrel.GetComponent<Collider>());

                    var mag = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    mag.transform.SetParent(currentEquippedWeaponObject.transform, false);
                    mag.transform.localPosition = new Vector3(0, -0.10f, 0.10f);
                    mag.transform.localRotation = Quaternion.Euler(-18, 0, 0);
                    mag.transform.localScale = new Vector3(0.04f, 0.14f, 0.06f);
                    mag.GetComponent<Renderer>().material = metalMat;
                    Destroy(mag.GetComponent<Collider>());
                    break;
            }
        }

        private Transform CreatePivot(string name, Transform parent, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go.transform;
        }
    }
}


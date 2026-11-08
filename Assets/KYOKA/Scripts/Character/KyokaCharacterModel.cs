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
    /// Constructs a high-detail, stylized 3D anime humanoid character for KYOKA.
    /// Features sculpted facial contours, layered volumetric hair clumps,
    /// tailored tactical outfit weaves, seamless anatomical joints, combat boots,
    /// and modular weapon attachments.
    /// </summary>
    public class KyokaCharacterModel : MonoBehaviour
    {
        [Header("Modular Skeletal Nodes")]
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

        [Header("Renderers for Dynamic Styling")]
        public Renderer headRenderer;
        public Renderer hairRenderer;
        public Renderer chestRenderer;
        public Renderer waistRenderer;
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
        public Renderer collarRenderer;
        public Renderer neckRenderer;
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

            // 1. Pelvis / Hips
            hipsNode = CreatePivot("Hips", rootNode, new Vector3(0, 0.88f * scaleH, 0));
            var pelvisGO = CreateMeshChild("Mesh_Pelvis", hipsNode, KyokaMeshBuilder.CreateLimbSegmentMesh(0.17f * scaleW, 0.18f * scaleW, 0.16f * scaleW, 0.22f, 10));
            pelvisGO.transform.localPosition = new Vector3(0, 0, 0);
            pantsRenderer = pelvisGO.GetComponent<Renderer>();

            // 2. Spine & Abdominal Waist (Seamless Bridge - NO GAPS)
            spineNode = CreatePivot("Spine", hipsNode, new Vector3(0, 0.14f, 0));
            var waistGO = CreateMeshChild("Mesh_Waist", spineNode, KyokaMeshBuilder.CreateLimbSegmentMesh(0.16f * scaleW, 0.145f * scaleW, 0.168f * scaleW, 0.20f, 10));
            waistGO.transform.localPosition = new Vector3(0, 0.06f, 0);
            waistRenderer = waistGO.GetComponent<Renderer>();

            // Tactical Combat Belt Buckle
            var belt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            belt.name = "Mesh_TacticalBelt";
            belt.transform.SetParent(spineNode, false);
            belt.transform.localPosition = new Vector3(0, 0.01f, 0.09f);
            belt.transform.localScale = new Vector3(0.08f, 0.05f, 0.035f);
            Destroy(belt.GetComponent<Collider>());
            belt.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.12f, 0.85f, 0.95f), 1.6f);

            // 3. Sculpted Chest / Torso
            chestNode = CreatePivot("Chest", spineNode, new Vector3(0, 0.16f, 0));
            var chestGO = CreateMeshChild("Mesh_Chest", chestNode, KyokaMeshBuilder.CreateTorsoMesh(0.36f * scaleW, 0.36f, 0.22f, isFemale));
            chestGO.transform.localPosition = new Vector3(0, 0.10f, 0);
            chestRenderer = chestGO.GetComponent<Renderer>();

            // Tactical Collar Trim with Dark Armored Material (Fixed Magenta Issue)
            var collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            collar.name = "Mesh_Collar";
            collar.transform.SetParent(chestNode, false);
            collar.transform.localPosition = new Vector3(0, 0.27f, 0.01f);
            collar.transform.localScale = new Vector3(0.15f, 0.035f, 0.14f);
            Destroy(collar.GetComponent<Collider>());
            var collarMat = KyokaMaterialFactory.CreateLit(new Color(0.14f, 0.16f, 0.20f));
            collar.GetComponent<Renderer>().material = collarMat;
            collarRenderer = collar.GetComponent<Renderer>();

            // 4. Neck & Sculpted Anime Head
            neckNode = CreatePivot("Neck", chestNode, new Vector3(0, 0.28f, 0));
            var neckGO = CreateMeshChild("Mesh_Neck", neckNode, KyokaMeshBuilder.CreateLimbSegmentMesh(0.062f, 0.058f, 0.065f, 0.10f, 8));
            neckGO.transform.localPosition = new Vector3(0, 0.04f, 0);
            neckRenderer = neckGO.GetComponent<Renderer>();

            headNode = CreatePivot("Head", neckNode, new Vector3(0, 0.09f, 0));
            var headMesh = KyokaMeshBuilder.CreateAnimeHeadMesh(0.24f * scaleW, 0.28f, 0.24f, isFemale);
            var headGO = CreateMeshChild("Mesh_AnimeHead", headNode, headMesh);
            headGO.transform.localPosition = new Vector3(0, 0.04f, 0);
            headRenderer = headGO.GetComponent<Renderer>();

            // 5. Left Arm & Hand
            leftShoulder = CreatePivot("LeftShoulder", chestNode, new Vector3(-0.21f * scaleW, 0.22f, 0));
            leftArm = CreatePivot("LeftArm", leftShoulder, new Vector3(-0.04f, -0.12f, 0));
            var lArmGO = CreateMeshChild("Mesh_LeftArm", leftArm, KyokaMeshBuilder.CreateLimbSegmentMesh(0.065f, 0.060f, 0.052f, 0.26f, 10));
            lArmGO.transform.localPosition = new Vector3(0, -0.09f, 0);
            leftArmRenderer = lArmGO.GetComponent<Renderer>();

            leftHand = CreatePivot("LeftHand", leftArm, new Vector3(0, -0.24f, 0));
            var lHandGO = CreateMeshChild("Mesh_LeftHand", leftHand, KyokaMeshBuilder.CreateLimbSegmentMesh(0.045f, 0.048f, 0.038f, 0.11f, 8));
            lHandGO.transform.localPosition = new Vector3(0, -0.05f, 0);
            leftHandRenderer = lHandGO.GetComponent<Renderer>();

            // 6. Right Arm (Weapon Arm) & Hand
            rightShoulder = CreatePivot("RightShoulder", chestNode, new Vector3(0.21f * scaleW, 0.22f, 0));
            rightArm = CreatePivot("RightArm", rightShoulder, new Vector3(0.04f, -0.12f, 0));
            var rArmGO = CreateMeshChild("Mesh_RightArm", rightArm, KyokaMeshBuilder.CreateLimbSegmentMesh(0.065f, 0.060f, 0.052f, 0.26f, 10));
            rArmGO.transform.localPosition = new Vector3(0, -0.09f, 0);
            rightArmRenderer = rArmGO.GetComponent<Renderer>();

            rightHand = CreatePivot("RightHand", rightArm, new Vector3(0, -0.24f, 0));
            var rHandGO = CreateMeshChild("Mesh_RightHand", rightHand, KyokaMeshBuilder.CreateLimbSegmentMesh(0.045f, 0.048f, 0.038f, 0.11f, 8));
            rHandGO.transform.localPosition = new Vector3(0, -0.05f, 0);
            rightHandRenderer = rHandGO.GetComponent<Renderer>();

            // Weapon Sockets
            rightHandWeaponSocket = CreatePivot("RightHand_WeaponSocket", rightHand, new Vector3(0, -0.04f, 0.08f));
            backWeaponSocket = CreatePivot("Back_WeaponSocket", chestNode, new Vector3(0.14f, 0.12f, -0.16f));
            backWeaponSocket.localRotation = Quaternion.Euler(25, 45, 135);

            // 7. Left Leg & Tactical Boot
            leftLeg = CreatePivot("LeftLeg", hipsNode, new Vector3(-0.10f * scaleW, -0.11f, 0));
            var lLegGO = CreateMeshChild("Mesh_LeftLeg", leftLeg, KyokaMeshBuilder.CreateLimbSegmentMesh(0.078f, 0.072f, 0.058f, 0.44f, 10));
            lLegGO.transform.localPosition = new Vector3(0, -0.20f, 0);
            leftLegRenderer = lLegGO.GetComponent<Renderer>();

            leftFoot = CreatePivot("LeftFoot", leftLeg, new Vector3(0, -0.44f, 0.02f));
            var lBootGO = CreateMeshChild("Mesh_LeftBoot", leftFoot, KyokaMeshBuilder.CreateTacticalBootMesh(scaleW));
            lBootGO.transform.localPosition = new Vector3(0, 0, 0);
            leftFootRenderer = lBootGO.GetComponent<Renderer>();
            bootsRenderer = leftFootRenderer;

            // 8. Right Leg & Tactical Boot
            rightLeg = CreatePivot("RightLeg", hipsNode, new Vector3(0.10f * scaleW, -0.11f, 0));
            var rLegGO = CreateMeshChild("Mesh_RightLeg", rightLeg, KyokaMeshBuilder.CreateLimbSegmentMesh(0.078f, 0.072f, 0.058f, 0.44f, 10));
            rLegGO.transform.localPosition = new Vector3(0, -0.20f, 0);
            rightLegRenderer = rLegGO.GetComponent<Renderer>();

            rightFoot = CreatePivot("RightFoot", rightLeg, new Vector3(0, -0.44f, 0.02f));
            var rBootGO = CreateMeshChild("Mesh_RightBoot", rightFoot, KyokaMeshBuilder.CreateTacticalBootMesh(scaleW));
            rBootGO.transform.localPosition = new Vector3(0, 0, 0);
            rightFootRenderer = rBootGO.GetComponent<Renderer>();

            // Apply high-res procedural textures and outfits
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

            // 1. Anime Facial Features & Skin Shader
            if (headRenderer != null)
            {
                var faceTex = KyokaTextureGenerator.GetFaceTexture(skinColor, eyeColor, isFemale);
                var faceMat = KyokaMaterialFactory.CreateLit(Color.white, faceTex);
                if (faceMat.HasProperty("_Smoothness")) faceMat.SetFloat("_Smoothness", 0.40f);
                headRenderer.material = faceMat;
            }

            // Skin material for hands and neck
            var skinMat = KyokaMaterialFactory.CreateLit(skinColor);
            if (neckRenderer) neckRenderer.material = skinMat;
            if (leftHandRenderer) leftHandRenderer.material = skinMat;
            if (rightHandRenderer) rightHandRenderer.material = skinMat;

            // Collar dark tactical material
            if (collarRenderer) collarRenderer.material = KyokaMaterialFactory.CreateLit(new Color(0.14f, 0.16f, 0.20f));

            // 2. Layered Volumetric Hair
            RebuildHair(hair, hairColor);

            // 3. Tailored Outfit Texture & Materials
            ApplyOutfitStyle(outfit);
        }

        private void RebuildHair(HairStyleType hair, Color color)
        {
            if (currentHairObject != null)
            {
                DestroyImmediate(currentHairObject);
            }

            currentHairObject = new GameObject("Hair_Root");
            currentHairObject.transform.SetParent(headNode, false);

            var hairTex = KyokaTextureGenerator.GetHairTexture(color);
            var hairMat = KyokaMaterialFactory.CreateLit(color, hairTex);
            if (hairMat.HasProperty("_Smoothness")) hairMat.SetFloat("_Smoothness", 0.55f);

            // Scalp hair foundation cap
            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.name = "Hair_ScalpCap";
            cap.transform.SetParent(currentHairObject.transform, false);
            cap.transform.localPosition = new Vector3(0, 0.08f, -0.015f);
            cap.transform.localScale = new Vector3(0.245f, 0.17f, 0.245f);
            cap.GetComponent<Renderer>().material = hairMat;
            Destroy(cap.GetComponent<Collider>());
            hairRenderer = cap.GetComponent<Renderer>();

            // Layered stylized clumps based on style
            switch (hair)
            {
                case HairStyleType.SpikyAnime:
                    // 7 Swept Crown Spikes
                    for (int i = -3; i <= 3; i++)
                    {
                        Vector3 root = new Vector3(i * 0.035f, 0.12f, 0.02f);
                        Vector3 mid = new Vector3(i * 0.055f, 0.21f, 0.05f - Mathf.Abs(i) * 0.02f);
                        Vector3 tip = new Vector3(i * 0.075f, 0.27f, 0.08f - Mathf.Abs(i) * 0.03f);

                        var spikeGO = CreateMeshChild($"Spike_{i}", currentHairObject.transform,
                            KyokaMeshBuilder.CreateHairClumpMesh(root, mid, tip, 0.035f, 0.045f, 0.01f, 0.03f));
                        spikeGO.GetComponent<Renderer>().material = hairMat;
                    }

                    // Face-framing side bangs
                    var lBang = CreateMeshChild("LeftBang", currentHairObject.transform,
                        KyokaMeshBuilder.CreateHairClumpMesh(new Vector3(-0.08f, 0.10f, 0.09f), new Vector3(-0.11f, 0.01f, 0.11f), new Vector3(-0.09f, -0.07f, 0.10f), 0.03f, 0.035f, 0.008f));
                    lBang.GetComponent<Renderer>().material = hairMat;

                    var rBang = CreateMeshChild("RightBang", currentHairObject.transform,
                        KyokaMeshBuilder.CreateHairClumpMesh(new Vector3(0.08f, 0.10f, 0.09f), new Vector3(0.11f, 0.01f, 0.11f), new Vector3(0.09f, -0.07f, 0.10f), 0.03f, 0.035f, 0.008f));
                    rBang.GetComponent<Renderer>().material = hairMat;

                    // Forehead fringe
                    var fringe = CreateMeshChild("CenterFringe", currentHairObject.transform,
                        KyokaMeshBuilder.CreateHairClumpMesh(new Vector3(0, 0.12f, 0.10f), new Vector3(0.02f, 0.06f, 0.12f), new Vector3(0.03f, 0.01f, 0.12f), 0.04f, 0.035f, 0.008f));
                    fringe.GetComponent<Renderer>().material = hairMat;
                    break;

                case HairStyleType.CyberFade:
                    var pompadour = CreateMeshChild("Pompadour", currentHairObject.transform,
                        KyokaMeshBuilder.CreateHairClumpMesh(new Vector3(0, 0.10f, -0.08f), new Vector3(0, 0.19f, 0.04f), new Vector3(0, 0.22f, 0.11f), 0.09f, 0.10f, 0.04f, 0.06f));
                    pompadour.GetComponent<Renderer>().material = hairMat;

                    for (int s = -1; s <= 1; s += 2)
                    {
                        var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        panel.transform.SetParent(currentHairObject.transform, false);
                        panel.transform.localPosition = new Vector3(s * 0.10f, 0.08f, 0.01f);
                        panel.transform.localScale = new Vector3(0.025f, 0.09f, 0.18f);
                        panel.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateLit(color * 0.65f);
                        Destroy(panel.GetComponent<Collider>());
                    }
                    break;

                case HairStyleType.SamuraiPonytail:
                    var mane = CreateMeshChild("Mane", currentHairObject.transform,
                        KyokaMeshBuilder.CreateHairClumpMesh(new Vector3(0, 0.12f, 0.06f), new Vector3(0, 0.15f, -0.06f), new Vector3(0, 0.13f, -0.12f), 0.07f, 0.08f, 0.04f));
                    mane.GetComponent<Renderer>().material = hairMat;

                    var knot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    knot.name = "HairKnot";
                    knot.transform.SetParent(currentHairObject.transform, false);
                    knot.transform.localPosition = new Vector3(0, 0.13f, -0.13f);
                    knot.transform.localRotation = Quaternion.Euler(65, 0, 0);
                    knot.transform.localScale = new Vector3(0.05f, 0.025f, 0.05f);
                    knot.GetComponent<Renderer>().material = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.95f, 0.8f, 0.2f), 1.5f);
                    Destroy(knot.GetComponent<Collider>());

                    var tail = CreateMeshChild("Ponytail", currentHairObject.transform,
                        KyokaMeshBuilder.CreateHairClumpMesh(new Vector3(0, 0.13f, -0.14f), new Vector3(0, 0.02f, -0.22f), new Vector3(0, -0.14f, -0.26f), 0.045f, 0.05f, 0.01f));
                    tail.GetComponent<Renderer>().material = hairMat;
                    break;

                case HairStyleType.TacticalBuzzcut:
                default:
                    break;
            }
        }

        private void ApplyOutfitStyle(OutfitPreset outfit)
        {
            var outfitTex = KyokaTextureGenerator.GetOutfitTexture((int)outfit, isFemale);
            var outfitMat = KyokaMaterialFactory.CreateLit(Color.white, outfitTex);
            if (outfitMat.HasProperty("_Smoothness")) outfitMat.SetFloat("_Smoothness", 0.42f);

            var bootMat = KyokaMaterialFactory.CreateLit(new Color(0.14f, 0.15f, 0.18f));
            if (bootMat.HasProperty("_Smoothness")) bootMat.SetFloat("_Smoothness", 0.35f);

            // Apply to Torso, Waist & Pants
            if (chestRenderer) chestRenderer.material = outfitMat;
            if (waistRenderer) waistRenderer.material = outfitMat;
            if (pantsRenderer) pantsRenderer.material = outfitMat;

            // Apply to Arms & Legs
            if (leftArmRenderer) leftArmRenderer.material = outfitMat;
            if (rightArmRenderer) rightArmRenderer.material = outfitMat;
            if (leftLegRenderer) leftLegRenderer.material = outfitMat;
            if (rightLegRenderer) rightLegRenderer.material = outfitMat;

            // Apply to Boots
            if (leftFootRenderer) leftFootRenderer.material = bootMat;
            if (rightFootRenderer) rightFootRenderer.material = bootMat;
            bootsRenderer = leftFootRenderer;
        }

        public void EquipWeapon(string weaponId)
        {
            if (currentEquippedWeaponObject != null)
            {
                DestroyImmediate(currentEquippedWeaponObject);
            }

            if (rightHandWeaponSocket == null) return;

            switch (weaponId)
            {
                case "Katana":
                    currentEquippedWeaponObject = KyokaMeshBuilder.CreateDetailedKatana();
                    currentEquippedWeaponObject.transform.SetParent(rightHandWeaponSocket, false);
                    currentEquippedWeaponObject.transform.localPosition = new Vector3(0, 0, 0.08f);
                    currentEquippedWeaponObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    break;

                case "Shotgun":
                case "AssaultRifle":
                default:
                    currentEquippedWeaponObject = KyokaMeshBuilder.CreateDetailedAssaultRifle();
                    currentEquippedWeaponObject.transform.SetParent(rightHandWeaponSocket, false);
                    currentEquippedWeaponObject.transform.localPosition = new Vector3(0, -0.04f, 0.08f);
                    currentEquippedWeaponObject.transform.localRotation = Quaternion.identity;
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

        private GameObject CreateMeshChild(string name, Transform parent, Mesh mesh)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.material = KyokaMaterialFactory.CreateLit(Color.white);
            return go;
        }
    }
}

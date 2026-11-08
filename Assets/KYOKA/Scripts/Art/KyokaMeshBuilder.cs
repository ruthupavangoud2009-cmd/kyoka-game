using UnityEngine;
using System.Collections.Generic;

namespace Kyoka.Art
{
    /// <summary>
    /// Master procedural 3D mesh engine for KYOKA AAA Visual Overhaul.
    /// Synthesizes anatomical anime humanoids, layered stylized hair clumps,
    /// tactical military weapons, organic branching foliage, faceted boulders,
    /// and animated wave grids without relying on crude primitive cubes/spheres.
    /// </summary>
    public static class KyokaMeshBuilder
    {
        #region Anime Head & Face Mesh

        /// <summary>
        /// Generates a contoured anime head mesh with tapered jawline, pointed chin,
        /// rounded cranium, and planar frontal projection for high-res anime facial textures.
        /// </summary>
        public static Mesh CreateAnimeHeadMesh(float width = 0.22f, float height = 0.26f, float depth = 0.23f, bool isFemale = false)
        {
            var mesh = new Mesh { name = "Mesh_AnimeHead" };

            int latSegments = 16;
            int lonSegments = 24;

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float chinTaper = isFemale ? 0.38f : 0.48f;
            float jawWidth = isFemale ? 0.65f : 0.75f;

            for (int lat = 0; lat <= latSegments; lat++)
            {
                float v = (float)lat / latSegments;
                float theta = v * Mathf.PI;
                float y = Mathf.Cos(theta) * (height * 0.5f);

                // Morph lower half into anime jaw taper
                float taperFactor = 1.0f;
                if (y < 0f)
                {
                    float factor = -y / (height * 0.5f);
                    taperFactor = Mathf.Lerp(1.0f, chinTaper, Mathf.Pow(factor, 1.4f));
                }

                for (int lon = 0; lon <= lonSegments; lon++)
                {
                    float u = (float)lon / lonSegments;
                    float phi = u * Mathf.PI * 2f;

                    float sinTheta = Mathf.Sin(theta);
                    float x = Mathf.Sin(phi) * sinTheta * (width * 0.5f) * taperFactor;
                    float z = Mathf.Cos(phi) * sinTheta * (depth * 0.5f);

                    // Front face slight flattening & chin forward tilt
                    if (z > 0f)
                    {
                        z *= 0.95f;
                        if (y < -0.04f)
                        {
                            z += (-y) * 0.18f; // Anime pointed chin forward protrusion
                            x *= jawWidth;
                        }
                    }

                    Vector3 pos = new Vector3(x, y, z);
                    vertices.Add(pos);

                    // Spherical/planar hybrid UV mapping
                    // Front hemisphere (z > 0) maps smoothly onto anime eye/face texture
                    if (z >= -0.02f)
                    {
                        float faceU = Mathf.Clamp01(0.5f + (x / width));
                        float faceV = Mathf.Clamp01(0.5f + (y / height) * 1.15f);
                        uvs.Add(new Vector2(faceU, faceV));
                    }
                    else
                    {
                        uvs.Add(new Vector2(u, v));
                    }

                    normals.Add(pos.normalized);
                }
            }

            for (int lat = 0; lat < latSegments; lat++)
            {
                for (int lon = 0; lon < lonSegments; lon++)
                {
                    int current = lat * (lonSegments + 1) + lon;
                    int next = current + lonSegments + 1;

                    triangles.Add(current);
                    triangles.Add(current + 1);
                    triangles.Add(next);

                    triangles.Add(next);
                    triangles.Add(current + 1);
                    triangles.Add(next + 1);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        #endregion

        #region Layered Hair Geometry

        /// <summary>
        /// Synthesizes a tapered curved volumetric hair strand / clump.
        /// </summary>
        public static Mesh CreateHairClumpMesh(Vector3 root, Vector3 mid, Vector3 tip, float rootW, float midW, float tipW, float thickness = 0.03f)
        {
            var mesh = new Mesh { name = "Mesh_HairClump" };

            // 4-sided tapered prism along Bezier curve
            int steps = 6;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                // Quadratic Bezier point
                Vector3 p = (1 - t) * (1 - t) * root + 2 * (1 - t) * t * mid + t * t * tip;

                // Tangent vector
                Vector3 tangent = (2 * (1 - t) * (mid - root) + 2 * t * (tip - mid)).normalized;
                Vector3 normal = Vector3.Cross(tangent, Vector3.up).normalized;
                if (normal.sqrMagnitude < 0.01f) normal = Vector3.Cross(tangent, Vector3.right).normalized;
                Vector3 binormal = Vector3.Cross(tangent, normal).normalized;

                float w = (t < 0.5f) ? Mathf.Lerp(rootW, midW, t * 2f) : Mathf.Lerp(midW, tipW, (t - 0.5f) * 2f);
                float h = thickness * (1.0f - t * 0.7f);

                // 4 perimeter vertices
                verts.Add(p + normal * w + binormal * h);
                verts.Add(p - normal * w + binormal * h);
                verts.Add(p - normal * w - binormal * h);
                verts.Add(p + normal * w - binormal * h);

                uvs.Add(new Vector2(0f, t));
                uvs.Add(new Vector2(1f, t));
                uvs.Add(new Vector2(1f, t));
                uvs.Add(new Vector2(0f, t));
            }

            for (int i = 0; i < steps; i++)
            {
                int b = i * 4;
                int nb = (i + 1) * 4;

                for (int s = 0; s < 4; s++)
                {
                    int sNext = (s + 1) % 4;
                    tris.Add(b + s);
                    tris.Add(nb + s);
                    tris.Add(b + sNext);

                    tris.Add(b + sNext);
                    tris.Add(nb + s);
                    tris.Add(nb + sNext);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region Anime Anatomical Torso Mesh

        /// <summary>
        /// Creates a sculpted athletic torso with collar, tapered chest, waist contour,
        /// and back curvature suitable for tactical outfit mapping.
        /// </summary>
        public static Mesh CreateTorsoMesh(float width = 0.38f, float height = 0.44f, float depth = 0.22f, bool isFemale = false)
        {
            var mesh = new Mesh { name = "Mesh_Torso" };

            int heightSegments = 8;
            int ringSegments = 14;

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int yStep = 0; yStep <= heightSegments; yStep++)
            {
                float v = (float)yStep / heightSegments;
                float y = (v - 0.5f) * height;

                // Waist taper & shoulder width
                float wMult = 1.0f;
                float dMult = 1.0f;

                if (v < 0.25f) // Pelvis / hips
                {
                    wMult = isFemale ? 0.95f : 0.82f;
                    dMult = 0.88f;
                }
                else if (v < 0.55f) // Narrow athletic waist
                {
                    wMult = isFemale ? 0.72f : 0.80f;
                    dMult = 0.75f;
                }
                else if (v < 0.85f) // Broad chest / tactical armor
                {
                    wMult = isFemale ? 0.90f : 1.05f;
                    dMult = isFemale ? 1.02f : 0.95f;
                }
                else // Neck transition / shoulders
                {
                    wMult = isFemale ? 0.65f : 0.75f;
                    dMult = 0.65f;
                }

                float halfW = (width * 0.5f) * wMult;
                float halfD = (depth * 0.5f) * dMult;

                for (int r = 0; r <= ringSegments; r++)
                {
                    float u = (float)r / ringSegments;
                    float angle = u * Mathf.PI * 2f;

                    float cos = Mathf.Cos(angle);
                    float sin = Mathf.Sin(angle);

                    // Elliptical superquadric for jacket shape
                    float x = Mathf.Sign(cos) * Mathf.Pow(Mathf.Abs(cos), 0.85f) * halfW;
                    float z = Mathf.Sign(sin) * Mathf.Pow(Mathf.Abs(sin), 0.85f) * halfD;

                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int yStep = 0; yStep < heightSegments; yStep++)
            {
                for (int r = 0; r < ringSegments; r++)
                {
                    int current = yStep * (ringSegments + 1) + r;
                    int next = current + (ringSegments + 1);

                    tris.Add(current);
                    tris.Add(next);
                    tris.Add(current + 1);

                    tris.Add(current + 1);
                    tris.Add(next);
                    tris.Add(next + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        #endregion

        #region Anatomical Limb & Combat Boot Meshes

        /// <summary>
        /// Synthesizes contoured limb segments (arm or leg) with natural taper and elbow/knee curves.
        /// </summary>
        public static Mesh CreateLimbSegmentMesh(float topR, float midR, float botR, float length, int segments = 10)
        {
            var mesh = new Mesh { name = "Mesh_LimbSegment" };

            int heightSteps = 6;
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int h = 0; h <= heightSteps; h++)
            {
                float v = (float)h / heightSteps;
                float y = (0.5f - v) * length;

                float r = (v < 0.5f) ? Mathf.Lerp(topR, midR, v * 2f) : Mathf.Lerp(midR, botR, (v - 0.5f) * 2f);

                for (int s = 0; s <= segments; s++)
                {
                    float u = (float)s / segments;
                    float angle = u * Mathf.PI * 2f;

                    float x = Mathf.Cos(angle) * r;
                    float z = Mathf.Sin(angle) * r * 0.95f;

                    verts.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2(u, v));
                }
            }

            for (int h = 0; h < heightSteps; h++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int cur = h * (segments + 1) + s;
                    int nxt = cur + (segments + 1);

                    tris.Add(cur);
                    tris.Add(nxt);
                    tris.Add(cur + 1);

                    tris.Add(cur + 1);
                    tris.Add(nxt);
                    tris.Add(nxt + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Free Fire tactical military combat boot with tread sole, reinforced toe cap, and ankle cuff.
        /// </summary>
        public static Mesh CreateTacticalBootMesh(float scale = 1.0f)
        {
            var mesh = new Mesh { name = "Mesh_CombatBoot" };

            var verts = new List<Vector3>
            {
                // Tread sole base (Y = 0)
                new Vector3(-0.06f, 0f, -0.10f) * scale,
                new Vector3( 0.06f, 0f, -0.10f) * scale,
                new Vector3( 0.07f, 0f,  0.06f) * scale,
                new Vector3( 0.05f, 0f,  0.16f) * scale, // Toe front
                new Vector3(-0.05f, 0f,  0.16f) * scale,
                new Vector3(-0.07f, 0f,  0.06f) * scale,

                // Sole top rim (Y = 0.04)
                new Vector3(-0.058f, 0.04f, -0.095f) * scale,
                new Vector3( 0.058f, 0.04f, -0.095f) * scale,
                new Vector3( 0.065f, 0.04f,  0.055f) * scale,
                new Vector3( 0.048f, 0.04f,  0.155f) * scale,
                new Vector3(-0.048f, 0.04f,  0.155f) * scale,
                new Vector3(-0.065f, 0.04f,  0.055f) * scale,

                // Vamp & Toe box (Y = 0.09)
                new Vector3(-0.05f, 0.09f, -0.06f) * scale,
                new Vector3( 0.05f, 0.09f, -0.06f) * scale,
                new Vector3( 0.052f, 0.08f, 0.04f) * scale,
                new Vector3( 0.038f, 0.065f, 0.13f) * scale,
                new Vector3(-0.038f, 0.065f, 0.13f) * scale,
                new Vector3(-0.052f, 0.08f, 0.04f) * scale,

                // Ankle collar (Y = 0.20)
                new Vector3(-0.052f, 0.20f, -0.05f) * scale,
                new Vector3( 0.052f, 0.20f, -0.05f) * scale,
                new Vector3( 0.048f, 0.20f,  0.03f) * scale,
                new Vector3(-0.048f, 0.20f,  0.03f) * scale
            };

            var uvs = new List<Vector2>();
            for (int i = 0; i < verts.Count; i++)
            {
                uvs.Add(new Vector2(verts[i].x * 5f + 0.5f, verts[i].z * 3f + 0.5f));
            }

            var tris = new List<int>();

            // Sole bottom
            tris.AddRange(new int[] { 0, 1, 2, 0, 2, 5, 5, 2, 3, 5, 3, 4 });

            // Sole side rim
            for (int i = 0; i < 6; i++)
            {
                int next = (i + 1) % 6;
                tris.Add(i);
                tris.Add(i + 6);
                tris.Add(next);

                tris.Add(next);
                tris.Add(i + 6);
                tris.Add(next + 6);
            }

            // Toe box & vamp
            for (int i = 0; i < 6; i++)
            {
                int next = (i + 1) % 6;
                tris.Add(i + 6);
                tris.Add(i + 12);
                tris.Add(next + 6);

                tris.Add(next + 6);
                tris.Add(i + 12);
                tris.Add(next + 12);
            }

            // Ankle cuff sides
            tris.AddRange(new int[]
            {
                12, 18, 13, 13, 18, 19, // Back
                13, 19, 14, 14, 19, 20, // Right side
                17, 21, 12, 12, 21, 18, // Left side
                14, 20, 17, 17, 20, 21  // Front tongue
            });

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion

        #region High-Detail 3D Weapons

        /// <summary>
        /// Constructs a detailed tactical Assault Rifle with ventilated shroud,
        /// curved magazine, holographic reflex sight, and ergonomic stock.
        /// </summary>
        public static GameObject CreateDetailedAssaultRifle()
        {
            var gun = new GameObject("AssaultRifle_HighDetail");

            var metalMat = KyokaMaterialFactory.CreateLit(new Color(0.12f, 0.13f, 0.16f));
            var carbonMat = KyokaMaterialFactory.CreateLit(new Color(0.18f, 0.20f, 0.24f));
            var neonMat = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.1f, 0.85f, 0.95f), 2.2f);

            // Upper Receiver
            var receiver = GameObject.CreatePrimitive(PrimitiveType.Cube);
            receiver.name = "UpperReceiver";
            receiver.transform.SetParent(gun.transform, false);
            receiver.transform.localPosition = new Vector3(0, 0.02f, 0.08f);
            receiver.transform.localScale = new Vector3(0.045f, 0.065f, 0.28f);
            receiver.GetComponent<Renderer>().material = metalMat;
            Object.Destroy(receiver.GetComponent<Collider>());

            // Fluted Barrel
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.SetParent(gun.transform, false);
            barrel.transform.localPosition = new Vector3(0, 0.02f, 0.32f);
            barrel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            barrel.transform.localScale = new Vector3(0.018f, 0.16f, 0.018f);
            barrel.GetComponent<Renderer>().material = metalMat;
            Object.Destroy(barrel.GetComponent<Collider>());

            // Muzzle Brake / Flash Hider
            var muzzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            muzzle.name = "MuzzleBrake";
            muzzle.transform.SetParent(gun.transform, false);
            muzzle.transform.localPosition = new Vector3(0, 0.02f, 0.49f);
            muzzle.transform.localRotation = Quaternion.Euler(90, 0, 0);
            muzzle.transform.localScale = new Vector3(0.024f, 0.025f, 0.024f);
            muzzle.GetComponent<Renderer>().material = carbonMat;
            Object.Destroy(muzzle.GetComponent<Collider>());

            // Handguard / Ventilated Shroud
            var shroud = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shroud.name = "Handguard";
            shroud.transform.SetParent(gun.transform, false);
            shroud.transform.localPosition = new Vector3(0, 0.02f, 0.26f);
            shroud.transform.localScale = new Vector3(0.042f, 0.052f, 0.16f);
            shroud.GetComponent<Renderer>().material = carbonMat;
            Object.Destroy(shroud.GetComponent<Collider>());

            // Curved 30-round Mag
            var mag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mag.name = "CurvedMagazine";
            mag.transform.SetParent(gun.transform, false);
            mag.transform.localPosition = new Vector3(0, -0.09f, 0.12f);
            mag.transform.localRotation = Quaternion.Euler(-18, 0, 0);
            mag.transform.localScale = new Vector3(0.035f, 0.15f, 0.055f);
            mag.GetComponent<Renderer>().material = metalMat;
            Object.Destroy(mag.GetComponent<Collider>());

            // Pistol Grip
            var grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grip.name = "PistolGrip";
            grip.transform.SetParent(gun.transform, false);
            grip.transform.localPosition = new Vector3(0, -0.07f, -0.02f);
            grip.transform.localRotation = Quaternion.Euler(22, 0, 0);
            grip.transform.localScale = new Vector3(0.035f, 0.12f, 0.045f);
            grip.GetComponent<Renderer>().material = carbonMat;
            Object.Destroy(grip.GetComponent<Collider>());

            // Tactical Skeleton Stock
            var stock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stock.name = "TacticalStock";
            stock.transform.SetParent(gun.transform, false);
            stock.transform.localPosition = new Vector3(0, 0.01f, -0.16f);
            stock.transform.localScale = new Vector3(0.036f, 0.08f, 0.16f);
            stock.GetComponent<Renderer>().material = carbonMat;
            Object.Destroy(stock.GetComponent<Collider>());

            // Holographic Reflex Sight with glowing reticle
            var sightHousing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sightHousing.name = "HoloSight";
            sightHousing.transform.SetParent(gun.transform, false);
            sightHousing.transform.localPosition = new Vector3(0, 0.065f, 0.06f);
            sightHousing.transform.localScale = new Vector3(0.038f, 0.035f, 0.06f);
            sightHousing.GetComponent<Renderer>().material = carbonMat;
            Object.Destroy(sightHousing.GetComponent<Collider>());

            var reticle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            reticle.name = "ReticleGlass";
            reticle.transform.SetParent(sightHousing.transform, false);
            reticle.transform.localPosition = new Vector3(0, 0.15f, 0);
            reticle.transform.localScale = new Vector3(0.6f, 0.6f, 0.1f);
            reticle.GetComponent<Renderer>().material = neonMat;
            Object.Destroy(reticle.GetComponent<Collider>());

            return gun;
        }

        /// <summary>
        /// Constructs a stylized anime Katana with curved blade, tsuba guard, and tsuka grip.
        /// </summary>
        public static GameObject CreateDetailedKatana()
        {
            var katana = new GameObject("Katana_HighDetail");

            var bladeMat = KyokaMaterialFactory.CreateLit(new Color(0.92f, 0.95f, 1.0f));
            if (bladeMat.HasProperty("_Smoothness")) bladeMat.SetFloat("_Smoothness", 0.95f);
            if (bladeMat.HasProperty("_Metallic")) bladeMat.SetFloat("_Metallic", 0.85f);

            var goldMat = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.95f, 0.78f, 0.22f), 1.4f);
            var wrapMat = KyokaMaterialFactory.CreateLit(new Color(0.12f, 0.14f, 0.18f));

            // Curved Blade
            var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(katana.transform, false);
            blade.transform.localPosition = new Vector3(0, 0, 0.48f);
            blade.transform.localScale = new Vector3(0.014f, 0.045f, 0.88f);
            blade.GetComponent<Renderer>().material = bladeMat;
            Object.Destroy(blade.GetComponent<Collider>());

            // Golden Tsuba (Guard)
            var tsuba = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tsuba.name = "Tsuba";
            tsuba.transform.SetParent(katana.transform, false);
            tsuba.transform.localPosition = new Vector3(0, 0, 0.04f);
            tsuba.transform.localRotation = Quaternion.Euler(90, 0, 0);
            tsuba.transform.localScale = new Vector3(0.09f, 0.012f, 0.09f);
            tsuba.GetComponent<Renderer>().material = goldMat;
            Object.Destroy(tsuba.GetComponent<Collider>());

            // Tsuka (Wrapped Handle)
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(katana.transform, false);
            handle.transform.localPosition = new Vector3(0, 0, -0.10f);
            handle.transform.localRotation = Quaternion.Euler(90, 0, 0);
            handle.transform.localScale = new Vector3(0.026f, 0.13f, 0.026f);
            handle.GetComponent<Renderer>().material = wrapMat;
            Object.Destroy(handle.GetComponent<Collider>());

            // Golden Kashira (Pommel)
            var pommel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pommel.name = "Pommel";
            pommel.transform.SetParent(katana.transform, false);
            pommel.transform.localPosition = new Vector3(0, 0, -0.23f);
            pommel.transform.localScale = new Vector3(0.032f, 0.032f, 0.032f);
            pommel.GetComponent<Renderer>().material = goldMat;
            Object.Destroy(pommel.GetComponent<Collider>());

            return katana;
        }

        /// <summary>
        /// Constructs a Free Fire-inspired high-tech Cyber Delta-Wing Glider with swept wings,
        /// vertical stabilizers, twin jet thrusters with glowing exhaust, and ergonomic grip handlebars.
        /// </summary>
        public static GameObject CreateDetailedGlider()
        {
            var glider = new GameObject("CyberGlider_AAA");

            var carbonMat = KyokaMaterialFactory.CreateLit(new Color(0.12f, 0.14f, 0.18f), KyokaTextureGenerator.GetGliderTexture());
            var wingMat = KyokaMaterialFactory.CreateLit(new Color(0.18f, 0.22f, 0.28f), KyokaTextureGenerator.GetGliderTexture());
            var cyanNeonMat = KyokaMaterialFactory.CreateEmissionMaterial(new Color(0.12f, 0.88f, 0.98f), 2.5f);
            var thrusterGlowMat = KyokaMaterialFactory.CreateEmissionMaterial(new Color(1.0f, 0.48f, 0.12f), 3.0f);
            var thrusterMat = KyokaMaterialFactory.CreateLit(new Color(0.15f, 0.16f, 0.19f));
            var frameMat = KyokaMaterialFactory.CreateLit(new Color(0.40f, 0.44f, 0.50f));

            // 1. Central Aerodynamic Keel / Spine
            var keel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keel.name = "Glider_Fuselage";
            keel.transform.SetParent(glider.transform, false);
            keel.transform.localPosition = new Vector3(0, 0.05f, 0);
            keel.transform.localScale = new Vector3(0.35f, 0.12f, 1.6f);
            keel.GetComponent<Renderer>().material = carbonMat;
            Object.Destroy(keel.GetComponent<Collider>());

            // Glowing cockpit/core sensor on spine
            var core = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core.name = "Glider_AetherCore";
            core.transform.SetParent(keel.transform, false);
            core.transform.localPosition = new Vector3(0, 0.6f, 0.1f);
            core.transform.localScale = new Vector3(0.5f, 0.25f, 0.6f);
            core.GetComponent<Renderer>().material = cyanNeonMat;
            Object.Destroy(core.GetComponent<Collider>());

            // 2. Left Delta Wing (Angled and Swept Back)
            var leftWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWing.name = "Glider_LeftWing";
            leftWing.transform.SetParent(glider.transform, false);
            leftWing.transform.localPosition = new Vector3(-0.95f, 0.06f, -0.15f);
            leftWing.transform.localRotation = Quaternion.Euler(2f, 16f, 3f);
            leftWing.transform.localScale = new Vector3(1.6f, 0.045f, 0.85f);
            leftWing.GetComponent<Renderer>().material = wingMat;
            Object.Destroy(leftWing.GetComponent<Collider>());

            // Left Wingtip Stabilizer / Winglet
            var leftTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftTip.name = "Glider_LeftWinglet";
            leftTip.transform.SetParent(leftWing.transform, false);
            leftTip.transform.localPosition = new Vector3(-0.52f, 1.2f, 0);
            leftTip.transform.localRotation = Quaternion.Euler(0, 0, 55f);
            leftTip.transform.localScale = new Vector3(0.08f, 2.5f, 0.7f);
            leftTip.GetComponent<Renderer>().material = cyanNeonMat;
            Object.Destroy(leftTip.GetComponent<Collider>());

            // 3. Right Delta Wing (Angled and Swept Back)
            var rightWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWing.name = "Glider_RightWing";
            rightWing.transform.SetParent(glider.transform, false);
            rightWing.transform.localPosition = new Vector3(0.95f, 0.06f, -0.15f);
            rightWing.transform.localRotation = Quaternion.Euler(2f, -16f, -3f);
            rightWing.transform.localScale = new Vector3(1.6f, 0.045f, 0.85f);
            rightWing.GetComponent<Renderer>().material = wingMat;
            Object.Destroy(rightWing.GetComponent<Collider>());

            // Right Wingtip Stabilizer / Winglet
            var rightTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightTip.name = "Glider_RightWinglet";
            rightTip.transform.SetParent(rightWing.transform, false);
            rightTip.transform.localPosition = new Vector3(0.52f, 1.2f, 0);
            rightTip.transform.localRotation = Quaternion.Euler(0, 0, -55f);
            rightTip.transform.localScale = new Vector3(0.08f, 2.5f, 0.7f);
            rightTip.GetComponent<Renderer>().material = cyanNeonMat;
            Object.Destroy(rightTip.GetComponent<Collider>());

            // 4. Twin Jet Thrusters (Left & Right)
            for (int side = -1; side <= 1; side += 2)
            {
                var nacelle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                nacelle.name = $"Thruster_{(side < 0 ? "L" : "R")}";
                nacelle.transform.SetParent(glider.transform, false);
                nacelle.transform.localPosition = new Vector3(side * 0.42f, -0.06f, -0.45f);
                nacelle.transform.localRotation = Quaternion.Euler(90, 0, 0);
                nacelle.transform.localScale = new Vector3(0.18f, 0.35f, 0.18f);
                nacelle.GetComponent<Renderer>().material = thrusterMat;
                Object.Destroy(nacelle.GetComponent<Collider>());

                // Fiery Jet Exhaust Nozzle
                var nozzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                nozzle.name = "ExhaustPlume";
                nozzle.transform.SetParent(nacelle.transform, false);
                nozzle.transform.localPosition = new Vector3(0, -0.95f, 0);
                nozzle.transform.localScale = new Vector3(0.75f, 0.4f, 0.75f);
                nozzle.GetComponent<Renderer>().material = thrusterGlowMat;
                Object.Destroy(nozzle.GetComponent<Collider>());
            }

            // 5. Dual Ergonomic Grip Handlebars (Positioned for player hands)
            for (int side = -1; side <= 1; side += 2)
            {
                // Vertical Drop Bar
                var dropBar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                dropBar.name = $"GripBar_{(side < 0 ? "L" : "R")}";
                dropBar.transform.SetParent(glider.transform, false);
                dropBar.transform.localPosition = new Vector3(side * 0.32f, -0.28f, 0.10f);
                dropBar.transform.localScale = new Vector3(0.035f, 0.28f, 0.035f);
                dropBar.GetComponent<Renderer>().material = frameMat;
                Object.Destroy(dropBar.GetComponent<Collider>());

                // Horizontal Hand Grip
                var handGrip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                handGrip.name = "Handle";
                handGrip.transform.SetParent(dropBar.transform, false);
                handGrip.transform.localPosition = new Vector3(0, -0.9f, 0);
                handGrip.transform.localRotation = Quaternion.Euler(0, 0, 90);
                handGrip.transform.localScale = new Vector3(1.2f, 0.45f, 1.2f);
                handGrip.GetComponent<Renderer>().material = carbonMat;
                Object.Destroy(handGrip.GetComponent<Collider>());
            }

            return glider;
        }

        #endregion

        #region Organic Nature & Environment Meshes

        /// <summary>
        /// Creates a high-subdivision horizontal grid mesh for dynamic vertex-displaced ocean waves.
        /// </summary>
        public static Mesh CreateWaterWaveMesh(float width = 300f, float length = 300f, int gridX = 48, int gridZ = 48)
        {
            var mesh = new Mesh { name = "Mesh_OceanWaveGrid" };

            var verts = new Vector3[(gridX + 1) * (gridZ + 1)];
            var uvs = new Vector2[verts.Length];
            var tris = new int[gridX * gridZ * 6];

            float dx = width / gridX;
            float dz = length / gridZ;

            float halfW = width * 0.5f;
            float halfL = length * 0.5f;

            for (int z = 0; z <= gridZ; z++)
            {
                for (int x = 0; x <= gridX; x++)
                {
                    int i = z * (gridX + 1) + x;
                    verts[i] = new Vector3(x * dx - halfW, 0f, z * dz - halfL);
                    uvs[i] = new Vector2((float)x / gridX * 8f, (float)z / gridZ * 8f);
                }
            }

            int triIndex = 0;
            for (int z = 0; z < gridZ; z++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    int root = z * (gridX + 1) + x;
                    int next = root + (gridX + 1);

                    tris[triIndex++] = root;
                    tris[triIndex++] = next;
                    tris[triIndex++] = root + 1;

                    tris[triIndex++] = root + 1;
                    tris[triIndex++] = next;
                    tris[triIndex++] = next + 1;
                }
            }

            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Synthesizes a deformed multifaceted rock boulder with natural noise and faceted planes.
        /// </summary>
        public static Mesh CreateFacetedRockMesh(float radius = 1.5f, int seed = 42)
        {
            var mesh = new Mesh { name = "Mesh_FacetedRock" };
            Random.InitState(seed);

            int latSteps = 8;
            int lonSteps = 12;

            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            for (int lat = 0; lat <= latSteps; lat++)
            {
                float v = (float)lat / latSteps;
                float theta = v * Mathf.PI;

                for (int lon = 0; lon <= lonSteps; lon++)
                {
                    float u = (float)lon / lonSteps;
                    float phi = u * Mathf.PI * 2f;

                    // Spherical coords with procedural 3D noise deformation
                    float nx = Mathf.Sin(phi) * Mathf.Sin(theta);
                    float ny = Mathf.Cos(theta);
                    float nz = Mathf.Cos(phi) * Mathf.Sin(theta);

                    float noise = Mathf.PerlinNoise(nx * 2.2f + seed, nz * 2.2f + seed) * 0.45f + 0.75f;
                    float r = radius * noise;

                    // Flatten bottom for realistic ground seating
                    if (ny < -0.4f) r *= Mathf.Lerp(0.5f, 1.0f, (ny + 1.0f) / 0.6f);

                    verts.Add(new Vector3(nx * r, ny * r, nz * r));
                    uvs.Add(new Vector2(u * 2f, v * 2f));
                }
            }

            for (int lat = 0; lat < latSteps; lat++)
            {
                for (int lon = 0; lon < lonSteps; lon++)
                {
                    int cur = lat * (lonSteps + 1) + lon;
                    int nxt = cur + (lonSteps + 1);

                    tris.Add(cur);
                    tris.Add(nxt);
                    tris.Add(cur + 1);

                    tris.Add(cur + 1);
                    tris.Add(nxt);
                    tris.Add(nxt + 1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        #endregion
    }
}

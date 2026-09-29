using UnityEngine;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Reusable visual-only ground/circulation skins for Valoria.
    /// These pieces never own gameplay collision or route topology: certified floors,
    /// stairs, plots and hotspots remain authoritative underneath.
    /// </summary>
    public static class ValoriaGroundKit
    {
        // Validation trigger: Ground Kit v1 integrated review on current main.
        static readonly Color StreetStone = new Color(.43f,.405f,.35f);
        static readonly Color TerraceStone = new Color(.355f,.325f,.275f);
        static readonly Color RetainingStone = new Color(.315f,.305f,.285f);
        static readonly Color EarthBlend = new Color(.31f,.285f,.24f);

        public static GameObject StreetStraight(string name, Vector3 center, float length, float width, float yawDegrees)
        {
            var root = Root(name, center, yawDegrees);
            int segments = Mathf.Max(3, Mathf.CeilToInt(length / 1.25f));
            float segmentLength = length / segments;
            var art = ValoriaExternalAssetLibrary.Load();
            for (int i = 0; i < segments; i++)
            {
                float z = -length * .5f + segmentLength * (.5f + i);
                float w = width * (1f - ((i % 3) * .018f));
                float skew = (i % 4 == 0 ? -.035f : (i % 4 == 2 ? .03f : 0f));
                var patch = Patch(name + " · street " + (i + 1),
                    new Vector3(skew, .002f + (i % 2) * .002f, z),
                    w, segmentLength * 1.12f, StreetStone * (1.02f + (i % 3) * .018f), i);
                patch.transform.SetParent(root.transform, false);

                if (art != null && art.SlavicCobbleRoad != null)
                {
                    var authored = ValoriaKit.BenchmarkPieceModulated(name + " · authored cobble " + (i + 1),
                        art.SlavicCobbleRoad,
                        root.transform.TransformPoint(new Vector3(skew, .015f, z)),
                        w * .96f, .14f,
                        root.transform.rotation * Quaternion.Euler(0, i % 2 == 0 ? -2.5f : 2f, 0),
                        new Color(.90f,.88f,.82f,1f));
                    if (authored != null) authored.transform.SetParent(root.transform, true);
                }
            }
            return root;
        }

        public static GameObject StreetBlendWidening(string name, Vector3 center, float width, float depth, float yawDegrees)
        {
            var root = Root(name, center, yawDegrees);
            var basePatch = Patch(name + " · widening base", Vector3.zero, width, depth, StreetStone * .96f, 17);
            basePatch.transform.SetParent(root.transform, false);
            var inner = Patch(name + " · worn centre", new Vector3(width * .04f, .004f, -depth * .03f),
                width * .74f, depth * .83f, StreetStone * 1.08f, 31);
            inner.transform.SetParent(root.transform, false);
            var art = ValoriaExternalAssetLibrary.Load();
            if (art != null && art.SlavicCobbleRoad != null)
            {
                var authored = ValoriaKit.BenchmarkPieceModulated(name + " · authored widening cobble",
                    art.SlavicCobbleRoad, center + Vector3.up * .018f,
                    Mathf.Min(width, depth) * .92f, .14f, Quaternion.Euler(0, yawDegrees, 0),
                    new Color(.90f,.88f,.82f,1f));
                if (authored != null) authored.transform.SetParent(root.transform, true);
            }
            return root;
        }

        public static GameObject TerraceFloor(string name, Vector3 center, float width, float depth, float yawDegrees = 0f)
        {
            var root = Root(name, center, yawDegrees);
            var patch = Patch(name + " · terrace", Vector3.zero, width, depth, TerraceStone, 43);
            patch.transform.SetParent(root.transform, false);
            var earth = Patch(name + " · earth wear", new Vector3(width * .09f, .003f, depth * .07f),
                width * .57f, depth * .42f, EarthBlend * 1.06f, 59);
            earth.transform.SetParent(root.transform, false);
            var art = ValoriaExternalAssetLibrary.Load();
            if (art != null && art.SlavicMudFlat != null)
            {
                var authored = ValoriaKit.BenchmarkPieceModulated(name + " · authored terrace wear",
                    art.SlavicMudFlat, center + Vector3.up * .014f,
                    Mathf.Min(width, depth) * .86f, .10f, Quaternion.Euler(0, yawDegrees, 0),
                    new Color(.92f,.88f,.78f,1f));
                if (authored != null) authored.transform.SetParent(root.transform, true);
            }
            return root;
        }

        public static GameObject RetainingEdge(string name, Vector3 center, float length, float height, float yawDegrees = 0f)
        {
            var root = Root(name, center, yawDegrees);
            int blocks = Mathf.Max(3, Mathf.CeilToInt(length / .9f));
            float step = length / blocks;
            var art = ValoriaExternalAssetLibrary.Load();
            for (int i = 0; i < blocks; i++)
            {
                float x = -length * .5f + step * (.5f + i);
                float h = height * (.92f + (i % 3) * .035f);
                var block = ValoriaKit.Block(name + " · masonry " + (i + 1),
                    new Vector3(x, h * .5f, (i % 2 == 0 ? -.015f : .015f)),
                    new Vector3(step * 1.04f, h, .32f),
                    RetainingStone * (.98f + (i % 3) * .025f));
                StripCollider(block);
                block.transform.SetParent(root.transform, false);

                if (art != null && art.SlavicStoneFence != null && i % 2 == 0)
                {
                    var world = root.transform.TransformPoint(new Vector3(x, .03f, -.10f));
                    var authored = ValoriaKit.BenchmarkPieceModulated(name + " · authored retaining face " + (i + 1),
                        art.SlavicStoneFence, world, step * 1.65f, h * 1.12f,
                        root.transform.rotation, new Color(.82f,.84f,.82f,1f));
                    if (authored != null) authored.transform.SetParent(root.transform, true);
                }
            }
            return root;
        }

        public static GameObject GroundSeam(string name, Vector3 center, float width, float depth, float yawDegrees = 0f)
        {
            var root = Root(name, center, yawDegrees);
            var patch = Patch(name + " · seam earth", Vector3.zero, width, depth, EarthBlend, 71);
            patch.transform.SetParent(root.transform, false);
            for (int i = 0; i < 4; i++)
            {
                float x = (-.36f + i * .24f) * width;
                float z = ((i % 2 == 0) ? -.22f : .18f) * depth;
                ValoriaKit.RockCluster(name + " · buried rock " + (i + 1),
                    center + Quaternion.Euler(0, yawDegrees, 0) * new Vector3(x, -.08f, z),
                    .30f + i * .035f, 3);
            }
            return root;
        }

        static GameObject Root(string name, Vector3 center, float yawDegrees)
        {
            var root = new GameObject(name);
            root.transform.position = center;
            root.transform.rotation = Quaternion.Euler(0, yawDegrees, 0);
            return root;
        }

        static GameObject Patch(string name, Vector3 localCenter, float width, float depth, Color color, int seed)
        {
            var go = new GameObject(name);
            go.transform.localPosition = localCenter;

            float hx = width * .5f;
            float hz = depth * .5f;
            float j1 = Jitter(seed, .055f);
            float j2 = Jitter(seed + 7, .045f);
            float j3 = Jitter(seed + 13, .055f);
            float j4 = Jitter(seed + 19, .045f);

            var v = new[]
            {
                new Vector3(-hx + width*j1, 0, -hz),
                new Vector3(0, 0, -hz + depth*j2),
                new Vector3(hx, 0, -hz + depth*j3),
                new Vector3(hx - width*j2, 0, 0),
                new Vector3(hx + width*j4, 0, hz),
                new Vector3(0, 0, hz - depth*j1),
                new Vector3(-hx, 0, hz + depth*j2),
                new Vector3(-hx + width*j3, 0, 0),
                Vector3.zero
            };
            var triangles = new int[8 * 3];
            for (int i = 0; i < 8; i++)
            {
                int t = i * 3;
                triangles[t] = 8;
                triangles[t + 1] = i;
                triangles[t + 2] = (i + 1) % 8;
            }
            var uv = new Vector2[v.Length];
            for (int i = 0; i < v.Length; i++)
                uv[i] = new Vector2((v[i].x / width) + .5f, (v[i].z / depth) + .5f);

            var mesh = new Mesh { name = name + " mesh", vertices = v, triangles = triangles, uv = uv };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = GroundSurfaceMaterial(color,width,depth);
            return go;
        }

        static Material GroundSurfaceMaterial(Color color,float width,float depth)
        {
            var source=ValoriaKit.Material(color);
            var material=new Material(source){name="Valoria Ground · "+ColorUtility.ToHtmlStringRGB(color)};
            // ValoriaKit's generated base map already contains the target colour. Ground patches
            // use a neutral material tint so the texture is not multiplied/darkened a second time.
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",Color.white);
            if(material.HasProperty("_Color"))material.SetColor("_Color",Color.white);
            var tiling=new Vector2(Mathf.Max(3f,width/1.15f),Mathf.Max(3f,depth/1.15f));
            if(material.HasProperty("_BaseMap"))material.SetTextureScale("_BaseMap",tiling);
            else if(material.HasProperty("_MainTex"))material.SetTextureScale("_MainTex",tiling);
            if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.035f);
            return material;
        }

        static float Jitter(int seed, float magnitude)
        {
            int n = (seed * 1103515245 + 12345) & 0x7fffffff;
            return (((n % 1000) / 999f) - .5f) * magnitude;
        }

        static void StripCollider(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
                Object.Destroy(c);
        }
    }
}

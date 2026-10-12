using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using GLTFast;

namespace Eldoria.Art
{
    /// <summary>
    /// Isolated experimental Valoria family renderer.
    /// Uses actual GLB inputs; never modifies the canonical world.
    /// The same mesh instance is reused for baseline and lookdev captures.
    /// </summary>
    public sealed class ValoriaFamilyLookdev : MonoBehaviour
    {
        [Serializable]
        public sealed class Piece
        {
            public string fileName;
            public Vector3 position;
            public Vector3 rotationEuler;
            public Vector3 scale = Vector3.one;
        }

        public List<Piece> pieces = new List<Piece>();
        public bool enableLookdev = true;
        public Color stoneTint = new Color(0.91f, 0.85f, 0.72f);
        public Color sunColor = new Color(1f, 0.87f, 0.69f);
        public float sunIntensity = 1.3f;
        public float baselineSunIntensity = 1.0f;
        public Transform stagingRoot;
        public Camera captureCamera;
        public string inputDirectory = "ValoriaFamilyV3";

        private readonly List<GameObject> spawned = new List<GameObject>();
        private Light sun;
        private readonly Dictionary<Material, Material> originals = new Dictionary<Material, Material>();
        private readonly List<Material> temporary = new List<Material>();

        private IEnumerator Start()
        {
            if (stagingRoot == null)
            {
                var root = new GameObject("ValoriaFamilyV3_ExperimentalRoot");
                root.transform.SetParent(transform, false);
                stagingRoot = root.transform;
            }
            sun = FindOrCreateSun();
            foreach (var piece in pieces)
            {
                if (string.IsNullOrWhiteSpace(piece.fileName) || piece.fileName.Contains("..") ||
                    Path.GetFileName(piece.fileName) != piece.fileName)
                {
                    Debug.LogError("Invalid GLB filename; refusing to load candidate.");
                    continue;
                }
                string full = Path.Combine(Application.streamingAssetsPath, inputDirectory, piece.fileName);
                var importer = new GltfImport();
                var task = importer.Load("file://" + full.Replace('\\', '/'));
                while (!task.IsCompleted) yield return null;
                if (task.IsFaulted || !task.Result)
                {
                    Debug.LogError("GLB import failed: " + piece.fileName);
                    continue;
                }
                var holder = new GameObject(Path.GetFileNameWithoutExtension(piece.fileName));
                holder.transform.SetParent(stagingRoot, false);
                holder.transform.localPosition = piece.position;
                holder.transform.localEulerAngles = piece.rotationEuler;
                holder.transform.localScale = piece.scale;
                var inst = importer.InstantiateMainSceneAsync(holder.transform);
                while (!inst.IsCompleted) yield return null;
                if (inst.IsFaulted || !inst.Result)
                {
                    Debug.LogError("GLB instantiate failed: " + piece.fileName);
                    Destroy(holder);
                    continue;
                }
                spawned.Add(holder);
            }
            ApplyMode(enableLookdev);
        }

        private Light FindOrCreateSun()
        {
            var go = new GameObject("Valoria_StudioSun");
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            return light;
        }

        public void ApplyMode(bool lookdev)
        {
            enableLookdev = lookdev;
            if (sun == null) return;
            sun.color = lookdev ? sunColor : Color.white;
            sun.intensity = lookdev ? sunIntensity : baselineSunIntensity;
            foreach (var root in spawned)
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    var shared = renderer.sharedMaterials;
                    var changed = false;
                    for (var i = 0; i < shared.Length; i++)
                    {
                        var material = shared[i];
                        if (material == null) continue;
                        if (!originals.ContainsKey(material)) originals[material] = material;
                        if (!lookdev) continue;
                        // Non-destructive material instance; preserves source albedo and normal textures.
                        var copy = new Material(material);
                        copy.name = material.name + "_ValoriaLookdev";
                        if (copy.HasProperty("_BaseColor"))
                        {
                            var baseColor = copy.GetColor("_BaseColor");
                            copy.SetColor("_BaseColor", Color.Lerp(baseColor, baseColor * stoneTint, 0.2f));
                        }
                        if (copy.HasProperty("_Smoothness"))
                            copy.SetFloat("_Smoothness", Mathf.Clamp01(copy.GetFloat("_Smoothness") * .85f));
                        shared[i] = copy;
                        temporary.Add(copy);
                        changed = true;
                    }
                    if (changed) renderer.sharedMaterials = shared;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var material in temporary)
                if (material != null) Destroy(material);
        }
    }
}

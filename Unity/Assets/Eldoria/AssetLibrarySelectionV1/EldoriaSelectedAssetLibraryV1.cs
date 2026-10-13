using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.AssetLibrary
{
    // Opt-in reuse of certified source files. This file does NOT hook into Valoria's canonical scene.
    // Separate artist/Unity/mobile approval is required for production promotion.
    public static class EldoriaSelectedAssetLibraryV1
    {
        public const string CatalogResource = "EldoriaSelectedAssetCatalogV1";
        public const string GeneratedPrefabsResource = "EldoriaSelectedAssetsV1/";
        private static Catalog cached;

        [Serializable]
        public sealed class Catalog
        {
            public int schema_version;
            public string catalog_id;
            public bool not_a_unity_visual_pass;
            public Entry[] entries;
        }

        [Serializable]
        public sealed class Entry
        {
            public string id, origin, source_relative, resource_key, sha256, role, tier;
            public string source_commit, render_verification, comment;
            public float score;
            public bool unity_verified;
        }

        public static Catalog LoadCatalog()
        {
            if (cached != null) return cached;
            var json = Resources.Load<TextAsset>(CatalogResource);
            if (json == null) throw new InvalidOperationException("Selected asset catalog JSON missing");
            var catalog = JsonUtility.FromJson<Catalog>(json.text);
            if (catalog == null || catalog.schema_version != 1 || catalog.entries == null
                || catalog.entries.Length != 36 || !catalog.not_a_unity_visual_pass)
                throw new InvalidOperationException("Selected catalog schema or audit identity invalid.");
            cached = catalog;
            return cached;
        }

        public static Entry Lookup(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Empty asset id", nameof(id));
            foreach (var entry in LoadCatalog().entries)
                if (string.Equals(entry.id, id, StringComparison.Ordinal)) return entry;
            throw new ArgumentException("Unknown audited asset: " + id, nameof(id));
        }

        // No automatic scene mutation, gameplay colliders, hotspots, or global presentation toggles.
        // Model source or generated prefab is instantiated ONLY on a caller's explicit request.
        public static GameObject SpawnSelectedVisual(string id, Transform parent, Vector3 position,
            Quaternion rotation, Vector3 scale)
        {
            var e = Lookup(id);
            if (e.tier != "SELECTED" || e.origin != "legacy_canonical"
                || string.IsNullOrEmpty(e.resource_key) || string.IsNullOrEmpty(e.sha256))
                throw new InvalidOperationException("Asset is not selected for safe staging: " + id);

            var source = Resources.Load<GameObject>(GeneratedPrefabsResource + id);
            if (source == null) source = Resources.Load<GameObject>(e.resource_key);
            if (source == null) throw new InvalidOperationException("Selected Unity GLB source missing: " + e.resource_key);

            // Refuse any source carrying scripts BEFORE Instantiate/Awake can execute.
            if (source.GetComponentsInChildren<MonoBehaviour>(true).Length != 0)
                throw new InvalidOperationException("Selected source contains runtime scripts; not safe as visual-only: " + id);

            // Imported GLBs can resolve to model subobjects typed Component.
            Object instance = Object.Instantiate((Object)source);
            var go = instance as GameObject;
            if (go == null && instance is Component component) go = component.gameObject;
            if (go == null) throw new InvalidOperationException("Imported selected source is not a GameObject: " + id);

            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Object.Destroy(go);
                throw new InvalidOperationException("Selected source contains no renderers: " + id);
            }

            // Hard visual-only contract: imported source cannot introduce gameplay colliders/behaviours.
            foreach (var collider in go.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
            foreach (var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))
                behaviour.enabled = false;

            go.name = "Selected art only · " + id;
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            go.transform.localScale = scale;
            return go;
        }
    }
}

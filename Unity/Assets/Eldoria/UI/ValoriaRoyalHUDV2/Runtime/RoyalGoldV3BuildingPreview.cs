using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>One cached 256px view of the canonical selected building meshes.
    /// Copies render data only: no gameplay scripts, colliders, clocks or model imports.</summary>
    public sealed class RoyalGoldV3BuildingPreview : MonoBehaviour
    {
        public Func<string, GameObject> ModelFor;
        RawImage image;
        RenderTexture texture;
        GameObject lastModel;
        string lastKey;
        public bool HasModelPreview => image && image.enabled && texture;
        public void Initialize(RawImage target) { image = target; image.raycastTarget = false; }
        public bool Show(string id, string level)
        {
            var model = ModelFor != null ? ModelFor(id) : null;
            string key = id + ":" + level;
            if (model == lastModel && key == lastKey) return HasModelPreview;
            lastModel = model; lastKey = key;
            image.enabled = false;
            if (!model) return false;
            // The isolated snapshot never changes the real object's layers,
            // visibility, materials, transform, or the city/world cameras.
            var stage = new GameObject("Temporary canonical building preview");
            stage.hideFlags = HideFlags.HideAndDontSave;
            stage.transform.position = new Vector3(0, -10000, 0);
            Bounds bounds = default;
            bool any = false;
            try
            {
                foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
                {
                    var source = filter.GetComponent<MeshRenderer>();
                    if (!filter.sharedMesh || !source || !source.enabled) continue;
                    bool visible = true;
                    for (var t = filter.transform; t && t != model.transform; t = t.parent)
                        if (!t.gameObject.activeSelf) { visible = false; break; }
                    if (!visible) continue;
                    var part = new GameObject("Preview mesh");
                    part.layer = 31;
                    part.transform.SetParent(stage.transform, false);
                    part.transform.localPosition = filter.transform.position - model.transform.position;
                    part.transform.rotation = filter.transform.rotation;
                    part.transform.localScale = filter.transform.lossyScale;
                    part.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
                    var renderer = part.AddComponent<MeshRenderer>();
                    renderer.sharedMaterials = source.sharedMaterials;
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    if (any) bounds.Encapsulate(renderer.bounds); else { bounds = renderer.bounds; any = true; }
                }
                if (!any) return false;
                if (!texture)
                {
                    texture = new RenderTexture(256, 256, 24, RenderTextureFormat.ARGB32);
                    texture.name = "Canonical building thumbnail";
                    texture.Create();
                }
                var cameraObject = new GameObject("One-shot building camera");
                cameraObject.transform.SetParent(stage.transform, false);
                var camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.orthographic = true;
                camera.aspect = 1;
                camera.cullingMask = 1 << 31;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.035f, .055f, .075f, 1);
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.nearClipPlane = .01f;
                float radius = Mathf.Max(.1f, bounds.extents.magnitude);
                camera.farClipPlane = radius * 8;
                camera.orthographicSize = radius * 1.08f;
                camera.transform.rotation = Quaternion.Euler(25, -25, 0);
                camera.transform.position = bounds.center - camera.transform.forward * radius * 3;
                camera.targetTexture = texture;
                var lightObject = new GameObject("Preview key light");
                lightObject.transform.SetParent(stage.transform, false);
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.cullingMask = 1 << 31;
                light.intensity = 1.2f;
                light.shadows = LightShadows.None;
                light.transform.rotation = Quaternion.Euler(45, -35, 0);
                camera.Render();
                camera.targetTexture = null;
                image.texture = texture;
                image.enabled = true;
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("BUILDING_PREVIEW_UNAVAILABLE " + id + " " + error.GetType().Name);
                return false;
            }
            finally { stage.SetActive(false); Destroy(stage); }
        }
        void OnDestroy()
        {
            if (texture) { texture.Release(); Destroy(texture); }
        }
    }
}

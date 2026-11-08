using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Independent picture well for an existing REAL selected building. This
    /// takes a separate view of the actual Valoria meshes/materials/lighting
    /// without copying the old HUD miniature or changing city cameras/saves.
    /// </summary>
    public sealed class BreachWatchBuildingPortrait : MonoBehaviour
    {
        RawImage image;
        RenderTexture texture;
        GameObject cameraRoot;
        Camera portraitCamera;
        GameObject lastModel;
        string lastKey;

        public bool HasLiveBuilding => image && image.enabled && image.texture == texture && lastModel;
        public void Initialize(RawImage target)
        {
            image = target;
            if(image)
            {
                image.raycastTarget = false;
                image.enabled = false;
            }
        }

        public void Show(GameObject model, Camera source, string key)
        {
            if(!image) return;
            if(!model || !source || !model.activeInHierarchy)
            {
                image.enabled = false;
                lastModel = null;
                lastKey = null;
                return;
            }
            if(model == lastModel && lastKey == key && HasLiveBuilding) return;

            var renderers = model.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = default;
            bool hasBounds = false;
            foreach(var renderer in renderers)
            {
                if(!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                var b = renderer.bounds;
                if(b.size.sqrMagnitude < .0001f) continue;
                if(!hasBounds) { bounds = b;hasBounds = true; }
                else bounds.Encapsulate(b);
            }
            if(!hasBounds)
            {
                image.enabled = false;
                lastModel = null;
                return;
            }

            if(!texture)
            {
                texture = new RenderTexture(512,256,24,RenderTextureFormat.ARGB32);
                texture.name = "Vigilia / real Valoria building portrait";
                texture.antiAliasing = 2;
                texture.Create();
            }
            if(!portraitCamera)
            {
                cameraRoot = new GameObject("Vigilia / isolated portrait camera");
                cameraRoot.hideFlags = HideFlags.HideAndDontSave;
                portraitCamera = cameraRoot.AddComponent<Camera>();
                portraitCamera.enabled = false;
            }
            // Use actual source camera projection/light; never move or alter it.
            portraitCamera.CopyFrom(source);
            portraitCamera.enabled = false;
            portraitCamera.targetTexture = texture;
            portraitCamera.clearFlags = CameraClearFlags.SolidColor;
            portraitCamera.backgroundColor = new Color(.095f,.10f,.105f,1);
            portraitCamera.cullingMask = source.cullingMask & ~(1 << 5);
            portraitCamera.allowHDR = false;
            portraitCamera.aspect = texture.width/(float)texture.height;
            portraitCamera.orthographic = true;
            portraitCamera.transform.rotation = source.transform.rotation;

            // Project the original bounds in the thumbnail camera basis, not
            // against the previous HUD's camera or a fictional reference image.
            var right = portraitCamera.transform.right;
            var up = portraitCamera.transform.up;
            float maxX = 0, maxY = 0;
            var bmin = bounds.min;var bmax = bounds.max;
            for(int i=0;i<8;i++)
            {
                var p=new Vector3((i&1)!=0?bmax.x:bmin.x,
                    (i&2)!=0?bmax.y:bmin.y,
                    (i&4)!=0?bmax.z:bmin.z)-bounds.center;
                maxX = Mathf.Max(maxX,Mathf.Abs(Vector3.Dot(p,right)));
                maxY = Mathf.Max(maxY,Mathf.Abs(Vector3.Dot(p,up)));
            }
            portraitCamera.orthographicSize = Mathf.Max(2.5f,
                Mathf.Max(maxY, maxX/portraitCamera.aspect)*1.25f);
            portraitCamera.nearClipPlane = .1f;
            portraitCamera.farClipPlane = 1400f;
            portraitCamera.transform.position =
                bounds.center-portraitCamera.transform.forward*130f;
            portraitCamera.Render();
            portraitCamera.targetTexture = null;
            image.texture = texture;
            image.enabled = true;
            lastModel = model;
            lastKey = key;
        }

        void OnDestroy()
        {
            if(cameraRoot)Destroy(cameraRoot);
            if(texture)
            {
                texture.Release();
                Destroy(texture);
            }
        }
    }
}

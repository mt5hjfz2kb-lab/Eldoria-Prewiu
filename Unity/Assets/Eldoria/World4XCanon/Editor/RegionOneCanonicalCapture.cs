#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEngine;

namespace Eldoria.World4XCanon.Editor
{
    /// <summary>
    /// Captures the active *playable integrated* scene, not an off-line mock-up.
    /// Run from Unity's menu WHILE Valoria is playing and the real World has unlocked.
    /// Produces exactly identified 3 zoom levels in mobile landscape and portrait.
    /// No save, gateway, progression or publication operations are performed.
    /// </summary>
    public static class RegionOneCanonicalCapture
    {
        [Serializable]
        sealed class CameraEvidence
        {
            public string view, environment, scene, unityVersion, sha256;
            public int width, height, visibleRenderers;
            public float orthographicSize, aspect;
            public Vector3 worldTarget, cameraPosition;
            public bool isActualUnityCamera, isPlayMode, isPhysicalIphone;
        }

        [MenuItem("Eldoria/Region 1/Capture 3 actual integrated game views")]
        public static void Capture()
        {
            if (!EditorApplication.isPlaying)
                throw new InvalidOperationException("Capture requires PlayMode in the real integrated game; no staged/offline scenes.");
            var host = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            if (!host || !host.Ready || !host.InWorld || !host.WorldRoot ||
                !host.WorldRoot.activeInHierarchy || !host.WorldCamera ||
                !host.WorldCamera.enabled || !host.WorldNavigation)
                throw new InvalidOperationException("Enter the real unlocked Mundo from Valoria before capturing.");
            if (!host.WorldRoot.transform.Find("Region 1 canonical continuous geography"))
                throw new InvalidOperationException("Canonical geography adapter is not present in actual WorldRoot.");

            string output = Environment.GetEnvironmentVariable("ELDORIA_REGION1_CAPTURE_OUT");
            if (string.IsNullOrEmpty(output)) output = "RegionOneCanonicalUnityCaptures";
            output = Path.GetFullPath(output);
            if (Directory.Exists(output) && Directory.GetFileSystemEntries(output).Length != 0)
                throw new InvalidOperationException("Use a fresh evidence folder; never replace previous Unity images.");
            Directory.CreateDirectory(output);

            var camera = host.WorldCamera;
            var nav = host.WorldNavigation;
            float oldZoom = camera.orthographicSize;
            float oldAspect = camera.aspect;
            Vector3 oldTarget = nav.target;
            RenderTexture oldTargetTexture = camera.targetTexture;
            RenderTexture oldActive = RenderTexture.active;
            try
            {
                foreach (var size in new [] { (1280,720,"landscape"), (390,844,"portrait") })
                {
                    camera.aspect = size.Item1 / (float)size.Item2;
                    foreach (var stage in new [] { (2,"cercana"), (1,"regional"), (0,"panoramica") })
                    {
                        nav.SetZoomLevel(stage.Item1, true);
                        nav.PanTo(host.WorldHome);
                        nav.FitToAuthoredBounds();
                        Canvas.ForceUpdateCanvases();
                        CaptureCamera(output, camera, nav, stage.Item2, size.Item3, size.Item1, size.Item2);
                    }
                }
                Debug.Log("REGION1_CANON_PLAYMODE_CAPTURE_SAVED views=6 native_unity=1 physical_ios=0 path=" + output);
            }
            finally
            {
                camera.targetTexture = oldTargetTexture;
                RenderTexture.active = oldActive;
                camera.aspect = oldAspect;
                nav.RestoreView(oldTarget, oldZoom);
            }
        }

        static void CaptureCamera(string output, Camera camera, Eldoria.World4XPrototype.ZoneOneStrategicCamera nav,
                                  string view, string orientation, int width, int height)
        {
            var target = new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex = new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                tex.ReadPixels(new Rect(0,0,width,height),0,0);
                tex.Apply();
                string name = view + "-" + orientation;
                var bytes = tex.EncodeToPNG();
                string filename = Path.Combine(output, name + "-unity.png");
                File.WriteAllBytes(filename, bytes);
                string hash;
                using (var sha = SHA256.Create())
                    hash = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
                var evidence = new CameraEvidence
                {
                    view = view, environment = "Unity Editor PlayMode integrated Valoria",
                    scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path,
                    unityVersion = Application.unityVersion, sha256 = hash,
                    width = width, height = height,
                    visibleRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length,
                    orthographicSize = camera.orthographicSize, aspect = camera.aspect,
                    worldTarget = nav.target, cameraPosition = camera.transform.position,
                    isActualUnityCamera = true, isPlayMode = Application.isPlaying,
                    isPhysicalIphone = false
                };
                File.WriteAllText(Path.Combine(output,name + "-source.json"),JsonUtility.ToJson(evidence,true));
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = null;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.ArtReboot.CC0System.Editor
{
    /// <summary>
    /// Builds and captures an original isolated ART LOOKDEV scene from PHYSICAL GLB imports.
    /// Does not touch the shipping city/world scenes, gameplay, save files, or build settings.
    /// Caution: captures are Unity Editor renders, not physical iPhone tests or approval.
    /// </summary>
    public static class CC0SystemSceneBuilder
    {
        private const string Root = "Assets/Art_Reboot/CC0System";
        private const string Models = Root + "/Generated/";
        private const string Scenes = Root + "/Scenes";
        private const string Proof = "docs/cc0-art-system/previews/unity";

        [MenuItem("Eldoria/CC0 Art System/Build independent stage I and V + real Unity captures")]
        public static void BuildAndCapture()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Authoring is editor only. Exit Play Mode first.");

            Directory.CreateDirectory(Scenes);
            Directory.CreateDirectory(Proof);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            for (int stage = 1; stage <= 5; stage++)
            {
                var source = Models + "CC0_Valoria_Stage" + stage + ".glb";
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(source);
                if (model == null)
                {
                    AssetDatabase.ImportAsset(source, ImportAssetOptions.ForceSynchronousImport);
                    model = AssetDatabase.LoadAssetAtPath<GameObject>(source);
                }

                if (model == null)
                    throw new InvalidOperationException(
                        "Original physical GLB is not imported; first execute the isolated Blender authoring workflow: " + source);

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                SceneManager.SetActiveScene(scene);

                GameObject valoria = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (valoria == null)
                    valoria = UnityEngine.Object.Instantiate(model);
                if (valoria == null)
                    throw new InvalidOperationException("GLB could not instantiate: " + source);

                valoria.name = "ELDORIA ART SYSTEM / original authored Valoria Stage " + stage;
                valoria.transform.position = Vector3.zero;

                Color dusk = Hex("#646F80");
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = Hex("#9DA5AF");
                RenderSettings.ambientEquatorColor = Hex("#7E8392");
                RenderSettings.ambientGroundColor = Hex("#42485B");
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogColor = dusk;
                RenderSettings.fogStartDistance = 42;
                RenderSettings.fogEndDistance = 92;
                RenderSettings.reflectionIntensity = 0.35f;

                var sky = new GameObject("Sun | warm sunset");
                var sun = sky.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.color = Hex("#FFD0A0");
                sun.intensity = 1.45f;
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.72f;
                sun.shadowBias = 0.06f;
                sun.transform.rotation = Quaternion.Euler(61, -36, 0);
                RenderSettings.sun = sun;

                for (int i = 0; i < 2; i++)
                {
                    float x = i == 0 ? -1.7f : 1.7f;
                    var obj = new GameObject("Gate torch / point " + (i + 1) + " of 2");
                    obj.transform.position = new Vector3(x, 3.1f, 6.2f);
                    var l = obj.AddComponent<Light>();
                    l.type = LightType.Point;
                    l.color = Hex("#FFC27D");
                    l.range = 5.5f;
                    l.intensity = 1.15f;
                    l.shadows = LightShadows.None; // mobile-conscious
                }

                var cameraObject = new GameObject("Main Camera — authored isometric");
                cameraObject.tag = "MainCamera";
                var cam = cameraObject.AddComponent<Camera>();
                cam.orthographic = true;
                cam.nearClipPlane = 0.15f;
                cam.farClipPlane = 180;
                cam.backgroundColor = dusk;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.allowHDR = false;
                cam.allowMSAA = true;
                cam.transform.position = new Vector3(29, 35, 43);
                cam.transform.LookAt(new Vector3(0, 4.6f, -1.5f));
                cam.orthographicSize = 25f;

                var path = Scenes + "/CC0_Valoria_Bastion" + stage + ".unity";
                if (!EditorSceneManager.SaveScene(scene, path))
                    throw new InvalidOperationException("Scene save failed: " + path);
                if (stage == 5 &&
                    !EditorSceneManager.SaveScene(scene, Scenes + "/CC0_Valoria_Sandbox.unity", true))
                    throw new InvalidOperationException("Sandbox scene copy save failed");

                // Only endpoint captures are mandatory; all five stages remain available as serialized Unity scenes.
                if (stage == 1 || stage == 5)
                {
                    Capture(cam, stage, 1600, 900, 25f, new Vector3(29, 35, 43), "landscape");
                    Capture(cam, stage, 390, 844, 18f, new Vector3(23, 35, 43), "portrait");
                    Capture(cam, stage, 1100, 1250, 22f, new Vector3(29, 35, 43), "review");
                }
                Debug.Log("ELDORIA_CC0_NATIVE_AUTHORING_STAGE_" + stage +
                          " — real Unity Editor scene and renders; NOT art-approved or phone tested.");
            }
            AssetDatabase.SaveAssets();
        }

        private static Color Hex(string hex)
        {
            if (!ColorUtility.TryParseHtmlString(hex, out Color color))
                throw new FormatException("Invalid color: " + hex);
            return color;
        }

        private static void Capture(Camera cam, int stage, int width, int height,
                                    float size, Vector3 position, string profile)
        {
            float previousSize = cam.orthographicSize;
            Vector3 previousPosition = cam.transform.position;
            RenderTexture previousTarget = cam.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture rt = null;
            Texture2D tex = null;
            try
            {
                cam.orthographicSize = size;
                cam.transform.position = position;
                cam.transform.LookAt(new Vector3(0, 4.6f, -1.5f));
                rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                tex.Apply();
                string fullPath = Path.Combine(Proof, "CC0_UNITY_STAGE" + stage +
                                             "_" + profile + "_" + width + "x" + height + ".png");
                byte[] data = tex.EncodeToPNG();
                if (data == null || data.Length < 20000)
                    throw new InvalidOperationException("Empty Unity capture: " + fullPath);
                File.WriteAllBytes(fullPath, data);
                Debug.Log("ELDORIA_CC0_REAL_UNITY_CAPTURE " + fullPath + " " + data.Length);
            }
            finally
            {
                cam.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                cam.orthographicSize = previousSize;
                cam.transform.position = previousPosition;
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
                if (rt != null) RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}
#endif

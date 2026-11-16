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
    // Independent Unity 6 editor review of actual V17 GLBs.
    // Does not alter shipping scenes, gameplay, saved data or Build Settings.
    // An Editor source file is not proof of a successful Unity/mobile run.
    public static class CC0V17SceneBuilder
    {
        private const string Models = "Assets/Art_Reboot/CC0System/DirectorV17/";
        private const string Scenes = "Assets/Art_Reboot/CC0System/ScenesV17/";

        [MenuItem("Eldoria/CC0 Art System/V17 - Build five independent review scenes")]
        public static void BuildFiveV17Scenes()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Exit Play Mode before authoring.");

            Directory.CreateDirectory(Scenes);
            string screenshots = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "..", "docs", "cc0-art-system",
                "previews", "unity-v17"));
            Directory.CreateDirectory(screenshots);

            for (int stage = 1; stage <= 5; stage++)
            {
                string source = Models + "DirectorV17_Stage" + stage + ".glb";
                string diskFile = Path.GetFullPath(Path.Combine(Application.dataPath, "..", source));
                if (!File.Exists(diskFile))
                    throw new FileNotFoundException("Extract the pinned V17 GLB artifact into the Unity Assets folder first", diskFile);

                // The project includes com.unity.cloud.gltfast 6.14.1: its importer
                // produces native Unity prefab assets for files with a glb extension.
                AssetDatabase.ImportAsset(source, ImportAssetOptions.ForceSynchronousImport);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(source);
                if (prefab == null)
                    throw new InvalidOperationException("The actual V17 GLB did not import as a Unity prefab: " + source);

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                SceneManager.SetActiveScene(scene);
                GameObject city = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (city == null)
                    throw new InvalidOperationException("Cannot instantiate imported GLB prefab");
                city.name = "CC0 V17 real Valoria stage " + stage;
                city.transform.position = Vector3.zero;

                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = Hex("#96A5B1");
                RenderSettings.ambientEquatorColor = Hex("#737F8F");
                RenderSettings.ambientGroundColor = Hex("#414957");
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = 53f;
                RenderSettings.fogEndDistance = 113f;
                RenderSettings.fogColor = Hex("#768291");

                var sunObject = new GameObject("V17 dusk directional");
                var sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.color = Hex("#FFD0A1");
                sun.intensity = 1.55f;
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = 0.7f;
                sun.transform.rotation = Quaternion.Euler(57, -35, 0);
                RenderSettings.sun = sun;

                for (int index = 0; index < 2; index++)
                {
                    var obj = new GameObject("Gate torch point " + index);
                    obj.transform.position = new Vector3(index == 0 ? -1.7f : 1.7f, 3.0f, 6.3f);
                    Light light = obj.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = Hex("#FFC286");
                    light.intensity = 1.0f;
                    light.range = 5.5f;
                    light.shadows = LightShadows.None;
                }

                var cameraObject = new GameObject("Main Camera V17");
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 44.5f;
                camera.nearClipPlane = .25f;
                camera.farClipPlane = 240f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Hex("#768291");
                camera.allowHDR = false;
                camera.transform.position = new Vector3(43, 48, 61);
                camera.transform.LookAt(new Vector3(0, 5, 0));

                string destination = Scenes + "CC0_V17_Bastion" + stage + ".unity";
                if (!EditorSceneManager.SaveScene(scene, destination))
                    throw new InvalidOperationException("Cannot save isolated review scene: " + destination);

                if (stage == 1 || stage == 5)
                {
                    Capture(camera, screenshots, stage, 1600, 900, 44.5f, "wide");
                    Capture(camera, screenshots, stage, 390, 844, 36f, "portrait");
                }
                Debug.Log("V17_REAL_UNITY_SCENE_SAVED " + destination);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("V17_SCENES_CREATED -- NOT iPhone tested or art approved");
        }

        private static Color Hex(string value)
        {
            if (!ColorUtility.TryParseHtmlString(value, out var result))
                throw new FormatException("Color parse failed: " + value);
            return result;
        }

        private static void Capture(Camera cam, string directory, int stage,
                                    int width, int height, float ortho, string profile)
        {
            float originalSize = cam.orthographicSize;
            RenderTexture originalTarget = cam.targetTexture;
            RenderTexture originalActive = RenderTexture.active;
            RenderTexture target = null;
            Texture2D texture = null;
            try
            {
                cam.orthographicSize = ortho;
                target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                cam.targetTexture = target;
                cam.Render();
                RenderTexture.active = target;
                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                byte[] png = texture.EncodeToPNG();
                if (png == null || png.Length < 20000)
                    throw new InvalidOperationException("V17 Unity capture is blank");
                string path = Path.Combine(directory,
                    "REAL_V17_UNITY_" + stage + "_" + profile + "_" + width + "x" + height + ".png");
                File.WriteAllBytes(path, png);
                Debug.Log("V17_REAL_UNITY_EDITOR_CAPTURE " + path + " " + png.Length);
            }
            finally
            {
                cam.orthographicSize = originalSize;
                cam.targetTexture = originalTarget;
                RenderTexture.active = originalActive;
                if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
                if (target != null) RenderTexture.ReleaseTemporary(target);
            }
        }
    }
}
#endif

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using Eldoria.Experimentos.Ciudad2D;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.ValoriaSpatial25D.Editor
{
    /// <summary>
    /// Authentic RenderTexture evidence from the archived immutable A and the
    /// new B scene. Never writes or saves A/B, PlayerState or a production scene.
    /// A moving camera is rendered frame-by-frame: no static-photo pan compositing.
    /// </summary>
    public static class SpatialPilotNativeEvidence
    {
        private const string Root = "Assets/Experimentos/";
        private const string SceneA = Root +
            "ValoriaSpatial25D/Scenes/Valoria_Spatial_A_Control.unity";
        private const string SceneB = Root +
            "ValoriaSpatial25D/Scenes/Valoria_Spatial_B_Pilot.unity";
        private const string OutputDirectory = "ValoriaSpatialNativeEvidence";

        [Serializable]
        private sealed class Frame
        {
            public string file, variant, camera_step, sha256, scene_asset_path;
            public int width, height, stage, frame_number;
            public float simulated_seconds, zoom, focus_x, focus_y;
            public bool editor_render_texture, camera_moved_in_unity;
        }

        [Serializable]
        private sealed class Evidence
        {
            public string engine = "Unity 6.3 Editor RenderTexture";
            public string unity_version, generated_utc, control_source_scene, candidate_scene;
            public string test_type = "Editor capture, NOT PlayMode or hardware certification";
            public bool gameplay_modified = false;
            public bool author_approval = false;
            public bool physically_tested = false;
            public bool video_file_created = false;
            public string known_limitations =
                "Terrain/wall still baked in control; proxy workers are NOT final art; " +
                "PNG timeline requires encoder, not a video test; physical test pending";
            public Frame[] frames;
        }

        private struct Pose
        {
            public int stage, width, height;
            public float focusX, focusY, zoom, seconds;
            public string label;
            public Pose(int st,int w,int h,float x,float y,float z,float sec,string name)
            {
                stage=st;width=w;height=h;focusX=x;focusY=y;
                zoom=z;seconds=sec;label=name;
            }
        }

        [MenuItem("Eldoria/Experimentos/Valoria Spatial/Captura nativa A-B (sin tocar main)")]
        public static void Capture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Capture cannot run during PlayMode");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneA) == null ||
                AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneB) == null)
                throw new InvalidOperationException("Canonical A or isolated B is missing");
            var original = EditorSceneManager.GetSceneManagerSetup();
            if (!UnityEngine.Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            // All output is outside Unity/Assets so no import/postprocessor
            // can modify source assets. A and B are loaded read-only.
            string directory = Path.GetFullPath(
                Path.Combine(UnityEngine.Application.dataPath, "..", OutputDirectory));
            Directory.CreateDirectory(directory);
            var samples = new List<Frame>(110);
            try
            {
                foreach (var variant in new [] { "A", "B" })
                    RecordVariant(variant, directory, samples);
                var info = new Evidence
                {
                    unity_version = UnityEngine.Application.unityVersion,
                    generated_utc = DateTime.UtcNow.ToString("o"),
                    control_source_scene = SceneA,
                    candidate_scene = SceneB,
                    frames = samples.ToArray()
                };
                File.WriteAllText(Path.Combine(directory, "evidence.json"),
                    JsonUtility.ToJson(info, true) + "\n");
                Debug.Log("[ValoriaSpatial] Saved " + samples.Count +
                    " REAL Editor RenderTexture frames to " + directory +
                    " (NOT artist approved / NOT device-tested)");
            }
            finally
            {
                if (!UnityEngine.Application.isBatchMode && original != null && original.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(original);
            }
        }

        private static void RecordVariant(string variant, string directory,List<Frame> frames)
        {
            string source = variant == "A" ? SceneA : SceneB;
            string sourceOnDisk = Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath, "..", source));
            string originalSourceSha = Sha(File.ReadAllBytes(sourceOnDisk));
            Scene scene = EditorSceneManager.OpenScene(source, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.isLoaded ||
                scene.path != source)
                throw new InvalidOperationException("Scene must open unchanged: " + source);

            var camera = Camera.main;
            var fit = camera == null ? null : camera.GetComponent<City2DCameraFit>();
            var bridge = UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
            var pilot = UnityEngine.Object.FindFirstObjectByType<SpatialPilotBootstrap>();
            if (camera == null || !camera.orthographic || fit == null ||
                bridge == null || bridge.IsLiveBound || bridge.RegisteredBuildingCount != 6)
            {
                if (bridge != null) bridge.RebindSceneBuildings();
                if (bridge == null || bridge.IsLiveBound || bridge.RegisteredBuildingCount != 6)
                    throw new InvalidOperationException("Scene has no read-only 6-building bridge");
            }

            if ((variant == "A" && pilot != null) ||
                (variant == "B" && pilot == null))
                throw new InvalidOperationException("Contaminated A/B experiment");
            if (pilot != null)
            {
                pilot.EnsureBuilt();
                if (!pilot.IsReady || pilot.WorkerCount != 3 ||
                    pilot.ForegroundRegionCount != 5 ||
                    pilot.OcclusionTree == null ||
                    pilot.WorkerFor("aserradero") == null ||
                    pilot.WorkerFor("cuartel") == null ||
                    pilot.WorkerFor("granero") == null)
                    throw new InvalidOperationException("B spatial actors not bootstrapped");
            }
            var ids = new Dictionary<string, CityBuilding>(StringComparer.Ordinal);
            foreach (var item in UnityEngine.Object.FindObjectsByType<CityBuilding>(
                         FindObjectsSortMode.None))
                if (item.gameObject.scene == scene) ids.Add(item.BuildingId, item);
            if (ids.Count != 6 || !ids.ContainsKey("aserradero"))
                throw new InvalidOperationException("Six original building IDs required");
            Vector2 saw = ids["aserradero"].transform.position;
            var staticPoses = new [] {
                new Pose(1,390,844,0,0,1,0,"overview_I"),
                new Pose(3,390,844,0,0,1,0,"overview_III"),
                new Pose(5,390,844,0,0,1,0,"overview_V"),
                new Pose(5,1170,2532,0,0,1,0,"overview_V_retina"),
                new Pose(5,844,390,0,0,1,0,"overview_V_landscape"),
                new Pose(5,390,844,saw.x,saw.y,1.75f,4.2f,"aserradero_close"),
                new Pose(5,1170,2532,saw.x,saw.y,1.75f,6f,"aserradero_retina"),
                new Pose(5,390,844,saw.x,saw.y,1.75f,8f,"actor_tree_crossing")
            };
            foreach (var pose in staticPoses)
                CaptureOne(variant,source,scene,camera,fit,bridge,pilot,pose,
                    directory,frames,-1);

            // One frame of actual Unity render per camera state and time sample,
            // identical A/B camera path. B additionally advances actor sprite state.
            for (int step = 0; step < 36; ++step)
            {
                float progress = step/35f;
                float x=0, y=0, zoom=1f;
                string label;
                if(step<8)
                {
                    label="horizontal";
                    x=Mathf.Lerp(-1.5f,1.5f,step/7f);
                }
                else if(step<16)
                {
                    label="vertical";
                    y=Mathf.Lerp(-2f,2f,(step-8)/7f);
                }
                else if(step<24)
                {
                    label="sawmill_approach";
                    float v=(step-16)/7f;
                    x=Mathf.Lerp(0,saw.x,v);
                    y=Mathf.Lerp(0,saw.y,v);
                    zoom=Mathf.Lerp(1f,1.8f,v);
                }
                else if(step<30)
                {
                    label="occlusion_crossing";
                    x=saw.x;y=saw.y;zoom=1.8f;
                }
                else
                {
                    label="return_overview";
                    float v=(step-30)/5f;
                    x=Mathf.Lerp(saw.x,0,v);
                    y=Mathf.Lerp(saw.y,0,v);
                    zoom=Mathf.Lerp(1.8f,1f,v);
                }
                // 0.3s steps allow the woodcutter to pass before/behind the
                // independently sorted tree in a time-sampled Unity scene.
                var pose = new Pose(5,390,844,x,y,zoom,step*0.30f,label);
                CaptureOne(variant,source,scene,camera,fit,bridge,pilot,pose,
                    directory,frames,step);
            }
            if (Sha(File.ReadAllBytes(sourceOnDisk)) != originalSourceSha)
                throw new InvalidOperationException(
                    "A/B serialized scene bytes changed during capture: " + source);
        }

        private static void CaptureOne(string variant,string scenePath,Scene scene,
            Camera camera,City2DCameraFit fit,City2DStateBridge bridge,
            SpatialPilotBootstrap pilot,Pose pose,string folder,List<Frame> frames,int index)
        {
            bridge.SetDemoBastionLevel(pose.stage);
            foreach (var shadow in UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                         FindObjectsSortMode.None))
                if(shadow.gameObject.scene==scene)shadow.Refresh();
            if(pilot != null)
            {
                pilot.SampleAt(pose.seconds);
                int visible = 0;
                foreach(var id in new[]{"aserradero","cuartel","granero"})
                    if(pilot.WorkerFor(id).IsVisible)visible++;
                int expected = pose.stage >= 4 ? 3 : pose.stage >= 3 ? 2 :
                               pose.stage >= 2 ? 1 : 0;
                if(visible != expected)
                    throw new InvalidOperationException("Worker gating invalid at stage "+
                        pose.stage+": "+visible+" vs "+expected);
            }

            fit.SetEvidenceViewport(pose.width,pose.height);
            if(pose.zoom > 1f)fit.SetEvidenceZoom(pose.zoom);
            fit.SetEvidenceFocus(new Vector2(pose.focusX,pose.focusY));
            string file = index < 0
                ? variant+"_"+pose.label+"_"+pose.width+"x"+pose.height+".png"
                : "timeline_"+variant+"_"+index.ToString("D3")+".png";
            byte[] png=Render(camera,pose.width,pose.height);
            File.WriteAllBytes(Path.Combine(folder,file),png);
            frames.Add(new Frame{
                file=file,variant=variant,scene_asset_path=scenePath,
                camera_step=pose.label,width=pose.width,height=pose.height,
                stage=pose.stage,frame_number=index,zoom=pose.zoom,
                focus_x=pose.focusX,focus_y=pose.focusY,
                simulated_seconds=pose.seconds,
                camera_moved_in_unity=index>=0,editor_render_texture=true,
                sha256=Sha(png)
            });
        }

        private static byte[] Render(Camera cam,int width,int height)
        {
            var original=RenderTexture.active;
            var originalTarget=cam.targetTexture;
            RenderTexture temp=null;
            Texture2D readback=null;
            try
            {
                temp=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
                temp.Create();
                cam.targetTexture=temp;
                cam.Render();
                RenderTexture.active=temp;
                readback=new Texture2D(width,height,TextureFormat.RGBA32,false);
                readback.ReadPixels(new Rect(0,0,width,height),0,0);
                readback.Apply();
                return readback.EncodeToPNG();
            }
            finally
            {
                cam.targetTexture=originalTarget;
                RenderTexture.active=original;
                if(readback!=null)UnityEngine.Object.DestroyImmediate(readback);
                if(temp!=null){temp.Release();UnityEngine.Object.DestroyImmediate(temp);}
            }
        }

        private static string Sha(byte[] bytes)
        {
            using(var hash=SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(bytes))
                    .Replace("-","").ToLowerInvariant();
        }
    }
}
#endif

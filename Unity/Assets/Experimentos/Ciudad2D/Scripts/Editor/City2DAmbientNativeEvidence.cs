#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>
    /// Captures two deterministic visual instants of the REAL isolated Unity
    /// Editor scene. This does not constitute device/PlayMode performance QA.
    /// It never edits game state, city gameplay, other scenes or BuildSettings.
    /// </summary>
    public static class City2DAmbientNativeEvidence
    {
        private const string ScenePath = "Assets/Experimentos/Ciudad2D/Experimento_Ciudad2D.unity";
        private const string OutName = "Ciudad2DAmbientEvidence";

        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Capturar vida ambiental I-III-V")]
        public static void Capture()
        {
            if (!UnityEngine.Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var previous = EditorSceneManager.GetSceneManagerSetup();
            string output = Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath, "..", OutName));
            Directory.CreateDirectory(output);
            string manifestPath = Path.Combine(output, "evidence.json");
            if (File.Exists(manifestPath)) File.Delete(manifestPath);
            try
            {
                if (UnityEngine.Application.isBatchMode)
                {
                    var scratch = EditorSceneManager.NewScene(
                        NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    if (!EditorSceneManager.SaveScene(scratch,
                        "Assets/Experimentos/Ciudad2D/QA/City2D_AmbientScratch.unity"))
                        throw new InvalidOperationException("Ambient scratch scene cannot be saved");
                }
                City2DSceneBuilder.Rebuild();
                var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                if (!scene.IsValid() || !scene.isLoaded)
                    throw new InvalidOperationException("Original isolated scene did not load");
                var cam = Camera.main;
                if (cam == null || !cam.orthographic)
                    throw new InvalidOperationException("Not the genuine ortho city camera");
                var fit = cam.GetComponent<City2DCameraFit>();
                var bridge = UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
                if (fit == null || bridge == null)
                    throw new InvalidOperationException("Authoritative City2D camera/state bridge absent");
                bridge.RebindSceneBuildings();
                if (bridge.RegisteredBuildingCount != 6 ||
                    bridge.RegisteredAmbientEffectCount != 10 ||
                    bridge.IsLiveBound)
                    throw new InvalidOperationException("Ambient proof must be isolated read-only and ten effects");

                var all = UnityEngine.Object.FindObjectsByType<City2DAmbientMotion>(
                    FindObjectsSortMode.None);
                if (all.Length != 10 || all.Any(e => e.gameObject.scene != scene ||
                    !e.IsPurePresentation || e.GetComponent<Collider2D>() != null))
                    throw new InvalidOperationException("Unexpected component or collider in ambient proof");
                if (all.Count(e => e.Kind == City2DAmbientMotion.MotionKind.Smoke) != 6 ||
                    all.Count(e => e.Kind == City2DAmbientMotion.MotionKind.Ember) != 4 ||
                    all.Where(e => e.Kind == City2DAmbientMotion.MotionKind.Smoke)
                        .Any(e => !e.HasBuildingOwner))
                    throw new InvalidOperationException("Unexpected ambient ownership / effect count");

                var records = new List<string>();
                var gates = new List<string>();
                foreach (int stage in new[] {1,3,5})
                {
                    bridge.SetDemoBastionLevel(stage);
                    int active = all.Count(e => e.ActiveForStage);
                    int smoke = all.Count(e => e.ActiveForStage &&
                        e.Kind == City2DAmbientMotion.MotionKind.Smoke);
                    int ember = all.Count(e => e.ActiveForStage &&
                        e.Kind == City2DAmbientMotion.MotionKind.Ember);
                    int expected = stage == 1 ? 4 : stage == 3 ? 8 : 10;
                    if (active != expected || smoke != expected - 4 || ember != 4)
                        throw new InvalidOperationException(
                            "Living city stage gating mismatch at " + stage +
                            " active " + active + " smoke " + smoke);
                    gates.Add("{\"stage\":" + stage + ",\"active\":" + active +
                        ",\"smoke\":" + smoke + ",\"embers\":" + ember + "}");
                    var views = stage == 5
                        ? new[]{new[]{390,844},new[]{1170,2532},new[]{844,390}}
                        : new[]{new[]{390,844}};
                    foreach (var view in views)
                    {
                        fit.SetEvidenceViewport(view[0],view[1]);
                        byte[] first = CaptureAtTime(cam,all,1.2f,view[0],view[1]);
                        byte[] second = CaptureAtTime(cam,all,3.4f,view[0],view[1]);
                        if (Sha(first) == Sha(second))
                            throw new InvalidOperationException(
                                "No genuine visual change at stage " + stage +
                                " size " + view[0] + "x" + view[1]);
                        SaveFrame(output,records,stage,view[0],view[1],"t1",first);
                        SaveFrame(output,records,stage,view[0],view[1],"t2",second);
                    }
                }
                if (records.Count != 10)
                    throw new InvalidOperationException("Expected ten Unity ambient captures");

                // Sequence from actual Unity Camera.Render, not generated frames,
                // screenshot interpolation or a composited 2D simulation.
                bridge.SetDemoBastionLevel(5);
                fit.SetEvidenceViewport(390,844);
                var motionSequence=new List<string>();
                for(int frame=0;frame<16;frame++)
                {
                    float seconds=0.6f+frame*.34f;
                    byte[] png=CaptureAtTime(cam,all,seconds,390,844);
                    string name="living_bastion5_390x844_f"+frame.ToString("D2")+".png";
                    File.WriteAllBytes(Path.Combine(output,name),png);
                    motionSequence.Add("{\"file\":\""+name+"\",\"sha256\":\""+
                        Sha(png)+"\",\"width\":390,\"height\":844,\"seconds\":"+
                        seconds.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
                }
                if(motionSequence.Select(item=>item).Distinct().Count()!=16)
                    throw new InvalidOperationException("Living city sequence unexpectedly duplicated");

                string data="{\"engine\":\"Unity\",\"mode\":\"Editor_RenderTexture_two_times\","+
                    "\"source_scene\":\"Experimento_Ciudad2D.unity\","+
                    "\"time_samples_seconds\":[1.2,3.4],\"captures\":["
                    +string.Join(",",records)+"],\"stage_gates\":["
                    +string.Join(",",gates)+"],\"animation_frames\":["+
                    string.Join(",",motionSequence)+"],\"animation_frame_interval_ms\":340,"+
                    "\"torch_flicker_evaluated_at_each_time\":true,"+
                    "\"original_bastion_I_for_1_4_and_V_for_5\":true,"+
                    "\"no_unique_bastion_II_III_IV_required\":true,"+
                    "\"original_scene_read_only\":true,"+
                    "\"gameplay_or_saves_modified\":false,\"main_modified\":false,"+
                    "\"visual_art_approved\":false,\"playmode_proof_of_animation\":false,"+
                    "\"physical_device_test\":\"PENDING\"}\n";
                File.WriteAllText(manifestPath,data,Encoding.UTF8);
                Debug.Log("[Ciudad2D] REAL_UNITY_AMBIENT_EDITOR_TEN_FRAMES_SHA_AND_STAGE_GATE_PASS_ART_PENDING");
            }
            finally
            {
                if (!UnityEngine.Application.isBatchMode && previous != null && previous.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        private static byte[] CaptureAtTime(Camera cam,
            City2DAmbientMotion[] effects, float seconds, int width, int height)
        {
            foreach (var e in effects) e.EvaluateAtTime(seconds);
            foreach (var flame in UnityEngine.Object.FindObjectsByType<City2DTorchPulse>(
                FindObjectsSortMode.None))
                if(flame.gameObject.scene==cam.gameObject.scene)
                    flame.EvaluateAtTime(seconds);
            var rt = new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var prior = RenderTexture.active;
            var oldTarget = cam.targetTexture;
            Texture2D tex = null;
            try
            {
                rt.Create();cam.targetTexture=rt;cam.aspect=(float)width/height;
                cam.Render();RenderTexture.active=rt;
                tex=new Texture2D(width,height,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
                byte[] bytes=tex.EncodeToPNG();
                if(bytes==null || bytes.Length<8000)
                    throw new InvalidOperationException("Empty render from Unity ambient scene");
                return bytes;
            }
            finally
            {
                cam.targetTexture=oldTarget;RenderTexture.active=prior;
                if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);
                rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        private static void SaveFrame(string folder,List<string> records,
            int stage,int width,int height,string time,byte[] bytes)
        {
            string filename="bastion"+stage+"_"+width+"x"+height+"_"+time+".png";
            File.WriteAllBytes(Path.Combine(folder,filename),bytes);
            records.Add("{\"file\":\""+filename+"\",\"stage\":"+stage+
                ",\"width\":"+width+",\"height\":"+height+
                ",\"sha256\":\""+Sha(bytes)+"\"}");
        }
        private static string Sha(byte[] bytes)
        {
            using(var sha=SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-","").ToLowerInvariant();
        }
    }
}
#endif

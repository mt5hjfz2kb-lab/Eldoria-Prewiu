#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>
    /// Source-authentic A/B motion proof. A is loaded from the immutable archived
    /// Premium V6 Unity scene. B is a distinct copy; neither original A nor any
    /// functional integration scene, save, PlayerState or main files are written.
    /// A real Unity RenderTexture produces EVERY evidence frame.
    /// Static preflights must not be misreported as an executed Unity comparison.
    /// </summary>
    public static class City2DDepthPilotEvidence
    {
        public const string Root = "Assets/Experimentos/Ciudad2D/";
        public const string SourceScene =
            Root+"QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity";
        public const string CandidateScene =
            Root+"QA/DepthPilot/Experimento_Ciudad2D_V6_Depth_B.unity";
        public const string SourceSha256 =
            "aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06";
        private const string Output = "Ciudad2DDepthPilotEvidence";
        private const int MobileFps = 8;
        private const int FramesPerPhase = 8;

        private sealed class CameraPose
        {
            public Vector3 position;
            public float ortho;
        }

        private sealed class CaptureRow
        {
            public string file, sha, variant, phase;
            public int width, height, frame;
            public float seconds, ortho, x, y;
        }

        [Serializable] private sealed class OutputManifest
        {
            public string engine="Unity";
            public string mode="Editor_RenderTexture";
            public string original_scene=SourceScene;
            public string immutable_original_sha256=SourceSha256;
            public string candidate_scene=CandidateScene;
            public string source_branch="art/ciudad-2d-hitos-1-5-20261116";
            public string scenario="Bastion_V_Sawmill_built_Quarry_available";
            public string source_commit="10dbc197d48ea8517c6a3dd6c22f1a2abf75d508";
            public string artistic_approval="PENDING";
            public string iphone_physical="NOT_TESTED";
            public string webgl="NOT_TESTED";
            public bool main_modified=false;
            public bool gameplay_modified=false;
            public bool scene_A_was_not_overwritten;
            public bool camera_poses_equal;
            public bool alpha_check_requires_human_review=true;
            public int mobile_fps=MobileFps;
            public CaptureOutput[] captures;
        }

        [Serializable] private sealed class CaptureOutput
        {
            public string file, sha256, variant, phase;
            public int width, height, frame;
            public float seconds, camera_x, camera_y, orthographic_size;
        }

        [MenuItem("Eldoria/Experimentos/Ciudad2D/Profundidad - piloto A-B Unity en movimiento")]
        public static void Capture()
        {
            string originalPath=Physical(SourceScene);
            if (!File.Exists(originalPath) ||
                !string.Equals(Sha(File.ReadAllBytes(originalPath)), SourceSha256,
                    StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "Premium V6 scene SHA mismatch. Control A must remain exactly immutable.");

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(CandidateScene)!=null ||
                File.Exists(Physical(CandidateScene)))
                throw new InvalidOperationException(
                    "Pilot B already exists. Never overwrite a review candidate; archive it first.");

            var setup=EditorSceneManager.GetSceneManagerSetup();
            if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            string outDir=Path.GetFullPath(Path.Combine(Application.dataPath,"..",Output));
            if (Directory.Exists(outDir))
                throw new InvalidOperationException("Existing A/B evidence. Never mix runs.");
            Directory.CreateDirectory(outDir);
            try
            {
                PrepareB();
                var expectedPoses=new Dictionary<string,CameraPose>(StringComparer.Ordinal);
                var frames=new List<CaptureRow>();
                RenderVariant("A",SourceScene,outDir,frames,expectedPoses,false);
                RenderVariant("B",CandidateScene,outDir,frames,expectedPoses,true);
                if (frames.Count != 2*(24+9))
                    throw new InvalidOperationException("A/B frame count mismatch");
                var records=new CaptureOutput[frames.Count];
                for(int i=0;i<frames.Count;i++)
                {
                    var f=frames[i];
                    records[i]=new CaptureOutput
                    {
                        file=f.file,sha256=f.sha,variant=f.variant,phase=f.phase,
                        width=f.width,height=f.height,frame=f.frame,
                        seconds=f.seconds,camera_x=f.x,camera_y=f.y,orthographic_size=f.ortho
                    };
                }
                var manifest=new OutputManifest
                {
                    captures=records,
                    camera_poses_equal=true,
                    scene_A_was_not_overwritten=
                        Sha(File.ReadAllBytes(originalPath))==SourceSha256
                };
                if(!manifest.scene_A_was_not_overwritten)
                    throw new InvalidOperationException("Immutable scene A was altered");
                File.WriteAllText(Path.Combine(outDir,"evidence.json"),
                    JsonUtility.ToJson(manifest,true),new UTF8Encoding(false));
                Debug.Log("[ValoriaDepth] TRUE_UNITY_MOTION_AB_FRAMES_RECORDED_" +
                          frames.Count+"_NOT_ARTISTICALLY_APPROVED");
            }
            finally
            {
                if (!Application.isBatchMode && setup!=null && setup.Length>0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }

        private static void PrepareB()
        {
            string directory=Path.GetDirectoryName(CandidateScene).Replace('\\','/');
            if(!AssetDatabase.IsValidFolder(directory))
            {
                var root=Root+"QA";
                if(!AssetDatabase.IsValidFolder(root))
                    throw new InvalidOperationException("City2D QA directory missing");
                AssetDatabase.CreateFolder(root,"DepthPilot");
            }
            var control=EditorSceneManager.OpenScene(SourceScene,OpenSceneMode.Single);
            if(!control.IsValid() || !control.isLoaded)
                throw new InvalidOperationException("Could not load original Unity scene");
            if(!EditorSceneManager.SaveScene(control,CandidateScene,true))
                throw new InvalidOperationException("Could not create separate B scene");
            var b=EditorSceneManager.OpenScene(CandidateScene,OpenSceneMode.Single);
            var cam=Camera.main;
            var groundObj=GameObject.Find("Terreno_Valoria");
            var ground=groundObj!=null?groundObj.GetComponent<SpriteRenderer>():null;
            if(cam==null || !cam.orthographic || ground==null || ground.sprite==null)
                throw new InvalidOperationException("Original PremiumV6 camera/ground missing");
            var marker=new GameObject("DEPTH_PILOT_B_UNAPPROVED_VISUAL_ONLY");
            var sky=new GameObject("Profundidad_CieloLejano_NoEdificios");
            sky.AddComponent<SpriteRenderer>();
            sky.AddComponent<City2DDepthSkyMotion>().Configure(cam,ground);
            if (sky.GetComponent<Collider2D>()!=null)
                throw new InvalidOperationException("Parallax layer must not block input");

            var workshop=FindBuilding("aserradero");
            var bastion=FindBuilding("bastion");
            if(workshop==null || bastion==null)
                throw new InvalidOperationException("Bastion and workshop must remain playable");
            // Existing artwork: no duplicate car or invented low-quality human sprite.
            AddExistingGroundedMotion("Atrezzo_decor_04",workshop,4.4f,.45f,.035f,.12f);
            AddExistingGroundedMotion("Atrezzo_decor_00",bastion,5.2f,.38f,.004f,.33f);
            if(!EditorSceneManager.SaveScene(b,CandidateScene))
                throw new InvalidOperationException("Could not save isolated candidate B");
            if(marker==null) throw new InvalidOperationException("Candidate marker unexpectedly missing");
        }

        private static CityBuilding FindBuilding(string id)
        {
            foreach(var b in UnityEngine.Object.FindObjectsByType<CityBuilding>(
                FindObjectsSortMode.None))
                if(b.BuildingId==id && b.gameObject.scene==SceneManager.GetActiveScene()) return b;
            return null;
        }

        private static void AddExistingGroundedMotion(
            string name, CityBuilding owner, float period, float angle,
            float travel, float phase)
        {
            var o=GameObject.Find(name);
            if(o==null || o.GetComponent<SpriteRenderer>()==null ||
               o.GetComponent<Collider2D>()!=null)
                throw new InvalidOperationException("Original noninteractive prop absent: "+name);
            o.AddComponent<City2DGroundedActivity>().Configure(
                owner,period,angle,travel,phase);
        }

        private static void RenderVariant(string variant,string scenePath,string output,
            List<CaptureRow> rows, Dictionary<string,CameraPose> reference, bool candidate)
        {
            var scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
            if(!scene.IsValid() || !scene.isLoaded)
                throw new InvalidOperationException("Cannot open A/B scene");
            var bridge=UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
            var cam=Camera.main;
            var fit=cam!=null?cam.GetComponent<City2DCameraFit>():null;
            if(bridge==null || bridge.IsLiveBound || fit==null ||
               !cam.orthographic || FindBuilding("bastion")==null)
                throw new InvalidOperationException("A/B not a valid read-only City2D scene");
            bridge.RebindSceneBuildings();
            bridge.SetDemoBastionLevel(5); // QA state, NOT saved, no gateway commands.
            foreach(var shadow in UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                FindObjectsSortMode.None))
                shadow.Refresh();
            var sky=UnityEngine.Object.FindFirstObjectByType<City2DDepthSkyMotion>();
            if(candidate != (sky!=null))
                throw new InvalidOperationException("Control A and pilot B have leaked components");
            foreach(var size in new[]{new[]{390,844},new[]{1170,2532}})
            {
                int w=size[0],h=size[1];
                fit.SetEvidenceViewport(w,h);
                for(int phase=0;phase<3;phase++)
                {
                    string phaseName=phase==0?"pan_xy":phase==1?"zoom_in":"zoom_out";
                    for(int f=0;f<FramesPerPhase;f++)
                    {
                        // Retina captures are deterministic samples from the SAME route.
                        if(w==1170 && f%3!=0) continue;
                        float t=(float)f/(FramesPerPhase-1);
                        float zoom;
                        Vector2 focus;
                        if(phase==0)
                        {
                            zoom=1.38f;
                            focus=new Vector2(Mathf.Lerp(-.65f,.65f,t),
                                              Mathf.Lerp(-.60f,.60f,t));
                        }
                        else if(phase==1)
                        {
                            zoom=Mathf.Lerp(1.38f,1.82f,t);
                            focus=new Vector2(Mathf.Lerp(.10f,-.70f,t),
                                              Mathf.Lerp(.10f,-.65f,t));
                        }
                        else
                        {
                            zoom=Mathf.Lerp(1.82f,1f,t);
                            focus=Vector2.Lerp(new Vector2(-.70f,-.65f),Vector2.zero,t);
                        }
                        fit.SetEvidenceZoom(zoom);
                        fit.SetEvidenceFocus(focus);
                        float seconds=(phase*FramesPerPhase+f)/(float)MobileFps;
                        foreach(var effect in UnityEngine.Object.FindObjectsByType<City2DAmbientMotion>(
                            FindObjectsSortMode.None)) effect.EvaluateAtTime(seconds);
                        foreach(var actor in UnityEngine.Object.FindObjectsByType<City2DGroundedActivity>(
                            FindObjectsSortMode.None)) actor.EvaluateAtTime(seconds);
                        if(sky!=null) sky.EvaluateForCamera(cam.transform.position);

                        string poseKey=w+"x"+h+"/"+phaseName+"/"+f;
                        var position=cam.transform.position;
                        if(!candidate)
                            reference.Add(poseKey,new CameraPose{
                                position=position,ortho=cam.orthographicSize});
                        else if(!reference.TryGetValue(poseKey,out var expected) ||
                            Vector3.Distance(expected.position,position)>0.0001f ||
                            Mathf.Abs(expected.ortho-cam.orthographicSize)>0.0001f)
                            throw new InvalidOperationException(
                                "Camera pose mismatch. A/B comparison is invalid at "+poseKey);

                        string relative=variant+"/"+w+"x"+h+"/"+phaseName+
                                        "/frame_"+f.ToString("D3",CultureInfo.InvariantCulture)+".png";
                        string absolute=Path.Combine(output,relative.Replace('/',Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(Path.GetDirectoryName(absolute));
                        byte[] image=Render(cam,w,h);
                        File.WriteAllBytes(absolute,image);
                        rows.Add(new CaptureRow
                        {
                            file=relative,sha=Sha(image),variant=variant,phase=phaseName,
                            width=w,height=h,frame=f,seconds=seconds,
                            ortho=cam.orthographicSize,x=position.x,y=position.y
                        });
                    }
                }
            }
        }

        private static byte[] Render(Camera camera,int width,int height)
        {
            var previous=RenderTexture.active;
            var oldTarget=camera.targetTexture;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            Texture2D tex=null;
            try
            {
                rt.Create();
                camera.targetTexture=rt;
                camera.aspect=(float)width/height;
                camera.Render();
                RenderTexture.active=rt;
                tex=new Texture2D(width,height,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,width,height),0,0);
                tex.Apply(false,false);
                var bytes=tex.EncodeToPNG();
                if(bytes==null || bytes.Length<8000)
                    throw new InvalidOperationException("Unity produced an empty A/B frame");
                return bytes;
            }
            finally
            {
                camera.targetTexture=oldTarget;
                RenderTexture.active=previous;
                if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        private static string Physical(string asset)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath,"..",
                asset.Replace('/',Path.DirectorySeparatorChar)));
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

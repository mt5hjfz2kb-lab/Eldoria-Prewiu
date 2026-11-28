#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.StylizedDirectionPilot.Editor
{
    /// <summary>
    /// Genuine Unity camera + experimental GPU illustrated finish.
    /// NO 3D replacement, no style-broken texture import, no gameplay scene save.
    /// A/B captured at identical positions and projection per resolution.
    /// This is an Editor visual prototype; native mobile compatibility remains untested.
    /// </summary>
    public static class GraphicFinishNativeCapture
    {
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string ShaderName="Hidden/Eldoria/StylizedDirectionPilot/GraphicFinish";
        [Serializable] sealed class Report
        {
            public string kind,sourceSha,scene,shader,notes;
            public int wideWidth,wideHeight,phoneWidth,phoneHeight;
            public float painterlyWideDelta,illustratedWideDelta,painterlyPhoneDelta,illustratedPhoneDelta;
            public bool originalSceneAndMaterialsUntouched,focusedBastionIsActualMesh,artApproved,mobileTested;
        }

        public static void Capture()
        {
            var output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../StylizedDirectionPilotCaptures"));
            Directory.CreateDirectory(output);
            Material material=null;
            try
            {
                if(!AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene))
                    throw new InvalidOperationException("The current real city scene is missing");
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>(FindObjectsInactive.Include);
                if(!city||!city.PlayerCamera)
                    throw new InvalidOperationException("The real player camera is missing");
                city.Apply(new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
                var shader=Shader.Find(ShaderName);
                if(!shader||!shader.isSupported)
                    throw new InvalidOperationException("Original-geometry graphic finish shader unavailable/unsupported");
                material=new Material(shader){name="EDITOR_ONLY_GRAPHIC_FINISH_DO_NOT_SHIP"};

                var rawWide=Path.Combine(output,"A-original-city-1600x900.png");
                var rawPhone=Path.Combine(output,"A-original-city-390x844.png");
                var paintedWide=Path.Combine(output,"G1-painted-original-1600x900.png");
                var paintedPhone=Path.Combine(output,"G1-painted-original-390x844.png");
                var illustratedWide=Path.Combine(output,"G2-illustrated-original-1600x900.png");
                var illustratedPhone=Path.Combine(output,"G2-illustrated-original-390x844.png");

                Save(city.PlayerCamera,rawWide,1600,900,null);
                Save(city.PlayerCamera,rawPhone,390,844,null);
                material.SetFloat("_StyleStrength",.55f);
                material.SetFloat("_OutlineStrength",.18f);
                material.SetFloat("_Bands",18f);
                Save(city.PlayerCamera,paintedWide,1600,900,material);
                Save(city.PlayerCamera,paintedPhone,390,844,material);
                material.SetFloat("_StyleStrength",.85f);
                material.SetFloat("_OutlineStrength",.36f);
                material.SetFloat("_Bands",10f);
                Save(city.PlayerCamera,illustratedWide,1600,900,material);
                Save(city.PlayerCamera,illustratedPhone,390,844,material);
                // New art-director framing is explicitly a DIFFERENT camera
                // candidate, not a false pixel-matched material comparison.
                SaveFocused(city.PlayerCamera,
                    Path.Combine(output,"G3-focused-illustrated-1600x900.png"),
                    1600,900,material,.79f,new Vector2(.52f,.59f));
                SaveFocused(city.PlayerCamera,
                    Path.Combine(output,"G3-focused-illustrated-390x844.png"),
                    390,844,material,.97f,new Vector2(.50f,.62f));

                var report=new Report{
                    kind="TRUE_UNITY_CAMERA_EDITOR_POST_FINISH_STUDY_NOT_PLAYABLE",
                    sourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA")??"manual-source",
                    scene=Scene,shader=ShaderName,
                    notes="A and G1/G2 share the exact geometry, source materials, light, camera and PlayerState. Only temporary Graphics.Blit differs. G1/G2 are illustrative candidates without runtime integration.",
                    wideWidth=1600,wideHeight=900,phoneWidth=390,phoneHeight=844,
                    painterlyWideDelta=Difference(rawWide,paintedWide),
                    illustratedWideDelta=Difference(rawWide,illustratedWide),
                    painterlyPhoneDelta=Difference(rawPhone,paintedPhone),
                    illustratedPhoneDelta=Difference(rawPhone,illustratedPhone),
                    originalSceneAndMaterialsUntouched=true,focusedBastionIsActualMesh=true,artApproved=false,mobileTested=false
                };
                if(report.painterlyWideDelta<2f || report.illustratedWideDelta<2f ||
                   report.painterlyPhoneDelta<2f || report.illustratedPhoneDelta<2f)
                    throw new InvalidOperationException("GPU illustration is too subtle to evaluate");
                File.WriteAllText(Path.Combine(output,"graphic-finish-evidence.json"),JsonUtility.ToJson(report,true));
                Debug.Log("ELDORIA_GRAPHIC_FINISH_REAL_UNITY_CAPTURE_PASS");
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                File.WriteAllText(Path.Combine(output,"graphic-finish-error.txt"),e.ToString());
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
            finally {if(material)Object.DestroyImmediate(material);}
        }

        static void Save(Camera cam,string path,int w,int h,Material finish)
        {
            var initial=RenderTexture.active;
            var sourceTarget=cam.targetTexture;
            float savedAspect=cam.aspect,savedSize=cam.orthographicSize;
            bool enabled=cam.enabled;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var dest=finish?new RenderTexture(w,h,0,RenderTextureFormat.ARGB32):null;
            var texture=new Texture2D(w,h,TextureFormat.RGB24,false,false);
            try
            {
                cam.enabled=true;
                cam.aspect=w/(float)h;
                if(h>w)cam.orthographicSize=savedSize*1.18f;
                cam.targetTexture=rt;
                cam.Render();
                if(finish)Graphics.Blit(rt,dest,finish);
                RenderTexture.active=finish?dest:rt;
                texture.ReadPixels(new Rect(0,0,w,h),0,0);
                texture.Apply();
                var png=texture.EncodeToPNG();
                if(png.Length<30000)throw new InvalidOperationException("Camera image empty "+path);
                File.WriteAllBytes(path,png);
            }
            finally
            {
                cam.targetTexture=sourceTarget;
                cam.aspect=savedAspect;
                cam.orthographicSize=savedSize;
                cam.enabled=enabled;
                RenderTexture.active=initial;
                rt.Release();Object.DestroyImmediate(rt);
                if(dest){dest.Release();Object.DestroyImmediate(dest);}
                Object.DestroyImmediate(texture);
            }
        }

        // Reframe around the actual Bastion renderer bounds; never teleport
        // gameplay objects, modify their colliders, or alter scene assets.
        static Vector3 BastionAnchor()
        {
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(!t||t.name!="Bastion_I_Grounding")continue;
                var renderers=t.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length==0)continue;
                var bounds=renderers[0].bounds;
                for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
                return bounds.center;
            }
            throw new InvalidOperationException("Cannot validate a focused city view without the actual Bastion mesh");
        }

        static void SaveFocused(Camera cam,string path,int width,int height,
            Material finish,float zoom,Vector2 desiredViewport)
        {
            var oldPosition=cam.transform.position;
            var oldRotation=cam.transform.rotation;
            var oldSize=cam.orthographicSize;
            var oldAspect=cam.aspect;
            try
            {
                cam.aspect=width/(float)height;
                // Reproduce the portrait viewport sizing used by Save().
                float renderSize=oldSize*zoom*(height>width?1.18f:1f);
                cam.orthographicSize=renderSize;
                var bastion=BastionAnchor();
                var projected=cam.WorldToViewportPoint(bastion);
                var xShift=(projected.x-desiredViewport.x)*2f*renderSize*cam.aspect;
                var yShift=(projected.y-desiredViewport.y)*2f*renderSize;
                cam.transform.position+=cam.transform.right*xShift+cam.transform.up*yShift;
                cam.orthographicSize=oldSize*zoom;
                Save(cam,path,width,height,finish);
            }
            finally
            {
                cam.transform.SetPositionAndRotation(oldPosition,oldRotation);
                cam.orthographicSize=oldSize;
                cam.aspect=oldAspect;
            }
        }

        static float Difference(string raw,string candidate)
        {
            var a=new Texture2D(2,2,TextureFormat.RGB24,false);
            var b=new Texture2D(2,2,TextureFormat.RGB24,false);
            try
            {
                if(!a.LoadImage(File.ReadAllBytes(raw))||!b.LoadImage(File.ReadAllBytes(candidate))||
                   a.width!=b.width||a.height!=b.height)
                    throw new InvalidOperationException("Native graphic finish dimensions invalid");
                var x=a.GetPixels32();var y=b.GetPixels32();double total=0;int count=0;
                for(int i=0;i<x.Length;i+=17)
                {
                    total+=(Math.Abs((int)x[i].r-y[i].r)+Math.Abs((int)x[i].g-y[i].g)+Math.Abs((int)x[i].b-y[i].b))/3.0;
                    count++;
                }
                return (float)(total/Math.Max(count,1));
            }
            finally{Object.DestroyImmediate(a);Object.DestroyImmediate(b);}
        }
    }
}
#endif

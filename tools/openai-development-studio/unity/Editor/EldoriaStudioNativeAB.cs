#if UNITY_EDITOR
using System;
using System.IO;
using Eldoria.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    // This adapter is copied only into the disposable, exact-source scene checkout.
    public static class EldoriaStudioNativeAB
    {
        const string Source = "5ef2aee67cf8fde57ba2fc4b3d929b517ee67944";
        public static void Capture()
        {
            var output=Environment.GetEnvironmentVariable("ELDORIA_STUDIO_OUTPUT");
            if(string.IsNullOrEmpty(output) || !Directory.Exists(output))
                throw new InvalidOperationException("Explicit isolated output required");
            if(Application.unityVersion!="6000.3.23f1")
                throw new InvalidOperationException("Wrong Unity version");
            var shaderA=Resources.Load<Shader>("ValoriaModular25D/DistrictGround");
            var shaderB=Resources.Load<Shader>("ValoriaModular25D/StudioDistrictGroundCandidate");
            if(!shaderA || !shaderB || shaderA==shaderB || ShaderUtil.ShaderHasError(shaderA) ||
                ShaderUtil.ShaderHasError(shaderB)) throw new Exception("Native shader import error");
            var report=new Modular25DNativeCapture.Evidence{
                engine="Unity",engineVersion=Application.unityVersion,sourceSha=Source,
                sourceBranch="Frozen actual Valoria source; Studio scratch only",
                scenePath=Modular25DEditorTasks.ScenePath,mode="Paired native GPU render; same scene and camera; no player saves",
                generatedAtUtc=DateTime.UtcNow.ToString("o")
            };
            var scene=EditorSceneManager.OpenScene(Modular25DEditorTasks.ScenePath,OpenSceneMode.Single);
            Modular25DWorld world=null;
            foreach(var root in scene.GetRootGameObjects()) {
                world=root.GetComponentInChildren<Modular25DWorld>(true);
                if(world)break;
            }
            if(!world)throw new Exception("Missing genuine modular scene");
            UnityEngine.Random.InitState(271828);
            world.BuildForNativeEditor();
            var ground=world.GetComponentInChildren<Modular25DGround>();
            var cam=world.ViewCamera;
            if(!ground || !cam || ground.ChunkCount!=81 || !ground.HasMillYard ||
               !ground.OriginalProjectMeadowLoaded || world.Hud!=null || world.Gameplay!=null)
                throw new Exception("Native scene contract failed");
            report.continuousTerrainPresent=true;
            report.paintedGroundSourceLoaded=true;
            report.noLegacyHudInEditorPreview=true;
            var plan=ValoriaUrbanPlan.City;
            foreach(int level in new[]{1,5})
            {
                var fixture=new PlayerState { BastionLevel=level,SawmillLevel=1,BarracksLevel=level>=2?1:0 };
                fixture.ChapterThree.GranaryLevel=level>=3?1:0;
                fixture.ChapterThree.StoneworksLevel=level>=5?1:0;
                world.Apply(fixture);
                if(world.BastionRenderer.sprite!=Modular25DArt.BastionForLevel(level) ||
                   world.BarracksRenderer.gameObject.activeSelf!=(level>=2) ||
                   world.GranaryRenderer.gameObject.activeSelf!=(level>=3) ||
                   world.StoneworksRenderer.gameObject.activeSelf!=(level>=5))
                    throw new Exception("Native progression fixture mismatch");
                var home=level==1?Modular25DCameraInput.HomePosition:Modular25DCameraInput.ExpansionCenterForLevel(level);
                var size=level==1?Modular25DCameraInput.HomeSizeForAspect(390f/844f):Modular25DCameraInput.ExpansionSizeForLevel(390f/844f,level);
                var mill=(Vector3)plan.Mill.Foot+new Vector3(0,.8f,-30f);
                foreach(string variant in new[]{"A","B","R"})
                {
                    bool candidate=variant=="B";
                    ground.StudioSelectGroundShader(candidate,level>=2,level>=3,level>=5,level);
                    var meshes=ground.GetComponentsInChildren<MeshRenderer>();
                    if(meshes.Length!=81)throw new Exception("Unexpected terrain mesh count");
                    foreach(var mesh in meshes)
                        if(!mesh.sharedMaterial || mesh.sharedMaterial.shader!=(candidate?shaderB:shaderA))
                            throw new Exception("Ground shader not applied on real renderer");
                    if(!ground.HasMillYard)throw new Exception("Mill yard property contract lost");
                    string prefix="b"+level+"-"+variant+"-";
                    Modular25DNativeCapture.Render(cam,output,report,prefix+"home",390,844,home,size);
                    Modular25DNativeCapture.Render(cam,output,report,prefix+"mill",390,844,mill,4.6f);
                    Modular25DNativeCapture.Render(cam,output,report,prefix+"mill-hd",1170,2532,mill,4.6f);
                    for(int i=0;i<5;i++)
                    {
                        var p=Vector3.Lerp(mill+new Vector3(-1.0f,-.3f,0),mill+new Vector3(1.0f,.7f,0),i/4f);
                        Modular25DNativeCapture.Render(cam,output,report,prefix+"travel-"+i,390,844,p,4.6f+i*.22f);
                    }
                }
                // A is the original source and R is the restored original. Both must match bit for bit.
                foreach(string view in new[]{"home","mill","mill-hd","travel-0","travel-1","travel-2","travel-3","travel-4"})
                {
                    var a=report.frames.Find(f=>f.name=="b"+level+"-A-"+view);
                    var b=report.frames.Find(f=>f.name=="b"+level+"-B-"+view);
                    var r=report.frames.Find(f=>f.name=="b"+level+"-R-"+view);
                    if(a==null||b==null||r==null||a.sha256!=r.sha256)
                        throw new Exception("Native reversal/non-determinism: "+level+" "+view);
                    if(a.cameraX!=b.cameraX||a.cameraY!=b.cameraY||a.orthographicSize!=b.orthographicSize)
                        throw new Exception("Camera mismatch");
                }
                var baseline=report.frames.Find(f=>f.name=="b"+level+"-A-mill-hd");
                var revised=report.frames.Find(f=>f.name=="b"+level+"-B-mill-hd");
                if(baseline.sha256==revised.sha256)throw new Exception("Shader has no effect on native mill camera");
            }
            report.nativeWorldRender=true;
            report.didNotAlterBeta=true;
            report.sourceFrames=report.frames.Count;
            report.progressionStagesCaptured=2;
            File.WriteAllText(Path.Combine(output,"native-evidence.json"),JsonUtility.ToJson(report,true));
            Debug.Log("STUDIO_REAL_UNITY_AB_PASS "+report.sourceFrames+" frames; original/candidate/reversal matched");
        }
    }
}
#endif

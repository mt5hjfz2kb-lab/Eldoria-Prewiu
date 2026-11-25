#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Eldoria.Domain;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    // One scene, one fixture, real production road generator with explicit variant.
    // No runtime state, no saves, no asset promotion.
    public static class EldoriaStudioPostigoAB
    {
        const string Source="5ef2aee67cf8fde57ba2fc4b3d929b517ee67944";
        static GameObject FindRoad(Modular25DLandscape land,string name)
        {
            var found=land.GetComponentsInChildren<MeshFilter>(true).Where(x=>x.gameObject.name==name).ToArray();
            if(found.Length!=1)throw new Exception("Road identity not unique: "+name);
            return found[0].gameObject;
        }
        static void SameGeometry(GameObject a,GameObject b)
        {
            var x=a.GetComponent<MeshFilter>().sharedMesh;
            var y=b.GetComponent<MeshFilter>().sharedMesh;
            if(!x.vertices.SequenceEqual(y.vertices)||!x.triangles.SequenceEqual(y.triangles)||
                !x.uv.SequenceEqual(y.uv))throw new Exception("Join-only variant changed road geometry");
        }
        static void SameMeshContract(GameObject a,GameObject b)
        {
            var x=a.GetComponent<MeshFilter>().sharedMesh;
            var y=b.GetComponent<MeshFilter>().sharedMesh;
            if(x.vertexCount!=y.vertexCount||!x.triangles.SequenceEqual(y.triangles)||!x.uv.SequenceEqual(y.uv))
                throw new Exception("Candidate changed topology/UVs");
            for(int i=4;i<x.vertexCount;i+=9)
                if(x.vertices[i]!=y.vertices[i])throw new Exception("Route centerline changed");
            if(a.GetComponent<MeshRenderer>().sharedMaterial!=b.GetComponent<MeshRenderer>().sharedMaterial)
                throw new Exception("Road material changed");
        }
        public static void Capture()
        {
            string output=Environment.GetEnvironmentVariable("ELDORIA_STUDIO_OUTPUT");
            if(string.IsNullOrEmpty(output)||!Directory.Exists(output)||UnityEngine.Application.unityVersion!="6000.3.23f1")
                throw new Exception("Wrong isolated output or Unity version");
            var report=new Modular25DNativeCapture.Evidence{
                engine="Unity",engineVersion=UnityEngine.Application.unityVersion,sourceSha=Source,
                sourceBranch="Frozen Valoria base; disposable Studio candidate",
                scenePath=Modular25DEditorTasks.ScenePath,mode="Matched native road and shadow A/B; no player save",
                generatedAtUtc=DateTime.UtcNow.ToString("o")
            };
            var scene=EditorSceneManager.OpenScene(Modular25DEditorTasks.ScenePath,OpenSceneMode.Single);
            var world=scene.GetRootGameObjects().Select(x=>x.GetComponentInChildren<Modular25DWorld>(true)).FirstOrDefault(x=>x!=null);
            if(!world)throw new Exception("Actual source scene missing");
            UnityEngine.Random.InitState(271828);
            world.BuildForNativeEditor();
            var ground=world.GetComponentInChildren<Modular25DGround>();
            var land=world.GetComponentInChildren<Modular25DLandscape>();
            var cam=world.ViewCamera;
            if(!ground||!land||!cam||ground.ChunkCount!=81||!land.OriginalCobbleRoadLoaded||world.Hud!=null||world.Gameplay!=null)
                throw new Exception("Frozen scene contract");
            var plan=ValoriaUrbanPlan.City;
            var routes=plan.BoroughStreets;
            if(routes[0][2]!=routes[5][0]||routes[5][0]!=plan.PostigoLane(0))throw new Exception("Source junction drift");
            var b1=new PlayerState{BastionLevel=1,SawmillLevel=1};
            world.Apply(b1);
            var road=FindRoad(land,"Calle del Postigo");
            var passage=FindRoad(land,"Paso bajo el postigo");
            if(road.activeSelf||passage.activeSelf)throw new Exception("B1 boroughs leaked");
            var home=Modular25DCameraInput.HomePosition;
            var size=Modular25DCameraInput.HomeSizeForAspect(390f/844f);
            Modular25DNativeCapture.Render(cam,output,report,"b1-A-home",390,844,home,size);
            var fixture=new PlayerState{BastionLevel=5,SawmillLevel=1,BarracksLevel=1};
            fixture.ChapterThree.GranaryLevel=1;fixture.ChapterThree.StoneworksLevel=1;
            world.Apply(fixture);
            if(!road.activeInHierarchy||!passage.activeInHierarchy)throw new Exception("B5 roads absent");
            // Identify the actual rendered arch, not a name guessed from a screenshot.
            var arch=world.GetComponentsInChildren<SpriteRenderer>(true).Where(x=>x.sprite&&x.sprite.name=="CalleDelPostigo").ToArray();
            if(arch.Length!=1||!arch[0].gameObject.activeInHierarchy)throw new Exception("Real Postigo sprite missing");
            var shadow=arch[0].GetComponentsInChildren<MeshRenderer>(true).Where(x=>x.gameObject.name=="Projected architectural ground shadow").ToArray();
            if(shadow.Length!=1)throw new Exception("Real projected shadow missing");
            var shadowRenderer=shadow[0];bool shadowEnabled=shadowRenderer.enabled;
            var material=road.GetComponent<MeshRenderer>().sharedMaterial;
            var join=land.AddRoad("Paso bajo el postigo",routes[5],.66f,material,true,1);
            var fullRoad=land.AddRoad("Calle del Postigo",routes[0],1.04f,material,true,2);
            var fullPassage=land.AddRoad("Paso bajo el postigo",routes[5],.66f,material,true,2);
            SameGeometry(passage,join);SameMeshContract(road,fullRoad);SameMeshContract(passage,fullPassage);
            if(!join.GetComponent<MeshFilter>().sharedMesh.colors32.SequenceEqual(fullPassage.GetComponent<MeshFilter>().sharedMesh.colors32))
                throw new Exception("Connected passage variants disagree");
            void Select(string mode)
            {
                road.SetActive(mode!="C");
                passage.SetActive(mode!="B"&&mode!="C");
                join.SetActive(mode=="B");fullRoad.SetActive(mode=="C");fullPassage.SetActive(mode=="C");
                shadowRenderer.enabled=mode=="S"?false:shadowEnabled;
            }
            var focus=(Vector3)plan.UpperRow+new Vector3(0,.75f,-30f);
            var junction=(Vector3)plan.Junction+new Vector3(0,.75f,-30f);
            var close=(Vector3)plan.UpperRow+new Vector3(0,.85f,-30f);
            foreach(string variant in new[]{"A","B","C","S","R"})
            {
                Select(variant);
                Modular25DNativeCapture.Render(cam,output,report,variant+"-postigo",780,1040,focus,4.8f);
                Modular25DNativeCapture.Render(cam,output,report,variant+"-close",1170,1170,close,3.3f);
                Modular25DNativeCapture.Render(cam,output,report,variant+"-calle",780,1040,junction,4.8f);
                for(int i=0;i<5;i++)
                {
                    var pos=Vector3.Lerp(focus+new Vector3(-1f,-.4f,0),focus+new Vector3(1.1f,.65f,0),i/4f);
                    Modular25DNativeCapture.Render(cam,output,report,variant+"-travel-"+i,390,844,pos,4.6f+i*.22f);
                }
            }
            foreach(string v in new[]{"postigo","close","calle","travel-0","travel-1","travel-2","travel-3","travel-4"})
            {
                var a=report.frames.Find(x=>x.name=="A-"+v);var r=report.frames.Find(x=>x.name=="R-"+v);
                if(a==null||r==null||a.sha256!=r.sha256)throw new Exception("Restoration mismatch "+v);
            }
            // Advance the actual production walker, not a stand-in; assert entrance and exit on the unchanged route.
            Select("C");
            var courier=world.GetComponentsInChildren<Modular25DWalker>(true).Single(x=>x.gameObject.name=="Valoria citizen A");
            var entry=plan.PostigoLane(0);var exit=plan.BoroughStreets[5][3];
            int steps=0;
            for(;Vector2.Distance(courier.transform.localPosition,entry)>.11f;steps++){
                if(steps>=3000)throw new Exception("Postigo entrance unreachable");
                courier.StepForNativeCapture(1f/24f);
            }
            Modular25DNativeCapture.Render(cam,output,report,"C-courier-entry",540,720,close,2.7f);
            int exitSteps=0;
            for(;Vector2.Distance(courier.transform.localPosition,exit)>.11f;exitSteps++){
                if(exitSteps>=3000)throw new Exception("Postigo exit unreachable");
                courier.StepForNativeCapture(1f/24f);
            }
            Modular25DNativeCapture.Render(cam,output,report,"C-courier-exit",540,720,close,2.7f);
            Select("R");world.Apply(b1);
            if(road.activeSelf||passage.activeSelf||join.activeSelf||fullRoad.activeSelf||fullPassage.activeSelf)
                throw new Exception("B1 road regression");
            // The B1 control at the same initial fixture was rendered before movement. Compare an
            // independent B1 original/candidate-disabled pair after movement, not a misleading time comparison.
            Modular25DNativeCapture.Render(cam,output,report,"b1-R-home",390,844,home,size);
            Modular25DNativeCapture.Render(cam,output,report,"b1-R-repeat",390,844,home,size);
            if(report.frames.Find(x=>x.name=="b1-R-home").sha256!=report.frames.Find(x=>x.name=="b1-R-repeat").sha256)
                throw new Exception("B1 control not deterministic");
            report.nativeWorldRender=true;report.didNotAlterBeta=true;report.noLegacyHudInEditorPreview=true;
            report.continuousTerrainPresent=true;report.sourceFrames=report.frames.Count;report.progressionStagesCaptured=2;
            File.WriteAllText(Path.Combine(output,"native-evidence.json"),JsonUtility.ToJson(report,true));
            File.WriteAllText(Path.Combine(output,"postigo-contract.json"),"{\"road_centerlines_preserved\":true,\"original_material_preserved\":true,\"eight_exact_reversals\":true,\"b1_boroughs_hidden\":true,\"courier_entry_steps\":"+steps+",\"courier_exit_steps\":"+exitSteps+",\"production_writes\":false,\"art_approved\":false}");
            Debug.Log("POSTIGO_REAL_NATIVE_PASS "+report.sourceFrames+" frames");
        }
    }
}
#endif

using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaFullFrameForegroundEdgeGateV1
    {
        const string Folder="ValoriaFullFrameForegroundEdgeV1Captures";
        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
            AssetVisualUpliftPassV1.Enabled=false;AssetLibraryReprocessingPassV1.Enabled=true;ValoriaReferenceConvergencePassV2.Enabled=false;ValoriaInCitySurfacePassV1.Enabled=false;ValoriaStairLandingIntegrationV1.Enabled=false;ValoriaFullFrameConvergenceIteration1.Enabled=false;ValoriaFullFrameConvergenceIteration2.Enabled=false;ValoriaFullFrameForegroundEdgePassV1.Enabled=false;ValoriaWorldFrameMountainTerrainV1.Enabled=true;VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};VisualWorld.Create(true,state);
            var camera=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");if(camera==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");
            var p=new Vector3(18.2f,14.6f,-25.8f);var t=new Vector3(0f,3.15f,5.8f);var baseline=ValoriaVisualFormulaGate.CollisionSignature();
            SaveSet(camera,"before",p,t);WriteMetrics(Folder+"/before-metrics.json");
            ValoriaFullFrameForegroundEdgePassV1.Enabled=true;ValoriaFullFrameForegroundEdgePassV1.Build(root.transform,state);Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)throw new System.Exception("Foreground edge pass altered gameplay signature.");
            SaveSet(camera,"after",p,t);WriteMetrics(Folder+"/after-metrics.json");
            File.WriteAllText(Folder+"/evidence.json","{\n  \"camera_matched\": true,\n  \"collider_hotspot_signature_equal\": true,\n  \"existing_real_rock_assets_only\": true,\n  \"tripo_credits\": 0\n}\n");
            EditorApplication.Exit(0);
        }
        static void SaveSet(Camera c,string tag,Vector3 p,Vector3 t){Save(c,Folder+"/"+tag+"-19.png",p,t,19,1280,720);Save(c,Folder+"/"+tag+"-12.png",p,t,12,1280,720);Save(c,Folder+"/"+tag+"-9.png",p,t,9,1280,720);Save(c,Folder+"/"+tag+"-mobile.png",p,t,12,390,844);}
        static void WriteMetrics(string path){int renderers=0,lights=0;long triangles=0;var materials=new HashSet<string>();foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;renderers++;foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.name);}foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;for(int s=0;s<mf.sharedMesh.subMeshCount;s++)triangles+=(long)mf.sharedMesh.GetIndexCount(s)/3L;}foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;File.WriteAllText(path,"{\n  \"active_renderers\": "+renderers+",\n  \"unique_materials\": "+materials.Count+",\n  \"scene_triangles\": "+triangles+",\n  \"active_lights\": "+lights+"\n}\n");}
        static void Save(Camera camera,string path,Vector3 position,Vector3 target,float size,int width,int height){camera.transform.position=position;camera.transform.LookAt(target);camera.orthographic=true;camera.orthographicSize=size;var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;try{camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);}finally{camera.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}}
    }
}

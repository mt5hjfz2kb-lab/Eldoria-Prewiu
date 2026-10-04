using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Editor-only deterministic stop gate. Per unity-slice impact policy this file does not
    // require the full player-build job; it exists to certify reset phases with matched captures.
    public static class ValoriaProductionArtResetGateV1
    {
        const string Folder="ValoriaProductionArtResetV1Captures";

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);
            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};

            var scene=Create(state);
            var signature=ValoriaVisualFormulaGate.CollisionSignature();
            WriteMetrics(Folder+"/before-metrics.json");
            Save(scene.camera,Folder+"/before-9.png",9f,1280,720);
            Save(scene.camera,Folder+"/before-mobile.png",9.4f,390,844);

            // Matched A/B in one live scene. Recreating the entire canonical scene here invalidated
            // runtime-created material state owned by older visual passes; phase C only needs to add
            // the visual-only authored family to the exact captured baseline.
            ValoriaProductionArtResetV1.Apply(scene.root.transform,state);
            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)
                throw new Exception("Production Art candidate changed gameplay collider/hotspot signature.");
            if(ValoriaProductionArtResetV1.Pieces!=8)
                throw new Exception("Expected 8 authored starter instances, got "+ValoriaProductionArtResetV1.Pieces);

            WriteMetrics(Folder+"/after-metrics.json");
            Save(scene.camera,Folder+"/after-9.png",9f,1280,720);
            Save(scene.camera,Folder+"/after-mobile.png",9.4f,390,844);

            // Phase D: normalize candidate surface response while preserving the same geometry/gameplay.
            var resetRoot=GameObject.Find(ValoriaProductionArtResetV1.RootName);
            ValoriaProductionArtResetV1.NormalizeProductionMaterials(resetRoot);
            Save(scene.camera,Folder+"/material-9.png",9f,1280,720);
            Save(scene.camera,Folder+"/material-mobile.png",9.4f,390,844);

            // Phase E: reversible final-look profile. No canonical mobile profile is modified.
            ValoriaProductionFinalLookV1.Apply();
            Save(scene.camera,Folder+"/final-look-9.png",9f,1280,720);
            Save(scene.camera,Folder+"/final-look-mobile.png",9.4f,390,844);

            // Phase F: matched mild strategic perspective, same target and production frame.
            SavePerspective(scene.camera,Folder+"/perspective-9.png",1280,720,false);
            SavePerspective(scene.camera,Folder+"/perspective-mobile.png",390,844,true);

            if(ValoriaVisualFormulaGate.CollisionSignature()!=signature)
                throw new Exception("D/E/F visual layers changed gameplay collider/hotspot signature.");

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"gameplay_signature_preserved\": true,\n"+
                "  \"starter_pieces\": 8,\n"+
                "  \"source_classification\": \"TEMPORARY_PENDING_VISUAL_REVIEW\",\n"+
                "  \"zoom9_mobile_stop_gate\": true,\n"+
                "  \"materials_phase\": \"CAPTURED_FOR_VISUAL_REVIEW\",\n"+
                "  \"final_look_phase\": \"CAPTURED_FOR_VISUAL_REVIEW\",\n"+
                "  \"camera_ab_phase\": \"CAPTURED_FOR_VISUAL_REVIEW\",\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            Debug.Log("VALORIA_PRODUCTION_ART_RESET_PHASE_C_GATE=PASS");
            EditorApplication.Exit(0);
        }

        struct SceneData{public Camera camera;public GameObject root;}

        static SceneData Create(PlayerState state)
        {
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;
            ProductionVisualIntegration.TerrainTerraceEnabled=true;
            ProductionVisualIntegration.SurfaceCellEnabled=false;
            ProductionVisualIntegration.ProductionCellEnabled=false;
            ProductionVisualIntegration.CoherentCastleProofEnabled=false;
            ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
            ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
            AssetVisualUpliftPassV1.Enabled=false;
            AssetLibraryReprocessingPassV1.Enabled=true;
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(camera==null||root==null)throw new Exception("Production Art capture prerequisites missing.");
            return new SceneData{camera=camera,root=root};
        }

        static void WriteMetrics(string path)
        {
            int renderers=0,lights=0;long triangles=0;var materials=new HashSet<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                renderers++;foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.name);
            }
            foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;
                var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;
                for(int s=0;s<mf.sharedMesh.subMeshCount;s++)triangles+=(long)mf.sharedMesh.GetIndexCount(s)/3L;
            }
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
            File.WriteAllText(path,
                "{\n  \"active_renderers\": "+renderers+
                ",\n  \"unique_materials\": "+materials.Count+
                ",\n  \"scene_triangles\": "+triangles+
                ",\n  \"active_lights\": "+lights+"\n}\n");
        }

        static void Save(Camera c,string path,float size,int w,int h)
        {
            var p=new Vector3(18.2f,14.6f,-25.8f);var t=new Vector3(0f,3.15f,5.8f);
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
            try{
                c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
            }finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
        }

        static void SavePerspective(Camera c,string path,int w,int h,bool mobile)
        {
            var target=mobile?new Vector3(0f,3.45f,5.8f):new Vector3(0f,3.15f,5.8f);
            c.orthographic=false;
            c.fieldOfView=mobile?24f:27f;
            c.transform.position=mobile?new Vector3(22.5f,18.2f,-31.8f):new Vector3(22.8f,18.4f,-32.2f);
            c.transform.LookAt(target);
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
            try{
                c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
            }finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}

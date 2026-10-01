using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class MidTierDistrictProductionGate
    {
        const string Folder="MidTierDistrictProductionCaptures";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            Directory.CreateDirectory(Folder);

            MidTierDistrictProduction.Enabled=false;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,State(3));
            var camera=Camera.main;if(camera==null)throw new Exception("Baseline camera missing");
            string baseline=ValoriaVisualFormulaGate.CollisionSignature();
            Save(camera,Folder+"/baseline-19.png",19,1280,720);
            Save(camera,Folder+"/baseline-12.png",12,1280,720);
            Save(camera,Folder+"/baseline-9.png",9,1280,720);
            Save(camera,Folder+"/baseline-mobile.png",12,390,844);

            MidTierDistrictProduction.Enabled=true;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,State(3));
            camera=Camera.main;if(camera==null)throw new Exception("Production camera missing");
            string after=ValoriaVisualFormulaGate.CollisionSignature();
            if(after!=baseline)throw new Exception("Mid-Tier production rollout altered gameplay collider/hotspot signature.");
            var district=GameObject.Find("Valoria · Mid-Tier District v1 · production visual only");
            if(district==null)throw new Exception("Production Mid-Tier district was not instantiated at Bastion III.");
            int renderers=district.GetComponentsInChildren<Renderer>(true).Length;
            int enabledColliders=0;foreach(var c in district.GetComponentsInChildren<Collider>(true))if(c.enabled)enabledColliders++;
            int hotspots=district.GetComponentsInChildren<WorldHotspot>(true).Length;
            if(enabledColliders!=0||hotspots!=0)throw new Exception("Production district owns gameplay authority.");
            Save(camera,Folder+"/production-19.png",19,1280,720);
            Save(camera,Folder+"/production-12.png",12,1280,720);
            Save(camera,Folder+"/production-9.png",9,1280,720);
            Save(camera,Folder+"/production-mobile.png",12,390,844);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,State(2));
            if(GameObject.Find("Valoria · Mid-Tier District v1 · production visual only")!=null)
                throw new Exception("Mid-Tier district leaked into Bastion II progression.");

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"phase\": \"MID_TIER_DISTRICT_PRODUCTION_V1\",\n"+
                "  \"bastion_iii_instantiated\": true,\n"+
                "  \"bastion_ii_hidden\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"gameplay_colliders_added\": 0,\n"+
                "  \"gameplay_hotspots_added\": 0,\n"+
                "  \"district_renderers\": "+renderers+",\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static PlayerState State(int bastion)=>new PlayerState{BastionLevel=bastion,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CameraPosition;camera.transform.LookAt(CameraTarget);camera.orthographic=true;camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}

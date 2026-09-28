using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Zero-credit diagnostic only. It does not modify production materials or lighting.
    public static class ValoriaLookDevCapture
    {
        sealed class Profile
        {
            public string Id;
            public Color Ambient;
            public Color Fog;
            public float FogStart;
            public float FogEnd;
            public Color Sun;
            public float SunIntensity;
            public float ShadowStrength;
            public Vector3 SunEuler;
        }

        public static void Capture()
        {
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var state=new PlayerState { BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true };
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Valoria camera was not created");

            Light sun=null;
            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(light.type==LightType.Directional){sun=light;break;}
            if(sun==null)throw new System.Exception("Valoria directional light was not created");

            const string folder="LookDevCaptures";
            Directory.CreateDirectory(folder);
            var profiles=new List<Profile>{
                new Profile{Id="baseline",Ambient=new Color(.92f,.91f,.87f),Fog=new Color(.64f,.66f,.65f),FogStart=28f,FogEnd=62f,Sun=new Color(1f,.88f,.74f),SunIntensity=1.95f,ShadowStrength=.48f,SunEuler=new Vector3(50f,-32f,0f)},
                new Profile{Id="neutral-pbr",Ambient=new Color(.60f,.62f,.64f),Fog=new Color(.52f,.55f,.58f),FogStart=30f,FogEnd=66f,Sun=new Color(.98f,.96f,.92f),SunIntensity=1.35f,ShadowStrength=.65f,SunEuler=new Vector3(48f,-28f,0f)},
                new Profile{Id="ruins-contrast",Ambient=new Color(.47f,.50f,.52f),Fog=new Color(.42f,.46f,.49f),FogStart=31f,FogEnd=68f,Sun=new Color(1f,.78f,.58f),SunIntensity=1.55f,ShadowStrength=.70f,SunEuler=new Vector3(54f,-38f,0f)},
                new Profile{Id="neutral-overcast",Ambient=new Color(.70f,.70f,.70f),Fog=new Color(.63f,.63f,.63f),FogStart=28f,FogEnd=62f,Sun=Color.white,SunIntensity=.90f,ShadowStrength=.55f,SunEuler=new Vector3(55f,-25f,0f)},
                new Profile{Id="valoria-v1-candidate",Ambient=new Color(.54f,.56f,.57f),Fog=new Color(.46f,.49f,.51f),FogStart=31f,FogEnd=68f,Sun=new Color(1f,.84f,.68f),SunIntensity=1.45f,ShadowStrength=.68f,SunEuler=new Vector3(52f,-34f,0f)}
            };

            var officialPosition=new Vector3(18.2f,14.6f,-25.8f);
            var officialTarget=new Vector3(0,3.15f,5.8f);
            var sawmillShift=new Vector3(-7f,-1.55f,-8.6f);
            var barracksShift=new Vector3(7f,-1.55f,-9.8f);
            var json=new System.Text.StringBuilder();
            json.AppendLine("{\n  \"profiles\": [");
            for(int i=0;i<profiles.Count;i++)
            {
                var p=profiles[i];
                Apply(p,sun);
                Save(camera,folder+"/"+p.Id+"-overview.png",officialPosition,officialTarget,12f,1280,720);
                Save(camera,folder+"/"+p.Id+"-sawmill.png",officialPosition+sawmillShift,officialTarget+sawmillShift,9f,1280,720);
                Save(camera,folder+"/"+p.Id+"-barracks.png",officialPosition+barracksShift,officialTarget+barracksShift,9f,1280,720);
                json.Append("    {\"id\":\"").Append(p.Id).Append("\",\"sunIntensity\":").Append(p.SunIntensity).Append(",\"shadowStrength\":").Append(p.ShadowStrength).Append("}");
                if(i<profiles.Count-1)json.Append(",");
                json.AppendLine();
            }
            json.AppendLine("  ]\n}");
            File.WriteAllText(folder+"/lookdev-profiles.json",json.ToString());
            Debug.Log("Valoria LookDev captures saved to "+Path.GetFullPath(folder));
            UnityEditor.EditorApplication.Exit(0);
        }

        static void Apply(Profile p,Light sun)
        {
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=p.Ambient;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=p.Fog;
            RenderSettings.fogStartDistance=p.FogStart;
            RenderSettings.fogEndDistance=p.FogEnd;
            sun.color=p.Sun;
            sun.intensity=p.SunIntensity;
            sun.shadowStrength=p.ShadowStrength;
            sun.transform.rotation=Quaternion.Euler(p.SunEuler);
        }

        static void Save(Camera camera,string path,Vector3 position,Vector3 target,float size,int width,int height)
        {
            camera.transform.position=position;
            camera.transform.LookAt(target);
            camera.orthographic=true;
            camera.orthographicSize=size;
            camera.backgroundColor=RenderSettings.fogColor;
            var targetTexture=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=targetTexture;
                camera.Render();
                RenderTexture.active=targetTexture;
                var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);
                image.Apply();
                File.WriteAllBytes(path,image.EncodeToPNG());
                Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture=null;
                RenderTexture.active=previous;
                targetTexture.Release();
                Object.DestroyImmediate(targetTexture);
            }
        }
    }
}

using Eldoria.Domain;
using UnityEngine;

namespace Eldoria.Presentation
{
    public static class ValoriaPanoramicSkyCandidateV1
    {
        public static bool Enabled=false;
        public static bool Apply()
        {
            if(!Enabled)return false;
            var tex=Resources.Load<Texture2D>("Valoria/SkyCandidates/kiara_1_dawn");
            if(tex==null)return false;
            var shader=Shader.Find("Skybox/Panoramic");
            if(shader==null)return false;
            var sky=new Material(shader){name="Valoria · Panoramic Sky Candidate v1"};
            if(sky.HasProperty("_MainTex"))sky.SetTexture("_MainTex",tex);
            if(sky.HasProperty("_Tint"))sky.SetColor("_Tint",new Color(.78f,.86f,.94f,1f));
            if(sky.HasProperty("_Exposure"))sky.SetFloat("_Exposure",.82f);
            if(sky.HasProperty("_Rotation"))sky.SetFloat("_Rotation",78f);
            if(sky.HasProperty("_Mapping"))sky.SetFloat("_Mapping",1f);
            if(sky.HasProperty("_ImageType"))sky.SetFloat("_ImageType",0f);
            RenderSettings.skybox=sky;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.66f,.73f,.79f);
            RenderSettings.ambientEquatorColor=new Color(.48f,.47f,.43f);
            RenderSettings.ambientGroundColor=new Color(.24f,.22f,.20f);
            RenderSettings.ambientIntensity=.90f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.54f,.61f,.66f);
            RenderSettings.fogStartDistance=38f;
            RenderSettings.fogEndDistance=105f;
            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=CameraClearFlags.Skybox;
                camera.allowHDR=true;
            }
            DynamicGI.UpdateEnvironment();
            return true;
        }
    }
}

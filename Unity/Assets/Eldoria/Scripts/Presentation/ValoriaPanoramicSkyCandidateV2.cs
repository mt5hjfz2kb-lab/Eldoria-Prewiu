using UnityEngine;

namespace Eldoria.Presentation
{
    // CC0 panoramic background candidate for the fixed Valoria gameplay camera.
    // Proof-only until full-frame review accepts one rotation and a runtime-sized texture.
    public static class ValoriaPanoramicSkyCandidateV2
    {
        public static bool Enabled=false;
        public static float Rotation=0f;
        static Material sky;

        public static bool Apply()
        {
            if(!Enabled)return false;
            var tex=Resources.Load<Texture2D>("Valoria/SkyCandidates/alps_field");
            if(tex==null)return false;
            var shader=Shader.Find("Skybox/Panoramic");
            if(shader==null)return false;

            if(sky==null)
            {
                sky=new Material(shader){name="Valoria · Alps Field Panoramic Sky v2"};
                if(sky.HasProperty("_MainTex"))sky.SetTexture("_MainTex",tex);
                if(sky.HasProperty("_Tint"))sky.SetColor("_Tint",new Color(.76f,.84f,.91f,1f));
                if(sky.HasProperty("_Exposure"))sky.SetFloat("_Exposure",.92f);
                if(sky.HasProperty("_Mapping"))sky.SetFloat("_Mapping",1f);
                if(sky.HasProperty("_ImageType"))sky.SetFloat("_ImageType",0f);
            }
            if(sky.HasProperty("_Rotation"))sky.SetFloat("_Rotation",Rotation);

            RenderSettings.skybox=sky;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.68f,.76f,.83f);
            RenderSettings.ambientEquatorColor=new Color(.49f,.48f,.43f);
            RenderSettings.ambientGroundColor=new Color(.24f,.22f,.19f);
            RenderSettings.ambientIntensity=.93f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.58f,.66f,.71f);
            RenderSettings.fogStartDistance=34f;
            RenderSettings.fogEndDistance=104f;

            var camera=Camera.main;
            if(camera!=null)
            {
                camera.clearFlags=CameraClearFlags.Skybox;
                camera.backgroundColor=RenderSettings.fogColor;
                camera.allowHDR=true;
            }

            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if(light.type!=LightType.Directional)continue;
                light.color=new Color(1f,.91f,.79f);
                light.intensity=Mathf.Max(light.intensity,1.12f);
                light.shadowStrength=.48f;
                light.shadows=LightShadows.Soft;
            }
            DynamicGI.UpdateEnvironment();
            return true;
        }
    }
}

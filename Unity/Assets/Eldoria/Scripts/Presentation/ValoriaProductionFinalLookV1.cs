using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.Presentation
{
    // Reversible development/final-look layer for the reset evidence gate.
    // Runtime/mobile profile remains untouched unless this layer is explicitly applied.
    public static class ValoriaProductionFinalLookV1
    {
        public static GameObject Apply()
        {
            var old=GameObject.Find("Valoria · Production Final Look v1");
            if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject("Valoria · Production Final Look v1");

            // Directional fill/shadow definition. Kept modest so Hero lighting is not blown out.
            var go=new GameObject("Final Look · warm key");
            go.transform.SetParent(root.transform,false);
            go.transform.rotation=Quaternion.Euler(46f,-32f,0f);
            var key=go.AddComponent<Light>();
            key.type=LightType.Directional;
            key.color=new Color(1f,.84f,.67f);
            key.intensity=.38f;
            key.shadows=LightShadows.Soft;
            key.shadowStrength=.72f;

            // Low-intensity cool fill gives slate/stone separation and prevents flat grey shadows.
            var fillGo=new GameObject("Final Look · cool fill");
            fillGo.transform.SetParent(root.transform,false);
            fillGo.transform.rotation=Quaternion.Euler(58f,142f,0f);
            var fill=fillGo.AddComponent<Light>();
            fill.type=LightType.Directional;
            fill.color=new Color(.55f,.68f,.82f);
            fill.intensity=.10f;
            fill.shadows=LightShadows.None;

            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.30f,.34f,.38f);
            RenderSettings.ambientEquatorColor=new Color(.22f,.20f,.17f);
            RenderSettings.ambientGroundColor=new Color(.10f,.09f,.075f);
            RenderSettings.ambientIntensity=.72f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.ExponentialSquared;
            RenderSettings.fogColor=new Color(.40f,.44f,.46f);
            RenderSettings.fogDensity=.0048f;

            var volGo=new GameObject("Final Look · volume");
            volGo.transform.SetParent(root.transform,false);
            var vol=volGo.AddComponent<Volume>();vol.isGlobal=true;vol.priority=120f;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();
            vol.sharedProfile=profile;
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
            var ca=profile.Add<ColorAdjustments>(true);
            ca.postExposure.Override(-.12f);ca.contrast.Override(10f);ca.saturation.Override(-3f);
            var wb=profile.Add<WhiteBalance>(true);wb.temperature.Override(4f);wb.tint.Override(-2f);
            return root;
        }
    }
}

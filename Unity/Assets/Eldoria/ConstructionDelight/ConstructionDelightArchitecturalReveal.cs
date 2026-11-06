using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.ReferenceClean
{
    // An architectural animation on the TRUE existing meshes, not a floating
    // icon or a carpet of gold on the lawn. Owns temporary renderer overlays,
    // a shadowless local highlight and an optional short URP Bloom volume.
    // Never edits a source mesh, a shared material, a saved scene or a save.
    internal sealed class ConstructionDelightArchitecturalReveal
    {
        const float FinishSeconds=1.65f;
        const int MaxOverlayMeshes=28;

        readonly List<MeshRenderer> overlays=new List<MeshRenderer>(MaxOverlayMeshes);
        readonly List<GameObject> overlayObjects=new List<GameObject>(MaxOverlayMeshes);
        Material glowMaterial;
        GameObject overlayRoot;
        GameObject lightRoot;
        GameObject bloomRoot;
        Light localLight;
        Volume bloomVolume;
        VolumeProfile bloomProfile;
        bool running, finishing, visible=true;
        float began;
        float boundsMinY, boundsHeight;
        int builtCount;

        static readonly int GlowColor=Shader.PropertyToID("_GlowColor");
        static readonly int Strength=Shader.PropertyToID("_Strength");
        static readonly int Sweep=Shader.PropertyToID("_Sweep");
        static readonly int MinY=Shader.PropertyToID("_MinY");
        static readonly int Height=Shader.PropertyToID("_Height");
        static readonly int Mode=Shader.PropertyToID("_Mode");

        public int MeshCount=>running ? builtCount:0;
        public bool Finishing=>running && finishing;
        public bool HasBloomVolume=>bloomVolume && bloomProfile;

        public void BeginWork(GameObject scaffold,float time)
        {
            Clear();
            if(!BuildOverlays(scaffold))return;
            began=time;
            finishing=false;
            running=true;
            visible=true;
            UpdateVisuals(time);
        }

        public void Complete(GameObject finishedBuilding,Camera camera,float time,
                             bool includeStagedParts=false)
        {
            Clear();
            if(!BuildOverlays(finishedBuilding,includeStagedParts))return;
            began=time;
            finishing=true;
            running=true;
            visible=true;

            // A single unshadowed temporary light has a short lifetime.
            // It lifts the actual facade and gable, never the entire city.
            var bounds=GetBounds();
            lightRoot=new GameObject("Construction Delight - warm practical light");
            var middle=bounds.center;
            middle.y=bounds.min.y+bounds.size.y*.66f;
            Vector3 toEye=camera ? camera.transform.position-middle : Vector3.back;
            toEye.y=0f;
            if(toEye.sqrMagnitude>.01f)middle+=toEye.normalized*(bounds.extents.magnitude*.43f);
            lightRoot.transform.position=middle;
            localLight=lightRoot.AddComponent<Light>();
            localLight.type=LightType.Point;
            localLight.color=new Color(1f,.78f,.52f);
            localLight.intensity=0f;
            localLight.range=Mathf.Clamp(bounds.extents.magnitude*1.95f,3.5f,13f);
            localLight.shadows=LightShadows.None;
            localLight.renderMode=LightRenderMode.ForcePixel;
            localLight.enabled=false;

            // Bloom is supplementary and opt-in to the existing camera's
            // URP postprocess settings. Never alter a shared camera or volume.
            var data=camera ? camera.GetComponent<UniversalAdditionalCameraData>() : null;
            if(data && data.renderPostProcessing &&
               (data.volumeLayerMask.value & (1 << camera.gameObject.layer))!=0)
            {
                bloomRoot=new GameObject("Construction Delight - temporary Bloom");
                bloomRoot.layer=camera.gameObject.layer;
                bloomVolume=bloomRoot.AddComponent<Volume>();
                bloomVolume.isGlobal=true;
                bloomVolume.priority=18f;
                bloomVolume.weight=0f;
                bloomProfile=ScriptableObject.CreateInstance<VolumeProfile>();
                var bloom=bloomProfile.Add<Bloom>(true);
                bloom.threshold.Override(1.13f);
                bloom.intensity.Override(.24f);
                bloom.scatter.Override(.48f);
                bloom.tint.Override(new Color(1f,.89f,.74f));
                bloom.highQualityFiltering.Override(false);
                bloomVolume.sharedProfile=bloomProfile;
            }
            UpdateVisuals(time);
        }

        bool BuildOverlays(GameObject building,bool includeStagedParts=false)
        {
            if(!building || !building.activeInHierarchy)return false;
            var shader=Resources.Load<Shader>("ConstructionDelightArchitecturalReveal");
            if(!shader || !shader.isSupported)
            {
                Debug.LogWarning("CONSTRUCTION_DELIGHT_ARCHITECTURAL_SHADER_UNSUPPORTED");
                return false;
            }
            glowMaterial=new Material(shader){name="Eldoria - authentic masonry and timber sweep"};
            glowMaterial.SetColor(GlowColor,new Color(1.12f,.74f,.40f,1f));
            overlayRoot=new GameObject("Construction Delight - transient architectural sweep");
            overlayRoot.transform.SetParent(building.transform,false);
            overlayRoot.layer=building.layer;
            var sources=building.GetComponentsInChildren<MeshRenderer>(true);
            var hasBounds=false;
            var combined=new Bounds();
            for(int j=0;j<sources.Length && overlays.Count<MaxOverlayMeshes;j++)
            {
                var source=sources[j];
                if(!source || (!source.enabled && !includeStagedParts) ||
                   !source.gameObject.activeInHierarchy ||
                   source.transform.IsChildOf(overlayRoot.transform))continue;
                var filter=source.GetComponent<MeshFilter>();
                if(!filter || !filter.sharedMesh || !source.sharedMaterial)continue;
                // Never overlay transparent windows, foliage or UI surfaces.
                if(source.sharedMaterial.renderQueue>=(int)RenderQueue.Transparent)continue;
                if(!hasBounds){combined=source.bounds;hasBounds=true;}
                else combined.Encapsulate(source.bounds);

                var child=new GameObject("Construction Delight - real mesh highlight",
                    typeof(MeshFilter),typeof(MeshRenderer));
                child.layer=source.gameObject.layer;
                child.transform.SetParent(source.transform,false);
                child.GetComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
                var renderer=child.GetComponent<MeshRenderer>();
                int submeshes=Mathf.Max(1,filter.sharedMesh.subMeshCount);
                var materials=new Material[submeshes];
                for(int i=0;i<submeshes;i++)materials[i]=glowMaterial;
                renderer.sharedMaterials=materials;
                renderer.shadowCastingMode=ShadowCastingMode.Off;
                renderer.receiveShadows=false;
                renderer.enabled=true;
                overlays.Add(renderer);
                overlayObjects.Add(child);
            }
            if(!hasBounds || overlays.Count==0){Clear();return false;}
            boundsMinY=combined.min.y;
            boundsHeight=Mathf.Max(.35f,combined.size.y);
            builtCount=overlays.Count;
            glowMaterial.SetFloat(MinY,boundsMinY);
            glowMaterial.SetFloat(Height,boundsHeight);
            return true;
        }

        Bounds GetBounds()
        {
            bool has=false;
            Bounds result=new Bounds();
            foreach(var overlay in overlays)
            {
                if(!overlay)continue;
                if(!has){result=overlay.bounds;has=true;}
                else result.Encapsulate(overlay.bounds);
            }
            return result;
        }

        public void Tick(float time)
        {
            if(!running)return;
            if(finishing && time-began>=FinishSeconds){Clear();return;}
            UpdateVisuals(time);
        }

        void UpdateVisuals(float time)
        {
            if(!running || !glowMaterial)return;
            float t=Mathf.Max(0f,time-began);
            bool drawSurfaces=true;
            if(finishing)
            {
                float progress=Mathf.Clamp01((t-.03f)/.84f);
                float sweep=Mathf.Lerp(-.18f,1.20f,Mathf.SmoothStep(0f,1f,progress));
                float rise=Mathf.SmoothStep(0f,1f,Mathf.Clamp01(t/.32f));
                float fade=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((FinishSeconds-t)/.52f));
                float envelope=rise*fade;
                glowMaterial.SetFloat(Mode,1f);
                glowMaterial.SetFloat(Sweep,sweep);
                glowMaterial.SetFloat(Strength,envelope*.63f);
                if(localLight)
                {
                    float flash=Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/1.35f));
                    localLight.intensity=visible ? Mathf.Max(0f,flash)*.62f : 0f;
                    localLight.enabled=visible && localLight.intensity>.01f;
                }
                if(bloomVolume)
                {
                    bloomVolume.weight=visible ?
                        Mathf.Clamp01(Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.94f)))*.24f:0f;
                    bloomVolume.enabled=visible && bloomVolume.weight>.001f;
                }
            }
            else
            {
                // Tactile scaffold activity, not constant magic on unfinished work.
                float cycle=Mathf.Repeat(t/3.0f,1f);
                // Render scaffold highlights only in short active work pulses.
                // Keeps mobile GPU cost low during long real-world build timers.
                drawSurfaces=cycle<.35f;
                glowMaterial.SetFloat(Mode,0f);
                glowMaterial.SetFloat(Sweep,Mathf.Lerp(-.18f,1.16f,cycle));
                glowMaterial.SetFloat(Strength,.20f+.11f*Mathf.Sin(Mathf.PI*cycle));
            }
            foreach(var overlay in overlays)
            {
                if(!overlay)continue;
                // A staged structure's original mesh part may still be landing.
                // Never draw a glowing outline of a roof before that roof exists.
                var source=overlay.transform.parent ?
                    overlay.transform.parent.GetComponent<MeshRenderer>() : null;
                overlay.enabled=visible && drawSurfaces &&
                    (!source || source.enabled);
            }
        }

        public void SetVisible(bool show,float time)
        {
            visible=show;
            if(!show)
            {
                foreach(var overlay in overlays)if(overlay)overlay.enabled=false;
                if(localLight)localLight.enabled=false;
                if(bloomVolume)bloomVolume.enabled=false;
            }
            else UpdateVisuals(time);
        }

        public void Clear()
        {
            foreach(var overlay in overlays)if(overlay)overlay.enabled=false;
            foreach(var child in overlayObjects)if(child)Object.Destroy(child);
            overlayObjects.Clear();
            overlays.Clear();
            builtCount=0;
            running=false;finishing=false;visible=true;
            if(localLight)localLight.enabled=false;
            if(bloomVolume)bloomVolume.enabled=false;
            if(overlayRoot)Object.Destroy(overlayRoot);
            if(lightRoot)Object.Destroy(lightRoot);
            if(bloomRoot)Object.Destroy(bloomRoot);
            if(bloomProfile)Object.Destroy(bloomProfile);
            if(glowMaterial)Object.Destroy(glowMaterial);
            overlayRoot=null;lightRoot=null;bloomRoot=null;
            localLight=null;bloomVolume=null;bloomProfile=null;glowMaterial=null;
        }
    }
}

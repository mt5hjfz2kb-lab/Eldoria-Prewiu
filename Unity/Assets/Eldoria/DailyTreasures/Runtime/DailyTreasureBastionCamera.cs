using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// One-frame cinematic close-up of the ACTUAL active Bastion in the
    /// player's own loaded city. No invented building, future tier, loot,
    /// screenshot upscaling or mutation of gameplay camera/state.
    /// The optional GPU texture is owned and destroyed by DailyTreasurePanel.
    /// </summary>
    public static class DailyTreasureBastionCamera
    {
        public const string SceneKey="RealBastion/LiveAuthenticatedCamera";

        // VERIFIED against actual main source:
        // ValoriaIntegratedWorld owns PlayerCamera but NOT BastionOne/Two.
        // Those GameObjects belong to ValoriaReferenceCleanGameplay; the old
        // reflection searched the wrong component and always returned null,
        // silently leaving the milestone on a black placeholder.
        static bool Resolve(out Camera source,out Bounds bounds)
        {
            source=null;
            bounds=default;
            var components=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(var item in components)
            {
                if(!item || item.GetType().Name!="ValoriaReferenceCleanGameplay" ||
                   !item.gameObject.scene.isLoaded)continue;
                var type=item.GetType();
                var property=type.GetProperty("PlayerCamera",
                    BindingFlags.Instance|BindingFlags.Public);
                var realCamera=property?.GetValue(item,null) as Camera;
                if(!realCamera ||
                   realCamera.gameObject.scene!=item.gameObject.scene)continue;
                // The real gameplay class activates BastionTwo for ALL
                // built levels >=2 and BastionOne for level 1. Use only
                // active existing renderers; never create a fake future 3D.
                foreach(var slot in new[]{"BastionTwo","BastionOne"})
                {
                    var field=type.GetField(slot,
                        BindingFlags.Public|BindingFlags.Instance);
                    var propertySlot=type.GetProperty(slot,
                        BindingFlags.Public|BindingFlags.Instance);
                    var building=(propertySlot!=null?
                        propertySlot.GetValue(item,null):
                        field?.GetValue(item)) as GameObject;
                    if(!building || !building.activeInHierarchy)continue;
                    bool any=false;
                    Bounds realBounds=default;
                    foreach(var renderer in building.GetComponentsInChildren<Renderer>(true))
                    {
                        if(!renderer || !renderer.enabled ||
                           !renderer.gameObject.activeInHierarchy)continue;
                        if(!any){realBounds=renderer.bounds;any=true;}
                        else realBounds.Encapsulate(renderer.bounds);
                    }
                    if(!any || realBounds.size.sqrMagnitude<=.01f)continue;
                    source=realCamera;
                    bounds=realBounds;
                    return true;
                }
            }
            Debug.LogWarning("TRUE_BASTION_NOT_ACTIVE: cannot manufacture ceremonial architecture");
            return false;
        }

        // The original city camera is a useful gameplay isometric view,
        // but its near-vertical angle produced a tiny, emotionally flat
        // building in the REAL 43005395878 captures. This ceremonial camera
        // should reveal the already-constructed facade and depth: a 38-degree
        // perspective 3/4 portrait lit by actual scene geometry. No invented
        // mesh, replacement asset, panorama or altered player camera.
        static void Frame(Camera camera,Bounds b)
        {
            var originalHeading=Vector3.ProjectOnPlane(
                -camera.transform.forward,Vector3.up);
            // A straight top-down gameplay camera has no reliable azimuth.
            // Preserve the same player-facing quadrant when possible,
            // otherwise use one deterministic forward direction.
            if(originalHeading.sqrMagnitude<.0001f)
                originalHeading=Vector3.forward;
            originalHeading.Normalize();
            var stageHeading=Quaternion.AngleAxis(18f,Vector3.up)*
                originalHeading;
            const float elevationDegrees=36f;
            float elevation=elevationDegrees*Mathf.Deg2Rad;
            var away=stageHeading*Mathf.Cos(elevation)+
                Vector3.up*Mathf.Sin(elevation);
            camera.orthographic=false;
            camera.usePhysicalProperties=false;
            camera.fieldOfView=38f;
            camera.transform.rotation=Quaternion.LookRotation(
                -away,Vector3.up);
            var right=camera.transform.right;
            var up=camera.transform.up;
            var f=camera.transform.forward;
            var ext=b.extents;
            float dx=Mathf.Abs(right.x)*ext.x+Mathf.Abs(right.y)*ext.y+
                Mathf.Abs(right.z)*ext.z;
            float dy=Mathf.Abs(up.x)*ext.x+Mathf.Abs(up.y)*ext.y+
                Mathf.Abs(up.z)*ext.z;
            float dz=Mathf.Abs(f.x)*ext.x+Mathf.Abs(f.y)*ext.y+
                Mathf.Abs(f.z)*ext.z;
            float tanY=Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f);
            float tanX=tanY*Mathf.Max(.3f,camera.aspect);
            // The genuine facade and its silhouette must fit even on the
            // narrowest 390px mobile: a generous 70% cinema-safe margin
            // plus depth, no digital zoom / pixelated fake improvement.
            float distance=Mathf.Max(
                dy/(Mathf.Max(.01f,tanY)*.73f),
                dx/(Mathf.Max(.01f,tanX)*.73f))+dz*1.5f;
            distance=Mathf.Max(distance,8f);
            camera.transform.position=b.center+away*distance;
            camera.nearClipPlane=.12f;
            camera.farClipPlane=Mathf.Max(
                camera.farClipPlane,distance+ext.magnitude*7f);
        }

        static void DestroyTemporary(UnityEngine.Object obj)
        {
            if(!obj)return;
            if(UnityEngine.Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else UnityEngine.Object.DestroyImmediate(obj);
        }

        public static RenderTexture Capture(float panelWidth,float panelHeight)
        {
            if(!Resolve(out var source,out var bounds))return null;
            bool portrait=panelHeight>panelWidth;
            // Render the ACTUAL city at the same shape as the visible
            // cathedral-sized ceremony, not a 3:2 fixed photo cropped again
            // inside a 1:2 portrait. This also prevents GPU upscale blur
            // after expanding the genuine full-world modal on desktop.
            var frame=DailyTreasureStageDirection.NarrativeTheatreFrame(
                panelWidth,panelHeight);
            float screenAspect=Mathf.Max(.35f,
                (frame.Width-25f)/Mathf.Max(1f,frame.Height-28f));
            int height=portrait?(panelWidth>=650f?1152:1024):
                (panelWidth>=950f?720:540);
            int width=Mathf.Clamp(Mathf.RoundToInt(
                height*screenAspect),500,1360);
            RenderTexture result=null;
            GameObject cameraObject=null,lightObject=null;
            var previous=RenderTexture.active;
            try
            {
                // One original-source render, not an enlarged low-resolution
                // PNG. Copy the ACTUAL player camera but never move that
                // camera, touch the player's construction state or save.
                cameraObject=new GameObject("Daily treasure / temporary genuine Bastion lens");
                var camera=cameraObject.AddComponent<Camera>();
                camera.CopyFrom(source);
                camera.transform.SetPositionAndRotation(
                    source.transform.position,source.transform.rotation);
                camera.enabled=false;
                camera.rect=new Rect(0f,0f,1f,1f);
                camera.aspect=(float)width/height;
                camera.cullingMask=source.cullingMask & ~(1<<5);
                camera.allowHDR=true;
                camera.allowMSAA=true;
                Frame(camera,bounds);
                // The entire Eldoria project runs URP 17.3.0. Unity's
                // Camera.Render() is NOT the supported out-of-loop route in
                // the Scriptable Render Pipeline. Use the official Unity 6
                // SingleCameraRequest API and a detached Base Camera.
                // Preserve the real world camera's physical scene settings,
                // but never modify that active camera or its gameplay stack.
                var originalData=source.GetComponent<UniversalAdditionalCameraData>();
                var renderData=camera.GetUniversalAdditionalCameraData();
                renderData.renderType=CameraRenderType.Base;
                if(originalData)
                {
                    renderData.renderPostProcessing=originalData.renderPostProcessing;
                    renderData.renderShadows=originalData.renderShadows;
                    renderData.volumeLayerMask=originalData.volumeLayerMask;
                    renderData.antialiasing=originalData.antialiasing;
                }
                camera.targetTexture=null;
                result=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32)
                {
                    name="TRUE Bastion / one-frame authenticated milestone",
                    filterMode=FilterMode.Bilinear,
                    wrapMode=TextureWrapMode.Clamp,
                    antiAliasing=2
                };
                result.Create();
                camera.targetTexture=result;
                // One broad warm bounce, never a globally applied light or
                // violet/neon firework. Created/destroyed synchronously.
                lightObject=new GameObject("Daily treasure / temporary amber relief");
                var light=lightObject.AddComponent<Light>();
                light.type=LightType.Point;
                light.color=new Color(1f,.81f,.57f);
                light.range=Mathf.Max(8f,bounds.size.magnitude*2.2f);
                light.intensity=.82f;
                light.shadows=LightShadows.None;
                light.cullingMask=camera.cullingMask;
                // A single amber key from the camera's front-left
                // reveals original masonry relief, not imaginary VFX.
                lightObject.transform.position=bounds.center+
                    camera.transform.right*(-bounds.extents.magnitude*.72f)+
                    Vector3.up*(bounds.extents.magnitude*.95f)-
                    camera.transform.forward*(bounds.extents.magnitude*.86f);
                // Unity 6000.3 URP manual explicitly requires a supported
                // SingleCameraRequest for an outside-loop snapshot. A live
                // camera.Render call may compile yet never paint a frame in
                // SRP; that explains the old black/empty premium Bastion.
                var request=new UniversalRenderPipeline.SingleCameraRequest();
                if(!RenderPipeline.SupportsRenderRequest(camera,request))
                    throw new InvalidOperationException(
                        "URP_SNAPSHOT_UNSUPPORTED: genuine Bastion not captured");
                request.destination=result;
                RenderPipeline.SubmitRenderRequest(camera,request);
                return result;
            }
            catch(Exception error)
            {
                Debug.LogWarning("REAL_BASTION_CAMERA_UNAVAILABLE "+error.Message);
                if(result){result.Release();DestroyTemporary(result);}
                return null;
            }
            finally
            {
                RenderTexture.active=previous;
                DestroyTemporary(cameraObject);
                DestroyTemporary(lightObject);
            }
        }
        public static void Release(RenderTexture texture)
        {
            if(!texture)return;
            if(RenderTexture.active==texture)RenderTexture.active=null;
            texture.Release();
            DestroyTemporary(texture);
        }
    }
}

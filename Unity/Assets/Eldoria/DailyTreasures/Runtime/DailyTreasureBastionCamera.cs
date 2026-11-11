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

        // The saved city supports TWO genuine views, and tier V has a
        // different ACTIVE real model, not BastionTwo! The old resolver
        // found the tier-I/II city owner only; at Bastion V the progressive
        // world presentation explicitly disables those legacy renderers,
        // leading to a black stage. Read only LIVE existing meshes.
        // These scene hosts are in the foreign Assembly-CSharp; reflection
        // is necessary from the isolated DailyTreasures asmdef.
        static object ReadMember(object target,string member)
        {
            if(target==null)return null;
            var type=target.GetType();
            return type.GetProperty(member,
                BindingFlags.Instance|BindingFlags.Public)
                ?.GetValue(target,null) ??
                type.GetField(member,BindingFlags.Instance|
                    BindingFlags.Public)?.GetValue(target);
        }

        static bool TryActiveBounds(GameObject root,Camera camera,
            out Bounds result)
        {
            result=default;
            if(!root || !root.activeInHierarchy || !camera ||
                root.scene!=camera.gameObject.scene)return false;
            bool any=false;
            foreach(var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if(!r || !r.enabled || !r.gameObject.activeInHierarchy)
                    continue;
                if((camera.cullingMask&(1<<r.gameObject.layer))==0)
                    continue;
                if(!any){result=r.bounds;any=true;}
                else result.Encapsulate(r.bounds);
            }
            return any && result.size.sqrMagnitude>.01f;
        }

        static bool TryCaptureOwner(object owner,Camera camera,
            string[] slots,out Bounds bounds)
        {
            bounds=default;
            if(owner==null||!camera)return false;
            foreach(var name in slots)
            {
                var building=ReadMember(owner,name) as GameObject;
                if(TryActiveBounds(building,camera,out bounds))return true;
            }
            return false;
        }

        static bool Resolve(out Camera source,out Bounds bounds)
        {
            source=null;bounds=default;
            var components=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            // FIRST: true playable owner. It chooses between city and region
            // and can display the real level-V Bastion via BastionIVVWorld-
            // Presentation, replacing WorldBastionTwo or City.BastionTwo.
            // Never show a tier-II building as the milestone for level V.
            foreach(var item in components)
            {
                if(!item || !item.isActiveAndEnabled ||
                   item.GetType().Name!="ValoriaIntegratedWorld" ||
                   !item.gameObject.scene.isLoaded)continue;
                var ready=ReadMember(item,"Ready");
                if(!(ready is bool) || !(bool)ready)continue;
                var camera=ReadMember(item,"PlayerCamera") as Camera;
                if(!camera || !camera.gameObject.scene.isLoaded)continue;
                bool inWorld=ReadMember(item,"InWorld") is bool value && value;
                MonoBehaviour progression=null;
                foreach(var component in item.GetComponents<MonoBehaviour>())
                    if(component &&
                       component.GetType().Name=="BastionIVVWorldPresentation")
                    {progression=component;break;}
                if(progression && progression.isActiveAndEnabled)
                {
                    // WorldModel is the genuine active tier-V render. In the
                    // city the real replacement is ModelFor("bastion").
                    GameObject tierFive=null;
                    if(inWorld)
                        tierFive=ReadMember(progression,"WorldModel") as GameObject;
                    else
                    {
                        var method=progression.GetType().GetMethod("ModelFor",
                            BindingFlags.Instance|BindingFlags.Public);
                        tierFive=method?.Invoke(progression,
                            new object[]{"bastion"}) as GameObject;
                    }
                    if(TryActiveBounds(tierFive,camera,out bounds))
                    {source=camera;return true;}
                }
                if(inWorld)
                {
                    // Real owned world geometry, not invented fake castle.
                    if(TryCaptureOwner(item,camera,
                       new[]{"WorldBastionTwo","WorldBastionOne"},out bounds))
                    {source=camera;return true;}
                }
                else
                {
                    var city=ReadMember(item,"City");
                    if(TryCaptureOwner(city,camera,
                       new[]{"BastionTwo","BastionOne"},out bounds))
                    {source=camera;return true;}
                }
            }
            // SECOND: native editor's genuine standalone city scene without
            // an initialized integrated world. Retain existing validated
            // test coverage and original active player camera.
            foreach(var item in components)
            {
                if(!item || !item.isActiveAndEnabled ||
                   item.GetType().Name!="ValoriaReferenceCleanGameplay" ||
                   !item.gameObject.scene.isLoaded)continue;
                var camera=ReadMember(item,"PlayerCamera") as Camera;
                if(TryCaptureOwner(item,camera,
                   new[]{"BastionTwo","BastionOne"},out bounds))
                {source=camera;return true;}
            }
            Debug.LogWarning(
                "TRUE_BASTION_NOT_ACTIVE: cannot manufacture ceremonial architecture");
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

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

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

        static void Frame(Camera camera,Bounds b)
        {
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
            float aspect=Mathf.Max(.3f,camera.aspect);
            if(camera.orthographic)
            {
                camera.orthographicSize=Mathf.Max(1f,
                    Mathf.Max(dy/.66f,dx/(aspect*.72f)));
                camera.transform.position=b.center-f*Mathf.Max(12f,dz*3f);
            }
            else
            {
                float tanY=Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f);
                float distance=Mathf.Max(dy/(tanY*.66f),
                    dx/(tanY*aspect*.72f))+dz*1.5f;
                camera.transform.position=b.center-f*Mathf.Max(12f,distance);
            }
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
            int width=portrait?600:960;
            int height=portrait?900:540;
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
                // Keep existing URP camera semantics without creating a
                // dependency on a foreign workstream or new Unity package.
                foreach(var component in source.GetComponents<Behaviour>())
                    if(component && component.GetType().Name==
                        "UniversalAdditionalCameraData")
                        cameraObject.AddComponent(component.GetType());
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
                lightObject.transform.position=bounds.center+
                    source.transform.up*bounds.extents.magnitude*.8f-
                    source.transform.forward*bounds.extents.magnitude*.9f;
                camera.Render();
                camera.targetTexture=null;
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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Original BreachWatch portrait. Only the selected REAL building's enabled
    /// meshes/materials are cloned into a temporary isolated render layer.
    /// The main city, cameras, lighting and saves are never modified.
    /// No RoyalHUD, Forja or generic fantasy replacement thumbnail is used.
    /// </summary>
    public sealed class BreachWatchBuildingPortrait : MonoBehaviour
    {
        const int PreviewLayer = 30;
        RawImage image;
        RenderTexture texture;
        GameObject cameraRoot;
        Camera portraitCamera;
        GameObject lastModel;
        string lastKey;

        public bool HasLiveBuilding => image && image.enabled && image.texture == texture && lastModel;
        public bool RenderedFromIsolatedRealMeshes { get; private set; }
        public float NonBackgroundPixelFraction { get; private set; }
        public void Initialize(RawImage target)
        {
            image = target;
            if(image)
            {
                image.raycastTarget = false;
                image.enabled = false;
            }
        }

        public void Show(GameObject model, Camera source, string key)
        {
            if(!image)return;
            if(!model || !source || !model.activeInHierarchy)
            {
                image.enabled = false;
                lastModel = null;
                lastKey = null;
                return;
            }
            if(model == lastModel && lastKey == key && HasLiveBuilding)return;
            RenderedFromIsolatedRealMeshes=false;
            NonBackgroundPixelFraction=0f;
            if(!texture)
            {
                texture = new RenderTexture(512,256,24,RenderTextureFormat.ARGB32);
                texture.name = "Vigilia / selected real building portrait";
                texture.antiAliasing = 2;
                texture.Create();
            }
            if(!portraitCamera)
            {
                cameraRoot = new GameObject("Vigilia / independent portrait camera");
                cameraRoot.hideFlags = HideFlags.HideAndDontSave;
                portraitCamera = cameraRoot.AddComponent<Camera>();
                portraitCamera.enabled = false;
            }

            // A disposable render-only replica avoids photographing the city's
            // grass, neighbours and particles. The genuine building is untouched.
            var stage = new GameObject("Vigilia / one-shot genuine selected building");
            stage.hideFlags = HideFlags.HideAndDontSave;
            stage.transform.position = new Vector3(0f,-10000f,0f);
            var bakedMeshes = new List<Mesh>();
            bool hasGeometry = false;
            Bounds naturalBounds = default;
            try
            {
                void IncludeBounds(Bounds b)
                {
                    if(b.size.sqrMagnitude<.00001f)return;
                    if(!hasGeometry){naturalBounds=b;hasGeometry=true;}
                    else naturalBounds.Encapsulate(b);
                }

                var originals=model.GetComponentsInChildren<Renderer>(true);
                foreach(var renderer in originals)
                {
                    if(!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy)continue;
                    if(renderer is MeshRenderer mr)
                    {
                        var filter=mr.GetComponent<MeshFilter>();
                        if(filter && filter.sharedMesh)IncludeBounds(mr.bounds);
                    }
                    else if(renderer is SkinnedMeshRenderer skin && skin.sharedMesh)
                        IncludeBounds(skin.bounds);
                }
                if(!hasGeometry)
                {
                    image.enabled = false;
                    lastModel = null;
                    lastKey = null;
                    return;
                }

                int rendered = 0;
                Bounds copiedBounds = default;
                foreach(var original in originals)
                {
                    if(!original || !original.enabled || !original.gameObject.activeInHierarchy)continue;
                    Mesh mesh = null;
                    if(original is MeshRenderer meshRenderer)
                    {
                        var filter=meshRenderer.GetComponent<MeshFilter>();
                        if(filter)mesh=filter.sharedMesh;
                    }
                    else if(original is SkinnedMeshRenderer skinRenderer && skinRenderer.sharedMesh)
                    {
                        mesh=new Mesh { name = "Vigilia / disposable posed skin" };
                        skinRenderer.BakeMesh(mesh);
                        bakedMeshes.Add(mesh);
                    }
                    if(!mesh)continue;
                    var part=new GameObject("Original geometry / isolated copy",typeof(MeshFilter),typeof(MeshRenderer));
                    part.hideFlags=HideFlags.HideAndDontSave;
                    part.layer=PreviewLayer;
                    part.transform.SetParent(stage.transform,false);
                    // Preserve original geometry and world rotations; only move
                    // the preview into the invisible isolated stage.
                    part.transform.position=stage.transform.position +
                        (original.transform.position-naturalBounds.center);
                    part.transform.rotation=original.transform.rotation;
                    part.transform.localScale=original.transform.lossyScale;
                    part.GetComponent<MeshFilter>().sharedMesh=mesh;
                    var render=part.GetComponent<MeshRenderer>();
                    render.sharedMaterials=original.sharedMaterials;
                    render.shadowCastingMode=ShadowCastingMode.Off;
                    render.receiveShadows=false;
                    render.lightProbeUsage=LightProbeUsage.Off;
                    render.reflectionProbeUsage=ReflectionProbeUsage.Off;
                    var properties=new MaterialPropertyBlock();
                    original.GetPropertyBlock(properties);
                    if(!properties.isEmpty)render.SetPropertyBlock(properties);
                    if(rendered++==0)copiedBounds=render.bounds;
                    else copiedBounds.Encapsulate(render.bounds);
                }
                if(rendered==0)
                {
                    image.enabled=false;lastModel=null;lastKey=null;
                    return;
                }

                var keyLight=new GameObject("Isolated miniature warm key",typeof(Light));
                keyLight.hideFlags=HideFlags.HideAndDontSave;
                keyLight.transform.SetParent(stage.transform,false);
                var light=keyLight.GetComponent<Light>();
                light.type=LightType.Directional;
                light.cullingMask=1<<PreviewLayer;
                light.intensity=1.25f;
                light.color=new Color(1f,.86f,.72f);
                light.shadows=LightShadows.None;
                keyLight.transform.rotation=Quaternion.Euler(50,-38,0);

                portraitCamera.CopyFrom(source);
                portraitCamera.enabled=false;
                portraitCamera.clearFlags=CameraClearFlags.SolidColor;
                portraitCamera.backgroundColor=new Color(.07f,.078f,.084f,1);
                portraitCamera.cullingMask=1<<PreviewLayer;
                portraitCamera.allowHDR=false;
                portraitCamera.aspect=texture.width/(float)texture.height;
                portraitCamera.orthographic=true;
                portraitCamera.targetTexture=texture;
                portraitCamera.transform.rotation=source.transform.rotation;

                Vector3 right=portraitCamera.transform.right;
                Vector3 up=portraitCamera.transform.up;
                float maxX=0f,maxY=0f;
                var min=copiedBounds.min;
                var max=copiedBounds.max;
                for(int i=0;i<8;i++)
                {
                    var corner=new Vector3((i&1)!=0?max.x:min.x,
                        (i&2)!=0?max.y:min.y,(i&4)!=0?max.z:min.z);
                    var local=corner-copiedBounds.center;
                    maxX=Mathf.Max(maxX,Mathf.Abs(Vector3.Dot(local,right)));
                    maxY=Mathf.Max(maxY,Mathf.Abs(Vector3.Dot(local,up)));
                }
                portraitCamera.orthographicSize=Mathf.Max(.5f,
                    Mathf.Max(maxY,maxX/portraitCamera.aspect)*1.20f);
                portraitCamera.nearClipPlane=.1f;
                portraitCamera.farClipPlane=1000;
                portraitCamera.transform.position=copiedBounds.center -
                    portraitCamera.transform.forward*120f;

                // Actual separately rendered meshes must be inside the current
                // portrait frame. No synthetic values and no image generation.
                portraitCamera.Render();
#if UNITY_EDITOR
                // A source mesh count does NOT prove the image looks like a
                // building. Count genuinely rendered pixels after the camera.
                var previousActive=RenderTexture.active;
                var readback=new Texture2D(texture.width,texture.height,
                    TextureFormat.RGB24,false);
                try
                {
                    RenderTexture.active=texture;
                    readback.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);
                    readback.Apply();
                    var pixels=readback.GetPixels32();
                    int nonBackground=0;
                    foreach(var p in pixels)
                        if(Mathf.Abs(p.r-18)+Mathf.Abs(p.g-20)+Mathf.Abs(p.b-21)>40)
                            nonBackground++;
                    NonBackgroundPixelFraction=nonBackground/(float)pixels.Length;
                }
                finally
                {
                    RenderTexture.active=previousActive;
                    Destroy(readback);
                }
#endif
                portraitCamera.targetTexture=null;
                image.texture=texture;
                image.enabled=true;
                lastModel=model;
                lastKey=key;
                RenderedFromIsolatedRealMeshes=rendered>0 &&
                    portraitCamera.cullingMask==(1<<PreviewLayer);
            }
            finally
            {
                stage.SetActive(false);
                Destroy(stage);
                foreach(var mesh in bakedMeshes)if(mesh)Destroy(mesh);
                if(portraitCamera)portraitCamera.targetTexture=null;
            }
        }
        void OnDestroy()
        {
            if(cameraRoot)Destroy(cameraRoot);
            if(texture){texture.Release();Destroy(texture);}
        }
    }
}

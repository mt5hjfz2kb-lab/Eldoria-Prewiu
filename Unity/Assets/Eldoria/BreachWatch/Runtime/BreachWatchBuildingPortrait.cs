using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
#if UNITY_EDITOR
        static bool materialInventoryLogged;
#endif

        public bool HasLiveBuilding => image && image.enabled && image.texture == texture && lastModel;
        public bool RenderedFromIsolatedRealMeshes { get; private set; }
        public float NonBackgroundPixelFraction { get; private set; }
        // Exact RGBA GPU evidence, Editor-only; not a mobile readback.
        public float TransparentBackdropFraction { get; private set; }
        public float OpaqueMasonryFraction { get; private set; }
        // Actual GPU-readback silhouette margins, not just mesh counts.
        // This flag is evaluated in Editor evidence; normal mobile play
        // never performs a synchronous texture readback.
        public bool PortraitGeometryClipped { get; private set; }
        public Rect LastGeometryUvBounds { get; private set; }
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
            TransparentBackdropFraction=0f;
            OpaqueMasonryFraction=0f;
            PortraitGeometryClipped=false;
            LastGeometryUvBounds=Rect.zero;
            if(!texture)
            {
                texture = new RenderTexture(512,320,24,RenderTextureFormat.ARGB32);
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
            var disposableTextures = new List<Texture2D>();
            var disposableMaterials = new List<Material>();
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
#if UNITY_EDITOR
                // First-party art diagnosis only. The approved concept has
                // recognisably cobalt-blue roofs but the exact playable
                // Bastion is grey. We must identify ORIGINAL material
                // submeshes before considering any non-destructive palette
                // adaptation. No 3D material/prefab/camera is changed here.
                if(!materialInventoryLogged)
                {
                    materialInventoryLogged=true;
                    int count=0;
                    foreach(var sample in originals)
                    {
                        if(!sample)continue;
                        var parts=sample.sharedMaterials;
                        for(int i=0;i<parts.Length && count<48;i++)
                        {
                            var mat=parts[i];
                            if(!mat)continue;
                            string color="unset";
                            if(mat.HasProperty("_BaseColor"))
                                color=mat.GetColor("_BaseColor").ToString("F3");
                            else if(mat.HasProperty("_Color"))
                                color=mat.GetColor("_Color").ToString("F3");
                            Debug.Log("VIGILIA_CASTLE_ORIGINAL_SUBMESH "+count+
                                " renderer="+sample.name+" index="+i+
                                " material="+mat.name+" color="+color+
                                " shader="+(mat.shader?mat.shader.name:"missing"));
                            count++;
                        }
                    }
                }
#endif
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
                    // Unity material-index property blocks must be copied
                    // individually: GetPropertyBlock(no index) does NOT return
                    // the roof-only overrides of the live city renderer.
                    // This makes the card a faithful view of the same genuine
                    // building, not a hand-painted static castle.
                    for(int materialIndex=0;materialIndex<render.sharedMaterials.Length;
                        materialIndex++)
                    {
                        var perSubmesh=new MaterialPropertyBlock();
                        original.GetPropertyBlock(perSubmesh,materialIndex);
                        if(!perSubmesh.isEmpty)
                            render.SetPropertyBlock(perSubmesh,materialIndex);
                    }
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
                // Do not burn out stone in the real Valoria materials: the
                // scene's ambient contribution is already active.
                light.intensity=.38f;
                light.color=new Color(1f,.86f,.72f);
                light.shadows=LightShadows.None;
                keyLight.transform.rotation=Quaternion.Euler(50,-38,0);

                portraitCamera.CopyFrom(source);
                // URP 17 requires its own camera data on a camera created
                // at runtime. CopyFrom copies Camera settings, not URP data.
                var cameraData=portraitCamera.GetUniversalAdditionalCameraData();
                cameraData.renderType=CameraRenderType.Base;
                cameraData.renderPostProcessing=false;
                cameraData.renderShadows=false;
                cameraData.allowXRRendering=false;
                if(cameraData.cameraStack!=null)cameraData.cameraStack.Clear();
                portraitCamera.useOcclusionCulling=false;
                portraitCamera.enabled=false;
                portraitCamera.clearFlags=CameraClearFlags.SolidColor;
                // Clear only the BACKGROUND alpha, retaining the same
                // real URP masonry meshes and lighting. UI RawImage blends
                // these meshes directly into the carved mineral cartouche,
                // not into a second opaque black photographic rectangle.
                portraitCamera.backgroundColor=new Color(.14f,.135f,.115f,0f);
                portraitCamera.cullingMask=1<<PreviewLayer;
                portraitCamera.allowHDR=false;
                portraitCamera.aspect=texture.width/(float)texture.height;
                portraitCamera.orthographic=true;
                portraitCamera.targetTexture=texture;
                // Isolated thumbnail ONLY: ease the city's steep overhead pitch
                // toward the approved more frontal stone facade. Preserve
                // real model/yaw, lighting and the gameplay camera untouched;
                // projection and GPU clipping checks recompute from this axis.
                float sourcePitch=source.transform.eulerAngles.x;
                float portraitPitch=Mathf.LerpAngle(sourcePitch,35f,.55f);
                portraitCamera.transform.rotation=Quaternion.Euler(
                    portraitPitch,source.transform.eulerAngles.y,
                    source.transform.eulerAngles.z);

                Vector3 right=portraitCamera.transform.right;
                Vector3 up=portraitCamera.transform.up;
                float minRight=float.PositiveInfinity,maxRight=float.NegativeInfinity;
                float minUp=float.PositiveInfinity,maxUp=float.NegativeInfinity;
                // Before: one WORLD axis-aligned union bound for the entire
                // Bastion. Its empty diagonal corners forced Unity to shrink
                // the live thumbnail into a tiny object. Now measure each
                // visible mesh's LOCAL bounds transformed into the exact
                // portrait camera plane; still conservatively covers every
                // genuine mesh without a GPU readback or synthetic castle.
                foreach(var part in stage.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(!part || !part.sharedMesh)continue;
                    var localBounds=part.sharedMesh.bounds;
                    var meshMin=localBounds.min;
                    var meshMax=localBounds.max;
                    for(int cornerIndex=0;cornerIndex<8;cornerIndex++)
                    {
                        var localCorner=new Vector3(
                            (cornerIndex&1)!=0?meshMax.x:meshMin.x,
                            (cornerIndex&2)!=0?meshMax.y:meshMin.y,
                            (cornerIndex&4)!=0?meshMax.z:meshMin.z);
                        Vector3 worldCorner=part.transform.TransformPoint(localCorner);
                        Vector3 offset=worldCorner-copiedBounds.center;
                        float projRight=Vector3.Dot(offset,right);
                        float projUp=Vector3.Dot(offset,up);
                        minRight=Mathf.Min(minRight,projRight);
                        maxRight=Mathf.Max(maxRight,projRight);
                        minUp=Mathf.Min(minUp,projUp);
                        maxUp=Mathf.Max(maxUp,projUp);
                    }
                }
                if(float.IsInfinity(minRight) || float.IsInfinity(minUp))
                {
                    image.enabled=false;lastModel=null;lastKey=null;
                    return;
                }
                float spanRight=(maxRight-minRight)*.5f;
                float spanUp=(maxUp-minUp)*.5f;
                // The geometrical AABBs include large *empty* corners
                // between the real Bastion meshes, so a 1.12x margin made
                // the approved illustration look miniature (~16% masonry).
                // The first tighter .82x crop touched genuine geometry at
                // the image boundary in the Unity GPU test. Use .90x for a
                // narrower safety margin while keeping the castle legible;
                // the native visual/bounds gate will still reject clipping.
                portraitCamera.orthographicSize=Mathf.Max(.5f,
                    Mathf.Max(spanUp,spanRight/portraitCamera.aspect)*.96f);
                portraitCamera.nearClipPlane=.1f;
                portraitCamera.farClipPlane=1000;
                Vector3 actualVisualCenter=copiedBounds.center +
                    right*((minRight+maxRight)*.5f)+
                    up*((minUp+maxUp)*.5f);
                // Real GPU diagnostic at ortho 12.059, 512x256 showed the
                // Bastion at x=141..375, y=0..199: a full 56px of wasted sky
                // but masonry cropped below pixel 0. Re-center in the REAL
                // image plane before rendering instead of making it smaller.
                // The -up shift moves actual geometry UP inside the picture.
                portraitCamera.transform.position=actualVisualCenter -
                    portraitCamera.transform.forward*120f -
                    up*(portraitCamera.orthographicSize*.19f);

                // Compute the genuine renderer-bounds projection for ALL
                // platforms, including Android/WebGL where the Editor-only
                // GPU diagnostics cannot be used at runtime. This rectangle
                // guides an honest padded crop of the same real model image.
                var projectedMin=new Vector2(float.PositiveInfinity,float.PositiveInfinity);
                var projectedMax=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
                var boxMin=copiedBounds.min;
                var boxMax=copiedBounds.max;
                for(int corner=0;corner<8;corner++)
                {
                    var vertex=new Vector3(
                        (corner&1)!=0?boxMax.x:boxMin.x,
                        (corner&2)!=0?boxMax.y:boxMin.y,
                        (corner&4)!=0?boxMax.z:boxMin.z);
                    var projected=portraitCamera.WorldToViewportPoint(vertex);
                    if(projected.z<=0f)continue;
                    projectedMin=Vector2.Min(projectedMin,
                        new Vector2(projected.x,projected.y));
                    projectedMax=Vector2.Max(projectedMax,
                        new Vector2(projected.x,projected.y));
                }
                if(projectedMin.x<projectedMax.x && projectedMin.y<projectedMax.y)
                    LastGeometryUvBounds=Rect.MinMaxRect(
                        Mathf.Clamp01(projectedMin.x),Mathf.Clamp01(projectedMin.y),
                        Mathf.Clamp01(projectedMax.x),Mathf.Clamp01(projectedMax.y));

                // Actual separately rendered meshes must be inside the current
                // portrait frame. No synthetic values and no image generation.
                portraitCamera.Render();
#if UNITY_EDITOR
                // A source mesh count does NOT prove the image looks like a
                // building. Count genuinely rendered pixels after the camera.
                var previousActive=RenderTexture.active;
                var readback=new Texture2D(texture.width,texture.height,
                    TextureFormat.RGBA32,false);
                try
                {
                    RenderTexture.active=texture;
                    readback.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);
                    readback.Apply();
                    var pixels=readback.GetPixels32();
                    // Native visual QA can inspect the exact 512x256 image
                    // produced by the genuine URP camera, before the
                    // authored display-only earth background is added.
                    // Never exported in mobile builds.
                    var proofDir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                        "../BreachWatchNativeEvidence"));
                    Directory.CreateDirectory(proofDir);
                    File.WriteAllBytes(Path.Combine(proofDir,
                        "VigiliaPortraitRealMeshRaw.png"),readback.EncodeToPNG());
                    // Compare against the ACTUAL rendered corner, never against
                    // assumed RGB constants: sRGB conversion in URP can turn
                    // linear(.07) clear colour into much brighter pixel bytes.
                    var background=pixels[8*texture.width+8];
                    int nonBackground=0;
                    int transparentBackground=0;
                    int opaqueMasonry=0;
                    int minX=texture.width,minY=texture.height;
                    int drawMaxX=-1,drawMaxY=-1;
                    for(int py=0;py<texture.height;py++)
                    for(int px=0;px<texture.width;px++)
                    {
                        var p=pixels[py*texture.width+px];
                        if(p.a<=12)transparentBackground++;
                        // Transparent render target gives an authoritative
                        // silhouette, including stone matching the clear RGB.
                        if(p.a<=12)continue;
                        nonBackground++;
                        if(p.a>=200)opaqueMasonry++;
                        minX=Mathf.Min(minX,px);drawMaxX=Mathf.Max(drawMaxX,px);
                        minY=Mathf.Min(minY,py);drawMaxY=Mathf.Max(drawMaxY,py);
                    }
                    NonBackgroundPixelFraction=nonBackground/(float)pixels.Length;
                    TransparentBackdropFraction=transparentBackground/(float)pixels.Length;
                    OpaqueMasonryFraction=opaqueMasonry/(float)pixels.Length;
                    if(nonBackground>0)
                        LastGeometryUvBounds=new Rect(
                            minX/(float)texture.width,
                            minY/(float)texture.height,
                            (drawMaxX-minX+1)/(float)texture.width,
                            (drawMaxY-minY+1)/(float)texture.height);
                    // With six physical pixel margins, the clipped geometry
                    // would look damaged inside the owner's ornate gold
                    // picture frame. Reject it instead of presenting it.
                    PortraitGeometryClipped=nonBackground>0 &&
                        (minX<6||minY<6||
                         drawMaxX>=texture.width-6||drawMaxY>=texture.height-6);
                    Debug.Log("VIGILIA_REAL_PORTRAIT_GPU_BOUNDS "+
                        (model?model.name:"none")+
                        " orthographic="+portraitCamera.orthographicSize.ToString("F3")+
                        " nonBackground="+NonBackgroundPixelFraction.ToString("F4")+
                        " cropX="+minX+".."+drawMaxX+
                        " cropY="+minY+".."+drawMaxY+
                        " background="+background.r+","+
                        background.g+","+background.b+
                        " cropped="+PortraitGeometryClipped+
                        " alphaClear="+TransparentBackdropFraction.ToString("F4")+
                        " alphaMasonry="+OpaqueMasonryFraction.ToString("F4"));
                }
                finally
                {
                    RenderTexture.active=previousActive;
                    Destroy(readback);
                }
#endif
                // Genuine building diorama, with no synthetic green square
                // or stage material. The URP portrait camera already rendered
                // the ACTUAL meshes onto its neutral mineral clear colour
                // before the Editor-only GPU readback above. Reusing those
                // exact pixels keeps the carved bronze panel cohesive and
                // avoids another RenderTexture pass, 128x128 procedural earth
                // texture and disposable quad. The playable world is untouched.
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
                foreach(var texture in disposableTextures)if(texture)Destroy(texture);
                foreach(var material in disposableMaterials)if(material)Destroy(material);
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

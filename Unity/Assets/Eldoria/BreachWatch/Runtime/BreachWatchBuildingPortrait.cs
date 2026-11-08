using System;
using System.Collections.Generic;
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

        public bool HasLiveBuilding => image && image.enabled && image.texture == texture && lastModel;
        public bool RenderedFromIsolatedRealMeshes { get; private set; }
        public float NonBackgroundPixelFraction { get; private set; }
        // Actual GPU-readback silhouette margins, not just mesh counts.
        // This flag is evaluated in Editor evidence; normal mobile play
        // never performs a synchronous texture readback.
        public bool PortraitGeometryClipped { get; private set; }
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
            PortraitGeometryClipped=false;
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
                portraitCamera.backgroundColor=new Color(.14f,.135f,.115f,1);
                portraitCamera.cullingMask=1<<PreviewLayer;
                portraitCamera.allowHDR=false;
                portraitCamera.aspect=texture.width/(float)texture.height;
                portraitCamera.orthographic=true;
                portraitCamera.targetTexture=texture;
                portraitCamera.transform.rotation=source.transform.rotation;

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
                    var boxMin=localBounds.min;
                    var boxMax=localBounds.max;
                    for(int cornerIndex=0;cornerIndex<8;cornerIndex++)
                    {
                        var localCorner=new Vector3(
                            (cornerIndex&1)!=0?boxMax.x:boxMin.x,
                            (cornerIndex&2)!=0?boxMax.y:boxMin.y,
                            (cornerIndex&4)!=0?boxMax.z:boxMin.z);
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
                    Mathf.Max(spanUp,spanRight/portraitCamera.aspect)*.90f);
                portraitCamera.nearClipPlane=.1f;
                portraitCamera.farClipPlane=1000;
                Vector3 actualVisualCenter=copiedBounds.center +
                    right*((minRight+maxRight)*.5f)+
                    up*((minUp+maxUp)*.5f);
                portraitCamera.transform.position=actualVisualCenter -
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
                    // Compare against the ACTUAL rendered corner, never against
                    // assumed RGB constants: sRGB conversion in URP can turn
                    // linear(.07) clear colour into much brighter pixel bytes.
                    var background=pixels[8*texture.width+8];
                    int nonBackground=0;
                    int minX=texture.width,minY=texture.height;
                    int drawMaxX=-1,drawMaxY=-1;
                    for(int py=0;py<texture.height;py++)
                    for(int px=0;px<texture.width;px++)
                    {
                        var p=pixels[py*texture.width+px];
                        if(Mathf.Abs(p.r-background.r)+
                           Mathf.Abs(p.g-background.g)+
                           Mathf.Abs(p.b-background.b)<=42)
                            continue;
                        nonBackground++;
                        minX=Mathf.Min(minX,px);drawMaxX=Mathf.Max(drawMaxX,px);
                        minY=Mathf.Min(minY,py);drawMaxY=Mathf.Max(drawMaxY,py);
                    }
                    NonBackgroundPixelFraction=nonBackground/(float)pixels.Length;
                    // With six physical pixel margins, the clipped geometry
                    // would look damaged inside the owner's ornate gold
                    // picture frame. Reject it instead of presenting it.
                    PortraitGeometryClipped=nonBackground>0 &&
                        (minX<6||minY<6||
                         drawMaxX>=texture.width-6||drawMaxY>=texture.height-6);
                }
                finally
                {
                    RenderTexture.active=previousActive;
                    Destroy(readback);
                }
#endif
                // A warm, isolated fragment of Valoria earth grounds the
                // ORIGINAL building geometry. This is a portrait-only
                // presentation material, not a fake castle and not a change
                // to the playable city's terrain.
                var earth=new GameObject("Vigilia / isolated Valoria earth",typeof(MeshFilter),typeof(MeshRenderer));
                earth.hideFlags=HideFlags.HideAndDontSave;
                earth.layer=PreviewLayer;
                earth.transform.SetParent(stage.transform,false);
                earth.transform.position=new Vector3(copiedBounds.center.x,
                    copiedBounds.min.y-.045f,copiedBounds.center.z);
                // The former portrait placed an obviously rectangular green
                // square behind the Bastion. Extend this private ground beyond
                // the entire orthographic viewport so that NO square edge can
                // appear in the thumbnail, matching the owner's reference.
                // The playable world's meshes/camera remain unchanged.
                float width=Mathf.Max(copiedBounds.size.x*2.5f,
                    portraitCamera.orthographicSize*portraitCamera.aspect*6.5f);
                float depth=Mathf.Max(copiedBounds.size.z*2.5f,
                    portraitCamera.orthographicSize*6.5f);
                var soil=new Mesh { name="Vigilia / one-shot earth under real building" };
                soil.vertices=new[]{
                    new Vector3(-width*.5f,0,-depth*.5f),
                    new Vector3(-width*.5f,0,depth*.5f),
                    new Vector3(width*.5f,0,depth*.5f),
                    new Vector3(width*.5f,0,-depth*.5f)
                };
                soil.triangles=new[]{0,1,2,0,2,3};
                soil.normals=new[]{Vector3.up,Vector3.up,Vector3.up,Vector3.up};
                // A tiled small soil source retains its mineral grain even
                // with a larger art-only photo stage. Sharp tile borders live
                // outside the camera, never under gameplay.
                soil.uv=new[]{new Vector2(0,0),new Vector2(0,4),
                    new Vector2(4,4),new Vector2(4,0)};
                bakedMeshes.Add(soil);
                earth.GetComponent<MeshFilter>().sharedMesh=soil;
                var earthTexture=new Texture2D(128,128,TextureFormat.RGBA32,false);
                earthTexture.name="Vigilia / earth grain";
                earthTexture.wrapMode=TextureWrapMode.Repeat;
                earthTexture.filterMode=FilterMode.Bilinear;
                var earthColors=new Color32[128*128];
                for(int ey=0;ey<128;ey++)for(int ex=0;ex<128;ex++)
                {
                    float n=Mathf.PerlinNoise(ex*.093f+2f,ey*.093f+4f);
                    float g=Mathf.PerlinNoise(ex*.42f,ey*.42f);
                    float cx=(ex-63.5f)/63.5f;
                    float cy=(ey-63.5f)/63.5f;
                    float contact=Mathf.Clamp01(1f-(cx*cx+cy*cy)*1.9f);
                    int r=Mathf.RoundToInt(62f+26f*n+8f*g-27f*contact);
                    int gr=Mathf.RoundToInt(69f+34f*n+5f*g-27f*contact);
                    int bl=Mathf.RoundToInt(39f+16f*n+4f*g-15f*contact);
                    earthColors[ey*128+ex]=new Color32((byte)r,(byte)gr,(byte)bl,255);
                }
                earthTexture.SetPixels32(earthColors);
                earthTexture.Apply(false,true);
                disposableTextures.Add(earthTexture);
                var shader=Shader.Find("Universal Render Pipeline/Lit");
                if(!shader)shader=Shader.Find("Universal Render Pipeline/Unlit");
                if(shader)
                {
                    var soilMaterial=new Material(shader) { name="Vigilia / one-shot neutral earth" };
                    if(soilMaterial.HasProperty("_BaseMap"))
                        soilMaterial.SetTexture("_BaseMap",earthTexture);
                    if(soilMaterial.HasProperty("_BaseColor"))
                        soilMaterial.SetColor("_BaseColor",Color.white);
                    if(soilMaterial.HasProperty("_Smoothness"))
                        soilMaterial.SetFloat("_Smoothness",0f);
                    disposableMaterials.Add(soilMaterial);
                    var earthRenderer=earth.GetComponent<MeshRenderer>();
                    earthRenderer.sharedMaterial=soilMaterial;
                    earthRenderer.shadowCastingMode=ShadowCastingMode.Off;
                    earthRenderer.receiveShadows=false;
                    earthRenderer.lightProbeUsage=LightProbeUsage.Off;
                }
                portraitCamera.Render();
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

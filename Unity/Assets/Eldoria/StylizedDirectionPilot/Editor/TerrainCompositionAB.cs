#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Eldoria.ReferenceClean;
using Eldoria.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace Eldoria.StylizedDirectionPilot.Editor
{
    /// <summary>
    /// TEMP EDITOR-ONLY: replace flat yellow terrain with a coherent stylized
    /// grass/earth atlas using original Unity Terrain layers and scene prefabs.
    /// No production scene save, no runtime changes, no imported mesh mutation.
    /// </summary>
    public static class TerrainCompositionAB
    {
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        [Serializable] sealed class Evidence {
            public string kind,sourceSha;
            public int sourceTerrainLayers,paintedDiffuseTextures,placedOriginalPrefabs;
            public float landscapeDeltaWide,landscapeDeltaPhone;
            public bool sceneSerialized,gameplayTested,artApproved,mobileTested;
        }
        public static void Capture()
        {
            string output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../StylizedDirectionPilotCaptures"));
            Directory.CreateDirectory(output);
            try
            {
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>(FindObjectsInactive.Include);
                var terrain=Object.FindFirstObjectByType<Terrain>(FindObjectsInactive.Include);
                if(!city||!city.PlayerCamera||!terrain)
                    throw new InvalidOperationException("Canonical city/camera/terrain not loaded");
                city.Apply(new PlayerState {BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
                var camera=city.PlayerCamera;
                string wideA=Path.Combine(output,"A-original-city-1600x900.png");
                string phoneA=Path.Combine(output,"A-original-city-390x844.png");
                string wideF=Path.Combine(output,"F-painted-landscape-1600x900.png");
                string phoneF=Path.Combine(output,"F-painted-landscape-390x844.png");
                Screenshot(camera,wideA,1600,900);
                Screenshot(camera,phoneA,390,844);
                int painted=0,layers=0,placed=0;
                var audit=new List<string>();
                using(var ground=new GroundPaint(terrain,city))
                {
                    painted=ground.Painted;layers=ground.Layers;
                    audit.AddRange(ground.Audit);
                    using(var decorations=StylizedCompositionStudy.Build(city,camera))
                    {
                        placed=decorations.Count;
                        audit.AddRange(decorations.Sources);
                        using(var lighting=new LightPass(camera))
                        {
                            Screenshot(camera,wideF,1600,900);
                            Screenshot(camera,phoneF,390,844);
                        }
                    }
                }
                float d1=Diff(wideA,wideF),d2=Diff(phoneA,phoneF);
                if(d1<6||d2<6||painted==0||placed<45)
                    throw new InvalidOperationException("Real-world visual effect too small: pixels "+
                        d1.ToString("F2")+"/"+d2.ToString("F2")+" placed="+placed);
                File.WriteAllLines(Path.Combine(output,"landscape-pilot-audit.txt"),audit);
                var e=new Evidence{
                    kind="TRUE_UNITY_EDITOR_CAMERA_REAL_TERRAIN_LAYER_A_B_NOT_GAMEPLAY",
                    sourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",
                    sourceTerrainLayers=layers,paintedDiffuseTextures=painted,
                    placedOriginalPrefabs=placed,landscapeDeltaWide=d1,landscapeDeltaPhone=d2,
                    sceneSerialized=false,gameplayTested=false,artApproved=false,mobileTested=false
                };
                File.WriteAllText(Path.Combine(output,"landscape-pilot-evidence.json"),JsonUtility.ToJson(e,true));
                Debug.Log("STYLIZED_LANDSCAPE_NATIVE_CAPTURE_OK");
                EditorApplication.Exit(0);
            }
            catch(Exception ex)
            {
                File.WriteAllText(Path.Combine(output,"landscape-pilot-error.txt"),ex.ToString());
                Debug.LogException(ex);EditorApplication.Exit(1);
            }
        }

        static void Screenshot(Camera cam,string path,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
            RenderTexture originalRT=cam.targetTexture,previous=RenderTexture.active;
            bool enabled=cam.enabled;float aspect=cam.aspect,size=cam.orthographicSize;
            try {
                cam.enabled=true;cam.aspect=(float)w/h;
                if(h>w)cam.orthographicSize=size*1.18f;
                cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply(false,false);
                var png=tex.EncodeToPNG();
                if(png.Length<40000)throw new InvalidOperationException("Empty Unity screenshot");
                File.WriteAllBytes(path,png);
            }
            finally {
                cam.targetTexture=originalRT;cam.enabled=enabled;cam.aspect=aspect;
                cam.orthographicSize=size;RenderTexture.active=previous;
                rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
            }
        }
        static float Diff(string left,string right)
        {
            var a=new Texture2D(2,2); var b=new Texture2D(2,2);
            try {
                if(!a.LoadImage(File.ReadAllBytes(left))||!b.LoadImage(File.ReadAllBytes(right))||
                   a.width!=b.width||a.height!=b.height)throw new IOException("A/B dimensions mismatch");
                var x=a.GetPixels32();var y=b.GetPixels32();double sum=0;int count=0;
                for(int i=0;i<x.Length;i+=19) {
                    sum+=(Math.Abs((int)x[i].r-y[i].r)+
                        Math.Abs((int)x[i].g-y[i].g)+Math.Abs((int)x[i].b-y[i].b))/3d;
                    count++;
                }
                return (float)(sum/Math.Max(1,count));
            }
            finally {Object.DestroyImmediate(a);Object.DestroyImmediate(b);}
        }
        sealed class LightPass:IDisposable
        {
            readonly Camera camera;
            readonly UniversalAdditionalCameraData additional;
            readonly bool previousHDR,previousPost;
            readonly GameObject root;
            readonly VolumeProfile profile;
            readonly List<Light> suns=new List<Light>();
            readonly List<Color> oldColors=new List<Color>();
            readonly List<float> oldStrengths=new List<float>();
            readonly Color ambient;readonly AmbientMode mode;
            public LightPass(Camera cam)
            {
                camera=cam;previousHDR=cam.allowHDR;
                additional=cam.GetUniversalAdditionalCameraData();
                previousPost=additional.renderPostProcessing;
                cam.allowHDR=true;additional.renderPostProcessing=true;
                ambient=RenderSettings.ambientLight;mode=RenderSettings.ambientMode;
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.62f,.66f,.65f);
                foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                {
                    if(l.type!=LightType.Directional||!l.gameObject.activeInHierarchy)continue;
                    suns.Add(l);oldColors.Add(l.color);oldStrengths.Add(l.intensity);
                    l.color=new Color(1f,.92f,.83f);l.intensity*=.91f;
                }
                root=new GameObject("Art direction only / tonal post");
                root.hideFlags=HideFlags.DontSaveInBuild|HideFlags.DontSaveInEditor;
                profile=ScriptableObject.CreateInstance<VolumeProfile>();
                var volume=root.AddComponent<Volume>();volume.isGlobal=true;volume.priority=4000;
                volume.sharedProfile=profile;
                var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
                var grade=profile.Add<ColorAdjustments>(true);
                grade.postExposure.Override(-.05f);grade.contrast.Override(10f);
                grade.saturation.Override(-15f);grade.colorFilter.Override(new Color(.96f,.99f,1f));
            }
            public void Dispose()
            {
                for(int i=0;i<suns.Count;i++)if(suns[i]){
                    suns[i].color=oldColors[i];suns[i].intensity=oldStrengths[i];}
                camera.allowHDR=previousHDR;
                if(additional)additional.renderPostProcessing=previousPost;
                RenderSettings.ambientMode=mode;RenderSettings.ambientLight=ambient;
                if(root)Object.DestroyImmediate(root);
                if(profile)Object.DestroyImmediate(profile);
            }
        }
        sealed class GroundPaint:IDisposable
        {
            readonly Terrain terrain;readonly TerrainData original,clone;
            readonly TerrainCollider collider;
            readonly List<TerrainLayer> ownedLayers=new List<TerrainLayer>();
            readonly List<Texture2D> ownedTextures=new List<Texture2D>();
            readonly List<string> audit=new List<string>();
            public IList<string> Audit=>audit;
            public int Layers=>ownedLayers.Count;
            public int Painted=>ownedTextures.Count;
            public GroundPaint(Terrain t,ValoriaReferenceCleanGameplay city)
            {
                terrain=t;original=t.terrainData;collider=t.GetComponent<TerrainCollider>();
                if(!original)throw new InvalidOperationException("No source terrain data");
                clone=Object.Instantiate(original);
                clone.name="Editor visual candidate / detached TerrainData";
                clone.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
                var originalLayers=original.terrainLayers;
                if(originalLayers==null||originalLayers.Length==0)throw new InvalidOperationException("No terrain texture layers");
                var layers=new List<TerrainLayer>();
                foreach(var src in originalLayers)
                {
                    var layer=Object.Instantiate(src);
                    layer.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
                    layer.smoothness=Mathf.Min(.15f,layer.smoothness);
                    layer.metallic=0;
                    if(src.diffuseTexture){
                        var tex=Recolor(src.diffuseTexture);
                        layer.diffuseTexture=tex;ownedTextures.Add(tex);}
                    ownedLayers.Add(layer);layers.Add(layer);
                    audit.Add("terrain-source="+src.name+
                        " / diff="+(src.diffuseTexture?src.diffuseTexture.name:"null"));
                }
                if(layers.Count<=6)
                {
                    var earth=new TerrainLayer {name="Painted worn earth / source-road blending"};
                    earth.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
                    earth.diffuseTexture=Earth();
                    earth.tileSize=new Vector2(4.2f,4.2f);earth.smoothness=.08f;
                    ownedTextures.Add(earth.diffuseTexture);ownedLayers.Add(earth);layers.Add(earth);
                    var alpha=original.GetAlphamaps(0,0,original.alphamapWidth,original.alphamapHeight);
                    int h=original.alphamapHeight,w=original.alphamapWidth,n=originalLayers.Length;
                    var updated=new float[h,w,n+1];
                    var center=city.BastionTwo?city.BastionTwo.transform.position:Vector3.zero;
                    var saw=city.Parcels&&city.Parcels.LeftBuilt?city.Parcels.LeftBuilt.transform.position:
                        new Vector3(-8f,0,1f);
                    var gate=Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                        .FirstOrDefault(go=>go.name=="Gate_LowerGate_Arch");
                    var entry=gate?gate.position:center+new Vector3(0,0,-14f);
                    for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                    {
                        float px=t.transform.position.x+x*(original.size.x/(w-1f));
                        float pz=t.transform.position.z+y*(original.size.z/(h-1f));
                        var pos=new Vector2(px,pz);
                        float noise=Mathf.PerlinNoise(px*.21f+5f,pz*.21f+9f)-.5f;
                        var road=SegmentDistance(pos,new Vector2(center.x,center.z),
                            new Vector2(entry.x,entry.z));
                        float path=Mathf.Clamp01((3.5f+noise*1.8f-road)/3.5f)*.68f;
                        float plaza=Mathf.Clamp01((8.5f+noise*2f-
                            Vector2.Distance(pos,new Vector2(center.x,center.z)))/8.5f)*.45f;
                        float timber=Mathf.Clamp01((6.5f+noise*2f-
                            Vector2.Distance(pos,new Vector2(saw.x,saw.z)))/6.5f)*.52f;
                        float weight=Mathf.Clamp01(Mathf.Max(path,Mathf.Max(plaza,timber)));
                        for(int i=0;i<n;i++)updated[y,x,i]=alpha[y,x,i]*(1-weight);
                        updated[y,x,n]=weight;
                    }
                    clone.terrainLayers=layers.ToArray();
                    clone.SetAlphamaps(0,0,updated);
                    audit.Add("local-earth-splat="+w+"x"+h);
                }else clone.terrainLayers=layers.ToArray();
                terrain.terrainData=clone;
                if(collider)collider.terrainData=clone;
                clone.SetBaseMapDirty();
                terrain.Flush();
            }
            static float SegmentDistance(Vector2 p,Vector2 a,Vector2 b){
                var v=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,v)/Mathf.Max(v.sqrMagnitude,.01f));
                return Vector2.Distance(p,a+v*t);
            }
            static Texture2D Recolor(Texture source)
            {
                int w=Mathf.Clamp(source.width,64,512),h=Mathf.Clamp(source.height,64,512);
                var target=RenderTexture.GetTemporary(w,h,0,RenderTextureFormat.ARGB32);
                var old=RenderTexture.active;
                try{
                    Graphics.Blit(source,target);
                    RenderTexture.active=target;
                    var tex=new Texture2D(w,h,TextureFormat.RGBA32,true,false);
                    tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
                    var pixels=tex.GetPixels32();
                    for(int i=0;i<pixels.Length;i++){
                        Color col=pixels[i];float l=.2126f*col.r+.7152f*col.g+.0722f*col.b;
                        Color sage=new Color(.26f+.34f*l,.28f+.37f*l,.22f+.27f*l,col.a);
                        Color c=Color.Lerp(col,sage,.70f);c.a=col.a;pixels[i]=c;}
                    tex.SetPixels32(pixels);tex.Apply(true,true);
                    tex.name="Cloned weathered sage surface "+source.name;
                    tex.wrapMode=source.wrapMode;tex.filterMode=FilterMode.Trilinear;
                    return tex;
                }finally{RenderTexture.active=old;RenderTexture.ReleaseTemporary(target);}
            }
            static Texture2D Earth()
            {
                int n=128;var tex=new Texture2D(n,n,TextureFormat.RGBA32,true,false);
                var px=new Color[n*n];
                for(int y=0;y<n;y++)for(int x=0;x<n;x++){
                    float n1=Mathf.PerlinNoise(x*.064f+11f,y*.079f+36f)-.5f;
                    float n2=Mathf.PerlinNoise(x*.24f+46f,y*.19f+18f)-.5f;
                    float s=n1*.16f+n2*.055f;
                    px[y*n+x]=new Color(.48f+s,.43f+s*.83f,.33f+s*.68f,1f);}
                tex.SetPixels(px);tex.Apply(true,true);
                tex.wrapMode=TextureWrapMode.Repeat;tex.filterMode=FilterMode.Trilinear;
                return tex;
            }
            public void Dispose(){
                terrain.terrainData=original;if(collider)collider.terrainData=original;
                terrain.Flush();
                foreach(var a in ownedLayers)if(a)Object.DestroyImmediate(a);
                foreach(var a in ownedTextures)if(a)Object.DestroyImmediate(a);
                if(clone)Object.DestroyImmediate(clone);
            }
        }
    }
}
#endif
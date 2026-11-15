#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.StylizedDirectionPilot.Editor
{
    /// <summary>
    /// A/B on the authentic Valoria city Terrain, using a DISPOSABLE clone of
    /// the original TerrainData. Does not touch height, POIs, meshes, pathing,
    /// assets on disk, player save or gameplay. Three reusable painterly
    /// ground materials; no paid/generated external assets.
    /// </summary>
    public static class TerrainPaletteNativeCapture
    {
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        public static void Capture()
        {
            string output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../StylizedDirectionPilotCaptures"));
            Directory.CreateDirectory(output);
            try
            {
                EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
                var city=Object.FindFirstObjectByType<Eldoria.ReferenceClean.ValoriaReferenceCleanGameplay>(FindObjectsInactive.Include);
                if(!city||!city.PlayerCamera)throw new InvalidOperationException("Actual Valoria city camera missing");
                city.Apply(new Eldoria.Domain.PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
                var terrain=Object.FindFirstObjectByType<Terrain>(FindObjectsInactive.Include);
                if(!terrain||!terrain.terrainData)throw new InvalidOperationException("Real Unity authored city terrain missing");
                var camera=city.PlayerCamera;
                var aWide=Path.Combine(output,"A-genuine-terrain-1600x900.png");
                var aPhone=Path.Combine(output,"A-genuine-terrain-390x844.png");
                var fWide=Path.Combine(output,"F-painterly-terrain-1600x900.png");
                var fPhone=Path.Combine(output,"F-painterly-terrain-390x844.png");
                Save(camera,aWide,1600,900);
                Save(camera,aPhone,390,844);
                int layers,alpha;
                using(var scoped=new TemporaryTerrainPalette(terrain))
                {
                    layers=scoped.Layers;
                    alpha=scoped.Alphamap;
                    Save(camera,fWide,1600,900);
                    Save(camera,fPhone,390,844);
                }
                int changed=PixelDifference(aWide,fWide);
                if(changed<18)throw new InvalidOperationException("Ground treatment is visually ineffective; pixel delta="+changed);
                File.WriteAllText(Path.Combine(output,"terrain-pilot-evidence.json"),JsonUtility.ToJson(
                    new Receipt{kind="ACTUAL_UNITY_EDITOR_TERRAIN_CLONE_A_B_NOT_MOBILE_OR_GAMEPLAY",
                        sourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA")??"unbound",
                        numberOfLayers=layers,alphaResolution=alpha,meanPixelDifference=changed,
                        playerSaveAccessed=false,sceneSerialized=false,artApproved=false,mobileValidated=false},true));
                Debug.Log("ELDORIA_TERRAIN_AB_COMPLETE changed="+changed+" layers="+layers+" alpha="+alpha);
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                File.WriteAllText(Path.Combine(output,"terrain-pilot-error.txt"),e.ToString());
                Debug.LogError("ELDORIA_TERRAIN_AB_FAILED "+e);
                EditorApplication.Exit(1);
            }
        }
        [Serializable]
        sealed class Receipt
        {
            public string kind,sourceSha;
            public int numberOfLayers,alphaResolution,meanPixelDifference;
            public bool playerSaveAccessed,sceneSerialized,artApproved,mobileValidated;
        }
        static void Save(Camera c,string path,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
            var target=c.targetTexture;
            var active=RenderTexture.active;
            float ratio=c.aspect,size=c.orthographicSize;
            bool wasEnabled=c.enabled;
            try
            {
                c.enabled=true;c.aspect=w/(float)h;
                if(h>w)c.orthographicSize=size*1.18f;
                c.targetTexture=rt;c.Render();
                RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,w,h),0,0);
                tex.Apply(false,false);
                var bytes=tex.EncodeToPNG();
                if(bytes.Length<30000)throw new InvalidOperationException("Blank actual Unity camera "+path);
                File.WriteAllBytes(path,bytes);
            }
            finally
            {
                c.targetTexture=target;c.aspect=ratio;c.orthographicSize=size;
                c.enabled=wasEnabled;RenderTexture.active=active;
                rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
            }
        }
        static int PixelDifference(string a,string b)
        {
            var ta=new Texture2D(2,2);var tb=new Texture2D(2,2);
            try
            {
                if(!ta.LoadImage(File.ReadAllBytes(a))||!tb.LoadImage(File.ReadAllBytes(b))||
                    ta.width!=tb.width||ta.height!=tb.height)throw new InvalidOperationException("Incompatible real renders");
                var pa=ta.GetPixels32();var pb=tb.GetPixels32();
                long sum=0;int sample=0;
                for(int i=0;i<pa.Length;i+=13)
                {
                    sum+=Math.Abs(pa[i].r-pb[i].r)+Math.Abs(pa[i].g-pb[i].g)+Math.Abs(pa[i].b-pb[i].b);
                    sample++;
                }
                return (int)(sum/Math.Max(1,sample*3));
            }
            finally{Object.DestroyImmediate(ta);Object.DestroyImmediate(tb);}
        }
        sealed class TemporaryTerrainPalette : IDisposable
        {
            readonly Terrain surface;
            readonly TerrainData source;
            readonly TerrainData data;
            readonly TerrainLayer[] layers;
            readonly Texture2D[] maps;
            readonly AmbientMode oldMode;
            readonly Color oldAmbient;
            public int Layers=>layers.Length;
            public int Alphamap=>data.alphamapResolution;

            public TemporaryTerrainPalette(Terrain terrain)
            {
                surface=terrain;source=terrain.terrainData;
                data=Object.Instantiate(source);
                data.name="ART PROOF ONLY / painted procedural terrain data";
                data.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
                oldMode=RenderSettings.ambientMode;
                oldAmbient=RenderSettings.ambientLight;
                // All textures are authored at run-time for an isolated, deterministic
                // look development test. No source terrain layers are rewritten.
                maps=new[]{
                    MakeTexture(new Color(.30f,.40f,.29f),new Color(.48f,.56f,.35f),2.6f),
                    MakeTexture(new Color(.18f,.30f,.25f),new Color(.34f,.45f,.30f),6.3f),
                    MakeTexture(new Color(.42f,.40f,.31f),new Color(.60f,.55f,.40f),9.8f)
                };
                layers=new TerrainLayer[3];
                for(int i=0;i<layers.Length;i++)
                {
                    var l=ScriptableObject.CreateInstance<TerrainLayer>();
                    l.name="ART PROOF / "+new[]{"sage meadow","forest undergrowth","warm exposed earth"}[i];
                    l.hideFlags=HideFlags.DontSaveInEditor|HideFlags.DontSaveInBuild;
                    l.diffuseTexture=maps[i];
                    l.tileSize=new Vector2(i==1?24f:35f,i==1?24f:35f);
                    l.tileOffset=Vector2.zero;
                    l.metallic=0f;l.smoothness=.05f;
                    layers[i]=l;
                }
                data.terrainLayers=layers;
                var w=data.alphamapWidth;var h=data.alphamapHeight;
                var alpha=new float[h,w,3];
                for(int y=0;y<h;y++)for(int x=0;x<w;x++)
                {
                    float u=(x+.5f)/w,v=(y+.5f)/h;
                    float big=Mathf.PerlinNoise(u*4.4f+4.11f,v*5.6f+5.71f);
                    float med=Mathf.PerlinNoise(u*13.1f+12.4f,v*14.8f+1.6f);
                    float fine=Mathf.PerlinNoise(u*33.1f+6.7f,v*28.3f+4.3f);
                    float dark=Mathf.Clamp01((big*.65f+med*.35f-.47f)*1.9f);
                    float exposed=Mathf.Clamp01((.48f-big*.60f-med*.35f+fine*.12f)*1.40f)*.43f;
                    dark*=.62f;
                    float meadow=1f-dark-exposed;
                    alpha[y,x,0]=Mathf.Max(0f,meadow);
                    alpha[y,x,1]=dark;
                    alpha[y,x,2]=exposed;
                }
                data.SetAlphamaps(0,0,alpha);
                surface.terrainData=data;
                surface.Flush();
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.64f,.69f,.70f);
            }
            static Texture2D MakeTexture(Color low,Color high,float offset)
            {
                const int size=256;
                var tex=new Texture2D(size,size,TextureFormat.RGBA32,true,false);
                tex.name="ART PROOF / authored painterly albedo";
                var px=new Color32[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    // Hand-authored source palette with multi-scale grain:
                    // long tonal strokes, no neon / no photoreal texture bloat.
                    float baseValue=Mathf.PerlinNoise(x*.022f+offset,y*.016f+offset);
                    float small=Mathf.PerlinNoise(x*.15f+offset*4f,y*.09f+offset*3f);
                    float grain=Mathf.PerlinNoise(x*.41f+offset,y*.33f+offset*9f);
                    float value=Mathf.Clamp01(.17f+.62f*baseValue+.16f*small+.05f*grain);
                    var c=Color.Lerp(low,high,value);
                    px[y*size+x]=new Color(c.r,c.g,c.b,1f);
                }
                tex.SetPixels32(px);
                tex.Apply(true,false);
                tex.wrapMode=TextureWrapMode.Repeat;
                tex.filterMode=FilterMode.Trilinear;
                return tex;
            }
            public void Dispose()
            {
                surface.terrainData=source;
                surface.Flush();
                RenderSettings.ambientMode=oldMode;RenderSettings.ambientLight=oldAmbient;
                Object.DestroyImmediate(data);
                foreach(var layer in layers)if(layer)Object.DestroyImmediate(layer);
                foreach(var tex in maps)if(tex)Object.DestroyImmediate(tex);
            }
        }
    }
}
#endif
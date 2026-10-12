using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Experimental, batch-executable capture of the exact V3 family GLB assets.
    // This never opens or changes a production scene.
    public static class ValoriaFamilyV3Capture
    {
        static readonly string[] Names = {
            "wall_straight", "watchtower", "wall_corner", "wall_connections",
            "stone_path_platform", "cliff_rocks", "defensive_props"
        };
        static readonly Vector3[] Positions = {
            new Vector3(-14,0,0), new Vector3(-7,0,0), new Vector3(0,0,0),
            new Vector3(7,0,0), new Vector3(14,0,0), new Vector3(-5,0,16),
            new Vector3(6,0,16)
        };
        static readonly Dictionary<Renderer, Material[]> Originals = new Dictionary<Renderer, Material[]>();
        static readonly List<Material> Materials = new List<Material>();

        public static async void Capture()
        {
            try
            {
                SceneSetup.SetupRenderPipeline();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject("Approved_Valoria_Family_V3");
                var objects = new List<GameObject>();
                var baseDir = Path.Combine(UnityEngine.Application.streamingAssetsPath, "ValoriaFamilyV3");
                foreach (var name in Names)
                {
                    var path = Path.Combine(baseDir, name + ".glb");
                    if (!File.Exists(path)) throw new FileNotFoundException(name, path);
                    var importer = new GltfImport(null, new UninterruptedDeferAgent());
                    bool loaded = await importer.Load(new Uri(path).AbsoluteUri);
                    if (!loaded) throw new Exception("glTFast load failure: " + name);
                    var holder = new GameObject(name);
                    holder.transform.SetParent(root.transform, false);
                    holder.transform.localPosition = Positions[objects.Count];
                    bool instantiated = await importer.InstantiateMainSceneAsync(holder.transform);
                    if (!instantiated) throw new Exception("GLB instantiate failure: " + name);
                    objects.Add(holder);
                }
                var sunlight = new GameObject("Sun").AddComponent<Light>();
                sunlight.type = LightType.Directional;
                sunlight.shadows = LightShadows.Soft;
                var camera = new GameObject("Family capture camera").AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.orthographic = true;
                camera.nearClipPlane = 0.05f;
                camera.farClipPlane = 300f;
                var folder = Path.Combine(UnityEngine.Application.dataPath, "../ValoriaV3UnityCaptures");
                Directory.CreateDirectory(folder);
                foreach (var hero in new[]{false,true})
                {
                    SetLookdev(false, objects, sunlight);
                    CaptureSet(folder, "baseline", camera, objects, hero);
                    SetLookdev(true, objects, sunlight);
                    CaptureSet(folder, "lookdev", camera, objects, hero);
                }
                File.WriteAllText(Path.Combine(folder,"capture-manifest.json"),
                    "{\"source\":\"approved family v3\",\"engine\":\"Unity 6000.3.23f1\",\"capture\":\"real Unity camera.Render\",\"art_pass\":false}");
                Debug.Log("VALORIA_V3_CAPTURE_COMPLETED " + folder);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError("VALORIA_V3_CAPTURE_FAILED " + ex);
                EditorApplication.Exit(1);
            }
        }

        static void SetLookdev(bool lookdev,List<GameObject> objects,Light sun)
        {
            sun.color=lookdev ? new Color(1f,.87f,.72f) : Color.white;
            sun.intensity=lookdev ? 1.48f : 1f;
            sun.transform.rotation=Quaternion.Euler(lookdev? 46f:55f,-38f,0f);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=lookdev ? new Color(.53f,.56f,.60f):new Color(.70f,.70f,.70f);
            RenderSettings.fog=lookdev;
            RenderSettings.fogColor=new Color(.48f,.54f,.59f);
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=36f;
            RenderSettings.fogEndDistance=85f;
            foreach (var obj in objects)
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>(true))
            {
                if (!Originals.ContainsKey(renderer)) Originals[renderer]=renderer.sharedMaterials;
                var src=Originals[renderer];
                if (!lookdev) { renderer.sharedMaterials=src; continue; }
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    if(src[i]==null) continue;
                    var m=new Material(src[i]);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness", Mathf.Clamp01(m.GetFloat("_Smoothness")*.82f));
                    if(m.HasProperty("_BaseColor"))
                    {
                        var c=m.GetColor("_BaseColor");
                        m.SetColor("_BaseColor",Color.Lerp(c,new Color(c.r*.95f,c.g*.91f,c.b*.85f,c.a),.28f));
                    }
                    Materials.Add(m);
                    dst[i]=m;
                }
                renderer.sharedMaterials=dst;
            }
        }

        static void CaptureSet(string dir,string mode,Camera cam,List<GameObject> objects,bool hero)
        {
            for(int i=0;i<objects.Count;i++)
            {
                var obj=objects[i];
                var bounds=BoundsFor(obj);
                Vector3 focus=bounds.center;
                var radius=Mathf.Max(bounds.extents.x,Mathf.Max(bounds.extents.y,bounds.extents.z));
                var direction=new Vector3(1.15f,.85f,-1.4f).normalized;
                cam.transform.position=focus+direction*(radius*5+9);
                cam.transform.LookAt(focus);
                cam.orthographicSize=Mathf.Max(2f,radius*(hero?1.6f:2.75f));
                foreach(var other in objects)other.SetActive(other==obj);
                Render(cam,Path.Combine(dir,mode+"-"+obj.name+(hero?"-hero":"-strategic")+".png"));
                obj.SetActive(true);
            }
        }

        static Bounds BoundsFor(GameObject obj)
        {
            var renderers=obj.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)throw new Exception("No meshes in "+obj.name);
            var b=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        static void Render(Camera cam,string path)
        {
            var rt=new RenderTexture(960,640,24,RenderTextureFormat.ARGB32);
            var prev=RenderTexture.active;
            try {
                cam.targetTexture=rt;
                cam.Render();
                RenderTexture.active=rt;
                var tex=new Texture2D(960,640,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,960,640),0,0);
                tex.Apply();
                File.WriteAllBytes(path,tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
            } finally {
                cam.targetTexture=null;
                RenderTexture.active=prev;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}

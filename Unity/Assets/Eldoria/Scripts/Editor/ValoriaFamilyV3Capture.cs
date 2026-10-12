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
            "stone_path_platform", "cliff_rocks", "defensive_props", "hero_gate_v3"
        };
        static readonly Vector3[] Positions = {
            new Vector3(-14,0,0), new Vector3(-7,0,0), new Vector3(0,0,0),
            new Vector3(7,0,0), new Vector3(14,0,0), new Vector3(-5,0,16),
            new Vector3(6,0,16), new Vector3(28,0,0)
        };
        static readonly Dictionary<Renderer, Material[]> Originals = new Dictionary<Renderer, Material[]>();
        static readonly List<Material> Materials = new List<Material>();

        public static async void Capture()
        {
            try
            {
                SceneSetup.SetupRenderPipeline();
                ValoriaUnityFinishKit.ConfigureIsolatedPipeline();
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
                foreach(var stale in Directory.GetFiles(folder,"*.png")) File.Delete(stale);
                foreach (var hero in new[]{false,true})
                {
                    SetLookdev(false, objects, sunlight);
                    CaptureSet(folder, "baseline", camera, objects, hero);
                    SetLookdev(true, objects, sunlight);
                    CaptureSet(folder, "lookdev", camera, objects, hero);
                }
                await CaptureAssembly(objects,sunlight,camera,baseDir,folder);
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
            sun.intensity=lookdev ? 1.45f : 1f;
            sun.shadowStrength=lookdev ? .87f : .5f;
            sun.transform.rotation=Quaternion.Euler(lookdev? 36f:55f,-43f,0f);
            RenderSettings.ambientMode=lookdev?AmbientMode.Trilight:AmbientMode.Flat;
            RenderSettings.ambientSkyColor=new Color(.37f,.46f,.61f);
            RenderSettings.ambientEquatorColor=new Color(.28f,.32f,.35f);
            RenderSettings.ambientGroundColor=new Color(.17f,.14f,.10f);
            RenderSettings.ambientLight=lookdev ? new Color(.35f,.39f,.41f):new Color(.70f,.70f,.70f);
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
                    var m=ValoriaUnityFinishKit.FinishMaterial(src[i],obj.name);
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
                var lookdev=mode=="lookdev";
                var terrain=lookdev?ValoriaUnityFinishKit.MakeContextFloor(bounds,obj.name):null;
                var bounce=lookdev?ValoriaUnityFinishKit.MakeBounce(bounds):null;
                var nature=lookdev?ValoriaNatureDresser.Dress(bounds,obj.name):null;
                cam.backgroundColor=lookdev?new Color(.43f,.52f,.60f):new Color(.20f,.31f,.47f);
                Vector3 focus=bounds.center;
                var radius=Mathf.Max(bounds.extents.x,Mathf.Max(bounds.extents.y,bounds.extents.z));
                var direction=new Vector3(1.15f,.85f,-1.4f).normalized;
                cam.transform.position=focus+direction*(radius*5+9);
                cam.transform.LookAt(focus);
                cam.orthographicSize=Mathf.Max(2f,radius*(hero?1.6f:2.75f));
                cam.farClipPlane=Mathf.Max(500f,radius*18f);
                Debug.Log($"VALORIA_V3_CAMERA {obj.name} center={focus} radius={radius} pos={cam.transform.position} ortho={cam.orthographicSize} far={cam.farClipPlane} renderers={obj.GetComponentsInChildren<Renderer>(true).Length}");
                foreach(var other in objects)other.SetActive(other==obj);
                Render(cam,Path.Combine(dir,mode+"-"+obj.name+(hero?"-hero":"-strategic")+".png"));
                obj.SetActive(true);
                if(terrain!=null) UnityEngine.Object.DestroyImmediate(terrain);
                if(bounce!=null) UnityEngine.Object.DestroyImmediate(bounce.gameObject);
                if(nature!=null) UnityEngine.Object.DestroyImmediate(nature);
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


        // Full-family validation scene uses real authored GLB meshes, not stand-in boxes.
        static async Task CaptureAssembly(List<GameObject> pieces,Light sun,Camera cam,string sourceDir,string outDir)
        {
            foreach(var piece in pieces)piece.SetActive(true);
            SetLookdev(false,pieces,sun);
            var assembled=new List<GameObject>(pieces);
            var wallRight=UnityEngine.Object.Instantiate(pieces[0]);
            wallRight.name="wall_straight_right"; assembled.Add(wallRight);
            var towerRight=UnityEngine.Object.Instantiate(pieces[1]);
            towerRight.name="watchtower_right"; assembled.Add(towerRight);
            var cornerRight=UnityEngine.Object.Instantiate(pieces[2]);
            cornerRight.name="wall_corner_right"; assembled.Add(cornerRight);
            var connectionRight=UnityEngine.Object.Instantiate(pieces[3]);
            connectionRight.name="wall_connection_right"; assembled.Add(connectionRight);
            var cliffMiddle=UnityEngine.Object.Instantiate(pieces[5]);
            cliffMiddle.name="cliff_rocks_middle"; assembled.Add(cliffMiddle);
            var cliffRight=UnityEngine.Object.Instantiate(pieces[5]);
            cliffRight.name="cliff_rocks_right"; assembled.Add(cliffRight);
            var extraPath=UnityEngine.Object.Instantiate(pieces[4]);
            extraPath.name="stone_path_platform_second_segment"; assembled.Add(extraPath);

            var gate=pieces[7];
            await Task.CompletedTask;

            // Snap architectural module feet and align a readable independent study scene.
            Place(pieces[0],-10f,0f);      Place(wallRight,10f,0f);
            Place(pieces[1],-16f,0f);      Place(towerRight,16f,0f);
            Place(pieces[2],-21f,0f);      Place(cornerRight,21f,0f);
            Place(pieces[3],-6f,0f);       Place(connectionRight,6f,0f);
            Place(pieces[4],0f,-11f);      Place(extraPath,0f,-17f);
            Place(pieces[5],-21f,8f); Place(cliffMiddle,-6f,8f); Place(cliffRight,9f,8f);
            Place(pieces[6],0f,-7f);       Place(gate,0f,0f);
            Bounds bounds=BoundsFor(assembled[0]);
            foreach(var o in assembled)bounds.Encapsulate(BoundsFor(o));
            var center=bounds.center;
            Debug.Log("VALORIA_V3_ASSEMBLY_BOUNDS="+bounds+" active="+assembled.Count);
            cam.transform.position=center+new Vector3(1.08f,.84f,-1.40f).normalized*
                (Mathf.Max(bounds.size.x,bounds.size.z)*2.9f+35f);
            cam.transform.LookAt(center);
            cam.orthographicSize=Mathf.Max(bounds.extents.x*.92f,bounds.extents.z*1.15f)+1.6f;
            cam.farClipPlane=1000f;
            cam.backgroundColor=new Color(.20f,.31f,.47f);
            Debug.Log("VALORIA_V3_ASSEMBLY_CAMERA="+cam.transform.position+" size="+cam.orthographicSize);

            SetLookdev(false,assembled,sun);
            Render(cam,Path.Combine(outDir,"previous-assembled-hero.png"));
            // Assemble sockets from real world bounds, preserving approved source geometry.
            var gb=BoundsFor(gate);
            float left=gb.min.x, right=gb.max.x;
            Place(pieces[3],left-.30f,0f); Place(connectionRight,right+.30f,0f);
            Place(pieces[0],left-BoundsFor(pieces[0]).extents.x+.12f,0f);
            Place(wallRight,right+BoundsFor(wallRight).extents.x-.12f,0f);
            float lx=BoundsFor(pieces[0]).min.x, rx=BoundsFor(wallRight).max.x;
            Place(pieces[1],lx,0f); Place(towerRight,rx,0f);
            Place(pieces[2],lx-2.2f,1f); Place(cornerRight,rx+2.2f,1f);
            float roadEnd=BoundsFor(gate).min.z+.35f;
            float pathDepth=BoundsFor(pieces[4]).size.z;
            Place(pieces[4],0f,roadEnd-pathDepth*.5f);
            Place(extraPath,0f,roadEnd-pathDepth*1.5f+.15f);
            Place(pieces[6],-4f,roadEnd-1f);
            // Cliff top supports the terrace; never put the cliff bottom at wall feet.
            Place(pieces[5],-18f,-18.5f);
            Place(cliffMiddle,-4.5f,-19.2f);
            Place(cliffRight,9f,-17.8f);
            foreach(var cliff in new[]{pieces[5],cliffMiddle,cliffRight})
                cliff.transform.position+=Vector3.up*(-.12f-BoundsFor(cliff).max.y);
            var rightProps=UnityEngine.Object.Instantiate(pieces[6]);
            rightProps.name="defensive_props_right";
            Place(rightProps,4f,roadEnd-1f);
            assembled.Add(rightProps);
            Render(cam,Path.Combine(outDir,"baseline-assembled-hero.png"));
            // Frozen Hero V3 retains its original material collection; avoid cloning >1000 source parts.
            var styled=new List<GameObject>(assembled);
            // Hero V3 shares the same cached material finishing as the seven family modules.
            SetLookdev(true,styled,sun);
            cam.backgroundColor=new Color(.43f,.52f,.60f);
            // Scene-relative fog: fixed short-range fog erased every distant structure.
            float viewDistance=Vector3.Distance(cam.transform.position,center);
            RenderSettings.fogStartDistance=viewDistance*.93f;
            RenderSettings.fogEndDistance=viewDistance*2.85f;
            var ground=ValoriaUnityFinishKit.MakeContextFloor(bounds,"assembled_family");
            var trees=ValoriaNatureDresser.DressAssembly(bounds);
            var bounce=ValoriaUnityFinishKit.MakeBounce(bounds);
            Render(cam,Path.Combine(outDir,"lookdev-assembled-hero.png"));
            Render(cam,Path.Combine(outDir,"lookdev-assembled-mobile.png"),780,520);
            ValoriaUnityFinishKit.SaveStudyScene();
            if(ground!=null)UnityEngine.Object.DestroyImmediate(ground);
            if(trees!=null)UnityEngine.Object.DestroyImmediate(trees);
            if(bounce!=null)UnityEngine.Object.DestroyImmediate(bounce.gameObject);
            Debug.Log("VALORIA_V3_ASSEMBLED bounds="+bounds+" meshes="+assembled.Count);
        }

        static void Place(GameObject obj,float x,float z)
        {
            Bounds b=BoundsFor(obj);
            obj.transform.position+=new Vector3(x-b.center.x,-b.min.y,z-b.center.z);
        }

        static void Render(Camera cam,string path,int width=1440,int height=960)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            rt.antiAliasing=4;
            var prev=RenderTexture.active;
            try {
                cam.targetTexture=rt;
                cam.Render();
                RenderTexture.active=rt;
                var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,width,height),0,0);
                tex.Apply();
                var center=tex.GetPixel(tex.width/2,tex.height/2);
                var corner=tex.GetPixel(0,0);
                var unique=false;
                for(int y=8;y<tex.height;y+=24)
                for(int x=8;x<tex.width;x+=24)
                {
                    var color=tex.GetPixel(x,y);
                    if(color!=corner) { unique=true; break; }
                }
                Debug.Log($"VALORIA_V3_IMAGE {Path.GetFileName(path)} center={center} corner={corner} populated={unique}");
                if(!unique) throw new Exception("Empty Unity render: "+path);
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

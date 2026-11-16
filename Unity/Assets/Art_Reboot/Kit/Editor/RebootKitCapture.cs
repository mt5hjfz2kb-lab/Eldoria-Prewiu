using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Eldoria.ArtReboot.Kit.Editor
{
    /// <summary>Authentic Unity 6 URP capture of real reusable FBX kit instances.
    /// Source scenes and approved Phase 1 hero FBX/atlas remain byte-identical.</summary>
    public static class RebootKitCapture
    {
        const string Kit = "Assets/Art_Reboot/Kit";
        const string Phase1 = "Assets/Art_Reboot/Phase1";

        [Serializable] class Entry
        {
            public string id, family, fbx, uv_layer, vertex_color_layer;
            public int triangles, faces, grid_m;
            public int[] footprint_cells;
            public float[] local_min_m, local_max_m, pivot_m;
            public string[] materials;
        }
        [Serializable] class Catalog
        {
            public int version, grid_m, material_count, total_triangles;
            public int[] atlas_size;
            public string atlas, atlas_sha256;
            public string[] materials;
            public Entry[] pieces;
        }
        [Serializable] class NativeProof
        {
            public string unity, sha, kitScene, kitGridScene, atlas;
            public int pieces, families, kitTriangles, villageInstances, villageTriangles, materialCount;
            public float cameraTilt=42f, cameraYaw=-13f, sunElevation=17f;
            public bool phase1Protected=true, allBuildingsKitInstances=true, physicalMobileTested=false, ownerApproved=false;
        }
        static readonly Dictionary<string,GameObject> prefabs = new Dictionary<string,GameObject>();
        static readonly Dictionary<string,Entry> entries = new Dictionary<string,Entry>();
        static Material surface, warm, corrupt;
        static int instances, trianglesInstanced;
        static readonly string[] approved = new[]
        {
            Phase1+"/Models/Reboot_Hero.fbx",
            Phase1+"/Models/Reboot_Ruin.fbx",
            Phase1+"/Models/Reboot_Corrupt.fbx",
            Phase1+"/Textures/Hero_Atlas_256.png",
            Phase1+"/Scenes/Phase1_Village_Atmosphere.unity",
            Phase1+"/Scenes/Eldoria_Visual_Sandbox.unity"
        };

        [MenuItem("Art Reboot/Phase 2/Capture modular kit and settlement")]
        public static void BuildAndCapture()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Kit scene can only be captured in Edit Mode.");
            for(int i=0;i<SceneManager.sceneCount;i++)
                if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save dirty scene before kit capture.");
            var hashBefore = approved.Select(x => HashBytes(File.ReadAllBytes(x))).ToArray();
            var setup=EditorSceneManager.GetSceneManagerSetup();
            var previousPipeline=QualitySettings.renderPipeline;
            bool wasAsync=ShaderUtil.allowAsyncCompilation;
            string output=Environment.GetEnvironmentVariable("KIT_CAPTURE_OUTPUT") ??
                           Path.GetFullPath("../artifacts/kit-phase2");
            Directory.CreateDirectory(output);
            try
            {
                Directory.CreateDirectory(Kit+"/Scenes");
                Directory.CreateDirectory(Kit+"/Materials");
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ShaderUtil.allowAsyncCompilation=false;
                // Use the previously owner-approved URP asset, without modifying it.
                var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    Phase1+"/Materials/Village_URP.asset");
                if(pipeline==null)throw new InvalidOperationException("Approved Phase 1 URP profile missing.");
                QualitySettings.renderPipeline=pipeline;
                var catalog=ReadCatalog();
                LoadAndValidateAssets(catalog);
                SetupMaterials();

                var kitScene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var kitRoot=new GameObject("Phase2 reusable kit aligned on 4m-grid");
                ApplyDusk(kitRoot,false);
                var galleryCamera=NewCamera(kitRoot);
                galleryCamera.clearFlags=CameraClearFlags.SolidColor;
                galleryCamera.backgroundColor=new Color(.21f,.20f,.29f,1);
                BuildGalleryFloor(kitRoot);
                var galleryBounds=BuildGallery(kitRoot, kitScene, catalog);
                Fit(galleryCamera,galleryBounds,2400,1350,new Vector3(42,-14,0),1.08f);
                Capture(galleryCamera,2400,1350,Path.Combine(output,"kit-grid-2400x1350.png"));
                EditorSceneManager.SaveScene(kitScene,Kit+"/Scenes/Phase2_Kit_Grid.unity");
                var gridCount=instances;
                if(gridCount!=31)throw new InvalidOperationException("Gallery must instantiate all 31 unique kit pieces.");

                instances=0;trianglesInstanced=0;
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var root=new GameObject("Phase2 true modular hamlet — no old full-building meshes");
                ApplyDusk(root,true);
                Ground(root);
                LowMist(root);
                var architecturalBounds=BuildVillage(root,scene);
                var villageCount=instances;
                var villageTriangles=trianglesInstanced;
                if(villageCount<50)throw new InvalidOperationException("Missing modular architecture instances.");
                AddVegetation(root,scene);
                var camera=NewCamera(root);
                Fit(camera,architecturalBounds,1600,900,new Vector3(42,-13,0),1.08f);
                Capture(camera,1600,900,Path.Combine(output,"village-1600x900.png"));
                Fit(camera,architecturalBounds,1100,1250,new Vector3(42,-13,0),1.08f);
                Capture(camera,1100,1250,Path.Combine(output,"village-1100x1250.png"));
                var corruptBounds=new Bounds(new Vector3(0,4,-13),new Vector3(14,10,13));
                Fit(camera,corruptBounds,1100,1250,new Vector3(42,-13,0),1.07f);
                Capture(camera,1100,1250,Path.Combine(output,"corrupt-kit-detail.png"));
                // The owner-approved camera remains the one saved in the scene.
                Fit(camera,architecturalBounds,1600,900,new Vector3(42,-13,0),1.08f);
                EditorSceneManager.SaveScene(scene,Kit+"/Scenes/Eldoria_Kit_Sandbox.unity");
                AssetDatabase.SaveAssets();

                var proof=new NativeProof{
                    unity=Application.unityVersion,
                    sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),
                    kitScene=Kit+"/Scenes/Eldoria_Kit_Sandbox.unity",
                    kitGridScene=Kit+"/Scenes/Phase2_Kit_Grid.unity",
                    atlas=catalog.atlas_sha256,
                    pieces=catalog.pieces.Length,
                    families=catalog.pieces.Select(x=>x.family).Distinct().Count(),
                    kitTriangles=catalog.total_triangles,
                    villageInstances=villageCount,
                    villageTriangles=villageTriangles,
                    materialCount=3,
                    phase1Protected=true,
                    allBuildingsKitInstances=true,
                    physicalMobileTested=false,
                    ownerApproved=false
                };
                File.WriteAllText(Path.Combine(output,"kit-native-proof.json"),JsonUtility.ToJson(proof,true));
                File.WriteAllText(Path.Combine(output,"kit-polygons.csv"),
                    "piece,family,triangles\n"+string.Join("\n",catalog.pieces.Select(x=>x.id+","+x.family+","+x.triangles))+"\n");
                Debug.Log("UNITY_KIT_CAPTURE_PASS "+catalog.pieces.Length+" pieces, "+villageCount+" assembled prefab instances, source "+proof.sha);
            }
            finally
            {
                QualitySettings.renderPipeline=previousPipeline;
                ShaderUtil.allowAsyncCompilation=wasAsync;
                if(setup.Any(s=>s.isLoaded)&&setup.Count(s=>s.isActive)==1)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                for(int i=0;i<approved.Length;i++)
                {
                    var now=HashBytes(File.ReadAllBytes(approved[i]));
                    if(!now.SequenceEqual(hashBefore[i]))
                        throw new InvalidOperationException("Protected Phase1 bytes changed: "+approved[i]);
                }
            }
        }

        static byte[] HashBytes(byte[] bytes)
        {
            using(var sha=System.Security.Cryptography.SHA256.Create())
                return sha.ComputeHash(bytes);
        }

        static Catalog ReadCatalog()
        {
            var file=Kit+"/Source~/kit_manifest.json";
            var result=JsonUtility.FromJson<Catalog>(File.ReadAllText(file));
            if(result==null||result.pieces==null||result.pieces.Length!=31||result.grid_m!=4 ||
               result.material_count!=3||result.atlas_size==null||result.atlas_size.Length!=2 ||
               result.atlas_size[0]!=256||result.atlas_size[1]!=256)
                throw new InvalidOperationException("Invalid kit catalog contract");
            var atlasFile=File.ReadAllBytes(Phase1+"/Textures/Hero_Atlas_256.png");
            string actual=BitConverter.ToString(HashBytes(atlasFile))
                 .Replace("-","").ToLowerInvariant();
            if(!actual.Equals(result.atlas_sha256,StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Atlas hash is different from approved Phase1 source");
            if(result.pieces.Sum(x=>x.triangles)!=result.total_triangles)
                throw new InvalidOperationException("Kit manifest triangles inconsistent");
            return result;
        }

        static void SetupMaterials()
        {
            var shader=AssetDatabase.LoadAssetAtPath<Shader>(Phase1+"/Materials/Reboot_Atlas.shadergraph");
            var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(Phase1+"/Textures/Hero_Atlas_256.png");
            if(shader==null||ShaderUtil.ShaderHasError(shader)||atlas==null||atlas.width!=256||atlas.height!=256)
                throw new InvalidOperationException("Approved phase 1 atlas or URP shader invalid");
            surface=CreateKitMaterial("Kit_Surface",shader,atlas,0);
            warm=CreateKitMaterial("Kit_WarmLight",shader,atlas,1.55f);
            corrupt=CreateKitMaterial("Kit_Corruption",shader,atlas,2.15f);
            AssetDatabase.SaveAssets();
        }
        static Material CreateKitMaterial(string name, Shader shader, Texture atlas, float glow)
        {
            var path=Kit+"/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}
            m.shader=shader;m.SetTexture("_Atlas",atlas);m.SetFloat("_Glow",glow);
            EditorUtility.SetDirty(m);return m;
        }
        static void LoadAndValidateAssets(Catalog catalog)
        {
            prefabs.Clear();entries.Clear();long total=0;
            foreach(var item in catalog.pieces)
            {
                if(item.grid_m!=4||item.pivot_m==null||item.pivot_m.Length!=3||
                   item.pivot_m.Any(x=>Math.Abs(x)>.001f)||
                   item.local_min_m==null||item.local_min_m[2]<-.016f)
                    throw new InvalidOperationException("Broken base or grid contract: "+item.id);
                string path=Kit+"/"+item.fbx;
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=AssetImporter.GetAtPath(path) as ModelImporter;
                if(importer==null)throw new InvalidOperationException("FBX importer missing "+item.id);
                importer.globalScale=1;importer.useFileScale=true;importer.useFileUnits=true;
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
                importer.meshCompression=ModelImporterMeshCompression.Off;
                importer.importNormals=ModelImporterNormals.Import;
                importer.SaveAndReimport();
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(prefab==null)throw new InvalidOperationException("Kit FBX prefab missing "+path);
                var meshes=prefab.GetComponentsInChildren<MeshFilter>();
                if(meshes.Length!=1)throw new InvalidOperationException("Kit part must be one reusable mesh: "+item.id);
                var mesh=meshes[0].sharedMesh;
                if(mesh==null||!mesh.HasVertexAttribute(VertexAttribute.TexCoord0)||
                   !mesh.HasVertexAttribute(VertexAttribute.Color))
                    throw new InvalidOperationException("Missing authored atlas UV or vertex colors: "+item.id);
                int triangles=0;for(int s=0;s<mesh.subMeshCount;s++)triangles+=(int)mesh.GetIndexCount(s)/3;
                if(triangles!=item.triangles)throw new InvalidOperationException("FBX triangles mismatch "+item.id+": "+triangles+" != "+item.triangles);
                total+=triangles;
                if(prefabs.ContainsKey(item.id))throw new InvalidOperationException("Duplicate kit ID "+item.id);
                prefabs.Add(item.id,prefab);entries.Add(item.id,item);
            }
            if(total!=catalog.total_triangles)
                throw new InvalidOperationException("Kit imported triangle count mismatches manifest");
        }

        static GameObject Place(string id, Transform root, Scene scene, Vector3 where,
            float yaw=0, Vector3? scale=null)
        {
            if(!prefabs.TryGetValue(id,out var source))throw new InvalidOperationException("Unknown piece "+id);
            var prefab=(GameObject)PrefabUtility.InstantiatePrefab(source,scene);
            prefab.name=id+" #"+instances;
            prefab.transform.SetParent(root,false);
            prefab.transform.position=where;
            prefab.transform.rotation=Quaternion.Euler(0,yaw,0)*source.transform.rotation;
            if(scale.HasValue)prefab.transform.localScale=scale.Value;
            var renderers=prefab.GetComponentsInChildren<MeshRenderer>();
            if(renderers.Length==0)throw new InvalidOperationException("Kit prefab not renderable "+id);
            foreach(var renderer in renderers)
            {
                var previous=renderer.sharedMaterials;
                renderer.sharedMaterials=previous.Select(x=>x!=null&&x.name.Contains("Kit_Corruption")?corrupt:
                    x!=null&&x.name.Contains("Kit_WarmLight")?warm:surface).ToArray();
            }
            instances++;trianglesInstanced+=entries[id].triangles;
            return prefab;
        }
        static void BuildGalleryFloor(GameObject root)
        {
            // Presentation-only studio floor, NOT one of the 31 kit prefabs.
            // Individual grounded cells make shape, scale and pivot checkable in one PNG.
            var vertices=new List<Vector3>();
            var colors=new List<Color>();
            var uv=new List<Vector2>();
            var indices=new List<int>();
            for(int row=0;row<4;row++)for(int column=0;column<8;column++)
            {
                float cx=(column-3.5f)*7.15f;
                float cz=(1.5f-row)*8.75f;
                float w=6.85f,d=8.35f;
                int n=vertices.Count;
                vertices.Add(new Vector3(cx-w*.5f,-.17f,cz-d*.5f));
                vertices.Add(new Vector3(cx-w*.5f,-.17f,cz+d*.5f));
                vertices.Add(new Vector3(cx+w*.5f,-.17f,cz+d*.5f));
                vertices.Add(new Vector3(cx+w*.5f,-.17f,cz-d*.5f));
                // Neutral stone patch within the owner-approved 256 atlas.
                for(int i=0;i<4;i++)
                {
                    uv.Add(new Vector2(.095f,.095f));
                    colors.Add((column+row)%2==0?
                        new Color(.82f,.80f,.77f,1):
                        new Color(.70f,.71f,.72f,1));
                }
                indices.Add(n);indices.Add(n+1);indices.Add(n+2);
                indices.Add(n);indices.Add(n+2);indices.Add(n+3);
            }
            var mesh=new Mesh{indexFormat=IndexFormat.UInt32};
            mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetUVs(0,uv);
            mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            const string assetPath=Kit+"/Materials/Kit_GalleryFloor.asset";
            var stored=AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if(stored==null){AssetDatabase.CreateAsset(mesh,assetPath);stored=mesh;}
            else{EditorUtility.CopySerialized(mesh,stored);Object.DestroyImmediate(mesh);}
            var go=new GameObject("Studio review board - 8 by 4 neutral atlas cells");
            go.transform.SetParent(root.transform,false);
            go.AddComponent<MeshFilter>().sharedMesh=stored;
            var render=go.AddComponent<MeshRenderer>();render.sharedMaterial=surface;
            render.receiveShadows=true;
        }

        static Bounds BuildGallery(GameObject root, Scene scene, Catalog catalog)
        {
            Bounds b=new Bounds();bool first=true;
            for(int i=0;i<catalog.pieces.Length;i++)
            {
                // Stable row/column order matches the exported manifest/CSV.
                float x=(i%8-3.5f)*7.15f;
                float z=(1.5f-i/8)*8.75f;
                var instance=Place(catalog.pieces[i].id,root.transform,scene,new Vector3(x,0,z));
                foreach(var renderer in instance.GetComponentsInChildren<MeshRenderer>())
                    if(first){b=renderer.bounds;first=false;}else b.Encapsulate(renderer.bounds);
            }
            if(first)throw new InvalidOperationException("Empty kit gallery");
            File.WriteAllText(Path.Combine(
                Environment.GetEnvironmentVariable("KIT_CAPTURE_OUTPUT")??Path.GetFullPath("../artifacts/kit-phase2"),"kit-grid-index.csv"),
                "row,column,id\n"+string.Join("\n",catalog.pieces.Select((e,i)=>(i/8+1)+","+(i%8+1)+","+e.id))+"\n");
            return b;
        }

        static Bounds BuildVillage(GameObject root, Scene scene)
        {
            var architecture=new GameObject("Building kit prefabs only").transform;
            architecture.SetParent(root.transform,false);
            var b=new Bounds();bool first=true;
            Action<string,Vector3,float> add=(id,at,yaw)=>
            {
                var g=Place(id,architecture,scene,at,yaw);
                foreach(var r in g.GetComponentsInChildren<MeshRenderer>())
                    if(first){b=r.bounds;first=false;}else b.Encapsulate(r.bounds);
            };
            BuildHouse(add,new Vector3(-11,0,4));
            BuildWorkshop(add,new Vector3(11,0,4));
            BuildCorruptFort(add,new Vector3(0,0,-13));
            if(first)throw new InvalidOperationException("Kit settlement has no geometry");
            return b;
        }
        static void BuildHouse(Action<string,Vector3,float> add,Vector3 center)
        {
            Action<string,float,float,float,float> p=(id,x,y,z,rot)=>add(id,center+new Vector3(x,y,z),rot);
            // True 8 × 8 intact house from 4m wall modules, no inherited Hero FBX.
            p("Wall_Door_Intact",-2,0,-4,0);p("Wall_Window_Intact",2,0,-4,0);
            p("Wall_Window_Intact",-2,0,4,180);p("Wall_Straight_Intact",2,0,4,180);
            for(int z=-2;z<=2;z+=4){p("Wall_Straight_Intact",-4,0,z,90);p("Wall_Straight_Intact",4,0,z,-90);}
            p("Roof_Gable",0,3.67f,0,0);
            p("Tower_Bell",6,0,1,0);p("Roof_Tower",6,7.56f,1,0);
            p("Balcony",-1.8f,2.42f,-4.6f,0);p("Shield",1.72f,1.36f,-4.53f,0);
            p("Beam",-4.1f,0,-3.85f,0);p("Beam",3.9f,0,3.84f,0);
            p("Chimney",-2,2.5f,2.4f,0);
        }
        static void BuildWorkshop(Action<string,Vector3,float> add,Vector3 center)
        {
            Action<string,float,float,float,float> p=(id,x,y,z,rot)=>add(id,center+new Vector3(x,y,z),rot);
            p("Wall_Door_Intact",-2,0,-4,0);p("Wall_Window_Intact",2,0,-4,0);
            p("Wall_Straight_Intact",-2,0,4,180);p("Wall_Window_Intact",2,0,4,180);
            p("Wall_Straight_Intact",-4,0,-2,90);p("Wall_Corner_Intact",-4,0,2,90);
            p("Wall_Straight_Intact",4,0,-2,-90);p("Wall_Straight_Intact",4,0,2,-90);
            p("Roof_Gable",0,3.67f,0,180);
            // Asymmetric timber lean-to and heavy forge stack create a different silhouette.
            p("Roof_Shed",5.95f,3.14f,-1.65f,90);
            for(float x=4.3f;x<8.5f;x+=3.8f)
                for(float z=-3.85f;z<.25f;z+=3.8f)p("Beam",x,0,z,0);
            p("Chimney",3.4f,2.25f,1.15f,0);
            p("Shield",-1.8f,1.42f,-4.53f,0);
            p("Balcony",.7f,2.6f,4.7f,180);
            p("Rock_Small",5.5f,0,-4.2f,0);
            p("Rock_Medium",6.3f,0,-5.1f,0);
        }
        static void BuildCorruptFort(Action<string,Vector3,float> add,Vector3 center)
        {
            Action<string,float,float,float,float> p=(id,x,y,z,rot)=>add(id,center+new Vector3(x,y,z),rot);
            p("Wall_Door_Damaged",-2,0,-4,0);p("Wall_Window_Damaged",2,0,-4,0);
            p("Wall_Straight_Damaged",-2,0,4,180);p("Wall_Corner_Damaged",2,0,4,180);
            p("Wall_Straight_Damaged",-4,0,-2,90);p("Wall_Window_Damaged",-4,0,2,90);
            p("Wall_Corner_Damaged",4,0,-2,-90);p("Wall_Straight_Damaged",4,0,2,-90);
            p("Roof_Gable",0,3.67f,0,0);
            p("Tower_Broken",6,0,1.4f,0);
            p("Corrupt_Roots",-2.4f,0,-4.4f,0);
            p("Corrupt_Roots",2.3f,0,-4.45f,0);
            p("Corrupt_Roots",4.25f,0,-1.6f,-90);
            p("Corrupt_Roots",6.45f,1.5f,1.25f,180);
            p("Crystal_Large",4.8f,0,-4.1f,0);
            p("Crystal_Medium",-2.75f,0,-4.4f,0);
            p("Crystal_Medium",5.90f,0,2.1f,0);
            p("Crystal_Small",2.4f,0,-4.8f,0);
            p("Crystal_Small",7.1f,0,-1.8f,0);
            p("Rock_Large",7.8f,0,3.3f,0);
            p("Rock_Medium",-5.9f,0,-3.3f,0);
            p("Fallen_Log",8.1f,0,-2.4f,0);
        }
        static void AddVegetation(GameObject root, Scene scene)
        {
            var props=new GameObject("Varying pines, paths, stones and living ground").transform;
            props.SetParent(root.transform,false);
            var rnd=new System.Random(2161126);
            var pinelist=new[]{
                new Vector2(-28,6),new Vector2(-29,-6),new Vector2(-26,19),
                new Vector2(27,8),new Vector2(30,-11),new Vector2(25,20),
                new Vector2(-20,26),new Vector2(-13,27),new Vector2(0,27),
                new Vector2(12,28),new Vector2(20,24),new Vector2(-30,-22),
                new Vector2(-19,-24),new Vector2(-12,-27),new Vector2(4,-29),
                new Vector2(14,-28),new Vector2(26,-23),new Vector2(-31,0),
                new Vector2(32,0),new Vector2(-21,14),new Vector2(20,15)
            };
            for(int i=0;i<pinelist.Length;i++)
            {
                var v=pinelist[i];var id=new[]{"Pine_Small","Pine_Medium","Pine_Tall"}[i%3];
                Place(id,props,scene,new Vector3(v.x+(float)rnd.NextDouble()*.9f,0,v.y),rnd.Next(0,360));
                if(i%3==0)
                    Place("Shrubs",props,scene,new Vector3(v.x+1.4f,0,v.y-1.8f),rnd.Next(0,360));
            }
            var tracks=new[]{
                new Vector2(-11,-1),new Vector2(-9,-6),new Vector2(-6,-9),
                new Vector2(-2,-9),new Vector2(2,-8),new Vector2(5,-6),
                new Vector2(8,-2),new Vector2(11,-1)
            };
            for(int i=0;i<tracks.Length;i++)
            {
                var v=tracks[i];
                Place("Path_Slab",props,scene,new Vector3(v.x, .02f,v.y),i%2==0?-11:14);
                if(i%2==0)Place("Tall_Grass",props,scene,new Vector3(v.x+1.85f,0,v.y+.6f),i*17);
            }
            for(int i=0;i<17;i++)
            {
                float a=i*2.399f;
                float x=(float)Math.Cos(a)*rnd.Next(14,29);
                float z=(float)Math.Sin(a)*rnd.Next(12,25);
                Place(i%3==0?"Rock_Medium":"Rock_Small",props,scene,new Vector3(x,0,z),rnd.Next(0,360));
                Place("Tall_Grass",props,scene,new Vector3(x+1.2f,0,z+.8f),rnd.Next(0,360));
            }
            Place("Fallen_Log",props,scene,new Vector3(-25,0,-5),125);
        }
        static void Ground(GameObject root)
        {
            const int n=105;const float step=.98f;
            var v=new Vector3[n*n];var uv=new Vector2[n*n];var col=new Color[n*n];
            var t=new int[(n-1)*(n-1)*6];
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                float xx=(x-(n-1)*.5f)*step;float zz=(z-(n-1)*.5f)*step;
                float h=.14f*(float)Math.Sin(xx*.21f)*(float)Math.Cos(zz*.18f)+.18f*(float)Math.Sin(zz*.47f);
                float flatten=Mathf.Clamp01((new Vector2(xx+11,zz-4).magnitude-4)/5f);
                flatten=Mathf.Min(flatten,Mathf.Clamp01((new Vector2(xx-11,zz-4).magnitude-4)/5f));
                flatten=Mathf.Min(flatten,Mathf.Clamp01((new Vector2(xx,zz+13).magnitude-4)/5f));
                h*=flatten;
                // The working buildings are never raised onto independent polygonal pads.
                int i=z*n+x;v[i]=new Vector3(xx,h-.04f,zz);
                float rough=.5f+.18f*Mathf.Sin(xx*.58f+zz*.43f)+.15f*Mathf.Cos(xx*.27f-zz*.64f);
                var moss=Color.Lerp(new Color(.45f,.72f,.42f),new Color(.62f,.78f,.50f),Mathf.Clamp01(rough));
                float road=Mathf.Exp(-Mathf.Pow(xx*.18f+zz*.09f,2))*Mathf.Clamp01((1-zz/25f));
                float apron=Mathf.Exp(-new Vector2(xx+11,zz-4).sqrMagnitude/50f) +
                            Mathf.Exp(-new Vector2(xx-11,zz-4).sqrMagnitude/50f) +
                            Mathf.Exp(-new Vector2(xx,zz+13).sqrMagnitude/55f);
                col[i]=Color.Lerp(moss,new Color(.79f,.62f,.45f),Mathf.Clamp01(road*.48f+apron*.47f));
                uv[i]=new Vector2((1+(8+36f*x/(n-1))/64f)/4f,(8+36f*z/(n-1))/256f);
            }
            int q=0;for(int z=0;z<n-1;z++)for(int x=0;x<n-1;x++)
            {
                int i=z*n+x;t[q++]=i;t[q++]=i+n;t[q++]=i+1;t[q++]=i+1;t[q++]=i+n;t[q++]=i+n+1;
            }
            var mesh=new Mesh { indexFormat=IndexFormat.UInt32 };
            mesh.vertices=v;mesh.uv=uv;mesh.colors=col;mesh.triangles=t;mesh.RecalculateNormals();mesh.RecalculateBounds();
            var path=Kit+"/Materials/Kit_Terrain.asset";
            var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}
            else{EditorUtility.CopySerialized(mesh,saved);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);}
            var ground=new GameObject("Four-tone vertex-painted ground");
            ground.transform.SetParent(root.transform,false);
            ground.AddComponent<MeshFilter>().sharedMesh=saved;
            ground.AddComponent<MeshRenderer>().sharedMaterial=surface;
        }

        static void LowMist(GameObject root)
        {
            var shader=AssetDatabase.LoadAssetAtPath<Shader>(Phase1+"/Materials/Reboot_Atmosphere.shader");
            if(shader==null||ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Approved mist shader missing");
            var path=Kit+"/Materials/Kit_LowMist.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}
            material.shader=shader;material.SetFloat("_EmissionScale",.90f);
            for(int layer=0;layer<2;layer++)
            {
                const int n=21;var v=new Vector3[n*n];var c=new Color[n*n];var t=new int[(n-1)*(n-1)*6];
                for(int j=0;j<n;j++)for(int i=0;i<n;i++)
                {
                    float x=-37+i*3.7f,z=10+j*1.35f;
                    float edge=Mathf.Sin(Mathf.PI*i/(n-1))*Mathf.Sin(Mathf.PI*j/(n-1));
                    float pool=Mathf.SmoothStep(0,1,Mathf.InverseLerp(15,30,z))*.25f;
                    int k=j*n+i;v[k]=new Vector3(x,.16f+layer*.30f,z);
                    c[k]=new Color(.47f+.12f*layer,.35f,.48f,.12f*edge+pool*edge);
                }
                int q=0;for(int j=0;j<n-1;j++)for(int i=0;i<n-1;i++)
                {int a=j*n+i;t[q++]=a;t[q++]=a+n;t[q++]=a+1;t[q++]=a+1;t[q++]=a+n;t[q++]=a+n+1;}
                var mesh=new Mesh { indexFormat=IndexFormat.UInt32 };
                mesh.vertices=v;mesh.colors=c;mesh.triangles=t;mesh.RecalculateBounds();
                var mpath=Kit+"/Materials/Kit_Mist_"+layer+".asset";
                var saved=AssetDatabase.LoadAssetAtPath<Mesh>(mpath);
                if(saved==null){AssetDatabase.CreateAsset(mesh,mpath);saved=mesh;}
                else{EditorUtility.CopySerialized(mesh,saved);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(saved);}
                var go=new GameObject("Ground-distance mist "+layer);go.transform.SetParent(root.transform,false);
                go.AddComponent<MeshFilter>().sharedMesh=saved;
                var renderer=go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            }
        }
        static void ApplyDusk(GameObject root,bool dusk)
        {
            var skyShader=AssetDatabase.LoadAssetAtPath<Shader>(Kit+"/Materials/Kit_SunsetSky.shader");
            if(skyShader==null||ShaderUtil.ShaderHasError(skyShader))
                throw new InvalidOperationException("Phase2 dusk sky shader absent");
            var path=Kit+"/Materials/Kit_Sky.mat";
            var sky=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(sky==null){sky=new Material(skyShader);AssetDatabase.CreateAsset(sky,path);}
            sky.shader=skyShader;
            sky.SetColor("_Horizon",new Color(.89f,.50f,.29f));
            sky.SetColor("_Zenith",new Color(.23f,.19f,.43f));
            RenderSettings.skybox=sky;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.49f,.46f,.57f);
            RenderSettings.ambientEquatorColor=new Color(.35f,.36f,.53f);
            RenderSettings.ambientGroundColor=new Color(.21f,.22f,.31f);
            RenderSettings.fog=dusk;RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogStartDistance=50;RenderSettings.fogEndDistance=115;
            RenderSettings.fogColor=new Color(.53f,.40f,.46f);
            var sun=new GameObject("17 degree amber directional light").AddComponent<Light>();
            sun.transform.SetParent(root.transform,false);
            sun.type=LightType.Directional;
            sun.transform.rotation=Quaternion.Euler(17,-31,0);
            sun.color=new Color(1,.77f,.54f);sun.intensity=1.47f;
            sun.shadows=LightShadows.Soft;sun.shadowStrength=.67f;sun.shadowNormalBias=.11f;
            RenderSettings.sun=sun;
            var skyfill=new GameObject("Blue-violet diffuse open sky").AddComponent<Light>();
            skyfill.transform.SetParent(root.transform,false);skyfill.type=LightType.Directional;
            skyfill.transform.rotation=Quaternion.Euler(46,156,0);
            skyfill.color=new Color(.60f,.64f,.90f);skyfill.intensity=.40f;
            skyfill.shadows=LightShadows.None;
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Phase1+"/Materials/Village_Dusk.asset");
            if(profile==null)throw new InvalidOperationException("Owner-approved grading asset missing");
            var volume=new GameObject("Inherited Phase1 dusk tonemapping").AddComponent<Volume>();
            volume.transform.SetParent(root.transform,false);volume.isGlobal=true;volume.sharedProfile=profile;
        }
        static Camera NewCamera(GameObject root)
        {
            var c=new GameObject("Native URP camera").AddComponent<Camera>();
            c.transform.SetParent(root.transform,false);
            c.orthographic=true;c.nearClipPlane=.1f;c.farClipPlane=180f;
            c.allowHDR=true;c.clearFlags=CameraClearFlags.Skybox;
            var data=c.GetUniversalAdditionalCameraData();
            data.renderPostProcessing=true;
            data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            return c;
        }
        static void Fit(Camera camera,Bounds bounds,int width,int height,Vector3 euler,float padding)
        {
            var rot=Quaternion.Euler(euler);
            camera.transform.SetPositionAndRotation(bounds.center-rot*Vector3.forward*65f,rot);
            float xMax=0,yMax=0;
            for(int sx=-1;sx<=1;sx+=2)
            for(int sy=-1;sy<=1;sy+=2)
            for(int sz=-1;sz<=1;sz+=2)
            {
                var p=camera.transform.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(sx,sy,sz)));
                xMax=Mathf.Max(xMax,Mathf.Abs(p.x));yMax=Mathf.Max(yMax,Mathf.Abs(p.y));
            }
            camera.orthographicSize=Mathf.Max(yMax,xMax/((float)width/height))*padding;
        }
        static void Capture(Camera camera,int width,int height,string path)
        {
            var previous=RenderTexture.active;var previousTarget=camera.targetTexture;float prevAspect=camera.aspect;
            var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            Texture2D tex=null;
            try
            {
                if(!target.Create())throw new InvalidOperationException("Capture target allocation failed");
                camera.aspect=(float)width/height;
                var request=new RenderPipeline.StandardRequest{ destination=target };
                if(!RenderPipeline.SupportsRenderRequest(camera,request))
                {
                    camera.targetTexture=target;camera.Render();camera.targetTexture=previousTarget;
                }
                if(!RenderPipeline.SupportsRenderRequest(camera,request))
                    throw new InvalidOperationException("Native URP render request unavailable");
                for(int i=0;i<3;i++)
                {
                    var probe=new SphericalHarmonicsL2();
                    probe.AddAmbientLight(new Color(.39f,.38f,.45f));
                    RenderSettings.ambientProbe=probe;
                    RenderPipeline.SubmitRenderRequest(camera,request);
                }
                RenderTexture.active=target;
                tex=new Texture2D(width,height,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
                var pixels=tex.GetPixels32();
                int min=255,max=0,magenta=0;
                foreach(var pixel in pixels)
                {
                    int l=(pixel.r+pixel.g+pixel.b)/3;
                    min=Math.Min(min,l);max=Math.Max(max,l);
                    if(pixel.r>220&&pixel.b>220&&pixel.g<40)magenta++;
                }
                File.WriteAllBytes(path,tex.EncodeToPNG());
                File.WriteAllText(path+".metrics.txt",$"min={min} max={max} magenta={magenta} pixels={pixels.Length}");
                if(max-min<20||magenta>pixels.Length/1000)
                    throw new InvalidOperationException("Rejected near-empty or magenta Unity capture "+Path.GetFileName(path));
            }
            finally
            {
                camera.targetTexture=previousTarget;camera.aspect=prevAspect;
                RenderTexture.active=previous;
                if(tex!=null)Object.DestroyImmediate(tex);
                target.Release();Object.DestroyImmediate(target);
            }
        }
    }
}

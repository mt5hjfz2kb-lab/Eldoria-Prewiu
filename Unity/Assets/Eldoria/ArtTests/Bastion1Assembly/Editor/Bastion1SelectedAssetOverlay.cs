using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // Native-Unity visual candidate only. Every render mesh is imported from an
    // existing, SHA-pinned Blender GLB. No new geometry or gameplay hotspots.
    internal static class Bastion1SelectedAssetOverlay
    {
        const string Source = "Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedSource/SelectedAssets";
        struct Placement
        {
            public string Asset;
            public Vector3 Position;
            public float TargetMeters;
            public bool Height;
            public Placement(string asset,float x,float z,float metres,bool height)
            { Asset=asset; Position=new Vector3(x,0,z); TargetMeters=metres; Height=height; }
        }

        static readonly Placement[] Trial = {
            new Placement("pine_medium",-19,-12,7.0f,true),
            new Placement("pine_medium",19,-12,7.0f,true),
            new Placement("cliff_stone_chip",-21,12,1.65f,false),
            new Placement("cliff_stone_chip",21,12,1.65f,false)
        };
        static readonly Dictionary<string,string> Sha256 = new Dictionary<string,string> {
            {"pine_medium","003d59a9d184cb6cb04b2b2b21a8cb35c2b92e838cf788575044e576f7846e3e"},
            {"cliff_stone_chip","95c2f4e809a1c8c466f29f2d83cff2c3229956b5c772bdd8be21ac317de22fc5"}
        };

        public static async Task Apply(GameObject sourceRoot)
        {
            if(Environment.GetEnvironmentVariable("BASTION1_LIBRARY_PHASE")!="after")
                return; // The BEFORE is an untouched historical source, same camera.

            MeshFilter terrain=null;
            foreach(var filter in sourceRoot.GetComponentsInChildren<MeshFilter>(true))
            {
                if(filter.name.IndexOf("natural playable hilltop",StringComparison.OrdinalIgnoreCase)>=0)
                {terrain=filter;break;}
            }
            if(terrain==null||terrain.sharedMesh==null)
                throw new Exception("No original terrain to provide genuine surface contact");

            var ground=terrain.gameObject.AddComponent<MeshCollider>();
            ground.sharedMesh=terrain.sharedMesh;
            var container=new GameObject("Bastion I selected artist GLB candidate - NON CANONICAL");
            container.transform.SetParent(sourceRoot.transform,false);
            var report=new List<string>();
            try
            {
                foreach(var item in Trial)
                {
                    if(Mathf.Abs(item.Position.x)<8f)
                        throw new Exception("Selected asset overlaps protected central route");
                    float parcelDistance=Mathf.Min(
                        Vector2.Distance(new Vector2(item.Position.x,item.Position.z),new Vector2(-16,3)),
                        Vector2.Distance(new Vector2(item.Position.x,item.Position.z),new Vector2(16,3)));
                    if(parcelDistance<8f)
                        throw new Exception("Selected asset invades future buildable parcel");

                    RaycastHit hit;
                    if(!ground.Raycast(new Ray(new Vector3(item.Position.x,120,item.Position.z),
                                               Vector3.down),out hit,240f))
                        throw new Exception("Selected asset missing real terrain contact: "+item.Asset);
                    if(hit.normal.y<.65f)
                        throw new Exception("Selected asset on steep slope: "+item.Asset);
                    var glb=Path.GetFullPath(Source+"/"+item.Asset+".glb");
                    if(!File.Exists(glb))throw new FileNotFoundException("Pinned existing GLB missing",glb);
                    using(var stream=File.OpenRead(glb))
                    using(var hash=SHA256.Create())
                    {
                        var digest=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
                        if(digest!=Sha256[item.Asset])throw new Exception("Existing artist GLB SHA changed: "+item.Asset);
                    }
                    var importer=new GltfImport(null,new UninterruptedDeferAgent());
                    if(!await importer.Load(new Uri(glb).AbsoluteUri,new ImportSettings {
                        GenerateMipMaps=true,AnisotropicFilterLevel=2
                    }))throw new Exception("Cannot import selected GLB "+item.Asset);
                    var model=new GameObject("REUSED_"+item.Asset);
                    if(!await importer.InstantiateMainSceneAsync(model.transform))
                        throw new Exception("Cannot instantiate selected GLB "+item.Asset);
                    Bastion1MaterialBindings.Apply(model,importer,glb);
                    Bounds bounds=GetBounds(model);
                    float extent=item.Height?bounds.size.y:Mathf.Max(bounds.size.x,bounds.size.z);
                    if(extent<.01f)throw new Exception("Invalid existing asset size "+item.Asset);
                    float scale=item.TargetMeters/extent;
                    if(scale<.03f||scale>30f)
                        throw new Exception("Implausible source scale "+item.Asset+" "+scale);
                    model.transform.localScale=Vector3.one*scale;
                    bounds=GetBounds(model);
                    model.transform.position=hit.point-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                    model.transform.SetParent(container.transform,true);
                    if(Mathf.Abs(GetBounds(model).min.y-hit.point.y)>.025f)
                        throw new Exception("Asset float or penetration after placement "+item.Asset);
                    report.Add(item.Asset+" at "+hit.point+" scale="+scale.ToString("F4")+
                               " source_sha256="+Sha256[item.Asset]);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ground);
            }
            Debug.Log("BASTION1_SELECTED_ARTIST_GLBS_IMPORTED count="+report.Count+
                      " no_new_models=true gameplay_unmodified=true\n"+string.Join("\n",report));
        }

        static Bounds GetBounds(GameObject root)
        {
            var renderers=root.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)throw new Exception("Imported GLB has no renderer");
            var b=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)b.Encapsulate(renderers[i].bounds);
            return b;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    public static class BlenderLODReview
    {
        const string SourceRoot="Assets/Eldoria/ArtTests/ToolchainProof/Generated/";
        const string OutputRoot="BlenderCapabilityProof";

        [Serializable] class Metrics
        {
            public int lod;
            public long triangles;
            public long vertices;
            public int meshes;
            public int renderers;
            public int materials;
            public bool uvPresent;
            public bool normalsPresent;
            public Vector3 rawBounds;
            public bool cityCaptureNonEmpty;
            public bool detailCaptureNonEmpty;
        }

        [Serializable] class Summary
        {
            public Metrics[] lods;
            public bool boundsStable;
            public bool trianglesMonotonic;
            public bool uvNormalsPreserved;
            public bool capturesValid;
        }

        public static void Run()
        {
            SceneSetup.SetupRenderPipeline();
            Directory.CreateDirectory(OutputRoot);
            var all=new List<Metrics>();

            for(int lod=0;lod<3;lod++)
            {
                string source=SourceRoot+"lod"+lod+".glb";
                if(!File.Exists(source)) throw new FileNotFoundException(source);
                AssetDatabase.ImportAsset(source,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(source);
                if(prefab==null) throw new Exception("Could not import "+source);

                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var go=UnityEngine.Object.Instantiate(prefab);
                go.name="Blender LOD Proof "+lod;

                var raw=BoundsOf(go);
                float span=Mathf.Max(raw.size.x,raw.size.z);
                if(span<=.001f) throw new Exception("Invalid bounds LOD "+lod);
                go.transform.localScale*=18f/span;
                var b=BoundsOf(go);
                go.transform.position+=new Vector3(-b.center.x,-b.min.y,-b.center.z);

                foreach(var mf in go.GetComponentsInChildren<MeshFilter>())
                {
                    if(mf.sharedMesh==null) continue;
                    var mc=mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh=mf.sharedMesh;
                }

                var terrain=GameObject.CreatePrimitive(PrimitiveType.Plane);
                terrain.transform.localScale=new Vector3(5,1,5);
                terrain.transform.position=new Vector3(0,-.08f,0);
                ApplyColor(terrain,new Color(.38f,.36f,.29f));

                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.58f,.61f,.64f);
                var sun=new GameObject("Review light").AddComponent<Light>();
                sun.type=LightType.Directional; sun.intensity=1.05f; sun.shadows=LightShadows.Soft;
                sun.transform.rotation=Quaternion.Euler(42,-38,0);

                b=BoundsOf(go);
                var cam=new GameObject("LOD camera").AddComponent<Camera>();
                cam.orthographic=true;
                cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=new Color(.64f,.72f,.8f);
                cam.transform.position=b.center+new Vector3(18,14,-25);
                cam.transform.LookAt(b.center+Vector3.up*(b.extents.y*.10f));

                var m=Measure(go,lod,raw.size);
                string folder=OutputRoot+"/LOD"+lod;
                Directory.CreateDirectory(folder);
                cam.orthographicSize=12f;
                m.cityCaptureNonEmpty=Save(cam,folder+"/city.png");
                cam.orthographicSize=9f;
                m.detailCaptureNonEmpty=Save(cam,folder+"/detail.png");
                File.WriteAllText(folder+"/metrics.json",JsonUtility.ToJson(m,true));
                all.Add(m);
            }

            var summary=new Summary{lods=all.ToArray()};
            summary.trianglesMonotonic=all.Count==3 && all[0].triangles>all[1].triangles && all[1].triangles>all[2].triangles;
            summary.uvNormalsPreserved=all.TrueForAll(x=>x.uvPresent&&x.normalsPresent);
            summary.capturesValid=all.TrueForAll(x=>x.cityCaptureNonEmpty&&x.detailCaptureNonEmpty);
            summary.boundsStable=BoundsClose(all[0].rawBounds,all[1].rawBounds,.03f)&&BoundsClose(all[0].rawBounds,all[2].rawBounds,.05f);
            File.WriteAllText(OutputRoot+"/summary.json",JsonUtility.ToJson(summary,true));

            if(!summary.trianglesMonotonic) throw new Exception("LOD triangles are not strictly decreasing.");
            if(!summary.uvNormalsPreserved) throw new Exception("LOD generation lost UVs or normals.");
            if(!summary.boundsStable) throw new Exception("LOD generation changed bounds beyond tolerance.");
            if(!summary.capturesValid) throw new Exception("LOD captures invalid.");
            Debug.Log("BLENDER_LOD_REVIEW_PASS "+JsonUtility.ToJson(summary));
        }

        static bool BoundsClose(Vector3 a,Vector3 b,float tolerance)
        {
            for(int i=0;i<3;i++)
            {
                float denom=Mathf.Max(.001f,Mathf.Abs(a[i]));
                if(Mathf.Abs(a[i]-b[i])/denom>tolerance) return false;
            }
            return true;
        }

        static Metrics Measure(GameObject go,int lod,Vector3 rawBounds)
        {
            var m=new Metrics{lod=lod,rawBounds=rawBounds};
            var mats=new HashSet<Material>();
            foreach(var mf in go.GetComponentsInChildren<MeshFilter>())
            {
                var mesh=mf.sharedMesh;
                if(mesh==null) continue;
                m.meshes++; m.vertices+=mesh.vertexCount;
                bool uv=mesh.uv!=null&&mesh.uv.Length==mesh.vertexCount;
                bool normals=mesh.normals!=null&&mesh.normals.Length==mesh.vertexCount;
                m.uvPresent|=uv; m.normalsPresent|=normals;
                for(int s=0;s<mesh.subMeshCount;s++) m.triangles+=(long)mesh.GetIndexCount(s)/3;
            }
            foreach(var r in go.GetComponentsInChildren<Renderer>())
            {
                m.renderers++;
                foreach(var mat in r.sharedMaterials) if(mat!=null) mats.Add(mat);
            }
            m.materials=mats.Count;
            if(m.triangles<=0||!m.uvPresent||!m.normalsPresent||m.materials<=0) throw new Exception("LOD "+lod+" lost required data.");
            return m;
        }

        static Bounds BoundsOf(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>();
            if(rs.Length==0) throw new Exception("No renderer.");
            var b=rs[0].bounds;
            foreach(var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static void ApplyColor(GameObject go,Color color)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            var m=new Material(shader); m.color=color;
            go.GetComponent<Renderer>().sharedMaterial=m;
        }

        static bool Save(Camera cam,string path)
        {
            var rt=new RenderTexture(1280,720,24);
            var prev=RenderTexture.active;
            try
            {
                cam.targetTexture=rt; cam.Render(); RenderTexture.active=rt;
                var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,1280,720),0,0); tex.Apply();
                var px=tex.GetPixels32(); byte min=255,max=0;
                for(int i=0;i<px.Length;i+=257)
                {
                    byte l=(byte)((px[i].r+px[i].g+px[i].b)/3);
                    if(l<min)min=l;if(l>max)max=l;
                }
                var png=tex.EncodeToPNG();
                File.WriteAllBytes(path,png);
                UnityEngine.Object.DestroyImmediate(tex);
                return png.Length>10000&&(max-min)>=8;
            }
            finally
            {
                cam.targetTexture=null; RenderTexture.active=prev; rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}

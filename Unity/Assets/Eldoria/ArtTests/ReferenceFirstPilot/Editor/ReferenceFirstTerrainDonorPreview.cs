using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools.ReferenceFirstPilot
{
    // Isolated A/B: inspect actual existing large cliff/terrace GLBs before replacing any terrain.
    public static class ReferenceFirstTerrainDonorPreview
    {
        const string Root = "Assets/Eldoria/ArtTests/ReferenceFirstPilot/TerrainDonors";
        const string SourceA = "Assets/Eldoria/Resources/Valoria/TerrainTerraceKit_v1/BroadRockPlatform.glb";
        const string SourceB = "Assets/Eldoria/Resources/Valoria/TerrainTerraceKit_v1/SteppedRockTerrace.glb";
        const int W=1536,H=1024;
        [Serializable] private class Evidence
        {
            public string test="NATIVE_UNITY_TERRAIN_DONOR_AB_UNAPPROVED";
            public string reference="Owner image 1513x1039; cliff ledges below two defensive gatehouses";
            public string sourceA=SourceA,sourceB=SourceB;
            public string[] frames={"31-broad-vs-stepped-oblique.png","32-broad-vs-stepped-high.png"};
            public string revision="";
            public int width=W,height=H;
            public int rendererCountA,rendererCountB;
            public float nativeLongestDimensionA,nativeLongestDimensionB;
            public bool artApproved=false,licenseVerified=false,productionModified=false;
        }
        public static void Capture()
        {
            if(!UnityEngine.Application.isBatchMode)throw new InvalidOperationException("Batchmode only.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            GameObject a=AssetDatabase.LoadAssetAtPath<GameObject>(SourceA);
            GameObject b=AssetDatabase.LoadAssetAtPath<GameObject>(SourceB);
            if(a==null || b==null)throw new FileNotFoundException("Missing exact donor GLB sources; do not fabricate replacement.");
            var lit=Shader.Find("Universal Render Pipeline/Lit");
            if(lit==null)throw new InvalidOperationException("URP/Lit unavailable");
            Directory.CreateDirectory(Root);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var main=new GameObject("Two real inherited terrain donor meshes, untouched source");
            var left=new GameObject("A | BroadRockPlatform").transform;
            var right=new GameObject("B | SteppedRockTerrace").transform;
            left.SetParent(main.transform,false);
            right.SetParent(main.transform,false);
            var boundsA=OriginalBounds(a);
            var boundsB=OriginalBounds(b);
            GameObject ga=PositionExisting(a,left,-24f,0,28f);
            GameObject gb=PositionExisting(b,right,24f,0,28f);

            // Keep imported source meshes and materials 100% original; ground is neutral for silhouette.
            var groundmat=new Material(lit);
            groundmat.SetColor("_BaseColor",new Color(.31f,.34f,.36f));
            groundmat.SetFloat("_Smoothness",.08f);
            var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name="Neutral floor for geometric evaluation, not part of donors";
            ground.transform.position=new Vector3(0,-.12f,0);
            ground.transform.localScale=new Vector3(15f,1f,11f);
            ground.GetComponent<Renderer>().sharedMaterial=groundmat;
            Object.DestroyImmediate(ground.GetComponent<Collider>());

            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.48f,.50f,.55f);
            RenderSettings.fog=false;
            var sun=new GameObject("Identical A/B diffuse studio sun").AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.color=new Color(1f,.89f,.77f);
            sun.intensity=1.38f;
            sun.shadows=LightShadows.Soft;
            sun.transform.rotation=Quaternion.Euler(42f,-38f,0f);
            var cam=new GameObject("A/B fixed comparison camera").AddComponent<Camera>();
            cam.orthographic=true;
            cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.25f,.32f,.40f);
            cam.nearClipPlane=.15f;cam.farClipPlane=900f;

            if(!EditorSceneManager.SaveScene(scene,Root+"/RealTerrainDonorAB.unity"))
                throw new IOException("Cannot save genuine donor scene");
            string outdir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                "../../artifacts/reference-first-terrain-donors"));
            Directory.CreateDirectory(outdir);
            Save(cam,outdir,"31-broad-vs-stepped-oblique.png",new Vector3(55,56,-73),new Vector3(0,8,0),44f);
            Save(cam,outdir,"32-broad-vs-stepped-high.png",new Vector3(24,82,-45),new Vector3(0,7,0),43f);
            var proof=new Evidence {
                revision=Environment.GetEnvironmentVariable("GITHUB_SHA")??"LOCAL",
                rendererCountA=ga.GetComponentsInChildren<Renderer>(true).Length,
                rendererCountB=gb.GetComponentsInChildren<Renderer>(true).Length,
                nativeLongestDimensionA=Mathf.Max(boundsA.size.x,boundsA.size.y,boundsA.size.z),
                nativeLongestDimensionB=Mathf.Max(boundsB.size.x,boundsB.size.y,boundsB.size.z)
            };
            File.WriteAllText(Path.Combine(outdir,"terrain-donor-manifest.json"),JsonUtility.ToJson(proof,true));
            Debug.Log("REFERENCE_FIRST_TERRAIN_DONORS_CAPTURED: exact two GLBs, two PNG; ART_APPROVED=false");
        }
        static Bounds OriginalBounds(GameObject source)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(source);
            var b=BoundsOf(go);
            Object.DestroyImmediate(go);
            return b;
        }
        static Bounds BoundsOf(GameObject go)
        {
            bool valid=false;
            var bounds=new Bounds(go.transform.position,Vector3.zero);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
                if(r.enabled && r.gameObject.activeInHierarchy)
                {
                    if(!valid){bounds=r.bounds;valid=true;}
                    else bounds.Encapsulate(r.bounds);
                }
            if(!valid || bounds.size.sqrMagnitude<.001f)
                throw new InvalidOperationException("No real mesh renderer in donor "+go.name);
            return bounds;
        }
        static GameObject PositionExisting(GameObject source,Transform parent,float centerX,float centerZ,float longest)
        {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(source);
            if(go==null)throw new InvalidOperationException("Cannot import GLB "+source.name);
            go.transform.SetParent(parent,true);
            var b=BoundsOf(go);
            float factor=Mathf.Clamp(longest/Mathf.Max(b.size.x,b.size.y,b.size.z),.001f,80f);
            go.transform.localScale*=factor;
            b=BoundsOf(go);
            go.transform.position+=new Vector3(centerX-b.center.x,0f-b.min.y,centerZ-b.center.z);
            return go;
        }
        static void Save(Camera camera,string outdir,string filename,Vector3 pos,Vector3 target,float size)
        {
            camera.transform.position=pos;camera.transform.LookAt(target);
            camera.orthographicSize=size;
            var rt=new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);
            var before=RenderTexture.active;
            Texture2D image=null;
            try
            {
                camera.targetTexture=rt;
                camera.Render();camera.Render();camera.Render();
                RenderTexture.active=rt;
                image=new Texture2D(W,H,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,W,H),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(outdir,filename),image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=null;RenderTexture.active=before;
                if(image!=null)Object.DestroyImmediate(image);
                rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}

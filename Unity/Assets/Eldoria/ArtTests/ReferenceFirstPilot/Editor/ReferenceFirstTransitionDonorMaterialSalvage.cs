using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools.ReferenceFirstPilot
{
    // Independent, native Unity donor evaluation; no changes to player content or imported source.
    public static class ReferenceFirstTransitionDonorMaterialSalvage
    {
        const string Root="Assets/Eldoria/ArtTests/ReferenceFirstPilot/TransitionDonorsStoneStudy";
        const int W=1536,H=1024;
        static readonly string[] Paths={
            "Assets/Eldoria/Resources/Valoria/Rescued/RockTerrainSeamFiller.glb",
            "Assets/Eldoria/Resources/Valoria/Rescued/StreetLandingTransition.glb",
            "Assets/Eldoria/Resources/Valoria/Rescued/TerraceStairRock.glb",
            "Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/RockToWallTransition.glb"
        };
        static readonly string[] Labels={
            "A - Seam Filler","B - Street Landing","C - Terrace Stair","D - Rock to Wall"
        };
        [Serializable] sealed class Evidence
        {
            public string stage="NATIVE_UNITY_V18_STONE_LIT_MATERIAL_SALVAGE_STUDY";
            public string reference="Canonical owner supplied fortified plateau image 1513x1039";
            public string revision;
            public string[] sources=Paths;
            public string[] positions={"A left front","B right front","C left rear","D right rear"};
            public string[] frames={"37-stone-study-oblique.png","38-stone-study-high.png"};
            public int[] renderers=new int[4];
            public int[] originalMaterialSlotCounts=new int[4];
            public string[] originalShaderNames=new string[4];
            public bool allRendererMaterialsReplacedInSceneOnly=false;
            public string rockTexture="Assets/Eldoria/ArtTests/OriginalHero/Textures/rock.png";
            public string limestoneTexture="Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png";
            public string[] materialPolicy={
                "A: original GLB geometry, URP Lit project rock texture",
                "B: original GLB geometry, URP Lit project limestone texture",
                "C: original GLB geometry, URP Lit project limestone texture",
                "D: original GLB materials unchanged, baseline reference"
            };
            public float[] originalLongestDimensions=new float[4];
            public int width=W,height=H;
            public bool artApproved=false,licenseVerified=false,productionModified=false;
        }
        public static void Capture()
        {
            if(!UnityEngine.Application.isBatchMode)throw new InvalidOperationException("Batchmode only");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var sources=new GameObject[4];
            for(int i=0;i<4;i++)
            {
                sources[i]=AssetDatabase.LoadAssetAtPath<GameObject>(Paths[i]);
                if(sources[i]==null)throw new FileNotFoundException("Actual modular donor source absent: "+Paths[i]);
            }
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)throw new InvalidOperationException("URP/Lit missing");
            Directory.CreateDirectory(Root);
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("REFERENCE FIRST 4 ORIGINAL DONOR MESHES - NOT APPROVED").transform;
            var mat=new Material(shader);
            mat.SetColor("_BaseColor",new Color(.37f,.41f,.43f));
            mat.SetFloat("_Smoothness",.12f);
            var patch=GameObject.CreatePrimitive(PrimitiveType.Plane);
            patch.name="Shared neutral ground - not a candidate asset";
            patch.transform.SetParent(root,false);
            patch.transform.localScale=new Vector3(14f,1f,14f);
            patch.transform.position=new Vector3(0f,-.18f,0f);
            patch.GetComponent<Renderer>().sharedMaterial=mat;
            Object.DestroyImmediate(patch.GetComponent<Collider>());

            var evidence=new Evidence{revision=Environment.GetEnvironmentVariable("GITHUB_SHA")??"LOCAL"};
            // Study-only URP PBR materials from EXISTING project-owned texture files.
            // No mesh replacement; D keeps original materials as a control sample.
            Texture2D rockTex=AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/rock.png");
            Texture2D limeTex=AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Eldoria/ArtTests/OriginalHero/Textures/limestone.png");
            if(rockTex==null||limeTex==null)throw new FileNotFoundException("Project stone albedo missing");
            Material rockMat=StudyMaterial(Root+"/StudyStoneRock.mat",shader,rockTex);
            Material limeMat=StudyMaterial(Root+"/StudyStoneLime.mat",shader,limeTex);
            for(int i=0;i<4;i++)
            {
                int col=i%2,row=i/2;
                float x=(col==0?-30f:30f);
                float z=(row==0?-31f:31f);
                var parent=new GameObject(Labels[i]).transform;
                parent.SetParent(root,false);
                var go=(GameObject)PrefabUtility.InstantiatePrefab(sources[i]);
                if(go==null)throw new InvalidOperationException("Cannot instantiate "+Paths[i]);
                go.transform.SetParent(parent,true);
                var b=BoundsOf(go);
                evidence.originalLongestDimensions[i]=Mathf.Max(b.size.x,b.size.y,b.size.z);
                go.transform.localScale*=Mathf.Clamp(23f/evidence.originalLongestDimensions[i],.001f,200f);
                b=BoundsOf(go);
                go.transform.position+=new Vector3(x-b.center.x,-b.min.y,z-b.center.z);
                var rr=go.GetComponentsInChildren<Renderer>(true);
                evidence.renderers[i]=rr.Length;
                var shaders=new System.Collections.Generic.HashSet<string>();
                foreach(var renderer in rr)
                {
                    var imported=renderer.sharedMaterials;
                    var overrideSlots=new Material[imported.Length];
                    for(int j=0;j<imported.Length;j++)
                    {
                        var m=imported[j];
                        shaders.Add(m==null ? "(missing material)" :
                                    (m.shader==null ? "(missing shader)" : m.shader.name));
                        overrideSlots[j]=i==0?rockMat:limeMat;
                        evidence.originalMaterialSlotCounts[i]++;
                    }
                    if(i<3)renderer.sharedMaterials=overrideSlots; // only first 3 copies; D original
                }
                evidence.originalShaderNames[i]=string.Join("; ",shaders);
                if(evidence.renderers[i]<1)throw new InvalidOperationException("Source has no renderers: "+Paths[i]);
            }
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.27f,.30f,.33f);
            RenderSettings.fog=false;
            var sun=new GameObject("Identical fixed A B C D light").AddComponent<Light>();
            sun.type=LightType.Directional;
            sun.intensity=.94f;
            sun.color=new Color(1f,.89f,.81f);
            sun.shadows=LightShadows.Soft;
            sun.transform.rotation=Quaternion.Euler(40f,-35f,0f);
            var camera=new GameObject("Two controlled same-scene native cameras").AddComponent<Camera>();
            camera.orthographic=true;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.24f,.31f,.39f);
            camera.nearClipPlane=.1f;
            camera.farClipPlane=950f;
            if(!EditorSceneManager.SaveScene(scene,Root+"/FourTransitionDonorsStoneStudy.unity"))
                throw new IOException("Cannot save isolated source-comparison scene");
            string output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                "../../artifacts/reference-first-transition-stone-study"));
            Directory.CreateDirectory(output);
            Save(camera,output,evidence.frames[0],new Vector3(84f,107f,-108f),new Vector3(0f,5f,0f),73f);
            Save(camera,output,evidence.frames[1],new Vector3(30f,150f,-60f),new Vector3(0f,4f,0f),65f);
            File.WriteAllText(Path.Combine(output,"transition-donor-manifest.json"),JsonUtility.ToJson(evidence,true));
            Debug.Log("REFERENCE_FIRST_TRANSITION_STONE_STUDY_RENDERED: 3 temporary project-stone material replacements, D unchanged; two PNG; artApproved=false");
        }
        static Material StudyMaterial(string path,Shader shader,Texture2D albedo)
        {
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}
            m.shader=shader;
            m.SetColor("_BaseColor",new Color(.67f,.62f,.58f));
            m.SetTexture("_BaseMap",albedo);
            m.SetFloat("_Metallic",0f);
            m.SetFloat("_Smoothness",.045f);
            EditorUtility.SetDirty(m);
            return m;
        }
        static Bounds BoundsOf(GameObject go)
        {
            bool valid=false;
            var b=new Bounds(go.transform.position,Vector3.zero);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                if(!r.enabled||!r.gameObject.activeInHierarchy)continue;
                if(!valid){b=r.bounds;valid=true;}else b.Encapsulate(r.bounds);
            }
            if(!valid||b.size.sqrMagnitude<.0001f)throw new InvalidOperationException("Empty native donor renderer");
            return b;
        }
        static void Save(Camera camera,string directory,string filename,Vector3 p,Vector3 target,float orthographicSize)
        {
            camera.transform.position=p;
            camera.transform.LookAt(target);
            camera.orthographicSize=orthographicSize;
            var rt=new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);
            var prior=RenderTexture.active;
            Texture2D png=null;
            try
            {
                camera.targetTexture=rt;
                camera.Render();camera.Render();camera.Render();
                RenderTexture.active=rt;
                png=new Texture2D(W,H,TextureFormat.RGB24,false);
                png.ReadPixels(new Rect(0,0,W,H),0,0);png.Apply();
                File.WriteAllBytes(Path.Combine(directory,filename),png.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=null;RenderTexture.active=prior;
                if(png!=null)Object.DestroyImmediate(png);
                rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}

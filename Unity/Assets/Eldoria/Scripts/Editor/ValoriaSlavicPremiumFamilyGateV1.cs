using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Bounded proof to choose a better existing secondary-architecture family before any paid generation.
    public static class ValoriaSlavicPremiumFamilyGateV1
    {
        const string Folder="ValoriaSlavicPremiumFamilyV1Captures";

        static readonly (string id,string path)[] Candidates={
            ("town_house_01","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab"),
            ("town_house_02","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_02a_PRE.prefab"),
            ("town_house_03a","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03a_PRE.prefab"),
            ("town_house_03b","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03b_PRE.prefab"),
            ("town_house_03c","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03c_PRE.prefab"),
            ("admin_01a","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Administrative/EA03_Town_Building_Administrative _01a_PRE.prefab"),
            ("admin_01c","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Administrative/EA03_Town_Building_Administrative _01c_PRE.prefab"),
            ("village_tower","Assets/EmaceArt/Slavic World Free/Prefabs/Village/Building/Other/EA03_Village_Tover_01a_PRE.prefab")
        };

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

            var root=new GameObject("Valoria · Slavic Premium Family Proof").transform;
            var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name="proof ground";ground.transform.SetParent(root,true);ground.transform.position=Vector3.zero;ground.transform.localScale=new Vector3(4.5f,1f,3f);
            ground.GetComponent<Renderer>().sharedMaterial=Valoria.Presentation.ValoriaKit.Material(new Color(.33f,.31f,.26f,1f));
            Object.DestroyImmediate(ground.GetComponent<Collider>());

            var lightGo=new GameObject("key");
            lightGo.transform.rotation=Quaternion.Euler(43f,-36f,0f);
            var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(1f,.91f,.80f);light.intensity=1.25f;light.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.68f,.74f,.78f);
            RenderSettings.ambientEquatorColor=new Color(.48f,.46f,.42f);
            RenderSettings.ambientGroundColor=new Color(.23f,.21f,.18f);
            RenderSettings.ambientIntensity=.92f;

            var positions=new[]{
                new Vector3(-7.5f,0f,3.3f),new Vector3(-2.5f,0f,3.3f),new Vector3(2.5f,0f,3.3f),new Vector3(7.5f,0f,3.3f),
                new Vector3(-7.5f,0f,-3.3f),new Vector3(-2.5f,0f,-3.3f),new Vector3(2.5f,0f,-3.3f),new Vector3(7.5f,0f,-3.3f)
            };

            int loaded=0;
            for(int i=0;i<Candidates.Length;i++)
            {
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(Candidates[i].path);
                if(source==null)continue;
                var go=(GameObject)PrefabUtility.InstantiatePrefab(source);
                go.name="candidate · "+Candidates[i].id;
                Fit(go,positions[i],3.7f,4.2f);
                foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
                loaded++;
                Label(Candidates[i].id,positions[i]+new Vector3(0f,.05f,-2.15f));
            }
            if(loaded<5)throw new System.Exception("Too few Slavic premium candidates loaded: "+loaded);

            var camGo=new GameObject("Main Camera");camGo.tag="MainCamera";
            var ccam=camGo.AddComponent<Camera>();
            ccam.clearFlags=CameraClearFlags.SolidColor;ccam.backgroundColor=new Color(.51f,.62f,.68f);
            ccam.orthographic=true;ccam.orthographicSize=8.2f;
            ccam.transform.position=new Vector3(14.5f,11.5f,-19f);ccam.transform.LookAt(new Vector3(0f,1.6f,0f));
            Save(ccam,Folder+"/family-grid.png",1536,864);

            for(int i=0;i<Candidates.Length;i++)
            {
                var go=GameObject.Find("candidate · "+Candidates[i].id);if(go==null)continue;
                foreach(var other in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    if(other==null)continue;
                    bool keep=other.transform.IsChildOf(go.transform)||other.transform==go.transform||other.gameObject.name=="proof ground";
                    other.enabled=keep;
                }
                ccam.orthographicSize=3.8f;
                ccam.transform.position=positions[i]+new Vector3(7.2f,5.6f,-8.7f);
                ccam.transform.LookAt(positions[i]+Vector3.up*1.7f);
                Save(ccam,Folder+"/"+Candidates[i].id+".png",960,720);
                foreach(var other in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))other.enabled=true;
            }

            File.WriteAllText(Folder+"/evidence.json","{\n  \"candidate_count\": "+loaded+",\n  \"source\": \"existing EmaceArt Slavic World Free prefabs\",\n  \"paid_assets\": false,\n  \"tripo_credits\": 0\n}\n");
            EditorApplication.Exit(0);
        }

        static void Fit(GameObject go,Vector3 ground,float span,float height)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return;
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            float s=Mathf.Min(span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z)),height/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=s;
            rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void Label(string label,Vector3 p)
        {
            var t=new GameObject("label "+label).AddComponent<TextMesh>();
            t.text=label;t.fontSize=48;t.characterSize=.09f;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;
            t.transform.position=p;t.transform.rotation=Quaternion.Euler(55f,0f,0f);
        }

        static void Save(Camera c,string path,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
            finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}

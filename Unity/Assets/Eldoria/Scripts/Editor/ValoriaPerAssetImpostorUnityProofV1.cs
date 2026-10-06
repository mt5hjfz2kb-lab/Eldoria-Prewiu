using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    public static class ValoriaPerAssetImpostorUnityProofV1
    {
        const string OutFolder = "ValoriaPerAssetImpostorUnityProofV1Captures";
        static Camera cam;
        static GameObject visual;
        static Material visualMat;
        static Texture2D repairedTex, brokenTex;
        static Vector3 viewDir, right, up;
        static readonly List<GameObject> generated = new();

        [MenuItem("Eldoria/R&D/Capture Per-Asset Impostor Proof v1")]
        public static void Capture()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = Directory.GetParent(Application.dataPath).Parent.FullName;
            var sourceDir = Path.Combine(root,"docs","evidence","valoria-golden-lookdev-slice-v1","camera-first-depth-shell-v1");
            var back = LoadPng(Path.Combine(sourceDir,"gate-back.png"));
            var front = LoadPng(Path.Combine(sourceDir,"gate-front.png"));
            repairedTex = TightComposite(back,front,18);
            brokenTex = MakeBrokenVariant(repairedTex);

            var outDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName,OutFolder);
            Directory.CreateDirectory(outDir);
            File.WriteAllBytes(Path.Combine(outDir,"gate-repaired.png"), repairedTex.EncodeToPNG());
            File.WriteAllBytes(Path.Combine(outDir,"gate-broken-proof.png"), brokenTex.EncodeToPNG());

            BuildScene();

            var evidence = new Dictionary<string,object>();
            evidence["method"]="PER-ASSET ISOMETRIC IMPOSTOR: exact transparent canonical building representation anchored to functional 3D proxy; no full-scene plate";
            evidence["camera_contract"]="orientation locked; orthographic pan+zoom allowed; no rotation";
            evidence["target_texture_size"]=new[]{repairedTex.width,repairedTex.height};
            evidence["functional_proxy_colliders"]=3;
            evidence["visual_quad_count"]=1;
            evidence["visual_state_count"]=2;
            evidence["paid_credits"]=0;
            evidence["visual_source"]="canonical semantic Gate composite (tight alpha bbox)";
            evidence["dynamic_depth_test"]="one unit in front of visual plane; one unit behind and visible through alpha opening";
            evidence["limitations"]="single per-asset impostor cannot support arbitrary camera rotation; complex walk-through occlusion may need bounded front/back impostor slices or sorting zones";

            visualMat.mainTexture = repairedTex;
            CapturePng(outDir,"home-repaired.png",1280,720,Vector3.zero,5.2f);
            CapturePng(outDir,"pan-left.png",1280,720,-right*1.35f,5.2f);
            CapturePng(outDir,"pan-right.png",1280,720,right*1.35f,5.2f);
            CapturePng(outDir,"zoom-in.png",1280,720,Vector3.zero,3.9f);
            CapturePng(outDir,"zoom-out.png",1280,720,Vector3.zero,6.6f);
            CapturePng(outDir,"portrait.png",720,1280,Vector3.zero,5.8f);
            CapturePng(outDir,"landscape.png",1280,720,Vector3.zero,5.2f);
            CapturePng(outDir,"golden-close.png",1280,720,Vector3.zero,3.35f);

            visualMat.mainTexture = brokenTex;
            CapturePng(outDir,"home-broken.png",1280,720,Vector3.zero,5.2f);
            visualMat.mainTexture = repairedTex;
            CapturePng(outDir,"home-repaired-after-state-swap.png",1280,720,Vector3.zero,5.2f);

            File.WriteAllText(Path.Combine(outDir,"evidence.json"),JsonUtility.ToJson(new EvidenceSerializable{
                tech="PASS_PENDING_VISUAL_INSPECTION",
                method=(string)evidence["method"],
                camera=(string)evidence["camera_contract"],
                source=(string)evidence["visual_source"],
                state_transition="BROKEN <-> REPAIRED texture swap on same functional proxy",
                occlusion=(string)evidence["dynamic_depth_test"],
                limitations=(string)evidence["limitations"],
                proxy_colliders=3,
                visual_quads=1,
                paid_credits=0
            },true));
            Debug.Log("PER-ASSET IMPOSTOR PROOF CAPTURED: "+outDir);
        }

        [Serializable] class EvidenceSerializable {
            public string tech,method,camera,source,state_transition,occlusion,limitations;
            public int proxy_colliders,visual_quads,paid_credits;
        }

        static Texture2D LoadPng(string path) {
            if(!File.Exists(path)) throw new FileNotFoundException(path);
            var t=new Texture2D(2,2,TextureFormat.RGBA32,false,true);
            if(!t.LoadImage(File.ReadAllBytes(path),false)) throw new Exception("LoadImage failed "+path);
            t.wrapMode=TextureWrapMode.Clamp; t.filterMode=FilterMode.Bilinear; return t;
        }

        static Texture2D TightComposite(Texture2D back, Texture2D front, int pad) {
            if(back.width!=front.width||back.height!=front.height) throw new Exception("layer size mismatch");
            int w=back.width,h=back.height;
            var a=back.GetPixels32(); var b=front.GetPixels32(); var c=new Color32[w*h];
            int minx=w,miny=h,maxx=-1,maxy=-1;
            for(int y=0;y<h;y++) for(int x=0;x<w;x++){
                int i=y*w+x;
                Color cb=a[i], cf=b[i];
                float af=cf.a/255f, ab=cb.a/255f;
                float ao=af+ab*(1-af);
                Color rgb=ao>0.0001f ? (cf*af + cb*ab*(1-af))/ao : Color.clear;
                var co=(Color32)new Color(rgb.r,rgb.g,rgb.b,ao);
                c[i]=co;
                if(co.a>8){minx=Math.Min(minx,x);maxx=Math.Max(maxx,x);miny=Math.Min(miny,y);maxy=Math.Max(maxy,y);}
            }
            if(maxx<minx) throw new Exception("empty composite");
            minx=Math.Max(0,minx-pad); miny=Math.Max(0,miny-pad); maxx=Math.Min(w-1,maxx+pad); maxy=Math.Min(h-1,maxy+pad);
            int tw=maxx-minx+1, th=maxy-miny+1;
            var outTex=new Texture2D(tw,th,TextureFormat.RGBA32,false,true);
            var pix=new Color32[tw*th];
            for(int y=0;y<th;y++) Array.Copy(c,(miny+y)*w+minx,pix,y*tw,tw);
            outTex.SetPixels32(pix); outTex.Apply(false,false); outTex.wrapMode=TextureWrapMode.Clamp; outTex.filterMode=FilterMode.Bilinear;
            return outTex;
        }

        static Texture2D MakeBrokenVariant(Texture2D src) {
            var t=new Texture2D(src.width,src.height,TextureFormat.RGBA32,false,true);
            var p=src.GetPixels32(); int w=src.width,h=src.height;
            // Mechanism-only damaged state: deterministic missing battlement chunks + dark fracture marks.
            for(int y=0;y<h;y++) for(int x=0;x<w;x++){
                int i=y*w+x;
                if(p[i].a<8) continue;
                float nx=x/(float)w, ny=y/(float)h;
                bool missing=(ny>0.73f && nx>0.58f && nx<0.72f && ((x+y)%17<9)) ||
                             (ny>0.62f && nx<0.22f && ((x*3+y)%23<8));
                if(missing) p[i].a=0;
                bool crack=Math.Abs((x-(int)(w*.63f)) - (h-y)*0.18f)<2.2f && ny>.22f && ny<.70f;
                if(crack){p[i].r=(byte)(p[i].r*.28f);p[i].g=(byte)(p[i].g*.25f);p[i].b=(byte)(p[i].b*.24f);}
            }
            t.SetPixels32(p); t.Apply(false,false); t.wrapMode=TextureWrapMode.Clamp; t.filterMode=FilterMode.Bilinear; return t;
        }

        static void BuildScene() {
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.42f,.46f,.53f);

            var cameraGo=new GameObject("ProofCamera"); generated.Add(cameraGo);
            cam=cameraGo.AddComponent<Camera>(); cam.orthographic=true; cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.055f,.07f,.085f); cam.nearClipPlane=.1f; cam.farClipPlane=100f;
            cam.transform.position=new Vector3(7.8f,-9.5f,7.2f);
            var target=new Vector3(0,0,2.4f);
            cam.transform.rotation=Quaternion.LookRotation(target-cam.transform.position,Vector3.up);
            viewDir=cam.transform.forward.normalized; right=cam.transform.right.normalized; up=cam.transform.up.normalized;

            // Ground/functional context.
            var ground=GameObject.CreatePrimitive(PrimitiveType.Plane); generated.Add(ground); ground.name="FunctionalGround";
            ground.transform.localScale=new Vector3(2.8f,1f,2.8f);
            ground.GetComponent<Renderer>().material=SolidMat(new Color(.11f,.14f,.12f));

            // Hidden functional proxy: three simple collider volumes, no art renderer.
            MakeProxy("GateProxy_Left",new Vector3(-2.15f,0,2.1f),new Vector3(1.8f,1.4f,4.2f));
            MakeProxy("GateProxy_Right",new Vector3(2.15f,0,2.1f),new Vector3(1.8f,1.4f,4.2f));
            MakeProxy("GateProxy_Top",new Vector3(0,0,3.25f),new Vector3(2.7f,1.2f,1.0f));

            // Visual quad lies on a plane perpendicular to the locked camera.
            float h=5.65f, w=h*repairedTex.width/(float)repairedTex.height;
            visual=new GameObject("Gate_VisualImpostor"); generated.Add(visual);
            var mf=visual.AddComponent<MeshFilter>(); var mr=visual.AddComponent<MeshRenderer>();
            mf.sharedMesh=QuadMesh(w,h);
            visual.transform.position=new Vector3(0,0,2.65f);
            visual.transform.rotation=Quaternion.LookRotation(-viewDir,up);
            visualMat=CutoutMat(repairedTex); mr.sharedMaterial=visualMat;

            // Unit in front of gate visual plane.
            var frontUnit=GameObject.CreatePrimitive(PrimitiveType.Sphere); generated.Add(frontUnit); frontUnit.name="DynamicUnit_FRONT";
            frontUnit.transform.localScale=Vector3.one*.52f;
            frontUnit.transform.position=visual.transform.position - viewDir*.75f + right*.9f - up*1.05f;
            frontUnit.GetComponent<Renderer>().material=SolidMat(new Color(.22f,.86f,.48f));

            // Unit behind plane, placed in arch opening so alpha cutout reveals it.
            var backUnit=GameObject.CreatePrimitive(PrimitiveType.Sphere); generated.Add(backUnit); backUnit.name="DynamicUnit_BEHIND_THROUGH_ARCH";
            backUnit.transform.localScale=Vector3.one*.48f;
            backUnit.transform.position=visual.transform.position + viewDir*.7f - up*1.15f;
            backUnit.GetComponent<Renderer>().material=SolidMat(new Color(.85f,.3f,.75f));
        }

        static void MakeProxy(string name,Vector3 pos,Vector3 scale){
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube); generated.Add(g); g.name=name; g.transform.position=pos; g.transform.localScale=scale;
            var r=g.GetComponent<Renderer>(); if(r) r.enabled=false;
        }

        static Mesh QuadMesh(float w,float h){
            var m=new Mesh{name="ImpostorQuad"};
            m.vertices=new[]{new Vector3(-w/2,-h/2,0),new Vector3(w/2,-h/2,0),new Vector3(w/2,h/2,0),new Vector3(-w/2,h/2,0)};
            m.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};
            m.triangles=new[]{0,2,1,0,3,2}; m.RecalculateNormals(); m.RecalculateBounds(); return m;
        }

        static Material CutoutMat(Texture2D tex){
            Shader sh=Shader.Find("Universal Render Pipeline/Unlit");
            if(sh==null) sh=Shader.Find("Unlit/Transparent Cutout");
            if(sh==null) throw new Exception("No suitable cutout shader");
            var m=new Material(sh){name="PerAssetImpostorCutout"};
            if(m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap",tex); else m.mainTexture=tex;
            if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",Color.white);
            if(m.HasProperty("_Surface")) m.SetFloat("_Surface",0);
            if(m.HasProperty("_AlphaClip")) m.SetFloat("_AlphaClip",1);
            if(m.HasProperty("_Cutoff")) m.SetFloat("_Cutoff",.025f);
            if(m.HasProperty("_AlphaCutoff")) m.SetFloat("_AlphaCutoff",.025f);
            if(m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite",1);
            m.EnableKeyword("_ALPHATEST_ON"); m.renderQueue=(int)RenderQueue.AlphaTest; m.SetOverrideTag("RenderType","TransparentCutout");
            return m;
        }

        static Material SolidMat(Color c){
            var sh=Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m=new Material(sh);
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c); else m.color=c;
            return m;
        }

        static void CapturePng(string folder,string name,int width,int height,Vector3 offset,float ortho){
            var basePos=new Vector3(7.8f,-9.5f,7.2f);
            cam.transform.position=basePos+offset;
            var target=new Vector3(0,0,2.4f)+offset;
            cam.transform.rotation=Quaternion.LookRotation(target-cam.transform.position,Vector3.up);
            cam.orthographicSize=ortho;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};
            cam.targetTexture=rt; var prev=RenderTexture.active; RenderTexture.active=rt; cam.Render();
            var tex=new Texture2D(width,height,TextureFormat.RGBA32,false); tex.ReadPixels(new Rect(0,0,width,height),0,0); tex.Apply();
            File.WriteAllBytes(Path.Combine(folder,name),tex.EncodeToPNG());
            cam.targetTexture=null; RenderTexture.active=prev; UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}

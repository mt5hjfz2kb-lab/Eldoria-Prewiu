using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    public static class EldoriaRadicalVisualProofGate
    {
        const string Folder="RadicalVisualProofCaptures";
        static readonly Vector3 CamPos=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CamTarget=new Vector3(0f,3.15f,5.8f);

        public static void Capture()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");

            Directory.CreateDirectory(Folder);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            RadicalLook.Apply();

            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Radical visual proof altered gameplay collider/hotspot signature.");

            Save(camera,Folder+"/after-19.png",19f,1280,720);
            Save(camera,Folder+"/after-12.png",12f,1280,720);
            Save(camera,Folder+"/after-9.png",9f,1280,720);
            Save(camera,Folder+"/after-mobile.png",12f,390,844);

            File.WriteAllText(Folder+"/proof.json",
                "{\n"+
                "  \"branch\": \"visual-proof/eldoria-radical-v1\",\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"new_paid_assets\": 0,\n"+
                "  \"scope\": [\"terrain framing\",\"surfaces\",\"lighting\",\"atmosphere\",\"architecture hierarchy\",\"dressing\"],\n"+
                "  \"official_zooms\": [19,12,9],\n"+
                "  \"mobile_capture\": true\n"+
                "}\n");
            UnityEditor.EditorApplication.Exit(0);
        }

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CamPos;
            camera.transform.LookAt(CamTarget);
            camera.orthographic=true;
            camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            camera.targetTexture=rt;
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.Render();
            RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);
            tex.Apply();
            File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;
            RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(rt);
        }
    }

    static class RadicalLook
    {
        static Transform Root;
        static Shader Lit;
        static readonly Color Limestone=new Color(.62f,.58f,.49f,1f);
        static readonly Color LimestoneLight=new Color(.72f,.68f,.59f,1f);
        static readonly Color Rock=new Color(.25f,.27f,.27f,1f);
        static readonly Color Earth=new Color(.30f,.26f,.20f,1f);
        static readonly Color Moss=new Color(.22f,.31f,.22f,1f);
        static readonly Color Blue=new Color(.08f,.22f,.38f,1f);
        static readonly Color Gold=new Color(.78f,.55f,.18f,1f);
        static readonly Color Violet=new Color(.34f,.12f,.48f,1f);

        public static void Apply()
        {
            var existing=GameObject.Find("RADICAL VISUAL PROOF v1");
            if(existing!=null)UnityEngine.Object.DestroyImmediate(existing);
            Root=new GameObject("RADICAL VISUAL PROOF v1").transform;
            Lit=Shader.Find("Universal Render Pipeline/Lit");
            if(Lit==null)throw new Exception("URP/Lit unavailable.");

            ConfigureAtmosphere();
            RegradeScene();
            BuildMountainFrame();
            BuildCirculation();
            BuildRuinsAndSkyline();
            BuildLifeAndVegetation();
        }

        static void ConfigureAtmosphere()
        {
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.68f,.73f,.76f);
            RenderSettings.ambientEquatorColor=new Color(.45f,.45f,.40f);
            RenderSettings.ambientGroundColor=new Color(.23f,.21f,.18f);
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.62f,.69f,.73f);
            RenderSettings.fogStartDistance=32f;
            RenderSettings.fogEndDistance=95f;

            var cam=Camera.main;
            if(cam!=null)
            {
                cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=new Color(.58f,.67f,.73f);
            }

            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l.type==LightType.Directional){l.intensity=.78f;l.color=new Color(1f,.88f,.70f);l.shadowStrength=.58f;}
            var fill=new GameObject("RADICAL · cool sky fill").AddComponent<Light>();
            fill.transform.SetParent(Root);fill.type=LightType.Directional;fill.intensity=.38f;
            fill.color=new Color(.58f,.70f,1f);fill.transform.rotation=Quaternion.Euler(36f,145f,0f);fill.shadows=LightShadows.None;
            var rim=new GameObject("RADICAL · warm bastion rim").AddComponent<Light>();
            rim.transform.SetParent(Root);rim.type=LightType.Point;rim.range=18f;rim.intensity=6f;
            rim.color=new Color(1f,.60f,.28f);rim.transform.position=new Vector3(-3f,9f,12f);rim.shadows=LightShadows.None;
        }

        static void RegradeScene()
        {
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||r.transform.IsChildOf(Root))continue;
                string n=r.gameObject.name.ToLowerInvariant();
                if(n.Contains("target")||n.Contains("debug"))continue;
                var slots=r.sharedMaterials;
                for(int i=0;i<slots.Length;i++)
                {
                    var src=slots[i]; if(src==null)continue;
                    var m=new Material(src){name="RADICAL · "+src.name};
                    Color tint=Limestone;
                    if(n.Contains("roof")||n.Contains("slate")||n.Contains("crown"))tint=new Color(.10f,.14f,.18f,1f);
                    else if(n.Contains("timber")||n.Contains("wood")||n.Contains("scaffold"))tint=new Color(.27f,.16f,.08f,1f);
                    else if(n.Contains("rock")||n.Contains("cliff")||n.Contains("mountain"))tint=Rock;
                    else if(n.Contains("ground")||n.Contains("earth")||n.Contains("street"))tint=Earth;
                    else if(n.Contains("banner"))tint=Blue;
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Color.Lerp(src.HasProperty("_BaseColor")?src.GetColor("_BaseColor"):Color.white,tint,.44f));
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",n.Contains("roof")?.18f:.10f);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    slots[i]=m;
                }
                r.sharedMaterials=slots;
            }
        }

        static void BuildMountainFrame()
        {
            // Large continuous sculpted skins: center kept open for gameplay buildings/clicks.
            HeightPatch("RADICAL · lower mountain apron",new Vector3(0f,-.18f,-11.0f),34f,15f,15,8,-.4f,1.5f,Earth,0.44f);
            HeightPatch("RADICAL · west mountain",new Vector3(-15f,-.2f,2.5f),16f,30f,9,15,0f,5.2f,Rock,0.76f);
            HeightPatch("RADICAL · east mountain",new Vector3(15f,-.2f,3f),16f,30f,9,15,0f,5.8f,Rock,0.71f);
            HeightPatch("RADICAL · upper mountain",new Vector3(0f,-.1f,18f),34f,16f,15,9,1.0f,7.0f,Rock,0.68f);

            // Authorial terrace shelves at three readable levels.
            TerraceShelf("RADICAL · west civic terrace",new Vector3(-9.6f,.32f,-2.4f),new Vector3(7.8f,.35f,8.2f),-4f);
            TerraceShelf("RADICAL · east military terrace",new Vector3(9.2f,.32f,-3.4f),new Vector3(7.4f,.35f,8.0f),5f);
            TerraceShelf("RADICAL · upper bastion terrace",new Vector3(0f,2.62f,7.5f),new Vector3(14.5f,.40f,8.6f),0f);
        }

        static void BuildCirculation()
        {
            RoadRibbon("RADICAL · processional road",
                new[]{new Vector3(0,.50f,-10f),new Vector3(-.2f,.52f,-6.5f),new Vector3(.15f,.55f,-3.1f),new Vector3(.10f,.75f,.6f),new Vector3(0f,2.74f,6.1f)},2.8f);
            RoadRibbon("RADICAL · sawmill branch",
                new[]{new Vector3(-.2f,.53f,-4.0f),new Vector3(-3.2f,.55f,-3.5f),new Vector3(-6.5f,.56f,-2.8f)},1.55f);
            RoadRibbon("RADICAL · barracks branch",
                new[]{new Vector3(.2f,.53f,-4.4f),new Vector3(3.5f,.55f,-4.1f),new Vector3(6.7f,.56f,-4.0f)},1.55f);

            for(int i=0;i<8;i++)
            {
                float z=-8.5f+i*1.85f;
                Flag(new Vector3(-1.75f,.55f,z),i%2==0?Blue:Gold,.36f,1.5f);
                Flag(new Vector3(1.75f,.55f,z),i%2==0?Gold:Blue,.36f,1.5f);
            }
        }

        static void BuildRuinsAndSkyline()
        {
            // Monumental ruins frame rather than compete with Bastion.
            RuinArch("RADICAL · imperial ruin west",new Vector3(-11.8f,.3f,12.5f),4.2f,6.6f,18f);
            RuinArch("RADICAL · imperial ruin east",new Vector3(12.0f,.4f,14.0f),3.8f,5.8f,-18f);

            // Distant corruption: one restrained secondary read.
            var glow=Primitive(PrimitiveType.Cylinder,"RADICAL · distant breach glow",new Vector3(19f,4.8f,29f),new Vector3(2.8f,8f,2.8f),Violet);
            glow.transform.rotation=Quaternion.Euler(0,0,7f);
            var light=new GameObject("RADICAL · distant breach light").AddComponent<Light>();
            light.transform.SetParent(Root);light.type=LightType.Point;light.range=28f;light.intensity=4.5f;light.color=new Color(.62f,.22f,.88f);light.transform.position=new Vector3(19f,6f,28f);
        }

        static void BuildLifeAndVegetation()
        {
            var pts=new[]{
                new Vector3(-11f,.3f,-7f),new Vector3(-12.5f,.5f,-2f),new Vector3(-11.2f,1.0f,4f),
                new Vector3(11f,.3f,-7f),new Vector3(12.2f,.7f,-1f),new Vector3(11.5f,1.1f,5f),
                new Vector3(-8.5f,2.9f,10.5f),new Vector3(8.6f,2.9f,10.8f),new Vector3(-6.5f,1.0f,14f),new Vector3(7f,1.2f,15f)};
            for(int i=0;i<pts.Length;i++)Tree(pts[i],.75f+(i%3)*.12f);

            // Warm settlement pools.
            foreach(var p in new[]{new Vector3(-6.8f,2.5f,-2.8f),new Vector3(6.9f,2.4f,-4f),new Vector3(0f,5.3f,6.1f)})
            {
                var l=new GameObject("RADICAL · inhabited warm light").AddComponent<Light>();
                l.transform.SetParent(Root);l.type=LightType.Point;l.range=7f;l.intensity=2.6f;l.color=new Color(1f,.55f,.25f);l.transform.position=p;
            }
        }

        static void HeightPatch(string name,Vector3 center,float width,float depth,int xCount,int zCount,float baseY,float amplitude,Color color,float edgeBias)
        {
            var verts=new List<Vector3>();var tris=new List<int>();
            for(int z=0;z<zCount;z++)for(int x=0;x<xCount;x++)
            {
                float u=x/(float)(xCount-1),v=z/(float)(zCount-1);
                float px=(u-.5f)*width,pz=(v-.5f)*depth;
                float edge=Mathf.Pow(Mathf.Abs(u-.5f)*2f,1.5f)*edgeBias+Mathf.Pow(v,1.2f)*.35f;
                float noise=(Mathf.Sin((x*1.73f+z*.91f))*0.32f+Mathf.Cos((z*1.31f-x*.67f))*0.22f);
                float y=baseY+amplitude*(edge+.16f*noise);
                verts.Add(new Vector3(px,y,pz));
            }
            for(int z=0;z<zCount-1;z++)for(int x=0;x<xCount-1;x++)
            {
                int a=z*xCount+x,b=a+1,c=a+xCount,d=c+1;
                tris.Add(a);tris.Add(c);tris.Add(b);tris.Add(b);tris.Add(c);tris.Add(d);
            }
            var mesh=new Mesh{name=name+" mesh"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(name);go.transform.SetParent(Root);go.transform.position=center;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Mat(color,.08f);
        }

        static void TerraceShelf(string name,Vector3 pos,Vector3 scale,float yaw)
        {
            var baseGo=Primitive(PrimitiveType.Cube,name,pos,scale,LimestoneLight);baseGo.transform.rotation=Quaternion.Euler(0,yaw,0);
            for(int i=-3;i<=3;i++)
            {
                var b=Primitive(PrimitiveType.Cube,name+" · retaining block",pos+Quaternion.Euler(0,yaw,0)*new Vector3(i*scale.x/7f,-.46f,-scale.z*.49f),new Vector3(scale.x/7.5f,.8f,.55f),Limestone);
                b.transform.rotation=Quaternion.Euler(0,yaw+(i%2==0?1.3f:-1.2f),0);
            }
        }

        static void RoadRibbon(string name,Vector3[] points,float width)
        {
            var verts=new List<Vector3>();var tris=new List<int>();
            for(int i=0;i<points.Length;i++)
            {
                var f=i==points.Length-1?points[i]-points[i-1]:points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(i-1,0)];
                var side=Vector3.Cross(Vector3.up,f).normalized*width*.5f;
                verts.Add(points[i]-side);verts.Add(points[i]+side);
                if(i>0){int a=(i-1)*2,b=a+1,c=i*2,d=c+1;tris.Add(a);tris.Add(c);tris.Add(b);tris.Add(b);tris.Add(c);tris.Add(d);}
            }
            var mesh=new Mesh{name=name+" mesh"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(name);go.transform.SetParent(Root);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Mat(new Color(.49f,.45f,.37f),.16f);
        }

        static void RuinArch(string name,Vector3 p,float halfWidth,float height,float yaw)
        {
            var q=Quaternion.Euler(0,yaw,0);
            foreach(float s in new[]{-1f,1f})
            {
                var pier=Primitive(PrimitiveType.Cube,name+" · pier",p+q*new Vector3(s*halfWidth,height*.45f,0),new Vector3(1.3f,height,1.9f),Limestone);
                pier.transform.rotation=q*Quaternion.Euler(0,0,s*2f);
            }
            var crown=Primitive(PrimitiveType.Cube,name+" · broken crown",p+q*new Vector3(-.5f,height,0),new Vector3(halfWidth*1.65f,.9f,1.8f),LimestoneLight);
            crown.transform.rotation=q*Quaternion.Euler(0,0,-7f);
        }

        static void Flag(Vector3 p,Color c,float width,float height)
        {
            Primitive(PrimitiveType.Cube,"RADICAL · banner pole",p+Vector3.up*.8f,new Vector3(.05f,1.6f,.05f),new Color(.22f,.14f,.08f));
            var f=Primitive(PrimitiveType.Cube,"RADICAL · banner",p+new Vector3(width*.48f,1.35f,0),new Vector3(width,height*.45f,.04f),c);
            f.transform.rotation=Quaternion.Euler(0,0,-4f);
        }

        static void Tree(Vector3 p,float s)
        {
            Primitive(PrimitiveType.Cylinder,"RADICAL · tree trunk",p+Vector3.up*.7f*s,new Vector3(.16f,.75f,.16f)*s,new Color(.22f,.14f,.08f));
            Primitive(PrimitiveType.Sphere,"RADICAL · tree crown",p+Vector3.up*1.8f*s,new Vector3(1.1f,1.55f,1.1f)*s,Moss);
        }

        static GameObject Primitive(PrimitiveType type,string name,Vector3 pos,Vector3 scale,Color color)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(Root);go.transform.position=pos;go.transform.localScale=scale;
            var c=go.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
            go.GetComponent<Renderer>().sharedMaterial=Mat(color,.10f);return go;
        }

        static Material Mat(Color color,float smooth)
        {
            var m=new Material(Lit);m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",0f);m.SetFloat("_Smoothness",smooth);return m;
        }
    }
}

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
                    float blend=(n.StartsWith("aserradero")||n.StartsWith("cuartel"))?.18f:(n.Contains("bastion")?.64f:.56f);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",Color.Lerp(src.HasProperty("_BaseColor")?src.GetColor("_BaseColor"):Color.white,tint,blend));
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",n.Contains("roof")?.18f:.10f);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    slots[i]=m;
                }
                r.sharedMaterials=slots;
            }
        }

        static void BuildMountainFrame()
        {
            // V2: the first proof failed because foreground heightfields swallowed the city.
            // Keep all new terrain behind the readable city silhouette: landscape must frame, never occlude.
            HeightPatch("RADICAL · distant valley ridge",new Vector3(0f,-3.8f,32f),70f,22f,25,9,0f,6.2f,Rock,.20f);
            HeightPatch("RADICAL · west distant shoulder",new Vector3(-25f,-3.2f,24f),24f,18f,11,8,0f,4.2f,Rock,.18f);
            HeightPatch("RADICAL · east distant shoulder",new Vector3(25f,-3.2f,24f),24f,18f,11,8,0f,4.6f,Rock,.18f);

            // Readable built terraces remain thin architectural edges around the certified plots.
            TerraceShelf("RADICAL · west civic terrace",new Vector3(-8.9f,.20f,-2.6f),new Vector3(7.2f,.20f,7.2f),-3f);
            TerraceShelf("RADICAL · east military terrace",new Vector3(8.8f,.20f,-3.8f),new Vector3(7.0f,.20f,7.0f),4f);
            TerraceShelf("RADICAL · upper bastion terrace",new Vector3(0f,2.50f,7.2f),new Vector3(13.2f,.22f,7.2f),0f);
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
            // V2: ruins are distant vertical remnants, not giant primitive arches crossing the frame.
            RuinRemnant("RADICAL · imperial ruin west",new Vector3(-13.5f,.15f,16.8f),-12f);
            RuinRemnant("RADICAL · imperial ruin east",new Vector3(13.4f,.18f,17.6f),14f);

            // The canonical scene already owns the distant violet scar. Do not duplicate it with debug geometry.
            // Reinforce it only with a faint atmospheric light kept behind the city.
            var light=new GameObject("RADICAL · distant breach ambience").AddComponent<Light>();
            light.transform.SetParent(Root);light.type=LightType.Point;light.range=20f;light.intensity=1.25f;
            light.color=new Color(.52f,.20f,.72f);light.transform.position=new Vector3(15.5f,4.5f,18.5f);light.shadows=LightShadows.None;
        }

        static void BuildLifeAndVegetation()
        {
            // Settlement rhythm is carried by light and heraldry; base scene already has authored pines.
            foreach(var p in new[]{new Vector3(-6.8f,2.5f,-2.8f),new Vector3(6.9f,2.4f,-4f),new Vector3(0f,5.3f,6.1f)})
            {
                var l=new GameObject("RADICAL · inhabited warm light").AddComponent<Light>();
                l.transform.SetParent(Root);l.type=LightType.Point;l.range=7f;l.intensity=2.35f;
                l.color=new Color(1f,.55f,.25f);l.transform.position=p;l.shadows=LightShadows.None;
            }
            Flag(new Vector3(-4.6f,2.72f,4.7f),Blue,.46f,1.8f);
            Flag(new Vector3(4.6f,2.72f,4.7f),Gold,.46f,1.8f);
            Flag(new Vector3(-2.7f,2.72f,5.3f),Gold,.40f,1.6f);
            Flag(new Vector3(2.7f,2.72f,5.3f),Blue,.40f,1.6f);
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

        static void RuinRemnant(string name,Vector3 p,float yaw)
        {
            var q=Quaternion.Euler(0,yaw,0);
            float[] heights={5.8f,3.7f,6.6f};
            float[] xs={-1.8f,0f,1.6f};
            for(int i=0;i<3;i++)
            {
                var col=Primitive(PrimitiveType.Cube,name+" · remnant "+i,
                    p+q*new Vector3(xs[i],heights[i]*.5f,0),new Vector3(.72f,heights[i],1.05f),Limestone*.92f);
                col.transform.rotation=q*Quaternion.Euler(0,0,i==1?-4f:(i==2?3f:0f));
            }
            var lintel=Primitive(PrimitiveType.Cube,name+" · broken lintel",
                p+q*new Vector3(-.25f,4.25f,0),new Vector3(3.8f,.55f,1.0f),LimestoneLight*.90f);
            lintel.transform.rotation=q*Quaternion.Euler(0,0,-7f);
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

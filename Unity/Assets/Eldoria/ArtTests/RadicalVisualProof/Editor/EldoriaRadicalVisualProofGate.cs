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
            RenderSettings.ambientSkyColor=new Color(.80f,.84f,.86f);
            RenderSettings.ambientEquatorColor=new Color(.55f,.54f,.49f);
            RenderSettings.ambientGroundColor=new Color(.28f,.25f,.21f);
            RenderSettings.ambientIntensity=1.05f;
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.55f,.64f,.69f);
            RenderSettings.fogStartDistance=46f;
            RenderSettings.fogEndDistance=126f;

            var cam=Camera.main;
            if(cam!=null)
            {
                cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=new Color(.48f,.61f,.70f);
                cam.allowHDR=true;
            }

            foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l.type==LightType.Directional)
                {
                    l.intensity=1.05f;
                    l.color=new Color(1f,.90f,.76f);
                    l.shadowStrength=.62f;
                    l.shadows=LightShadows.Soft;
                    l.transform.rotation=Quaternion.Euler(48f,-34f,0f);
                }

            var fill=new GameObject("RADICAL v3 · cool sky fill").AddComponent<Light>();
            fill.transform.SetParent(Root);fill.type=LightType.Directional;fill.intensity=.22f;
            fill.color=new Color(.62f,.74f,1f);fill.transform.rotation=Quaternion.Euler(34f,150f,0f);fill.shadows=LightShadows.None;

            var rim=new GameObject("RADICAL v3 · bastion sunset rim").AddComponent<Light>();
            rim.transform.SetParent(Root);rim.type=LightType.Point;rim.range=15f;rim.intensity=3.4f;
            rim.color=new Color(1f,.58f,.27f);rim.transform.position=new Vector3(-2.5f,9f,11.5f);rim.shadows=LightShadows.None;
        }

        static void RegradeScene()
        {
            // Preserve authored maps; selectively lift masonry value hierarchy.
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||r.transform.IsChildOf(Root))continue;
                string n=r.gameObject.name.ToLowerInvariant();
                bool hero=n.Contains("bastion")||n.Contains("masonry")||n.Contains("stonearch")||
                          n.Contains("retaining")||n.Contains("wall")||n.Contains("terrace");
                if(!hero)continue;
                var slots=r.sharedMaterials;
                for(int i=0;i<slots.Length;i++)
                {
                    var src=slots[i];if(src==null)continue;
                    var m=new Material(src){name="RADICAL v3 · "+src.name};
                    if(m.HasProperty("_BaseColor"))
                    {
                        var b=m.GetColor("_BaseColor");
                        m.SetColor("_BaseColor",Color.Lerp(b,new Color(.76f,.72f,.64f,1f),.25f));
                    }
                    if(m.HasProperty("_Color"))
                    {
                        var b=m.GetColor("_Color");
                        m.SetColor("_Color",Color.Lerp(b,new Color(.74f,.70f,.62f,1f),.18f));
                    }
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.11f);
                    slots[i]=m;
                }
                r.sharedMaterials=slots;
            }
        }

        static void BuildMountainFrame()
        {
            // v3: no extra mountain silhouettes. Keep the canonical valley horizon and only
            // stitch visible side seams with authored rock so the city remains the dominant read.
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;
            var rockTint=new Color(.86f,.86f,.80f,1f);
            var rocks=new[]{
                new Vector3(-12.2f,-.05f,-7.6f),new Vector3(-13.0f,.12f,-1.5f),new Vector3(-11.8f,.30f,5.2f),
                new Vector3(12.2f,-.05f,-7.5f),new Vector3(13.0f,.12f,-1.2f),new Vector3(11.8f,.30f,5.4f)
            };
            for(int i=0;i<rocks.Length;i++)
                ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · valley rock seam "+i,art.SlavicFlatRock,
                    rocks[i],3.6f,1.8f,Quaternion.Euler(0,i*43f,0),rockTint);
        }

        static void BuildCirculation()
        {
            // Keep the certified roads visible. New heraldry reinforces the main processional axis
            // without drawing a new blockout road over it.
            for(int i=0;i<7;i++)
            {
                float z=-8.2f+i*1.95f;
                Flag(new Vector3(-1.82f,.48f,z),i%2==0?Blue:Gold,.32f,1.35f);
                Flag(new Vector3(1.82f,.48f,z),i%2==0?Gold:Blue,.32f,1.35f);
            }

            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;
            foreach(var p in new[]{new Vector3(-7.8f,.42f,-6.9f),new Vector3(7.7f,.42f,-6.9f)})
                ValoriaKit.BenchmarkPieceModulated("RADICAL v2 · civic gate marker",art.SlavicRockGate,
                    p,2.4f,2.8f,Quaternion.Euler(0,p.x<0?-8f:188f,0),new Color(.88f,.86f,.78f,1f));
        }

        static void BuildRuinsAndSkyline()
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            // Turn the existing Bastion into a real fortress compound using the coherent Mega/Masonry subset.
            ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · bastion rear wall",art.MegaWallPassage,
                new Vector3(0f,2.72f,10.3f),7.6f,4.4f,Quaternion.Euler(0,180f,0),new Color(.95f,.91f,.82f,1f));
            ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · bastion crown tower",art.MegaTower,
                new Vector3(-.5f,2.74f,11.1f),3.4f,8.2f,Quaternion.Euler(0,4f,0),new Color(.96f,.92f,.84f,1f));
            ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · bastion rear tower west",art.MasonryTower,
                new Vector3(-4.7f,2.70f,10.0f),2.9f,6.4f,Quaternion.Euler(0,8f,0),new Color(.92f,.89f,.82f,1f));
            ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · bastion rear tower east",art.MasonryTower,
                new Vector3(4.5f,2.70f,10.2f),2.8f,5.8f,Quaternion.Euler(0,-9f,0),new Color(.90f,.88f,.81f,1f));

            // Monumental ruin fragments frame the city at the edges.
            ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · imperial ruin west",art.MegaDestroyedTower,
                new Vector3(-11.0f,.18f,13.4f),4.4f,6.0f,Quaternion.Euler(0,22f,0),new Color(.89f,.87f,.80f,1f));
            ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · imperial ruin east",art.MegaWallPassage,
                new Vector3(11.0f,.16f,14.0f),5.1f,4.7f,Quaternion.Euler(0,-20f,0),new Color(.87f,.86f,.80f,1f));

            // Environmental corruption: low fractured glow cluster, not a beacon/cylinder.
            for(int i=0;i<7;i++)
            {
                float x=17.2f+(i%3)*1.0f;
                float z=24.5f+(i/3)*1.15f;
                var shard=Primitive(PrimitiveType.Sphere,"RADICAL v3 · breach scar "+i,
                    new Vector3(x,.30f+(i%2)*.22f,z),new Vector3(.45f+(i%3)*.18f,.30f,.70f),Violet*(.72f+(i%2)*.12f));
                shard.transform.rotation=Quaternion.Euler(i*9f,i*31f,i*7f);
            }
            var light=new GameObject("RADICAL v3 · breach atmospheric glow").AddComponent<Light>();
            light.transform.SetParent(Root);light.type=LightType.Point;light.range=16f;light.intensity=2.1f;
            light.color=new Color(.56f,.20f,.78f);light.transform.position=new Vector3(18.2f,2.0f,25.2f);
        }

        static void BuildLifeAndVegetation()
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            // Keep medium density and coherent scale. Four small authored residences are enough.
            var houses=new[]{
                new Vector3(-10.0f,.34f,-4.2f),new Vector3(-9.6f,.42f,1.7f),
                new Vector3(9.8f,.34f,-4.6f),new Vector3(9.7f,.42f,1.8f)
            };
            for(int i=0;i<houses.Length;i++)
                ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · edge residence "+i,art.SlavicHouse,houses[i],
                    2.35f,2.75f,Quaternion.Euler(0,i<2?22f+i*13f:195f-i*12f,0),
                    new Color(.91f,.86f,.76f,1f));

            // Low retaining/fence fragments create urban edges without adding more whole buildings.
            foreach(var p in new[]{new Vector3(-8.6f,.38f,-6.2f),new Vector3(-8.8f,.40f,3.7f),
                                   new Vector3(8.6f,.38f,-6.3f),new Vector3(8.8f,.40f,3.8f)})
                ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · terrace edge",art.SlavicStoneFence,p,
                    3.0f,1.1f,Quaternion.Euler(0,p.x<0?10f:190f,0),new Color(.92f,.90f,.84f,1f));

            // Functional props: work/material story where it belongs.
            foreach(var p in new[]{new Vector3(-8.5f,.42f,-3.1f),new Vector3(-7.8f,.42f,-1.8f)})
                ValoriaKit.BenchmarkPieceModulated("RADICAL v3 · sawmill firewood",art.Firewood,p,
                    1.25f,.85f,Quaternion.Euler(0,p.z*17f,0),new Color(.94f,.82f,.66f,1f));

            foreach(var p in new[]{new Vector3(-6.8f,2.5f,-2.8f),new Vector3(6.9f,2.4f,-4f),new Vector3(0f,6.0f,7.3f)})
            {
                var l=new GameObject("RADICAL v3 · inhabited warm light").AddComponent<Light>();
                l.transform.SetParent(Root);l.type=LightType.Point;l.range=6.4f;l.intensity=2.0f;
                l.color=new Color(1f,.58f,.30f);l.transform.position=p;
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

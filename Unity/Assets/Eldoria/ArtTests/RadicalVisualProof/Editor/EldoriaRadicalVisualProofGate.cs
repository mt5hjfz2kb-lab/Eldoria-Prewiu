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
            // v2: preserve authored PBR textures. Only pull the fortress/civic stone toward
            // Eldoria's clear limestone family; never replace entire imported materials.
            foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||r.transform.IsChildOf(Root))continue;
                string n=r.gameObject.name.ToLowerInvariant();
                bool hero=n.Contains("bastion")||n.Contains("masonry")||n.Contains("stonearch")||
                          n.Contains("retaining")||n.Contains("wall");
                if(!hero)continue;
                var slots=r.sharedMaterials;
                for(int i=0;i<slots.Length;i++)
                {
                    var src=slots[i];if(src==null)continue;
                    var m=new Material(src){name="RADICAL v2 · "+src.name};
                    if(m.HasProperty("_BaseColor"))
                    {
                        var b=m.GetColor("_BaseColor");
                        m.SetColor("_BaseColor",Color.Lerp(b,LimestoneLight,.16f));
                    }
                    if(m.HasProperty("_Color"))
                    {
                        var b=m.GetColor("_Color");
                        m.SetColor("_Color",Color.Lerp(b,LimestoneLight,.12f));
                    }
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",Mathf.Min(.16f,m.GetFloat("_Smoothness")));
                    slots[i]=m;
                }
                r.sharedMaterials=slots;
            }
        }

        static void BuildMountainFrame()
        {
            // v2: authored terrain only. The v1 procedural mountain sheets were visually rejected.
            // Side/rear masses frame the city but never cross the central gameplay/readability cone.
            ValoriaKit.TerrainPieceModulated("SM_Mountains_11","RADICAL v2 · west mountain frame",
                new Vector3(-23f,-2.1f,12f),15f,9f,Quaternion.Euler(0,28f,0),new Color(.82f,.86f,.84f,1f));
            ValoriaKit.TerrainPieceModulated("SM_Mountains_11","RADICAL v2 · east mountain frame",
                new Vector3(23f,-2.0f,13f),15f,9f,Quaternion.Euler(0,-30f,0),new Color(.82f,.85f,.84f,1f));
            ValoriaKit.TerrainPieceModulated("SM_Mountains_11","RADICAL v2 · high valley rear",
                new Vector3(0f,-3.2f,36f),20f,11f,Quaternion.Euler(0,-8f,0),new Color(.76f,.81f,.83f,1f));

            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;
            var rockTint=new Color(.76f,.78f,.74f,1f);
            var rocks=new[]{
                new Vector3(-12.5f,-.05f,-7.8f),new Vector3(-13.5f,.15f,-2.0f),new Vector3(-12.2f,.35f,4.5f),
                new Vector3(12.5f,-.05f,-7.6f),new Vector3(13.5f,.15f,-1.5f),new Vector3(12.4f,.35f,5.0f)
            };
            for(int i=0;i<rocks.Length;i++)
                ValoriaKit.BenchmarkPieceModulated("RADICAL v2 · mountain seam "+i,art.SlavicFlatRock,
                    rocks[i],4.3f,2.2f,Quaternion.Euler(0,i*47f,0),rockTint);
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

            // Monumental authored remnants frame, rather than eclipse, the Bastion.
            ValoriaKit.BenchmarkPieceModulated("RADICAL v2 · imperial ruin west",art.MegaDestroyedTower,
                new Vector3(-10.8f,.15f,13.5f),5.0f,6.8f,Quaternion.Euler(0,20f,0),new Color(.88f,.86f,.79f,1f));
            ValoriaKit.BenchmarkPieceModulated("RADICAL v2 · imperial ruin east",art.MegaWallPassage,
                new Vector3(10.2f,.12f,14.4f),6.2f,5.5f,Quaternion.Euler(0,-18f,0),new Color(.86f,.85f,.79f,1f));
            ValoriaKit.BenchmarkPieceModulated("RADICAL v2 · rear imperial sentinel",art.RuinedTower,
                new Vector3(-5.8f,.05f,19.5f),4.4f,7.4f,Quaternion.Euler(0,10f,0),new Color(.82f,.83f,.79f,1f));

            // One distant, secondary purple threat. Small enough to remain background narrative.
            var glow=Primitive(PrimitiveType.Cylinder,"RADICAL v2 · distant breach",new Vector3(20.5f,3.7f,31f),
                new Vector3(1.15f,5.8f,1.15f),Violet);
            glow.transform.rotation=Quaternion.Euler(0,0,7f);
            var light=new GameObject("RADICAL v2 · breach haze").AddComponent<Light>();
            light.transform.SetParent(Root);light.type=LightType.Point;light.range=20f;light.intensity=2.8f;
            light.color=new Color(.58f,.22f,.82f);light.transform.position=new Vector3(20.5f,5.2f,30f);
        }

        static void BuildLifeAndVegetation()
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            // Medium-density inhabited terraces built from the existing coherent Slavic set.
            var houses=new[]{
                new Vector3(-10.1f,.35f,-4.5f),new Vector3(-10.4f,.48f,1.4f),new Vector3(-8.8f,2.78f,9.2f),
                new Vector3(10.0f,.34f,-5.0f),new Vector3(10.6f,.48f,1.3f),new Vector3(8.8f,2.78f,9.4f)
            };
            for(int i=0;i<houses.Length;i++)
            {
                var prefab=i%3==1?art.SlavicShed:art.SlavicHouse;
                ValoriaKit.BenchmarkPieceModulated("RADICAL v2 · inhabited terrace "+i,prefab,houses[i],
                    i%3==1?2.7f:3.4f,i%3==1?2.5f:3.6f,
                    Quaternion.Euler(0,i<3?18f+i*11f:192f-i*9f,0),new Color(.98f,.94f,.86f,1f));
            }

            var trees=new[]{
                new Vector3(-13f,.1f,-5.5f),new Vector3(-13.8f,.2f,2.5f),new Vector3(-11.8f,.6f,7.5f),
                new Vector3(13f,.1f,-5.2f),new Vector3(13.8f,.2f,2.8f),new Vector3(11.9f,.6f,7.8f),
                new Vector3(-8.2f,.3f,15.8f),new Vector3(8.0f,.3f,16.3f)
            };
            for(int i=0;i<trees.Length;i++)
                ValoriaKit.BenchmarkPieceModulated("RADICAL v2 · authored tree "+i,
                    i%3==0?art.SlavicTreeTall:art.SlavicTree,trees[i],2.4f,4.8f,
                    Quaternion.Euler(0,i*31f,0),new Color(.72f,.86f,.70f,1f));

            foreach(var p in new[]{new Vector3(-6.8f,2.5f,-2.8f),new Vector3(6.9f,2.4f,-4f),new Vector3(0f,5.3f,6.1f)})
            {
                var l=new GameObject("RADICAL v2 · inhabited warm light").AddComponent<Light>();
                l.transform.SetParent(Root);l.type=LightType.Point;l.range=6.5f;l.intensity=2.1f;
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

using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Dressing over certified runtime topology. No gameplay, parcel, camera or collision ownership.
    public static class ProductionVisualIntegration
    {
        static Material landscape;
        static Material sharedStone;
        static readonly Dictionary<string,Material> adapted = new();
        static readonly Color Blue = new Color(.13f,.24f,.38f);
        static readonly Color Rock = new Color(.42f,.43f,.39f);
        static Transform root;

        public static void World(PlayerState state)
        {
            root = new GameObject("Frontier · integrated 4X visual layer").transform;
            UnifyLandscape(false);
            Suppress("Sir Aldric ","Aldric ","Archer ","Bow");
            // Replace the primitive foliage read with two mapped, authored tree variants.
            Suppress("Frontier · forest pine", "Frontier · tall evergreen", "Frontier · ridge pine",
                "Frontier · undergrowth", "Frontier · forest moss");
            var clusters = new[] {
                new Vector3(-7,0,1.5f), new Vector3(-13,0,7),
                new Vector3(13,0,2), new Vector3(-10,0,-9), new Vector3(1,0,13)
            };
            for(int c=0;c<clusters.Length;c++)
            for(int i=0;i<(c==0?13:9);i++)
            {
                float a=i*2.39996f+c*.71f;
                float radius=Mathf.Sqrt(i+.7f)*(c==0?.72f:.9f);
                var p=clusters[c]+new Vector3(Mathf.Cos(a)*radius,.05f,Mathf.Sin(a)*radius);
                Imported("4X · forest canopy", "Tree01"+(i%2==0?"A":"B"),p,
                    1.6f+(i%3)*.24f,2.35f+(i%4)*.23f,i*47+c*29,new Color(.54f,.61f,.48f),true);
            }
            // Mountain barriers live behind the nodes; low rock skirts merge into continuous ground.
            foreach(var p in new[]{new Vector3(-17,-.25f,12),new Vector3(14,-.25f,13),new Vector3(-3,-.35f,21)})
            {
                Imported("4X · mountain barrier", "Mountain01",p,12.0f,6.3f,p.x*7,Rock,false);
                Imported("4X · buried foothill", "Rock02",p+new Vector3(2,-.1f,-3),5.3f,2.5f,p.x*11,Rock,false);
            }
            foreach(var p in new[]{new Vector3(-11,.02f,5),new Vector3(10,.02f,7),new Vector3(14,.02f,-6),new Vector3(-13,.02f,-4)})
                Imported("4X · route rock shoulder","Rock01",p,3.4f,1.4f,p.z*19,Rock,false);

            foreach(var spec in new[]{new Vector4(-3.15f,.5f,5.8f,90f),new Vector4(3.15f,-1.8f,5.8f,90f)})
            {
                var route=ValoriaGroundKit.TrailStraight("4X · resource access route",new Vector3(spec.x,.15f,spec.y),spec.z,.85f,spec.w);
                route.transform.SetParent(root,true);
            }
            var art=ValoriaExternalAssetLibrary.Load();
            // Wood identity: stocked timber frontage distinct from background forest.
            Piece("4X · wood stock",art!=null?art.Firewood:null,new Vector3(-5.3f,.12f,-.15f),1.7f,.85f,-16,Color.white);
            // Existing quarry kit remains independent from the authoritative target.
            foreach(var p in new[]{new Vector3(6.35f,.03f,-2.15f),new Vector3(-4,.03f,7.4f)})
            {
                Imported("4X · cut stone face","Rock02",p+new Vector3(.2f,0,.6f),2.1f,1.2f,35, new Color(.63f,.61f,.53f),false);
                Piece("4X · quarry edge",art!=null?art.SlavicStoneFence:null,p+new Vector3(-.5f,.02f,-.65f),1.45f,.45f,0,new Color(.61f,.57f,.49f));
            }
            // Origin city and deployed party are map representations, not extra gameplay buildings.
            CityOrigin(new Vector3(-1.4f,.12f,-5.8f),art);
            if(state.March.Phase!="idle")March(new Vector3(1.2f,.08f,-3.8f));
            // The future chapter-III strategic layer is visible only in its proper progression state.
            if(state.BastionLevel>=3)
            {
                Beast("4X · wolf · placeholder",new Vector3(-2.7f,.10f,3.4f),false);
                Beast("4X · boar · placeholder",new Vector3(6.7f,.10f,-6.0f),true);
                Ruin(new Vector3(-1.9f,.08f,9.4f));
                Piece("4X · food cache · placeholder",art!=null?art.SlavicShed:null,
                    new Vector3(-8.8f,.08f,-4.7f),1.45f,1.15f,-14,new Color(.56f,.51f,.42f));
                Piece("4X · food sacks · placeholder",Resources.Load<GameObject>("Valoria/UrbanProps/Sack"),new Vector3(-8.25f,.08f,-5.2f),.8f,.6f,0,new Color(.8f,.73f,.57f));
                Piece("4X · food barrel · placeholder",Resources.Load<GameObject>("Valoria/UrbanProps/Barrel"),new Vector3(-9.3f,.08f,-5.25f),.55f,.75f,0,new Color(.72f,.63f,.48f));
            }
            Node("Valoria",new Vector3(-1.4f,.15f,-5.8f),Blue,1.6f);
            Node("Madera",new Vector3(-5.3f,.15f,-.15f),new Color(.65f,.48f,.22f),1.1f);
            Node("Piedra",new Vector3(6.35f,.15f,-2.15f),new Color(.65f,.63f,.53f),1.35f);
            Node("Engendro",new Vector3(5f,.15f,3.15f),new Color(.43f,.22f,.39f),1.25f);
            Node("Brecha",new Vector3(10f,.15f,8f),new Color(.48f,.23f,.43f),1.55f);
            if(state.March.Phase!="idle")Node("Marcha",new Vector3(1.2f,.15f,-3.8f),Blue,.85f);
            if(state.BastionLevel>=3)
            {
                Node("Lobo",new Vector3(-2.7f,.15f,3.4f),new Color(.63f,.39f,.25f),.85f);
                Node("Jabalí",new Vector3(6.7f,.15f,-6f),new Color(.63f,.39f,.25f),.85f);
                Node("Ruinas",new Vector3(-1.9f,.15f,9.4f),new Color(.64f,.58f,.39f),1.35f);
                Node("Alimento",new Vector3(-8.8f,.15f,-4.7f),new Color(.65f,.48f,.22f),1f);
            }
            // Existing territorial scar gains an installation silhouette, without neon crystals.
            Imported("4X · breach broken arch","Arch_Gothic",new Vector3(9.4f,.02f,8.4f),2.5f,3.25f,-22,new Color(.23f,.22f,.26f),false);
            Imported("4X · breach ruin flank","Wall_Broken",new Vector3(11.3f,.03f,8.0f),1.6f,1.6f,53,new Color(.27f,.25f,.28f),false);
            Finish();
        }

        public static void City(PlayerState state)
        {
            root = new GameObject("Valoria · integrated construction visual layer").transform;
            UnifyLandscape(true);
            Suppress("Sir Aldric ","Aldric ","Archer ","Bow");
            // Subordinate inhabited silhouettes replace oversized provisional staging primitives.
            foreach(var p in new[]{new Vector3(-10.5f,.47f,-1.1f),new Vector3(-14.4f,.49f,-.7f),
                new Vector3(-11.3f,.46f,3.45f),new Vector3(-7.6f,.44f,-4.35f),new Vector3(6.4f,.44f,-5.8f),
                new Vector3(-4.8f,2.77f,5.9f),new Vector3(1.0f,.43f,-4.6f)})
                Civilian(p);

            var art=ValoriaExternalAssetLibrary.Load();
            // Fit real support architecture into the already-authored residential footprints.
            if(art!=null&&art.SlavicHouse!=null)
            {
                Suppress("VPD · west rebuilders home · roof", "VPD · west rebuilders upper dwelling · roof");
                var homes=new[]{new Vector3(-12,.34f,-3.25f),new Vector3(-15.25f,.36f,-3.05f),
                    new Vector3(-13.25f,.36f,2.45f),new Vector3(-17.35f,.38f,2.15f),
                    new Vector3(-15.8f,1.28f,5.55f),new Vector3(-18.05f,1.18f,6.05f)};
                for(int i=0;i<homes.Length;i++)
                    Piece("Valoria · reused civil house "+i,art.SlavicHouse,
                        homes[i]+Vector3.up*(i==5?1.12f:i>=4?1.28f:1.18f),i>=4?2.35f:2.20f,.94f,0,new Color(.62f,.57f,.48f));
            }
            // StoneKit surface and border functions. Y-normalized skins never become floors.
            for(int i=0;i<7;i++)
            {
                float z=-6.0f+i*.92f;
                StonePiece(1,"Valoria · stone street slab",new Vector3((i%2==0?-.16f:.19f),.405f,z),
                    new Vector3(2.22f,.11f,1.22f),i%2==0?0:180);
            }
            for(int i=0;i<5;i++)
                StonePiece(2,"Valoria · west street transition",new Vector3(-9.55f-i*1.5f,.445f,-1.02f),
                    new Vector3(1.72f,.10f,1.62f),i*71);
            // Deliberate small courts, buried seams and frontages, outside the central walking envelope.
            StonePiece(2,"Valoria · workshop court",new Vector3(-8.7f,.43f,-4.6f),new Vector3(2.2f,.10f,1.65f),14);
            StonePiece(1,"Valoria · barracks apron",new Vector3(8.0f,.44f,-5.9f),new Vector3(2.35f,.10f,1.48f),0);
            StonePiece(2,"Valoria · upper civil court",new Vector3(-5.1f,2.74f,5.9f),new Vector3(2.3f,.10f,1.7f),97);
            for(int i=0;i<3;i++)
            {
                StonePiece(3,"Valoria · low street edge",new Vector3(-1.75f,.41f,-5.55f+i*1.80f),new Vector3(.24f,.21f,1.95f),0);
                StonePiece(3,"Valoria · east street edge",new Vector3(1.75f,.41f,-5.55f+i*1.80f),new Vector3(.24f,.21f,1.95f),180);
            }
            StonePiece(4,"Valoria · workshop court corner",new Vector3(-9.1f,.37f,-5.2f),new Vector3(1.02f,.28f,1.2f),180);
            StonePiece(4,"Valoria · training court corner",new Vector3(9.5f,.37f,-6.0f),new Vector3(1.1f,.26f,1.2f),90);
            // The certified twelve physical treads are untouched; only a tiny cheek accent is added.
            StonePiece(5,"Valoria · stair cheek stone",new Vector3(-2.2f,.5f,1.1f),new Vector3(.75f,.26f,.48f),0);
            StonePiece(5,"Valoria · upper landing cheek",new Vector3(2.5f,2.70f,6.5f),new Vector3(.8f,.24f,.46f),90);
            StonePiece(6,"Valoria · workshop foundation stone",new Vector3(-9.8f,.22f,-2.25f),new Vector3(.65f,.6f,.72f),24);
            StonePiece(6,"Valoria · retaining foundation stone",new Vector3(4.1f,1.56f,4.75f),new Vector3(.65f,.72f,.70f),72);
            if(state.BastionLevel>=3)
            {
                StonePiece(2,"Valoria · granary court",new Vector3(-17.5f,.43f,-4.5f),new Vector3(2.2f,.1f,1.45f),13);
                StonePiece(3,"Valoria · granary edge",new Vector3(-19.1f,.39f,-4.6f),new Vector3(.24f,.23f,2.15f),15);
            }
            // Existing hard-surface props support work areas; no invented functional buildings.
            foreach(var p in new[]{new Vector3(-12.2f,.40f,3.7f),new Vector3(-16.5f,.40f,.65f),new Vector3(-7.9f,.41f,-4.5f)})
                Piece("Valoria · stocked work frontage",art!=null?art.Firewood:null,p,1.03f,.62f,p.x*13,new Color(.78f,.69f,.55f));
            for(int i=0;i<6;i++)
            {
                var p=new Vector3(-11.8f-(i%3)*2.15f,.40f,i<3?3.75f:-3.7f);
                Piece("Valoria · civil store crate",Resources.Load<GameObject>("Valoria/UrbanProps/Crate"),p,.50f,.50f,i*23,new Color(.75f,.65f,.5f));
                Piece("Valoria · civil store barrel",Resources.Load<GameObject>("Valoria/UrbanProps/Barrel"),p+new Vector3(.45f,0,.17f),.38f,.6f,i*31,new Color(.70f,.61f,.48f));
                if(state.BastionLevel>=3)Piece("Valoria · food sack",Resources.Load<GameObject>("Valoria/UrbanProps/Sack"),p+new Vector3(.1f,0,.49f),.55f,.38f,0,new Color(.8f,.72f,.57f));
            }
            DressBastion();
            var tower=Resources.Load<GameObject>("Valoria/Rescued/TowerWallRock");
            Piece("Valoria · rescued hero west anchor",tower,new Vector3(-3.3f,2.55f,8.5f),3.5f,7.2f,18,new Color(.72f,.74f,.70f));
            Piece("Valoria · rescued hero rear anchor",tower,new Vector3(1.9f,2.55f,10.2f),3.1f,6.1f,196,new Color(.66f,.69f,.66f));
            Finish();
        }

        static void DressBastion()
        {
            // Legacy prefab FBX file IDs lose mesh bindings under the current importer.
            // Exact source FBX aliases restore authored masonry over the same visual envelope.
            Suppress("Bastion · connected", "Bastion · rear connected", "Bastion · high lantern",
                "Bastion · keep facing fallback", "Bastion · keep side fallback", "Bastion · keep rear fallback",
                "Bastion · dead palace wall", "Bastion · dead palace tower");
            var p=new Vector3(0,3.0f,7.25f);
            foreach(var q in new[]{new Vector4(-3.28f,-2.48f,2.05f,5.85f),new Vector4(3.18f,-2.45f,1.82f,4.95f),
                new Vector4(-2.55f,2.25f,1.85f,6.75f),new Vector4(2.45f,1.95f,1.65f,5.45f),new Vector4(-.62f,2.28f,1.62f,6.95f)})
                Imported("Valoria · restored masonry tower","MegaTower",p+new Vector3(q.x,.08f,q.y),q.z,q.w,0,new Color(.52f,.51f,.45f),false);
            Imported("Valoria · restored entry arch","MegaGate",p+new Vector3(0,.12f,-3.08f),2.35f,3.15f,0,new Color(.52f,.51f,.45f),false);
            foreach(float x in new[]{-2.15f,2.15f})
                Imported("Valoria · restored curtain masonry","MegaWall",p+new Vector3(x,.12f,-3.02f),2.55f,3.05f,0,new Color(.52f,.51f,.45f),false);
            foreach(float x in new[]{-1.55f,1.55f})
                Imported("Valoria · keep masonry facing","MegaWall",p+new Vector3(x,2.42f,-1.28f),3f,2.62f,0,new Color(.48f,.47f,.41f),false);
            foreach(float x in new[]{-2.25f,2.25f})
                Imported("Valoria · keep side masonry","MegaWall",p+new Vector3(x,2.32f,1.05f),2.75f,2.48f,90,new Color(.48f,.47f,.41f),false);
            Imported("Valoria · restored palace remnant","MegaWall",p+new Vector3(-4.15f,.10f,3.75f),4.2f,3.15f,16,new Color(.42f,.43f,.39f),false);
            Imported("Valoria · restored palace remnant","MegaWall",p+new Vector3(3.75f,.10f,4.20f),3.65f,2.85f,-18,new Color(.42f,.43f,.39f),false);
        }

        static void UnifyLandscape(bool city)
        {
            if(landscape==null)landscape=LandscapeMaterial();
            foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                string n=r.gameObject.name;
                bool isGround=city?(n.Contains("valley floor")||n.Contains("expansion terrain")||n.Contains("authored apron")||n.Contains("rebuilders terrace")||n.Contains("rebuilders upper shelf")||n.Contains("organic civic ground")):
                    (n.StartsWith("Frontier ·")&&!n.Contains("quarry")&&!n.Contains("corrupted")&&(n.Contains("floor")||n.Contains("earth")||n.Contains("approach")));
                if(!isGround||!r.enabled)continue;
                var filter=r.GetComponent<MeshFilter>();
                if(filter==null||filter.sharedMesh==null)continue;
                var mesh=Object.Instantiate(filter.sharedMesh);
                var v=mesh.vertices;var uv=new Vector2[v.Length];
                if(!city&&n=="Frontier · valley floor")
                {
                    // Extend only the collision-free geographic sheet beyond all official zooms.
                    for(int i=0;i<v.Length;i++){v[i].x*=3f;v[i].z*=3f;}
                    mesh.vertices=v;mesh.RecalculateBounds();
                }
                for(int i=0;i<v.Length;i++)
                {var w=filter.transform.TransformPoint(v[i]);uv[i]=new Vector2(w.x/180f+.5f,w.z/180f+.5f);}
                mesh.uv=uv;filter.sharedMesh=mesh;r.sharedMaterial=landscape;
            }
        }
        static Material LandscapeMaterial()
        {
            const int size=512;
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true){name="Eldoria continuous soil and moss",wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[size*size];
            var soil=new Color(.28f,.255f,.20f);var grass=new Color(.205f,.25f,.17f);var gravel=new Color(.35f,.34f,.285f);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float n=Mathf.PerlinNoise(x*.018f+14.3f,y*.018f+5.7f);
                float detail=Mathf.PerlinNoise(x*.27f+3.1f,y*.27f+11.2f);
                var c=Color.Lerp(soil,grass,Mathf.SmoothStep(0,1,(n-.29f)*2.4f));
                c=Color.Lerp(c,gravel,Mathf.Max(0,Mathf.PerlinNoise(x*.04f+31,y*.04f+47)-.58f)*1.0f);
                pixels[y*size+x]=c*(.92f+detail*.14f);
            }
            texture.SetPixels(pixels);texture.Apply(true,false);
            var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Eldoria surface earth · continuous terrain"};
            mat.SetTexture("_BaseMap",texture);mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_Smoothness",.02f);
            return mat;
        }

        static void Imported(string name,string resource,Vector3 p,float footprint,float height,float yaw,Color tint,bool foliage)
        {
            var source=Resources.Load<GameObject>("WorldInventory/"+resource);
            if(source==null)throw new InvalidOperationException("Missing recovered inventory: "+resource);
            var go=ValoriaKit.BenchmarkPiece(name,source,p,footprint,height,Quaternion.Euler(0,yaw,0));
            if(go==null)throw new InvalidOperationException("Empty recovered inventory: "+resource);
            go.transform.SetParent(root,true);
            Normalize(go,tint,foliage,resource);
        }
        static void Piece(string name,GameObject source,Vector3 p,float footprint,float height,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceModulated(name,source,p,footprint,height,Quaternion.Euler(0,yaw,0),tint);
            if(go!=null)go.transform.SetParent(root,true);
        }
        static void Normalize(GameObject go,Color tint,bool foliage,string resource)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mf=r.GetComponent<MeshFilter>();
                int count=mf!=null&&mf.sharedMesh!=null?mf.sharedMesh.subMeshCount:Mathf.Max(1,r.sharedMaterials.Length);
                var originals=r.sharedMaterials;var mats=new Material[count];
                for(int i=0;i<count;i++)
                {
                    var source=i<originals.Length?originals[i]:null;
                    bool leaves=foliage&&i>0;
                    string key=resource+"/"+i+"/"+ColorUtility.ToHtmlStringRGB(tint);
                    if(adapted.TryGetValue(key,out var cached)&&cached!=null){mats[i]=cached;continue;}
                    var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Eldoria adapted · "+resource+" "+i};
                    Texture texture=null,normal=null;
                    if(foliage)
                    {
                        string family=leaves?"Leaf01":"Trunk01";
                        texture=Resources.Load<Texture2D>("WorldInventory/"+family+"_ALB");
                        normal=Resources.Load<Texture2D>("WorldInventory/"+family+"_NRM");
                        m.SetColor("_BaseColor",leaves?new Color(.30f,.43f,.23f):new Color(.39f,.30f,.20f));
                    }
                    else if(resource.StartsWith("Rock")||resource=="Mountain01")
                    {
                        texture=Resources.Load<Texture2D>("WorldInventory/Rock01_ALB");
                        normal=Resources.Load<Texture2D>("WorldInventory/Rock01_NRM");m.SetColor("_BaseColor",tint);
                    }
                    else
                    {
                        foreach(string property in new[]{"_BaseMap","_MainTex","_Albedo"})
                            if(source!=null&&source.HasProperty(property)&&source.GetTexture(property)!=null){texture=source.GetTexture(property);break;}
                        if(texture!=null)m.SetColor("_BaseColor",tint);
                        else m=ValoriaKit.SurfaceMaterial(tint,"stone",new Vector2(3,3));
                    }
                    if(texture!=null)m.SetTexture("_BaseMap",texture);
                    if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
                    m.SetFloat("_Smoothness",.025f);m.SetFloat("_Metallic",0);
                    if(leaves){m.SetFloat("_AlphaClip",1);m.SetFloat("_Cutoff",.35f);m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cull",0);}
                    adapted[key]=m;mats[i]=m;
                }
                r.sharedMaterials=mats;
            }
        }
        static void StonePiece(int index,string name,Vector3 p,Vector3 dimensions,float yaw)
        {
            string[] names={"piece_01_10364tris","piece_02_12602tris","piece_03_6433tris","piece_04_7824tris","piece_05_4966tris","piece_06_7602tris"};
            var source=Resources.Load<GameObject>("Valoria/StoneKit/"+names[index-1]);
            if(source==null)throw new InvalidOperationException("Missing certified StoneKit piece: "+index);
            var go=Object.Instantiate(source);go.name=name;go.transform.rotation=Quaternion.identity;
            var bounds=Bounds(go);var s=bounds.size;
            go.transform.localScale=Vector3.Scale(go.transform.localScale,new Vector3(dimensions.x/s.x,dimensions.y/s.y,dimensions.z/s.z));
            bounds=Bounds(go);go.transform.position+=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z);
            go.transform.RotateAround(Vector3.zero,Vector3.up,yaw);go.transform.position+=p;go.transform.SetParent(root,true);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var materials=r.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    if(materials[i]==null)continue;
                    // Six source GLBs embed byte-identical albedo/normal/MR atlases.
                    // Share the first material across instances rather than duplicating GPU textures.
                    if(sharedStone!=null){materials[i]=sharedStone;continue;}
                    var m=new Material(materials[i]);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",new Color(.48f,.46f,.40f));
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.02f);
                    sharedStone=m;materials[i]=m;
                }
                r.sharedMaterials=materials;
            }
        }
        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;
        }
        static void Suppress(params string[] prefixes)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            foreach(var prefix in prefixes)if(r.gameObject.name.StartsWith(prefix))r.enabled=false;
        }
        static void Finish()
        {
            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }
        static void CityOrigin(Vector3 p,ValoriaExternalAssetLibrary art)
        {
            Piece("4X · player city keep · placeholder",Resources.Load<GameObject>("WorldInventory/MegaTower"),p+new Vector3(0,0,.55f),1.75f,2.8f,0,new Color(.64f,.61f,.54f));
            Piece("4X · player city gate · placeholder",Resources.Load<GameObject>("WorldInventory/MegaGate"),p+new Vector3(0,0,-.5f),2.1f,1.55f,0,new Color(.66f,.61f,.51f));
            Piece("4X · player city civil roof",art!=null?art.SlavicShed:null,p+new Vector3(-1.1f,0,.1f),1.2f,1.35f,18,new Color(.71f,.65f,.54f));
            Flag("4X · Valoria standard",p+new Vector3(.9f,.15f,.5f),Blue,1.4f);
        }
        static void March(Vector3 p)
        {
            // Small deliberate symbols; final animated troops remain a library gap.
            for(int i=0;i<5;i++)
            {
                var q=p+new Vector3((i%2)*.38f,0,(i/2)*.34f);
                Primitive("4X · deployed army · placeholder",PrimitiveType.Capsule,q+Vector3.up*.34f,new Vector3(.17f,.35f,.17f),new Color(.28f,.30f,.31f));
            }
            Flag("4X · march heading",p+new Vector3(-.18f,0,.22f),Blue,1.1f);
        }
        static void Ruin(Vector3 p)
        {
            Imported("4X · neutral ruin arch","Arch_Gothic",p,2.2f,2.8f,-12,new Color(.58f,.56f,.47f),false);
            Imported("4X · neutral ruin wall","Wall_Broken",p+new Vector3(1.1f,0,.6f),1.85f,1.2f,78,new Color(.52f,.52f,.45f),false);
            Imported("4X · neutral ruin column","Column_Round",p+new Vector3(-1.1f,0,.3f),.5f,1.35f,0,new Color(.57f,.56f,.48f),false);
        }
        static void Beast(string name,Vector3 p,bool boar)
        {
            // Compact silhouette placeholders explicitly tracked against World P0 Beast Kit.
            var c=boar?new Color(.31f,.25f,.18f):new Color(.39f,.41f,.40f);
            Primitive(name+" body",PrimitiveType.Sphere,p+new Vector3(0,.45f,0),new Vector3(boar?.95f:.73f,.54f,1.35f),c);
            Primitive(name+" head",PrimitiveType.Sphere,p+new Vector3(0,.56f,-.6f),new Vector3(.48f,.45f,.52f),c*.9f);
            for(int i=0;i<4;i++)Primitive(name+" leg",PrimitiveType.Capsule,p+new Vector3(i%2==0?-.25f:.25f,.23f,i<2?-.35f:.38f),new Vector3(.15f,.26f,.16f),c*.8f);
            if(boar)for(int i=0;i<2;i++)Primitive(name+" tusk",PrimitiveType.Capsule,p+new Vector3(i==0?-.20f:.20f,.49f,-.82f),new Vector3(.08f,.18f,.08f),new Color(.78f,.73f,.59f));
            else for(int i=0;i<2;i++)Primitive(name+" ear",PrimitiveType.Cube,p+new Vector3(i==0?-.17f:.17f,.89f,-.53f),new Vector3(.13f,.23f,.13f),c*.75f);
        }
        static void Civilian(Vector3 p)
        {
            Primitive("Valoria · worker silhouette · placeholder",PrimitiveType.Capsule,p+Vector3.up*.40f,new Vector3(.20f,.38f,.20f),new Color(.34f,.29f,.22f));
            Primitive("Valoria · worker head · placeholder",PrimitiveType.Sphere,p+Vector3.up*.86f,Vector3.one*.16f,new Color(.49f,.39f,.27f));
        }
        static void Flag(string name,Vector3 p,Color color,float height)
        {
            Primitive(name+" pole",PrimitiveType.Cylinder,p+Vector3.up*height*.5f,new Vector3(.035f,height*.5f,.035f),new Color(.25f,.20f,.13f));
            Primitive(name+" cloth",PrimitiveType.Cube,p+new Vector3(.23f,height*.77f,0),new Vector3(.45f,height*.30f,.035f),color);
        }
        static void Node(string label,Vector3 p,Color color,float radius)
        {
            var ring=new GameObject("4X · "+label+" strategic footprint").AddComponent<LineRenderer>();
            ring.transform.SetParent(root,true);ring.loop=true;ring.useWorldSpace=true;ring.positionCount=32;
            ring.startWidth=ring.endWidth=.065f;ring.sharedMaterial=ValoriaKit.Material(color);
            for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32;ring.SetPosition(i,p+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
            var text=new GameObject("4X · "+label+" semantic label").AddComponent<TextMesh>();
            text.transform.SetParent(root,true);text.transform.position=p+new Vector3(0,.38f,-radius-.22f);
            text.transform.rotation=Quaternion.LookRotation(new Vector3(-20,-24,22));
            text.text=label;text.fontSize=48;text.characterSize=.075f;text.anchor=TextAnchor.MiddleCenter;
            text.color=new Color(.87f,.81f,.66f);
        }
        static void Primitive(string name,PrimitiveType type,Vector3 p,Vector3 size,Color color)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=p;go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=ValoriaKit.Material(color);go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>())c.enabled=false;
        }
    }
}

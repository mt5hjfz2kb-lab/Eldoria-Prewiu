using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Visual-only macro-composition proof:
    /// mostly-flat buildable city + one controlled Bastion rise + readable wall/plaza axis.
    /// Gameplay topology, hotspots, colliders and progression remain owned by the canonical scene.
    /// </summary>
    public static class ValoriaFlatCitadelProofV1
    {
        public static bool Enabled = true;
        public const string RootName = "Valoria · Flat Citadel Proof v1";
        public static int HiddenLegacyRenderers { get; private set; }
        public static int WallPieces { get; private set; }
        public static int FunctionalBuildings { get; private set; }
        public static int NaturePieces { get; private set; }

        static readonly Color Earth = new Color(.43f,.37f,.27f,1f);
        static readonly Color WarmStone = new Color(.58f,.56f,.50f,1f);
        static readonly Color WallStone = new Color(.50f,.49f,.45f,1f);
        static readonly Color Blue = new Color(.16f,.28f,.44f,1f);
        static readonly Color Amber = new Color(1f,.56f,.20f,1f);

        public static void Build(Transform parent, PlayerState state)
        {
            if(!Enabled || parent==null) return;

            var old=GameObject.Find(RootName);
            if(old!=null) Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            int clonedHeroPieces=CloneCanonicalHeroBastion(root,new Vector3(0f,-.58f,0f));
            HiddenLegacyRenderers=HideAllNonProofRenderers(root);

            BuildFlatCitySurface(root);
            BuildPrimaryAxis(root);
            BuildBastionRise(root);
            BuildFunctionalArchitecture(root,state);
            BuildOuterWall(root);
            BuildSparseNature(root);
            BuildLifeCues(root);

            DisableGameplay(root.gameObject);
        }

        static int CloneCanonicalHeroBastion(Transform proofRoot,Vector3 offset)
        {
            var sources=new System.Collections.Generic.HashSet<GameObject>();
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t==null||t==proofRoot)continue;
                string n=t.name.ToLowerInvariant();
                bool hero=n.StartsWith("bastion ·")||n.StartsWith("valoria · bastion hero");
                if(!hero)continue;

                // Clone only the highest object in a same-family chain to avoid duplicate child meshes.
                var p=t.parent;
                bool parentHero=false;
                while(p!=null)
                {
                    string pn=p.name.ToLowerInvariant();
                    if(pn.StartsWith("bastion ·")||pn.StartsWith("valoria · bastion hero")){parentHero=true;break;}
                    p=p.parent;
                }
                if(!parentHero && t.GetComponentInChildren<Renderer>(true)!=null)sources.Add(t.gameObject);
            }

            int count=0;
            foreach(var source in sources)
            {
                var clone=Object.Instantiate(source);
                clone.name="Valoria · Flat Citadel · hero clone · "+source.name;
                clone.transform.position+=offset;
                clone.transform.SetParent(proofRoot,true);
                foreach(var col in clone.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
                foreach(var h in clone.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
                count++;
            }
            return count;
        }

        static int HideAllNonProofRenderers(Transform proofRoot)
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled)continue;
                if(r.transform.IsChildOf(proofRoot))continue;
                r.enabled=false;
                count++;
            }
            return count;
        }

        static void BuildFlatCitySurface(Transform root)
        {
            // Large surrounding natural floor means the city no longer reads as a raised island/podium.
            AddSlab(root,"surrounding meadow",new Vector3(0,-.06f,1.0f),new Vector3(31f,.08f,26f),
                ValoriaKit.DetailedSurfaceMaterial(new Color(.34f,.40f,.27f,1f),"earth",new Vector2(6.5f,6.5f),.88f));

            // 82% of the useful footprint is one coherent, near-flat buildable plane.
            // The perimeter is intentionally irregular enough not to read as a rectangular test board.
            Vector2[] ring={
                new Vector2(-10.4f,-7.0f),new Vector2(-6.2f,-7.7f),new Vector2(-1.8f,-7.5f),
                new Vector2( 3.0f,-7.7f),new Vector2( 8.0f,-7.1f),new Vector2(10.4f,-5.0f),
                new Vector2(10.7f, 0.4f),new Vector2(10.2f, 6.2f),new Vector2( 7.7f, 9.7f),
                new Vector2( 2.9f,10.5f),new Vector2(-2.8f,10.4f),new Vector2(-7.9f, 9.5f),
                new Vector2(-10.3f,6.0f),new Vector2(-10.8f,.2f)
            };
            CreatePrism(root,"main buildable city plane",ring,.015f,.075f,
                ValoriaKit.DetailedSurfaceMaterial(Earth,"earth",new Vector2(3.2f,3.2f),.92f),
                ValoriaKit.DetailedSurfaceMaterial(Earth*.72f,"rock",new Vector2(3.0f,3.0f),1.0f));

            // Reserve parcels are readable but quiet. They are intentionally empty space for future growth.
            AddParcel(root,"west growth parcel",new Vector3(-6.6f,.105f,3.2f),new Vector3(4.0f,.05f,3.1f),new Color(.38f,.34f,.27f,1f));
            AddParcel(root,"east growth parcel",new Vector3( 6.5f,.105f,3.0f),new Vector3(4.1f,.05f,3.0f),new Color(.38f,.34f,.27f,1f));
        }

        static void BuildPrimaryAxis(Transform root)
        {
            var roadMat=ValoriaKit.DetailedSurfaceMaterial(new Color(.55f,.53f,.48f,1f),"stone",new Vector2(2.3f,2.3f),1.0f);

            AddSlab(root,"gate road",new Vector3(0,.13f,-4.7f),new Vector3(2.8f,.06f,4.3f),roadMat);
            AddSlab(root,"central plaza",new Vector3(0,.14f,.65f),new Vector3(6.4f,.07f,5.0f),roadMat);
            AddSlab(root,"bastion approach",new Vector3(0,.15f,4.1f),new Vector3(3.4f,.07f,2.5f),roadMat);

            // Two restrained cross streets make the construction plots legible at strategic zoom.
            AddSlab(root,"west branch",new Vector3(-4.2f,.135f,-1.0f),new Vector3(5.3f,.055f,1.55f),roadMat);
            AddSlab(root,"east branch",new Vector3( 4.2f,.135f,-1.0f),new Vector3(5.3f,.055f,1.55f),roadMat);

            // Plaza monument establishes a visual center without filling the city.
            ValoriaKit.Cylinder("Valoria · Flat Citadel · plaza plinth",
                new Vector3(0,.26f,.65f),new Vector3(.70f,.18f,.70f),WarmStone,Quaternion.identity);
            ValoriaKit.Banner("Valoria · Flat Citadel · central standard",
                new Vector3(0,1.25f,.65f),new Vector3(.42f,1.35f,.06f),Blue);
        }

        static void BuildBastionRise(Transform root)
        {
            // One controlled elevation only: a civic/defensive plinth, not a mountain.
            Vector2[] ring={
                new Vector2(-4.25f,4.9f),new Vector2(-3.5f,8.8f),new Vector2(-1.6f,9.7f),
                new Vector2(1.8f,9.7f),new Vector2(3.6f,8.7f),new Vector2(4.25f,5.0f),
                new Vector2(2.4f,4.35f),new Vector2(-2.4f,4.35f)
            };
            CreatePrism(root,"bastion civic rise",ring,.08f,.72f,
                ValoriaKit.DetailedSurfaceMaterial(new Color(.46f,.44f,.39f,1f),"stone",new Vector2(2.6f,2.6f),1.0f),
                ValoriaKit.DetailedSurfaceMaterial(new Color(.37f,.36f,.33f,1f),"stone",new Vector2(2.2f,2.2f),1.0f));

            for(int i=0;i<7;i++)
            {
                float y=.16f+i*.085f;
                float z=4.25f+i*.25f;
                AddSlab(root,"bastion stair "+i,new Vector3(0,y,z),
                    new Vector3(3.25f,.08f,.42f),
                    ValoriaKit.DetailedSurfaceMaterial(WarmStone*.86f,"stone",new Vector2(2f,2f),1.0f));
            }
        }

        static void BuildFunctionalArchitecture(Transform root, PlayerState state)
        {
            FunctionalBuildings=0;
            int before=0;

            var heroSource=Resources.Load<GameObject>("Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1");
            if(heroSource!=null)
            {
                var hero=Object.Instantiate(heroSource);
                hero.name="Valoria · Flat Citadel · Hero Bastion";
                FitPrefab(hero,new Vector3(0,.76f,7.25f),7.6f,7.9f);
                hero.transform.SetParent(root,true);
                DisableGameplay(hero);
                FunctionalBuildings++;
            }
            else
            {
                before=root.childCount;
                ValoriaKit.BastionCore("Bastion · Flat Citadel",new Vector3(0,.78f,7.25f),Glow);
                ReparentNew(root,before); FunctionalBuildings++;
            }

            before=root.childCount;
            ValoriaKit.SawmillArchitecture("Aserradero · Flat Citadel",new Vector3(-5.9f,.16f,-1.55f),state.SawmillLevel>0,Glow);
            ReparentNew(root,before); FunctionalBuildings++;

            before=root.childCount;
            ValoriaKit.BarracksArchitecture("Cuartel · Flat Citadel",new Vector3(5.9f,.16f,-1.75f),state.BarracksLevel>0,Glow);
            ReparentNew(root,before); FunctionalBuildings++;

            before=root.childCount;
            ValoriaKit.GranaryArchitecture("Granero · Flat Citadel",new Vector3(-2.7f,.15f,-4.4f),true,Glow);
            ReparentNew(root,before); FunctionalBuildings++;

            // Only two small rebuilt homes: this is a settlement with headroom, not a finished metropolis.
            before=root.childCount;
            ValoriaKit.House("Valoria · Flat Citadel · cottage west",new Vector3(-5.8f,.18f,3.55f),new Vector3(2.0f,1.15f,1.7f),true,Glow);
            ReparentNew(root,before);
            before=root.childCount;
            ValoriaKit.House("Valoria · Flat Citadel · cottage east",new Vector3(5.7f,.18f,3.45f),new Vector3(2.0f,1.15f,1.7f),true,Glow);
            ReparentNew(root,before);
        }

        static void BuildOuterWall(Transform root)
        {
            WallPieces=0;
            var art=ValoriaExternalAssetLibrary.Load();

            // Front wall deliberately opens at the main gate.
            for(int i=0;i<4;i++)
            {
                float x=-8.8f+i*2.2f;
                WallSegment(root,new Vector3(x,.12f,-6.35f),new Vector3(2.1f,1.18f,.42f),0f); WallPieces++;
            }
            for(int i=0;i<4;i++)
            {
                float x=2.2f+i*2.2f;
                WallSegment(root,new Vector3(x,.12f,-6.35f),new Vector3(2.1f,1.18f,.42f),0f); WallPieces++;
            }

            for(int i=0;i<8;i++)
            {
                float z=-4.9f+i*2.0f;
                WallSegment(root,new Vector3(-9.55f,.12f,z),new Vector3(.42f,1.18f,1.95f),0f); WallPieces++;
                WallSegment(root,new Vector3( 9.55f,.12f,z),new Vector3(.42f,1.18f,1.95f),0f); WallPieces++;
            }
            for(int i=0;i<8;i++)
            {
                float x=-7.7f+i*2.2f;
                WallSegment(root,new Vector3(x,.12f,9.25f),new Vector3(2.1f,1.18f,.42f),0f); WallPieces++;
            }

            // Authored gate and four compact corner towers improve silhouette while the wall remains continuous.
            if(art!=null && art.SlavicRockGate!=null)
            {
                var g=ValoriaKit.BenchmarkPieceTinted("Valoria · Flat Citadel · main gate",art.SlavicRockGate,
                    new Vector3(0,.12f,-6.45f),3.5f,3.1f,Quaternion.identity,WarmStone*.88f);
                if(g!=null) g.transform.SetParent(root,true);
            }
            foreach(var p in new[]{
                new Vector3(-9.5f,.22f,-6.2f),new Vector3(9.5f,.22f,-6.2f),
                new Vector3(-9.5f,.22f,9.1f),new Vector3(9.5f,.22f,9.1f)})
            {
                ValoriaKit.Wall("Valoria · Flat Citadel · corner tower",p+new Vector3(0,.85f,0),
                    new Vector3(1.15f,2.05f,1.15f),WallStone,false);
                ReparentNewest(root);
            }
        }

        static void BuildSparseNature(Transform root)
        {
            NaturePieces=0;
            foreach(var p in new[]{
                new Vector3(-11.2f,.02f,-4.8f),new Vector3(-11.4f,.02f,1.0f),new Vector3(-11.0f,.02f,7.4f),
                new Vector3(11.2f,.02f,-4.5f),new Vector3(11.3f,.02f,1.4f),new Vector3(11.0f,.02f,7.3f),
                new Vector3(-7.9f,.02f,10.8f),new Vector3(7.8f,.02f,10.9f)})
            {
                int before=root.childCount;
                ValoriaKit.PineTree("Valoria · Flat Citadel · perimeter pine",p,.58f);
                ReparentNew(root,before); NaturePieces++;
            }

            var art=ValoriaExternalAssetLibrary.Load();
            if(art!=null && art.SlavicBoulder!=null)
            {
                foreach(var p in new[]{new Vector3(-10.7f,.05f,4.5f),new Vector3(10.8f,.05f,4.2f)})
                {
                    var go=ValoriaKit.BenchmarkPieceTinted("Valoria · Flat Citadel · perimeter boulder",art.SlavicBoulder,
                        p,1.55f,1.0f,Quaternion.Euler(0,p.x<0?28f:205f,0),new Color(.38f,.39f,.36f,1f));
                    if(go!=null){go.transform.SetParent(root,true);NaturePieces++;}
                }
            }
        }

        static void BuildLifeCues(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art!=null && art.Firewood!=null)
            {
                var wood=ValoriaKit.BenchmarkPiece("Valoria · Flat Citadel · sawmill stock",art.Firewood,
                    new Vector3(-7.2f,.16f,-2.9f),1.15f,.8f,Quaternion.Euler(0,18f,0));
                if(wood!=null) wood.transform.SetParent(root,true);
            }
            Glow("Valoria · Flat Citadel · bastion warmth",new Vector3(0,3.7f,7.0f),Amber,1.25f,5.6f);
            Glow("Valoria · Flat Citadel · sawmill warmth",new Vector3(-5.3f,1.4f,-1.8f),Amber,1.0f,3.0f);
            Glow("Valoria · Flat Citadel · barracks warmth",new Vector3(5.3f,1.4f,-1.9f),Amber,.95f,2.8f);
        }

        static void WallSegment(Transform root,Vector3 p,Vector3 size,float yaw)
        {
            ValoriaKit.Wall("Valoria · Flat Citadel · outer wall",p+new Vector3(0,size.y*.50f,0),size,WallStone,false);
            ReparentNewest(root);
        }

        static void AddParcel(Transform root,string name,Vector3 p,Vector3 size,Color tint)
        {
            AddSlab(root,name,p,size,ValoriaKit.DetailedSurfaceMaterial(tint,"earth",new Vector2(2.8f,2.8f),.95f));
        }

        static void AddSlab(Transform root,string name,Vector3 p,Vector3 size,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="Valoria · Flat Citadel · "+name;
            go.transform.SetParent(root,true);
            go.transform.position=p;
            go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=mat;
            var c=go.GetComponent<Collider>(); if(c!=null)c.enabled=false;
        }

        static void CreatePrism(Transform root,string role,Vector2[] ring,float bottom,float top,Material topMat,Material sideMat)
        {
            int n=ring.Length;
            var topVerts=new Vector3[n+1];
            topVerts[0]=new Vector3(0,top,1.2f);
            for(int i=0;i<n;i++)topVerts[i+1]=new Vector3(ring[i].x,top,ring[i].y);
            var topTris=new int[n*3];
            for(int i=0;i<n;i++)
            {
                topTris[i*3]=0;
                topTris[i*3+1]=i+1;
                topTris[i*3+2]=((i+1)%n)+1;
            }
            var topMesh=new Mesh{name="Flat Citadel "+role+" top"};
            topMesh.vertices=topVerts; topMesh.triangles=topTris; topMesh.RecalculateNormals(); topMesh.RecalculateBounds();
            var topGo=new GameObject("Valoria · Flat Citadel · "+role+" top");
            topGo.transform.SetParent(root,true);
            topGo.AddComponent<MeshFilter>().sharedMesh=topMesh;
            var tr=topGo.AddComponent<MeshRenderer>();tr.sharedMaterial=topMat;tr.shadowCastingMode=ShadowCastingMode.Off;tr.receiveShadows=true;

            var sideVerts=new Vector3[n*4];
            var sideTris=new int[n*6];
            for(int i=0;i<n;i++)
            {
                int j=(i+1)%n;
                int v=i*4;
                sideVerts[v]=new Vector3(ring[i].x,top,ring[i].y);
                sideVerts[v+1]=new Vector3(ring[j].x,top,ring[j].y);
                sideVerts[v+2]=new Vector3(ring[i].x,bottom,ring[i].y);
                sideVerts[v+3]=new Vector3(ring[j].x,bottom,ring[j].y);
                int t=i*6;
                sideTris[t]=v;sideTris[t+1]=v+2;sideTris[t+2]=v+1;
                sideTris[t+3]=v+1;sideTris[t+4]=v+2;sideTris[t+5]=v+3;
            }
            var sideMesh=new Mesh{name="Flat Citadel "+role+" sides"};
            sideMesh.vertices=sideVerts;sideMesh.triangles=sideTris;sideMesh.RecalculateNormals();sideMesh.RecalculateBounds();
            var sideGo=new GameObject("Valoria · Flat Citadel · "+role+" sides");
            sideGo.transform.SetParent(root,true);
            sideGo.AddComponent<MeshFilter>().sharedMesh=sideMesh;
            var sr=sideGo.AddComponent<MeshRenderer>();sr.sharedMaterial=sideMat;sr.shadowCastingMode=ShadowCastingMode.On;sr.receiveShadows=true;
        }

        static void FitPrefab(GameObject go,Vector3 ground,float span,float maxHeight)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return;
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            float horizontal=Mathf.Max(b.size.x,b.size.z);
            float scale=Mathf.Min(span/Mathf.Max(.001f,horizontal),maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            rs=go.GetComponentsInChildren<Renderer>(true);
            b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void ReparentNew(Transform root,int previousChildCount)
        {
            // ValoriaKit helpers create under the canonical integration root.
            // Pull only newly-created top-level visuals into this proof root.
            var canonical=root.parent;
            if(canonical==null)return;
            for(int i=canonical.childCount-1;i>=0;i--)
            {
                var c=canonical.GetChild(i);
                if(c==root)continue;
                if(c.GetSiblingIndex()<previousChildCount)continue;
                if(c.name.StartsWith("Valoria · Flat Citadel") || c.name.Contains("Flat Citadel"))
                    c.SetParent(root,true);
            }
        }

        static void ReparentNewest(Transform root)
        {
            var canonical=root.parent;
            if(canonical==null)return;
            for(int i=canonical.childCount-1;i>=0;i--)
            {
                var c=canonical.GetChild(i);
                if(c==root)continue;
                if(c.name.Contains("Flat Citadel")){c.SetParent(root,true);return;}
            }
        }

        static void Glow(string name,Vector3 p,Color color,float intensity,float range)
        {
            var go=new GameObject(name);
            go.transform.position=p;
            var l=go.AddComponent<Light>();
            l.type=LightType.Point;l.color=color;l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
            var root=GameObject.Find(RootName);
            if(root!=null)go.transform.SetParent(root.transform,true);
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}

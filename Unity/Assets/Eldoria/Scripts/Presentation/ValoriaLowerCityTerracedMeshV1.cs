using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Visual-only authored terraced mesh family for the lower city.
    // It replaces both the broad TerrainData slab read and scattered floating-rock supports
    // while leaving all gameplay colliders/hotspots/routes untouched.
    public static class ValoriaLowerCityTerracedMeshV1
    {
        public static bool Enabled=false;
        public static int PiecesBuilt{get;private set;}

        const string RootName="Valoria · Lower City Terraced Mesh v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            PiecesBuilt=0;
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            var earth=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.36f,.315f,.25f,1f),new Vector2(1.15f,1.15f),.016f,.90f)
                ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.35f,.305f,.245f,1f),"earth",new Vector2(1.15f,1.15f),.90f);

            Material rock;
            var template=Resources.Load<Material>("Valoria/LowerCityWorldRock");
            if(template!=null)
            {
                rock=new Material(template){name="Valoria · terraced mesh world rock"};
                if(rock.HasProperty("_Color"))rock.SetColor("_Color",new Color(.41f,.42f,.39f,1f));
                if(rock.HasProperty("_Tiling"))rock.SetFloat("_Tiling",.080f);
                if(rock.HasProperty("_Smoothness"))rock.SetFloat("_Smoothness",.12f);
                if(rock.HasProperty("_Strength"))rock.SetFloat("_Strength",.76f);
            }
            else
            {
                rock=ValoriaKit.ExternalPbrSurfaceMaterial(
                    "rock",new Color(.38f,.39f,.37f,1f),new Vector2(.95f,.95f),.018f,.98f)
                    ?? ValoriaKit.DetailedSurfaceMaterial(new Color(.37f,.38f,.36f,1f),"stone",new Vector2(.95f,.95f),.98f);
            }

            var topRock=new Material(rock){name="Valoria · terraced mesh top rock"};
            if(topRock.HasProperty("_Color"))topRock.SetColor("_Color",new Color(.47f,.46f,.41f,1f));
            if(topRock.HasProperty("_Tiling"))topRock.SetFloat("_Tiling",.070f);
            if(topRock.HasProperty("_Smoothness"))topRock.SetFloat("_Smoothness",.10f);

            // v26: v22-v25b prove the existing terrace library cannot create a single
            // continuous lower-city mass at the official camera. Build one visual-only
            // authored cliff substrate and reuse the existing production rock material.
            // Gameplay colliders, hotspots and the real stair route remain untouched.
            BuildUnifiedCliffMass(root,rock);
        }

        static void BuildUnifiedCliffMass(Transform root,Material rock)
        {
            var go=new GameObject("Valoria · Lower City unified cliff mass v1");
            go.transform.SetParent(root,false);
            var mf=go.AddComponent<MeshFilter>();
            var mr=go.AddComponent<MeshRenderer>();
            mr.sharedMaterial=rock;

            // v27: retract the mass from the camera and grow it into the hero-island foot.
            // Keep the footprint compact and the vertical drop shallow so the result reads
            // as a rocky transition under the buildings, not a broad cloth-like foreground skirt.
            var z=new[]{1.05f,1.85f,2.70f,3.60f,4.50f,5.35f,6.05f};
            var half=new[]{2.15f,2.55f,3.00f,3.45f,3.85f,4.10f,4.25f};
            var topY=new[]{.82f,.98f,1.18f,1.38f,1.58f,1.76f,1.92f};
            var drop=new[]{.46f,.54f,.62f,.72f,.82f,.90f,.96f};

            var v=new List<Vector3>();
            var uv=new List<Vector2>();
            var tris=new List<int>();

            for(int i=0;i<z.Length;i++)
            {
                float wobble=Mathf.Sin(i*1.83f)*.11f;
                float h=half[i];
                float y=topY[i]+Mathf.Sin(i*2.27f)*.035f;
                float low=y-drop[i];

                v.Add(new Vector3(-h-.26f+wobble,low,z[i]-.05f));
                v.Add(new Vector3(-h*.58f+wobble*.35f,y-.16f,z[i]+.04f));
                v.Add(new Vector3(wobble*.18f,y,z[i]));
                v.Add(new Vector3(h*.58f+wobble*.20f,y-.13f,z[i]-.03f));
                v.Add(new Vector3(h+.26f+wobble*.15f,low+.04f,z[i]+.05f));

                for(int k=0;k<5;k++)
                {
                    var p=v[v.Count-5+k];
                    uv.Add(new Vector2(p.x*.105f,p.z*.105f));
                }
            }

            for(int i=0;i<z.Length-1;i++)
            {
                int a=i*5,b=(i+1)*5;
                for(int k=0;k<4;k++)
                {
                    tris.Add(a+k);tris.Add(b+k+1);tris.Add(a+k+1);
                    tris.Add(a+k);tris.Add(b+k);tris.Add(b+k+1);
                }
            }

            // Close the front/rear faces so the approach reads as one mountain mass.
            tris.Add(0);tris.Add(2);tris.Add(1);
            tris.Add(0);tris.Add(4);tris.Add(2);
            tris.Add(2);tris.Add(4);tris.Add(3);
            int e=(z.Length-1)*5;
            tris.Add(e);tris.Add(e+1);tris.Add(e+2);
            tris.Add(e);tris.Add(e+2);tris.Add(e+4);
            tris.Add(e+2);tris.Add(e+3);tris.Add(e+4);

            var mesh=new Mesh{name="Valoria Lower City Unified Cliff Mass v1"};
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(v);
            mesh.SetTriangles(tris,0);
            mesh.SetUVs(0,uv);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mf.sharedMesh=mesh;

            foreach(var col in go.GetComponentsInChildren<Collider>(true))col.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            PiecesBuilt=1;
        }

        static void BuildCertifiedSubstrate(Transform root)
        {
            // v22: BroadRockPlatform proved visually oversized/detached at the official camera.
            // Keep only the certified stepped terrain family and use it as compact rock seating.
            // v23: the three front supports in v22 still read as detached pillars.
            // Use only two compact shelves tucked under the lower buildings so the central
            // stair visually reaches the fortress mass without a separate foreground island.
            // v25: form one continuous geological approach with overlapping certified
            // terraces instead of a procedural stair support + detached side shelves.
            // v26: artifact audit proved the lower spine and side shelves remain visible
            // as detached rocks under the city. Keep only the two upper overlapping pieces
            // that actually merge into the fortress foot.
            AddCertifiedTerrain(root,"SteppedRockTerrace","middle spine",new Vector3(0f,0f,2.05f),.92f,2.95f,188f);
            AddCertifiedTerrain(root,"SteppedRockTerrace","upper spine",new Vector3(0f,0f,3.25f),1.42f,3.25f,12f);
        }

        static void AddCertifiedTerrain(Transform root,string resource,string role,Vector3 anchor,float topY,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);
            if(source==null)return;

            var go=Object.Instantiate(source);
            if(go==null)return;
            go.name="Valoria · Lower City authored terrain · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);

            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0){Object.DestroyImmediate(go);return;}
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            float scale=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            go.transform.localScale*=scale;

            rs=go.GetComponentsInChildren<Renderer>(true);
            b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=new Vector3(anchor.x-b.center.x,topY-b.max.y,anchor.z-b.center.z);
            go.transform.SetParent(root,true);

            foreach(var col in go.GetComponentsInChildren<Collider>(true))col.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            PiecesBuilt++;
        }

        static void DressEdges(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            var rock=art.SlavicBoulder??art.SlavicFlatRock;
            if(rock!=null)
            {
                AddDecor(root,rock,"central edge west",new Vector3(-3.15f,-.62f,-2.15f),1.85f,1.05f,26f);
                AddDecor(root,rock,"central edge east",new Vector3(3.10f,-.64f,-2.18f),1.85f,1.05f,206f);
                AddDecor(root,rock,"west edge outer",new Vector3(-5.20f,-.42f,-2.42f),1.35f,.78f,38f);
                AddDecor(root,rock,"west edge inner",new Vector3(-4.05f,-.40f,-2.12f),1.20f,.70f,74f);
                AddDecor(root,rock,"east edge inner",new Vector3(4.05f,-.40f,-2.12f),1.20f,.70f,238f);
                AddDecor(root,rock,"east edge outer",new Vector3(5.25f,-.42f,-2.45f),1.35f,.78f,314f);

                // v17: break the broad front lip into authored rock masses so the
                // lower city reads as terrain cut into the mountain, not a grey board.
                AddDecor(root,rock,"front west",new Vector3(-2.55f,-.50f,-2.12f),1.35f,.66f,18f);
                AddDecor(root,rock,"front centre west",new Vector3(-.78f,-.54f,-2.22f),1.25f,.70f,74f);
                AddDecor(root,rock,"front centre east",new Vector3(.82f,-.54f,-2.20f),1.28f,.70f,122f);
                AddDecor(root,rock,"front east",new Vector3(2.55f,-.50f,-2.12f),1.35f,.66f,198f);

                // v18: seed the top plane with low rock shelves so the earth surface breaks
                // into authored terraces instead of remaining one uninterrupted flat colour.
                var flat=art.SlavicFlatRock;
                if(flat!=null)
                {
                    AddDecor(root,flat,"top west shelf",new Vector3(-4.20f,-.04f,-1.35f),2.10f,.24f,28f);
                    AddDecor(root,flat,"top west inner",new Vector3(-2.65f,-.06f,-1.20f),1.65f,.20f,72f);
                    AddDecor(root,flat,"top east inner",new Vector3(2.55f,-.06f,-1.18f),1.65f,.20f,252f);
                    AddDecor(root,flat,"top east shelf",new Vector3(4.18f,-.04f,-1.38f),2.10f,.24f,208f);
                }
            }

            if(art.SlavicBush!=null)
            {
                AddDecor(root,art.SlavicBush,"west scrub a",new Vector3(-5.05f,-.02f,-1.95f),.70f,.85f,18f);
                AddDecor(root,art.SlavicBush,"west scrub b",new Vector3(-4.15f,-.02f,-1.55f),.68f,.82f,66f);
                AddDecor(root,art.SlavicBush,"east scrub a",new Vector3(4.15f,-.02f,-1.60f),.68f,.82f,208f);
                AddDecor(root,art.SlavicBush,"east scrub b",new Vector3(5.10f,-.02f,-2.00f),.70f,.85f,292f);
            }
        }

        static void AddDecor(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw)
        {
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · terraced edge · "+role,source,ground,footprint,maxHeight,
                Quaternion.Euler(0f,yaw,0f),new Color(.48f,.48f,.44f,1f));
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static Vector2 V(float x,float z)=>new Vector2(x,z);

        static void Create(Transform root,string role,Vector2[] poly,float topY,float midY,float bottomY,Material earth,Material rock)
        {
            if(poly==null||poly.Length<3)return;

            var go=new GameObject("Valoria · terraced mesh · "+role);
            go.transform.SetParent(root,false);
            var mf=go.AddComponent<MeshFilter>();
            var mr=go.AddComponent<MeshRenderer>();
            mf.sharedMesh=BuildMesh(role,poly,topY,midY,bottomY);
            mr.sharedMaterials=new[]{earth,rock};
            PiecesBuilt++;
        }

        static Mesh BuildMesh(string role,Vector2[] poly,float topY,float midY,float bottomY)
        {
            int n=poly.Length;
            var centroid=Vector2.zero;
            for(int i=0;i<n;i++)centroid+=poly[i];
            centroid/=n;

            var verts=new List<Vector3>(1+n*3);
            var uvs=new List<Vector2>(1+n*3);
            var top=new List<int>(n*3);
            var sides=new List<int>(n*12);

            verts.Add(new Vector3(centroid.x,topY,centroid.y));
            uvs.Add(new Vector2(centroid.x*.12f,centroid.y*.12f));

            int topStart=verts.Count;
            for(int i=0;i<n;i++)
            {
                var p=poly[i];
                float noise=(Mathf.PerlinNoise((p.x+13f)*.23f,(p.y+17f)*.23f)-.5f)*.10f;
                verts.Add(new Vector3(p.x,topY+noise,p.y));
                uvs.Add(new Vector2(p.x*.12f,p.y*.12f));
            }

            int midStart=verts.Count;
            for(int i=0;i<n;i++)
            {
                var p=poly[i];
                var dir=(p-centroid).normalized;
                float irregular=.18f+(Mathf.PerlinNoise((p.x+29f)*.31f,(p.y+3f)*.31f)-.5f)*.16f;
                var q=p+dir*irregular;
                verts.Add(new Vector3(q.x,midY+(i%3-1)*.07f,q.y));
                uvs.Add(new Vector2(i/(float)n,.5f));
            }

            int botStart=verts.Count;
            for(int i=0;i<n;i++)
            {
                var p=poly[i];
                var dir=(p-centroid).normalized;
                float irregular=.48f+(Mathf.PerlinNoise((p.x+7f)*.19f,(p.y+41f)*.19f)-.5f)*.28f;
                var q=p+dir*irregular;
                verts.Add(new Vector3(q.x,bottomY+(i%4-1.5f)*.09f,q.y));
                uvs.Add(new Vector2(i/(float)n,0f));
            }

            for(int i=0;i<n;i++)
            {
                int a=topStart+i;
                int b=topStart+((i+1)%n);
                top.Add(0);top.Add(b);top.Add(a);

                int ma=midStart+i,mb=midStart+((i+1)%n);
                int ba=botStart+i,bb=botStart+((i+1)%n);

                sides.Add(a);sides.Add(b);sides.Add(mb);
                sides.Add(a);sides.Add(mb);sides.Add(ma);
                sides.Add(ma);sides.Add(mb);sides.Add(bb);
                sides.Add(ma);sides.Add(bb);sides.Add(ba);
            }

            var mesh=new Mesh{name="Valoria Lower City Terraced Mesh · "+role};
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.subMeshCount=2;
            mesh.SetTriangles(top,0);
            mesh.SetTriangles(sides,1);
            mesh.SetUVs(0,uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

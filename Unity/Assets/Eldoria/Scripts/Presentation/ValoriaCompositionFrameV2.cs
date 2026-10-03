using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Experimental presentation only. All functional objects and camera contracts stay put.
    public static class ValoriaCompositionFrameV2
    {
        public static int VisualPieces{get;private set;}
        public static int RestoredFunctionalRenderers{get;private set;}
        static Transform root;
        static Material ground,rock,stone;

        public static void Build(Transform parent,PlayerState state)
        {
            if(parent==null||state==null)throw new ArgumentNullException();
            var old=GameObject.Find("Valoria · Composition Frame v2");
            if(old!=null)Object.DestroyImmediate(old);
            root=new GameObject("Valoria · Composition Frame v2").transform;
            root.SetParent(parent,true);
            VisualPieces=0;RestoredFunctionalRenderers=0;
            var shader=Shader.Find("Eldoria/ValoriaCompositionGround");
            if(shader==null)throw new Exception("Composition ground shader missing.");
            ground=new Material(shader){name="Valoria v2 · blended rock and earth"};
            foreach(var pair in new[]{("_RockTex","rock_diff"),("_GroundTex","dirt_diff"),("_RockNormal","rock_normal")})
            {
                var tex=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/"+pair.Item2);
                if(tex==null)throw new Exception("Shared surface missing: "+pair.Item2);
                tex.wrapMode=TextureWrapMode.Repeat;
                tex.anisoLevel=8;
                ground.SetTexture(pair.Item1,tex);
            }
            rock=ValoriaKit.ExternalPbrSurfaceMaterial("rock",new Color(.76f,.75f,.69f),new Vector2(.52f,.52f),.035f,.95f);
            stone=ValoriaKit.ExternalPbrSurfaceMaterial("stone",new Color(.77f,.75f,.68f),new Vector2(.54f,.54f),.035f,.92f);
            if(ground==null||rock==null||stone==null)throw new Exception("Composition frame shared PBR maps missing.");
            BuildAuthoredCityMass();
            RestoreFunctional("Aserradero · dedicated sawmill");
            RestoreFunctional("Cuartel · dedicated barracks");
            Add("Valoria/Valoria_Granero_BIII_v1","civic granary",new Vector3(-2.6f,.34f,-6.1f),3.3f,2.7f,16f);
            BuildTerracedMasonry();
            BuildEmbeddedDistrict();
            BuildRoute();
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.55f,.61f,.68f);
            RenderSettings.fogStartDistance=38f;
            RenderSettings.fogEndDistance=95f;
        }

        static float Smooth(float a,float b,float x)
        {float t=Mathf.Clamp01((x-a)/(b-a));return t*t*(3f-2f*t);}
        static float Seat(float x,float z,float cx,float cz,float rx,float rz,float edge)
        {
            float r=Mathf.Sqrt(Mathf.Pow((x-cx)/rx,2)+Mathf.Pow((z-cz)/rz,2));
            return 1f-Smooth(1f-edge,1f+edge,r);
        }
        static float Height(float x,float z)
        {
            float macro=(Mathf.PerlinNoise(x*.18f+20f,z*.16f+31f)-.5f)*.46f;
            float micro=(Mathf.PerlinNoise(x*.91f+41f,z*.78f+18f)-.5f)*.12f;
            float lower=Seat(x,z,0,-2.8f,10.7f,8.3f,.29f);
            float y=Mathf.Lerp(-2.2f+macro,.35f+micro,lower);
            float mid=Seat(x,z,0,3.5f,8.35f,5.15f,.15f);
            y=Mathf.Lerp(y,1.18f+micro*.45f,mid);
            float upper=Seat(x,z,0,8.75f,7.3f,7.6f,.18f);
            y=Mathf.Lerp(y,2.46f+micro*.55f,upper);
            // Walkway rises from the civic terrace into the original Bastion stair.
            float route=1f-Smooth(1.15f,2f,Mathf.Abs(x));
            float segment=Smooth(-1f,.2f,z)*(1f-Smooth(5.8f,6.5f,z));
            y=Mathf.Lerp(y,Mathf.Lerp(.35f,2.35f,Smooth(-.3f,5.9f,z))-.13f,route*segment*.8f);
            return y;
        }
        static void BuildAuthoredCityMass()
        {
            // Camera-authored Hero-to-city transition: three inhabited shelves, not one exposed terrain slab.
            TerraceDeck("lower inhabited shelf",-10.8f,10.8f,-7.0f,-1.1f,.22f,.35f);
            TerraceDeck("middle civic shelf",-9.1f,9.1f,-1.4f,4.0f,1.18f,1.28f);
            TerraceDeck("upper hero shelf",-7.4f,7.4f,3.75f,7.25f,2.18f,2.28f);

            RetainingFront("lower retaining city wall",-10.8f,10.8f,-1.12f,-2.25f,.32f,8);
            RetainingFront("middle retaining city wall",-9.1f,9.1f,3.98f,.28f,1.30f,7);
            RetainingFront("upper retaining hero wall",-7.4f,7.4f,7.18f,1.20f,2.31f,6);

            // Continuous rock shoulders close the shelves into the mountain and conceal hard joins.
            RockShoulder("west mountain shoulder",-11.7f,-7.0f,-6.9f,7.2f);
            RockShoulder("east mountain shoulder",7.0f,11.7f,-6.9f,7.2f);
            RockShoulder("front mountain apron",-10.9f,10.9f,-9.0f,-6.75f);
        }
        static void TerraceDeck(string name,float left,float right,float front,float back,float y,float crown)
        {
            var v=new[]{
                new Vector3(left,y,front),new Vector3(right,y,front),
                new Vector3(left,crown,back),new Vector3(right,crown,back)
            };
            var uv=new[]{new Vector2(0,0),new Vector2((right-left)*.32f,0),
                         new Vector2(0,(back-front)*.32f),new Vector2((right-left)*.32f,(back-front)*.32f)};
            var m=new Mesh{name="Valoria v2 · "+name};
            m.vertices=v;m.uv=uv;m.triangles=new[]{0,2,1,1,2,3};m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();
            var go=new GameObject(m.name);go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=m;go.AddComponent<MeshRenderer>().sharedMaterial=ground;VisualPieces++;
        }
        static void RetainingFront(string name,float left,float right,float z,float bottom,float top,int bays)
        {
            var verts=new System.Collections.Generic.List<Vector3>();
            var uv=new System.Collections.Generic.List<Vector2>();
            var tris=new System.Collections.Generic.List<int>();
            float w=(right-left)/bays;
            void Quad(float x0,float y0,float x1,float y1,float dz)
            {
                int k=verts.Count;
                verts.Add(new Vector3(x0,y0,z+dz));verts.Add(new Vector3(x1,y0,z+dz));
                verts.Add(new Vector3(x0,y1,z+dz));verts.Add(new Vector3(x1,y1,z+dz));
                uv.Add(new Vector2(x0*.48f,y0*.48f));uv.Add(new Vector2(x1*.48f,y0*.48f));
                uv.Add(new Vector2(x0*.48f,y1*.48f));uv.Add(new Vector2(x1*.48f,y1*.48f));
                tris.Add(k);tris.Add(k+2);tris.Add(k+1);tris.Add(k+1);tris.Add(k+2);tris.Add(k+3);
            }
            for(int i=0;i<bays;i++)
            {
                float a=left+i*w,b=a+w, pier=w*.18f, spring=bottom+(top-bottom)*.58f;
                Quad(a,bottom,b,top,.04f);
                // Recessed arch rhythm makes the support read as occupied architecture, not a bare cliff.
                Quad(a,bottom,a+pier,top,-.11f);Quad(b-pier,bottom,b,top,-.11f);
                Quad(a+pier,spring,b-pier,top,-.11f);
            }
            var mesh=new Mesh{name="Valoria v2 · "+name,indexFormat=IndexFormat.UInt32};
            mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var obj=new GameObject(mesh.name);obj.transform.SetParent(root,true);
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=stone;VisualPieces++;
        }
        static void RockShoulder(string name,float left,float right,float front,float back)
        {
            const int rows=7, cols=7;
            var v=new Vector3[rows*cols];var uv=new Vector2[v.Length];var tris=new System.Collections.Generic.List<int>();
            for(int j=0;j<rows;j++)for(int i=0;i<cols;i++)
            {
                float tx=i/(float)(cols-1),tz=j/(float)(rows-1);
                float x=Mathf.Lerp(left,right,tx),z=Mathf.Lerp(front,back,tz);
                float edge=Mathf.Min(tx,1f-tx);
                float y=-2.35f+1.15f*tz+Mathf.Sin((x+z)*.73f)*.20f+edge*.55f;
                int k=j*cols+i;v[k]=new Vector3(x,y,z);uv[k]=new Vector2(x*.4f,z*.4f);
            }
            for(int j=0;j<rows-1;j++)for(int i=0;i<cols-1;i++)
            {int a=j*cols+i,b=a+1,c=a+cols,d=c+1;tris.Add(a);tris.Add(c);tris.Add(b);tris.Add(b);tris.Add(c);tris.Add(d);}
            var m=new Mesh{name="Valoria v2 · "+name};m.vertices=v;m.uv=uv;m.triangles=tris.ToArray();m.RecalculateNormals();m.RecalculateBounds();
            var go=new GameObject(m.name);go.transform.SetParent(root,true);go.AddComponent<MeshFilter>().sharedMesh=m;go.AddComponent<MeshRenderer>().sharedMaterial=rock;VisualPieces++;
        }
        static void RestoreFunctional(string name)
        {
            var go=GameObject.Find(name);
            if(go==null)throw new Exception("Functional source missing: "+name);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {if(!r.enabled){r.enabled=true;RestoredFunctionalRenderers++;}}
        }
        static void BuildTerracedMasonry()
        {
            // Two nested tiers are one joined masonry family, with the certified stair axis clear.
            // Recessed bays are structural rhythm rather than isolated freestanding kit fragments.
            Arcade("upper west retaining wall",-8.1f,-1.35f,4.45f,-.45f,2.18f,4);
            Arcade("upper east retaining wall",1.35f,8.1f,4.45f,-.45f,2.18f,4);
            Arcade("west civic foundation",-10.35f,-1.45f,-3.35f,-2.7f,.36f,5);
            Arcade("east civic foundation",1.45f,10.35f,-3.35f,-2.7f,.36f,5);
            // These authored transition pieces merge the upper wall ends into the Bastion's skirt.
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "west bastion rock-to-wall join",new Vector3(-6.6f,1.45f,5.1f),2.75f,2.15f,30f);
            Add("Valoria/StoneArchitectureKit_v1/RockToWallTransition",
                "east bastion rock-to-wall join",new Vector3(6.6f,1.45f,5.1f),2.75f,2.15f,205f);
        }
        static void Arcade(string name,float left,float right,float z,float bottom,float top,int bays)
        {
            var verts=new System.Collections.Generic.List<Vector3>();
            var uv=new System.Collections.Generic.List<Vector2>();
            var tris=new System.Collections.Generic.List<int>();
            void Quad(float x0,float y0,float x1,float y1,float face)
            {
                int k=verts.Count;
                verts.Add(new Vector3(x0,y0,face));verts.Add(new Vector3(x1,y0,face));
                verts.Add(new Vector3(x0,y1,face));verts.Add(new Vector3(x1,y1,face));
                uv.Add(new Vector2(x0*.55f,y0*.55f));uv.Add(new Vector2(x1*.55f,y0*.55f));
                uv.Add(new Vector2(x0*.55f,y1*.55f));uv.Add(new Vector2(x1*.55f,y1*.55f));
                tris.Add(k);tris.Add(k+2);tris.Add(k+1);tris.Add(k+1);tris.Add(k+2);tris.Add(k+3);
            }
            float width=(right-left)/bays;
            for(int i=0;i<bays;i++)
            {
                float a=left+i*width,b=a+width;
                float inset=width*.19f, spring=bottom+(top-bottom)*.52f;
                // The dark recess is behind the parapet; broad connected piers carry the tier.
                Quad(a,bottom,b,top,z+.19f);
                Quad(a,bottom,a+inset,top,z-.05f);
                Quad(b-inset,bottom,b,top,z-.05f);
                Quad(a+inset,spring,b-inset,top,z-.05f);
                Quad(a,top-.22f,b,top+.08f,z-.13f);
            }
            var mesh=new Mesh{name="Valoria v2 · "+name,indexFormat=IndexFormat.UInt32};
            mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var obj=new GameObject(mesh.name);obj.transform.SetParent(root,true);
            obj.AddComponent<MeshFilter>().sharedMesh=mesh;
            obj.AddComponent<MeshRenderer>().sharedMaterial=stone;
            VisualPieces++;
        }
        static void BuildEmbeddedDistrict()
        {
            // Architecture is embedded in the retaining sequence so each terrace reads as a district, not an empty pad.
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","lower west workshop row",new Vector3(-6.85f,.32f,-4.85f),2.35f,2.35f,18f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","lower east military row",new Vector3(6.55f,.32f,-4.55f),2.45f,2.45f,342f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece04","lower civic edge",new Vector3(2.85f,.34f,-5.35f),2.15f,2.20f,10f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece03","middle west housing",new Vector3(-6.15f,1.31f,.65f),2.35f,2.55f,14f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece01","middle east housing",new Vector3(6.05f,1.31f,.90f),2.20f,2.40f,346f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece02","upper west service",new Vector3(-5.25f,2.31f,4.95f),2.05f,2.35f,16f);
            Add("Valoria/MidTierArchitectureKit_v1/Piece04","upper east service",new Vector3(5.15f,2.31f,5.05f),2.05f,2.35f,344f);
            Add("Valoria/StoneArchitectureKit_v1/CornerWallL","west terrace corner",new Vector3(-8.05f,.35f,-1.15f),2.2f,1.85f,92f);
            Add("Valoria/StoneArchitectureKit_v1/CornerWallL","east terrace corner",new Vector3(8.05f,.35f,-1.15f),2.2f,1.85f,268f);
        }
        static void BuildRoute()
        {
            const int n=32;var v=new Vector3[n*2];var uv=new Vector2[n*2];var tris=new int[(n-1)*6];
            for(int i=0;i<n;i++)
            {
                float z=Mathf.Lerp(-5.4f,5.55f,i/(float)(n-1));
                float h=Height(0,z)+.055f;
                v[i*2]=new Vector3(-1.05f,h,z);v[i*2+1]=new Vector3(1.05f,h,z);
                uv[i*2]=new Vector2(-.45f,z*.42f);uv[i*2+1]=new Vector2(.45f,z*.42f);
            }
            int t=0;for(int i=0;i<n-1;i++)
            {int k=i*2;tris[t++]=k;tris[t++]=k+2;tris[t++]=k+1;tris[t++]=k+1;tris[t++]=k+2;tris[t++]=k+3;}
            var mesh=new Mesh{name="Valoria v2 · civic stone route"};mesh.vertices=v;mesh.uv=uv;mesh.triangles=tris;mesh.RecalculateNormals();
            var go=new GameObject(mesh.name);go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=stone;
            VisualPieces++;
        }
        static void Add(string path,string name,Vector3 point,float span,float height,float yaw)
        {
            var src=Resources.Load<GameObject>(path);if(src==null)throw new Exception("Composition asset missing: "+path);
            var go=Object.Instantiate(src);go.name="Valoria v2 · "+name;go.transform.rotation=Quaternion.Euler(0,yaw,0);
            var rs=go.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)throw new Exception("Asset renderer missing: "+path);
            var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.localScale*=Mathf.Min(span/Mathf.Max(b.size.x,b.size.z),height/b.size.y);
            rs=go.GetComponentsInChildren<Renderer>(true);b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=point-new Vector3(b.center.x,b.min.y,b.center.z);
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var behaviour in go.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            VisualPieces++;
        }
    }
}

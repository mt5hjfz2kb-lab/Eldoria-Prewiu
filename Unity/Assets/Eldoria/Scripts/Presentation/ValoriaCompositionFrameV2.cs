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
            BuildConnectedBedrock();
            RestoreFunctional("Aserradero · dedicated sawmill");
            RestoreFunctional("Cuartel · dedicated barracks");
            Add("Valoria/Valoria_Granero_BIII_v1","civic granary",new Vector3(-2.6f,.34f,-6.1f),3.3f,2.7f,16f);
            BuildTerracedMasonry();
            Add("Valoria/MidTierArchitectureKit_v1/Piece03","upper civic service",new Vector3(-5.15f,1.7f,5.8f),2.1f,2.55f,12f);
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
        static void BuildConnectedBedrock()
        {
            const int n=145;
            var vertices=new Vector3[n*n];var uvs=new Vector2[n*n];
            var tris=new System.Collections.Generic.List<int>();
            for(int j=0;j<n;j++)for(int i=0;i<n;i++)
            {
                float x=Mathf.Lerp(-13.8f,13.8f,i/(float)(n-1));
                // The visible perimeter is a fractured mountain edge, not a rectangular board.
                float front=-8.15f+.48f*Mathf.Sin(x*.63f)+.31f*Mathf.Sin(x*1.72f)
                    +1.3f*Mathf.Pow(Mathf.Abs(x)/13.8f,2f);
                float z=Mathf.Lerp(front,18.5f,j/(float)(n-1));
                int k=j*n+i;vertices[k]=new Vector3(x,Height(x,z),z);uvs[k]=new Vector2(x*.34f,z*.34f);
            }
            for(int j=0;j<n-1;j++)for(int i=0;i<n-1;i++)
            {
                int a=j*n+i,b=a+1,c=a+n,d=c+1;
                tris.Add(a);tris.Add(c);tris.Add(b);tris.Add(b);tris.Add(c);tris.Add(d);
            }
            var mesh=new Mesh{name="Valoria v2 · one connected city bedrock",indexFormat=IndexFormat.UInt32};
            mesh.vertices=vertices;mesh.uv=uvs;mesh.triangles=tris.ToArray();
            mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            var go=new GameObject(mesh.name);go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=ground;
            VisualPieces++;
            BuildCliffRim(vertices,n);
        }
        static void BuildCliffRim(Vector3[] surface,int n)
        {
            // The same top-edge vertices anchor the visible rock section. No independent slab seam.
            var v=new Vector3[n*2];var uv=new Vector2[v.Length];
            var tris=new System.Collections.Generic.List<int>();
            for(int i=0;i<n;i++)
            {
                var top=surface[i];
                float depth=-5.0f-.63f*Mathf.Sin(i*.37f)-.38f*Mathf.Sin(i*.93f);
                v[2*i]=top;v[2*i+1]=new Vector3(top.x,depth,top.z-.7f);
                uv[2*i]=new Vector2(top.x*.42f,top.y*.42f);
                uv[2*i+1]=new Vector2(top.x*.42f,depth*.42f);
                if(i==0)continue;
                int k=2*i;
                tris.Add(k-2);tris.Add(k);tris.Add(k-1);
                tris.Add(k);tris.Add(k+1);tris.Add(k-1);
            }
            var m=new Mesh{name="Valoria v2 · integrated foreground mountain section",indexFormat=IndexFormat.UInt32};
            m.vertices=v;m.uv=uv;m.triangles=tris.ToArray();m.RecalculateNormals();m.RecalculateBounds();
            var go=new GameObject(m.name);go.transform.SetParent(root,true);
            go.AddComponent<MeshFilter>().sharedMesh=m;go.AddComponent<MeshRenderer>().sharedMaterial=rock;
            VisualPieces++;
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

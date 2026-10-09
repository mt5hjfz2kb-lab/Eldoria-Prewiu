using UnityEngine;

namespace Eldoria.Presentation
{
    // Visual-only World Region 1 surface. No colliders, state, camera or 4X topology.
    public static class WorldRegion1SurfaceV2
    {
        const float Width=108f,Depth=94f;
        static Material material;

        static float Blob(float x,float z,float cx,float cz,float r)
        {
            float dx=(x-cx)/r,dz=(z-cz)/r;
            return Mathf.Exp(-1.6f*(dx*dx+dz*dz));
        }
        static float Ridge(float x,float z)
        {
            return Mathf.Max(
                Mathf.Max(Blob(x,z,-22f,11f,8f),Blob(x,z,-16f,23f,8f)),
                Mathf.Max(Blob(x,z,21f,16f,8f),Blob(x,z,7f,27f,8f)));
        }
        static float Forest(float x,float z)
        {
            return Mathf.Max(
                Mathf.Max(Blob(x,z,-13f,7f,7f),Blob(x,z,-11f,17f,7f)),
                Mathf.Max(Blob(x,z,-1f,20f,7f),
                    Mathf.Max(Blob(x,z,16f,7f,7f),Blob(x,z,-5.5f,4.6f,6f))));
        }
        public static float Height(float x,float z)
        {
            float rolling=(Mathf.PerlinNoise(x*.052f+12f,z*.052f+9f)-.5f)*.85f;
            float fine=(Mathf.PerlinNoise(x*.16f+43f,z*.16f+23f)-.5f)*.20f;
            float outer=Mathf.SmoothStep(.25f,.98f,
                Mathf.Max(Mathf.Abs(x)/54f,Mathf.Abs(z-4f)/47f))*.23f;
            float h=-.24f+rolling+fine+Mathf.Sin(x*.15f+z*.07f)*.11f+
                Ridge(x,z)*1.05f+outer;
            float flat=Mathf.Max(Blob(x,z,0f,-5.4f,6f),
                Mathf.Max(Blob(x,z,-5.5f,4.6f,3f),
                Mathf.Max(Blob(x,z,4.5f,4f,3f),
                Mathf.Max(Blob(x,z,6.8f,8.2f,2.5f),Blob(x,z,6.7f,-.5f,3f)))));
            return Mathf.Lerp(h,-.055f,flat*.92f);
        }
        public static Vector3 Grounded(Vector3 p,float lift)
        {
            p.y=Height(p.x,p.z)+lift;
            return p;
        }
        static float Segment(Vector2 p,Vector2 a,Vector2 b)
        {
            Vector2 d=b-a;
            float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.001f,d.sqrMagnitude));
            return Vector2.Distance(p,a+d*t);
        }
        static float Road(float x,float z)
        {
            var p=new Vector2(x,z);
            float d=Segment(p,new Vector2(0,-7),new Vector2(0,1.75f));
            d=Mathf.Min(d,Segment(p,new Vector2(-.5f,-1),new Vector2(-5.5f,5.4f)));
            d=Mathf.Min(d,Segment(p,new Vector2(.55f,-.8f),new Vector2(4.9f,4.8f)));
            d=Mathf.Min(d,Segment(p,new Vector2(5.2f,4.8f),new Vector2(8.2f,10.2f)));
            float erosion=(Mathf.PerlinNoise(x*.55f+33,z*.55f+21)-.5f)*.45f;
            return 1f-Mathf.SmoothStep(.55f,2.5f,d+erosion);
        }
        static Material Surface()
        {
            if(material!=null)return material;
            const int size=640;
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true)
            {name="World Region 1 biome and route blend",wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[size*size];
            var soil=new Color(.31f,.28f,.225f);
            var meadow=new Color(.22f,.28f,.20f);
            var woods=new Color(.16f,.225f,.165f);
            var rock=new Color(.37f,.37f,.33f);
            var track=new Color(.27f,.205f,.145f);
            for(int y=0;y<size;y++)
            for(int x=0;x<size;x++)
            {
                float wx=((x+.5f)/size-.5f)*Width;
                float wz=((y+.5f)/size-.5f)*Depth+4f;
                float n=Mathf.PerlinNoise(wx*.095f+19,wz*.095f+31);
                float detail=Mathf.PerlinNoise(wx*.38f+3,wz*.38f+14);
                float grain=Mathf.PerlinNoise(wx*1.4f+47,wz*1.4f+23);
                var col=Color.Lerp(soil,meadow,Mathf.SmoothStep(.25f,.75f,n)*.82f);
                col=Color.Lerp(col,woods,Forest(wx,wz)*.80f);
                col=Color.Lerp(col,rock,Ridge(wx,wz)*.80f);
                col=Color.Lerp(col,new Color(.36f,.33f,.26f),Blob(wx,wz,0,-5.4f,8f)*.5f);
                col=Color.Lerp(col,track,Road(wx,wz)*.91f);
                col=Color.Lerp(col,new Color(.23f,.17f,.24f),
                    Blob(wx,wz,6.8f,8.2f,3.4f)*.45f);
                pixels[y*size+x]=col*(.83f+detail*.20f+grain*.09f);
            }
            texture.SetPixels(pixels);texture.Apply(true,false);
            var shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null)shader=Shader.Find("Standard");
            material=new Material(shader){name="World Region 1 continuous terrain v2"};
            material.SetTexture("_BaseMap",texture);
            material.SetColor("_BaseColor",Color.white);
            material.SetFloat("_Smoothness",.012f);
            return material;
        }
        public static void Build(Transform parent)
        {
            const int xs=72,zs=64;
            var verts=new Vector3[(xs+1)*(zs+1)];
            var uv=new Vector2[verts.Length];
            var tris=new int[xs*zs*6];
            int v=0;
            for(int z=0;z<=zs;z++)
            for(int x=0;x<=xs;x++)
            {
                float nx=x/(float)xs,nz=z/(float)zs;
                float wx=(nx-.5f)*Width,wz=(nz-.5f)*Depth+4f;
                verts[v]=new Vector3(wx,Height(wx,wz),wz);
                uv[v]=new Vector2(nx,nz);v++;
            }
            int t=0;
            for(int z=0;z<zs;z++)
            for(int x=0;x<xs;x++)
            {
                int a=z*(xs+1)+x,b=a+1,c=a+xs+1,d=c+1;
                tris[t++]=a;tris[t++]=c;tris[t++]=b;
                tris[t++]=b;tris[t++]=c;tris[t++]=d;
            }
            var go=new GameObject("World Region 1 · terrain base");
            go.transform.SetParent(parent,true);
            var mesh=new Mesh{name="World Region 1 relief v2",vertices=verts,uv=uv,triangles=tris};
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=Surface();
        }
    }
}

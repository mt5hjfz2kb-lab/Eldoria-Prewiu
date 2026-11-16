// Ashen Citadel: independent, reproducible architectural finishing meshes.
// City meshes only. No gameplay, production assets, extra colliders or clicks.
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.ExperimentalAshenCitadel.Editor
{
    internal static class AshenArchitecturalFinishing
    {
        private const string AssetRoot="Assets/Eldoria/ExperimentalAshenCitadel/Meshes/";
        private static readonly string[] Keys={"oak","wood","brass","stoneLight","cut","banner","iron","roof"};
        private sealed class Shape
        {
            internal readonly List<Vector3> vertices=new List<Vector3>(3500);
            internal readonly List<Vector2> uv=new List<Vector2>(3500);
            internal readonly List<int>[] indices=new List<int>[Keys.Length];
            internal Shape(){for(int i=0;i<indices.Length;i++)indices[i]=new List<int>();}
            internal void Tri(int m,Vector3 a,Vector3 b,Vector3 c)
            {
                int offset=vertices.Count;
                vertices.Add(a);vertices.Add(b);vertices.Add(c);
                uv.Add(new Vector2(a.x*.20f,a.y*.20f));
                uv.Add(new Vector2(b.x*.20f,b.y*.20f));
                uv.Add(new Vector2(c.x*.20f,c.y*.20f));
                indices[m].Add(offset);indices[m].Add(offset+1);indices[m].Add(offset+2);
            }
            internal void Quad(int m,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
            {Tri(m,a,b,c);Tri(m,a,c,d);}
            internal void Box(int m,Vector3 p,Vector3 r)
            {
                float x=p.x,y=p.y,z=p.z;
                var v=new[]{
                    new Vector3(x-r.x,y-r.y,z-r.z),new Vector3(x+r.x,y-r.y,z-r.z),
                    new Vector3(x+r.x,y-r.y,z+r.z),new Vector3(x-r.x,y-r.y,z+r.z),
                    new Vector3(x-r.x,y+r.y,z-r.z),new Vector3(x+r.x,y+r.y,z-r.z),
                    new Vector3(x+r.x,y+r.y,z+r.z),new Vector3(x-r.x,y+r.y,z+r.z)
                };
                Quad(m,v[0],v[4],v[5],v[1]);Quad(m,v[1],v[5],v[6],v[2]);
                Quad(m,v[2],v[6],v[7],v[3]);Quad(m,v[3],v[7],v[4],v[0]);
                Quad(m,v[4],v[7],v[6],v[5]);Quad(m,v[3],v[0],v[1],v[2]);
            }
            internal void Beam(int m,Vector3 a,Vector3 b,float radius)
            {
                var d=(b-a).normalized;
                var h=Vector3.Cross(d,Vector3.up);
                if(h.sqrMagnitude<.01f)h=Vector3.Cross(d,Vector3.forward);
                h.Normalize();var up=Vector3.Cross(d,h).normalized;
                var aa=new Vector3[4];var bb=new Vector3[4];
                for(int i=0;i<4;i++){
                    float angle=(i+.5f)*Mathf.PI*.5f;
                    Vector3 q=radius*(Mathf.Cos(angle)*h+Mathf.Sin(angle)*up);
                    aa[i]=a+q;bb[i]=b+q;
                }
                for(int i=0;i<4;i++){
                    int j=(i+1)%4;
                    Quad(m,aa[i],aa[j],bb[j],bb[i]);
                }
                Quad(m,aa[3],aa[2],aa[1],aa[0]);
                Quad(m,bb[0],bb[1],bb[2],bb[3]);
            }
            internal void Arch(int m,float x,float y,float z,float width,float rise)
            {
                var last=new Vector3(x-width*.5f,y,z);
                for(int i=1;i<=12;i++){
                    float a=Mathf.PI-i*Mathf.PI/12f;
                    var next=new Vector3(x+Mathf.Cos(a)*width*.5f,y+Mathf.Sin(a)*rise,z);
                    Beam(m,last,next,.085f);last=next;
                }
            }
            internal void Medallion(int mat,Vector3 c,float r)
            {
                for(int i=0;i<12;i++){
                    float a=2*Mathf.PI*i/12f,b=2*Mathf.PI*(i+1)/12f;
                    Tri(mat,c,c+new Vector3(r*Mathf.Cos(b),r*Mathf.Sin(b),0),
                        c+new Vector3(r*Mathf.Cos(a),r*Mathf.Sin(a),0));
                }
            }
            internal void Cloth(float x,float y,float z,float w,float h)
            {
                Vector3 a=new Vector3(x-w*.5f,y,z),b=new Vector3(x+w*.5f,y,z),
                    c=new Vector3(x+w*.42f,y-h*.8f,z+.05f),
                    d=new Vector3(x,y-h*.92f,z+.12f),
                    e=new Vector3(x-w*.43f,y-h,z+.05f);
                Tri(5,a,b,d);Tri(5,b,c,d);Tri(5,a,d,e);
                Beam(2,new Vector3(x-w*.58f,y+.06f,z),
                    new Vector3(x+w*.58f,y+.06f,z),.052f);
            }
            internal Mesh Save(string name)
            {
                var mesh=new Mesh{name="Ashen character construction · "+name};
                mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(vertices);
                mesh.SetUVs(0,uv);mesh.subMeshCount=indices.Length;
                for(int i=0;i<indices.Length;i++)mesh.SetTriangles(indices[i],i,false);
                mesh.RecalculateNormals();mesh.RecalculateBounds();
                string path=AssetRoot+"AshenFinish_"+name+".asset";
                var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(old==null){AssetDatabase.CreateAsset(mesh,path);return mesh;}
                EditorUtility.CopySerialized(mesh,old);
                UnityEngine.Object.DestroyImmediate(mesh);
                EditorUtility.SetDirty(old);return old;
            }
        }
        public static void Add(GameObject root,string family,int level,
            IReadOnlyDictionary<string,Material> colours)
        {
            var g=new Shape();
            switch(family){
                case "Bastion":Bastion(g,level);break;
                case "Sawmill":Sawmill(g);break;
                case "Barracks":Barracks(g);break;
                case "Granary":Granary(g);break;
                case "Stoneworks":Stoneworks(g);break;
                default:throw new ArgumentOutOfRangeException(nameof(family));
            }
            if(g.vertices.Count==0)return;
            string name=family+(family=="Bastion"?"_"+level:"");
            var obj=new GameObject("Ashen · connected artisan/fortress character · "+name);
            obj.transform.SetParent(root.transform,false);
            obj.AddComponent<MeshFilter>().sharedMesh=g.Save(name);
            var renderer=obj.AddComponent<MeshRenderer>();
            var mats=new Material[Keys.Length];
            for(int i=0;i<Keys.Length;i++)
                if(!colours.TryGetValue(Keys[i],out mats[i])||mats[i]==null)
                    throw new InvalidOperationException("Missing authored Ashen material "+Keys[i]);
            renderer.sharedMaterials=mats;
            renderer.receiveShadows=true;renderer.shadowCastingMode=ShadowCastingMode.On;
            // Click boxes and gameplay logic stay on the original imported roots.
            Debug.Log("ASHEN_FINISH_ASSET "+name+" triangles="+g.vertices.Count/3);
        }
        private static void Bastion(Shape g,int level)
        {
            if(level==1)return;
            float front=level>=4?-7f:-6.05f;
            if(level>=3){
                // Gate: carved profile + suspended burgundy cloth, integrated
                // into the real wall and not glued to a freestanding HUD-like box.
                g.Arch(3,0f,2.32f,front-1.15f,2.7f,.73f);
                for(int sign=-1;sign<=1;sign+=2){
                    float x=sign*(level>=4?2.2f:1.65f);
                    g.Beam(3,new Vector3(x,.75f,front-.72f),
                        new Vector3(x-sign*.18f,2.77f,front-.38f),.18f);
                }
                g.Cloth(-2.38f,level>=4?5.42f:4.42f,front-.68f,.63f,1.02f);
                g.Cloth(2.42f,level>=4?5.42f:4.42f,front-.68f,.63f,1.02f);
                // One continuous occupied bridge connects ancestral tower
                // and the main hall, reducing the 'separate blocks' look.
                g.Box(0,new Vector3(-.32f,3.13f,1.18f),new Vector3(1.59f,.11f,.65f));
                for(int i=0;i<5;i++){
                    float x=-1.75f+i*.71f;
                    g.Beam(1,new Vector3(x,3.23f,.6f),new Vector3(x,4.08f,.6f),.069f);
                    g.Beam(1,new Vector3(x,3.23f,1.76f),new Vector3(x,4.08f,1.76f),.069f);
                    if(i%2==0)g.Beam(0,new Vector3(x,2.72f,.6f),
                        new Vector3(x,3.23f,1.76f),.11f);
                }
                g.Beam(1,new Vector3(-1.76f,4.08f,.6f),new Vector3(1.09f,4.08f,.6f),.086f);
                g.Beam(1,new Vector3(-1.76f,4.08f,1.76f),new Vector3(1.09f,4.08f,1.76f),.086f);
            }
            if(level>=4){
                // Defended catwalk, constructed from the same wall and oak
                // language as the keep, with purposeful braces instead of props.
                g.Box(0,new Vector3(6.44f,3.60f,.65f),new Vector3(.72f,.09f,3.05f));
                for(int i=0;i<6;i++){
                    float z=-1.97f+i*1.07f;
                    g.Beam(0,new Vector3(6.36f,2.65f,z),
                        new Vector3(6.14f,3.60f,z),.107f);
                    g.Beam(0,new Vector3(5.76f,3.72f,z),
                        new Vector3(5.76f,4.39f,z),.073f);
                }
                g.Beam(1,new Vector3(5.76f,4.40f,-2.0f),
                    new Vector3(5.76f,4.40f,3.36f),.09f);
            }
            // No disconnected roof finial: the first native visual review
            // revealed the prototype ornament apparently floating above the
            // distant roof. Keep the historic tower roof itself as the accent.
        }
        private static void Sawmill(Shape g)
        {
            for(int sign=-1;sign<=1;sign+=2){
                float x=sign*1.95f;
                g.Beam(0,new Vector3(x,1.08f,-2.54f),
                    new Vector3(x,2.53f,-2.54f),.145f);
                g.Beam(1,new Vector3(x,2.53f,-2.54f),
                    new Vector3(x*.60f,3.10f,-1.75f),.112f);
            }
            g.Beam(1,new Vector3(-1.97f,2.53f,-2.57f),
                new Vector3(1.97f,2.53f,-2.57f),.18f);
            g.Beam(6,new Vector3(.14f,2.47f,-2.62f),
                new Vector3(.14f,1.58f,-2.62f),.04f);
            g.Medallion(6,new Vector3(.14f,1.48f,-2.67f),.23f);
            for(int i=0;i<4;i++){
                float x=-2.15f+i*.52f;
                g.Beam(0,new Vector3(x,.23f,-3.22f),
                    new Vector3(x+.10f,.25f,-1.74f),.19f);
            }
            g.Cloth(-2.15f,2.44f,-2.67f,.48f,.64f);
        }
        private static void Barracks(Shape g)
        {
            // Integrate porch joinery BELOW the existing roof eave; earlier
            // V2 diagonal gable beams crossed the slate roof in real Unity.
            g.Beam(1,new Vector3(-2.38f,2.34f,-2.49f),
                new Vector3(2.38f,2.34f,-2.49f),.11f);
            g.Cloth(-2.60f,2.48f,-2.66f,.42f,.75f);
            g.Cloth(2.60f,2.48f,-2.66f,.42f,.75f);
            g.Box(4,new Vector3(-1.8f,4.40f,1.46f),new Vector3(.32f,.59f,.33f));
            g.Box(3,new Vector3(-1.8f,5.0f,1.46f),new Vector3(.42f,.12f,.44f));
        }
        private static void Granary(Shape g)
        {
            // Unity V2 revealed new thin vertical staves detached from the
            // silo and protruding through the granary roof. Retain the
            // underlying sculpted rounded silo and strengthen only its
            // already-authored, physically joined transfer beam.
            g.Beam(0,new Vector3(-.58f,2.27f,.52f),
                new Vector3(1.09f,3.10f,.52f),.17f);
            for(int i=0;i<4;i++){
                float x=-2.5f+i*.67f;
                g.Beam(0,new Vector3(x,.24f,-3.0f),
                    new Vector3(x,.69f,-3.0f),.13f);
            }
            g.Cloth(-.96f,3.15f,-1.85f,.51f,.78f);
        }
        private static void Stoneworks(Shape g)
        {
            // Shared hoist frame and spoked winch wheel. Larger purpose-specific
            // silhouette, not more random stone scatter or a second roof.
            // Existing canvas and mast already have their own continuous
            // frame. Omit the additional V2 diagonal beams that crossed the
            // cloth like unfinished construction scaffolding.
            var centre=new Vector3(.72f,1.63f,.62f);
            for(int i=0;i<12;i++){
                float a=2*Mathf.PI*i/12f,b=2*Mathf.PI*(i+1)/12f;
                g.Beam(0,centre+new Vector3(.54f*Mathf.Cos(a),.54f*Mathf.Sin(a),0),
                    centre+new Vector3(.54f*Mathf.Cos(b),.54f*Mathf.Sin(b),0),.069f);
                if(i%3==0)g.Beam(1,centre,
                    centre+new Vector3(.54f*Mathf.Cos(a),.54f*Mathf.Sin(a),0),.068f);
            }
            g.Medallion(2,centre+Vector3.back*.09f,.14f);
            for(int i=0;i<5;i++){
                float x=-2.23f+i*.60f;
                g.Box(i%3==0?3:4,new Vector3(x,.20f,-2.43f),
                    new Vector3(.26f,.19f,.38f));
            }
        }
    }
}

using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Reversible connective vocabulary for the fixed Flat Citadel. Everything is visual-only;
    // source architecture, gameplay roads/colliders and parcel capacities remain authoritative.
    public static class ValoriaUnifiedVisualLanguageV1
    {
        public const string RootName="Valoria · unified visual language proof v1";
        public static int ConnectorCount,LandscapeSections;
        static Material paving,earth,field,stone;

        public static void Apply(Transform canonicalRoot,PlayerState state)
        {
            if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
            ConnectorCount=LandscapeSections=0;
            var shader=Shader.Find("Eldoria/Valoria Coherence");
            if(shader==null)throw new InvalidOperationException("Valoria PBR response missing");
            paving=Surface(shader,"cobble",7,.80f);earth=Surface(shader,"dirt",6,.72f);
            field=Surface(shader,"dirt",6,.61f);stone=Surface(shader,"stone",5,.72f);
            if(paving==null||earth==null||stone==null)throw new InvalidOperationException("CC0 source maps missing");

            // One civic material and proportion rule for the existing gate, public court,
            // work-yard approaches and Hero access. Empty future plots remain soil.
            Ribbon(root,"central public street",
                new[]{new Vector3(0,.026f,-5.45f),new Vector3(0,.035f,-2.8f),new Vector3(0,.044f,-.50f),new Vector3(0,.055f,1.9f),new Vector3(0,.067f,3.50f)},
                new[]{1.86f,1.88f,2.15f,2.40f,2.55f},paving,.86f);ConnectorCount++;
            Ribbon(root,"west work-yard walk",
                new[]{new Vector3(-.95f,.043f,-1.55f),new Vector3(-2.4f,.044f,-1.45f),new Vector3(-3.45f,.045f,-1.48f)},
                new[]{1.06f,1.06f,.87f},paving,.78f);ConnectorCount++;
            Ribbon(root,"east training-yard walk",
                new[]{new Vector3(.95f,.043f,-1.60f),new Vector3(2.4f,.044f,-1.52f),new Vector3(3.45f,.045f,-1.50f)},
                new[]{1.04f,1.04f,.87f},paving,.78f);ConnectorCount++;
            Ribbon(root,"west Hero access return",
                new[]{new Vector3(-1.55f,.062f,2.55f),new Vector3(-2.40f,.075f,3.45f),new Vector3(-3.35f,.09f,4.05f)},
                new[]{.80f,.78f,.45f},stone,.74f);ConnectorCount++;
            Ribbon(root,"east Hero access return",
                new[]{new Vector3(1.55f,.062f,2.55f),new Vector3(2.40f,.075f,3.45f),new Vector3(3.35f,.09f,4.05f)},
                new[]{.80f,.78f,.45f},stone,.74f);ConnectorCount++;
            // These are strictly temporary ground treatments, not construction or
            // occupancy of R4/R5/R6. The full maximum building envelopes stay clear.
            Patch(root,"R4 future quarry compacted earth",new Vector3(-6.25f,.028f,3.20f),4.80f,3.95f,earth,.48f);LandscapeSections++;
            Patch(root,"R5 future forge compacted earth",new Vector3(6.25f,.028f,3.15f),4.55f,3.86f,earth,.48f);LandscapeSections++;
            Patch(root,"R6 future hospital compacted earth",new Vector3(2.70f,.027f,-4.35f),4.10f,3.55f,earth,.48f);LandscapeSections++;
            foreach(var c in root.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static Material Surface(Shader shader,string prefix,int family,float value)
        {
            var tex=Resources.Load<Texture2D>("Valoria/SurfaceCellExternal/"+prefix+"_diff");
            if(tex==null)return null;
            var m=new Material(shader){name="Unified source-rich "+prefix};m.SetTexture("_BaseMap",tex);
            m.SetFloat("_Family",family);m.SetFloat("_Ground",family==5?0:1);
            m.SetFloat("_BumpScale",0);m.SetFloat("_Smoothness",.055f);
            m.SetColor("_BaseColor",new Color(value,value,value,1));return m;
        }

        static void Patch(Transform parent,string name,Vector3 centre,float width,float depth,Material material,float density)
        {
            var pts=new[]{centre+new Vector3(0,0,-depth*.5f),centre+new Vector3(0,0,depth*.5f)};
            Ribbon(parent,name,pts,new[]{width,width},material,density);
        }

        static void Ribbon(Transform parent,string name,Vector3[] points,float[] widths,Material material,float uvDensity)
        {
            if(points.Length!=widths.Length||points.Length<2)throw new ArgumentException("Ribbon point/width mismatch");
            var vertices=new Vector3[points.Length*2];var uv=new Vector2[vertices.Length];
            var indices=new int[(points.Length-1)*6];var colors=new Color[vertices.Length];float length=0;
            for(int i=0;i<points.Length;i++)
            {
                var dir=(points[Math.Min(points.Length-1,i+1)]-points[Math.Max(0,i-1)]);
                var normal=new Vector3(-dir.z,0,dir.x).normalized;
                vertices[i*2]=points[i]-normal*widths[i]*.5f;
                vertices[i*2+1]=points[i]+normal*widths[i]*.5f;
                if(i>0)length+=Vector3.Distance(points[i],points[i-1]);
                uv[i*2]=new Vector2(0,length*uvDensity);uv[i*2+1]=new Vector2(widths[i]*uvDensity,length*uvDensity);
                colors[i*2]=colors[i*2+1]=Color.white;
            }
            for(int i=0;i<points.Length-1;i++)
            {
                int b=i*2,k=i*6;
                indices[k]=b;indices[k+1]=b+1;indices[k+2]=b+2;
                indices[k+3]=b+1;indices[k+4]=b+3;indices[k+5]=b+2;
            }
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.uv=uv;mesh.colors=colors;mesh.triangles=indices;
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject("Valoria · "+name);go.transform.SetParent(parent,false);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
    }
}

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

            // One continuous approach supplies an outside-world destination, not another island.
            Ribbon(root,"gate road · connected external approach",new[]{new Vector3(0,-.018f,-6.65f),new Vector3(-.4f,-.017f,-10f),new Vector3(-1.4f,-.012f,-15.5f),new Vector3(-3.1f,-.01f,-22f),new Vector3(-5.4f,-.008f,-30f)},new[]{2.45f,2.35f,2.20f,2.05f,1.85f},earth,0.65f);
            ConnectorCount++;
            // The city centre shares its material rhythm with the Bastion approach and gate.
            Ribbon(root,"processional stone apron",new[]{new Vector3(0,.035f,-.5f),new Vector3(0,.041f,1.25f),new Vector3(0,.055f,2.90f),new Vector3(0,.09f,3.95f)},new[]{1.92f,2.02f,2.18f,2.32f},paving,.95f);
            ConnectorCount++;
            // Side buttress footprints sit at the access seam; central stair and C0 remain open.
            Ribbon(root,"west architectural transition",new[]{new Vector3(-3.75f,.045f,3.75f),new Vector3(-3.30f,.06f,4.25f),new Vector3(-2.60f,.075f,4.68f)},new[]{.75f,.64f,.35f},stone,.7f);
            Ribbon(root,"east architectural transition",new[]{new Vector3(3.75f,.045f,3.75f),new Vector3(3.30f,.06f,4.25f),new Vector3(2.60f,.075f,4.68f)},new[]{.75f,.64f,.35f},stone,.7f);
            ConnectorCount+=2;

            // Crop territory has bounded low relief, disconnected from all growth corridors and parcels.
            // Fields use one source material grammar; no random surface carpet or high silhouette.
            for(int side=-1;side<=1;side+=2)
            {
                for(int row=0;row<4;row++)
                {
                    float z=-10.1f-row*4.15f;
                    Ribbon(root,"outer cultivated strip "+side+"/"+row,
                        new[]{new Vector3(side*(11.9f+.22f*row),-.032f,z),new Vector3(side*(16.4f+.35f*row),-.026f,z-.42f),new Vector3(side*(21.4f+.36f*row),-.020f,z-.1f)},
                        new[]{1.22f,1.55f,1.30f},row%2==0?field:earth,.67f);
                    LandscapeSections++;
                }
            }
            // A service track visually links the lumber working yard to the exterior but ends
            // well before the reserved XW corridor at z +3.8.
            Ribbon(root,"sawmill service spur",new[]{new Vector3(-7.7f,.025f,-1.6f),new Vector3(-9.4f,.012f,-2.8f),new Vector3(-11.2f,-.008f,-4.55f),new Vector3(-14.0f,-.023f,-7.4f)},new[]{.78f,.86f,1.02f,.90f},earth,.64f);
            ConnectorCount++;
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

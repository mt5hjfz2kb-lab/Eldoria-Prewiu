using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Reversible environment-art proof. All geometry belongs to one root; no gameplay authority.
    public static class ValoriaAuthoredGameplayCellV1
    {
        public const string RootName = "Valoria · Authored Gameplay Cell v1";
        public static int Pavers, Masonry, WorkProps;
        static readonly Color WarmStone = new Color(.59f,.56f,.51f,1);
        static readonly Color DarkStone = new Color(.45f,.44f,.41f,1);
        static readonly Color Timber = new Color(.34f,.26f,.20f,1);

        public static void Apply(Transform canonicalRoot, PlayerState state)
        {
            if (canonicalRoot == null) throw new ArgumentNullException(nameof(canonicalRoot));
            var previous = GameObject.Find(RootName);
            if (previous != null) Object.DestroyImmediate(previous);
            var root = new GameObject(RootName).transform;
            root.SetParent(canonicalRoot, true);
            Pavers = Masonry = WorkProps = 0;
            var cobble = ValoriaKit.DetailedSurfaceMaterial(WarmStone,"cobble",new Vector2(1.25f,1.25f),.93f);
            var edge = ValoriaKit.DetailedSurfaceMaterial(DarkStone,"stone",new Vector2(1.5f,1.5f),.96f);
            var wood = ValoriaKit.DetailedSurfaceMaterial(Timber,"wood",new Vector2(1.4f,1.4f),.94f);

            // One authored vocabulary: a worn processional core, small masonry returns at the
            // Hero's foot and a working apron at the west building. Preserve C0/C1, plot envelopes.
            Pave(root,"gate to plaza",-.70f,-2.45f,1.40f,3.10f,cobble,edge,17,4);
            Pave(root,"plaza",-2.62f,.62f,5.24f,2.50f,cobble,edge,15,7);
            Pave(root,"Bastion threshold",-1.90f,3.05f,3.80f,.82f,cobble,edge,14,2);
            Pave(root,"Aserradero apron",-4.85f,-.72f,2.35f,1.32f,cobble,edge,9,4);
            // Uneven, low retaining courses frame rather than encase the source Hero rock.
            for(int side=-1;side<=1;side+=2)
            {
                Course(root,"hero return",side*2.05f,3.35f,1.30f,edge,side);
                Course(root,"plaza seat",side*2.72f,1.86f,.95f,edge,side);
            }
            // Functional timber uses the same restrained material family; all props lie inside F1.
            for(int i=0;i<4;i++)
            {
                var p=new Vector3(-7.75f+i*.25f,.24f,-2.63f+i*.12f);
                Box(root,"sawmill stacked lumber",p,new Vector3(1.20f,.15f,.12f),wood,true);
                WorkProps++;
            }
            Box(root,"sawmill trestle",new Vector3(-7.42f,.18f,-2.75f),new Vector3(1.28f,.13f,.40f),wood,true);
            WorkProps++;
            foreach(var c in root.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static void Pave(Transform root,string role,float x,float z,float width,float depth,Material stone,Material edging,int columns,int rows)
        {
            // Staggered individual stones and a narrow perimeter course read as construction,
            // not a flat tinted overlay. Deterministic wear is encoded in the geometry itself.
            float dx=width/columns,dz=depth/rows;
            var vertices=new List<Vector3>();var indices=new List<int>();var uv=new List<Vector2>();
            for(int row=0;row<rows;row++)for(int col=0;col<columns;col++)
            {
                float offset=(row%2)*dx*.48f;
                float cx=x+(col+.5f)*dx+offset;
                if(cx>x+width-dx*.25f)continue;
                float cz=z+(row+.5f)*dz;
                int hash=(col*17+row*31)%11;
                float inset=.025f+hash*.002f;
                float y=.182f+hash*.001f,rx=(dx-inset)*.5f,rz=(dz-.033f)*.5f;
                int start=vertices.Count;
                vertices.Add(new Vector3(cx-rx,y,cz-rz));vertices.Add(new Vector3(cx-rx,y,cz+rz));
                vertices.Add(new Vector3(cx+rx,y,cz+rz));vertices.Add(new Vector3(cx+rx,y,cz-rz));
                indices.Add(start);indices.Add(start+1);indices.Add(start+2);
                indices.Add(start);indices.Add(start+2);indices.Add(start+3);
                uv.Add(new Vector2(0,0));uv.Add(new Vector2(0,1));uv.Add(new Vector2(1,1));uv.Add(new Vector2(1,0));
                Pavers++;
            }
            var mesh=new Mesh{name="Valoria dressed paving · "+role};
            mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();
            var paving=new GameObject("Valoria · authored cell · "+role+" modular paving");
            paving.transform.SetParent(root,true);paving.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=paving.AddComponent<MeshRenderer>();renderer.sharedMaterial=stone;
            renderer.shadowCastingMode=ShadowCastingMode.Off;
            // Long edges are modular replaceable masonry and remain within the path footprint.
            Box(root,role+" west curb",new Vector3(x+.035f,.165f,z+depth*.5f),new Vector3(.07f,.12f,depth),edging,false);
            Box(root,role+" east curb",new Vector3(x+width-.035f,.165f,z+depth*.5f),new Vector3(.07f,.12f,depth),edging,false);
        }

        static void Course(Transform root,string role,float x,float z,float length,Material stone,int side)
        {
            for(int i=0;i<4;i++)
            {
                float step=length/4f;
                float h=.18f+(i%2)*.05f;
                Box(root,role+" buttressed course",new Vector3(x+side*.12f,.16f+h*.5f,z+i*step),
                    new Vector3(.28f,h,step-.035f),stone,true);
                Masonry++;
            }
        }

        static void Box(Transform root,string role,Vector3 position,Vector3 size,Material material,bool shadows)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="Valoria · authored cell · "+role;
            go.transform.SetParent(root,true);
            go.transform.position=position;
            go.transform.localScale=size;
            var r=go.GetComponent<Renderer>();r.sharedMaterial=material;
            r.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }
    }
}

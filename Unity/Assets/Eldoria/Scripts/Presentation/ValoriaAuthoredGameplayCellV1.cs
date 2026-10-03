using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Reversible construction-site storytelling. No permanent occupation of future parcels.
    public static class ValoriaAuthoredGameplayCellV1
    {
        public const string RootName="Valoria · Authored Gameplay Cell v1";
        public static int Pavers,Masonry,WorkProps;
        public static void Apply(Transform canonicalRoot,PlayerState state)
        {
            if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
            var previous=GameObject.Find(RootName);
            if(previous!=null)Object.DestroyImmediate(previous);
            var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);
            Pavers=Masonry=WorkProps=0;
            var stone=ValoriaKit.DetailedSurfaceMaterial(new Color(.53f,.50f,.46f,1f),"stone",new Vector2(1.25f,1.25f),.75f);
            var wood=ValoriaKit.DetailedSurfaceMaterial(new Color(.35f,.26f,.19f,1f),"wood",new Vector2(1.2f,1.2f),.75f);
            // R4/R5 future plots: staging is shallow, removable, and deliberately placed
            // along the SOUTH shoulder, leaving each 5m building envelope unobstructed.
            Site(root,"Cantera staging",-7.65f,1.35f,wood,stone);
            Site(root,"Forja staging",6.95f,1.35f,wood,stone);
            // Aserradero identity extends out of its source facade into its own F1 yard.
            for(int i=0;i<5;i++)
            {
                Box(root,"Aserradero lumber",new Vector3(-7.80f+i*.19f,.20f,-2.68f+i*.105f),
                    new Vector3(1.30f,.12f,.12f),wood,true);WorkProps++;
            }
            foreach(var c in root.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }
        static void Site(Transform root,string name,float x,float z,Material wood,Material stone)
        {
            // Interrupted low courses suggest recoverable foundations rather than another house.
            for(int i=0;i<4;i++)
            {
                float xx=x+i*.34f;
                Box(root,name+" stacked block",new Vector3(xx,.22f,z),new Vector3(.31f,.24f,.34f),stone,true);Masonry++;
                if(i<3)Box(root,name+" timber",new Vector3(xx,.44f,z+.49f),new Vector3(.11f,.09f,.85f),wood,true);
            }
            for(int i=0;i<3;i++)
            {
                float xx=x+i*.60f;
                Box(root,name+" removable stake",new Vector3(xx,.35f,z+1.05f),new Vector3(.055f,.65f,.055f),wood,true);
                WorkProps++;
            }
            var crate=Resources.Load<GameObject>("Valoria/UrbanProps/Crate");
            if(crate!=null)
            {
                var go=ValoriaKit.BenchmarkPiece("Valoria · authored cell · "+name+" supply crate",crate,
                    new Vector3(x+.65f,.12f,z-.50f),.48f,.55f,Quaternion.Euler(0,17f,0));
                if(go!=null){go.transform.SetParent(root,true);WorkProps++;}
            }
        }
        static void Box(Transform root,string role,Vector3 p,Vector3 size,Material mat,bool shadow)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="Valoria · authored cell · "+role;go.transform.SetParent(root,true);
            go.transform.position=p;go.transform.localScale=size;
            var renderer=go.GetComponent<Renderer>();renderer.sharedMaterial=mat;
            renderer.shadowCastingMode=shadow?ShadowCastingMode.On:ShadowCastingMode.Off;
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }
    }
}

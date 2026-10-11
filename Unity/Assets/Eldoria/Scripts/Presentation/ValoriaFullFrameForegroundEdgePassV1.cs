using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    public static class ValoriaFullFrameForegroundEdgePassV1
    {
        public static bool Enabled=true;
        const string RootName="Valoria · Full Frame Foreground Edge v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);

            var rockA=Resources.Load<GameObject>("WorldInventory/Rock02");
            var rockB=Resources.Load<GameObject>("WorldInventory/Rock01");
            if(rockA==null)return;

            var specs=new[]{
                new Vector4(-13.5f,-9.0f,4.6f,14f),new Vector4(-9.2f,-9.7f,4.2f,47f),
                new Vector4(-5.2f,-10.0f,3.8f,83f),new Vector4(-1.7f,-10.4f,3.4f,122f),
                new Vector4(2.0f,-10.5f,3.5f,161f),new Vector4(5.8f,-10.0f,3.9f,205f),
                new Vector4(9.8f,-9.5f,4.3f,248f),new Vector4(13.6f,-8.9f,4.7f,291f)
            };
            for(int i=0;i<specs.Length;i++)
            {
                var s=specs[i];
                Add(root,i%2==0?rockA:(rockB??rockA),"front ridge "+i,
                    new Vector3(s.x,-.42f,s.y),s.z,1.50f+(i%3)*.18f,s.w,
                    new Color(.32f,.33f,.30f,1f));
            }

            var art=ValoriaExternalAssetLibrary.Load();
            if(art!=null&&art.SlavicBoulder!=null)
            {
                Add(root,art.SlavicBoulder,"west boulder",new Vector3(-14.7f,-.10f,-6.8f),2.2f,1.15f,37f,new Color(.34f,.35f,.32f,1f));
                Add(root,art.SlavicBoulder,"east boulder",new Vector3(14.8f,-.10f,-6.5f),2.3f,1.20f,213f,new Color(.34f,.35f,.32f,1f));
            }
        }

        static void Add(Transform root,GameObject source,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceIntegrated(
                "Valoria · Foreground Edge · "+role,
                source,anchor,span,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;

            // Large foreground rocks must not retain near-black legacy atlas response.
            // Keep authored geometry, but unify every visible submesh into Valoria's neutral PBR rock family.
            var rockMaterial=ValoriaKit.ExternalPbrSurfaceMaterial(
                "rock",new Color(.39f,.39f,.35f,1f),new Vector2(2.7f,2.7f),.025f,1.02f)
                ?? ValoriaKit.SurfaceMaterial(new Color(.36f,.36f,.33f,1f),"stone",new Vector2(2.7f,2.7f));
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=rockMaterial;
                renderer.sharedMaterials=mats;
            }

            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var mb in go.GetComponentsInChildren<MonoBehaviour>(true))mb.enabled=false;
            go.transform.SetParent(root,true);
        }
    }
}

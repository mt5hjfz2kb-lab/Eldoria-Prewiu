using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Full-frame surface + occupation batch. Existing materials/props only; no gameplay ownership.
    public static class ValoriaFullFrameSurfaceLifeBatchV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame Surface Life Batch v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            // Reuse the already-proven surface correction as one layer inside the whole-frame batch.
            ValoriaInCitySurfacePassV1.Enabled=true;
            ValoriaInCitySurfacePassV1.Build(parent,state);

            var art=ValoriaExternalAssetLibrary.Load();
            BuildSawmillOccupation(root,art);
            BuildBarracksOccupation(root,art);
            BuildLowerApproachOccupation(root,art);
            BuildUpperTerraceOccupation(root,art);
        }

        static void BuildSawmillOccupation(Transform root,ValoriaExternalAssetLibrary art)
        {
            var center=new Vector3(-8.7f,.42f,-3.8f);
            AddProp(root,art!=null?art.Firewood:null,"sawmill firewood A",center+new Vector3(-1.0f,0f,.55f),1.15f,.75f,17f,new Color(.78f,.68f,.52f,1f));
            AddProp(root,art!=null?art.Firewood:null,"sawmill firewood B",center+new Vector3(.72f,0f,.90f),.95f,.66f,-28f,new Color(.74f,.64f,.48f,1f));
            AddUrban(root,"Crate","sawmill crate A",center+new Vector3(-.25f,0f,-.62f),.58f,.58f,13f);
            AddUrban(root,"Barrel","sawmill barrel A",center+new Vector3(.52f,0f,-.58f),.48f,.68f,-9f);
            AddUrban(root,"Crate","sawmill crate B",center+new Vector3(.95f,0f,-.10f),.46f,.46f,36f);
            AddBush(root,art,center+new Vector3(-1.55f,0f,-.35f),.85f);
            AddWarmth(root,"sawmill",center+new Vector3(.10f,1.0f,.25f),.24f,3.3f);
        }

        static void BuildBarracksOccupation(Transform root,ValoriaExternalAssetLibrary art)
        {
            var center=new Vector3(8.6f,.42f,-4.2f);
            AddFence(root,art,center+new Vector3(-1.35f,0f,.35f),2.25f,90f);
            AddFence(root,art,center+new Vector3(1.20f,0f,.35f),2.25f,90f);
            AddUrban(root,"Barrel","barracks barrel",center+new Vector3(-.55f,0f,-.72f),.48f,.66f,0f);
            AddUrban(root,"Crate","barracks crate",center+new Vector3(.15f,0f,-.78f),.54f,.54f,-18f);
            AddUrban(root,"Sack","barracks sack A",center+new Vector3(.70f,0f,-.62f),.62f,.45f,9f);
            AddUrban(root,"Sack","barracks sack B",center+new Vector3(.92f,0f,-.28f),.54f,.40f,-13f);
            AddWarmth(root,"barracks",center+new Vector3(.15f,1.05f,.35f),.26f,3.2f);
        }

        static void BuildLowerApproachOccupation(Transform root,ValoriaExternalAssetLibrary art)
        {
            var center=new Vector3(-.2f,.42f,-5.7f);
            for(int i=0;i<4;i++)
            {
                float side=i%2==0?-1f:1f;
                AddUrban(root,i<2?"Crate":"Barrel","approach stores "+i,
                    center+new Vector3(side*(2.65f+(i/2)*.55f),0f,(i%2)*.55f),
                    i<2?.48f:.43f,i<2?.48f:.62f,side<0?-12f:17f);
            }
            AddFence(root,art,new Vector3(-3.8f,.42f,-5.25f),2.4f,8f);
            AddFence(root,art,new Vector3(3.7f,.42f,-5.25f),2.4f,-8f);
            AddBush(root,art,new Vector3(-4.25f,.42f,-4.55f),.78f);
            AddBush(root,art,new Vector3(4.15f,.42f,-4.55f),.78f);
        }

        static void BuildUpperTerraceOccupation(Transform root,ValoriaExternalAssetLibrary art)
        {
            var points=new[]{
                new Vector3(-4.9f,2.75f,5.6f),
                new Vector3(4.8f,2.72f,5.5f),
                new Vector3(-3.9f,2.72f,7.4f),
                new Vector3(3.8f,2.70f,7.2f)
            };
            for(int i=0;i<points.Length;i++)
            {
                AddUrban(root,i%2==0?"Crate":"Barrel","upper stores "+i,points[i],.42f,i%2==0?.42f:.60f,i*29f);
                AddBush(root,art,points[i]+new Vector3((i%2==0?-.55f:.55f),0f,.25f),.62f);
            }
            AddWarmth(root,"upper west",new Vector3(-4.25f,3.55f,6.2f),.18f,2.8f);
            AddWarmth(root,"upper east",new Vector3(4.2f,3.50f,6.2f),.18f,2.8f);
        }

        static void AddUrban(Transform root,string resource,string role,Vector3 p,float footprint,float height,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/UrbanProps/"+resource);
            AddProp(root,source,role,p,footprint,height,yaw,new Color(.78f,.68f,.54f,1f));
        }

        static void AddFence(Transform root,ValoriaExternalAssetLibrary art,Vector3 p,float span,float yaw)
        {
            AddProp(root,art!=null?art.SlavicStoneFence:null,"occupied yard fence",p,span,.85f,yaw,new Color(.63f,.61f,.56f,1f));
        }

        static void AddBush(Transform root,ValoriaExternalAssetLibrary art,Vector3 p,float span)
        {
            AddProp(root,art!=null?art.SlavicBush:null,"occupied greenery",p,span,.85f,0f,new Color(.62f,.72f,.57f,1f));
        }

        static void AddProp(Transform root,GameObject source,string role,Vector3 p,float footprint,float height,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceModulated("Valoria · SurfaceLife · "+role,source,p,footprint,height,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))b.enabled=false;
        }

        static void AddWarmth(Transform root,string role,Vector3 p,float intensity,float range)
        {
            var go=new GameObject("Valoria · SurfaceLife · "+role+" warmth");
            go.transform.SetParent(root,true);
            go.transform.position=p;
            var light=go.AddComponent<Light>();
            light.type=LightType.Point;
            light.color=new Color(1f,.60f,.31f);
            light.intensity=intensity;
            light.range=range;
            light.shadows=LightShadows.None;
        }
    }
}

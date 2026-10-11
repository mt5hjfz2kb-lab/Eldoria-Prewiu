using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Monumental lateral ruin framing inspired by the approved benchmark composition.
    // Existing project assets only; presentation-only.
    public static class ValoriaFullFrameRuinFrameV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Full Frame Ruin Frame v1";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);

            var art=ValoriaExternalAssetLibrary.Load();
            var gothic=Resources.Load<GameObject>("WorldInventory/Arch_Gothic");
            var broken=Resources.Load<GameObject>("WorldInventory/Wall_Broken");

            // West ruin: broken arch/tower mass behind the sawmill side, never over the route.
            Add(root,art!=null?art.MegaDestroyedTower:null,"west ruined tower",
                new Vector3(-11.8f,.05f,5.6f),4.2f,8.0f,16f,new Color(.54f,.53f,.48f,1f));
            Add(root,gothic,"west broken arch",
                new Vector3(-9.9f,.04f,7.7f),4.9f,7.1f,28f,new Color(.57f,.56f,.51f,1f));
            Add(root,art!=null?art.MasonryWall:null,"west ruin wall",
                new Vector3(-13.1f,.04f,7.8f),4.4f,4.1f,102f,new Color(.52f,.51f,.47f,1f));
            Add(root,broken,"west broken flank",
                new Vector3(-14.2f,.04f,4.1f),3.6f,3.8f,55f,new Color(.48f,.47f,.45f,1f));

            // East ruin: stronger arch silhouette, echoing the benchmark's large broken ring.
            Add(root,art!=null?art.MegaHalfGate:null,"east monumental arch",
                new Vector3(11.3f,.03f,6.1f),5.2f,8.6f,188f,new Color(.56f,.55f,.50f,1f));
            Add(root,art!=null?art.MegaDestroyedTower:null,"east ruined tower",
                new Vector3(14.0f,.03f,7.9f),3.7f,6.9f,211f,new Color(.51f,.50f,.47f,1f));
            Add(root,art!=null?art.MegaWallPassage:null,"east passage ruin",
                new Vector3(12.9f,.03f,3.2f),4.4f,4.0f,248f,new Color(.50f,.49f,.45f,1f));
            Add(root,broken,"east broken flank",
                new Vector3(15.0f,.03f,4.0f),3.2f,3.5f,229f,new Color(.47f,.46f,.44f,1f));

            // Blue identity accents belong to the inhabited Valoria side, never to the distant threat.
            AddBanner(root,"west",new Vector3(-9.6f,5.7f,6.8f),new Vector3(.42f,1.45f,.07f));
            AddBanner(root,"east",new Vector3(11.0f,5.9f,5.4f),new Vector3(.42f,1.45f,.07f));

            AddWarmth(root,"west ruin",new Vector3(-10.2f,2.3f,5.8f),.22f,3.4f);
            AddWarmth(root,"east ruin",new Vector3(10.7f,2.2f,5.5f),.22f,3.4f);
        }

        static void Add(Transform root,GameObject source,string role,Vector3 p,float footprint,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Ruin Frame · "+role,source,p,footprint,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }

        static void AddBanner(Transform root,string role,Vector3 p,Vector3 size)
        {
            var cloth=GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name="Valoria · Ruin Frame · "+role+" banner";
            cloth.transform.SetParent(root,true);cloth.transform.position=p;cloth.transform.localScale=size;
            cloth.GetComponent<Renderer>().sharedMaterial=ValoriaKit.Material(new Color(.10f,.22f,.38f,1f));
            var c=cloth.GetComponent<Collider>();if(c!=null)c.enabled=false;
        }

        static void AddWarmth(Transform root,string role,Vector3 p,float intensity,float range)
        {
            var go=new GameObject("Valoria · Ruin Frame · "+role+" warmth");go.transform.SetParent(root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.58f,.28f);l.intensity=intensity;l.range=range;l.shadows=LightShadows.None;
        }
    }
}

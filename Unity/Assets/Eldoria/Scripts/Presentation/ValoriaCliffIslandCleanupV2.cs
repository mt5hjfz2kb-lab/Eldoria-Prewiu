using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Cleanup pass over the compact cliff-island proof:
    // remove obsolete outer presentation clutter and replace the visually broken barracks shell.
    public static class ValoriaCliffIslandCleanupV2
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Cliff Island Cleanup v2";
        public static int SuppressedOuterRenderers{get;private set;}
        public static int SuppressedBarracksRenderers{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);

            SuppressedOuterRenderers=0;SuppressedBarracksRenderers=0;
            SuppressOuterClutter();
            ReplaceBarracksVisual(root);
        }

        static void SuppressOuterClutter()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                bool outside=Mathf.Abs(b.center.x)>10.8f||b.center.z>12.4f||b.center.z<-10.0f;
                if(!outside)continue;

                string chain=Chain(r.transform);
                if(chain.Contains("bastion")||chain.Contains("aserradero")||chain.Contains("cuartel")||
                   chain.Contains("granary")||chain.Contains("granero")||
                   chain.Contains("backplate candidate")||chain.Contains("cliff island"))continue;

                bool environment=chain.Contains("tree")||chain.Contains("pine")||chain.Contains("rock")||
                                 chain.Contains("terrain")||chain.Contains("seam")||chain.Contains("geology")||
                                 chain.Contains("mountain")||chain.Contains("boulder")||chain.Contains("bush")||
                                 chain.Contains("moss")||chain.Contains("apron")||chain.Contains("future");
                if(!environment)continue;

                r.enabled=false;SuppressedOuterRenderers++;
            }
        }

        static void ReplaceBarracksVisual(Transform root)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                if(!Chain(r.transform).Contains("cuartel · dedicated barracks"))continue;
                r.enabled=false;SuppressedBarracksRenderers++;
            }

            var house=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece02");
            var wing=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece03");
            var roof=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/Piece04");

            var p=new Vector3(7.45f,.42f,-4.72f);
            Add(root,house,"military hall",p,3.35f,3.55f,176f,new Color(.64f,.59f,.51f,1f));
            Add(root,wing,"military store",p+new Vector3(-1.35f,.02f,.75f),2.05f,2.25f,162f,new Color(.52f,.43f,.33f,1f));
            Add(root,roof,"military roof crown",p+new Vector3(.20f,1.92f,.05f),2.35f,1.70f,178f,new Color(.29f,.31f,.31f,1f));

            AddBanner(root,p+new Vector3(-1.25f,2.55f,-.15f));
            AddBanner(root,p+new Vector3(1.22f,2.40f,.08f));

            AddProp(root,"Crate",p+new Vector3(-1.10f,.02f,-1.10f),.55f,.55f,12f);
            AddProp(root,"Barrel",p+new Vector3(-.45f,.02f,-1.15f),.48f,.66f,-8f);
            AddProp(root,"Crate",p+new Vector3(.92f,.02f,-1.02f),.48f,.48f,-18f);

            var lightGo=new GameObject("Valoria · Cliff Cleanup · barracks warmth");
            lightGo.transform.SetParent(root,true);lightGo.transform.position=p+new Vector3(0f,1.25f,-.65f);
            var l=lightGo.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1f,.58f,.28f);l.intensity=.27f;l.range=3.1f;l.shadows=LightShadows.None;
        }

        static void Add(Transform root,GameObject source,string role,Vector3 p,float footprint,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Cliff Cleanup · "+role,source,p,footprint,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;go.transform.SetParent(root,true);DisableGameplay(go);
        }

        static void AddProp(Transform root,string resource,Vector3 p,float footprint,float height,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/UrbanProps/"+resource);
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Cliff Cleanup · "+resource,source,p,footprint,height,Quaternion.Euler(0f,yaw,0f),new Color(.58f,.45f,.31f,1f));
            if(go==null)return;go.transform.SetParent(root,true);DisableGameplay(go);
        }

        static void AddBanner(Transform root,Vector3 p)
        {
            var cloth=GameObject.CreatePrimitive(PrimitiveType.Cube);
            cloth.name="Valoria · Cliff Cleanup · military banner";
            cloth.transform.SetParent(root,true);cloth.transform.position=p;cloth.transform.localScale=new Vector3(.34f,1.0f,.06f);
            cloth.GetComponent<Renderer>().sharedMaterial=ValoriaKit.Material(new Color(.10f,.22f,.38f,1f));
            var c=cloth.GetComponent<Collider>();if(c!=null)c.enabled=false;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }

        static string Chain(Transform t)
        {
            string s="";for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();return s;
        }
    }
}

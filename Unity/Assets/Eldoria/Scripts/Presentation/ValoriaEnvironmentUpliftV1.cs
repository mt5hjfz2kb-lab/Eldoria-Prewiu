using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Global zero-credit environment uplift: coherent PBR surfaces plus replacement of weak
    // presentation-only vegetation/rock markers with higher quality assets already in the project.
    public static class ValoriaEnvironmentUpliftV1
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Environment Uplift v1";

        public static int Repainted{get;private set;}
        public static int ReplacedTrees{get;private set;}
        public static int ReplacedRocks{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null)return;
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(parent,true);

            Repainted=0;ReplacedTrees=0;ReplacedRocks=0;
            var art=ValoriaExternalAssetLibrary.Load();

            var dirt=ValoriaKit.ExternalPbrSurfaceMaterial(
                "dirt",new Color(.69f,.61f,.47f,1f),new Vector2(4.2f,4.2f),.018f,.98f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaDirtSurface:null,
                    new Color(.66f,.58f,.45f,1f),new Vector2(4.2f,4.2f),.02f,.95f);

            var cobble=ValoriaKit.ExternalPbrSurfaceMaterial(
                "cobble",new Color(.80f,.78f,.70f,1f),new Vector2(3.4f,3.4f),.05f,1.02f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaCobbleSurface:null,
                    new Color(.78f,.75f,.68f,1f),new Vector2(3.4f,3.4f),.05f,1f);

            var stone=ValoriaKit.ExternalPbrSurfaceMaterial(
                "stone",new Color(.62f,.61f,.56f,1f),new Vector2(2.8f,2.8f),.03f,1.02f)
                ?? ValoriaKit.PbrSurfaceMaterial(art!=null?art.ValoriaStoneSurface:null,
                    new Color(.61f,.60f,.55f,1f),new Vector2(2.8f,2.8f),.03f,1f);

            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);

                if(IsGround(chain))
                {
                    r.sharedMaterial=dirt;Repainted++;continue;
                }
                if(IsPaving(chain))
                {
                    r.sharedMaterial=cobble;Repainted++;continue;
                }
                if(IsRetainingStone(chain))
                {
                    r.sharedMaterial=stone;Repainted++;
                }
            }

            if(art!=null)
            {
                ReplaceTrees(root,art);
                ReplaceRocks(root,art);
            }
        }

        static bool IsGround(string chain)
        {
            return chain.Contains("inhabited mountain floor")||
                   chain.Contains("lower terrace earth")||
                   chain.Contains("upper terrace earth")||
                   chain.Contains("west expansion terrain")||
                   chain.Contains("east expansion terrain")||
                   chain.Contains("authored apron")||
                   chain.Contains("work court")||
                   chain.Contains("groundkit")&&chain.Contains("earth");
        }

        static bool IsPaving(string chain)
        {
            return chain.Contains("main street")||
                   chain.Contains("entry widening")||
                   chain.Contains("workshop terrace")||
                   chain.Contains("military terrace")||
                   chain.Contains("l1 west terrace")||
                   chain.Contains("l1 east terrace")||
                   chain.Contains("processional edge")||
                   chain.Contains("street slab")||
                   chain.Contains("court")&&!chain.Contains("work court");
        }

        static bool IsRetainingStone(string chain)
        {
            return chain.Contains("retaining")||
                   chain.Contains("landing cheek")||
                   chain.Contains("stair cheek")||
                   chain.Contains("foundation stone");
        }

        static void ReplaceTrees(Transform root,ValoriaExternalAssetLibrary art)
        {
            var candidates=new List<Transform>();
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t==null||!t.gameObject.activeInHierarchy)continue;
                string n=t.name.ToLowerInvariant();
                if(!(n.Contains("pine")||n.Contains("rear tree")||n.Contains("side tree")))continue;
                if(t.GetComponentInParent<WorldHotspot>()!=null)continue;
                if(t.GetComponent<Renderer>()==null&&t.GetComponentInChildren<Renderer>(true)==null)continue;
                candidates.Add(t);
            }

            var used=new HashSet<int>();
            foreach(var t in candidates)
            {
                var top=PresentationTop(t);
                if(top==null||used.Contains(top.GetInstanceID()))continue;
                used.Add(top.GetInstanceID());
                var b=Bounds(top.gameObject);
                if(b.size.y<.5f||b.size.y>7f)continue;
                var source=(ReplacedTrees%2==0?art.SlavicTreeTall:art.SlavicTree)??art.SlavicTreeTall??art.SlavicTree;
                if(source==null)break;
                DisableRenderers(top.gameObject);
                var go=ValoriaKit.BenchmarkPieceIntegrated(
                    "Valoria · Environment Uplift · tree "+ReplacedTrees,
                    source,new Vector3(b.center.x,b.min.y,b.center.z),
                    Mathf.Clamp(Mathf.Max(b.size.x,b.size.z)*.72f,.55f,1.35f),
                    Mathf.Clamp(b.size.y*1.10f,1.7f,3.5f),
                    Quaternion.Euler(0f,(ReplacedTrees*47)%360,0f),
                    new Color(.42f,.55f,.40f,1f));
                if(go!=null)
                {
                    go.transform.SetParent(root,true);
                    DisableGameplay(go);
                    ReplacedTrees++;
                }
            }
        }

        static void ReplaceRocks(Transform root,ValoriaExternalAssetLibrary art)
        {
            var names=new[]{
                "hero frame buried ridge",
                "compactfootprint · edge geology",
                "compactfootprint · buried core rock",
                "terrain seam"
            };
            var used=new HashSet<int>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);
                bool match=false;
                foreach(var n in names)if(chain.Contains(n)){match=true;break;}
                if(!match)continue;
                var top=PresentationTop(r.transform);if(top==null||used.Contains(top.GetInstanceID()))continue;
                used.Add(top.GetInstanceID());
                var b=Bounds(top.gameObject);
                if(b.size.y<.2f||b.size.y>5f)continue;
                var source=(ReplacedRocks%2==0?art.SlavicBoulder:art.SlavicFlatRock)??art.SlavicBoulder??art.SlavicFlatRock;
                if(source==null)break;
                DisableRenderers(top.gameObject);
                var go=ValoriaKit.BenchmarkPieceIntegrated(
                    "Valoria · Environment Uplift · rock "+ReplacedRocks,
                    source,new Vector3(b.center.x,b.min.y,b.center.z),
                    Mathf.Clamp(Mathf.Max(b.size.x,b.size.z)*.82f,.9f,4.6f),
                    Mathf.Clamp(b.size.y*1.00f,.55f,2.6f),
                    Quaternion.Euler(0f,(ReplacedRocks*61)%360,0f),
                    new Color(.53f,.54f,.50f,1f));
                if(go!=null)
                {
                    go.transform.SetParent(root,true);
                    DisableGameplay(go);
                    ReplacedRocks++;
                }
            }
        }

        static Transform PresentationTop(Transform t)
        {
            var cur=t;
            while(cur.parent!=null)
            {
                string n=cur.parent.name.ToLowerInvariant();
                if(n.Contains("integrated construction visual layer")||
                   n.Contains("visual world")||
                   n.Contains("valoria · environment uplift"))break;
                cur=cur.parent;
            }
            return cur;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }

        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            return b;
        }

        static void DisableRenderers(GameObject go)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.enabled=false;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }
    }
}

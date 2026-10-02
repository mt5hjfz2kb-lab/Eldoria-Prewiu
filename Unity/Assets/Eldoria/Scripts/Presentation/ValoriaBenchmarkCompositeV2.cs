using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Benchmark composite v2: compact high-quality urban density + mountain-only horizon strip + finish.
    // Visual-only. Does not own gameplay topology, colliders, hotspots or progression.
    public static class ValoriaBenchmarkCompositeV2
    {
        public static bool Enabled=false;
        const string RootName="Valoria · Benchmark Composite v2";

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName); if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform; root.SetParent(parent,true);

            SuppressChain("Valoria · rescued hero flank");
            BuildUrbanDensity(root);
            ValoriaFullFrameFinishV1.Enabled=true;
            ValoriaFullFrameFinishV1.Build(root,state);
        }

        static void BuildUrbanDensity(Transform root)
        {
            AddCluster(root,"west lower",new Vector3(-8.4f,.43f,-1.65f),12f,1.00f);
            AddCluster(root,"east lower",new Vector3(8.1f,.43f,-1.95f),187f,.97f);
            AddCluster(root,"west mid",new Vector3(-6.9f,.60f,2.75f),22f,.92f);
            AddCluster(root,"east mid",new Vector3(6.7f,.60f,2.95f),174f,.90f);
            AddCluster(root,"west upper",new Vector3(-7.8f,.85f,5.30f),31f,.78f);
            AddCluster(root,"east upper",new Vector3(7.65f,.84f,5.35f),161f,.77f);
        }

        static void AddCluster(Transform root,string id,Vector3 basePos,float yaw,float scale)
        {
            AddPiece(root,"Piece02",id+" house",basePos,2.65f*scale,3.15f*scale,yaw,new Color(.70f,.66f,.58f,1f));
            AddPiece(root,"Piece03",id+" workshop",basePos+Rot(yaw,new Vector3(-1.40f,.01f,.55f)),1.72f*scale,2.05f*scale,yaw+18f,new Color(.61f,.55f,.46f,1f));
            AddPiece(root,"Piece04",id+" roof",basePos+Rot(yaw,new Vector3(.25f,1.62f,.12f)),1.75f*scale,1.40f*scale,yaw-8f,new Color(.48f,.46f,.42f,1f));
            AddWarmth(root,id,basePos+new Vector3(0f,1.25f,.35f));
        }

        static Vector3 Rot(float yaw,Vector3 v)=>Quaternion.Euler(0f,yaw,0f)*v;

        static void AddPiece(Transform root,string resource,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
        {
            var source=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/"+resource);
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Composite v2 · "+role,source,anchor,span,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }

        static void AddWarmth(Transform root,string role,Vector3 p)
        {
            var go=new GameObject("Valoria · Composite v2 · "+role+" warmth");
            go.transform.SetParent(root,true);go.transform.position=p;
            var l=go.AddComponent<Light>();
            l.type=LightType.Point;l.color=new Color(1f,.61f,.31f);l.intensity=.16f;l.range=2.6f;l.shadows=LightShadows.None;
        }

        static void SuppressChain(string exactName)
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(!string.Equals(t.name,exactName,StringComparison.Ordinal))continue;
                    r.enabled=false;break;
                }
            }
        }
    }
}

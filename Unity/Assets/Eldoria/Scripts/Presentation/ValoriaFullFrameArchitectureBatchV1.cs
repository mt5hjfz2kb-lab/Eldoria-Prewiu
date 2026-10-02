using System;
using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Full-frame candidate: denser authored urban masses from existing certified production modules.
    public static class ValoriaFullFrameArchitectureBatchV1
    {
        public static bool Enabled=true;
        const string RootName="Valoria · Full Frame Architecture Batch v1";
        static readonly Color Blue=new Color(.12f,.24f,.39f,1f);

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);

            // Compact city only: strengthen left/right urban shoulders, never widen the canonical footprint.
            BuildCluster(root,"west lower",new Vector3(-8.2f,.42f,-2.7f),12f,1.00f);
            BuildCluster(root,"east lower",new Vector3(7.9f,.42f,-2.9f),188f,.96f);
            BuildCluster(root,"west middle",new Vector3(-6.1f,.74f,2.75f),20f,.88f);
            BuildCluster(root,"east middle",new Vector3(6.0f,.72f,2.95f),176f,.86f);

            DisableGameplay(root.gameObject);
        }

        static void BuildCluster(Transform parent,string id,Vector3 center,float yaw,float scale)
        {
            var root=new GameObject("Valoria · Full Frame Architecture · "+id).transform;
            root.SetParent(parent,true);

            AddTerrain(root,"BroadRockPlatform",center,center.y+.01f,4.25f*scale,yaw+92f);
            AddTerrain(root,"SteppedRockTerrace",center+Rot(yaw,new Vector3(-1.35f,-.02f,-.40f)),center.y+.02f,2.45f*scale,yaw+6f);

            AddPiece(root,"Piece02","inhabited core",center+Rot(yaw,new Vector3(.15f,.04f,-.15f)),3.05f*scale,3.65f*scale,yaw,new Color(.68f,.64f,.57f,1f));
            AddPiece(root,"Piece03","work wing",center+Rot(yaw,new Vector3(-1.75f,.03f,.05f)),2.15f*scale,2.65f*scale,yaw+14f,new Color(.57f,.51f,.44f,1f));
            AddPiece(root,"Piece04","roof crown",center+Rot(yaw,new Vector3(.45f,1.92f,-.12f)),2.18f*scale,1.85f*scale,yaw-5f,new Color(.46f,.45f,.43f,1f));
            AddStone(root,"RockToWallTransition","buried seam",center+Rot(yaw,new Vector3(1.45f,.02f,-.55f)),1.65f*scale,yaw-60f);
            AddStone(root,"HighStraightWall","retaining spine",center+Rot(yaw,new Vector3(.25f,.03f,-1.55f)),2.10f*scale,yaw+3f);
            AddWarmth(root,center+Rot(yaw,new Vector3(.25f,1.15f,1.05f)));
        }

        static Vector3 Rot(float yaw,Vector3 v)=>Quaternion.Euler(0f,yaw,0f)*v;

        static void AddPiece(Transform root,string resource,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint)
        {
            var source=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/"+resource);
            if(source==null)return;
            var go=Object.Instantiate(source);
            go.name="Valoria · Full Frame Architecture · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            Fit(go,anchor,span,maxHeight);
            Tint(go,tint);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void AddStone(Transform root,string resource,string role,Vector3 anchor,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/StoneArchitectureKit_v1/"+resource);
            if(source==null)return;
            var go=Object.Instantiate(source);
            go.name="Valoria · Full Frame Architecture · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            go.transform.localScale*=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
            Tint(go,new Color(.58f,.57f,.53f,1f));
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void AddTerrain(Transform root,string resource,Vector3 anchor,float topY,float span,float yaw)
        {
            var source=Resources.Load<GameObject>("Valoria/TerrainTerraceKit_v1/"+resource);
            if(source==null)return;
            var go=Object.Instantiate(source);
            go.name="Valoria · Full Frame Architecture · buried "+resource;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            var b=Bounds(go);
            go.transform.localScale*=span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            b=Bounds(go);
            go.transform.position+=new Vector3(anchor.x-b.center.x,topY-b.max.y,anchor.z-b.center.z);
            go.transform.SetParent(root,true);
            DisableGameplay(go);
        }

        static void Fit(GameObject go,Vector3 anchor,float span,float maxHeight)
        {
            var b=Bounds(go);
            float horizontal=Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));
            float scale=Mathf.Min(span/horizontal,maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            b=Bounds(go);
            go.transform.position+=anchor-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void Tint(GameObject go,Color tint)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var m=r.sharedMaterial;
                if(m==null)continue;
                var block=new MaterialPropertyBlock();
                r.GetPropertyBlock(block);
                if(m.HasProperty("_BaseColor"))block.SetColor("_BaseColor",tint);
                else if(m.HasProperty("_Color"))block.SetColor("_Color",tint);
                r.SetPropertyBlock(block);
            }
        }

        static void AddWarmth(Transform root,Vector3 p)
        {
            var go=new GameObject("Valoria · Full Frame Architecture · occupied warmth");
            go.transform.SetParent(root,true);
            go.transform.position=p;
            var l=go.AddComponent<Light>();
            l.type=LightType.Point;
            l.color=new Color(1f,.60f,.30f);
            l.intensity=.17f;
            l.range=2.4f;
            l.shadows=LightShadows.None;
        }

        static Bounds Bounds(GameObject go)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return new Bounds(go.transform.position,Vector3.one);
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            return b;
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }
    }
}

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

            AddTerrain(root,"BroadRockPlatform",center,center.y+.01f,4.05f*scale,yaw+92f);
            AddTerrain(root,"SteppedRockTerrace",center+Rot(yaw,new Vector3(-1.25f,-.02f,-.38f)),center.y+.02f,2.25f*scale,yaw+6f);

            bool middle=id.Contains("middle");
            bool east=id.Contains("east");

            if(!middle)
            {
                AddPiece(root,"Piece02","inhabited core",center+Rot(yaw,new Vector3(.10f,.04f,-.15f)),2.90f*scale,3.45f*scale,yaw,new Color(.66f,.62f,.55f,1f),false);
                AddPiece(root,"Piece03","work wing",center+Rot(yaw,new Vector3(-1.58f,.03f,.06f)),1.95f*scale,2.45f*scale,yaw+(east?-11f:14f),new Color(.55f,.50f,.43f,1f),false);
                AddPiece(root,"Piece04","roof crown",center+Rot(yaw,new Vector3(.40f,1.82f,-.10f)),1.88f*scale,1.55f*scale,yaw+(east?7f:-6f),new Color(.29f,.32f,.34f,1f),true);
                AddStone(root,"RockToWallTransition","buried seam",center+Rot(yaw,new Vector3(1.32f,.02f,-.50f)),1.52f*scale,yaw-60f);
            }
            else
            {
                AddPiece(root,"Piece03","mid civic core",center+Rot(yaw,new Vector3(.05f,.04f,-.12f)),2.55f*scale,3.00f*scale,yaw,new Color(.61f,.56f,.49f,1f),false);
                AddPiece(root,"Piece02","mid residence",center+Rot(yaw,new Vector3(east?1.30f:-1.30f,.03f,.18f)),1.90f*scale,2.40f*scale,yaw+(east?-16f:18f),new Color(.69f,.65f,.58f,1f),false);
                if(east)
                    AddPiece(root,"Piece04","small offset roof",center+Rot(yaw,new Vector3(-.55f,1.52f,-.18f)),1.28f*scale,1.10f*scale,yaw+11f,new Color(.27f,.30f,.32f,1f),true);
                AddStone(root,"CornerWallL","corner retaining mass",center+Rot(yaw,new Vector3(east?1.18f:-1.18f,.03f,-.62f)),1.35f*scale,yaw+(east?88f:-88f));
            }

            AddWarmth(root,center+Rot(yaw,new Vector3(.20f,1.05f,1.00f)));
        }

        static Vector3 Rot(float yaw,Vector3 v)=>Quaternion.Euler(0f,yaw,0f)*v;

        static void AddPiece(Transform root,string resource,string role,Vector3 anchor,float span,float maxHeight,float yaw,Color tint,bool forceSlate)
        {
            var source=Resources.Load<GameObject>("Valoria/MidTierArchitectureKit_v1/"+resource);
            if(source==null)return;
            var go=Object.Instantiate(source);
            go.name="Valoria · Full Frame Architecture · "+role;
            go.transform.rotation=Quaternion.Euler(0f,yaw,0f);
            Fit(go,anchor,span,maxHeight);
            Tint(go,tint,forceSlate);
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
            Tint(go,new Color(.58f,.57f,.53f,1f),false);
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

        static void Tint(GameObject go,Color tint,bool forceSlate)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=r.sharedMaterials;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    if(src[i]==null){dst[i]=null;continue;}
                    var m=new Material(src[i]){name="Valoria Full Frame Architecture · "+src[i].name};
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                    if(m.HasProperty("_BaseColorFactor"))m.SetColor("_BaseColorFactor",tint.linear);
                    if(forceSlate)
                    {
                        var slate=new Color(.32f,.35f,.37f,1f);
                        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",slate);
                        if(m.HasProperty("_Color"))m.SetColor("_Color",slate);
                        if(m.HasProperty("_BaseColorFactor"))m.SetColor("_BaseColorFactor",slate.linear);
                        if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.025f);
                    }
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
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

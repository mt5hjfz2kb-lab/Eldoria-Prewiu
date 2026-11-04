using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.ReferenceClean
{
    // Visual-only choreography. Reuses AUTHENTIC building meshes/materials,
    // without cloning gameplay scripts, colliders, saves or construction clocks.
    internal sealed class ConstructionDelightStageAssembly
    {
        const int MaxParts=28;
        const float FinishDuration=1.68f;
        const float TicksPerSecond=10000000f;

        sealed class Part
        {
            public MeshRenderer clone, original;
            public bool originalEnabled;
            public Transform transform;
            public Vector3 position, scale;
            public Quaternion rotation;
            public float height;
        }
        readonly List<Part> parts=new List<Part>(12);
        GameObject root;
        bool working,completing,visible=true;
        long deadline;
        float totalSeconds,started;
        public bool Running=>root && parts.Count>0;
        public bool Working=>Running && working;
        public int MeshCount=>parts.Count;
        public GameObject VisualRoot=>root;

        public bool BeginWork(GameObject dormantModel,long deadlineTicks,
                              long nowTicks,float duration,float time)
        {
            Clear();
            if(!dormantModel || dormantModel.activeInHierarchy || deadlineTicks<=nowTicks)
                return false;
            if(!CloneVisibleMeshComponents(dormantModel))return false;
            working=true;completing=false;visible=true;
            deadline=deadlineTicks;totalSeconds=Mathf.Max(1f,duration);
            started=time;
            Tick(nowTicks,time);
            return true;
        }

        public bool Complete(GameObject builtModel,float time)
        {
            Clear();
            if(!builtModel || !builtModel.activeInHierarchy ||
               !CloneVisibleMeshComponents(builtModel))return false;
            working=false;completing=true;visible=true;started=time;
            // Hide the original renderers only while an identical visible
            // mesh copy physically assembles. Restore flags on every exit.
            foreach(var p in parts)
            {
                if(p.original)p.original.enabled=false;
            }
            // Leave the copies initially at their authentic final bounds so
            // the existing URP architectural sweep can measure them precisely
            // before the first animated frame moves/lowers any component.
            return true;
        }

        public void Tick(long nowTicks,float time)
        {
            if(!Running)return;
            if(working)
            {
                float remaining=(deadline-nowTicks)/TicksPerSecond;
                float real=Mathf.Clamp01(1f-remaining/totalSeconds);
                float initial=.19f*Mathf.SmoothStep(0f,1f,
                    Mathf.Clamp01((time-started)/1.0f));
                float reveal=Mathf.Min(.86f,Mathf.Max(real*.86f,initial));
                float count=Mathf.Max(1,parts.Count);
                for(int i=0;i<parts.Count;i++)
                {
                    float phase=Mathf.Clamp01((reveal-i/count)*count*1.18f);
                    Place(parts[i],phase,false);
                }
            }
            else if(completing)
            {
                float elapsed=Mathf.Max(0f,time-started);
                if(elapsed>=FinishDuration){Clear();return;}
                int count=parts.Count;
                for(int i=0;i<count;i++)
                {
                    float order=count==1?0f:i/(float)(count-1);
                    float phase=Mathf.Clamp01((elapsed-.07f-order*.59f)/.48f);
                    Place(parts[i],phase,true);
                }
            }
        }

        void Place(Part p,float progress,bool finishing)
        {
            if(!p.transform||!p.clone)return;
            float ease=1f-Mathf.Pow(1f-progress,3f);
            float offset=finishing ? Mathf.Lerp(.53f,.88f,p.height) :
                                     Mathf.Lerp(.22f,.42f,p.height);
            float settle=finishing ? Mathf.Sin(progress*Mathf.PI)*.035f:0f;
            p.transform.position=p.position+Vector3.down*(1f-ease)*offset+
                                 Vector3.up*settle;
            p.transform.rotation=p.rotation;
            p.transform.localScale=p.scale*(.90f+.10f*ease);
            p.clone.enabled=visible && progress>.008f;
        }

        public void SetVisible(bool show)
        {
            if(!Running)return;
            visible=show;
            foreach(var p in parts)
            {
                if(!show)
                {
                    if(p.clone)p.clone.enabled=false;
                    if(completing && p.original)p.original.enabled=p.originalEnabled;
                }
                else if(completing && p.original)p.original.enabled=false;
            }
        }

        bool CloneVisibleMeshComponents(GameObject sourceModel)
        {
            root=new GameObject("Construction Delight - authentic staged 3D building");
            root.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            foreach(var src in sourceModel.GetComponentsInChildren<MeshRenderer>(true))
            {
                if(parts.Count>=MaxParts)break;
                if(!src||!src.enabled||!LocallyActive(src.transform,sourceModel.transform))
                    continue;
                var filter=src.GetComponent<MeshFilter>();
                if(!filter || !filter.sharedMesh || src.sharedMaterials.Length==0)continue;
                bool opaque=true;
                foreach(var mat in src.sharedMaterials)
                    if(!mat || mat.renderQueue>=(int)RenderQueue.Transparent)
                    {opaque=false;break;}
                if(!opaque)continue;
                var copyObject=new GameObject("Delight - authentic original mesh",
                    typeof(MeshFilter),typeof(MeshRenderer));
                copyObject.layer=src.gameObject.layer;
                copyObject.transform.SetParent(root.transform,false);
                copyObject.transform.position=src.transform.position;
                copyObject.transform.rotation=src.transform.rotation;
                copyObject.transform.localScale=src.transform.lossyScale;
                copyObject.GetComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
                var r=copyObject.GetComponent<MeshRenderer>();
                r.sharedMaterials=src.sharedMaterials;
                r.receiveShadows=src.receiveShadows;
                r.shadowCastingMode=src.shadowCastingMode;
                r.lightProbeUsage=src.lightProbeUsage;
                r.reflectionProbeUsage=src.reflectionProbeUsage;
                parts.Add(new Part{
                    original=src,originalEnabled=src.enabled,
                    clone=r,transform=copyObject.transform,
                    position=src.transform.position,
                    rotation=src.transform.rotation,scale=src.transform.lossyScale,
                    height=src.transform.TransformPoint(filter.sharedMesh.bounds.center).y
                });
            }
            parts.Sort((a,b)=>a.height.CompareTo(b.height));
            if(parts.Count==0){Clear();return false;}
            float low=parts[0].height;
            float height=Mathf.Max(.15f,parts[parts.Count-1].height-low);
            foreach(var p in parts)p.height=Mathf.Clamp01((p.height-low)/height);
            return true;
        }

        // Inactive source root is expected during work. Its child variants
        // which are themselves inactive must NOT be unintentionally cloned.
        static bool LocallyActive(Transform child,Transform modelRoot)
        {
            for(var t=child;t!=null&&t!=modelRoot;t=t.parent)
                if(!t.gameObject.activeSelf)return false;
            return true;
        }

        public void Clear()
        {
            foreach(var p in parts)
                if(p.original)p.original.enabled=p.originalEnabled;
            parts.Clear();
            if(root)UnityEngine.Object.Destroy(root);
            root=null;working=false;completing=false;visible=true;
            deadline=0;totalSeconds=0f;
        }
    }
}

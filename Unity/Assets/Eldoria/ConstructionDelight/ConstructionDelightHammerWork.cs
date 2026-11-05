using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.ReferenceClean
{
    /// <summary>
    /// Two small, reversible artisan hammers that actually swing against the
    /// worksite's timber/stone edge. Pure scene presentation: no colliders,
    /// physics, pseudo-workers, gameplay commands or asset imports.
    /// </summary>
    internal sealed class ConstructionDelightHammerWork
    {
        sealed class Hammer
        {
            public Transform pivot;
            public Quaternion baseRotation;
            public MeshRenderer handle, head;
            public Vector3 contact;
            public float period, offset;
            public bool hitSent;
            public float side;
        }

        const int HammerCount=2;
        readonly Hammer[] hammers=new Hammer[HammerCount];
        GameObject root;
        Mesh box;
        Material wood, iron;
        bool visible=true;
        float startedAt;
        int impacts;
        public int VisibleRenderers
        {
            get
            {
                int count=0;
                foreach(var h in hammers)
                {
                    if(h==null)continue;
                    if(h.handle && h.handle.enabled)count++;
                    if(h.head && h.head.enabled)count++;
                }
                return count;
            }
        }
        public int TotalImpacts=>impacts;
        public Vector3 PrimaryContact => hammers[0]!=null
            ? hammers[0].contact : Vector3.zero;
        public bool Running=>root && visible;
        public Vector3 FirstImpact {get;private set;}
        public Vector3 SecondImpact {get;private set;}

        public bool Begin(Bounds site,Camera camera,float now)
        {
            Clear();
            if(!camera || site.size.sqrMagnitude<.001f)return false;
            var lit=Shader.Find("Universal Render Pipeline/Lit");
            if(!lit)lit=Shader.Find("Universal Render Pipeline/Simple Lit");
            if(!lit || !lit.isSupported)return false;
            box=CreateBox();
            wood=new Material(lit){name="Delight temporary dark oak hammer handles"};
            iron=new Material(lit){name="Delight temporary forged iron hammer heads"};
            SetColor(wood,new Color(.30f,.18f,.105f));
            SetColor(iron,new Color(.20f,.23f,.23f));
            if(iron.HasProperty("_Metallic"))iron.SetFloat("_Metallic",.35f);
            if(iron.HasProperty("_Smoothness"))iron.SetFloat("_Smoothness",.24f);
            root=new GameObject("Construction Delight - authentic artisan hammer strokes");
            startedAt=now;
            visible=true;
            impacts=0;

            // Horizontal camera-facing layout respects the actual scaffold's
            // bounds. The tools remain much smaller than the building.
            Vector3 right=camera.transform.right;
            right.y=0f;
            right=right.sqrMagnitude>.01f?right.normalized:Vector3.right;
            Vector3 toward=camera.transform.position-site.center;
            toward.y=0f;
            toward=toward.sqrMagnitude>.01f?toward.normalized:Vector3.back;
            float front=Mathf.Abs(toward.x)*site.extents.x+
                        Mathf.Abs(toward.z)*site.extents.z;
            float width=Mathf.Clamp(Mathf.Max(site.size.x,site.size.z),1.6f,7f);
            float size=Mathf.Clamp(width*.12f,.34f,.72f);
            float baseline=site.min.y+Mathf.Clamp(site.size.y*.30f,.34f,1.10f);

            for(int i=0;i<HammerCount;i++)
            {
                float side=i==0?-1f:1f;
                Vector3 contact=new Vector3(site.center.x,baseline,site.center.z)+
                    right*(side*width*.20f)+toward*(front*.91f+.09f);
                var holder=new GameObject("Hammer swing pivot "+i);
                holder.transform.SetParent(root.transform,false);
                holder.transform.position=contact;
                holder.transform.rotation=Quaternion.LookRotation(-toward,Vector3.up);
                holder.transform.localScale=Vector3.one*size;
                var handle=MakePart("oak grip",holder.transform,wood,
                    new Vector3(0f,.31f,0f),new Vector3(.14f,.62f,.17f));
                var head=MakePart("forged iron head",holder.transform,iron,
                    new Vector3(0f,.63f,0f),new Vector3(.64f,.26f,.31f));
                hammers[i]=new Hammer{
                    pivot=holder.transform,baseRotation=holder.transform.localRotation,
                    handle=handle,head=head,
                    contact=contact,period=i==0?1.46f:1.79f,
                    offset=i==0?.04f:.63f,side=side,hitSent=false
                };
            }
            return true;
        }

        static void SetColor(Material material,Color color)
        {
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",color);
            else if(material.HasProperty("_Color"))material.color=color;
        }

        MeshRenderer MakePart(string name,Transform pivot,Material material,
                              Vector3 position,Vector3 scale)
        {
            var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));
            obj.layer=pivot.gameObject.layer;
            obj.transform.SetParent(pivot,false);
            obj.transform.localPosition=position;
            obj.transform.localScale=scale;
            obj.GetComponent<MeshFilter>().sharedMesh=box;
            var r=obj.GetComponent<MeshRenderer>();
            r.sharedMaterial=material;
            r.receiveShadows=false;
            r.shadowCastingMode=ShadowCastingMode.Off;
            return r;
        }

        /// <returns>Number of actual hammer impact events (0..2) this tick.</returns>
        public int Tick(float now)
        {
            if(!Running)return 0;
            int count=0;
            FirstImpact=SecondImpact=Vector3.zero;
            for(int i=0;i<HammerCount;i++)
            {
                var h=hammers[i];
                if(h==null || !h.pivot)continue;
                float cycle=Mathf.Repeat(now-startedAt+h.offset,h.period)/h.period;
                float angle;
                if(cycle<.25f)angle=Mathf.Lerp(5f,55f,Mathf.SmoothStep(0f,1f,cycle/.25f));
                else if(cycle<.405f)
                    angle=Mathf.Lerp(55f,-17f,Mathf.SmoothStep(0f,1f,(cycle-.25f)/.155f));
                else if(cycle<.54f)
                    angle=Mathf.Lerp(-17f,10f,Mathf.SmoothStep(0f,1f,(cycle-.405f)/.135f));
                else angle=10f;
                h.pivot.localRotation=h.baseRotation*Quaternion.Euler(0f,0f,angle*h.side);
                if(cycle<.19f)h.hitSent=false;
                if(cycle>=.405f && !h.hitSent)
                {
                    h.hitSent=true;
                    impacts++;
                    count++;
                    if(count==1)FirstImpact=h.contact;
                    else SecondImpact=h.contact;
                }
            }
            return count;
        }

        public void SetVisible(bool show)
        {
            visible=show;
            if(root)root.SetActive(show);
        }

        public void Clear()
        {
            visible=false;
            foreach(var h in hammers)
            {
                if(h==null)continue;
                if(h.handle)h.handle.enabled=false;
                if(h.head)h.head.enabled=false;
            }
            for(int i=0;i<hammers.Length;i++)hammers[i]=null;
            if(root){root.SetActive(false);Object.Destroy(root);}
            root=null;
            if(wood)Object.Destroy(wood);
            if(iron)Object.Destroy(iron);
            if(box)Object.Destroy(box);
            box=null;wood=null;iron=null;impacts=0;
        }

        static Mesh CreateBox()
        {
            // 24 vertices preserve correct cube normals; no built-in primitive
            // GameObject/Collider and no import dependency.
            Vector3[] n={Vector3.forward,Vector3.back,Vector3.left,
                         Vector3.right,Vector3.up,Vector3.down};
            Vector3[] tangents={Vector3.right,Vector3.left,Vector3.forward,
                                Vector3.back,Vector3.right,Vector3.right};
            Vector3[] ups={Vector3.up,Vector3.up,Vector3.up,
                           Vector3.up,Vector3.back,Vector3.forward};
            var vertices=new Vector3[24];
            var normals=new Vector3[24];
            var indices=new int[36];
            for(int face=0;face<6;face++)
            {
                Vector3 center=n[face]*.5f;
                Vector3 u=tangents[face]*.5f,v=ups[face]*.5f;
                int p=face*4,k=face*6;
                vertices[p]=center-u-v;vertices[p+1]=center+u-v;
                vertices[p+2]=center-u+v;vertices[p+3]=center+u+v;
                for(int i=0;i<4;i++)normals[p+i]=n[face];
                indices[k]=p;indices[k+1]=p+1;indices[k+2]=p+2;
                indices[k+3]=p+1;indices[k+4]=p+3;indices[k+5]=p+2;
            }
            var mesh=new Mesh{name="Delight hammer box with lit face normals"};
            mesh.vertices=vertices;mesh.normals=normals;mesh.triangles=indices;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

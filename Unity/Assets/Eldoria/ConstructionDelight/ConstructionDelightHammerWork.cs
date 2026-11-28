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
        public int AnchoredContactCount {get;private set;}
        public float MinimumContactHeightFraction {get;private set;}
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

        // A hammer is only created if both head positions can be bound to the
        // top surfaces of AUTHENTIC enabled timber/stone MeshRenderers.
        // This avoids hovering tools in the grass outside a broad parcel collider.
        public bool Begin(Bounds site,Camera camera,float now,GameObject worksite)
        {
            Clear();
            if(!camera || !worksite || !worksite.activeInHierarchy ||
               site.size.sqrMagnitude<.001f)return false;
            Vector3 right=camera.transform.right;
            right.y=0f;
            right=right.sqrMagnitude>.01f?right.normalized:Vector3.right;
            Vector3 toward=camera.transform.position-site.center;
            toward.y=0f;
            toward=toward.sqrMagnitude>.01f?toward.normalized:Vector3.back;
            if(!TryFindContact(worksite,site,right,toward,-1f,out var left) ||
               !TryFindContact(worksite,site,right,toward,1f,out var rightContact))
                return false;
            float width=Mathf.Clamp(Mathf.Max(site.size.x,site.size.z),1.6f,7f);
            if(Vector3.Distance(left,rightContact)<Mathf.Max(.16f,width*.12f))
                return false;

            var lit=Shader.Find("Universal Render Pipeline/Lit");
            if(!lit)lit=Shader.Find("Universal Render Pipeline/Simple Lit");
            if(!lit || !lit.isSupported)return false;
            box=CreateBox();
            wood=new Material(lit){name="Delight temporary oak hammer grips"};
            iron=new Material(lit){name="Delight temporary forged iron faces"};
            SetColor(wood,new Color(.31f,.18f,.10f));
            SetColor(iron,new Color(.30f,.35f,.37f));
            if(iron.HasProperty("_Metallic"))iron.SetFloat("_Metallic",.28f);
            if(iron.HasProperty("_Smoothness"))iron.SetFloat("_Smoothness",.18f);
            root=new GameObject("Construction Delight - worksite-bound artisan hammers");
            startedAt=now;
            visible=true;
            impacts=0;
            AnchoredContactCount=2;
            MinimumContactHeightFraction=Mathf.Min(left.y,rightContact.y)-site.min.y;
            MinimumContactHeightFraction/=Mathf.Max(.01f,site.size.y);

            // Compromise between V6 oversized silhouettes and V7 invisible ones:
            // a small readable iron head at mobile zoom, with no floating yard tools.
            float size=Mathf.Clamp(width*.155f,.45f,.82f);
            var baseRotation=Quaternion.LookRotation(-toward,Vector3.up);
            for(int i=0;i<HammerCount;i++)
            {
                float side=i==0?-1f:1f;
                Vector3 contact=i==0?left:rightContact;
                var holder=new GameObject("Hammer swing pivot "+i);
                holder.transform.SetParent(root.transform,false);
                holder.transform.rotation=baseRotation;
                holder.transform.localScale=Vector3.one*size;
                // The underside of the real iron mesh is at local y=.565:
                // at a strike that underside meets the selected real surface.
                var contactPose=baseRotation*Quaternion.Euler(0f,0f,-17f*side);
                holder.transform.position=contact-
                    contactPose*(Vector3.up*(.565f*size));
                var handle=MakePart("oak grip",holder.transform,wood,
                    new Vector3(0f,.34f,0f),new Vector3(.12f,.68f,.13f));
                var head=MakePart("forged iron head",holder.transform,iron,
                    new Vector3(0f,.69f,0f),new Vector3(.64f,.25f,.28f));
                hammers[i]=new Hammer{
                    pivot=holder.transform,baseRotation=holder.transform.localRotation,
                    handle=handle,head=head,
                    contact=contact,period=i==0?1.46f:1.79f,
                    offset=i==0?.04f:.63f,side=side,hitSent=false
                };
            }
            return true;
        }

        // Use render geometry, never a guessed location, UI button hotspot,
        // terrain plane or a hidden completed model. Both anchors remain on
        // the same real worksite even if its scaffolding has many small stones.
        static bool TryFindContact(GameObject worksite,Bounds site,Vector3 right,
                                   Vector3 toward,float side,out Vector3 contact)
        {
            contact=Vector3.zero;
            float lateralSite=Mathf.Abs(right.x)*site.extents.x+
                              Mathf.Abs(right.z)*site.extents.z;
            float frontSite=Mathf.Abs(toward.x)*site.extents.x+
                            Mathf.Abs(toward.z)*site.extents.z;
            float target=side*lateralSite*.50f;
            // Select joinery/masonry actually underway, not the decorative
            // foundation logs. V8 rendered both hammer heads at grass height.
            float preferredTop=site.min.y+site.size.y*.64f;
            float best=float.MaxValue;
            bool found=false;
            foreach(var renderer in worksite.GetComponentsInChildren<MeshRenderer>())
            {
                if(!renderer || !renderer.enabled ||
                   !renderer.gameObject.activeInHierarchy ||
                   !renderer.GetComponent<MeshFilter>() ||
                   !renderer.GetComponent<MeshFilter>().sharedMesh)continue;
                Bounds surface=renderer.bounds;
                if(surface.size.sqrMagnitude<.0001f ||
                   surface.max.y<site.min.y+Mathf.Max(.18f,site.size.y*.36f))
                    continue;
                // Exclude broad ground planes even if included by the host.
                if(surface.size.y<.14f &&
                   surface.size.x*surface.size.z>site.size.x*site.size.z*.56f)
                    continue;
                float lateral=Vector3.Dot(surface.center-site.center,right);
                float front=Vector3.Dot(surface.center-site.center,toward);
                float halfSpan=Mathf.Abs(right.x)*surface.extents.x+
                               Mathf.Abs(right.z)*surface.extents.z;
                float positioned=Mathf.Clamp(target,lateral-halfSpan*.68f,
                                                   lateral+halfSpan*.68f);
                float score=Mathf.Abs(positioned-target)*1.65f+
                            Mathf.Abs(surface.max.y-preferredTop)*1.18f+
                            Mathf.Abs(front-frontSite*.34f)*.36f;
                // Work with a foreground timber/stone top: the strike is outside
                // the near face, not occluded behind its masonry as in V7.
                if(front<0f)score+=Mathf.Abs(front)*.75f;
                if(score>=best)continue;
                best=score;
                float face=Mathf.Abs(toward.x)*surface.extents.x+
                           Mathf.Abs(toward.z)*surface.extents.z;
                contact=surface.center+right*(positioned-lateral)+
                        toward*(face*.95f+Mathf.Clamp(site.size.y*.012f,.018f,.055f));
                contact.y=surface.max.y+Mathf.Clamp(site.size.y*.008f,.012f,.04f);
                found=true;
            }
            return found;
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
            // A subtle real contact shadow on the authentic facade is what
            // distinguishes the tiny tool from a hovering UI ornament.
            r.receiveShadows=true;
            r.shadowCastingMode=ShadowCastingMode.On;
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
            AnchoredContactCount=0;
            MinimumContactHeightFraction=0f;
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

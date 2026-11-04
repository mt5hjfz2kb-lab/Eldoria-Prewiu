using System;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.ReferenceClean
{
    // Read-only visual layer; LocalGateway owns every building command and timestamp.
    // Uses ordinary MeshRenderers rather than UnityEngine.ParticleSystemModule:
    // Eldoria's canonical manifest deliberately does not enable that built-in module.
    [DisallowMultipleComponent]
    public sealed class ValoriaConstructionDelightV1 : MonoBehaviour
    {
        const float PollSeconds = .18f;
        const float WorldReturnGraceSeconds = 14f;
        static bool registered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register()
        {
            if (!registered)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                registered = true;
            }
            Attach();
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

        static void Attach()
        {
            var city = UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
            if (city && !city.GetComponent<ValoriaConstructionDelightV1>())
                city.gameObject.AddComponent<ValoriaConstructionDelightV1>();
        }

        ValoriaReferenceCleanGameplay city;
        ValoriaIntegratedWorld integrated;
        LocalGateway observedGateway;
        ConstructionDelightTask previous;
        bool initialized;
        float nextObservation;
        string activeBuilding;
        bool pendingCompletion;
        string pendingId;
        float pendingExpiresAt;

        SpriteField loopDust, loopMotes, openingDust, finishDust;
        SpriteField finishEmbers, finishRing, finishHalo;
        SpriteField[] allFields;
        Texture2D dustTexture, ringTexture;
        Material dustMaterial, ringMaterial;
        Transform particleRoot;

        public int ActiveConstructionSprites => (loopDust?.VisibleCount ?? 0) +
                                                 (loopMotes?.VisibleCount ?? 0);
        public int ActiveFinishSprites => (finishDust?.VisibleCount ?? 0) +
                                         (finishEmbers?.VisibleCount ?? 0) +
                                         (finishRing?.VisibleCount ?? 0) +
                                         (finishHalo?.VisibleCount ?? 0);

        void Awake()
        {
            city = GetComponent<ValoriaReferenceCleanGameplay>();
            integrated = GetComponent<ValoriaIntegratedWorld>();
            InitializeArt();
        }

        void Update()
        {
            if (!city || !city.Ready || city.Gateway == null || allFields == null) return;
            if (!ReferenceEquals(observedGateway, city.Gateway))
            {
                observedGateway = city.Gateway;
                previous = default;
                initialized = false;
                pendingCompletion = false;
                StopWork();
                foreach (var field in allFields) field.Clear();
            }

            if (Time.unscaledTime >= nextObservation)
            {
                nextObservation = Time.unscaledTime + PollSeconds;
                Observe(city.Gateway.Snapshot());
            }

            bool visible = CanShow();
            if (pendingCompletion && visible)
            {
                if (Time.unscaledTime <= pendingExpiresAt) Complete(pendingId);
                pendingCompletion = false;
            }

            if (activeBuilding != null)
            {
                if (visible && !loopDust.Running)
                {
                    loopDust.Play(); loopMotes.Play();
                }
                else if (!visible && loopDust.Running)
                {
                    loopDust.Clear(); loopMotes.Clear();
                }
            }
            if (!visible && activeBuilding == null)
            {
                // No decorative construction geometry over the strategic map.
                foreach (var field in allFields) field.Clear();
            }

            var cam = city.PlayerCamera;
            float dt = Mathf.Min(.06f, Time.unscaledDeltaTime);
            foreach (var field in allFields) field.Tick(dt, cam);
        }

        bool CanShow()
        {
            if (!city || !city.PlayerCamera ||
                !city.PlayerCamera.gameObject.activeInHierarchy) return false;
            return !(integrated && integrated.Ready && integrated.InWorld);
        }

        void Observe(PlayerState state)
        {
            bool hasTask = ConstructionDelightState.TryRead(state, out var current);
            if (!initialized)
            {
                initialized = true;
                previous = hasTask ? current : default;
                if (hasTask) StartWork(current.BuildingId, false);
                return;
            }
            bool same = hasTask && ConstructionDelightState.SameTask(previous, current);
            if (previous.IsValid && !same)
            {
                var finished = previous;
                StopWork();
                if (ConstructionDelightState.JustCompleted(finished, state))
                {
                    if (CanShow()) Complete(finished.BuildingId);
                    else
                    {
                        pendingCompletion = true;
                        pendingId = finished.BuildingId;
                        pendingExpiresAt = Time.unscaledTime + WorldReturnGraceSeconds;
                    }
                }
            }
            if (hasTask && !same) StartWork(current.BuildingId, true);
            else if (hasTask && activeBuilding == null) StartWork(current.BuildingId, false);
            previous = hasTask ? current : default;
        }

        void StartWork(string id, bool animateStart)
        {
            StopWork();
            if (!PositionSite(id, false)) return;
            activeBuilding = id;
            loopDust.Tint = id == "bastion"
                ? new Color(.86f,.72f,.51f,.23f) : new Color(.77f,.69f,.53f,.26f);
            loopMotes.Tint = id == "sawmill"
                ? new Color(.99f,.76f,.46f,.56f) : new Color(.91f,.76f,.58f,.44f);
            if (!CanShow()) return;

            loopDust.Play();
            loopMotes.Play();
            loopDust.Emit(9);
            loopMotes.Emit(5);
            if (animateStart)
            {
                openingDust.Emit(14);
                finishEmbers.Tint = new Color(1f,.78f,.45f,.27f);
                finishEmbers.Emit(4);
            }
        }

        void StopWork()
        {
            activeBuilding = null;
            if (loopDust != null) loopDust.Clear();
            if (loopMotes != null) loopMotes.Clear();
        }

        void Complete(string id)
        {
            if (!CanShow() || !PositionSite(id, true)) return;
            finishDust.Tint = new Color(.84f,.75f,.60f,.33f);
            finishEmbers.Tint = id == "bastion"
                ? new Color(1f,.91f,.67f,.83f) : new Color(1f,.82f,.49f,.77f);
            finishRing.Tint = new Color(1f,.78f,.48f,.22f);
            finishHalo.Tint = new Color(1f,.82f,.54f,.14f);

            finishDust.Emit(17);
            finishEmbers.Emit(id == "bastion" ? 32 : 26);
            finishRing.Emit(1);
            finishHalo.Emit(1);
            // No model, material or level mutations. Existing scene switching is authoritative.
        }

        // Real colliders and visible model bounds only; never guess city coordinates.
        bool PositionSite(string id, bool preferFinishedModel)
        {
            if (!FindSite(id, preferFinishedModel, out var b)) return false;
            var cam = city.PlayerCamera;
            Vector3 eye = cam ? (cam.transform.position - b.center).normalized : Vector3.back;
            Vector3 toward = eye;
            toward.y = 0f;
            toward = toward.sqrMagnitude > .001f ? toward.normalized : Vector3.back;
            float footprint = Mathf.Clamp(Mathf.Max(b.size.x,b.size.z)*.78f,1.25f,6.25f);

            Vector3 ground = new Vector3(b.center.x,b.min.y + .22f,b.center.z) + toward*.26f;
            float frontDepth = Mathf.Abs(eye.x)*b.extents.x +
                               Mathf.Abs(eye.y)*b.extents.y +
                               Mathf.Abs(eye.z)*b.extents.z;
            Vector3 facade = b.center + eye*Mathf.Min(frontDepth+.18f,20f);
            loopDust.Site(ground,footprint);
            loopMotes.Site(ground+Vector3.up*.17f,footprint*.62f);
            openingDust.Site(ground,footprint);
            finishDust.Site(ground,footprint);
            finishEmbers.Site(facade,footprint*.85f);
            finishRing.Site(facade,0f);
            finishHalo.Site(facade,0f);
            float diameter = Mathf.Clamp(footprint*1.40f,2f,8.5f);
            finishRing.BaseSize = diameter;
            finishHalo.BaseSize = diameter*.83f;
            return true;
        }

        bool FindSite(string id, bool preferModel, out Bounds bounds)
        {
            bounds=default;
            if (preferModel && ModelBounds(id,out bounds)) return true;
            Collider target=null;
            var parcels=city.Parcels;
            if(parcels)
            {
                if(id=="sawmill")target=parcels.LeftTarget;
                else if(id=="barracks")target=parcels.RightTarget;
                if(!target && parcels.Bindings != null)
                    foreach(var binding in parcels.Bindings)
                        if(binding!=null && binding.BuildingId==id && binding.Target)
                        {
                            target=binding.Target;
                            break;
                        }
            }
            if(!target)
                foreach(var point in GetComponentsInChildren<WorldHotspot>(true))
                    if(point && point.Id==id && point.TryGetComponent(out Collider found))
                    {
                        target=found;
                        break;
                    }
            if(target && target.enabled && target.gameObject.activeInHierarchy &&
               target.bounds.size.sqrMagnitude>.001f)
            {
                bounds=target.bounds;
                return true;
            }
            return ModelBounds(id,out bounds);
        }

        bool ModelBounds(string id, out Bounds bounds)
        {
            bounds=default;
            var parcels=city.Parcels;
            GameObject model=null;
            if(id=="bastion")
                model=city.BastionTwo && city.BastionTwo.activeInHierarchy
                    ? city.BastionTwo : city.BastionOne;
            else if(parcels)
                model=id=="sawmill" ? parcels.LeftBuilt :
                      id=="barracks" ? parcels.RightBuilt : null;
            if(!model || !model.activeInHierarchy)return false;
            bool has=false;
            foreach(var renderer in model.GetComponentsInChildren<Renderer>())
            {
                if(!renderer || !renderer.enabled)continue;
                if(!has){bounds=renderer.bounds;has=true;}
                else bounds.Encapsulate(renderer.bounds);
            }
            return has && bounds.size.sqrMagnitude>.001f;
        }

        void InitializeArt()
        {
            var container=new GameObject("ConstructionDelightOwnedSprites");
            container.transform.SetParent(transform,false);
            particleRoot=container.transform;
            dustTexture=MakeTexture(false);
            ringTexture=MakeTexture(true);
            dustMaterial=MakeMaterial("Eldoria / artisan dust",dustTexture);
            ringMaterial=MakeMaterial("Eldoria / worn-gold aureole",ringTexture);
            if(!dustMaterial || !ringMaterial)
            {
                Debug.LogError("CONSTRUCTION_DELIGHT_SHADER_UNAVAILABLE");
                enabled=false;
                return;
            }
            loopDust=NewField("Work / drifting pale stone dust",dustMaterial,65,5.2f,.65f,1.40f,.14f,.33f,.13f,false,19);
            loopMotes=NewField("Work / timber and mortar flecks",dustMaterial,34,2.7f,.50f,1.05f,.06f,.13f,.37f,false,27);
            openingDust=NewField("Start / scattering construction dust",dustMaterial,34,0f,.32f,.85f,.15f,.33f,.28f,false,39);
            finishDust=NewField("Finish / last gentle dust",dustMaterial,45,0f,.45f,1.10f,.17f,.37f,.39f,false,43);
            finishEmbers=NewField("Finish / antique gold fine glints",dustMaterial,70,0f,.57f,1.22f,.07f,.15f,.76f,false,57);
            finishRing=NewField("Finish / soft engraved rim",ringMaterial,2,0f,1.44f,1.44f,1f,1f,0f,true,79);
            finishHalo=NewField("Finish / diffuse warm reveal",dustMaterial,2,0f,1.20f,1.20f,1f,1f,0f,true,83);
            allFields=new[]{loopDust,loopMotes,openingDust,finishDust,finishEmbers,finishRing,finishHalo};
        }

        SpriteField NewField(string name,Material material,int cap,float rate,
            float lifeLow,float lifeHigh,float sizeLow,float sizeHigh,float up,bool fixedCenter,int seed)
        {
            return new SpriteField(particleRoot,name,material,cap,rate,lifeLow,lifeHigh,
                                   sizeLow,sizeHigh,up,fixedCenter,seed);
        }

        static Texture2D MakeTexture(bool ring)
        {
            const int n=64;
            var t=new Texture2D(n,n,TextureFormat.RGBA32,false,true)
            {
                name=ring?"Delight antique circle":"Delight organic dust",
                filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp
            };
            var pixels=new Color[n*n];
            for(int y=0;y<n;y++)
                for(int x=0;x<n;x++)
                {
                    float px=(x+.5f-n*.5f)/(n*.5f);
                    float py=(y+.5f-n*.5f)/(n*.5f);
                    float rr=Mathf.Sqrt(px*px+py*py);
                    float a=ring
                        ? Mathf.Exp(-Mathf.Pow((rr-.73f)/.086f,2f))*Mathf.Clamp01((1f-rr)*9f)
                        : Mathf.Pow(Mathf.Clamp01(1f-rr),2.25f);
                    pixels[y*n+x]=new Color(1f,1f,1f,a);
                }
            t.SetPixels(pixels);
            t.Apply(false,true);
            return t;
        }

        static Material MakeMaterial(string name,Texture2D texture)
        {
            var shader=Resources.Load<Shader>("ConstructionDelightSoft") ??
                       Shader.Find("Eldoria/ConstructionDelight/SoftParticle");
            if(!shader)return null;
            var material=new Material(shader){name=name};
            if(material.HasProperty("_BaseMap"))material.SetTexture("_BaseMap",texture);
            if(material.HasProperty("_MainTex"))material.SetTexture("_MainTex",texture);
            material.renderQueue=(int)RenderQueue.Transparent;
            return material;
        }

        void OnDestroy()
        {
            if(allFields!=null)foreach(var field in allFields)field.Dispose();
            if(dustMaterial)Destroy(dustMaterial);
            if(ringMaterial)Destroy(ringMaterial);
            if(dustTexture)Destroy(dustTexture);
            if(ringTexture)Destroy(ringTexture);
        }

        // Seven small batched quad fields replace an unavailable ParticleSystem
        // module. No physics, AI, second clock, textures on disk or per-frame GC.
        sealed class SpriteField
        {
            struct Sprite
            {
                public bool live;
                public Vector3 offset,drift;
                public float age,life,size,twist;
            }

            public readonly MeshRenderer Renderer;
            readonly Mesh mesh;
            readonly Sprite[] sprites;
            readonly Vector3[] vertices;
            readonly Color32[] colors;
            readonly Vector2[] uvs;
            readonly int capacity;
            readonly float rate,minLife,maxLife,minSize,maxSize,upward;
            readonly bool centered;
            readonly System.Random rng;
            readonly Transform holder;
            float accumulator;
            int cursor;
            bool running;
            float footprint=1.5f;

            public Color Tint = new Color(.81f,.70f,.53f,.25f);
            public float BaseSize=1f;
            public bool Running => running;
            public int VisibleCount {get;private set;}

            public SpriteField(Transform parent,string name,Material material,
                int cap,float spawnRate,float lifeLow,float lifeHigh,
                float sizeLow,float sizeHigh,float up,bool fixedCenter,int seed)
            {
                capacity=cap;rate=spawnRate;minLife=lifeLow;maxLife=lifeHigh;
                minSize=sizeLow;maxSize=sizeHigh;upward=up;centered=fixedCenter;
                rng=new System.Random(seed);
                sprites=new Sprite[capacity];
                vertices=new Vector3[capacity*4];
                colors=new Color32[capacity*4];
                uvs=new Vector2[capacity*4];
                var triangles=new int[capacity*6];
                for(int i=0;i<capacity;i++)
                {
                    int v=i*4,k=i*6;
                    uvs[v]=new Vector2(0,0);
                    uvs[v+1]=new Vector2(1,0);
                    uvs[v+2]=new Vector2(0,1);
                    uvs[v+3]=new Vector2(1,1);
                    triangles[k]=v;triangles[k+1]=v+2;triangles[k+2]=v+1;
                    triangles[k+3]=v+1;triangles[k+4]=v+2;triangles[k+5]=v+3;
                }
                var go=new GameObject("Delight - "+name,typeof(MeshFilter),typeof(MeshRenderer));
                go.transform.SetParent(parent,true);
                holder=go.transform;
                mesh=new Mesh{name="Eldoria batched gentle "+name};
                mesh.MarkDynamic();
                mesh.vertices=vertices;
                mesh.uv=uvs;
                mesh.triangles=triangles;
                mesh.bounds=new Bounds(Vector3.zero,Vector3.one*45f);
                go.GetComponent<MeshFilter>().sharedMesh=mesh;
                Renderer=go.GetComponent<MeshRenderer>();
                Renderer.sharedMaterial=material;
                Renderer.shadowCastingMode=ShadowCastingMode.Off;
                Renderer.receiveShadows=false;
                Renderer.enabled=false;
            }

            double Random01() => rng.NextDouble();
            float RandomBetween(float min,float max) =>
                min+(float)Random01()*(max-min);
            public void Site(Vector3 worldPosition,float width)
            {
                holder.position=worldPosition;
                holder.rotation=Quaternion.identity;
                if(width>0f)footprint=width;
            }
            public void Play(){running=true;}
            public void Clear()
            {
                running=false;accumulator=0f;
                for(int i=0;i<capacity;i++)sprites[i].live=false;
                VisibleCount=0;
                Renderer.enabled=false;
            }
            public void Emit(int count)
            {
                for(int i=0;i<count;i++)
                {
                    int slot=cursor++%capacity;
                    float age=0f;
                    float life=RandomBetween(minLife,maxLife);
                    float size=centered?BaseSize:RandomBetween(minSize,maxSize);
                    var at=centered?Vector3.zero:
                        new Vector3(RandomBetween(-footprint*.46f,footprint*.46f),
                                    RandomBetween(-.12f,.25f),
                                    RandomBetween(-footprint*.29f,footprint*.29f));
                    var velocity=centered?Vector3.zero:
                        new Vector3(RandomBetween(-.12f,.13f),RandomBetween(upward*.70f,upward*1.35f),
                                    RandomBetween(-.09f,.12f));
                    sprites[slot]=new Sprite{live=true,offset=at,drift=velocity,
                        age=age,life=life,size=size,twist=RandomBetween(-2.0f,2.0f)};
                }
            }

            public void Tick(float dt,Camera cam)
            {
                if(running && rate>0f)
                {
                    accumulator+=dt*rate;
                    int spawn=Mathf.Min(capacity,(int)accumulator);
                    if(spawn>0){accumulator-=spawn;Emit(spawn);}
                }
                var right=cam?cam.transform.right:Vector3.right;
                var up=cam?cam.transform.up:Vector3.up;
                VisibleCount=0;
                for(int i=0;i<capacity;i++)
                {
                    int v=i*4;
                    var s=sprites[i];
                    if(!s.live || (s.age+=dt)>=s.life)
                    {
                        sprites[i].live=false;
                        colors[v]=colors[v+1]=colors[v+2]=colors[v+3]=new Color32(0,0,0,0);
                        vertices[v]=vertices[v+1]=vertices[v+2]=vertices[v+3]=Vector3.zero;
                        continue;
                    }
                    sprites[i]=s;
                    VisibleCount++;
                    float t=Mathf.Clamp01(s.age/s.life);
                    float fade=Mathf.Clamp01(t/.19f)*Mathf.Clamp01((1f-t)/.52f);
                    float scale=centered
                        ? Mathf.Lerp(.52f,1.18f,Mathf.SmoothStep(0f,1f,t))
                        : 1f+t*.17f;
                    float radius=s.size*scale*.5f;
                    float roll=s.twist*.25f;
                    float co=Mathf.Cos(roll),si=Mathf.Sin(roll);
                    Vector3 axisX=(right*co+up*si)*radius;
                    Vector3 axisY=(-right*si+up*co)*radius;
                    Vector3 p=s.offset+s.drift*s.age;
                    vertices[v]=p-axisX-axisY;
                    vertices[v+1]=p+axisX-axisY;
                    vertices[v+2]=p-axisX+axisY;
                    vertices[v+3]=p+axisX+axisY;
                    var color=new Color(Tint.r,Tint.g,Tint.b,Tint.a*fade);
                    var packed=(Color32)color;
                    colors[v]=colors[v+1]=colors[v+2]=colors[v+3]=packed;
                }
                Renderer.enabled=VisibleCount>0;
                if(VisibleCount==0)return;
                mesh.vertices=vertices;
                mesh.colors32=colors;
            }
            public void Dispose()
            {
                if(mesh)UnityEngine.Object.Destroy(mesh);
            }
        }
    }
}

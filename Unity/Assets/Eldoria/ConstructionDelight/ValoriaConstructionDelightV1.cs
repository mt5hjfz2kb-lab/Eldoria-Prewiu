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
        float nextWorkPulse;

        SpriteField loopDust, loopMotes, openingDust, finishDust;
        SpriteField finishEmbers, finishRing, finishHalo;
        // Actual facade details, relative to the real renderer bounds.
        readonly Vector3[] facadeAccents = new Vector3[4];
        SpriteField[] allFields;
        Texture2D dustTexture, ringTexture;
        Material dustMaterial, ringMaterial;
        Transform particleRoot;
        readonly ArchitecturalCompletionLustre materialLustre = new ArchitecturalCompletionLustre();

        public int ActiveArchitecturalSurfaces => materialLustre.LayerCount;
        public string ActiveBuildingId => activeBuilding ?? "";
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
                materialLustre.Clear();
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

            if (!visible)
            {
                // Important: even when another construction is active, none of
                // the *completion* sprites may leak into the world 4X camera.
                // A hidden/resumed building keeps only the Gateway's real task.
                foreach (var field in allFields) field.Clear();
                materialLustre.Clear();
                return;
            }
            if (activeBuilding != null)
            {
                if (!loopDust.Running)
                {
                    loopDust.Play();
                    loopMotes.Play();
                }
                if (Time.unscaledTime >= nextWorkPulse)
                {
                    // Occasional short bursts suggest real medieval joinery,
                    // not a continuous cartoon smoke plume.
                    openingDust.Tint = new Color(.83f,.76f,.62f,.34f);
                    openingDust.Emit(7);
                    nextWorkPulse = Time.unscaledTime + 1.70f;
                }
            }

            var cam = city.PlayerCamera;
            float dt = Mathf.Min(.06f, Time.unscaledDeltaTime);
            foreach (var field in allFields) field.Tick(dt, cam);
            materialLustre.Tick(Time.unscaledTime);
        }

#if UNITY_EDITOR
        // The native A/B harness suppresses/restores only the temporary sheen,
        // without touching the building's original shared materials or gameplay.
        public void SetArchitecturalLustreVisibleForCapture(bool visible)
        {
            materialLustre.SetVisible(visible);
        }
#endif

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
                ? new Color(.84f,.73f,.55f,.57f) : new Color(.79f,.71f,.57f,.56f);
            loopMotes.Tint = id == "sawmill"
                ? new Color(.97f,.76f,.50f,.85f) : new Color(.90f,.76f,.59f,.78f);
            if (!CanShow()) return;

            loopDust.Play();
            loopMotes.Play();
            loopDust.Emit(24);
            loopMotes.Emit(13);
            nextWorkPulse = Time.unscaledTime + 1.55f;
            if (animateStart)
            {
                openingDust.Tint = new Color(.84f,.77f,.63f,.40f);
                openingDust.Emit(23);
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
            finishDust.Tint = new Color(.85f,.76f,.61f,.51f);
            finishEmbers.Tint = id == "bastion"
                ? new Color(1f,.90f,.64f,.88f) : new Color(1f,.82f,.50f,.82f);
            // The thin ring lies flat at the foundation like settling dust,
            // not a magic hologram floating in front of the roof.
            // No drifting geometric circle on the grass: real completion
            // instead illuminates the facade and releases fine artisan dust.
            finishRing.Tint = new Color(.91f,.79f,.59f,0f);
            finishHalo.Tint = new Color(1f,.85f,.64f,.27f);

            finishDust.Emit(29);
            finishEmbers.Emit(id == "bastion" ? 43 : 38);
            finishEmbers.EmitAccents(facadeAccents,.46f,.73f);
            finishHalo.Emit(1);
            // A brief, restrained warm highlight on the ACTUAL stone/timber
            // renderers is more satisfying than gold specks on empty grass.
            // MaterialPropertyBlocks preserve all shared assets and are restored.
            materialLustre.Begin(CompletedModel(id),Time.unscaledTime);
            // Gameplay models, levels, saves and shared materials stay unchanged.
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

            // Dust must emerge from the near-side edge of the actual worksite.
            // Its previous position was behind the building's depth-tested roof,
            // making the entire construction loop invisible in real Unity A/B.
            float frontDepth = Mathf.Abs(toward.x)*b.extents.x +
                               Mathf.Abs(toward.z)*b.extents.z;
            // Put work dust on the authentic stone/wood foundations, not
            // several metres into the grass in front of a large click target.
            float foundationOffset=frontDepth*.40f+.07f;
            Vector3 ground = new Vector3(b.center.x,b.min.y+.43f,b.center.z) +
                             toward*foundationOffset;
            Vector3 facade = new Vector3(b.center.x,b.min.y+b.size.y*.46f,b.center.z) +
                             toward*(frontDepth+.30f);
            Vector3 lateral = cam ? cam.transform.right : Vector3.right;
            lateral.y=0f;
            lateral=lateral.sqrMagnitude>.001f?lateral.normalized:Vector3.right;
            float halfWidth=Mathf.Clamp(footprint*.29f,.42f,2.25f);
            float upper=b.extents.y*.37f;
            float lower=b.extents.y*.30f;
            facadeAccents[0]=lateral*(-halfWidth)+Vector3.up*upper;
            facadeAccents[1]=lateral*halfWidth+Vector3.up*upper;
            facadeAccents[2]=lateral*(-halfWidth*.67f)-Vector3.up*lower;
            facadeAccents[3]=lateral*(halfWidth*.67f)-Vector3.up*lower;
            loopDust.Site(ground,footprint*.77f);
            loopMotes.Site(ground+Vector3.up*.24f,footprint*.69f);
            openingDust.Site(ground,footprint*.84f);
            finishDust.Site(ground,footprint*.86f);
            finishEmbers.Site(facade,footprint*.85f);
            // A foundation-aligned sand ripple, not a screen-facing roof ring.
            finishRing.Site(ground+Vector3.up*.04f,0f);
            finishHalo.Site(facade,0f);
            finishRing.BaseSize=Mathf.Clamp(footprint*1.05f,1.8f,7.2f);
            finishHalo.BaseSize=Mathf.Clamp(footprint*.99f,1.6f,6.3f);
            return true;
        }

        bool FindSite(string id, bool preferModel, out Bounds bounds)
        {
            bounds=default;
            if (preferModel && ModelBounds(id,out bounds)) return true;
            Collider target=null;
            var parcels=city.Parcels;
            if(parcels && !preferModel)
            {
                // Canonical City state has already enabled the real scaffolding.
                // Bounds of visible geometry are much tighter than the large
                // invisible touch-target collider used for selecting the parcel.
                GameObject scaffold=id=="sawmill" ? parcels.LeftConstruction :
                                    id=="barracks" ? parcels.RightConstruction : null;
                if(VisibleBounds(scaffold,out bounds))return true;
                // During an upgrade, the built structure remains visible.
                if(ModelBounds(id,out bounds))return true;
            }
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
            if(!ColliderUsable(target)) target=null;
            // The canonical integrated scene does not necessarily parent its
            // WorldHotspots under ValoriaReferenceCleanGameplay. The authoritative
            // host already indexes real game targets; reuse it, not guessed coords.
            if(!target && integrated && integrated.Ready)
            {
                var hotspot=integrated.FindTarget(id);
                if(hotspot && hotspot.TryGetComponent(out Collider found) &&
                   ColliderUsable(found)) target=found;
            }
            if(!target)
                foreach(var point in GetComponentsInChildren<WorldHotspot>(true))
                    if(point && point.Id==id && point.TryGetComponent(out Collider found) &&
                       ColliderUsable(found))
                    {
                        target=found;
                        break;
                    }
            if(ColliderUsable(target))
            {
                bounds=target.bounds;
                return true;
            }
            // During a genuine first construction the future built model is
            // intentionally inactive. Use the visible scaffold or parcel ground
            // from the same real scene, never a placeholder world position.
            if(parcels)
            {
                GameObject construction=id=="sawmill" ? parcels.LeftConstruction :
                                        id=="barracks" ? parcels.RightConstruction : null;
                GameObject ground=id=="sawmill" ? parcels.LeftGround :
                                  id=="barracks" ? parcels.RightGround : null;
                if(VisibleBounds(construction,out bounds) ||
                   VisibleBounds(ground,out bounds)) return true;
            }
            return ModelBounds(id,out bounds);
        }

        static bool ColliderUsable(Collider c)
        {
            return c && c.enabled && c.gameObject.activeInHierarchy &&
                   c.bounds.size.sqrMagnitude>.001f;
        }

        static bool VisibleBounds(GameObject model, out Bounds bounds)
        {
            bounds=default;
            if(!model || !model.activeInHierarchy)return false;
            bool found=false;
            foreach(var renderer in model.GetComponentsInChildren<Renderer>())
            {
                if(!renderer || !renderer.enabled)continue;
                if(!found){bounds=renderer.bounds;found=true;}
                else bounds.Encapsulate(renderer.bounds);
            }
            return found && bounds.size.sqrMagnitude>.001f;
        }

        GameObject CompletedModel(string id)
        {
            var parcels=city.Parcels;
            if(id=="bastion")
                return city.BastionTwo && city.BastionTwo.activeInHierarchy
                    ? city.BastionTwo : city.BastionOne;
            if(!parcels)return null;
            return id=="sawmill" ? parcels.LeftBuilt :
                   id=="barracks" ? parcels.RightBuilt : null;
        }

        bool ModelBounds(string id, out Bounds bounds)
        {
            bounds=default;
            GameObject model=CompletedModel(id);
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
            // Larger soft puffs at the REAL construction perimeter preserve
            // legibility in the distant mobile camera without noisy confetti.
            loopDust=NewField("Work / fine masonry dust",dustMaterial,65,9.2f,.70f,1.34f,.49f,1.05f,.24f,false,19);
            loopMotes=NewField("Work / timber motes",dustMaterial,34,5.2f,.57f,1.03f,.23f,.45f,.44f,false,27);
            openingDust=NewField("Start / settling artisan dust",dustMaterial,34,0f,.42f,.95f,.60f,1.16f,.27f,false,39);
            finishDust=NewField("Finish / last settling dust",dustMaterial,45,0f,.55f,1.10f,.40f,.91f,.38f,false,43);
            finishEmbers=NewField("Finish / antique-gold glints",dustMaterial,70,0f,.58f,1.18f,.15f,.31f,.66f,false,57);
            finishRing=NewField("Finish / foundation settling wave",ringMaterial,2,0f,1.28f,1.28f,1f,1f,0f,true,79,true);
            finishHalo=NewField("Finish / warm timber reveal",dustMaterial,2,0f,1.12f,1.12f,1f,1f,0f,true,83);
            allFields=new[]{loopDust,loopMotes,openingDust,finishDust,finishEmbers,finishRing,finishHalo};
        }

        SpriteField NewField(string name,Material material,int cap,float rate,
            float lifeLow,float lifeHigh,float sizeLow,float sizeHigh,float up,
            bool fixedCenter,int seed,bool flatOnFoundation=false)
        {
            return new SpriteField(particleRoot,name,material,cap,rate,lifeLow,lifeHigh,
                                   sizeLow,sizeHigh,up,fixedCenter,seed,flatOnFoundation);
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
            materialLustre.Clear();
            if(allFields!=null)foreach(var field in allFields)field.Dispose();
            if(dustMaterial)Destroy(dustMaterial);
            if(ringMaterial)Destroy(ringMaterial);
            if(dustTexture)Destroy(dustTexture);
            if(ringTexture)Destroy(ringTexture);
        }

        // Real geometry-based reward: a brief antique-gold sheen on the
        // existing building's original URP surfaces. No camera stickers, lights,
        // new meshes, shader keywords, scene edits or material instantiation.
        sealed class ArchitecturalCompletionLustre
        {
            struct Layer
            {
                public Renderer renderer;
                public int materialIndex;
                public MaterialPropertyBlock original;
                public MaterialPropertyBlock animated;
                public Color source;
            }
            const float Duration=1.45f;
            static readonly int BaseColorId=Shader.PropertyToID("_BaseColor");
            readonly System.Collections.Generic.List<Layer> layers =
                new System.Collections.Generic.List<Layer>(48);
            float startedAt;
            bool shown=true;
            public int LayerCount => layers.Count;

            public void Begin(GameObject building,float now)
            {
                Clear();
                if(!building || !building.activeInHierarchy)return;
                foreach(var renderer in building.GetComponentsInChildren<Renderer>())
                {
                    if(!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy)continue;
                    var materials=renderer.sharedMaterials;
                    for(int i=0;i<materials.Length;i++)
                    {
                        var material=materials[i];
                        if(!material || !material.HasProperty(BaseColorId) ||
                           material.renderQueue >= (int)RenderQueue.Transparent)continue;
                        var original=new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(original,i);
                        // Existing animated property blocks must not be overridden.
                        if(!original.isEmpty)continue;
                        layers.Add(new Layer{
                            renderer=renderer,materialIndex=i,
                            original=original,animated=new MaterialPropertyBlock(),
                            source=material.GetColor(BaseColorId)
                        });
                    }
                }
                startedAt=now;
                shown=true;
                Tick(now+.02f);
            }

            public void SetVisible(bool visible)
            {
                shown=visible;
                if(!visible)Restore();
                else Tick(Time.unscaledTime);
            }

            public void Tick(float now)
            {
                if(layers.Count==0)return;
                float age=now-startedAt;
                if(age>=Duration){Clear();return;}
                if(!shown)return;
                float time=Mathf.Clamp01(age/Duration);
                // Gradual warm rise and fall. Keeps the original wood/stone
                // textures and light response, without a bright yellow flash.
                float warmth=Mathf.Sin(Mathf.PI*time);
                float red=1f+warmth*.31f;
                float green=1f+warmth*.13f;
                float blue=1f+warmth*.015f;
                foreach(var l in layers)
                {
                    if(!l.renderer)continue;
                    l.animated.SetColor(BaseColorId,new Color(
                        l.source.r*red,l.source.g*green,
                        l.source.b*blue,l.source.a));
                    l.renderer.SetPropertyBlock(l.animated,l.materialIndex);
                }
            }

            void Restore()
            {
                foreach(var l in layers)
                    if(l.renderer)l.renderer.SetPropertyBlock(l.original,l.materialIndex);
            }

            public void Clear()
            {
                Restore();
                layers.Clear();
                shown=true;
            }
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
            readonly bool centered, groundAligned;
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
                float sizeLow,float sizeHigh,float up,bool fixedCenter,int seed,
                bool flatOnFoundation=false)
            {
                capacity=cap;rate=spawnRate;minLife=lifeLow;maxLife=lifeHigh;
                minSize=sizeLow;maxSize=sizeHigh;upward=up;centered=fixedCenter;
                groundAligned=flatOnFoundation;
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

            // Place a few larger glints on real facade beam locations using
            // a single existing mesh batch (no additional draw calls).
            public void EmitAccents(Vector3[] offsets,float sizeMin,float sizeMax)
            {
                if(offsets==null)return;
                foreach(var at in offsets)
                {
                    int slot=cursor++%capacity;
                    sprites[slot]=new Sprite{
                        live=true,offset=at,
                        drift=new Vector3(0f,RandomBetween(.08f,.17f),0f),
                        age=0f,life=RandomBetween(.56f,.92f),
                        size=RandomBetween(sizeMin,sizeMax),
                        twist=RandomBetween(-.8f,.8f)
                    };
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
                var right=groundAligned?Vector3.right:
                          (cam?cam.transform.right:Vector3.right);
                var up=groundAligned?Vector3.forward:
                       (cam?cam.transform.up:Vector3.up);
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

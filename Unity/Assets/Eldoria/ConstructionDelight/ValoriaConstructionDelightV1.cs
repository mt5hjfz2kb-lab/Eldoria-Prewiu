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
        float nextStageDustAt,lateFinishBurstAt;
        bool lateFinishBurstPending;
        int previousStagedVisibleParts;

        SpriteField loopDust, loopMotes, openingDust, finishDust;
        SpriteField denseWorkDust, hammerImpactDust;
        SpriteField finishEmbers, finishRing, finishHalo, roofGlints, roofGleam;
        // Actual facade details, relative to the real renderer bounds.
        readonly Vector3[] facadeAccents = new Vector3[4];
        readonly Vector3[] roofAccents = new Vector3[4];
        SpriteField[] allFields;
        Texture2D dustTexture, ringTexture, denseTexture;
        Material dustMaterial, ringMaterial, denseMaterial;
        Transform particleRoot;
        readonly ArchitecturalCompletionLustre materialLustre = new ArchitecturalCompletionLustre();
        readonly ConstructionDelightArchitecturalReveal structureReveal =
            new ConstructionDelightArchitecturalReveal();
        readonly ConstructionDelightStageAssembly stagedBuilding =
            new ConstructionDelightStageAssembly();
        readonly ConstructionDelightHammerWork hammerTools =
            new ConstructionDelightHammerWork();
#if UNITY_EDITOR
        // Editor-only control to compare exactly the same animated construction
        // with/without staged 3D, using equivalent shader/Bloom timelines.
        readonly ConstructionDelightArchitecturalReveal architectureOnlyReveal =
            new ConstructionDelightArchitecturalReveal();
        readonly ArchitecturalCompletionLustre architectureOnlyLustre =
            new ArchitecturalCompletionLustre();
#endif

        public int ActiveHammerRenderers => hammerTools.VisibleRenderers;
        public int VerifiedHammerContacts => hammerTools.AnchoredContactCount;
        public float HammerContactHeightFraction => hammerTools.MinimumContactHeightFraction;
        public int TotalHammerImpacts => hammerTools.TotalImpacts;
        public int ActiveDenseConstructionDust => (denseWorkDust?.VisibleCount ?? 0) +
                                                   (hammerImpactDust?.VisibleCount ?? 0);
        public int ActiveStagedVisibleMeshes => stagedBuilding.VisibleMeshCount;
        public bool StagingContinuedAtCompletion => stagedBuilding.CompletionContinuedFromWork;
        public int ActiveStagedBuildingMeshes => stagedBuilding.MeshCount;
        public int ActiveStructuralRevealMeshes => structureReveal.MeshCount;
#if UNITY_EDITOR
        public bool BothTechniqueBloomProfilesReady =>
            structureReveal.HasBloomVolume && architectureOnlyReveal.HasBloomVolume;
#endif
        public int ActiveArchitecturalSurfaces => materialLustre.LayerCount;
        public string ActiveBuildingId => activeBuilding ?? "";
        public int ActiveConstructionSprites => (loopDust?.VisibleCount ?? 0) +
                                                 (loopMotes?.VisibleCount ?? 0) +
                                                 (denseWorkDust?.VisibleCount ?? 0) +
                                                 (hammerImpactDust?.VisibleCount ?? 0);
        public int ActiveFinishSprites => (finishDust?.VisibleCount ?? 0) +
                                         (finishEmbers?.VisibleCount ?? 0) +
                                         (finishRing?.VisibleCount ?? 0) +
                                         (finishHalo?.VisibleCount ?? 0) +
                                         (roofGlints?.VisibleCount ?? 0) +
                                         (roofGleam?.VisibleCount ?? 0);

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
                structureReveal.Clear();
                stagedBuilding.Clear();
                hammerTools.Clear();
#if UNITY_EDITOR
                architectureOnlyReveal.Clear();
                architectureOnlyLustre.Clear();
#endif
                lateFinishBurstPending=false;
                previousStagedVisibleParts=0;
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
                structureReveal.Clear();
                stagedBuilding.Clear();
                hammerTools.Clear();
#if UNITY_EDITOR
                architectureOnlyReveal.Clear();
                architectureOnlyLustre.Clear();
#endif
                lateFinishBurstPending=false;
                previousStagedVisibleParts=0;
                return;
            }
            if (activeBuilding != null)
            {
                // On return from 4X the original Gateway task is still active.
                // Recreate only the ephemeral artisan hammer animation.
                if(!hammerTools.Running && FindSite(activeBuilding,false,out var worksite))
                    hammerTools.Begin(worksite,city.PlayerCamera,Time.unscaledTime,
                                      WorksiteModel(activeBuilding));
                if(!stagedBuilding.Running && previous.IsValid &&
                   previous.LevelAtStart==0)
                    stagedBuilding.BeginWork(CompletedModel(previous.BuildingId),
                        previous.DeadlineUtcTicks,city.Clock.UtcTicks,
                        WorkDuration(previous),Time.unscaledTime);
                if (!loopDust.Running)
                {
                    loopDust.Play();
                    loopMotes.Play();
                }
                // Returning from 4X clears temporary graphics but the real
                // task continues. Rebuild the worksite highlight from the
                // authoritative city scene, never from a synthetic timer.
                if(structureReveal.MeshCount==0)
                    structureReveal.BeginWork(WorksiteModel(activeBuilding),Time.unscaledTime);
                if (Time.unscaledTime >= nextWorkPulse)
                {
                    // Occasional short bursts suggest real medieval joinery,
                    // not a continuous cartoon smoke plume.
                    openingDust.Tint = new Color(.83f,.76f,.62f,.34f);
                    openingDust.Emit(7);
                    if(hammerTools.Running)
                        denseWorkDust.EmitAtWorld(hammerTools.PrimaryContact,28,.46f);
                    nextWorkPulse = Time.unscaledTime + 1.70f;
                }
            }

            var cam = city.PlayerCamera;
            float dt = Mathf.Min(.06f, Time.unscaledDeltaTime);
            // Real iron-on-timber contact moments drive each short cloud:
            // no magical blanket of golden dust and no second gameplay clock.
            int hammerHits=hammerTools.Tick(Time.unscaledTime);
            if(hammerHits>0)
            {
                hammerImpactDust.EmitAtWorld(hammerTools.FirstImpact,28,.28f);
                denseWorkDust.EmitAtWorld(hammerTools.FirstImpact,38,.45f);
                loopMotes.EmitAtWorld(hammerTools.FirstImpact,4,.22f);
            }
            if(hammerHits>1)
            {
                hammerImpactDust.EmitAtWorld(hammerTools.SecondImpact,27,.28f);
                denseWorkDust.EmitAtWorld(hammerTools.SecondImpact,36,.45f);
                loopMotes.EmitAtWorld(hammerTools.SecondImpact,3,.20f);
            }
            foreach (var field in allFields) field.Tick(dt, cam);
            stagedBuilding.Tick(city.Clock.UtcTicks,Time.unscaledTime);
            int currentParts=stagedBuilding.Working ? stagedBuilding.VisibleMeshCount : 0;
            if(activeBuilding!=null && currentParts>previousStagedVisibleParts &&
               Time.unscaledTime>=nextStageDustAt)
            {
                // Wood or stone genuinely settled into the unfinished model.
                // A short puff at the scaffold foot follows that placement.
                openingDust.Emit(5);
                // A new genuine timber/stone section has seated; it should
                // release a visible, locally contained puff at the join.
                if(hammerTools.Running)
                    denseWorkDust.EmitAtWorld(hammerTools.PrimaryContact,25,.42f);
                nextStageDustAt=Time.unscaledTime+.38f;
            }
            previousStagedVisibleParts=currentParts;
            if(lateFinishBurstPending && Time.unscaledTime>=lateFinishBurstAt)
            {
                // The final roof/beam seating deserves the short glint, not
                // an unrelated full-screen flare at the start of completion.
                roofGlints.EmitAccents(roofAccents,.22f,.36f);
                finishDust.Emit(7);
                lateFinishBurstPending=false;
            }
            materialLustre.Tick(Time.unscaledTime);
            structureReveal.Tick(Time.unscaledTime);
#if UNITY_EDITOR
            architectureOnlyLustre.Tick(Time.unscaledTime);
            architectureOnlyReveal.Tick(Time.unscaledTime);
#endif
        }

#if UNITY_EDITOR
        // The native A/B harness suppresses/restores only the temporary sheen,
        // without touching the building's original shared materials or gameplay.
        public void SetArchitecturalLustreVisibleForCapture(bool visible)
        {
            stagedBuilding.SetVisible(visible);
            hammerTools.SetVisible(visible);
            if(city && city.Clock!=null)
                stagedBuilding.Tick(city.Clock.UtcTicks,Time.unscaledTime);
            materialLustre.SetVisible(visible);
            structureReveal.SetVisible(visible,Time.unscaledTime);
        }

        /// <summary>
        /// Same real task and fixed camera, same sprites and synchronized
        /// architectural shader/Bloom; only 3D staged placement is toggled.
        /// Unity Editor-only API, excluded from builds and production gameplay.
        /// </summary>
        public void SetTechniqueForCapture(bool useStagedAssembly)
        {
            float now=Time.unscaledTime;
            bool isCompletion=stagedBuilding.Running && !stagedBuilding.Working;
            if(useStagedAssembly)
            {
                architectureOnlyReveal.SetVisible(false,now);
                architectureOnlyLustre.SetVisible(false);
                stagedBuilding.SetVisible(true);
                if(city && city.Clock!=null)
                    stagedBuilding.Tick(city.Clock.UtcTicks,now);
                structureReveal.SetVisible(true,now);
                materialLustre.SetVisible(true);
            }
            else
            {
                structureReveal.SetVisible(!isCompletion,now);
                materialLustre.SetVisible(false);
                stagedBuilding.SetVisible(false);
                if(city && city.Clock!=null)
                    stagedBuilding.Tick(city.Clock.UtcTicks,now);
                architectureOnlyReveal.SetVisible(isCompletion,now);
                architectureOnlyLustre.SetVisible(isCompletion);
            }
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
                if (hasTask) StartWork(current, false);
                return;
            }
            bool same = hasTask && ConstructionDelightState.SameTask(previous, current);
            if (previous.IsValid && !same)
            {
                var finished = previous;
                bool actualCompletion=ConstructionDelightState.JustCompleted(finished,state);
                bool preserveStage=actualCompletion && CanShow();
                StopWork(preserveStage);
                if (actualCompletion)
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
            if (hasTask && !same) StartWork(current, true);
            else if (hasTask && activeBuilding == null) StartWork(current, false);
            previous = hasTask ? current : default;
        }

        void StartWork(ConstructionDelightTask task, bool animateStart)
        {
            string id=task.BuildingId;
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
            structureReveal.BeginWork(WorksiteModel(id),Time.unscaledTime);
            if(task.LevelAtStart==0)
                stagedBuilding.BeginWork(CompletedModel(id),task.DeadlineUtcTicks,
                    city.Clock.UtcTicks,WorkDuration(task),Time.unscaledTime);
            if(FindSite(id,false,out var hammerSite))
                hammerTools.Begin(hammerSite,city.PlayerCamera,Time.unscaledTime,
                                  WorksiteModel(id));
            if(hammerTools.Running)
                denseWorkDust.EmitAtWorld(hammerTools.PrimaryContact,35,.46f);
            previousStagedVisibleParts=stagedBuilding.VisibleMeshCount;
            nextStageDustAt=Time.unscaledTime+.38f;
            nextWorkPulse = Time.unscaledTime + 1.55f;
            if (animateStart)
            {
                openingDust.Tint = new Color(.84f,.77f,.63f,.40f);
                openingDust.Emit(23);
                finishEmbers.Tint = new Color(1f,.78f,.45f,.27f);
                finishEmbers.Emit(4);
            }
        }

        void StopWork(bool carryAuthenticAssembly=false)
        {
            activeBuilding = null;
            if (loopDust != null) loopDust.Clear();
            if (loopMotes != null) loopMotes.Clear();
            if (denseWorkDust != null) denseWorkDust.Clear();
            if (hammerImpactDust != null) hammerImpactDust.Clear();
            hammerTools.Clear();
            structureReveal.Clear();
            // A real gateway completion must keep the in-progress mesh poses:
            // clearing here would reset the roof/foundation choreography.
            if(!carryAuthenticAssembly)stagedBuilding.Clear();
            previousStagedVisibleParts=0;
            lateFinishBurstPending=false;
        }

        static float WorkDuration(ConstructionDelightTask task)
        {
            if(task.TaskKey.StartsWith("c3:",StringComparison.Ordinal))
                return task.LevelAtStart==0 ?
                    (task.BuildingId=="stoneworks" ? 12f:9f) :
                    (8f+task.LevelAtStart*2f);
            if(task.BuildingId=="sawmill")return SliceRules.SawmillBuildSeconds;
            if(task.BuildingId=="barracks")return SliceRules.BarracksBuildSeconds;
            return 20f;
        }

        void Complete(string id)
        {
            if (!CanShow() || !PositionSite(id, true))
            {
                stagedBuilding.Clear(); // no orphan presentation on missing site
                return;
            }
            finishDust.Tint = new Color(.85f,.76f,.61f,.51f);
            finishEmbers.Tint = id == "bastion"
                ? new Color(1f,.90f,.64f,.88f) : new Color(1f,.82f,.50f,.82f);
            // The thin ring lies flat at the foundation like settling dust,
            // not a magic hologram floating in front of the roof.
            // No drifting geometric circle on the grass: real completion
            // instead illuminates the facade and releases fine artisan dust.
            finishRing.Tint = new Color(.91f,.79f,.59f,0f);
            finishHalo.Tint = new Color(1f,.87f,.67f,.33f);
            roofGlints.Tint = new Color(1f,.82f,.55f,.79f);
            roofGleam.Tint = new Color(1f,.88f,.68f,.32f);

            finishDust.Emit(29);
            finishEmbers.Emit(id == "bastion" ? 43 : 38);
            finishEmbers.EmitAccents(facadeAccents,.46f,.73f);
            roofGlints.Emit(12);
            roofGlints.EmitAccents(roofAccents,.28f,.47f);
            roofGleam.Emit(1);
            finishHalo.Emit(1);
            // A brief, restrained warm highlight on the ACTUAL stone/timber
            // renderers is more satisfying than gold specks on empty grass.
            // MaterialPropertyBlocks preserve all shared assets and are restored.
            var completed=CompletedModel(id);
            bool staged=stagedBuilding.Complete(completed,Time.unscaledTime);
            lateFinishBurstPending=staged;
            lateFinishBurstAt=Time.unscaledTime+.91f;
            var visual=staged ? stagedBuilding.VisualRoot : completed;
            // Include currently hidden roof copies so their real material
            // acquires the warm finishing sweep WHEN the roof lands.
            materialLustre.Begin(visual,Time.unscaledTime,staged);
            structureReveal.Complete(visual,city.PlayerCamera,Time.unscaledTime,staged);
#if UNITY_EDITOR
            if(city && city.Probe)
            {
                // During creation inspect only the original renderers that
                // genuinely belong to the completed production building.
                // Temporarily restore their original enabled flags, rather
                // than using includeStagedParts=true (which could also include
                // normally hidden alternate mesh variants).
                if(staged)stagedBuilding.SetVisible(false);
                try
                {
                    architectureOnlyLustre.Begin(completed,Time.unscaledTime);
                    architectureOnlyReveal.Complete(completed,city.PlayerCamera,
                        Time.unscaledTime);
                }
                finally
                {
                    architectureOnlyLustre.SetVisible(false);
                    architectureOnlyReveal.SetVisible(false,Time.unscaledTime);
                    if(staged)stagedBuilding.SetVisible(true);
                }
            }
#endif
            stagedBuilding.Tick(city.Clock.UtcTicks,Time.unscaledTime);
            structureReveal.Tick(Time.unscaledTime);
            // A reversible 3D timber-and-roof landing; all source flags restore.
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
            Vector3 facade = new Vector3(b.center.x,b.min.y+b.size.y*.60f,b.center.z) +
                             toward*(frontDepth+.30f);
            // Camera-side gold glints follow the true roof and front gable,
            // not the empty foreground soil.
            Vector3 roofFront=new Vector3(b.center.x,b.min.y+b.size.y*.84f,b.center.z)+
                toward*(frontDepth+.36f);
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
            roofAccents[0]=lateral*(-halfWidth*.93f)-Vector3.up*(b.extents.y*.17f);
            roofAccents[1]=lateral*(halfWidth*.93f)-Vector3.up*(b.extents.y*.17f);
            roofAccents[2]=lateral*(-halfWidth*.50f)+Vector3.up*(b.extents.y*.10f);
            roofAccents[3]=lateral*(halfWidth*.50f)+Vector3.up*(b.extents.y*.10f);
            loopDust.Site(ground,footprint*.77f);
            loopMotes.Site(ground+Vector3.up*.24f,footprint*.69f);
            denseWorkDust.Site(ground+Vector3.up*.10f,footprint*.61f);
            hammerImpactDust.Site(ground+Vector3.up*.18f,footprint*.53f);
            openingDust.Site(ground,footprint*.84f);
            finishDust.Site(ground,footprint*.86f);
            finishEmbers.Site(facade,footprint*.85f);
            // A foundation-aligned sand ripple, not a screen-facing roof ring.
            finishRing.Site(ground+Vector3.up*.04f,0f);
            finishHalo.Site(roofFront,0f);
            roofGlints.Site(roofFront,footprint*.48f);
            roofGleam.Site(roofFront,0f);
            finishRing.BaseSize=Mathf.Clamp(footprint*1.05f,1.8f,7.2f);
            finishHalo.BaseSize=Mathf.Clamp(footprint*.85f,1.5f,5.6f);
            roofGleam.BaseSize=Mathf.Clamp(footprint*.70f,1.3f,4.4f);
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
                if(VisibleBounds(WorksiteModel(id),out bounds))return true;
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
                GameObject ground=id=="sawmill" ? parcels.LeftGround :
                                  id=="barracks" ? parcels.RightGround : null;
                if(!ground && parcels.Bindings!=null)
                    foreach(var binding in parcels.Bindings)
                        if(binding!=null && binding.BuildingId==id && binding.Ground)
                        {
                            ground=binding.Ground;
                            break;
                        }
                if(VisibleBounds(WorksiteModel(id),out bounds) ||
                   VisibleBounds(ground,out bounds))return true;
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

        GameObject WorksiteModel(string id)
        {
            var parcels=city.Parcels;
            if(parcels)
            {
                GameObject scaffold=id=="sawmill" ? parcels.LeftConstruction :
                                    id=="barracks" ? parcels.RightConstruction : null;
                if(!scaffold && parcels.Bindings!=null)
                    foreach(var binding in parcels.Bindings)
                        if(binding!=null && binding.BuildingId==id && binding.Construction)
                        {
                            scaffold=binding.Construction;
                            break;
                        }
                if(scaffold && scaffold.activeInHierarchy)return scaffold;
            }
            // On upgrades the existing structure may remain visible while
            // its real timer runs. Highlight those surfaces, never a fake asset.
            return CompletedModel(id);
        }

        GameObject CompletedModel(string id)
        {
            var parcels=city.Parcels;
            if(id=="bastion")
                return city.BastionTwo && city.BastionTwo.activeInHierarchy
                    ? city.BastionTwo : city.BastionOne;
            if(!parcels)return null;
            if(id=="sawmill")return parcels.LeftBuilt;
            if(id=="barracks")return parcels.RightBuilt;
            // Chapter III plots use the same authoritative parcel bindings;
            // never leave real granary/stoneworks upgrades without art.
            if(parcels.Bindings!=null)
                foreach(var binding in parcels.Bindings)
                    if(binding!=null && binding.BuildingId==id && binding.Built)
                        return binding.Built;
            return null;
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
            denseTexture=MakeDenseDustTexture();
            dustMaterial=MakeMaterial("Eldoria / artisan dust",dustTexture);
            ringMaterial=MakeMaterial("Eldoria / worn-gold aureole",ringTexture);
            denseMaterial=MakeMaterial("Eldoria / soft irregular masonry cloud",denseTexture);
            if(!dustMaterial || !ringMaterial || !denseMaterial)
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
            // Two additional BATCHED atmospheric fields, not hundreds of
            // renderer GameObjects. High density near the true hammer contact,
            // but softly feathered and short lived to keep wood and roof legible.
            denseWorkDust=NewField("Work / dense scaffold dust",denseMaterial,150,0f,.60f,1.08f,.50f,1.10f,.17f,false,113);
            hammerImpactDust=NewField("Work / hammer impact puffs",denseMaterial,110,0f,.45f,.82f,.36f,.83f,.23f,false,127);
            denseWorkDust.Tint=new Color(.78f,.71f,.60f,.77f);
            hammerImpactDust.Tint=new Color(.83f,.75f,.63f,.79f);
            finishDust=NewField("Finish / last settling dust",dustMaterial,45,0f,.55f,1.10f,.40f,.91f,.38f,false,43);
            finishEmbers=NewField("Finish / antique-gold glints",dustMaterial,70,0f,.58f,1.18f,.15f,.31f,.66f,false,57);
            finishRing=NewField("Finish / foundation settling wave",ringMaterial,2,0f,1.28f,1.28f,1f,1f,0f,true,79,true);
            finishHalo=NewField("Finish / warm timber reveal",dustMaterial,2,0f,1.12f,1.12f,1f,1f,0f,true,83);
            roofGlints=NewField("Finish / artisan roof accents",dustMaterial,30,0f,.73f,1.32f,.13f,.25f,.30f,false,97);
            roofGleam=NewField("Finish / soft rooflight reveal",dustMaterial,2,0f,1.28f,1.28f,1f,1f,0f,true,103);
            allFields=new[]{loopDust,loopMotes,openingDust,finishDust,finishEmbers,finishRing,finishHalo,roofGlints,roofGleam,denseWorkDust,hammerImpactDust};
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

        // Same low-cost URP quad batches, but the denser work dust gets a
        // dedicated feathered cloud texture. The soft Perlin-distorted rim
        // breaks the obvious circular sprite silhouette; no disk assets or
        // heavy smoke simulation, only a one-time 64x64 procedural texture.
        static Texture2D MakeDenseDustTexture()
        {
            const int n=64;
            var texture=new Texture2D(n,n,TextureFormat.RGBA32,false,true)
            {
                name="Eldoria / irregular artisan masonry plume",
                filterMode=FilterMode.Bilinear,
                wrapMode=TextureWrapMode.Clamp
            };
            var colors=new Color[n*n];
            for(int y=0;y<n;y++)
                for(int x=0;x<n;x++)
                {
                    float u=(x+.5f)/n, v=(y+.5f)/n;
                    float px=2f*u-1f, py=2f*v-1f;
                    float radius=Mathf.Sqrt(px*px+py*py);
                    float clouds=Mathf.PerlinNoise(1.41f+u*3.4f,2.38f+v*3.4f);
                    float grain=Mathf.PerlinNoise(5.13f+u*8.2f,7.82f+v*8.2f);
                    // Multi-lobed anisotropic silhouette plus cellular opacity. Large
                    // overlapping circular alpha disks made V6 a single
                    // beige blob; this fragments the edge AND the interior.
                    float theta=Mathf.Atan2(py,px);
                    float outline=.78f+.12f*Mathf.Sin(theta*3f+.7f)+
                        .075f*Mathf.Sin(theta*5f-1.4f)+.17f*(clouds-.5f);
                    float elliptical=Mathf.Sqrt(px*px*.88f+py*py*1.12f);
                    float feather=Mathf.SmoothStep(0f,1f,
                        Mathf.Clamp01((outline-elliptical)/.33f));
                    // V7's tiny dark fragments disappeared at mobile distance.
                    // Restore enough density to see real joinery dust without
                    // the circular opaque disks from V6.
                    float opacity=feather*(.44f+.46f*grain)*
                                  (.76f+.24f*clouds);
                    colors[y*n+x]=new Color(1f,1f,1f,opacity);
                }
            texture.SetPixels(colors);
            texture.Apply(false,true);
            return texture;
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
            structureReveal.Clear();
            materialLustre.Clear();
            stagedBuilding.Clear();
            hammerTools.Clear();
#if UNITY_EDITOR
            architectureOnlyReveal.Clear();
            architectureOnlyLustre.Clear();
#endif
            if(allFields!=null)foreach(var field in allFields)field.Dispose();
            if(dustMaterial)Destroy(dustMaterial);
            if(ringMaterial)Destroy(ringMaterial);
            if(denseMaterial)Destroy(denseMaterial);
            if(dustTexture)Destroy(dustTexture);
            if(ringTexture)Destroy(ringTexture);
            if(denseTexture)Destroy(denseTexture);
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

            public void Begin(GameObject building,float now,bool includeStagedParts=false)
            {
                Clear();
                if(!building || !building.activeInHierarchy)return;
                foreach(var renderer in building.GetComponentsInChildren<Renderer>())
                {
                    if(!renderer || (!renderer.enabled && !includeStagedParts) ||
                       !renderer.gameObject.activeInHierarchy)continue;
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
                float red=1f+warmth*.40f;
                float green=1f+warmth*.19f;
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

            // A dense but very local layered cloud at a REAL strike point.
            // Keep each sprite's position in the field's stable local space,
            // so earlier puffs never teleport when another hammer strikes.
            // No per-hit objects, meshes, textures, physics or garbage.
            public void EmitAtWorld(Vector3 world,int count,float radius)
            {
                Vector3 local=holder.InverseTransformPoint(world);
                float spread=Mathf.Max(.02f,radius);
                for(int i=0;i<count;i++)
                {
                    int slot=cursor++%capacity;
                    float angle=RandomBetween(0f,Mathf.PI*2f);
                    float distance=Mathf.Sqrt((float)Random01())*spread;
                    var at=local+new Vector3(
                        Mathf.Cos(angle)*distance,
                        RandomBetween(-.035f,.10f),
                        Mathf.Sin(angle)*distance*.64f);
                    var drift=new Vector3(
                        RandomBetween(-.12f,.12f),
                        RandomBetween(upward*.65f,upward*1.35f),
                        RandomBetween(-.11f,.11f));
                    sprites[slot]=new Sprite{
                        live=true,offset=at,drift=drift,
                        age=0f,life=RandomBetween(minLife,maxLife),
                        size=RandomBetween(minSize,maxSize),
                        twist=RandomBetween(-1.4f,1.4f)
                    };
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
                        : Mathf.Lerp(.86f,1.12f,Mathf.SmoothStep(0f,1f,t));
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

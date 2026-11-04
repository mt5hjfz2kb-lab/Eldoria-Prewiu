using System;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.ReferenceClean
{
    /// <summary>
    /// Self-attaching, read-only art layer over the actual saved Valoria city.
    /// Does not modify gameplay, prefabs, cameras, colliders, buttons or timers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ValoriaConstructionDelightV1 : MonoBehaviour
    {
        const float PollSeconds = .18f;
        const float CompletionSeconds = 1.70f;
        const float WorldReturnGraceSeconds = 14f;
        static bool registered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Register()
        {
            if (!registered)
            {
                SceneManager.sceneLoaded += SceneLoaded;
                registered = true;
            }
            Attach();
        }

        static void SceneLoaded(Scene scene, LoadSceneMode mode) => Attach();

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
        float finishUntil;
        string workBuildingId;
        bool pendingCompletion;
        string pendingId;
        float pendingExpiresAt;

        ParticleSystem loopDust, loopMotes, openingDust;
        ParticleSystem finishDust, finishEmbers, finishRing, finishHalo;
        Texture2D dustTexture, ringTexture;
        Material dustMaterial, ringMaterial;
        Transform particleRoot;

        void Awake()
        {
            city = GetComponent<ValoriaReferenceCleanGameplay>();
            integrated = GetComponent<ValoriaIntegratedWorld>();
            InitializeArt();
        }

        void Update()
        {
            if (!city || !city.Ready || city.Gateway == null || !dustMaterial) return;
            if (!ReferenceEquals(observedGateway, city.Gateway))
            {
                observedGateway = city.Gateway;
                previous = default;
                initialized = false;
                pendingCompletion = false;
                StopLoop();
            }

            if (Time.unscaledTime >= nextObservation)
            {
                nextObservation = Time.unscaledTime + PollSeconds;
                Observe(city.Gateway.Snapshot());
            }

            if (pendingCompletion && CanShow())
            {
                if (Time.unscaledTime <= pendingExpiresAt) Complete(pendingId);
                pendingCompletion = false;
            }

            if (workBuildingId != null)
            {
                if (CanShow() && !loopDust.isPlaying)
                {
                    loopDust.Play();
                    loopMotes.Play();
                }
                else if (!CanShow() && loopDust.isPlaying)
                    SuspendLoop();
            }

            if (finishUntil > 0f && Time.unscaledTime > finishUntil)
                finishUntil = 0f;
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
                // On resume, do not play a fake "start" or "completed" animation.
                initialized = true;
                previous = hasTask ? current : default;
                if (hasTask) StartLoop(current.BuildingId, false);
                return;
            }

            bool sameTask = hasTask && ConstructionDelightState.SameTask(previous, current);
            if (previous.IsValid && !sameTask)
            {
                var finished = previous;
                StopLoop();
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
            if (hasTask && !sameTask) StartLoop(current.BuildingId, true);
            else if (hasTask && workBuildingId == null) StartLoop(current.BuildingId, false);
            previous = hasTask ? current : default;
        }

        void StartLoop(string id, bool withOpening)
        {
            StopLoop();
            if (!MoveToSite(id)) return; // No arbitrary coordinates or fake site.
            workBuildingId = id;
            SetTint(loopDust, id == "bastion"
                ? new Color(.88f, .70f, .47f, .17f)
                : new Color(.74f, .62f, .44f, .18f));
            SetTint(loopMotes, id == "sawmill"
                ? new Color(.94f, .69f, .43f, .44f)
                : new Color(.80f, .73f, .53f, .34f));

            if (!CanShow()) return;
            loopDust.Play();
            loopMotes.Play();
            if (withOpening)
            {
                openingDust.Emit(13);
                finishEmbers.Emit(4);
            }
        }

        void StopLoop()
        {
            workBuildingId = null;
            SuspendLoop();
        }

        void SuspendLoop()
        {
            if (loopDust) loopDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (loopMotes) loopMotes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void Complete(string id)
        {
            if (!CanShow() || !MoveToSite(id, true)) return;
            Color accent = id == "bastion"
                ? new Color(1f, .85f, .53f, .47f)
                : new Color(.98f, .75f, .40f, .39f);
            SetTint(finishEmbers, accent);
            SetTint(finishRing, new Color(.98f, .81f, .52f, .32f));
            SetTint(finishHalo, new Color(1f, .78f, .46f, .15f));

            Burst(finishDust, 17);
            Burst(finishEmbers, id == "bastion" ? 31 : 23);
            Burst(finishRing, 1);
            Burst(finishHalo, 1);
            finishUntil = Time.unscaledTime + CompletionSeconds;
            // The building model and level are switched by LocalGateway/Parcels,
            // never by this flourish.
        }

        static void Burst(ParticleSystem system, int count)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.Play();
            system.Emit(count);
        }

        // Site calculations always come from a real building collider or mesh.
        bool MoveToSite(string id, bool preferFinishedModel = false)
        {
            if (!FindSite(id, preferFinishedModel, out var b)) return false;
            var cam = city.PlayerCamera;
            Vector3 towardCamera = cam ? cam.transform.position - b.center : Vector3.back;
            towardCamera.y = 0f;
            towardCamera = towardCamera.sqrMagnitude > .001f
                ? towardCamera.normalized : Vector3.back;

            float footprint = Mathf.Clamp(Mathf.Max(b.size.x, b.size.z) * .78f, 1.25f, 6.25f);
            Vector3 ground = new Vector3(b.center.x, b.min.y + .22f, b.center.z) +
                             towardCamera * .24f;
            // Project the finished ornament onto the facade-facing side of the
            // TRUE model bounds, not through the middle of its roof or splats.
            Vector3 eye = cam ? (cam.transform.position - b.center).normalized : Vector3.back;
            float nearSurface = Mathf.Abs(eye.x) * b.extents.x +
                                Mathf.Abs(eye.y) * b.extents.y +
                                Mathf.Abs(eye.z) * b.extents.z;
            Vector3 facade = b.center + eye * Mathf.Min(nearSurface + .12f, 20f);

            Place(loopDust, ground, footprint);
            Place(loopMotes, ground + Vector3.up * .18f, footprint * .65f);
            Place(openingDust, ground, footprint);
            Place(finishDust, ground, footprint);
            Place(finishEmbers, facade, footprint * .75f);
            Place(finishRing, facade, 0f);
            Place(finishHalo, facade, 0f);

            float diameter = Mathf.Clamp(footprint * 1.43f, 2f, 9f);
            SetSize(finishRing, diameter);
            SetSize(finishHalo, diameter * .79f);
            return true;
        }

        bool FindSite(string id, bool preferFinishedModel, out Bounds bounds)
        {
            bounds = default;
            if (preferFinishedModel && TryModelBounds(id, out bounds)) return true;
            Collider target = null;
            var parcels = city.Parcels;
            if (parcels)
            {
                if (id == "sawmill") target = parcels.LeftTarget;
                else if (id == "barracks") target = parcels.RightTarget;
                if (!target && parcels.Bindings != null)
                    foreach (var binding in parcels.Bindings)
                        if (binding != null && binding.BuildingId == id && binding.Target)
                        {
                            target = binding.Target;
                            break;
                        }
            }
            if (!target)
            {
                foreach (var h in GetComponentsInChildren<WorldHotspot>(true))
                    if (h && h.Id == id && h.TryGetComponent(out Collider found))
                    {
                        target = found;
                        break;
                    }
            }
            if (target && target.enabled && target.gameObject.activeInHierarchy &&
                target.bounds.size.sqrMagnitude > .001f)
            {
                bounds = target.bounds;
                return true;
            }

            return TryModelBounds(id, out bounds);
        }

        bool TryModelBounds(string id, out Bounds bounds)
        {
            bounds = default;
            var parcels = city.Parcels;
            GameObject model = null;
            if (id == "bastion")
                model = city.BastionTwo && city.BastionTwo.activeInHierarchy
                    ? city.BastionTwo : city.BastionOne;
            else if (parcels)
                model = id == "sawmill" ? parcels.LeftBuilt
                      : id == "barracks" ? parcels.RightBuilt : null;

            if (!model) return false;
            bool foundRenderer = false;
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            {
                if (!renderer || !renderer.enabled) continue;
                if (!foundRenderer) { bounds = renderer.bounds; foundRenderer = true; }
                else bounds.Encapsulate(renderer.bounds);
            }
            return foundRenderer && bounds.size.sqrMagnitude > .001f;
        }

        static void Place(ParticleSystem p, Vector3 at, float width)
        {
            p.transform.position = at;
            if (width > 0f)
            {
                var shape = p.shape;
                shape.scale = new Vector3(width, width * .22f, width * .48f);
            }
        }

        static void SetSize(ParticleSystem p, float size)
        {
            var main = p.main;
            main.startSize = size;
        }

        static void SetTint(ParticleSystem p, Color color)
        {
            var main = p.main;
            main.startColor = color;
        }

        void InitializeArt()
        {
            var ownedRoot=new GameObject("Construction Delight / owned particles");
            ownedRoot.transform.SetParent(transform,false);
            particleRoot=ownedRoot.transform;
            dustTexture = MakeParticleTexture(false);
            ringTexture = MakeParticleTexture(true);
            dustMaterial = CreateMaterial("Eldoria / artisan dust", dustTexture);
            ringMaterial = CreateMaterial("Eldoria / antique gold ring", ringTexture);
            if (!dustMaterial || !ringMaterial)
            {
                Debug.LogError("CONSTRUCTION_DELIGHT_MATERIAL_UNAVAILABLE");
                enabled = false;
                return;
            }

            loopDust = MakeSystem("Work / fine dust", dustMaterial,
                true, 1.1f, .72f, 1.28f, .13f, .25f, .17f, 55);
            loopMotes = MakeSystem("Work / timber motes", dustMaterial,
                true, .38f, .55f, .96f, .055f, .115f, .28f, 22);
            openingDust = MakeSystem("Opening / rising dust", dustMaterial,
                false, 0f, .38f, .85f, .13f, .30f, .30f, 30);
            finishDust = MakeSystem("Completion / settling dust", dustMaterial,
                false, 0f, .52f, 1.10f, .15f, .34f, .37f, 36);
            finishEmbers = MakeSystem("Completion / soft gilded motes", dustMaterial,
                false, 0f, .56f, 1.16f, .065f, .125f, .72f, 64);
            finishRing = MakeSystem("Completion / carved aureole", ringMaterial,
                false, 0f, 1.40f, 1.40f, 2f, 2f, 0f, 2);
            finishHalo = MakeSystem("Completion / warm reveal", dustMaterial,
                false, 0f, 1.18f, 1.18f, 2f, 2f, 0f, 2);

            var ringShape = finishRing.shape;
            ringShape.enabled = false;
            var haloShape = finishHalo.shape;
            haloShape.enabled = false;
            Grow(finishRing, new AnimationCurve(
                new Keyframe(0f, .56f), new Keyframe(.45f, 1f), new Keyframe(1f, 1.14f)));
            Grow(finishHalo, new AnimationCurve(
                new Keyframe(0f, .46f), new Keyframe(.28f, 1.05f), new Keyframe(1f, 1.23f)));
        }

        ParticleSystem MakeSystem(string name, Material material, bool continuous,
            float rate, float lifeMin, float lifeMax, float sizeMin, float sizeMax,
            float upward, int countLimit)
        {
            var obj = new GameObject("Delight · " + name, typeof(ParticleSystem));
            obj.transform.SetParent(particleRoot, true);
            var ps = obj.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = continuous;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = countLimit;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startSpeed = 0f;
            main.startColor = new Color(.82f, .66f, .45f, .21f);

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = rate;
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(1.5f, .35f, .75f);
            var drift = ps.velocityOverLifetime;
            drift.enabled = true;
            drift.space = ParticleSystemSimulationSpace.World;
            drift.y = new ParticleSystem.MinMaxCurve(upward * .72f, upward);
            drift.x = new ParticleSystem.MinMaxCurve(-.08f, .11f);
            var faded = ps.colorOverLifetime;
            faded.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, .18f),
                    new GradientAlphaKey(.78f, .58f),
                    new GradientAlphaKey(0f, 1f)
                });
            faded.color = new ParticleSystem.MinMaxGradient(gradient);

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        static void Grow(ParticleSystem p, AnimationCurve curve)
        {
            var lifetime = p.sizeOverLifetime;
            lifetime.enabled = true;
            lifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        }

        static Texture2D MakeParticleTexture(bool ring)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
            {
                name = ring ? "Delight carved ring" : "Delight organic soft mote",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float px = (x + .5f - size * .5f) / (size * .5f);
                    float py = (y + .5f - size * .5f) / (size * .5f);
                    float radius = Mathf.Sqrt(px * px + py * py);
                    float alpha = ring
                        ? Mathf.Exp(-Mathf.Pow((radius - .73f) / .086f, 2f)) *
                          Mathf.Clamp01((1f - radius) * 9f)
                        : Mathf.Pow(Mathf.Clamp01(1f - radius), 2.30f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        static Material CreateMaterial(string title, Texture2D texture)
        {
            var shader = Resources.Load<Shader>("ConstructionDelightSoft") ??
                         Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                         Shader.Find("UI/Default") ??
                         Shader.Find("Sprites/Default");
            if (!shader) return null;
            var m = new Material(shader) { name = title };
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", texture);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", texture);
            if (m.HasProperty("_Surface")) m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_ZWrite")) m.SetFloat("_ZWrite", 0f);
            if (m.HasProperty("_SrcBlend")) m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            if (m.HasProperty("_DstBlend")) m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.SetOverrideTag("RenderType", "Transparent");
            m.renderQueue = (int)RenderQueue.Transparent;
            return m;
        }

        void OnDestroy()
        {
            if (dustMaterial) Destroy(dustMaterial);
            if (ringMaterial) Destroy(ringMaterial);
            if (dustTexture) Destroy(dustTexture);
            if (ringTexture) Destroy(ringTexture);
        }
    }
}

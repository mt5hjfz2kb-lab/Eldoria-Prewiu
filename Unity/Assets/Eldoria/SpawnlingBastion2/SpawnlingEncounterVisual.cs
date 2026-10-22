using System;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.SpawnlingBastion2
{
    /// <summary>
    /// Visual presentation only. Combat, rewards, unlocks and save data are
    /// controlled exclusively by the existing engendro-valoria gateway.
    /// No bespoke game command, collider, health pool or new interactable node.
    /// </summary>
    public sealed class SpawnlingEncounterVisual : MonoBehaviour
    {
        public ValoriaIntegratedWorld Host;
        public Transform MotionPivot;
        public GameObject ModelRoot;
        public Animation SkeletalAnimation;
        public string SwipeClip = "";
        bool wasAttacking;
        [Range(0f, 0.10f)] public float BreatheAmplitude = .028f;
        [Range(0f, 8f)] public float SwayDegrees = 3.5f;

        // Only the skinned/LOD presentation scales. The original hotspot,
        // colliders, combat, rewards and persistent state are never modified.
        [Range(1f, 3f)] public float NearPresence = 1.9f;
        [Range(2f, 6f)] public float StrategicPresence = 4.5f;
        [Min(1f)] public float NearOrthographicZoom = 14f;
        [Min(2f)] public float FarOrthographicZoom = 70f;

        Vector3 restPosition, restScale;
        Quaternion restRotation;
        Vector3 restAnchorOffset;
        bool restAnchorReady;
        float nextStateCheck;

        void Awake()
        {
            if (!MotionPivot) MotionPivot = transform;
            restPosition = MotionPivot.localPosition;
            restRotation = MotionPivot.localRotation;
            restScale = MotionPivot.localScale;
            CacheRestAnchor();
        }

        // The imported V4 skeleton was aligned in WORLD SPACE before skinning.
        // Its actual geometry can be offset laterally from the idle pivot.
        // Compensate all three axes while scaling, not only Y: otherwise a
        // 4.5x version walks many metres away from the original hotspot.
        // Resolve lazily as well, because initially inactive LOD renderers
        // can have zero bounds until the real player scene is enabled.
        void CacheRestAnchor()
        {
            if (restAnchorReady || !MotionPivot || !ModelRoot) return;
            var rs = ModelRoot.GetComponentsInChildren<Renderer>(true);
            bool seen = false;
            Bounds joined = default;
            foreach (var r in rs)
            {
                if (!r || r.bounds.size.y <= .001f) continue;
                if (!seen) { joined = r.bounds; seen = true; }
                else joined.Encapsulate(r.bounds);
            }
            if (!seen) return;
            Vector3 worldFoot = new Vector3(joined.center.x, joined.min.y, joined.center.z);
            Vector3 parentFoot = MotionPivot.parent
                ? MotionPivot.parent.InverseTransformPoint(worldFoot) : worldFoot;
            restAnchorOffset = parentFoot - restPosition;
            restAnchorReady = true;
        }

        void OnEnable()
        {
            nextStateCheck = 0f;
        }

        void LateUpdate()
        {
            if (!Host || !Host.Ready || !ModelRoot || !MotionPivot) return;
            // Visual presence follows the ORIGINAL persisted Bastion II enemy flag.
            // A save reload or a win cannot leave a ghost creature on the map.
            if (Time.unscaledTime >= nextStateCheck)
            {
                nextStateCheck = Time.unscaledTime + .30f;
                var city = Host.City;
                if (city != null && city.Gateway != null)
                {
                    ApplySnapshot(city.Gateway.Snapshot(),city.Clock.UtcTicks,Host.InWorld);
                }
            }
            if (!ModelRoot.activeSelf || !Host.InWorld) return;
            // A small enemy disappears when the whole authored region is shown.
            // Increase its visual-only footprint with camera distance. It is
            // still a single original encounter, not a second strategic icon.
            if (!restAnchorReady) CacheRestAnchor();
            if (!restAnchorReady) return;
            float zoom = Host.WorldCamera ? Host.WorldCamera.orthographicSize : NearOrthographicZoom;
            float presence = PresenceAtZoom(zoom);
            MotionPivot.localScale = restScale * presence;
            float t = Time.unscaledTime;
            MotionPivot.localPosition = restPosition -
                restAnchorOffset * (presence - 1f) +
                Vector3.up * (BreatheAmplitude * Mathf.Sin(t * 1.55f));
            MotionPivot.localRotation = restRotation *
                Quaternion.Euler(1.1f * Mathf.Sin(t * 1.2f),
                    SwayDegrees * Mathf.Sin(t * .62f), 0f);
        }

        // Deterministic for editor/unit QA, even before the scene can be opened.
        public float PresenceAtZoom(float zoom)
        {
            float denominator = Mathf.Max(1f, FarOrthographicZoom - NearOrthographicZoom);
            float fraction = Mathf.Clamp01((zoom - NearOrthographicZoom) / denominator);
            float smooth = fraction * fraction * (3f - 2f * fraction);
            return Mathf.Lerp(NearPresence, StrategicPresence, smooth);
        }

        // The original gateway resolves combat directly from outbound to returning.
        // Start the visual windup during the last 1.5 seconds of approach; never
        // add a combat phase, delay victory, or write to the persisted state.
        void ApplySnapshot(PlayerState state,long now,bool inWorld)
        {
            bool shouldAppear=!state.EngendroDefeated;
            double remaining=(state.March.PhaseEndsUtcTicks-now)/(double)TimeSpan.TicksPerSecond;
            bool attacking=shouldAppear&&inWorld&&state.March.TargetId=="engendro-valoria"&&
                state.March.Phase=="outbound"&&remaining>0&&remaining<=1.5;
            if(SkeletalAnimation&&!string.IsNullOrEmpty(SwipeClip)) {
                if(attacking&&!wasAttacking)SkeletalAnimation.Play(SwipeClip,PlayMode.StopAll);
                if(!shouldAppear)ResetSkinPose();
            }
            wasAttacking=attacking;
            if(ModelRoot.activeSelf!=shouldAppear)ModelRoot.SetActive(shouldAppear);
        }

        void ResetSkinPose()
        {
            if(!SkeletalAnimation)return;
            SkeletalAnimation.Stop();
            var clip=SkeletalAnimation.GetClip(SwipeClip);
            if(clip)clip.SampleAnimation(SkeletalAnimation.gameObject,0);
        }

        void OnDisable()
        {
            wasAttacking=false;
            ResetSkinPose();
            if (!MotionPivot) return;
            MotionPivot.localPosition = restPosition;
            MotionPivot.localRotation = restRotation;
            MotionPivot.localScale = restScale;
        }
    }
}

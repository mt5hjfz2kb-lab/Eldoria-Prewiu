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
        float restFootOffset;
        float nextStateCheck;

        void Awake()
        {
            if (!MotionPivot) MotionPivot = transform;
            restPosition = MotionPivot.localPosition;
            restRotation = MotionPivot.localRotation;
            restScale = MotionPivot.localScale;
            // V4 uses a retained ground-aligned pivot. Cache its original foot
            // offset before changing scale, to keep the creature on the terrain.
            if (ModelRoot)
            {
                float lower = float.PositiveInfinity;
                foreach (var visual in ModelRoot.GetComponentsInChildren<Renderer>(true))
                    if (visual && visual.bounds.size.y > .001f)
                        lower = Mathf.Min(lower, visual.bounds.min.y);
                if (!float.IsInfinity(lower))
                    restFootOffset = Mathf.Clamp(
                        (lower - MotionPivot.position.y) /
                        Mathf.Max(.0001f, Mathf.Abs(MotionPivot.lossyScale.y)), -5f, 5f);
            }
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
            float zoom = Host.WorldCamera ? Host.WorldCamera.orthographicSize : NearOrthographicZoom;
            float presence = PresenceAtZoom(zoom);
            MotionPivot.localScale = restScale * presence;
            float groundCorrection = -restFootOffset * restScale.y * (presence - 1f);
            float t = Time.unscaledTime;
            MotionPivot.localPosition = restPosition +
                Vector3.up * (groundCorrection + BreatheAmplitude * Mathf.Sin(t * 1.55f));
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

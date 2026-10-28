using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GLTFast;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.AldricCity
{
    /// <summary>
    /// Optional single visual Aldric. The current game remains authoritative for
    /// missions, saves, combat and city state. No new gameplay entity is created.
    /// A missing model, animation or verified road means NO actor, never a fallback.
    /// </summary>
    public sealed class AldricCityRoaming : MonoBehaviour
    {
        private const string Asset = "Eldoria/Aldric/SirAldric_Approved_RiggedAnimated_v1.glb";
        private const string Idle = "Aldric_Idle_v1";
        private const string Walk = "Aldric_Walk_v1";
        private const float Height = 1.85f;
        private const float Speed = 1.05f;
        private const float Radius = 0.26f;
        private ValoriaIntegratedWorld host;
        private MeshCollider road;
        private GameObject actor;
        private GameObject model;
        private Animation clips;
        private Renderer[] renderers;
        private Vector3[] path;
        private int index, direction = 1;
        private bool walking;
        private float pauseUntil;
        private float facingYaw;
        private readonly List<Collider> obstacles = new List<Collider>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Attach()
        {
            var live = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            if (live && !live.GetComponent<AldricCityRoaming>())
                live.gameObject.AddComponent<AldricCityRoaming>();
        }

        private IEnumerator Start()
        {
            host = GetComponent<ValoriaIntegratedWorld>();
            if (!host) yield break;
            for (int i = 0; i < 120 && !host.Ready; ++i) yield return null;
            if (!host.Ready || !host.City || !host.City.PlayerCamera) yield break;

            // The actual Axial_Paving MeshCollider from Valoria's Paths prefab.
            // Never guess a route on grass, water or beneath buildings.
            road = UnityEngine.Object.FindObjectsByType<MeshCollider>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .FirstOrDefault(c => c && c.enabled &&
                    c.name.IndexOf("Axial_Paving", StringComparison.OrdinalIgnoreCase) >= 0);
            if (!road || !BuildAndValidatePath()) yield break;
            yield return LoadExactModel();
        }

        private bool OnRoad(Vector3 point, out Vector3 floor)
        {
            floor = default;
            if (!road || !road.enabled) return false;
            var ray = new Ray(point + Vector3.up * 5f, Vector3.down);
            if (!road.Raycast(ray, out var hit, 12f)) return false;
            if (Vector3.Angle(hit.normal, Vector3.up) > 28f) return false;
            floor = hit.point;
            return true;
        }

        private bool BodyClear(Vector3 ground)
        {
            var lo = ground + Vector3.up * (Radius + 0.12f);
            var hi = ground + Vector3.up * (Height - Radius);
            foreach (var c in obstacles)
            {
                if (!c || !c.enabled || !c.gameObject.activeInHierarchy ||
                    c.isTrigger || c == road) continue;
                // Conservative closest-point checks; a failed clearance keeps
                // Aldric stationary rather than walking through geometry.
                if (Vector3.Distance(c.ClosestPoint(lo), lo) < Radius ||
                    Vector3.Distance(c.ClosestPoint(hi), hi) < Radius)
                    return false;
            }
            return true;
        }

        private bool SegmentClear(Vector3 a, Vector3 b)
        {
            var length = Vector3.Distance(a, b);
            if (length < 0.75f || length > 18f) return false;
            var count = Mathf.CeilToInt(length / 0.35f);
            for (int j = 0; j <= count; ++j)
            {
                var p = Vector3.Lerp(a, b, j / (float)count);
                if (!OnRoad(p, out var floor) || !BodyClear(floor)) return false;
            }
            return true;
        }

        private bool BuildAndValidatePath()
        {
            var centre = road.bounds.center;
            var all = UnityEngine.Object.FindObjectsByType<Collider>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var c in all)
            {
                if (!c || c == road || c.isTrigger) continue;
                var n = c.name.ToLowerInvariant();
                // Roads, landscape and UI hotspots are not physical walls.
                // Buildings and walls still block the capsule.
                if (n.Contains("road") || n.Contains("paving") ||
                    n.Contains("ground") || n.Contains("terrain") ||
                    n.Contains("meadow") || n.Contains("hotspot")) continue;
                if (Vector3.Distance(c.bounds.center, centre) < 28f)
                    obstacles.Add(c);
            }
            var forward = road.transform.forward;
            var side = road.transform.right;
            // Multiple candidates so an irregular cobbled mesh is not forced
            // to accept its axis-aligned bounds centre as a valid pavement.
            var candidates = new[] { centre, centre + side, centre - side,
                                      centre + forward, centre - forward };
            foreach (var centreCandidate in candidates)
            {
                foreach (var axis in new[] { forward, -forward, side, -side })
                {
                    if (!OnRoad(centreCandidate, out var middle)) continue;
                    if (!OnRoad(centreCandidate + axis * 2f, out var p1)) continue;
                    if (!OnRoad(centreCandidate + axis * 4f, out var p2)) continue;
                    if (!SegmentClear(middle, p1) || !SegmentClear(p1, p2))
                        continue;
                    path = new[] { middle, p1, p2 };
                    return true;
                }
            }
            Debug.LogWarning("ALDRIC_CITY_NO_SAFE_PAVED_ROUTE");
            return false;
        }

        private async System.Threading.Tasks.Task<bool> Import(string url)
        {
            var source = new GltfImport();
            if (!await source.Load(url)) return false;
            actor = new GameObject("Sir Aldric | city-only visual actor");
            actor.transform.SetParent(transform, false);
            model = new GameObject("Sir Aldric / approved walking rig");
            model.transform.SetParent(actor.transform, false);
            var instantiator = new GameObjectInstantiator(source, model.transform);
            if (!await source.InstantiateMainSceneAsync(instantiator)) return false;
            clips = instantiator.SceneInstance?.LegacyAnimation;
            if (!clips || !clips.GetClip(Idle) || !clips.GetClip(Walk)) return false;
            renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0 || !model.GetComponentInChildren<SkinnedMeshRenderer>(true))
                return false;
            foreach (var r in renderers)
                if (r is SkinnedMeshRenderer s) s.updateWhenOffscreen = false;
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; ++i) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y < .05f) return false;
            model.transform.localScale *= Mathf.Clamp(Height / bounds.size.y, .01f, 100f);
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; ++i) bounds.Encapsulate(renderers[i].bounds);
            // Align the imported character's feet (not its empty glTF pivot)
            // to the real cobbled-road collider.
            model.transform.position += Vector3.up * (actor.transform.position.y - bounds.min.y);
            clips.playAutomatically = false;
            clips[Idle].wrapMode = WrapMode.Loop;
            clips[Walk].wrapMode = WrapMode.Loop;
            clips.Play(Idle);
            return true;
        }

        private IEnumerator LoadExactModel()
        {
            var basePath = Application.streamingAssetsPath.TrimEnd('/');
            var pathOnDisk = basePath + "/" + Asset;
            if (!pathOnDisk.Contains("://") && !File.Exists(pathOnDisk))
            {
                Debug.LogWarning("ALDRIC_CITY_APPROVED_GLB_MISSING");
                yield break;
            }
            // StreamingAssets on Android is a jar:file URI. glTFast handles
            // both that URL and ordinary file:// URIs.
            var url = pathOnDisk.Contains("://") ? pathOnDisk :
                      new Uri(pathOnDisk).AbsoluteUri;
            var task = Import(url);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted || task.IsCanceled || !task.Result)
            {
                if (model) Destroy(model);
                Debug.LogWarning("ALDRIC_CITY_IMPORT_OR_RIG_FAILED");
                yield break;
            }
            actor.transform.position = path[0];
            pauseUntil = Time.time + 2.5f;
        }

        private bool Away()
        {
            if (!host || !host.Ready || !host.City) return true;
            var s = host.City.Gateway.Snapshot();
            return host.InWorld ||
                s.ChapterThree.InvestigationEndsUtcTicks > host.City.Clock.UtcTicks ||
                (s.March != null && s.March.Phase != "idle" &&
                 (s.March.HeroId == "aldric" || s.March.HeroId == "aldric+lyra"));
        }

        private void Update()
        {
            if (!model || !actor || path == null || renderers == null) return;
            var away = Away();
            var cam = host.City.PlayerCamera;
            var distance = cam ? Vector3.Distance(cam.transform.position, actor.transform.position) : 999f;
            bool visible = !away && distance <= 95f;
            foreach (var r in renderers) if (r) r.enabled = visible;
            if (away)
            {
                if (walking) { walking = false; clips.CrossFade(Idle, .15f); }
                pauseUntil = Time.time + 2f;
                return;
            }
            if (!walking)
            {
                if (Time.time < pauseUntil) return;
                var next = index + direction;
                if (next < 0 || next >= path.Length) { direction *= -1; next = index + direction; }
                if (next < 0 || next >= path.Length || !SegmentClear(actor.transform.position, path[next]))
                { pauseUntil = Time.time + 4f; return; }
                index = next;
                walking = true;
                clips.CrossFade(Walk, .16f);
            }
            var delta = path[index] - actor.transform.position;
            delta.y = 0f;
            var step = Speed * Time.deltaTime;
            if (delta.magnitude <= step + .04f)
            {
                actor.transform.position = path[index];
                walking = false;
                clips.CrossFade(Idle, .16f);
                pauseUntil = Time.time + UnityEngine.Random.Range(2.5f, 6f);
                return;
            }
            var candidate = actor.transform.position + delta.normalized * step;
            if (!OnRoad(candidate, out var ground) || !BodyClear(ground))
            {
                walking = false;
                clips.CrossFade(Idle, .16f);
                pauseUntil = Time.time + 6f;
                return;
            }
            actor.transform.position = ground;
            actor.transform.rotation = Quaternion.Slerp(actor.transform.rotation,
                Quaternion.LookRotation(delta.normalized), Mathf.Clamp01(7f * Time.deltaTime));
        }

        private void OnDestroy() { if (actor) Destroy(actor); }
    }
}

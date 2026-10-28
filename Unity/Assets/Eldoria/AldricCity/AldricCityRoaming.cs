using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using GLTFast;
using UnityEngine.Networking;
using Eldoria.ReferenceClean;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private const int ModelBytes = 10140372;
        private const string ApprovedSha256 = "c28a65b6040225256ae047d8fd773849771895ac378041e0882f279517e78c8b";
        private const float ModelYawCorrection = -90f; // Check with real Unity front-view proof.
        private static readonly string[] ApprovedAnimations = {
            "Aldric_Idle_v1", "Aldric_Walk_v1", "Aldric_SwordAttack_v1",
            "Aldric_Hurt_v1", "Aldric_Defeat_v1"
        };
        private ValoriaIntegratedWorld host;
        private MeshCollider road;
        private GameObject actor;
        private GameObject model;
        private Animation clips;
        private GltfImport ownedImport;
        private Renderer[] renderers;
        private Vector3[] path;
        private int index, direction = 1;
        private bool walking;
        private float pauseUntil;
        private float nextAwayRefresh;
        private bool cachedAway;
        private readonly Collider[] clearanceHits = new Collider[64];
        private string firstBlockingCollider;
        // Native verification probes. Read-only; never mutate player saves or gameplay.
        public bool ApprovedVisualReady => actor && model && clips && path != null &&
            renderers != null && renderers.Length > 0;
        public int RoadWaypoints => path == null ? 0 : path.Length;
        public bool ActualWalkClipPlaying => clips && clips.IsPlaying(Walk);
        public bool CurrentBodyClear => actor && BodyClear(actor.transform.position);
        public Vector3 ActorGroundPosition => actor ? actor.transform.position : Vector3.zero;
        public Vector3 CurrentWalkDestination => path != null ? path[index] : Vector3.zero;
        public bool IsCityActorVisible => actor && !Away() && renderers != null &&
            renderers.Any(r => r && r.enabled && r.gameObject.activeInHierarchy);
        public float MeasuredModelHeight
        {
            get
            {
                if (renderers == null || renderers.Length == 0) return 0f;
                var found = false;
                var bounds = new Bounds();
                foreach (var renderer in renderers)
                {
                    if (!renderer) continue;
                    if (!found) { bounds = renderer.bounds; found = true; }
                    else bounds.Encapsulate(renderer.bounds);
                }
                return found ? bounds.size.y : 0f;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterCanonicalSceneHook()
        {
            // Also handle Test Runner and scene reloads, not just game boot.
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AttachToInitialScene()
        {
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.IsValid()) Attach();
        }

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
            var readyDeadline = Time.realtimeSinceStartup + 60f;
            while (host && !host.Ready && Time.realtimeSinceStartup < readyDeadline)
                yield return null;
            if (!host || !host.Ready || !host.City || !host.City.PlayerCamera) yield break;

            // Inspect every actual axial pavement, not a guessed point at the
            // centre of an irregular mesh's world bounds. Physics stays authoritative.
            var roads = UnityEngine.Object.FindObjectsByType<MeshCollider>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(c => c && c.enabled &&
                    c.name.IndexOf("Axial_Paving", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(c => c.GetInstanceID()).ToArray();
            if (roads.Length == 0)
            {
                Debug.LogWarning("ALDRIC_CITY_AXIAL_PAVING_COLLIDER_NOT_FOUND");
                yield break;
            }
            Physics.SyncTransforms();
            foreach (var candidateRoad in roads)
            {
                road = candidateRoad;
                yield return BuildAndValidatePath();
                if (path != null) break;
            }
            if (path == null) yield break;
            Debug.Log("ALDRIC_CITY_SAFE_ROUTE_VERIFIED_" + path.Length + "_POINTS");
            yield return LoadExactModel();
        }

        private bool OnRoad(Vector3 point, out Vector3 floor)
        {
            floor = default;
            if (!road || !road.enabled) return false;
            var bounds = road.bounds;
            var ray = new Ray(new Vector3(point.x, bounds.max.y + 2f, point.z), Vector3.down);
            if (!road.Raycast(ray, out var hit, bounds.size.y + 4f)) return false;
            if (Vector3.Angle(hit.normal, Vector3.up) > 28f) return false;
            floor = hit.point;
            return true;
        }

        private bool BodyClear(Vector3 ground)
        {
            var lo = ground + Vector3.up * (Radius + 0.12f);
            var hi = ground + Vector3.up * (Height - Radius);
            // Check the entire occupied capsule against current scene geometry.
            // Three point samples can miss a narrow obstruction between them;
            // a cached list also cannot detect colliders added after initialization.
            int count = Physics.OverlapCapsuleNonAlloc(lo, hi, Radius, clearanceHits,
                ~0, QueryTriggerInteraction.Ignore);
            if (count == clearanceHits.Length) return false; // fail closed on saturation
            for (int i = 0; i < count; i++)
            {
                var c = clearanceHits[i];
                if (!c || !c.enabled || !c.gameObject.activeInHierarchy || c == road ||
                    (actor && c.transform.IsChildOf(actor.transform))) continue;
                var n = c.name.ToLowerInvariant();
                if (n.Contains("road") || n.Contains("paving") || n.Contains("ground") ||
                    n.Contains("terrain") || n.Contains("meadow") || n.Contains("hotspot")) continue;
                if (firstBlockingCollider == null) firstBlockingCollider = c.name;
                return false;
            }
            return true;
        }

        private bool SegmentClear(Vector3 a, Vector3 b)
        {
            var length = Vector3.Distance(a, b);
            // A resumed leg may have only centimetres left. Rejecting every
            // remaining segment below0.75m could strand the actor after a mission.
            if (length < 0.04f || length > 18f) return false;
            var count = Mathf.CeilToInt(length / 0.35f);
            for (int j = 0; j <= count; ++j)
            {
                var p = Vector3.Lerp(a, b, j / (float)count);
                if (!OnRoad(p, out var floor) || !BodyClear(floor)) return false;
            }
            return true;
        }

        private bool InCityFrame(Vector3 ground)
        {
            var camera = host.City.PlayerCamera;
            var p = camera.WorldToViewportPoint(ground + Vector3.up * Height * .5f);
            return p.z > camera.nearClipPlane && p.z < camera.farClipPlane &&
                p.x > .08f && p.x < .92f && p.y > .08f && p.y < .92f;
        }

        private IEnumerator BuildAndValidatePath()
        {
            // Axial_Paving is an irregular joined mesh. Its bounds centre can
            // lie inside the fortress or in a gap. Sample its REAL surface over
            // the complete bounds, with bounded work spread across frames.
            var bounds = road.bounds;
            int nx = Mathf.Clamp(Mathf.CeilToInt(bounds.size.x / .75f), 2, 64);
            int nz = Mathf.Clamp(Mathf.CeilToInt(bounds.size.z / .75f), 2, 64);
            var candidates = new List<Vector3>();
            firstBlockingCollider = null;
            int samples = 0, onPaving = 0, occupied = 0;
            for (int x = 0; x <= nx; x++)
            {
                for (int z = 0; z <= nz; z++)
                {
                    var p = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, x / (float)nx),
                        bounds.center.y, Mathf.Lerp(bounds.min.z, bounds.max.z, z / (float)nz));
                    if (OnRoad(p, out var floor))
                    {
                        onPaving++;
                        if (!BodyClear(floor)) occupied++;
                        else if (InCityFrame(floor)) candidates.Add(floor);
                    }
                    if (++samples % 64 == 0) yield return null;
                }
            }
            // Prefer the genuine player's central city area; never move or zoom
            // the player's camera merely to manufacture a visible acceptance view.
            var camera = host.City.PlayerCamera;
            candidates.Sort((a,b) => {
                var pa = camera.WorldToViewportPoint(a);
                var pb = camera.WorldToViewportPoint(b);
                return ((pa.x-.5f)*(pa.x-.5f)+(pa.y-.5f)*(pa.y-.5f)).CompareTo(
                    (pb.x-.5f)*(pb.x-.5f)+(pb.y-.5f)*(pb.y-.5f));
            });
            var forward = Vector3.ProjectOnPlane(road.transform.forward, Vector3.up).normalized;
            var side = Vector3.ProjectOnPlane(road.transform.right, Vector3.up).normalized;
            var axes = new[] { forward, -forward, side, -side,
                (forward+side).normalized, (forward-side).normalized,
                (-forward+side).normalized, (-forward-side).normalized };
            int tried = 0;
            foreach (var middle in candidates)
            {
                foreach (var axis in axes)
                {
                    if (!OnRoad(middle + axis * 2f, out var p1) || !InCityFrame(p1)) continue;
                    if (!OnRoad(middle + axis * 4f, out var p2) || !InCityFrame(p2)) continue;
                    if (!SegmentClear(middle, p1) || !SegmentClear(p1, p2)) continue;
                    if (OnRoad(middle + axis * 6f, out var p3) && InCityFrame(p3) &&
                        SegmentClear(p2, p3))
                        path = new[] { middle, p1, p2, p3 };
                    else
                        path = new[] { middle, p1, p2 };
                    Debug.Log("ALDRIC_CITY_PAVING_SEARCH_PASS road=" + road.name +
                        " samples=" + samples + " surface=" + onPaving + " occupied=" + occupied +
                        " visible_clear=" + candidates.Count + " start=" + middle.ToString("F3") +
                        " finish=" + path[path.Length-1].ToString("F3"));
                    yield break;
                }
                if (++tried % 16 == 0) yield return null;
            }
            Debug.LogWarning("ALDRIC_CITY_NO_SAFE_PAVED_ROUTE road=" + road.name +
                " bounds=" + bounds + " samples=" + samples + " surface=" + onPaving +
                " occupied=" + occupied + " visible_clear=" + candidates.Count +
                " first_obstruction=" + (firstBlockingCollider ?? "none"));
        }

        private async System.Threading.Tasks.Task<bool> Import(byte[] verifiedBytes)
        {
            ownedImport = new GltfImport();
            if (!await ownedImport.Load(verifiedBytes)) return false;
            if (!this || !host) return false;
            actor = new GameObject("Sir Aldric | city-only visual actor");
            actor.transform.SetParent(transform, false);
            actor.SetActive(false);
            model = new GameObject("Sir Aldric / approved walking rig");
            model.transform.SetParent(actor.transform, false);
            var instantiator = new GameObjectInstantiator(ownedImport, model.transform);
            if (!await ownedImport.InstantiateMainSceneAsync(instantiator)) return false;
            if (!this || !host) return false;
            clips = instantiator.SceneInstance?.LegacyAnimation;
            if (!clips) return false;
            foreach (var name in ApprovedAnimations)
                if (!clips.GetClip(name)) return false;
            renderers = model.GetComponentsInChildren<Renderer>(true);
            var skins = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length == 0 ||
                skins.Length == 0 ||
                !skins.Any(s => s.bones != null && s.bones.Length == 26))
                return false;
            foreach (var skin in skins) skin.updateWhenOffscreen = false;
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; ++i) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y < .1f || bounds.size.y > 100f) return false;
            model.transform.localScale *= Height / bounds.size.y;
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; ++i) bounds.Encapsulate(renderers[i].bounds);
            model.transform.position += Vector3.up * (actor.transform.position.y - bounds.min.y);
            clips.playAutomatically = false;
            clips[Idle].wrapMode = WrapMode.Loop;
            clips[Walk].wrapMode = WrapMode.Loop;
            actor.transform.position = path[0];
            actor.SetActive(true);
            clips.Play(Idle);
            return true;
        }

        private IEnumerator LoadExactModel()
        {
            var pathOnDisk = UnityEngine.Application.streamingAssetsPath.TrimEnd('/') + "/" + Asset;
            // StreamingAssets uses jar:file:// on Android, plain file:// on desktop.
            var url = pathOnDisk.Contains("://") ? pathOnDisk : new Uri(pathOnDisk).AbsoluteUri;
            byte[] verifiedBytes;
            using (var request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning("ALDRIC_CITY_APPROVED_GLB_MISSING_OR_UNREADABLE");
                    yield break;
                }
                verifiedBytes = request.downloadHandler.data;
            }
            if (verifiedBytes == null || verifiedBytes.Length != ModelBytes)
            {
                Debug.LogError("ALDRIC_CITY_UNEXPECTED_MODEL_BYTES");
                yield break;
            }
            using (var sha = SHA256.Create())
            {
                var digest = BitConverter.ToString(sha.ComputeHash(verifiedBytes)).Replace("-", "").ToLowerInvariant();
                if (digest != ApprovedSha256)
                {
                    Debug.LogError("ALDRIC_CITY_MODEL_SHA256_MISMATCH");
                    yield break;
                }
            }
            var task = Import(verifiedBytes);
            while (!task.IsCompleted) yield return null;
            if (task.IsFaulted || task.IsCanceled || !task.Result)
            {
                if (actor) Destroy(actor);
                actor = null; model = null;
                if (ownedImport != null) { ownedImport.Dispose(); ownedImport = null; }
                Debug.LogError("ALDRIC_CITY_IMPORT_OR_26_JOINT_RIG_FAILED");
                yield break;
            }
            pauseUntil = Time.time + 2.5f;
            Debug.Log("ALDRIC_CITY_READY_APPROVED_RIG_26_BONES_5_CLIPS");
        }

        private bool Away()
        {
            if (!host || !host.Ready || !host.City) return true;
            if (host.InWorld) return true;
            // Snapshot() clones the authoritative state: never allocate one every
            // rendered frame on mobile merely to decide a cosmetic NPC's visibility.
            if (Time.unscaledTime < nextAwayRefresh) return cachedAway;
            nextAwayRefresh = Time.unscaledTime + .2f;
            var s = host.City.Gateway.Snapshot();
            cachedAway = s.ChapterThree.InvestigationEndsUtcTicks > host.City.Clock.UtcTicks ||
                (s.March != null && s.March.Phase != "idle" &&
                 (s.March.HeroId == "aldric" || s.March.HeroId == "aldric+lyra"));
            return cachedAway;
        }

        private void Update()
        {
            if (!model || !actor || path == null || renderers == null) return;
            var away = Away();
            var cam = host.City.PlayerCamera;
            // Valoria uses an elevated orthographic camera: camera-to-actor distance
            // can exceed 95 metres even for an actor at the centre of the screen.
            // Culling must use projected framing, not physical camera distance.
            bool visible = !away && cam && cam.enabled;
            if (visible)
            {
                var projected = cam.WorldToViewportPoint(actor.transform.position + Vector3.up * Height * .5f);
                visible = projected.z > cam.nearClipPlane && projected.z < cam.farClipPlane &&
                    projected.x >= -.05f && projected.x <= 1.05f &&
                    projected.y >= -.05f && projected.y <= 1.05f;
            }
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
                // index is the current destination while moving. An interruption
                // must resume that leg, then pause at its real waypoint, rather
                // than silently skipping it or reversing across a short segment.
                var next = Vector3.Distance(actor.transform.position, path[index]) > .04f
                    ? index : index + direction;
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
                Quaternion.LookRotation(delta.normalized) * Quaternion.Euler(0f, ModelYawCorrection, 0f), Mathf.Clamp01(7f * Time.deltaTime));
        }

        private void OnDestroy() { if (actor) Destroy(actor); if (ownedImport != null) { ownedImport.Dispose(); ownedImport = null; } }
    }
}

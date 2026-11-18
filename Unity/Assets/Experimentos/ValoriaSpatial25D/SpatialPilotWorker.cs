using Eldoria.Experimentos.Ciudad2D;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaSpatial25D
{
    /// <summary>Deterministic micro-activity, no pathfinding/gateway/economy/persistence.</summary>
    [DisallowMultipleComponent]
    public sealed class SpatialPilotWorker : MonoBehaviour
    {
        public enum Role { Woodcutter, Recruit, Porter }
        private CityBuilding owner;
        private Vector2[] waypoints;
        private Role role;
        private float speed, phase;
        private Vector2 pathWorldOrigin;
        private SpriteRenderer image, contact;
        public CityBuilding Owner => owner;
        public SpriteRenderer Image => image;
        public bool IsVisible => image != null && image.enabled;

        public void Configure(CityBuilding parent, Role occupation,
                              Vector2[] path, float unitsPerSecond, float phaseSeconds, Vector2? routeOrigin = null)
        {
            owner = parent;
            pathWorldOrigin = routeOrigin ?? (Vector2)parent.transform.position;
            role = occupation;
            waypoints = path;
            speed = Mathf.Max(0.10f, unitsPerSecond);
            phase = phaseSeconds;
            image = gameObject.AddComponent<SpriteRenderer>();
            image.sprite = SpatialPilotActorArt.Frame(role, 0);
            transform.localScale = Vector3.one * .85f;
            var shade = new GameObject("SombraContacto_Figurante");
            shade.transform.SetParent(transform, false);
            shade.transform.localPosition = new Vector3(0f, 0f, .01f);
            contact = shade.AddComponent<SpriteRenderer>();
            contact.sprite = SpatialPilotActorArt.Shadow;
            contact.color = new Color(1f, 1f, 1f, .48f);
            SampleAt(0f);
        }

        public void SampleAt(float seconds)
        {
            if (image == null || contact == null || owner == null ||
                waypoints == null || waypoints.Length < 2) return;
            bool built = owner.Level > 0 && owner.PlotPhase == City2DPlotPhase.Built;
            image.enabled = contact.enabled = built;
            if (!built) return;

            // Constant-distance segment traversal and a short dwell for each task.
            float t = Mathf.Max(0f, seconds + phase) * speed;
            float perimeter = 0f;
            for (int i = 0; i < waypoints.Length; ++i)
                perimeter += Vector2.Distance(waypoints[i],
                    waypoints[(i + 1) % waypoints.Length]) + .16f;
            if (perimeter <= .01f) return;
            t = Mathf.Repeat(t, perimeter);

            Vector2 local = waypoints[0], direction = Vector2.right;
            bool working = false;
            for (int i = 0; i < waypoints.Length; ++i)
            {
                Vector2 from = waypoints[i];
                Vector2 to = waypoints[(i + 1) % waypoints.Length];
                float distance = Vector2.Distance(from, to);
                if (t <= distance)
                {
                    float u = distance > .0001f ? Mathf.Clamp01(t / distance) : 0f;
                    local = Vector2.Lerp(from, to, u);
                    direction = to - from;
                    break;
                }
                t -= distance;
                if (t <= .16f)
                {
                    local = to;
                    direction = to - from;
                    working = true;
                    break;
                }
                t -= .16f;
            }

            transform.position = new Vector3(pathWorldOrigin.x + local.x,
                                             pathWorldOrigin.y + local.y, -.045f);
            // Visible legs/arms change with movement; stable pose during activity.
            int pose = working ? 2 : ((Mathf.FloorToInt(seconds * 3f + phase) & 1) == 0 ? 0 : 1);
            image.sprite = SpatialPilotActorArt.Frame(role, pose);
            image.flipX = direction.x < -.02f;
            image.sortingOrder = SpatialPilotBootstrap.FootOrder(transform.position.y) + 1;
            contact.sortingOrder = image.sortingOrder - 2;
        }
    }

}

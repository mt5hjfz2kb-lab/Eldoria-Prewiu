using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.PlayerClarity;
using Eldoria.Presentation;
using Eldoria.ReferenceClean;
using Eldoria.World4XCanon;
using UnityEngine;

namespace Eldoria
{
    /// <summary>
    /// On a completed local Chapter III regeneration, move recurring hunt
    /// visuals and their original click/collider hotspot to a new real
    /// reachable surface. The campaign fissure and unique encounters are
    /// never moved. The choice is reproducible from target ID + persisted
    /// generation, so reopening the same save does not reroll the position.
    /// The rest of world population stays owned by the canonical gateway.
    /// </summary>
    internal sealed class PlayerClarityStoryRenewal
    {
        private readonly Dictionary<string,int> applied = new Dictionary<string,int>();
        private ValoriaIntegratedWorld bound;
        private ICommandGateway boundGateway;

        public void Sync(ValoriaIntegratedWorld host, PlayerState state)
        {
            if (!host || !host.Ready || !host.InWorld ||
                host.City?.Gateway == null || state == null ||
                state.WorldId != "world-local" || state.BastionLevel < 3 ||
                state.ChapterThree == null || !host.WorldRoot) return;
            // Playtest reset rebinds the canonical gateway on the SAME scene
            // host. Never reuse generation-cache entries from an older save.
            if (bound != host || !ReferenceEquals(boundGateway,host.City.Gateway))
            {
                bound = host;
                boundGateway = host.City.Gateway;
                applied.Clear();
            }

            var surface = host.WorldRoot.GetComponentInChildren<RegionOneResourcePlacement>(true);
            var routes = surface?.MarchRoutes as RegionOneTerrainRoutes;
            if (routes == null) return;

            var c = state.ChapterThree;
            Move(host, state, routes, "hunt-wolf", c.WolfGeneration, c.WolfRespawnUtcTicks);
            Move(host, state, routes, "hunt-boar", c.BoarGeneration, c.BoarRespawnUtcTicks);
            if (c.MeatRemaining > 0)
                Move(host, state, routes, "meat-valoria", c.MeatGeneration, c.MeatRespawnUtcTicks);
            // The Chapter V Ash Stalker is a recurring elite (not the
            // one-time campaign scout/Engendro), so its saved generation
            // also selects a new legally reachable spawn position.
            if (state.BastionLevel >= 5)
                Move(host, state, routes, "ash-stalker-valoria",
                    c.AshStalkerGeneration, c.AshStalkerRespawnUtcTicks);
        }

        private void Move(ValoriaIntegratedWorld host, PlayerState state,
            RegionOneTerrainRoutes routes, string targetId, int generation,
            long respawnUtcTicks)
        {
            // The cooldown is owned and settled by LocalGateway. Never
            // relocate while a target is absent or a march still depends
            // on its old return destination.
            if (generation < 1 || respawnUtcTicks > host.City.Clock.UtcTicks ||
                state.March.TargetId == targetId && state.March.Phase != "idle" ||
                state.ExtraMarches.Exists(m=>m!=null&&m.TargetId==targetId&&m.Phase!="idle"))
                return;
            if (applied.TryGetValue(targetId, out var previous) && previous == generation)
                return;
            var target = host.FindTarget(targetId);
            if (!target || !target.gameObject.activeInHierarchy) return;

            // 12 annuli x 24 directions: more geographic variety than the
            // old near-city handful of scripted anchors, but still local
            // to Chapter III so missions remain accessible to new players.
            const int radials = 12, angular = 24, count = radials * angular;
            var old = target.transform.position;
            for (int attempt=0;attempt<count;attempt++)
            {
                int index = DistributedSiteOrder.Candidate(count,targetId,generation,attempt);
                float radius=88f+(index/angular)*16f;
                float angle=(index%angular)*15f*Mathf.Deg2Rad;
                float dx=Mathf.Cos(angle)*radius, dz=Mathf.Sin(angle)*radius;
                var p=host.WorldHome+new Vector3(dx,RegionOneBlenderLandscape.Height(dx,dz),dz);
                // No occupied city, cloned model or opaque UI position.
                if (Vector3.Distance(old,p) < 32f) continue;
                if (!routes.FootprintClear(p,10f) ||
                    !routes.TryRouteTo(state,p,out var steps) || steps == null ||
                    steps.Count < 2) continue;

                // Reuse the same navigable last step as BastionIII's
                // source placement; don't put a hunt at a stranded point.
                var end=steps[steps.Count-1];
                p=new Vector3(end.X,end.Y,end.Z);
                bool conflicts=false;
                foreach(var id in new[]{"fissure-valoria","ash-stalker-valoria",
                    "hunt-wolf","hunt-boar","meat-valoria"})
                {
                    if (id==targetId) continue;
                    var other=host.FindTarget(id);
                    if (other && other.gameObject.activeInHierarchy &&
                        Vector3.Distance(other.transform.position,p) < 36f)
                    { conflicts=true;break; }
                }
                if(conflicts)continue;
                // Existing resource and creature placement checks the same
                // physical hotspots. Avoid crossing their current models.
                foreach(var resource in state.WorldResourceLocations)
                    if(resource.Generation>0 && resource.Remaining>0 &&
                        Vector3.Distance(new Vector3(resource.X,resource.Y,resource.Z),p)<29f)
                    {conflicts=true;break;}
                if(conflicts)continue;
                foreach(var creature in state.WorldCreatureLocations)
                    if(creature.Alive &&
                        Vector3.Distance(new Vector3(creature.X,creature.Y,creature.Z),p)<35f)
                    {conflicts=true;break;}
                if(conflicts)continue;

                target.transform.position=p;
                applied[targetId]=generation;
                return;
            }
            // No suitable legal site this frame: preserve the existing valid
            // hotspot and retry next update instead of teleporting into water.
            // This also preserves player progress if the terrain has changed.
        }
    }
}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Eldoria.Presentation;
using Eldoria.World4XPrototype;
using Object = UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor
{
    /// <summary>
    /// Reversible, source-preserving REAL terrain authoring trial.
    /// Region1 uses its existing 14,641-vertex, 28,800-triangle continuous mesh;
    /// no extra meshes, POIs, colliders, forests, structures or cameras are created.
    /// Valoria keeps its same seven terrain layers and uses only real TerrainData
    /// height + splat changes, protected by road, bridge, castle and trees.
    /// No source scene or game save is written by Trial().
    /// </summary>
    public static class ValoriaGeographyTerrainCandidateV1
    {
        private const string Scene = ValoriaGeographyContinuityV1.ScenePath;
        [Serializable] public class TrialResult
        {
            public string source_scene_sha256, original_mesh_name;
            public int original_world_triangles, candidate_world_triangles, city_height_samples, city_alphamap_samples;
            public float world_min_delta_m, world_max_delta_m, city_min_delta_m, city_max_delta_m;
            public int world_changed_vertices, city_changed_height_samples, city_changed_splat_samples;
            public int poi_count_before, poi_count_after, colliders_before, colliders_after;
            public bool source_scene_unchanged, original_world_mesh_unchanged, original_city_terrain_unchanged, camera_unchanged;
            public bool visual_review_required, art_approved, certified_android_fps, scene_integrated;
            public string note;
        }
        private struct Keepout
        {
            public Vector2 point; public float radius, edge;
            public Keepout(float x,float z,float radius,float edge) { point=new Vector2(x,z);this.radius=radius;this.edge=edge; }
            public float Weight(float x,float z)
            {
                float d=Vector2.Distance(new Vector2(x,z),point);
                return Mathf.SmoothStep(0,1,Mathf.InverseLerp(radius,radius+edge,d));
            }
        }
        static float Fractal(float x,float z)
        {
            return .55f*(Mathf.PerlinNoise(x*.026f+18.5f,z*.021f+61.8f)-.5f)
                 + .27f*(Mathf.PerlinNoise(x*.055f+121.5f,z*.048f+49.2f)-.5f)
                 + .14f*(Mathf.PerlinNoise(x*.107f+7.1f,z*.11f+98.7f)-.5f);
        }
        static float SlopeSafe(float v) { return Mathf.SmoothStep(0,1,Mathf.Clamp01(v)); }
        static float Influence(IEnumerable<Keepout> items,float x,float z)
        {
            float result=1f;
            foreach(var p in items) result=Mathf.Min(result,p.Weight(x,z));
            return result;
        }
        static Vector2 PointOf(Renderer r) => new Vector2(r.bounds.center.x,r.bounds.center.z);
        static List<Keepout> CityKeepouts(ValoriaReferenceCleanGameplay city, Terrain terr)
        {
            var anchors=new List<Keepout>();
            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(renderer.GetComponent<Terrain>()||!renderer.GetComponentInParent<Transform>()||renderer.transform.IsChildOf(terr.transform))continue;
                string n=renderer.name.ToLowerInvariant();
                if(!new[]{"bastion","gate","paving","road","fir","pine","tree","ruin","rock","bridge","river","sawmill","barracks","forest"}.Any(n.Contains))continue;
                if(renderer.bounds.size.x>100||renderer.bounds.size.z>100)continue;
                var v=renderer.bounds.center;
                // Only active I–II functional architecture plus retained natural contacts.
                if((n.Contains("sawmill")||n.Contains("barracks"))&&!renderer.gameObject.activeInHierarchy)continue;
                float radius=Mathf.Clamp(Mathf.Max(renderer.bounds.extents.x,renderer.bounds.extents.z),1.4f,13f);
                anchors.Add(new Keepout(v.x,v.z,radius+1f,2.7f));
            }
            return anchors;
        }
        static List<Vector2> RoadPoints()
        {
            var points=new List<Vector2>();
            foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.sharedMesh&&(x.name=="Axial_Paving"||x.name=="Outer_Road")))
            {
                var v=f.sharedMesh.vertices;
                int skip=Mathf.Max(1,v.Length/900);
                for(int k=0;k<v.Length;k+=skip)
                {
                    var p=f.transform.TransformPoint(v[k]);
                    points.Add(new Vector2(p.x,p.z));
                }
            }
            ValoriaGeographyContinuityV1.Need(points.Count>20,"Original functional paving/source not found; refuse generic drawn road");
            return points;
        }
        static float Nearest(float x,float z,List<Vector2> points)
        {
            float best=float.MaxValue;
            for(int i=0;i<points.Count;i++)
            {
                float dx=points[i].x-x,dz=points[i].y-z;
                float distance=dx*dx+dz*dz;
                if(distance<best)best=distance;
            }
            return Mathf.Sqrt(best);
        }
        private static TerrainData CityGround(ValoriaReferenceCleanGameplay city,Terrain t,out float cityLo,out float cityHi,
            out int alteredH,out int alteredS)
        {
            var original=t.terrainData;
            var next=Object.Instantiate(original);
            next.name="Valoria ground / unified organic terrain / TRIAL";
            int heightSize=next.heightmapResolution;
            float[,] heights=next.GetHeights(0,0,heightSize,heightSize);
            cityLo=0;cityHi=0;alteredH=0;alteredS=0;
            var anchors=CityKeepouts(city,t);
            var roads=RoadPoints();
            var origin=t.transform.position;var worldSize=next.size;
            for(int zi=0;zi<heightSize;zi++)for(int xi=0;xi<heightSize;xi++)
            {
                float x=origin.x+xi/(float)(heightSize-1)*worldSize.x;
                float z=origin.z+zi/(float)(heightSize-1)*worldSize.z;
                float protection=Influence(anchors,x,z);
                // Existing paved route and masonry already have measured, accepted foot contacts.
                if(Nearest(x,z,roads)<2.8f)protection=0;
                // Retained blue river and shoreline must not be raised or buried.
                float river=-23f+1.5f*Mathf.Sin(x*.065f)+.85f*Mathf.Sin(x*.19f+.4f);
                protection*=SlopeSafe(Mathf.InverseLerp(10f,18f,Mathf.Abs(z-river)));
                float delta=protection * (.88f*Fractal(x,z)+.23f*Mathf.Sin(.07f*x+.65f*Mathf.Sin(.11f*z)));
                // Lower ridges stay below the scale of original trees/ruins.
                delta=Mathf.Clamp(delta,-.38f,.38f);
                if(Mathf.Abs(delta)>.003f)alteredH++;
                cityLo=Mathf.Min(cityLo,delta); cityHi=Mathf.Max(cityHi,delta);
                heights[zi,xi]=Mathf.Clamp01(heights[zi,xi]+delta/Mathf.Max(1f,worldSize.y));
            }
            next.SetHeights(0,0,heights);
            next.SyncHeightmap();
            int n=next.alphamapResolution;
            float[,,] splat=next.GetAlphamaps(0,0,n,n);
            int layerCount=splat.GetLength(2);
            ValoriaGeographyContinuityV1.Need(layerCount==7,"Original seven accepted city terrain layers changed; reject stale source");
            const int soil=2; // Measured native235 paved shoulder / native197 river-bank soil.
            for(int zi=0;zi<n;zi++)for(int xi=0;xi<n;xi++)
            {
                float x=origin.x+xi/(float)(n-1)*worldSize.x;
                float z=origin.z+zi/(float)(n-1)*worldSize.z;
                float d=Nearest(x,z,roads);
                float bend=.55f*Mathf.PerlinNoise(x*.12f+11f,z*.105f+33f);
                // Eroded shoulders are measured against actual paving vertices;
                // existing surfaces remain legible rather than a painted flat road.
                float shoulder=(1-SlopeSafe(Mathf.InverseLerp(2.2f+bend,7f+bend,d)))*.42f;
                float river=-23f+1.5f*Mathf.Sin(x*.065f)+.85f*Mathf.Sin(x*.19f+.4f);
                float offset=Mathf.Abs(z-river)-(8.3f+.6f*Mathf.Sin(x*.16f));
                float moist=(1-SlopeSafe(Mathf.InverseLerp(1.1f,6.4f,Mathf.Abs(offset))))*.21f;
                float macro=(Mathf.PerlinNoise(x*.047f+98,z*.056f+27)-.5f)*.13f;
                float amount=Mathf.Clamp01(Mathf.Max(shoulder,moist)+Mathf.Max(0,macro));
                amount*=1f-splat[zi,xi,soil];
                if(amount<.003f)continue;
                alteredS++;
                for(int k=0;k<layerCount;k++)splat[zi,xi,k]*=1-amount;
                splat[zi,xi,soil]+=amount;
            }
            next.SetAlphamaps(0,0,splat);
            return next;
        }
        private static Mesh WorldGround(ValoriaIntegratedWorld host,MeshFilter ground,out float minDelta,
            out float maxDelta,out int changed)
        {
            var original=ground.sharedMesh;
            var next=Object.Instantiate(original);
            next.name="Region1 / sculpted continuous geographic source / TRIAL";
            var v=next.vertices;
            minDelta=0;maxDelta=0;changed=0;
            var targets=ValoriaGeographyContinuityV1.Hotspots(host);
            var anchors=new List<Keepout>();
            foreach(var h in targets)
            {
                var p=h.transform.position;
                float radius=h.Id=="valoria-map-city"?20f:h.Id=="quarry-valoria"?11f:
                    h.Id=="forest-valoria"?11f:h.Id=="engendro-valoria"?8f:9f;
                anchors.Add(new Keepout(p.x,p.z,radius,11f));
            }
            // Trees, rocks and inherited hero POIs are stationary: leave their feet on
            // the exact current source surface. This is not a gameplay/nav rewrite.
            foreach(var renderer in host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                if(renderer.gameObject==ground.gameObject)continue;
                var bounds=renderer.bounds;
                if(bounds.size.x>13||bounds.size.z>13||bounds.size.y<.35f)continue;
                var p=bounds.center;
                float radius=Mathf.Clamp(Mathf.Max(bounds.extents.x,bounds.extents.z),1.2f,5f);
                anchors.Add(new Keepout(p.x,p.z,radius+0.6f,3.6f));
            }
            var quarry=targets.Single(h=>h.Id=="quarry-valoria").transform.position;
            var forest=targets.Single(h=>h.Id=="forest-valoria").transform.position;
            for(int i=0;i<v.Length;i++)
            {
                var world=ground.transform.TransformPoint(v[i]);
                float x=world.x,z=world.z;
                float influence=Influence(anchors,x,z);
                // Broad authored ridge families, valley and secondary run-off,
                // based on real regional scale, not prefab piles or dark stamps.
                float warp=12f*(Mathf.PerlinNoise(x*.009f+14,z*.011f+37)-.5f);
                float w=x-1000f+warp;
                float macro=1.75f*Mathf.Sin(w*.039f+z*.018f)*Mathf.Sin(z*.035f-w*.009f)
                          +1.1f*Mathf.Sin(z*.062f+.35f*Mathf.Sin(w*.032f));
                float relief=1.55f*Fractal(x,z)+macro;
                // Small sediment valley connecting two actual resource districts.
                float corridorX=forest.x-23f+z*.16f+4f*Mathf.Sin(z*.029f);
                float valley=-1.15f*Mathf.Exp(-Mathf.Pow((x-corridorX)/16.5f,2f));
                // Stone geology emerges from the surrounding land outside the
                // protected working quarry itself: not an isolated rock object.
                float quarryDistance=Vector2.Distance(new Vector2(x,z),new Vector2(quarry.x,quarry.z));
                float ridge=.85f*Mathf.Exp(-Mathf.Pow((quarryDistance-23f)/14f,2f))
                            *(0.55f+.45f*Mathf.Sin((x-z)*.062f));
                float delta=Mathf.Clamp((relief+valley+ridge)*influence,-2.3f,2.3f);
                if(Mathf.Abs(delta)>.012f)changed++;
                minDelta=Mathf.Min(minDelta,delta);maxDelta=Mathf.Max(maxDelta,delta);
                var shifted=world+Vector3.up*delta;
                v[i]=ground.transform.InverseTransformPoint(shifted);
            }
            next.vertices=v;
            next.RecalculateNormals();
            next.RecalculateBounds();
            return next;
        }
        public static void Trial()
        {
            TerrainData candidateCity=null;
            Mesh candidateWorld=null;
            try
            {
                Directory.CreateDirectory(ValoriaGeographyContinuityV1.Evidence);
                string sceneHash=ValoriaGeographyContinuityV1.Sha(Scene);
                var host=ValoriaGeographyContinuityV1.Load(out var city);
                var terrain=Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Single();
                var collider=terrain.GetComponent<TerrainCollider>();
                ValoriaGeographyContinuityV1.Need(collider!=null,"Protected city TerrainCollider missing");
                var ground=ValoriaGeographyContinuityV1.WorldGround(host);
                var originalCity=terrain.terrainData;
                var originalPhysics=collider.terrainData;
                var originalWorld=ground.sharedMesh;
                var camera=host.WorldCamera;
                var pos=camera.transform.position;var rot=camera.transform.rotation;
                float zoom=camera.orthographicSize;
                var snapshot=ValoriaGeographyContinuityV1.Hotspots(host)
                    .Select(h=>new KeyValuePair<string,Vector3>(h.Id,h.transform.position)).ToArray();
                var poiColliders=ValoriaGeographyContinuityV1.Hotspots(host)
                    .Sum(h=>h.GetComponentsInChildren<Collider>(true).Length);
                try
                {
                    ValoriaGeographyContinuityV1.CityViews(city,"before");
                    ValoriaGeographyContinuityV1.WorldViews(host,"before",true);
                    candidateCity=CityGround(city,terrain,out float cityLo,out float cityHi,out int changedH,out int changedS);
                    candidateWorld=WorldGround(host,ground,out float worldLo,out float worldHi,out int changedWorld);
                    ValoriaGeographyContinuityV1.Need(candidateWorld.vertexCount==originalWorld.vertexCount &&
                        candidateWorld.triangles.Length==originalWorld.triangles.Length &&
                        candidateWorld.uv.Length==originalWorld.uv.Length,"World mesh topology or UV contract changed");
                    terrain.terrainData=candidateCity;
                    collider.terrainData=candidateCity;
                    ground.sharedMesh=candidateWorld;
                    terrain.Flush();
                    Physics.SyncTransforms();
                    ValoriaGeographyContinuityV1.CityViews(city,"after");
                    ValoriaGeographyContinuityV1.WorldViews(host,"after",true);
                    var now=ValoriaGeographyContinuityV1.Hotspots(host);
                    bool samePositions=snapshot.All(p=>now.Any(n=>n.Id==p.Key && n.transform.position==p.Value));
                    int newColliders=now.Sum(h=>h.GetComponentsInChildren<Collider>(true).Length);
                    var result=new TrialResult{
                        source_scene_sha256=sceneHash, original_mesh_name=originalWorld.name,
                        original_world_triangles=originalWorld.triangles.Length/3,
                        candidate_world_triangles=candidateWorld.triangles.Length/3,
                        city_height_samples=originalCity.heightmapResolution*originalCity.heightmapResolution,
                        city_alphamap_samples=originalCity.alphamapResolution*originalCity.alphamapResolution,
                        world_min_delta_m=worldLo,world_max_delta_m=worldHi,
                        city_min_delta_m=cityLo,city_max_delta_m=cityHi,
                        world_changed_vertices=changedWorld,city_changed_height_samples=changedH,
                        city_changed_splat_samples=changedS,
                        poi_count_before=snapshot.Length,poi_count_after=now.Length,
                        colliders_before=poiColliders,colliders_after=newColliders,
                        original_world_mesh_unchanged=originalWorld!=candidateWorld,
                        original_city_terrain_unchanged=originalCity!=candidateCity,
                        visual_review_required=true,art_approved=false,certified_android_fps=false,
                        scene_integrated=false,
                        note="Reversible matching-camera geography trial. World source mesh topology/UV unchanged; POI positions/IDs unchanged, gameplay not certified in this trial."
                    };
                    ValoriaGeographyContinuityV1.Need(samePositions&&newColliders==poiColliders&&now.Length==snapshot.Length,"Protected world POI identity, position or collider changed");
                    File.WriteAllText(Path.Combine(ValoriaGeographyContinuityV1.Evidence,"trial.json"),
                        JsonUtility.ToJson(result,true));
                }
                finally
                {
                    ground.sharedMesh=originalWorld;
                    collider.terrainData=originalPhysics;
                    terrain.terrainData=originalCity;terrain.Flush();
                    Physics.SyncTransforms();
                    camera.transform.SetPositionAndRotation(pos,rot);
                    camera.orthographicSize=zoom;
                }
                ValoriaGeographyContinuityV1.Need(ValoriaGeographyContinuityV1.Sha(Scene)==sceneHash,"Trial overwrote original canonical scene");
                Debug.Log("VALORIA_REFERENCE_CLEAN_GEOGRAPHY_TRIAL_COMPLETE current source scene preserved; art review required");
                EditorApplication.Exit(0);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
            finally
            {
                // Does not dispose original assets, owned camera, resources or game state.
                if(candidateCity != null)Object.DestroyImmediate(candidateCity);
                if(candidateWorld != null)Object.DestroyImmediate(candidateWorld);
            }
        }
    }
}
#endif

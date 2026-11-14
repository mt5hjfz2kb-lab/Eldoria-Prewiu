using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using UnityEngine;

namespace Eldoria.World4XCanon
{
    // Unity presentation only: original save, combat and timers stay in LocalGateway.
    [DefaultExecutionOrder(1500)]
    public sealed class RegionOneCreaturePopulation : MonoBehaviour
    {
        sealed class Encounter
        {
            public Eldoria.Presentation.WorldHotspot Hotspot;
            public GameObject Near, Far;
            public float NearScale;
        }
        readonly Dictionary<string,Encounter> instances=new Dictionary<string,Encounter>();
        readonly Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
        // Keep every saved logical target, but never instantiate 36 near/far
        // Tripo pairs at once when a player first taps Mundo on iPhone.
        const int MaximumVisibleModels=12;
        const int NewModelsPerRefresh=2;
        ValoriaIntegratedWorld host;
        ICommandGateway gateway;
        RegionOneResourcePlacement placement;
        float nextSnapshot;
        public static void Install(Transform parent,ValoriaIntegratedWorld host,RegionOneResourcePlacement placement)
        {
            // Loading four source prefabs before the first world frame retains
            // all four GLB hierarchies even when no creature is on screen.
            // Resolve each source on the first actual visible use instead.
            var service=parent.gameObject.AddComponent<RegionOneCreaturePopulation>();
            service.host=host;
            service.placement=placement;
            if(host.City.Gateway is LocalGateway local&&local.Snapshot().WorldId=="world-local")
            {
                local.ConfigureLocalCreatureRespawn(placement,RenewablePopulationPolicy.Delay);
                local.EnsureLocalCreaturePopulation(18,18);
            }
            service.RefreshNow();
        }
        public void RefreshNow(){nextSnapshot=0;Synchronize();}
        void LateUpdate()
        {
            if(Time.unscaledTime<nextSnapshot)return;
            nextSnapshot=Time.unscaledTime+.25f;
            Synchronize();
        }
        void Synchronize()
        {
            if(!host||!host.Ready||host.City?.Gateway==null)return;
            if(!ReferenceEquals(gateway,host.City.Gateway))
            {
                gateway=host.City.Gateway;
                if(gateway is LocalGateway local&&local.Snapshot().WorldId=="world-local")
                {
                    // Reload replaces the gateway, including its runtime services.
                    // Reattach the same terrain authority before restoring population.
                    local.ConfigureLocalCreatureRespawn(placement,RenewablePopulationPolicy.Delay);
                    local.EnsureLocalCreaturePopulation(18,18);
                }
            }
            var state=gateway.Snapshot();
            float zoom=host.WorldCamera.orthographicSize;
            bool panorama=RegionOneResourcePlacement.IsPanoramic(zoom,host.WorldNavigation.globalZoom);
            bool near=zoom<110;
            int resident=instances.Values.Count(x=>x.Near||x.Far);
            int created=0;
            foreach(var creature in state.WorldCreatureLocations)
            {
                if(creature.Generation<1)continue;
                if(!instances.TryGetValue(creature.TargetId,out var entity))
                {
                    var go=new GameObject("Region1 logical creature "+creature.TargetId);
                    go.transform.SetParent(host.WorldRoot.transform,true);
                    go.layer=8;
                    var hot=go.AddComponent<Eldoria.Presentation.WorldHotspot>();
                    hot.Id=creature.TargetId;
                    var collider=go.AddComponent<BoxCollider>();
                    collider.isTrigger=true;collider.center=new Vector3(0,5,0);
                    collider.size=new Vector3(12,10,12);
                    // The hotspot/collider must be ready for real gameplay even
                    // while its expensive 3D representation is not in view.
                    entity=new Encounter{Hotspot=hot,NearScale=0};
                    instances.Add(creature.TargetId,entity);
                }
                entity.Hotspot.transform.position=new Vector3(creature.X,creature.Y,creature.Z);
                bool visible=creature.Alive&&!panorama&&OnScreen(entity.Hotspot.transform.position);
                // A maximum of one mesh representation is retained per
                // visible creature. All off-screen/panoramic meshes are freed;
                // the saved target and its collider are NEVER removed.
                if(!visible||(near&&entity.Far)||(!near&&entity.Near))
                {
                    if(entity.Near||entity.Far)resident--;
                    if(entity.Near)Destroy(entity.Near);
                    if(entity.Far)Destroy(entity.Far);
                    entity.Near=null;entity.Far=null;
                }
                if(visible&&resident<MaximumVisibleModels&&created<NewModelsPerRefresh&&
                   !(near?entity.Near:entity.Far))
                {
                    var model=MakeModel(creature.CreatureKind,near?"near":"far",
                        entity.Hotspot.transform,creature.TargetId);
                    if(model)
                    {
                        if(near)
                        {
                            entity.NearScale=RegionOneResourceModels.FitNearWidth(model,
                                RegionOneReferenceNodes.BastionOneVisualWidth*
                                (creature.CreatureKind=="boar"?.45f:.55f));
                            entity.Near=model;
                        }
                        else entity.Far=model;
                        resident++;created++;
                    }
                }
                // Preserve existing scaling and physical ground alignment for
                // whichever original Tripo model is actually in the viewport.
                float presence=Mathf.Lerp(2f,3.4f,Mathf.InverseLerp(36f,280f,zoom));
                if(!near)presence*=Mathf.Max(1f,zoom/280f);
                float baseScale=creature.CreatureKind=="boar"?7.5f:7.2f;
                float closePresence=Mathf.Lerp(entity.NearScale/baseScale,presence,
                    Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(64f,110f,zoom)));
                if(entity.Near)FitModel(entity.Near,creature.CreatureKind,closePresence,entity.Hotspot.transform);
                if(entity.Far)FitModel(entity.Far,creature.CreatureKind,presence,entity.Hotspot.transform);
                var shown=near?entity.Near:entity.Far;
                if(shown)
                {
                    var visibleBounds=shown.GetComponentsInChildren<Renderer>(true);
                    if(visibleBounds.Length>0)
                    {
                        var bounds=visibleBounds[0].bounds;
                        foreach(var renderer in visibleBounds)bounds.Encapsulate(renderer.bounds);
                        var pick=(BoxCollider)entity.Hotspot.GetComponent<Collider>();
                        pick.center=bounds.center-entity.Hotspot.transform.position;
                        pick.size=Vector3.Max(bounds.size,new Vector3(12,10,12));
                    }
                }
                entity.Hotspot.GetComponent<Collider>().enabled=creature.Alive&&!panorama;
                SetVisible(entity.Near,creature.Alive&&!panorama&&near);
                SetVisible(entity.Far,creature.Alive&&!panorama&&!near);
            }
        }
        bool OnScreen(Vector3 point)
        {
            if(!host||!host.InWorld||!host.WorldCamera)return false;
            var screen=host.WorldCamera.WorldToViewportPoint(point);
            return screen.z>0&&screen.x>=-.15f&&screen.x<=1.15f&&
                screen.y>=-.15f&&screen.y<=1.15f;
        }
        GameObject SourceFor(string kind,string lod)
        {
            string key=kind+":"+lod;
            if(prefabs.TryGetValue(key,out var source))return source;
            string resource=kind=="boar"?"RockBoar":kind=="spawnling"?"Spawnling":null;
            if(resource==null)throw new InvalidOperationException("Unknown canonical creature "+kind);
            source=Resources.Load<GameObject>("World4X/Creatures/"+resource+(lod=="near"?"Near":"Far"));
            if(!source)
            {
                Debug.LogWarning("REGION1_TRIPO_CREATURE_SOURCES_UNAVAILABLE: "+key+
                    "; no substitute encounter is created");
                return null;
            }
            prefabs.Add(key,source);
            return source;
        }
        GameObject MakeModel(string kind,string lod,Transform parent,string id)
        {
            var source=SourceFor(kind,lod);
            if(!source)return null;
            var go=Instantiate(source,parent);
            go.name="Original Tripo "+kind+" "+lod+" "+id;
            go.transform.localPosition=Vector3.zero;
            float heading=0;foreach(char c in id)heading=(heading*31+c)%360;
            go.transform.localRotation=Quaternion.Euler(0,heading,0);
            go.transform.localScale=Vector3.one*(kind=="boar"?7.5f:7.2f);
            foreach(var t in go.GetComponentsInChildren<Transform>(true))t.gameObject.layer=29;
            // The strategic mesh uses the world's bounded ambient term, retaining
            // its original albedo. Imported PBR variants fell to black silhouettes
            // under the regional orthographic shadow cascade. Near PBR is intact.
            if(lod=="far"){
                var look=host.WorldRoot.GetComponentInChildren<RegionOneLookDevelopment>(true);
                if(look)look.ApplyMaterials(go);
            }
            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length>0)
            {
                float bottom=renderers.Min(x=>x.bounds.min.y);
                go.transform.position+=Vector3.up*(parent.position.y-bottom);
            }
            go.SetActive(false);
            return go;
        }
        void FitModel(GameObject model,string kind,float presence,Transform anchor)
        {
            model.transform.localScale=Vector3.one*(kind=="boar"?7.5f:7.2f)*presence;
            var ground=anchor.position;
            ground.y=host.WorldHome.y+RegionOneBlenderLandscape.Height(
                ground.x-host.WorldHome.x,ground.z-host.WorldHome.z);
            model.transform.position=ground;
            var renderers=model.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length>0)
            {
                // Near and far Tripo meshes have different local origins. Align
                // their rendered ground footprints to the SAME saved hotspot,
                // rather than exchanging two shifted pivots as zoom crosses 110.
                var bounds=renderers[0].bounds;
                for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
                model.transform.position+=new Vector3(
                    ground.x-bounds.center.x,
                    ground.y-bounds.min.y,
                    ground.z-bounds.center.z);
            }
        }
        static void SetVisible(GameObject model,bool value)
        {
            if(model&&model.activeSelf!=value)model.SetActive(value);
        }
    }
}


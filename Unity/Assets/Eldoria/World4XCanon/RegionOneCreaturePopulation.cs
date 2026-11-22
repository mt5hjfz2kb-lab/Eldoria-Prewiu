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
            public MeshRenderer Painted;
            public float PaintScale=-1;
        }
        readonly Dictionary<string,Encounter> instances=new Dictionary<string,Encounter>();
        readonly Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
        ValoriaIntegratedWorld host;
        ICommandGateway gateway;
        RegionOneResourcePlacement placement;
        float nextSnapshot;
        RegionOneModularArt modularArt;
        MaterialPropertyBlock paintProperties;
        public static void Install(Transform parent,ValoriaIntegratedWorld host,RegionOneResourcePlacement placement)
        {
            var modular=parent.GetComponentInChildren<RegionOneModularArt>();
            // An illustrated encounter does not require two hidden Tripo
            // render LODs, two sets of mesh bounds, or their source prefabs.
            // Combat, respawn, target ID and collider remain authoritative.
            var boarNear=modular?null:Resources.Load<GameObject>("World4X/Creatures/RockBoarNear");
            var boarFar=modular?null:Resources.Load<GameObject>("World4X/Creatures/RockBoarFar");
            var spawnNear=modular?null:Resources.Load<GameObject>("World4X/Creatures/SpawnlingNear");
            var spawnFar=modular?null:Resources.Load<GameObject>("World4X/Creatures/SpawnlingFar");
            if(!modular&&(!boarNear||!boarFar||!spawnNear||!spawnFar))
            {
                Debug.LogWarning("REGION1_TRIPO_CREATURE_SOURCES_UNAVAILABLE: no fake encounters instantiated");
                return;
            }
            var service=parent.gameObject.AddComponent<RegionOneCreaturePopulation>();
            service.modularArt=modular;
            service.paintProperties=new MaterialPropertyBlock();
            service.host=host;
            service.placement=placement;
            service.prefabs["boar:near"]=boarNear;
            service.prefabs["boar:far"]=boarFar;
            service.prefabs["spawnling:near"]=spawnNear;
            service.prefabs["spawnling:far"]=spawnFar;
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
                    if(modularArt){
                        float width=RegionOneReferenceNodes.BastionOneVisualWidth*
                            (creature.CreatureKind=="boar"?.45f:.55f);
                        entity=new Encounter{Hotspot=hot,NearScale=1f};
                        entity.Painted=modularArt.Place(go.transform,
                            creature.CreatureKind=="boar"?"Boar":"Spawn",width,go.transform.position);
                    }else{
                        var close=MakeModel(creature.CreatureKind,"near",go.transform,creature.TargetId);
                        var far=MakeModel(creature.CreatureKind,"far",go.transform,creature.TargetId);
                        float nearScale=RegionOneResourceModels.FitNearWidth(close,
                            RegionOneReferenceNodes.BastionOneVisualWidth*(creature.CreatureKind=="boar"?.45f:.55f));
                        entity=new Encounter{Hotspot=hot,Near=close,Far=far,NearScale=nearScale};
                    }
                    instances.Add(creature.TargetId,entity);
                }
                entity.Hotspot.transform.position=new Vector3(creature.X,creature.Y,creature.Z);
                if(modularArt){
                    // Sprite, underlying picking collider and world population
                    // are ONE canonical encounter; no old 3D actor is spawned.
                    float width=RegionOneReferenceNodes.BastionOneVisualWidth*
                        (creature.CreatureKind=="boar"?.45f:.55f);
                    float scale=zoom<110?1f:
                        Mathf.Lerp(1f,1.95f,Mathf.InverseLerp(110f,280f,zoom))*
                        Mathf.Max(1f,zoom/280f)*
                        Mathf.Lerp(1f,.70f,Mathf.InverseLerp(280f,860f,zoom));
                    bool visible=creature.Alive&&!panorama;
                    var click=(BoxCollider)entity.Hotspot.GetComponent<Collider>();
                    click.enabled=visible;
                    click.center=new Vector3(0,Mathf.Max(5f,width*scale*.48f),0);
                    click.size=new Vector3(Mathf.Max(12f,width*scale),
                        Mathf.Max(10f,width*scale*1.02f),Mathf.Max(12f,width*scale));
                    var position=entity.Hotspot.transform.position;
                    position.y=host.WorldHome.y+RegionOneBlenderLandscape.Height(
                        position.x-host.WorldHome.x,position.z-host.WorldHome.z)+.08f;
                    entity.Painted.transform.position=position;
                    entity.Painted.enabled=visible;
                    // At strategic zoom keep the original touch area; the painted
                    // animal recedes so the fault and city remain landmarks.
                    float artScale=scale*Mathf.Lerp(1f,.65f,Mathf.InverseLerp(115f,760f,zoom));
                    if(Mathf.Abs(artScale-entity.PaintScale)>.0001f){
                        entity.PaintScale=artScale;
                        paintProperties.SetFloat("_BillboardScale",artScale);
                        entity.Painted.SetPropertyBlock(paintProperties);
                        entity.Painted.localBounds=new Bounds(
                            Vector3.up*width*artScale*.5f,Vector3.one*width*artScale*2f);
                    }
                    continue;
                }
                // Only the non-modular legacy baseline creates two 3D LODs.
                float presence=Mathf.Lerp(2f,3.4f,Mathf.InverseLerp(36f,280f,zoom));
                // Portrait's regional camera frames the same world with a larger
                // orthographic size. Preserve a target's share of viewport height
                // beyond the landscape reference, independently of device pixels.
                if(!near)presence*=Mathf.Max(1f,zoom/280f);
                float baseScale=creature.CreatureKind=="boar"?7.5f:7.2f;
                float closePresence=Mathf.Lerp(entity.NearScale/baseScale,presence,
                    Mathf.SmoothStep(0f,1f,Mathf.InverseLerp(64f,110f,zoom)));
                FitModel(entity.Near,creature.CreatureKind,closePresence,entity.Hotspot.transform);
                FitModel(entity.Far,creature.CreatureKind,presence,entity.Hotspot.transform);
                var shown=near?entity.Near:entity.Far;
                var visibleBounds=shown.GetComponentsInChildren<Renderer>(true);
                if(visibleBounds.Length>0)
                {
                    var bounds=visibleBounds[0].bounds;
                    foreach(var renderer in visibleBounds)bounds.Encapsulate(renderer.bounds);
                    var pick=(BoxCollider)entity.Hotspot.GetComponent<Collider>();
                    pick.center=bounds.center-entity.Hotspot.transform.position;
                    pick.size=Vector3.Max(bounds.size,new Vector3(12,10,12));
                }
                var alive=creature.Alive;
                entity.Hotspot.GetComponent<Collider>().enabled=alive&&!panorama;
                // Hide visual, impostor and shadows in panorama without removing state.
                SetVisible(entity.Near,alive&&!panorama&&near);
                SetVisible(entity.Far,alive&&!panorama&&!near);
                if(entity.Painted){
                    var foot=entity.Hotspot.transform.position;
                    foot.y=host.WorldHome.y+RegionOneBlenderLandscape.Height(foot.x-host.WorldHome.x,foot.z-host.WorldHome.z)+.08f;
                    entity.Painted.transform.position=foot;
                    entity.Painted.enabled=alive&&!panorama;
                    float scale=(near?closePresence:presence)*baseScale/entity.NearScale;
                    scale*=Mathf.Lerp(1f,.71f,Mathf.InverseLerp(280f,860f,zoom));
                    if(Mathf.Abs(scale-entity.PaintScale)>.0001f){
                        entity.PaintScale=scale;
                        paintProperties.SetFloat("_BillboardScale",scale);
                        entity.Painted.SetPropertyBlock(paintProperties);
                        var bounds=entity.Painted.GetComponent<MeshFilter>().sharedMesh.bounds;
                        bounds.size*=scale;
                        entity.Painted.localBounds=bounds;
                    }
                }
            }
        }
        GameObject MakeModel(string kind,string lod,Transform parent,string id)
        {
            var go=Instantiate(prefabs[kind+":"+lod],parent);
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
                if(look && !modularArt)look.ApplyMaterials(go);
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


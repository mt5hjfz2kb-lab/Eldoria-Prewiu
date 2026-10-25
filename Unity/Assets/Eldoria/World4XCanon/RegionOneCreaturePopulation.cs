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
        }
        readonly Dictionary<string,Encounter> instances=new Dictionary<string,Encounter>();
        readonly Dictionary<string,GameObject> prefabs=new Dictionary<string,GameObject>();
        ValoriaIntegratedWorld host;
        ICommandGateway gateway;
        float nextSnapshot;
        public static void Install(Transform parent,ValoriaIntegratedWorld host,RegionOneResourcePlacement placement)
        {
            var boarNear=Resources.Load<GameObject>("World4X/Creatures/RockBoarNear");
            var boarFar=Resources.Load<GameObject>("World4X/Creatures/RockBoarFar");
            var spawnNear=Resources.Load<GameObject>("World4X/Creatures/SpawnlingNear");
            var spawnFar=Resources.Load<GameObject>("World4X/Creatures/SpawnlingFar");
            if(!boarNear||!boarFar||!spawnNear||!spawnFar)
            {
                Debug.LogWarning("REGION1_TRIPO_CREATURE_SOURCES_UNAVAILABLE: no fake encounters instantiated");
                return;
            }
            var service=parent.gameObject.AddComponent<RegionOneCreaturePopulation>();
            service.host=host;
            service.prefabs["boar:near"]=boarNear;
            service.prefabs["boar:far"]=boarFar;
            service.prefabs["spawnling:near"]=spawnNear;
            service.prefabs["spawnling:far"]=spawnFar;
            if(host.City.Gateway is LocalGateway local&&local.Snapshot().WorldId=="world-local")
            {
                local.ConfigureLocalCreatureRespawn(placement,TimeSpan.FromMinutes(15));
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
                    local.EnsureLocalCreaturePopulation(18,18);
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
                    var close=MakeModel(creature.CreatureKind,"near",go.transform,creature.TargetId);
                    var far=MakeModel(creature.CreatureKind,"far",go.transform,creature.TargetId);
                    entity=new Encounter{Hotspot=hot,Near=close,Far=far};
                    instances.Add(creature.TargetId,entity);
                }
                entity.Hotspot.transform.position=new Vector3(creature.X,creature.Y,creature.Z);
                // Both source LODs use the same continuous scale at their switching distance.
                float presence=Mathf.Lerp(2f,6f,Mathf.InverseLerp(36f,280f,zoom));
                // Portrait's regional camera frames the same world with a larger
                // orthographic size. Preserve a target's share of viewport height
                // beyond the landscape reference, independently of device pixels.
                if(!near)presence*=Mathf.Max(1f,zoom/280f);
                FitModel(entity.Near,creature.CreatureKind,presence,entity.Hotspot.transform);
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
            var renderers=go.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length>0)
            {
                float bottom=renderers.Min(x=>x.bounds.min.y);
                go.transform.position+=Vector3.up*(parent.position.y-bottom);
            }
            go.SetActive(false);
            return go;
        }
        static void FitModel(GameObject model,string kind,float presence,Transform anchor)
        {
            model.transform.localScale=Vector3.one*(kind=="boar"?7.5f:7.2f)*presence;
            model.transform.position=anchor.position;
            var renderers=model.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length>0)
                model.transform.position+=Vector3.up*(anchor.position.y-renderers.Min(r=>r.bounds.min.y));
        }
        static void SetVisible(GameObject model,bool value)
        {
            if(model&&model.activeSelf!=value)model.SetActive(value);
        }
    }
}

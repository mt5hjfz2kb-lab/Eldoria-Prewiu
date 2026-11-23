using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// Small spatial world, not a sliced illustration: connected ground texture chunks,
    /// two independent structures, local props, actual moving agents and foot-Y sorting.
    /// World geometry and logic are intentionally independent of the canonical city scene.
    /// </summary>
    [DefaultExecutionOrder(-400)]
    public sealed class Modular25DWorld : MonoBehaviour
    {
        void OnDestroy() { Modular25DArt.ReleaseCitySprites(); }
        public GameObject ManagementHudPrefab;
        public Camera ViewCamera { get; private set; }
        public Modular25DGameplay Gameplay { get; private set; }
        public Modular25DInterface Hud { get; private set; }
        public SpriteRenderer BastionRenderer { get; private set; }
        public SpriteRenderer SawmillRenderer { get; private set; }
        public int DisplayedBastionLevel { get; private set; } = 1;
        public SpriteRenderer BarracksRenderer { get; private set; }
        public SpriteRenderer GranaryRenderer { get; private set; }
        public SpriteRenderer StoneworksRenderer { get; private set; }
        public int RecoveredPropsPlaced { get; private set; }
        public int WorkshopWorkersPlaced { get; private set; }
        public Modular25DWorkshopAction WorkshopAction { get; private set; }
        public int SettlementZonesPlaced { get; private set; }

        Transform millWorkYard, garrisonYard, grainYard, quarryYard, grainArcade;

        const float GroundSpacingX = 1f;
        const float GroundSpacingY = .5f;
        public static int FootOrder(float y) { return Mathf.RoundToInt(1000f-y*100f); }

        void Awake() => BuildWorld(true);

#if UNITY_EDITOR
        // Genuine Unity Editor camera proof without entering PlayMode. It never
        // opens the beta save, spawns the HUD, or issues any gameplay command.
        public void BuildForNativeEditor() => BuildWorld(false);
#endif

        void BuildWorld(bool withRuntime)
        {
            if(ViewCamera!=null)return;
            // This scene is its own experimental host; no part of the canonical
            // Valoria scene, prefabs, economics, or saves is mutated.
            ViewCamera = SetupCamera();
            Transform ground = NewRoot("Ground - contiguous 8m chunks with painterly grass material");
            Transform buildings = NewRoot("Buildings - foot anchored");
            Transform scenery = NewRoot("Scenery - dynamic occlusion");
            Transform people = NewRoot("People - actual movement");
            Transform paths = NewRoot("Footprints + organic dirt road meshes");

            var plan = ValoriaUrbanPlan.City;
            var terrain = ground.gameObject.AddComponent<Modular25DGround>();
            terrain.Build();
            var landscape = paths.gameObject.AddComponent<Modular25DLandscape>();
            landscape.Build();

            BastionRenderer = AddSprite("Bastion_I_unique", Modular25DArt.Bastion(),
                plan.Keep.Foot, FootOrder(plan.Keep.Foot.y), buildings);
            SawmillRenderer = AddSprite("Aserradero_state", Modular25DArt.Sawmill(),
                plan.Mill.Foot, FootOrder(plan.Mill.Foot.y), buildings);
            BastionRenderer.gameObject.AddComponent<Modular25DArchitecturalShadow>();
            SawmillRenderer.gameObject.AddComponent<Modular25DArchitecturalShadow>();
            AddContact("Bastion_contact", plan.Keep.Foot + Vector2.down*.08f, 300,
                FootOrder(plan.Keep.Foot.y)-2, buildings);
            BarracksRenderer = AddExpansionBuilding(buildings,"barracks",plan.Barracks.Foot);
            GranaryRenderer = AddExpansionBuilding(buildings,"granary",plan.Granary.Foot);
            StoneworksRenderer = AddExpansionBuilding(buildings,"stoneworks",plan.Quarry.Foot);

            millWorkYard = NewChildRoot(scenery,"Aserradero | acopio y carga");
            garrisonYard = NewChildRoot(scenery,"Cuartel | patio de armas");
            grainYard = NewChildRoot(scenery,"Granero | mercado y carga");
            quarryYard = NewChildRoot(scenery,"Cantera | frente de extracción");
            var yards = new[]{scenery, millWorkYard, garrisonYard, grainYard, quarryYard};
            foreach (var prop in plan.Environment)
            {
                var placed = AddOriginalProp(yards[prop.District],prop.Asset,prop.Foot,prop.Scale);
                if (prop.Asset=="PuestoDeAbastos") grainArcade=placed;
            }
            CreateVillager(people,"Valoria citizen A",plan.ReturnWalk(plan.Keep),0f);
            CreateVillager(people,"Valoria citizen B",plan.ReturnWalk(plan.Mill),1.8f);
            CreateVillager(garrisonYard,"Relevo de guardia",plan.ReturnWalk(plan.Barracks),.8f);
            CreateVillager(grainYard,"Portadora del granero",plan.ReturnWalk(plan.Granary),2.3f);
            CreateVillager(quarryYard,"Transportista de cantera",plan.ReturnWalk(plan.Quarry),1.35f);
            CreateWorkshopWorker(people,plan.Mill.Foot+new Vector2(1.02f,-.46f));
            foreach (var yard in new[]{millWorkYard,garrisonYard,grainYard,quarryYard}) yard.gameObject.SetActive(false);
            SettlementZonesPlaced=4;
            if(withRuntime)
            {
                Gameplay=gameObject.AddComponent<Modular25DGameplay>();
                Hud=gameObject.AddComponent<Modular25DInterface>();
                gameObject.AddComponent<Modular25DCameraInput>();
            }
        }

        static Transform NewChildRoot(Transform parent,string name)
        {
            var obj=new GameObject(name);
            obj.transform.SetParent(parent,false);
            return obj.transform;
        }

        Transform NewRoot(string name)
        {
            var obj=new GameObject(name);
            obj.transform.SetParent(transform,false);
            return obj.transform;
        }

        Camera SetupCamera()
        {
            var root=new GameObject("Orthographic camera - fixed isometric projection");
            root.transform.SetParent(transform,false);
            root.transform.position=Modular25DCameraInput.HomePosition;
            root.transform.rotation=Quaternion.identity;
            var camera=root.AddComponent<Camera>();
            camera.orthographic=true;
            camera.orthographicSize=Modular25DCameraInput.HomeSizeForAspect(390f/844f);
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.24f,.30f,.26f);
            camera.nearClipPlane=.1f;
            camera.farClipPlane=100f;
            camera.allowHDR=false;
            camera.allowMSAA=false;
            root.tag="MainCamera";
            return camera;
        }

        static SpriteRenderer AddSprite(string name,Sprite sprite,Vector2 pos,int order,Transform parent)
        {
            var go=new GameObject(name);
            go.transform.SetParent(parent,false);
            go.transform.localPosition=new Vector3(pos.x,pos.y,0);
            var renderer=go.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite;
            renderer.sortingOrder=order;
            return renderer;
        }

        void AddContact(string name,Vector2 position,int width,int order,Transform parent)
        {
            var shadow=AddSprite(name,Modular25DArt.Shadow(width,28),position,order,parent);
            shadow.color=new Color(1f,1f,1f,.7f);
        }

        Transform AddOriginalProp(Transform parent,string name,Vector2 foot,float scale)
        {
            var sprite=Resources.Load<Sprite>("ValoriaModular25D/RecoveredProps/"+name);
            if(sprite==null)
            {
                Debug.LogError("[Valoria Modular25D] Missing original image-authored prop "+
                    name+"; refuse to fake the visual-quality gate.",this);
                return null;
            }
            var visual=AddSprite("Original Eldoria art | "+name,sprite,
                foot,FootOrder(foot.y),parent);
            visual.transform.localScale=Vector3.one*scale;
            // The original sprites include hand-painted ground contact and
            // light/shadow from a consistent isometric direction. A second
            // baked shadow sprite would produce doubled dark blobs.
            RecoveredPropsPlaced++;
            return visual.transform;
        }

        void AddRuin(Transform root,Vector2 foot,int variant,float scale)
        {
            var asset=Modular25DArt.ImperialRuin(variant);
            if(asset==null)return; // Missing art is a failed QA gate, never a fake backdrop.
            var shade=AddSprite("Ruin stone contact",Modular25DArt.Shadow(186,46),
                foot+new Vector2(.19f,-.13f),FootOrder(foot.y)-2,root);
            shade.color=new Color(.71f,.75f,.67f,.72f);
            shade.transform.localScale=Vector3.one*scale;
            var ruin=AddSprite("Pre-Breach ruined arch "+variant,
                asset,foot,FootOrder(foot.y),root);
            ruin.transform.localScale=Vector3.one*scale;
        }

        void AddOak(Transform root,Vector2 foot,int variant,float scale)
        {
            // Warm sunlight from the NW projects the canopy softly toward
            // the SE. A separate translucent world sprite grounds the tree.
            var cast=AddSprite("Oak projected canopy shade",
                Modular25DArt.Shadow(250,84),foot+new Vector2(.46f,-.33f),
                FootOrder(foot.y)-3,root);
            cast.color=new Color(.68f,.75f,.59f,.79f);
            cast.transform.localScale=Vector3.one*scale;
            AddContact("tree_contact",foot,95,FootOrder(foot.y)-1,root);
            var oak=AddSprite("Olive oak "+variant,Modular25DArt.Oak(variant),
                foot,FootOrder(foot.y),root);
            oak.transform.localScale=Vector3.one*scale;
            // Keep trunk/feet grounded; canopy in fixed-depth 2.5D remains
            // an occluder rather than a billboard rotating with camera.
        }

        void AddTree(Transform root,Vector2 foot,int variant,float scale)
        {
            AddContact("tree_contact",foot,87,FootOrder(foot.y)-1,root);
            var tree=AddSprite("Pine "+variant,Modular25DArt.Tree(variant),foot,FootOrder(foot.y),root);
            tree.transform.localScale=Vector3.one*scale;
            tree.gameObject.AddComponent<Modular25DTreeSway>().Seed=variant*1.13f+foot.x;
        }

        void AddProp(Transform root,Sprite sprite,Vector2 foot,float scale=1f)
        {
            var prop=AddSprite("Separate environment prop",sprite,foot,FootOrder(foot.y),root);
            prop.transform.localScale=Vector3.one*scale;
        }

        void CreateVillager(Transform parent,string name,Vector2[] route,float delay)
        {
            var actor=new GameObject(name);
            actor.transform.SetParent(parent,false);
            actor.transform.localPosition=new Vector3(route[0].x,route[0].y,0);
            var shadow=AddSprite("moving contact shadow",Modular25DArt.Shadow(65,18),
                new Vector2(0,-.08f),0,actor.transform);
            var body=AddSprite("Direction-aware four-phase gait",Modular25DArt.Citizen(1,false),
                Vector2.zero,0,actor.transform);
            // Original actor sheet is a much larger pixel figure than the
            // detailed building sprites. Reduce visible body, preserve world
            // foot anchor and Y-sorting for honest occlusion behind trees.
            body.transform.localScale=Vector3.one*.55f;
            shadow.transform.localScale=Vector3.one*.54f;
            var mover=actor.AddComponent<Modular25DWalker>();
            mover.Configure(route,delay,body,shadow);
        }

        void CreateWorkshopWorker(Transform parent,Vector2 foot)
        {
            // Work-specific on-the-spot movement by the illustrated log rack.
            // It is NOT an extra production building, gameplay timer or reward.
            var actor=new GameObject("Lumberyard worker - sawing at timber rack");
            actor.transform.SetParent(parent,false);
            actor.transform.localPosition=new Vector3(foot.x,foot.y,0);
            var contact=AddSprite("worker ground contact",
                Modular25DArt.Shadow(64,17),new Vector2(0,-.07f),
                FootOrder(foot.y)-1,actor.transform);
            // Two painterly original keyframes already have a soft foot/trestle
            // cast. Keep this world contact delicate to avoid a second dark halo.
            contact.transform.localScale=Vector3.one*.43f;
            contact.color=new Color(.73f,.77f,.67f,.22f);
            var body=AddSprite("sawyer at log trestle — original 2.5D keyframe",
                Modular25DArt.Carpenter(0),
                Vector2.zero,FootOrder(foot.y),actor.transform);
            body.transform.localScale=Vector3.one*.81f;
            var action=actor.AddComponent<Modular25DWorkshopAction>();
            action.Configure(body,contact);
            WorkshopAction=action;
            actor.SetActive(false); // No productive sawmill before canonical build completion.
            WorkshopWorkersPlaced++;
        }

        SpriteRenderer AddExpansionBuilding(Transform parent,string id,Vector2 foot)
        {
            var visual=AddSprite("Progression | "+id,null,foot,FootOrder(foot.y),parent);
            // These three authored sprites already include irregular painted
            // ground contact. Projecting their full alpha again creates a
            // conspicuous rectangular band at the near texture edge.
            visual.gameObject.SetActive(false);
            return visual;
        }

        static void SetCompletedBuilding(SpriteRenderer visual,bool visible)
        {
            if(visual==null)return;
            if(visible && visual.sprite==null)
                visual.sprite=Modular25DArt.ExpansionBuilding(visual.gameObject.name.Substring("Progression | ".Length));
            visual.gameObject.SetActive(visible);
        }

        public void Apply(PlayerState snapshot)
        {
            if(snapshot==null||SawmillRenderer==null)return;
            int previouslyDisplayed=DisplayedBastionLevel;
            DisplayedBastionLevel=Mathf.Clamp(snapshot.BastionLevel,1,5);
            BastionRenderer.sprite=Modular25DArt.BastionForLevel(DisplayedBastionLevel);
            BastionRenderer.GetComponent<Modular25DArchitecturalShadow>()?.Refresh();
            SetCompletedBuilding(BarracksRenderer,snapshot.BastionLevel>=2 && snapshot.BarracksLevel>0);
            SetCompletedBuilding(GranaryRenderer,snapshot.BastionLevel>=3 && (snapshot.ChapterThree?.GranaryLevel??0)>0);
            SetCompletedBuilding(StoneworksRenderer,snapshot.BastionLevel>=5 && (snapshot.ChapterThree?.StoneworksLevel??0)>0);
            GetComponentInChildren<Modular25DGround>()?.SetDistricts(
                BarracksRenderer.gameObject.activeSelf,GranaryRenderer.gameObject.activeSelf,StoneworksRenderer.gameObject.activeSelf);
            bool garrison = BarracksRenderer.gameObject.activeSelf;
            GetComponentInChildren<Modular25DLandscape>().SetDistricts(garrison,
                GranaryRenderer.gameObject.activeSelf,StoneworksRenderer.gameObject.activeSelf);
            garrisonYard.gameObject.SetActive(garrison);
            grainYard.gameObject.SetActive(GranaryRenderer.gameObject.activeSelf);
            grainArcade.gameObject.SetActive(DisplayedBastionLevel>=4);
            quarryYard.gameObject.SetActive(StoneworksRenderer.gameObject.activeSelf);
            var stage=ParcelBuildingStates.For(snapshot,"sawmill");
            // Three independently authored visuals; only the canonical snapshot
            // chooses which one is visible. No tinted finished-building ghost.
            millWorkYard.gameObject.SetActive(stage==ParcelBuildingState.BUILT);
            SawmillRenderer.enabled=true;
            SawmillRenderer.color=Color.white;
            SawmillRenderer.sprite=stage==ParcelBuildingState.BUILT
                ? Modular25DArt.Sawmill()
                : stage==ParcelBuildingState.UNDER_CONSTRUCTION
                    ? Modular25DArt.SawmillWorksite() : Modular25DArt.SawmillSite();
            // Native captures may render in the same Editor call without a LateUpdate.
            // Synchronize presentation immediately with the canonical sprite transition.
            SawmillRenderer.GetComponent<Modular25DArchitecturalShadow>()?.Refresh();
            if(WorkshopAction!=null)
                WorkshopAction.gameObject.SetActive(stage==ParcelBuildingState.BUILT);
            // Reframe only on a real Bastion milestone, not on each resource
            // tick, camera gesture or save refresh. The scene's first Start
            // also calls Home to restore the correct saved-level composition.
            if(previouslyDisplayed!=DisplayedBastionLevel)
                GetComponent<Modular25DCameraInput>()?.Home();
        }

        public bool HitSawmill(Vector2 world)
        {
            // World-space footprint, independent of sprite alpha and canvas scaling.
            // Adapt the existing world interaction footprint to the moved
            // building without changing its canonical Build command or save.
            Vector2 local=world-ValoriaUrbanPlan.City.Mill.Foot;
            return new Rect(-1.84f,-.53f,3.76f,3.64f).Contains(local);
        }
    }
}




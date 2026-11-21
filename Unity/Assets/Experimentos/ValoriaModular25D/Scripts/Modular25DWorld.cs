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
        public Camera ViewCamera { get; private set; }
        public Modular25DGameplay Gameplay { get; private set; }
        public Modular25DInterface Hud { get; private set; }
        public SpriteRenderer BastionRenderer { get; private set; }
        public SpriteRenderer SawmillRenderer { get; private set; }
        public int RecoveredPropsPlaced { get; private set; }
        public int WorkshopWorkersPlaced { get; private set; }
        public Modular25DWorkshopAction WorkshopAction { get; private set; }
        public int SettlementZonesPlaced { get; private set; }

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

            // PRODUCTION PIVOT: separate 8x8 m contiguous terrain chunks.
            // A bounded 21x21 diamond carpet visibly ended in the 390x844
            // camera; the continuous repeatable material fixes that root cause.
            // Routes and contact props stay separate and can be reordered.
            var terrain=ground.gameObject.AddComponent<Modular25DGround>();
            terrain.Build();

            var landscape=paths.gameObject.AddComponent<Modular25DLandscape>();
            landscape.Build();
            BastionRenderer=AddSprite("Bastion_I_unique",Modular25DArt.Bastion(),new Vector2(-.40f,1.45f),
                FootOrder(1.3f),buildings);
            SawmillRenderer=AddSprite("Aserradero_state",Modular25DArt.Sawmill(),
                new Vector2(.96f,-2.07f),FootOrder(-2.2f),buildings);
            BastionRenderer.gameObject.AddComponent<Modular25DArchitecturalShadow>();
            SawmillRenderer.gameObject.AddComponent<Modular25DArchitecturalShadow>();
            AddContact("Bastion_contact",new Vector2(-.40f,1.37f),300,FootOrder(1.3f)-2,buildings);
            // The finished illustrated sprite already contains ground contact.
            // Its broad baked exterior shadow is trimmed by Modular25DArt; do
            // not add another large dark ellipse over the sawdust footprint.

            // BASTION I PRODUCTION COMPOSITION: a deliberate rebuilt hamlet,
            // not random RPG decorations on an empty lawn. Four readable zones
            // surround a free western plot. Every object is independent 2.5D
            // and retains real foot-Y sorting/occlusion and original artist PNG.
            var ruins=NewChildRoot(scenery,"01 | Imperial ridge — pre-Breach masonry");
            var grove=NewChildRoot(scenery,"02 | Guarded ancient grove — visual frame");
            var commons=NewChildRoot(scenery,"03 | Rebuilding commons — well and garden");
            var lumber=NewChildRoot(scenery,"04 | Working lumberyard — cart and log stock");

            // The old masonry belongs on one ridge behind the Keep, rather
            // than random low-poly stone fragments beside the lawn.
            AddOriginalProp(ruins,"MuroDerruido",new Vector2(-1.85f,4.14f),.88f);
            AddOriginalProp(ruins,"MatojosFlores",new Vector2(-.20f,4.65f),.54f);
            AddOriginalProp(ruins,"MatojosFlores",new Vector2(-1.53f,3.29f),.63f);

            // Two mature tree groupings frame the keep at different depths.
            // Their intentional open middle preserves the main gate silhouette.
            AddOriginalProp(grove,"ArbolAntiguo",new Vector2(3.37f,3.31f),.88f);
            AddOriginalProp(grove,"ArboledaAntigua",new Vector2(-3.75f,2.45f),.60f);
            AddOriginalProp(grove,"ArboledaAntigua",new Vector2(-3.15f,-1.4f),.85f);
            AddOriginalProp(grove,"ArboledaAntigua",new Vector2(3.04f,1.49f),.78f);
            // The southern foreground is framed only along the extreme left;
            // the centre stays open for future unbuilt plots.
            AddOriginalProp(grove,"ArboledaAntigua",new Vector2(-4.10f,-5.55f),.59f);

            // A well and a recognizable cultivated enclosure give the central
            // yard a social purpose, without introducing any new game mechanic.
            AddOriginalProp(commons,"PozoPiedra",new Vector2(-.37f,-.92f),.71f);
            AddOriginalProp(commons,"HuertosValoria",new Vector2(-1.98f,-4.74f),.67f);
            AddOriginalProp(commons,"MatojosFlores",new Vector2(-2.75f,-5.56f),.45f);

            // Timber staging visibly connects sawmill to the haulers' road.
            // Both props come from the same original painted wood/material kit.
            AddOriginalProp(lumber,"CarroMadera",new Vector2(2.19f,-3.56f),.74f);
            AddOriginalProp(lumber,"VallaLenador",new Vector2(1.40f,-4.17f),.75f);
            AddOriginalProp(lumber,"MatojosFlores",new Vector2(2.83f,-2.09f),.47f);
            AddOriginalProp(lumber,"MatojosFlores",new Vector2(2.10f,.45f),.53f);

            // Low-detail ground silhouettes stay rare and are never mistaken
            // for buildings or construction props.
            AddProp(scenery,Modular25DArt.Tuft(0),new Vector2(-.48f,-3.75f),.46f);
            AddProp(scenery,Modular25DArt.Tuft(1),new Vector2(1.07f,1.05f),.53f);

            CreateVillager(people,"Valoria citizen A",new [] {
                // Real keep gate route: a messenger approaches the courtyard
                // and returns on the curving walkable cobblestone road.
                new Vector2(-3.9f,-3.6f),new Vector2(-2.70f,-2.5f),
                new Vector2(-2.28f,-.40f),new Vector2(-1.23f,.10f),
                new Vector2(-2.28f,-.40f),new Vector2(-2.70f,-2.5f)
            },0f);
            CreateVillager(people,"Valoria citizen B",new [] {
                // A hauler uses the connecting side-road, not a decorative
                // patrol in an unrelated empty part of the meadow.
                new Vector2(-.21f,-3.50f),new Vector2(.57f,-3.33f),
                new Vector2(1.62f,-3.52f),new Vector2(.57f,-3.33f)
            },1.8f);
            CreateWorkshopWorker(people,new Vector2(1.11f,-3.88f));
            SettlementZonesPlaced=4;

            AddNaturalDetail(scenery);
            if(withRuntime)
            {
                Gameplay=gameObject.AddComponent<Modular25DGameplay>();
                Hud=gameObject.AddComponent<Modular25DInterface>();
                gameObject.AddComponent<Modular25DCameraInput>();
            }
        }

        void AddNaturalDetail(Transform root)
        {
            // Ground cover belongs to five ecotone edges, not 29 uniform
            // random locations. Cues: ruined ridge / grove / lane / workshop
            // / cultivated terrace. All are reusable original tuft sprites.
            var random=new System.Random(11813);
            var names=new[]{ "Ridge", "Forest boundary", "Courtyard verge",
                "Lumberyard verge", "Southern growing plot" };
            var centres=new[]{
                new Vector2(-1.2f,4.50f),new Vector2(-3.36f,-2.35f),
                new Vector2(.19f,-.08f),new Vector2(2.68f,-4.55f),
                new Vector2(-2.94f,-5.56f)
            };
            var radii=new[]{
                new Vector2(.95f,.50f),new Vector2(.72f,.82f),
                new Vector2(.72f,.45f),new Vector2(.66f,.80f),
                new Vector2(.81f,.56f)
            };
            var counts=new[]{4,7,4,6,6};
            for(int zone=0;zone<centres.Length;zone++)
            {
                int placed=0;
                for(int attempt=0;attempt<80&&placed<counts[zone];attempt++)
                {
                    float angle=(float)random.NextDouble()*2f*Mathf.PI;
                    float radial=Mathf.Sqrt((float)random.NextDouble());
                    var at=centres[zone]+new Vector2(
                        Mathf.Cos(angle)*radii[zone].x*radial,
                        Mathf.Sin(angle)*radii[zone].y*radial);
                    if(DistanceToRoad(at)<.53f)continue;
                    // Roads, actual entrances, their independent future plots
                    // and the working actor's contact point remain accessible.
                    if(Vector2.Distance(at,new Vector2(-.40f,1.45f))<2.20f||
                       Vector2.Distance(at,new Vector2(.96f,-2.07f))<1.65f||
                       Vector2.Distance(at,new Vector2(1.11f,-3.88f))<.42f)
                        continue;
                    var art=Modular25DArt.Tuft((zone+placed)%2);
                    var sr=AddSprite(names[zone]+" | grass transition "+placed,
                        art,at,FootOrder(at.y),root);
                    float scale=.28f+(float)random.NextDouble()*.18f;
                    sr.transform.localScale=Vector3.one*scale;
                    placed++;
                }
            }
        }

        static float DistanceToRoad(Vector2 point)
        {
            var guide=new []{
                new Vector2(-4.85f,-5.3f),new Vector2(-3.88f,-3.76f),
                new Vector2(-2.70f,-2.5f),new Vector2(-2.63f,-1.56f),
                new Vector2(-2.28f,-.40f),new Vector2(-1.75f,.1f),
                new Vector2(-.40f,1.00f)
            };
            var branch=new[]{
                new Vector2(-2.70f,-2.5f),new Vector2(-1.57f,-3.16f),
                new Vector2(-.21f,-3.50f),new Vector2(.52f,-3.26f),
                new Vector2(1.30f,-2.55f)
            };
            float best=float.MaxValue;
            foreach(var route in new []{guide,branch})
                for(int i=1;i<route.Length;i++)
                {
                    Vector2 span=route[i]-route[i-1];
                    float t=Mathf.Clamp01(Vector2.Dot(point-route[i-1],span)/span.sqrMagnitude);
                    best=Mathf.Min(best,Vector2.Distance(point,route[i-1]+span*t));
                }
            return best;
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

        void AddOriginalProp(Transform parent,string name,Vector2 foot,float scale)
        {
            var sprite=Resources.Load<Sprite>("ValoriaModular25D/RecoveredProps/"+name);
            if(sprite==null)
            {
                Debug.LogError("[Valoria Modular25D] Missing original image-authored prop "+
                    name+"; refuse to fake the visual-quality gate.",this);
                return;
            }
            var visual=AddSprite("Original Eldoria art | "+name,sprite,
                foot,FootOrder(foot.y),parent);
            visual.transform.localScale=Vector3.one*scale;
            // The original sprites include hand-painted ground contact and
            // light/shadow from a consistent isometric direction. A second
            // baked shadow sprite would produce doubled dark blobs.
            RecoveredPropsPlaced++;
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

        public void Apply(PlayerState snapshot)
        {
            if(snapshot==null||SawmillRenderer==null)return;
            var stage=ParcelBuildingStates.For(snapshot,"sawmill");
            // Three independently authored visuals; only the canonical snapshot
            // chooses which one is visible. No tinted finished-building ghost.
            SawmillRenderer.enabled=true;
            SawmillRenderer.color=Color.white;
            SawmillRenderer.sprite=stage==ParcelBuildingState.BUILT
                ? Modular25DArt.Sawmill()
                : stage==ParcelBuildingState.UNDER_CONSTRUCTION
                    ? Modular25DArt.SawmillWorksite() : Modular25DArt.SawmillSite();
            if(WorkshopAction!=null)
                WorkshopAction.gameObject.SetActive(stage==ParcelBuildingState.BUILT);
        }

        public bool HitSawmill(Vector2 world)
        {
            // World-space footprint, independent of sprite alpha and canvas scaling.
            // Adapt the existing world interaction footprint to the moved
            // building without changing its canonical Build command or save.
            return world.x>-.88f && world.x<2.88f && world.y>-2.60f && world.y<1.04f;
        }
    }
}


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
        public SpriteRenderer SawmillRenderer { get; private set; }

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
            AddSprite("Bastion_I_unique",Modular25DArt.Bastion(),new Vector2(-1.15f,1.3f),
                FootOrder(1.3f),buildings);
            SawmillRenderer=AddSprite("Aserradero_state",Modular25DArt.Sawmill(),
                new Vector2(1.95f,-2.2f),FootOrder(-2.2f),buildings);
            AddContact("Bastion_contact",new Vector2(-1.15f,1.22f),300,FootOrder(1.3f)-2,buildings);
            AddContact("Aserradero_contact",new Vector2(1.95f,-2.27f),220,FootOrder(-2.2f)-2,buildings);

            // Shared warm-olive broadleaf art replaces the isolated dark
            // blue low-poly pine cluster. The occlusion test still crosses
            // behind and in front of the SAME physical tree foot anchor.
            // Small independent ancient imperial ruins give the first scene
            // reconstruction lore and depth without a pre-painted panorama.
            // Foot-Y order puts both BEHIND Bastion I and its future levels.
            AddRuin(scenery,new Vector2(-2.71f,5.23f),0,.74f);
            AddRuin(scenery,new Vector2(2.73f,5.03f),1,.68f);
            AddOak(scenery,new Vector2(-3.15f,-1.4f),0,1.17f);
            AddOak(scenery,new Vector2(3.76f,1.55f),1,.95f);
            AddTree(scenery,new Vector2(-4.9f,2.7f),2,.72f);
            AddTree(scenery,new Vector2(4.6f,-4.1f),0,.78f);
            AddOak(scenery,new Vector2(-4.35f,-4.45f),1,.77f);
            AddProp(scenery,Modular25DArt.Bush(0),new Vector2(-2.6f,-3.55f),.58f);
            AddProp(scenery,Modular25DArt.Bush(1),new Vector2(3.25f,-4.15f),.57f);
            AddProp(scenery,Modular25DArt.Rock(),new Vector2(-4.15f,.6f),.58f);
            AddProp(scenery,Modular25DArt.Rock(),new Vector2(4.55f,-.45f),.48f);
            AddProp(scenery,Modular25DArt.Rock(),new Vector2(-.15f,-4.65f),.5f);

            CreateVillager(people,"Valoria citizen A",new [] {
                // Pass on the east edge of the tree crown; keep the feet
                // off its narrow trunk while crossing in front and behind.
                new Vector2(-3.9f,-3.6f),new Vector2(-2.70f,-2.5f),
                new Vector2(-2.62f,-.55f),new Vector2(-2.3f,.1f),
                new Vector2(-2.62f,-.55f),new Vector2(-2.70f,-2.5f)
            },0f);
            CreateVillager(people,"Valoria citizen B",new [] {
                new Vector2(.15f,-4.8f),new Vector2(1.65f,-4.2f),
                new Vector2(2.5f,-3.6f),new Vector2(1.65f,-4.2f)
            },1.8f);

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
            // Constrained small scatter; central building hitboxes and the
            // 0.9-unit main paths remain unobstructed. Never create plinths.
            var random=new System.Random(11813);
            int placed=0;
            for(int attempt=0;attempt<320&&placed<29;attempt++)
            {
                float x=-7f+(float)random.NextDouble()*14f;
                float y=-6.2f+(float)random.NextDouble()*12.5f;
                var point=new Vector2(x,y);
                if(Vector2.Distance(point,new Vector2(-1.15f,1.3f))<2.5f ||
                   Vector2.Distance(point,new Vector2(1.95f,-2.2f))<2.15f ||
                   Vector2.Distance(point,new Vector2(-3.15f,-1.4f))<.64f)
                    continue;
                if(DistanceToRoad(point)<.68f)continue;
                var prop=random.Next(16)==0 ? Modular25DArt.Rock() :
                    random.Next(4)==0?Modular25DArt.Bush(random.Next(3)):
                                      Modular25DArt.Tuft(random.Next(2));
                var draw=AddSprite("Interchangeable grass / stone "+placed,
                    prop,point,FootOrder(point.y),root);
                float scale=.27f+(float)random.NextDouble()*.25f;
                draw.transform.localScale=Vector3.one*scale;
                placed++;
            }
        }

        static float DistanceToRoad(Vector2 point)
        {
            var guide=new []{
                new Vector2(-4.85f,-5.3f),new Vector2(-3.88f,-3.76f),
                new Vector2(-2.70f,-2.5f),new Vector2(-2.63f,-1.56f),
                new Vector2(-2.62f,-.55f),new Vector2(-2.3f,.1f),
                new Vector2(-1.15f,.85f)
            };
            var branch=new[]{
                new Vector2(-2.70f,-2.5f),new Vector2(-1.57f,-3.16f),
                new Vector2(-.21f,-3.50f),new Vector2(1.04f,-3.41f),
                new Vector2(1.95f,-2.55f)
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
            root.transform.position=new Vector3(.18f,.15f,-30f);
            root.transform.rotation=Quaternion.identity;
            var camera=root.AddComponent<Camera>();
            camera.orthographic=true;
            camera.orthographicSize=8.25f;
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
            var body=AddSprite("four-frame gait",Modular25DArt.Villager(0),
                Vector2.zero,0,actor.transform);
            var mover=actor.AddComponent<Modular25DWalker>();
            mover.Configure(route,delay,body,shadow);
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
        }

        public bool HitSawmill(Vector2 world)
        {
            // World-space footprint, independent of sprite alpha and canvas scaling.
            return world.x>.10f && world.x<3.87f && world.y>-2.48f && world.y<.92f;
        }
    }
}

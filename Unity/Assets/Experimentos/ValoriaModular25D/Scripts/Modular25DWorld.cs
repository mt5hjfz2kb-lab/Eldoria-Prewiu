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
    /// Small spatial world, not a sliced illustration: interchangeable ground diamonds,
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

        void Awake()
        {
            // This scene is its own experimental host; no part of the canonical
            // Valoria scene, prefabs, economics, or saves is mutated.
            ViewCamera = SetupCamera();
            Transform ground = NewRoot("Ground - individual 2:1 diamond tiles");
            Transform buildings = NewRoot("Buildings - foot anchored");
            Transform scenery = NewRoot("Scenery - dynamic occlusion");
            Transform people = NewRoot("People - actual movement");

            for(int row=-10;row<=10;row++)
            for(int col=-10;col<=10;col++)
            {
                Vector2 pos=new Vector2((col-row)*GroundSpacingX,(col+row)*GroundSpacingY);
                int seed=Mathf.Abs(col*67+row*97);
                int kind=0;
                // Simple dirt transitions and a winding walkable track.
                if(Mathf.Abs(pos.y + pos.x*.68f + .4f)<.68f && pos.x>-2.4f && pos.x<3.7f) kind=1;
                else if(Mathf.Abs(pos.x-1.9f)<1.35f && Mathf.Abs(pos.y+2f)<1.25f)kind=2;
                else if(seed%19==0)kind=3;
                AddSprite("ground("+col+","+row+")", Modular25DArt.Tile(kind,seed%4),pos,
                    -3000 + col + row,ground);
            }

            AddSprite("Bastion_I_unique",Modular25DArt.Bastion(),new Vector2(-1.15f,1.3f),
                FootOrder(1.3f),buildings);
            SawmillRenderer=AddSprite("Aserradero_state",Modular25DArt.Sawmill(),
                new Vector2(1.95f,-2.2f),FootOrder(-2.2f),buildings);
            AddContact("Bastion_contact",new Vector2(-1.15f,1.22f),300,FootOrder(1.3f)-2,buildings);
            AddContact("Aserradero_contact",new Vector2(1.95f,-2.27f),220,FootOrder(-2.2f)-2,buildings);

            AddTree(scenery,new Vector2(-3.15f,-1.4f),0);
            AddTree(scenery,new Vector2(3.8f,1.6f),1);
            AddTree(scenery,new Vector2(-4.9f,2.7f),2);
            AddTree(scenery,new Vector2(4.6f,-4.1f),0);
            AddProp(scenery,Modular25DArt.Bush(0),new Vector2(-2.6f,-3.55f));
            AddProp(scenery,Modular25DArt.Bush(1),new Vector2(3.25f,-4.15f));
            AddProp(scenery,Modular25DArt.Rock(),new Vector2(-4.15f,.6f));
            AddProp(scenery,Modular25DArt.Rock(),new Vector2(4.55f,-.45f));
            AddProp(scenery,Modular25DArt.Rock(),new Vector2(-.15f,-4.65f));

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

            Gameplay=gameObject.AddComponent<Modular25DGameplay>();
            Hud=gameObject.AddComponent<Modular25DInterface>();
            gameObject.AddComponent<Modular25DCameraInput>();
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

        void AddTree(Transform root,Vector2 foot,int variant)
        {
            AddContact("tree_contact",foot,90,FootOrder(foot.y)-1,root);
            var tree=AddSprite("Pine "+variant,Modular25DArt.Tree(variant),foot,FootOrder(foot.y),root);
            tree.gameObject.AddComponent<Modular25DTreeSway>().Seed=variant*1.13f+foot.x;
        }

        void AddProp(Transform root,Sprite sprite,Vector2 foot)
        {
            AddSprite("Separate environment prop",sprite,foot,FootOrder(foot.y),root);
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

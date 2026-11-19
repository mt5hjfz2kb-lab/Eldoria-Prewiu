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
            Transform ground = NewRoot("Ground - individual 2:1 diamond tiles");
            Transform buildings = NewRoot("Buildings - foot anchored");
            Transform scenery = NewRoot("Scenery - dynamic occlusion");
            Transform people = NewRoot("People - actual movement");
            Transform paths = NewRoot("Paths - continuous earth ribbons");

            // PRODUCTION PIVOT: separate 8x8 m contiguous terrain chunks.
            // A bounded 21x21 diamond carpet visibly ended in the 390x844
            // camera; the continuous repeatable material fixes that root cause.
            // Routes and contact props stay separate and can be reordered.
            var terrain=ground.gameObject.AddComponent<Modular25DGround>();
            terrain.Build();

            BuildPaths(paths);
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

            AddNaturalDetail(scenery);
            if(withRuntime)
            {
                Gameplay=gameObject.AddComponent<Modular25DGameplay>();
                Hud=gameObject.AddComponent<Modular25DInterface>();
                gameObject.AddComponent<Modular25DCameraInput>();
            }
        }

        void BuildPaths(Transform parent)
        {
            // Paths are real scene elements between tiles and inhabitants, not
            // discontinuous brown islands painted into individual grass diamonds.
            // Their points follow the walkable isometric world plane exactly.
            var main=new[]{
                new Vector2(-4.85f,-5.3f),new Vector2(-3.88f,-3.76f),
                new Vector2(-2.70f,-2.5f),new Vector2(-2.63f,-1.56f),
                new Vector2(-2.62f,-.55f),new Vector2(-2.3f,.1f),
                new Vector2(-1.15f,.85f)
            };
            var lumber=new[]{
                new Vector2(-2.70f,-2.5f),new Vector2(-1.57f,-3.16f),
                new Vector2(-.21f,-3.50f),new Vector2(1.04f,-3.41f),
                new Vector2(1.95f,-2.55f)
            };
            for(int path=0;path<2;path++)
            {
                var pts=path==0?main:lumber;
                AddRoadStroke(parent,path==0?"Old Imperial Way":"Timber haulers' path",
                    pts,1.28f,new Color(.28f,.30f,.25f,.93f),-2899);
                AddRoadStroke(parent,path==0?"Packed earth centre":"Wheel-worn dirt",
                    pts,.91f,new Color(.53f,.43f,.30f,.88f),-2898);
                AddRoadStroke(parent,path==0?"Irregular central foot track":"Drag mark",
                    pts,.43f,new Color(.61f,.52f,.39f,.53f),-2897);
            }
        }

        static void AddRoadStroke(Transform parent,string label,Vector2[] points,
            float width,Color color,int order)
        {
            var go=new GameObject("Ground route | "+label);
            go.transform.SetParent(parent,false);
            var line=go.AddComponent<LineRenderer>();
            line.positionCount=points.Length;
            for(int i=0;i<points.Length;i++)
                line.SetPosition(i,new Vector3(points[i].x,points[i].y,0));
            line.useWorldSpace=true;
            line.startWidth=width;line.endWidth=width*.94f;
            line.numCornerVertices=4;line.numCapVertices=5;
            line.alignment=LineAlignment.View;
            line.startColor=color;line.endColor=color;
            line.sortingOrder=order;
            var shader=Shader.Find("Sprites/Default");
            if(shader==null)shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)
            {
                Debug.LogWarning("[Modular25D] No compatible line shader; path hidden until URP material resolved.");
                line.enabled=false;
            }
            else
            {
                var material=new Material(shader){name="Eldoria modular route | "+label};
                line.sharedMaterial=material;
            }
        }

        void AddNaturalDetail(Transform root)
        {
            // Constrained small scatter; central building hitboxes and the
            // 0.9-unit main paths remain unobstructed. Never create plinths.
            var random=new System.Random(11813);
            int placed=0;
            for(int attempt=0;attempt<300&&placed<46;attempt++)
            {
                float x=-7f+(float)random.NextDouble()*14f;
                float y=-6.2f+(float)random.NextDouble()*12.5f;
                var point=new Vector2(x,y);
                if(Vector2.Distance(point,new Vector2(-1.15f,1.3f))<2.5f ||
                   Vector2.Distance(point,new Vector2(1.95f,-2.2f))<2.15f ||
                   Vector2.Distance(point,new Vector2(-3.15f,-1.4f))<.64f)
                    continue;
                if(DistanceToRoad(point)<.68f)continue;
                var prop=random.Next(7)==0 ? Modular25DArt.Rock() :
                    Modular25DArt.Bush(random.Next(3));
                var draw=AddSprite("Interchangeable grass / stone "+placed,
                    prop,point,FootOrder(point.y),root);
                float scale=.30f+(float)random.NextDouble()*.39f;
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

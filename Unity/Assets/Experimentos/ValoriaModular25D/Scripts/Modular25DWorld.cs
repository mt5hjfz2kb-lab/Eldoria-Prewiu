using System;
using Eldoria.Domain;
using UnityEngine;
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
                new Vector2(-3.9f,-3.6f),new Vector2(-3.4f,-2.5f),
                new Vector2(-3.08f,-.55f),new Vector2(-2.3f,.1f),
                new Vector2(-3.08f,-.55f),new Vector2(-3.4f,-2.5f)
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
            // The in-world parcel is not a separate economy or state machine.
            // Provisional state silhouettes will be replaced by authored build-phase sprites.
            SawmillRenderer.enabled=true;
            SawmillRenderer.color=stage==ParcelBuildingState.BUILT
                ? Color.white : stage==ParcelBuildingState.UNDER_CONSTRUCTION
                    ? new Color(1f,.85f,.64f,.7f) : new Color(.65f,.75f,.66f,.24f);
        }

        public bool HitSawmill(Vector2 world)
        {
            // World-space footprint, independent of sprite alpha and canvas scaling.
            return world.x>.10f && world.x<3.87f && world.y>-2.48f && world.y<.92f;
        }
    }

    internal sealed class Modular25DWalker : MonoBehaviour
    {
        Vector2[] route;
        SpriteRenderer body,shadow;
        int step;
        float clock;
        float delay;
        const float Speed = .75f;

        public void Configure(Vector2[] points,float initialDelay,SpriteRenderer sprite,SpriteRenderer contact)
        {
            route=points;delay=initialDelay;body=sprite;shadow=contact;
        }

        void Update()
        {
            if(route==null||route.Length<2)return;
            if(delay>0){delay-=Time.deltaTime;return;}
            Vector2 goal=route[(step+1)%route.Length];
            Vector2 previous=transform.localPosition;
            Vector2 next=Vector2.MoveTowards(previous,goal,Speed*Time.deltaTime);
            transform.localPosition=new Vector3(next.x,next.y,0);
            if((next-goal).sqrMagnitude<.005f)step=(step+1)%route.Length;
            int sort=Modular25DWorld.FootOrder(next.y);
            body.sortingOrder=sort;
            shadow.sortingOrder=sort-1;
            clock+=Time.deltaTime;
            body.sprite=Modular25DArt.Villager(Mathf.FloorToInt(clock*5f)%2);
            if(Mathf.Abs(next.x-previous.x)>.0001f)body.flipX=next.x<previous.x;
            // One coherent foot anchor; no screen-space puppet movement.
            body.transform.localPosition=new Vector3(0,Mathf.Abs(Mathf.Sin(clock*12f))*.025f,0);
        }
    }

    internal sealed class Modular25DTreeSway : MonoBehaviour
    {
        public float Seed;
        void Update(){transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*.54f+Seed)*.65f);}
    }

    internal sealed class Modular25DCameraInput : MonoBehaviour
    {
        Camera camera;
        Modular25DWorld world;
        Vector2 lastPointer;
        bool moving;
        float pinchDistance;
        float startOrthoSize;
        void Start(){world=GetComponent<Modular25DWorld>();camera=world.ViewCamera;startOrthoSize=camera.orthographicSize;}
        void Update()
        {
            if(camera==null)return;
#if ENABLE_INPUT_SYSTEM
            var touches=Touchscreen.current;
            if(touches!=null)
            {
                var first=touches.primaryTouch;
                if(first.press.isPressed)
                {
                    Vector2 pos=first.position.ReadValue();
                    if(!moving){moving=true;lastPointer=pos;}
                    Vector2 delta=pos-lastPointer;
                    // Pan in camera-local world pixels; no camera rotation.
                    Pan(delta);lastPointer=pos;
                }
                else if(moving)
                {
                    moving=false;
                    if((lastPointer).sqrMagnitude>0)TrySelect(lastPointer);
                }
                var second=touches.touches.Count>1?touches.touches[1]:null;
                if(second!=null && first.press.isPressed && second.press.isPressed)
                {
                    float d=Vector2.Distance(first.position.ReadValue(),second.position.ReadValue());
                    if(pinchDistance>2f)Zoom(pinchDistance/d);
                    pinchDistance=d;
                }
                else pinchDistance=0;
                return;
            }
            var mouse=Mouse.current;
            if(mouse!=null)
            {
                Vector2 pos=mouse.position.ReadValue();
                if(mouse.leftButton.wasPressedThisFrame||mouse.middleButton.wasPressedThisFrame)
                {moving=true;lastPointer=pos;}
                if(moving && (mouse.leftButton.isPressed||mouse.middleButton.isPressed))
                {Pan(pos-lastPointer);lastPointer=pos;}
                if(moving && (mouse.leftButton.wasReleasedThisFrame||mouse.middleButton.wasReleasedThisFrame))
                {moving=false;TrySelect(pos);}
                float wheel=mouse.scroll.ReadValue().y;
                if(Mathf.Abs(wheel)>.01f)Zoom(Mathf.Exp(-wheel*.00065f));
            }
#else
            if(Input.touchCount>0)
            {
                Touch t=Input.GetTouch(0);
                if(t.phase==TouchPhase.Began){moving=true;lastPointer=t.position;}
                else if(t.phase==TouchPhase.Moved){Pan(t.position-lastPointer);lastPointer=t.position;}
                else if(t.phase==TouchPhase.Ended){moving=false;TrySelect(t.position);}
                if(Input.touchCount>1)
                {
                    float d=Vector2.Distance(Input.GetTouch(0).position,Input.GetTouch(1).position);
                    if(pinchDistance>2f)Zoom(pinchDistance/d);
                    pinchDistance=d;
                }
                else pinchDistance=0;
            }
            else
            {
                Vector2 mouse=Input.mousePosition;
                if(Input.GetMouseButtonDown(0)){moving=true;lastPointer=mouse;}
                if(moving&&Input.GetMouseButton(0)){Pan(mouse-lastPointer);lastPointer=mouse;}
                if(moving&&Input.GetMouseButtonUp(0)){moving=false;TrySelect(mouse);}
                float wheel=Input.mouseScrollDelta.y;
                if(Mathf.Abs(wheel)>.01f)Zoom(Mathf.Exp(-wheel*.1f));
            }
#endif
        }
        void Pan(Vector2 pixels)
        {
            if(pixels.sqrMagnitude<.001f)return;
            Vector3 p=camera.transform.position;
            float scale=2f*camera.orthographicSize/Mathf.Max(1f,Screen.height);
            p.x=Mathf.Clamp(p.x-pixels.x*scale,-6f,6f);
            p.y=Mathf.Clamp(p.y-pixels.y*scale,-5f,5f);
            camera.transform.position=p;
        }
        void Zoom(float ratio)
        {
            camera.orthographicSize=Mathf.Clamp(camera.orthographicSize*ratio,4.2f,11.8f);
        }
        public void Home()
        {
            if(camera==null)return;
            camera.orthographicSize=startOrthoSize;
            camera.transform.position=new Vector3(.18f,.15f,-30f);
        }
        void TrySelect(Vector2 screen)
        {
            if(world==null||world.Hud==null)return;
            // UI owns top/bottom strips; an isolated camera touch cannot click through HUD.
            if(screen.y<Screen.height*.13f||screen.y>Screen.height*.83f)return;
            Vector3 p=camera.ScreenToWorldPoint(new Vector3(screen.x,screen.y,30f));
            if(world.HitSawmill(p))world.Hud.OpenSawmill();
        }
    }
}

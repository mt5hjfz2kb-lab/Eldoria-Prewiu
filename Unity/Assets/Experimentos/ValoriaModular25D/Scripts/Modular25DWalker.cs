using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    public sealed class Modular25DWalker : MonoBehaviour
    {
        Vector2[] route;
        SpriteRenderer body,shadow;
        int step;
        float clock;
        float delay;
        float dwell;
        const float Speed = .75f;

        public void Configure(Vector2[] points,float initialDelay,SpriteRenderer sprite,SpriteRenderer contact)
        {
            route=points;delay=initialDelay;body=sprite;shadow=contact;
        }

        void Update() => Step(Time.deltaTime);

#if UNITY_EDITOR
        // Moves the actual Unity Transform/SpriteRenderer with the same Step
        // contract used in PlayMode; does not simulate or fabricate render frames.
        public void StepForNativeCapture(float deltaTime) => Step(deltaTime);
#endif

        void Step(float deltaTime)
        {
            if(route==null||route.Length<2)return;
            if(delay>0){delay-=deltaTime;return;}
            if(dwell>0)
            {
                dwell-=deltaTime;
                body.sprite=Modular25DArt.Villager(0);
                body.transform.localPosition=Vector3.zero;
                return;
            }
            Vector2 goal=route[(step+1)%route.Length];
            Vector2 previous=transform.localPosition;
            Vector2 next=Vector2.MoveTowards(previous,goal,Speed*deltaTime);
            transform.localPosition=new Vector3(next.x,next.y,0);
            if((next-goal).sqrMagnitude<.005f)
            {
                step=(step+1)%route.Length;
                dwell=.45f; // purposeful pause at junctions, gates and log rack
            }
            int sort=Modular25DWorld.FootOrder(next.y);
            body.sortingOrder=sort;
            shadow.sortingOrder=sort-1;
            clock+=deltaTime;
            body.sprite=Modular25DArt.Villager(Mathf.FloorToInt(clock*5f)%2);
            if(Mathf.Abs(next.x-previous.x)>.0001f)body.flipX=next.x<previous.x;
            // One coherent foot anchor; no screen-space puppet movement.
            body.transform.localPosition=new Vector3(0,Mathf.Abs(Mathf.Sin(clock*12f))*.025f,0);
        }
    }
}

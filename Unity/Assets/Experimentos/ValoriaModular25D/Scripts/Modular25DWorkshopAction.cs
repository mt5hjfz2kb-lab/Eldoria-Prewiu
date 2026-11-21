using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// A single sawyer works beside the existing original timber prop.
    /// Pure presentation: no production, cost, rewards, AI target or new save.
    /// The same transform and authored two-frame sprites are rendered in
    /// real Unity Editor capture and actual PlayMode/WebGL.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Modular25DWorkshopAction : MonoBehaviour
    {
        SpriteRenderer body,contact;
        Sprite[] keyframes;
        float clock;
        public bool Configured { get; private set; }
        public float WorkCycles { get; private set; }

        public void Configure(SpriteRenderer worker,SpriteRenderer ground)
        {
            body=worker;contact=ground;
            keyframes=new[]{Modular25DArt.Carpenter(0),Modular25DArt.Carpenter(1),
                Modular25DArt.Carpenter(2),Modular25DArt.Carpenter(3)};
            Configured=body!=null&&contact!=null&&
                keyframes[0]!=null&&keyframes[1]!=null;
            if(Configured)ApplyPose();
        }

        void Update() => Step(Time.deltaTime);
#if UNITY_EDITOR
        public void StepForNativeCapture(float seconds) => Step(seconds);
#endif
        void Step(float delta)
        {
            if(!Configured)return;
            clock+=Mathf.Max(0f,delta);
            WorkCycles=clock*1.07f;
            ApplyPose();
        }

        void ApplyPose()
        {
            float stroke=Mathf.Sin(clock*2f*Mathf.PI*1.07f);
            body.sprite=keyframes[Mathf.FloorToInt(WorkCycles*4f)%4];
            body.flipX=false;
            // Repeated two-phase push and recovery, visually grounded at one
            // sawing station; no pedestrian walking in place or world-space warp.
            body.transform.localPosition=new Vector3(stroke*.018f,
                Mathf.Max(0f,stroke)*.008f,0f);
            // Two already-different saw positions carry the readable action.
            // No body bob/tilt strong enough to make the trestle float.
            body.transform.localRotation=Quaternion.identity;
            int order=Modular25DWorld.FootOrder(transform.localPosition.y);
            body.sortingOrder=order;
            contact.sortingOrder=order-1;
        }
    }
}


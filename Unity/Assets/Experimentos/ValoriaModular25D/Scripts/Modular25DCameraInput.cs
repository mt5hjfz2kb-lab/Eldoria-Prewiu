using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Eldoria.Experimentos.ValoriaModular25D
{
    public sealed class Modular25DCameraInput : MonoBehaviour
    {
        Camera camera;
        Modular25DWorld world;
        Vector2 pressAt, previous;
        bool held, dragged, beganOnUI, pinched;
        float previousPinch, homeSize;

        // Same geometry in the real player and the native Editor A/B.
        // Reduce unused meadow while framing Keep, courtyard and working mill.
        public static Vector3 HomePosition => new Vector3(.05f,-.40f,-30f);
        public static float HomeSizeForAspect(float aspect)
        {
            return Mathf.Clamp(3.55f/Mathf.Max(.42f,aspect),5.05f,7.95f);
        }

        void Start()
        {
            world=GetComponent<Modular25DWorld>();
            camera=world.ViewCamera;
            homeSize=HomeSizeForAspect(camera.aspect);
            Home();
        }

        bool IsUI(Vector2 screen)
        {
            var events=EventSystem.current;
            if(events==null)return false;
            var data=new PointerEventData(events){position=screen};
            var hits=new List<RaycastResult>();
            events.RaycastAll(data,hits);
            return hits.Count!=0;
        }

        void Begin(Vector2 at)
        {
            held=true;dragged=false;pinched=false;
            pressAt=at;previous=at;beganOnUI=IsUI(at);
        }

        void Move(Vector2 at)
        {
            if(!held)return;
            if((at-pressAt).sqrMagnitude>100f)dragged=true;
            if(dragged&&!beganOnUI)Pan(at-previous);
            previous=at;
        }

        void End(Vector2 at)
        {
            if(!held)return;
            bool tap=!dragged&&!pinched&&!beganOnUI;
            held=false;dragged=false;pinched=false;beganOnUI=false;
            previousPinch=0f;
            if(tap)TrySelect(at);
        }

        void Update()
        {
            if(camera==null)return;
#if ENABLE_INPUT_SYSTEM
            var touchscreen=Touchscreen.current;
            if(touchscreen!=null)
            {
                var one=touchscreen.primaryTouch;
                var two=touchscreen.touches.Count>1?touchscreen.touches[1]:null;
                bool first=one.press.isPressed;
                bool second=two!=null && two.press.isPressed;
                bool began=one.press.wasPressedThisFrame;
                bool ended=one.press.wasReleasedThisFrame;
                if(began)Begin(one.position.ReadValue());
                if(first&&second)
                {
                    pinched=true;
                    float d=Vector2.Distance(one.position.ReadValue(),two.position.ReadValue());
                    if(previousPinch>2f&&d>2f&&!beganOnUI)
                        Zoom(previousPinch/d);
                    previousPinch=d;
                    previous=one.position.ReadValue();
                    return;
                }
                previousPinch=0f;
                if(first)Move(one.position.ReadValue());
                if(ended)End(one.position.ReadValue());
                // A Touchscreen device may exist on desktop with zero touches:
                // do not swallow real mouse events just because it is connected.
                if(first||began||ended||held)return;
            }
            var mouse=Mouse.current;
            if(mouse!=null)
            {
                Vector2 pos=mouse.position.ReadValue();
                if(mouse.leftButton.wasPressedThisFrame||mouse.middleButton.wasPressedThisFrame)
                    Begin(pos);
                if(held&&(mouse.leftButton.isPressed||mouse.middleButton.isPressed))
                    Move(pos);
                if(held&&(mouse.leftButton.wasReleasedThisFrame||mouse.middleButton.wasReleasedThisFrame))
                    End(pos);
                float wheel=mouse.scroll.ReadValue().y;
                if(Mathf.Abs(wheel)>.01f&&!IsUI(pos))
                    Zoom(Mathf.Exp(-Mathf.Clamp(wheel/120f,-3f,3f)*.12f));
            }
#else
            if(Input.touchCount>0)
            {
                var touch=Input.GetTouch(0);
                if(touch.phase==TouchPhase.Began)Begin(touch.position);
                if(Input.touchCount>=2)
                {
                    pinched=true;
                    float d=Vector2.Distance(touch.position,Input.GetTouch(1).position);
                    if(previousPinch>2f&&d>2f&&!beganOnUI)Zoom(previousPinch/d);
                    previousPinch=d;
                    previous=touch.position;
                    return;
                }
                previousPinch=0f;
                if(touch.phase==TouchPhase.Moved)Move(touch.position);
                if(touch.phase==TouchPhase.Ended||touch.phase==TouchPhase.Canceled)End(touch.position);
                return;
            }
            Vector2 position=Input.mousePosition;
            if(Input.GetMouseButtonDown(0)||Input.GetMouseButtonDown(2))Begin(position);
            if(held&&(Input.GetMouseButton(0)||Input.GetMouseButton(2)))Move(position);
            if(held&&(Input.GetMouseButtonUp(0)||Input.GetMouseButtonUp(2)))End(position);
            float wheelLegacy=Input.mouseScrollDelta.y;
            if(Mathf.Abs(wheelLegacy)>.01f&&!IsUI(position))
                Zoom(Mathf.Exp(-wheelLegacy*.12f));
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
            if(float.IsNaN(ratio)||float.IsInfinity(ratio)||ratio<=0)return;
            camera.orthographicSize=Mathf.Clamp(camera.orthographicSize*ratio,4.2f,11.8f);
        }

        public void Home()
        {
            if(camera==null)return;
            camera.orthographicSize=homeSize;
            camera.transform.position=HomePosition;
        }

        void TrySelect(Vector2 screen)
        {
            if(world==null||world.Hud==null||IsUI(screen))return;
            Vector3 p=camera.ScreenToWorldPoint(new Vector3(screen.x,screen.y,30f));
            if(world.HitSawmill(p))world.Hud.OpenSawmill();
        }
    }
}

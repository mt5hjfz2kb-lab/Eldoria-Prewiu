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
        float previousPinch;

        // Calibrated to the owner's portrait capture. Keep the same horizontal
        // coverage across phone viewports, measured on the game canvas alone.
        public static Vector3 HomePosition => (Vector3)ValoriaUrbanPlan.City.Keep.Foot
            + new Vector3(-1.98f,-1.46f,-30f);

        public static float HomeSizeForAspect(float aspect)
        {
            return Mathf.Clamp(6.04f / Mathf.Max(.4f, aspect), 4.9f, 15.1f);
        }

        public static Vector3 ExpansionCenterForLevel(int level) => HomePosition;
        public static float ExpansionSizeForLevel(float aspect,int level) => HomeSizeForAspect(aspect);
        // Existing editor tooling can still ask for the complete-city fit.
        public static Vector3 ExpansionCenter => ExpansionCenterForLevel(5);
        public static float ExpansionSize(float aspect) => ExpansionSizeForLevel(aspect,5);

        void Start()
        {
            world=GetComponent<Modular25DWorld>();
            camera=world.ViewCamera;
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
            var area=ValoriaUrbanPlan.City.CameraTravel;
            p.x=Mathf.Clamp(p.x-pixels.x*scale,area.xMin,area.xMax);
            p.y=Mathf.Clamp(p.y-pixels.y*scale,area.yMin,area.yMax);
            camera.transform.position=p;
        }

        void Zoom(float ratio)
        {
            if(float.IsNaN(ratio)||float.IsInfinity(ratio)||ratio<=0)return;
            float farLimit=Mathf.Max(11.8f,HomeSizeForAspect(camera.aspect)*1.5f);
            camera.orthographicSize=Mathf.Clamp(camera.orthographicSize*ratio,4.2f,farLimit);
        }

        public void Home()
        {
            if(camera==null)return;
            camera.orthographicSize=HomeSizeForAspect(camera.aspect);
            camera.transform.position=HomePosition;
        }

        void TrySelect(Vector2 screen)
        {
            if(world==null||world.Hud==null||IsUI(screen))return;
            Vector3 p=camera.ScreenToWorldPoint(new Vector3(screen.x,screen.y,30f));
            if(world.HitSawmill(p)) world.Hud.OpenSawmill();
            else if (Contains(world.BastionRenderer,p)) world.Hud.OpenBastion();
            else if (Contains(world.BarracksRenderer,p)) world.Hud.OpenDistrict("barracks");
            else if (Contains(world.GranaryRenderer,p)) world.Hud.OpenDistrict("granary");
            else if (Contains(world.StoneworksRenderer,p)) world.Hud.OpenDistrict("stoneworks");
        }

        static bool Contains(SpriteRenderer renderer, Vector3 position)
        {
            if (!renderer || !renderer.gameObject.activeInHierarchy || !renderer.sprite) return false;
            position.z = renderer.bounds.center.z;
            return renderer.bounds.Contains(position);
        }
    }
}


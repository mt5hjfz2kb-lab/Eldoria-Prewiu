using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Eldoria.Art
{
    // Candidate inspection only. Shares no input, state or renderer with SHARP.
    [RequireComponent(typeof(Camera))]
    public sealed class Bastion1AssemblyCamera : MonoBehaviour
    {
        public Vector2 PanHalfExtents=new Vector2(12f,12f);
        Camera view;
        Vector3 home;
        Vector2 offset;
        float homeSize=38.5f;
        bool initialized,gestureOwnedByUi;
        void Awake(){ConfigureHome();}
        public void ConfigureHome()
        {
            view=GetComponent<Camera>();home=transform.position;
            homeSize=Mathf.Max(38.5f,36f/Mathf.Max(.1f,view.aspect));
            initialized=true;Home();
        }
        public void Home()
        {
            if(!initialized)ConfigureHome();
            offset=Vector2.zero;transform.position=home;view.orthographicSize=homeSize;
        }
        public void PanByScreenDelta(Vector2 delta)
        {
            if(!initialized)ConfigureHome();
            float perPixel=2f*view.orthographicSize/Mathf.Max(1f,view.pixelHeight);
            var right=Vector3.ProjectOnPlane(transform.right,Vector3.up).normalized;
            var up=Vector3.ProjectOnPlane(transform.up,Vector3.up).normalized;
            var shift=-(right*delta.x+up*delta.y)*perPixel;
            offset.x=Mathf.Clamp(offset.x+shift.x,-PanHalfExtents.x,PanHalfExtents.x);
            offset.y=Mathf.Clamp(offset.y+shift.z,-PanHalfExtents.y,PanHalfExtents.y);
            transform.position=home+new Vector3(offset.x,0f,offset.y);
        }
        public void ZoomBy(float delta)
        {
            if(!initialized)ConfigureHome();
            view.orthographicSize=Mathf.Clamp(view.orthographicSize+delta,homeSize*.55f,homeSize*1.15f);
        }
        bool OverUi(Vector2 position)
        {
            if(EventSystem.current==null)return false;
            var hits=new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},hits);
            return hits.Count>0;
        }
        void Update()
        {
            if(Keyboard.current!=null&&Keyboard.current.homeKey.wasPressedThisFrame)Home();
            var mouse=Mouse.current;
            if(mouse!=null)
            {
                if(mouse.leftButton.wasPressedThisFrame)gestureOwnedByUi=OverUi(mouse.position.ReadValue());
                if(mouse.leftButton.isPressed&&!gestureOwnedByUi)PanByScreenDelta(mouse.delta.ReadValue());
                if(!OverUi(mouse.position.ReadValue()))ZoomBy(-mouse.scroll.ReadValue().y*.015f);
            }
            var screen=Touchscreen.current;
            if(screen==null)return;
            UnityEngine.InputSystem.Controls.TouchControl first=null,second=null;
            foreach(var touch in screen.touches)
                if(touch.press.isPressed){if(first==null)first=touch;else{second=touch;break;}}
            if(first==null)return;
            if(first.press.wasPressedThisFrame)gestureOwnedByUi=OverUi(first.position.ReadValue());
            if(gestureOwnedByUi)return;
            if(second==null){PanByScreenDelta(first.delta.ReadValue());return;}
            float current=Vector2.Distance(first.position.ReadValue(),second.position.ReadValue());
            float previous=Vector2.Distance(first.position.ReadValue()-first.delta.ReadValue(),second.position.ReadValue()-second.delta.ReadValue());
            ZoomBy(-(current-previous)*homeSize/Mathf.Max(1f,view.pixelHeight));
        }
    }
}

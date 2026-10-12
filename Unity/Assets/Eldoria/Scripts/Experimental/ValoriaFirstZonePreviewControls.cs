using UnityEngine;
using UnityEngine.InputSystem;

namespace Eldoria
{
    /// <summary>
    /// Isolated first-zone camera explorer. It does NOT run domain commands,
    /// spend resources or replace the certified Bastion I-II gameplay.
    /// Generated only in the editable Valoria prototype scene.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class ValoriaFirstZonePreviewControls : MonoBehaviour
    {
        [SerializeField] float panUnitsPerPixel=.065f;
        [SerializeField] float zoomSensitivity=.0025f;
        [SerializeField] float minimumOrthographicSize=6f;
        [SerializeField] float maximumOrthographicSize=45f;
        [SerializeField] Vector2 xLimits=new Vector2(-38f,38f);
        [SerializeField] Vector2 zLimits=new Vector2(-44f,28f);

        Camera view;
        Vector3 homePosition;
        string selectedInstanceId=string.Empty;
        Vector2 pressPosition;
        bool dragging;
        float lastTelemetryTime=-10f;
        public string SelectedInstanceId => selectedInstanceId;

        void Awake()
        {
            view=GetComponent<Camera>();
            homePosition=transform.position;
            if(view.orthographic) view.orthographicSize=Mathf.Clamp(view.orthographicSize,minimumOrthographicSize,maximumOrthographicSize);
            view.backgroundColor=new Color(.48f,.58f,.63f);
            RenderSettings.ambientIntensity=Mathf.Min(RenderSettings.ambientIntensity,.75f);
            foreach(var light in FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(light.type==LightType.Directional) light.intensity=Mathf.Min(light.intensity,.85f);
        }
        void Update()
        {
            if(view==null)return;
            var mouse=Mouse.current;
            if(mouse!=null)
            {
                if(mouse.middleButton.isPressed||mouse.rightButton.isPressed)
                    Pan(mouse.delta.ReadValue());
                float wheel=mouse.scroll.ReadValue().y;
                if(Mathf.Abs(wheel)>.001f)Zoom(-wheel*zoomSensitivity);
                if(mouse.leftButton.wasPressedThisFrame)
                {
                    pressPosition=mouse.position.ReadValue();
                    dragging=false;
                }
                if(mouse.leftButton.isPressed &&
                   (mouse.position.ReadValue()-pressPosition).sqrMagnitude>64f)
                    dragging=true;
                if(mouse.leftButton.isPressed && dragging)
                    Pan(mouse.delta.ReadValue());
                if(mouse.leftButton.wasReleasedThisFrame&&!dragging)
                    Select(mouse.position.ReadValue());
            }
            var touch=Touchscreen.current;
            if(touch==null)return;
            var first=touch.primaryTouch;
            var second=touch.touches.Count>1?touch.touches[1]:null;
            if(first.press.isPressed)
            {
                if(second!=null&&second.press.isPressed)
                {
                    float distance=Vector2.Distance(first.position.ReadValue(),
                        second.position.ReadValue());
                    float previousDistance=Vector2.Distance(
                        first.position.ReadValue()-first.delta.ReadValue(),
                        second.position.ReadValue()-second.delta.ReadValue());
                    Zoom(-(distance-previousDistance)*.09f);
                }
                else Pan(first.delta.ReadValue());
            }
            if(first.press.wasReleasedThisFrame &&
               first.delta.ReadValue().sqrMagnitude<64f &&
               (second==null||!second.press.isPressed))
                Select(first.position.ReadValue());
        }
        void Pan(Vector2 screenDelta)
        {
            if(screenDelta.sqrMagnitude<.001f)return;
            Vector3 right=transform.right;right.y=0f;right.Normalize();
            Vector3 forward=transform.up;forward.y=0f;forward.Normalize();
            Vector3 p=transform.position-
                (right*screenDelta.x+forward*screenDelta.y)*panUnitsPerPixel;
            p.x=Mathf.Clamp(p.x,homePosition.x+xLimits.x,homePosition.x+xLimits.y);
            p.z=Mathf.Clamp(p.z,homePosition.z+zLimits.x,homePosition.z+zLimits.y);
            transform.position=p;
            if(Time.unscaledTime-lastTelemetryTime>.45f)
            {
                lastTelemetryTime=Time.unscaledTime;
                Debug.Log("VALORIA_CAMERA_PAN_VERIFIED x="+p.x.ToString("F2")+" z="+p.z.ToString("F2"));
            }
        }
        void Zoom(float increment)
        {
            if(view.orthographic)
            {
                float previous=view.orthographicSize;
                view.orthographicSize=Mathf.Clamp(previous+increment,
                    minimumOrthographicSize,maximumOrthographicSize);
                if(Mathf.Abs(previous-view.orthographicSize)>.005f)
                    Debug.Log("VALORIA_CAMERA_ZOOM_VERIFIED size="+view.orthographicSize.ToString("F2"));
            }
            else
            {
                float previous=view.fieldOfView;
                view.fieldOfView=Mathf.Clamp(previous+increment*8f,48f,78f);
                if(Mathf.Abs(previous-view.fieldOfView)>.005f)
                    Debug.Log("VALORIA_CAMERA_ZOOM_VERIFIED fov="+view.fieldOfView.ToString("F2"));
            }
        }
        void Select(Vector2 screenPosition)
        {
            var ray=view.ScreenPointToRay(screenPosition);
            if(!Physics.Raycast(ray,out var hit,600f,~0,QueryTriggerInteraction.Collide))
                return;
            var anchor=hit.collider.GetComponentInParent<ValoriaFirstZoneAnchor>();
            if(anchor==null)return;
            selectedInstanceId=anchor.InstanceId;
            Debug.Log("VALORIA_FIRST_ZONE_PREVIEW_SELECTED "+selectedInstanceId+
                " asset="+anchor.SourceAssetId+" domain_wired="+anchor.GameplayWired);
        }
        void OnGUI()
        {
            if(view==null||!view.enabled)return;
            GUI.Label(new Rect(15,15,690,28),
                "VALORIA · drag to move · pinch / wheel to zoom · 3D prototype");
            GUI.Label(new Rect(15,70,690,28),"Camera zoom: "+view.orthographicSize.ToString("F1"));
            if(!string.IsNullOrEmpty(selectedInstanceId))
                GUI.Label(new Rect(15,42,690,28),"Selected: "+selectedInstanceId);
        }
    }
}

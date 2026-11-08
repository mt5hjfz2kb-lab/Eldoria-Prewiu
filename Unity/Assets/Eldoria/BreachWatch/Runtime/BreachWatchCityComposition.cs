using UnityEngine;
using Eldoria.ReferenceClean;

namespace Eldoria.BreachWatch
{
    /// <summary>
    /// Optional city camera framing owned by the NEW Vigilia preview only.
    /// The approved 864x1536 screenshot shows the whole bridge and the Bastion
    /// at approximately a third of phone width; old native 390x844 evidence
    /// zoomed so far in that the Bastion filled ~70% width and the bridge
    /// disappeared. That is a world-framing problem, not a button-skin problem.
    ///
    /// This adapter changes only the active city camera when portrait starts
    /// or orientation changes. It never changes meshes, terrain, saves or the
    /// canonical homeZoom; it never runs against the regional/world camera.
    /// A user's subsequent pan/pinch remains under canonical controls.
    /// </summary>
    public sealed class BreachWatchCityComposition : MonoBehaviour
    {
        ValoriaIntegratedWorld host;
        Camera cityCamera;
        float originalZoom;
        Vector3 originalPosition;
        bool captured,portraitFramed;
        float lastAspect;

        public bool PortraitFramed => portraitFramed;
        public float OrthographicSize => cityCamera ? cityCamera.orthographicSize : 0f;

        public void Initialize(ValoriaIntegratedWorld source)
        {
            host=source;
            cityCamera=source && source.City ? source.City.PlayerCamera:null;
            if(cityCamera)
            {
                originalZoom=cityCamera.orthographicSize;
                originalPosition=cityCamera.transform.position;
                captured=cityCamera.orthographic;
            }
        }

        static bool TryBounds(GameObject gameObject,out Bounds bounds)
        {
            bounds=default;
            if(!gameObject)return false;
            bool found=false;
            foreach(var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if(!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy ||
                   renderer.bounds.size.sqrMagnitude<.00001f)continue;
                if(!found){bounds=renderer.bounds;found=true;}
                else bounds.Encapsulate(renderer.bounds);
            }
            return found;
        }

        public void OnCityReentered(int pixelWidth,int pixelHeight)
        {
            if(!captured || !host || !host.City || host.InWorld || !cityCamera)return;
            // Canonical World -> City transition calls Controls.ResetView().
            // Our former portrait flag still says "already framed"; reset
            // that only at this actual mode transition, never on an ordinary
            // user pan or pinch. We then reapply the approved view exactly
            // once, preserving normal gesture ownership thereafter.
            portraitFramed=false;
            originalZoom=cityCamera.orthographicSize;
            originalPosition=cityCamera.transform.position;
            Layout(pixelWidth,pixelHeight);
        }

        public void Layout(int pixelWidth,int pixelHeight)
        {
            if(!captured || !cityCamera || !host || !host.City || host.InWorld)return;
            bool portrait=pixelHeight>pixelWidth;
            if(!portrait)
            {
                if(portraitFramed)
                {
                    // Returning to landscape restores the source camera's
                    // original home framing without changing its zoom preset.
                    cityCamera.orthographicSize=originalZoom;
                    cityCamera.transform.position=originalPosition;
                    host.City.Controls.FitToTerrain();
                    portraitFramed=false;
                }
                return;
            }
            float aspect=Mathf.Max(.30f,pixelWidth/(float)pixelHeight);
            if(portraitFramed && Mathf.Abs(aspect-lastAspect)<.012f)return;

            if(!portraitFramed)
            {
                originalZoom=cityCamera.orthographicSize;
                originalPosition=cityCamera.transform.position;
            }
            // At the 864:1536 approved reference, an 80–90-unit zoom
            // is approximately 1.8 times the former 48-unit first chapter
            // camera. Narrower phones need a little more separation so the
            // Bastion does not regain its giant 70%-screen footprint.
            float ratio=.5625f/aspect;
            float factor=Mathf.Clamp(1.78f*ratio,1.67f,2.15f);
            cityCamera.orthographicSize=Mathf.Clamp(originalZoom*factor,45f,105f);
            host.City.Controls.FitToTerrain();

            var state=host.City.Gateway!=null?host.City.Gateway.Snapshot():null;
            var bastion=state!=null && state.BastionLevel>=2 && host.City.BastionTwo
                && host.City.BastionTwo.activeInHierarchy
                    ?host.City.BastionTwo:host.City.BastionOne;
            if(TryBounds(bastion,out var bounds))
            {
                // Position the EXISTING model at the upper central portion
                // of the phone and expose road, gate, river and resource sites.
                // Camera offset is strictly in the ground plane: avoid
                // changing height/tilt or producing perspective errors.
                Vector3 screen=cityCamera.WorldToViewportPoint(bounds.center);
                if(screen.z>0f)
                {
                    float height=2f*cityCamera.orthographicSize;
                    float width=height*aspect;
                    var offset=cityCamera.transform.right*(screen.x-.575f)*width+
                        cityCamera.transform.up*(screen.y-.72f)*height;
                    var forward=cityCamera.transform.forward;
                    if(Mathf.Abs(forward.y)>.04f)
                        offset-=forward*(offset.y/forward.y);
                    else offset.y=0f;
                    // Prevent a bad model bound from generating a huge warp.
                    if(offset.sqrMagnitude<25000f)
                    {
                        cityCamera.transform.position+=offset;
                        host.City.Controls.FitToTerrain();
                    }
                }
            }
            portraitFramed=true;
            lastAspect=aspect;
        }

        void OnDestroy()
        {
            if(!captured || !portraitFramed || !cityCamera || !host || !host.City)return;
            cityCamera.orthographicSize=originalZoom;
            cityCamera.transform.position=originalPosition;
            host.City.Controls.FitToTerrain();
        }
    }
}

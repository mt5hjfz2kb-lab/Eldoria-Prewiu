using System;
using Eldoria.Domain;
using UnityEngine;
using Gsplat;

namespace Eldoria.Presentation
{
    // Scene presentation only. LocalGateway + FileStateStore own all progression.
    // The generated scene is reproducible from the locked PLY by the canonical capture gate.
    public sealed class ValoriaParcelPresentation : MonoBehaviour
    {
        [Serializable] public sealed class Binding
        {
            public string ParcelId, BuildingId;
            public int VariantBit;
            public GameObject Built, Ground, Construction, Available;
            public Collider Target;
        }
        public Binding[] Bindings = Array.Empty<Binding>();
        // Compatibility handles for the existing two-building proof; binding iteration owns rendering.
        public GameObject LeftBuilt, RightBuilt, LeftGround, RightGround;
        public GameObject LeftConstruction, RightConstruction, LeftAvailable, RightAvailable;
        public Collider LeftTarget, RightTarget;
        public Camera ProductionCamera;
        // Authorable world-space route. The serialized SHARP route stays available
        // until the replacement passes the player-scene and device gates.
        public bool MeshMode;
        public Vector3 MeshHomePosition;
        public Quaternion MeshHomeRotation=Quaternion.identity;
        public float MeshHomeOrthographicSize=38.5f;
        public Vector2 MeshPanHalfExtents=new Vector2(12f,12f);
        public GsplatRenderer SceneSplats;
        public GsplatAsset[] StateAssets; // 0 empty, 1 left built, 2 right built, 3 both built
        public int ActiveVariant { get; private set; }
        public GameObject[] BastionLevelTwoVisuals = Array.Empty<GameObject>();
        public int PresentedBastionLevel { get; private set; } = 1;
        public float HomeFov = 44.42281f;
        public const float PresentationHomeScale=1.00f;
        public float PresentationHomeFov => HomeFov*PresentationHomeScale;
        public ParcelBuildingState LeftState { get; private set; }
        public ParcelBuildingState RightState { get; private set; }

        public void Apply(PlayerState state)
        {
            LeftState=ParcelBuildingStates.For(state,"sawmill");
            RightState=ParcelBuildingStates.For(state,"barracks");
            ActiveVariant=0;
            PresentedBastionLevel=Math.Max(1,state.BastionLevel);
            bool bastionTwo=PresentedBastionLevel>=2;
            if(BastionLevelTwoVisuals!=null)
                foreach(var cue in BastionLevelTwoVisuals) if(cue!=null) cue.SetActive(bastionTwo);
            foreach(var parcel in Bindings)
            {
                var phase=ParcelBuildingStates.For(state,parcel.BuildingId);
                if(phase==ParcelBuildingState.BUILT)ActiveVariant|=parcel.VariantBit;
                ApplyParcel(phase,parcel.Built,parcel.Ground,parcel.Construction,parcel.Available,parcel.Target);
                if(MeshMode&&phase==ParcelBuildingState.UNDER_CONSTRUCTION&&parcel.Ground!=null)
                    parcel.Ground.SetActive(false);
            }
            if(MeshMode)return;
            #if UNITY_WEBGL && !UNITY_EDITOR
            var frameLoader=GetComponent<ValoriaWebGLSplatStateLoader>();
            if(frameLoader!=null)frameLoader.RequestVariant(ActiveVariant);
#else
            if(SceneSplats!=null)
            {
                if(StateAssets!=null && ActiveVariant<StateAssets.Length && StateAssets[ActiveVariant]!=null)
                {
                    if(SceneSplats.GsplatAsset!=StateAssets[ActiveVariant])SceneSplats.GsplatAsset=StateAssets[ActiveVariant];
                }
                else
                {
                    var webLoader=GetComponent<ValoriaWebGLSplatStateLoader>();
                    if(webLoader==null)throw new InvalidOperationException("Missing SHARP parcel variant "+ActiveVariant);
                    webLoader.RequestVariant(ActiveVariant);
                }
            }
#endif
        }
        static void ApplyParcel(ParcelBuildingState state,GameObject built,GameObject ground,
            GameObject construction,GameObject available,Collider target)
        {
            bool isBuilt=state==ParcelBuildingState.BUILT;
            if(built!=null)built.SetActive(isBuilt);
            if(ground!=null)ground.SetActive(!isBuilt);
            if(construction!=null)construction.SetActive(state==ParcelBuildingState.UNDER_CONSTRUCTION);
            if(available!=null)available.SetActive(state==ParcelBuildingState.AVAILABLE);
            // Locked parcels remain visible empty space, with no premature building panel.
            if(target!=null)target.enabled=state!=ParcelBuildingState.NOT_BUILT;
        }
        public const float HorizontalPanHalfExtent=2.80f;
        public const float VerticalPanHalfExtent=1.10f;
        public void Pan(Vector2 delta)
        {
            if(MeshMode)
            {
                float perPixel=2f*ProductionCamera.orthographicSize/Mathf.Max(1f,ProductionCamera.pixelHeight);
                var right=Vector3.ProjectOnPlane(ProductionCamera.transform.right,Vector3.up).normalized;
                var forward=Vector3.ProjectOnPlane(ProductionCamera.transform.up,Vector3.up).normalized;
                var change=-(right*delta.x+forward*delta.y)*perPixel;
                var meshPosition=ProductionCamera.transform.position+change;
                meshPosition.x=Mathf.Clamp(meshPosition.x,MeshHomePosition.x-MeshPanHalfExtents.x,MeshHomePosition.x+MeshPanHalfExtents.x);
                meshPosition.z=Mathf.Clamp(meshPosition.z,MeshHomePosition.z-MeshPanHalfExtents.y,MeshHomePosition.z+MeshPanHalfExtents.y);
                meshPosition.y=MeshHomePosition.y;
                ProductionCamera.transform.position=meshPosition;
                return;
            }
            var p=ProductionCamera.transform.position;
            float horizontalLimit=HorizontalPanHalfExtent;
#if UNITY_WEBGL && !UNITY_EDITOR
            var loader=GetComponent<ValoriaWebGLSplatStateLoader>();
            horizontalLimit=loader!=null?loader.WebHorizontalPanLimit:.80f;
#endif
            p.x=Mathf.Clamp(p.x-delta.x*.012f,-horizontalLimit,horizontalLimit);
            // Owner camera rule: Valoria is primarily a wide left/right space, but must
            // retain a smaller genuine vertical browse range on touch.
            p.y=Mathf.Clamp(p.y-delta.y*.006f,-VerticalPanHalfExtent,VerticalPanHalfExtent);
            p.z=0;
            ProductionCamera.transform.position=p;
        }
        public void Zoom(float delta)
        {
            if(MeshMode)
            {
                float home=Mathf.Max(MeshHomeOrthographicSize,36f/Mathf.Max(.1f,ProductionCamera.aspect));
                ProductionCamera.orthographicSize=Mathf.Clamp(ProductionCamera.orthographicSize+delta,home*.55f,home*1.15f);
                return;
            }
            float home=PresentationHomeFov;
            ProductionCamera.fieldOfView=Mathf.Clamp(ProductionCamera.fieldOfView+delta,home*.92f,home*1.08f);
        }
        public void Home()
        {
            if(MeshMode)
            {
                ProductionCamera.transform.SetPositionAndRotation(MeshHomePosition,MeshHomeRotation);
                ProductionCamera.orthographicSize=Mathf.Max(MeshHomeOrthographicSize,36f/Mathf.Max(.1f,ProductionCamera.aspect));
                return;
            }
            ProductionCamera.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            ProductionCamera.fieldOfView=PresentationHomeFov;
        }
        public void Focus(Vector3 point)
        {
            if(!MeshMode)return;
            var ray=ProductionCamera.ViewportPointToRay(new Vector3(.5f,.5f,0));
            var plane=new Plane(Vector3.up,new Vector3(0,point.y,0));
            if(!plane.Raycast(ray,out var distance))return;
            var desired=ProductionCamera.transform.position+(point-ray.GetPoint(distance));
            desired.x=Mathf.Clamp(desired.x,MeshHomePosition.x-MeshPanHalfExtents.x,MeshHomePosition.x+MeshPanHalfExtents.x);
            desired.z=Mathf.Clamp(desired.z,MeshHomePosition.z-MeshPanHalfExtents.y,MeshHomePosition.z+MeshPanHalfExtents.y);
            desired.y=MeshHomePosition.y;
            ProductionCamera.transform.position=desired;
        }
    }
}



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
        public GsplatRenderer SceneSplats;
        public GsplatAsset[] StateAssets; // 0 empty, 1 left built, 2 right built, 3 both built
        public int ActiveVariant { get; private set; }
        public GameObject[] BastionLevelTwoVisuals = Array.Empty<GameObject>();
        public int PresentedBastionLevel { get; private set; } = 1;
        public float HomeFov = 44.42281f;
        public const float PresentationHomeScale=.80f;
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
            }
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
        public const float HorizontalPanHalfExtent=.80f;
        public const float VerticalPanHalfExtent=.32f;
        public void Pan(Vector2 delta)
        {
            var p=ProductionCamera.transform.position;
            p.x=Mathf.Clamp(p.x-delta.x*.005f,-HorizontalPanHalfExtent,HorizontalPanHalfExtent);
            // Owner camera rule: Valoria is primarily a wide left/right space, but must
            // retain a smaller genuine vertical browse range on touch.
            p.y=Mathf.Clamp(p.y-delta.y*.002f,-VerticalPanHalfExtent,VerticalPanHalfExtent);
            p.z=0;
            ProductionCamera.transform.position=p;
        }
        public void Zoom(float delta)
        {
            float home=PresentationHomeFov;
            ProductionCamera.fieldOfView=Mathf.Clamp(ProductionCamera.fieldOfView+delta,home*.92f,home*1.08f);
        }
        public void Home()
        {
            ProductionCamera.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            ProductionCamera.fieldOfView=PresentationHomeFov;
        }
    }
}


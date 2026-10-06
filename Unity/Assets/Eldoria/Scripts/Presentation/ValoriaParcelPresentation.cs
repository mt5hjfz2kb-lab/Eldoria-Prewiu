using System;
using Eldoria.Domain;
using UnityEngine;

namespace Eldoria.Presentation
{
    // Scene presentation only. LocalGateway + FileStateStore own all progression.
    // The generated scene is reproducible from the locked PLY by the canonical capture gate.
    public sealed class ValoriaParcelPresentation : MonoBehaviour
    {
        public GameObject LeftBuilt, RightBuilt, LeftGround, RightGround;
        public GameObject LeftConstruction, RightConstruction, LeftAvailable, RightAvailable;
        public Collider LeftTarget, RightTarget;
        public Camera ProductionCamera;
        public float HomeFov = 44.42281f;
        public ParcelBuildingState LeftState { get; private set; }
        public ParcelBuildingState RightState { get; private set; }

        public void Apply(PlayerState state)
        {
            LeftState=ParcelBuildingStates.For(state,"sawmill");
            RightState=ParcelBuildingStates.For(state,"barracks");
            ApplyParcel(LeftState,LeftBuilt,LeftGround,LeftConstruction,LeftAvailable,LeftTarget);
            ApplyParcel(RightState,RightBuilt,RightGround,RightConstruction,RightAvailable,RightTarget);
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
        public void Pan(Vector2 delta)
        {
            var p=ProductionCamera.transform.position;
            p.x=Mathf.Clamp(p.x-delta.x*.005f,-.5f,.5f); p.y=0; p.z=0;
            ProductionCamera.transform.position=p;
        }
        public void Zoom(float delta)
        {
            ProductionCamera.fieldOfView=Mathf.Clamp(ProductionCamera.fieldOfView+delta,HomeFov*.9f,HomeFov*1.1f);
        }
        public void Home()
        {
            ProductionCamera.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            ProductionCamera.fieldOfView=HomeFov;
        }
    }
}

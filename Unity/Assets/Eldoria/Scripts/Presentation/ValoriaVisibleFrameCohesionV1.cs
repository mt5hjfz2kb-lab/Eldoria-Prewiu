using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Visual-only cohesion layer for the currently reachable Valoria camera envelope.
    // It closes ground/edge/route seams with existing authored assets and irregular ground skins.
    // Gameplay floors, routes, parcels, hotspots, colliders and camera policy remain authoritative underneath.
    public static class ValoriaVisibleFrameCohesionV1
    {
        public static bool Enabled=false;
        public const string RootName="Valoria · Visible Frame Cohesion v1";
        public static int PiecesBuilt{get;private set;}
        public static int SuppressedProxyRenderers{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);
            PiecesBuilt=0;
            SuppressedProxyRenderers=0;

            SuppressVisiblePlanningStones();
            BuildLowerCityGround(root);
            BuildLateralRouteContinuity(root);
            BuildArchitectureGroundSeats(root);
            BuildPeripheralTransitions(root);
            BuildLowUrbanEdges(root);

            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static void SuppressVisiblePlanningStones()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                string chain=Chain(r.transform);
                // These are presentation-only blockout stones over hidden reserved routes.
                // The real route/reservation objects remain untouched.
                if(chain.Contains("vpd · future route stone")||chain.Contains("vpd · civic approach stone"))
                {
                    r.enabled=false;
                    SuppressedProxyRenderers++;
                }
            }
        }

        static void BuildLowerCityGround(Transform root)
        {
            // Break the large rectangular lower-city read into overlapping civic/work surfaces.
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · west lower civic ground",
                new Vector3(-6.55f,.405f,-4.35f),7.35f,7.15f,-2f));
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · east lower civic ground",
                new Vector3(6.55f,.405f,-4.45f),7.35f,7.20f,2f));
            Add(root,ValoriaGroundKit.StreetBlendWidening("Valoria · VFC · central lower plaza",
                new Vector3(0f,.412f,-6.20f),6.60f,3.45f,0f));
            // A shallow front apron gives the foreground buildings a shared urban base rather than
            // separate floating pads. It intentionally stops before the exterior world.
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · foreground urban apron",
                new Vector3(0f,.385f,-7.70f),15.8f,2.65f,0f));
        }

        static void BuildLateralRouteContinuity(Transform root)
        {
            // Existing reserved lateral routes are preserved. These authored skins make them read
            // as streets that continue into the city rather than isolated rectangular strips.
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · west lateral urban ground",
                new Vector3(-11.35f,.392f,-1.05f),7.70f,2.75f,-1f));
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · east lateral urban ground",
                new Vector3(11.35f,.392f,-.95f),7.70f,2.75f,1f));

            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · west route junction",
                new Vector3(-8.65f,.385f,-1.10f),3.90f,3.35f,-2f));
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · east route junction",
                new Vector3(8.65f,.385f,-1.00f),3.90f,3.35f,2f));
        }

        static void BuildArchitectureGroundSeats(Transform root)
        {
            // Functional buildings keep their exact anchors. Only their contact zones are strengthened.
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · Granero contact court",
                new Vector3(-3.10f,.410f,-4.45f),5.75f,4.80f,-3f));
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · Cuartel contact court",
                new Vector3(6.90f,.410f,-3.65f),6.25f,5.00f,3f));

            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            // Large authored mud/stone seams bury hard model-to-ground contacts.
            foreach(var spec in new[]{
                new Vector4(-5.75f,-5.45f,3.0f,-12f),
                new Vector4(-1.25f,-6.05f,2.7f,18f),
                new Vector4(4.25f,-5.85f,2.9f,-15f),
                new Vector4(8.95f,-5.20f,3.1f,16f)})
                AddAuthored(root,art.SlavicMudFlat,"building-ground seam",
                    new Vector3(spec.x,.425f,spec.y),spec.z,.12f,spec.w,new Color(.84f,.76f,.62f,1f));

            foreach(var spec in new[]{
                new Vector4(-8.75f,-4.95f,2.25f,25f),
                new Vector4(-5.00f,-7.00f,2.10f,70f),
                new Vector4(4.95f,-6.95f,2.05f,118f),
                new Vector4(9.10f,-4.80f,2.25f,205f)})
                AddAuthored(root,art.SlavicFlatRock,"buried foundation rock",
                    new Vector3(spec.x,.285f,spec.y),spec.z,.42f,spec.w,new Color(.58f,.58f,.53f,1f));
        }

        static void BuildPeripheralTransitions(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            // Feather the visible city edge into the valley in the exact pan directions.
            foreach(var spec in new[]{
                new Vector4(-12.6f,-6.45f,4.20f,-8f),
                new Vector4(-13.8f,-2.60f,4.00f,18f),
                new Vector4(12.8f,-6.30f,4.20f,8f),
                new Vector4(14.0f,-2.40f,4.00f,-18f),
                new Vector4(-7.3f,-8.85f,3.60f,24f),
                new Vector4(7.5f,-8.80f,3.60f,-22f)})
                AddAuthored(root,art.SlavicMudFlat,"peripheral earth transition",
                    new Vector3(spec.x,.225f,spec.y),spec.z,.11f,spec.w,new Color(.72f,.64f,.50f,1f));

            foreach(var spec in new[]{
                new Vector4(-13.8f,-5.10f,2.85f,28f),
                new Vector4(-14.7f,.75f,2.65f,78f),
                new Vector4(13.9f,-5.00f,2.85f,208f),
                new Vector4(14.7f,.95f,2.65f,286f),
                new Vector4(-9.4f,-8.55f,2.40f,44f),
                new Vector4(9.6f,-8.45f,2.40f,224f)})
                AddAuthored(root,art.SlavicFlatRock,"peripheral buried rock",
                    new Vector3(spec.x,.13f,spec.y),spec.z,.55f,spec.w,new Color(.52f,.53f,.50f,1f));

            // Irregular earth/rock shoulders dissolve the authored city edge into the valley.
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · west world shoulder",
                new Vector3(-15.45f,.215f,-1.0f),5.25f,3.55f,-4f));
            Add(root,ValoriaGroundKit.TerraceFloor("Valoria · VFC · east world shoulder",
                new Vector3(15.45f,.215f,-.9f),5.25f,3.55f,4f));
        }

        static void BuildLowUrbanEdges(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null||art.SlavicStoneFence==null)return;

            // Low authored parcel/yard edges create continuous urban frontage while preserving
            // clear central and lateral openings. They are visual-only and do not consume parcels.
            foreach(var spec in new[]{
                new Vector4(-8.85f,-6.55f,2.45f,5f),
                new Vector4(-6.00f,-7.20f,2.20f,-4f),
                new Vector4(5.95f,-7.20f,2.20f,4f),
                new Vector4(8.85f,-6.55f,2.45f,-5f),
                new Vector4(-10.15f,-3.15f,2.10f,88f),
                new Vector4(10.15f,-3.05f,2.10f,92f)})
                AddAuthored(root,art.SlavicStoneFence,"low urban edge",
                    new Vector3(spec.x,.43f,spec.y),spec.z,.72f,spec.w,new Color(.76f,.75f,.70f,1f));
        }

        static void Add(Transform root,GameObject go)
        {
            if(go==null)return;
            go.transform.SetParent(root,true);
            PiecesBuilt++;
        }

        static void AddAuthored(Transform root,GameObject source,string role,Vector3 p,float footprint,float height,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceModulated("Valoria · VFC · "+role,source,p,footprint,height,
                Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return;
            go.transform.SetParent(root,true);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            PiecesBuilt++;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}

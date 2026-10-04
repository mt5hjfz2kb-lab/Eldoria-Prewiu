using Eldoria.Domain;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    // One significant visual-only replacement pass for the largest secondary/support masses
    // visible from the promoted HOME/PAN envelope. Gameplay topology remains untouched.
    public static class ValoriaSecondarySupportFormReplacementV1
    {
        public static bool Enabled=false;
        public const string RootName="Valoria · Secondary Support Form Replacement v1";
        public static int PiecesBuilt{get;private set;}
        public static int SuppressedRenderers{get;private set;}

        public static void Build(Transform parent,PlayerState state)
        {
            if(!Enabled||parent==null||state==null)return;
            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(parent,true);
            PiecesBuilt=0;
            SuppressedRenderers=0;

            // Remove only presentation renderers from the weak/repetitive support language.
            // Objects/colliders/routes remain in place underneath.
            SuppressWeakSupportAndHouseCluster();

            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            BuildPrimaryRetainingFront(root,art);
            BuildRockArchitectureInterfaces(root,art);
            BuildArticulatedWestQuarter(root,art);
            BuildSecondaryShoulders(root,art);

            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in root.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static void SuppressWeakSupportAndHouseCluster()
        {
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var chain=Chain(r.transform);
                bool weakSupport=
                    chain.Contains("vpd · groundkit l1 retaining edge")||
                    chain.Contains("vpd · retaining stone face")||
                    chain.Contains("vpd · authored retaining rock")||
                    chain.Contains("vpd · west rebuilders rescued seam");
                bool repeatedHousing=
                    chain.Contains("vpd · west rebuilders home")||
                    chain.Contains("vpd · west rebuilders upper dwelling")||
                    chain.Contains("valoria · reused civil house");
                if(!weakSupport&&!repeatedHousing)continue;
                r.enabled=false;
                SuppressedRenderers++;
            }
        }

        static void BuildPrimaryRetainingFront(Transform root,ValoriaExternalAssetLibrary art)
        {
            // Three large authored bays replace the previous fence/rock fragments. The centre stays
            // lower than the Bastion so the Hero remains dominant while the terrace reads as architecture.
            var wall=art.MasonryWall!=null?art.MasonryWall:(art.StoneWall!=null?art.StoneWall:art.SlavicStoneFence);
            Add(root,wall,"retaining west bay",new Vector3(-5.0f,.43f,4.58f),5.0f,2.35f,2f,
                new Color(.79f,.75f,.66f,1f));
            Add(root,wall,"retaining centre bay",new Vector3(0f,.43f,4.54f),4.5f,2.05f,0f,
                new Color(.82f,.77f,.68f,1f));
            Add(root,wall,"retaining east bay",new Vector3(5.0f,.43f,4.58f),5.0f,2.35f,-2f,
                new Color(.79f,.75f,.66f,1f));

            // End piers read as structural terminations rather than generic support towers.
            var pier=art.MasonryTower!=null?art.MasonryTower:art.StoneTower;
            Add(root,pier,"west retaining pier",new Vector3(-7.25f,.38f,4.72f),2.25f,3.0f,8f,
                new Color(.73f,.70f,.63f,1f));
            Add(root,pier,"east retaining pier",new Vector3(7.25f,.38f,4.72f),2.25f,3.0f,-8f,
                new Color(.73f,.70f,.63f,1f));
        }

        static void BuildRockArchitectureInterfaces(Transform root,ValoriaExternalAssetLibrary art)
        {
            // Large buried rock wedges terminate the masonry into the mountain. They are intentionally
            // asymmetric so the terrace does not read as a freestanding wall placed on a board.
            Add(root,art.SlavicFlatRock,"west wall-rock transition",new Vector3(-8.55f,.14f,4.95f),
                4.20f,1.55f,34f,new Color(.74f,.73f,.68f,1f));
            Add(root,art.SlavicBoulder!=null?art.SlavicBoulder:art.SlavicFlatRock,"west buried shoulder",
                new Vector3(-9.45f,.06f,6.10f),3.25f,1.70f,78f,new Color(.69f,.69f,.65f,1f));
            Add(root,art.SlavicFlatRock,"east wall-rock transition",new Vector3(8.60f,.14f,4.90f),
                4.30f,1.60f,214f,new Color(.74f,.73f,.68f,1f));
            Add(root,art.SlavicBoulder!=null?art.SlavicBoulder:art.SlavicFlatRock,"east buried shoulder",
                new Vector3(9.55f,.06f,6.00f),3.20f,1.72f,286f,new Color(.69f,.69f,.65f,1f));
        }

        static void BuildArticulatedWestQuarter(Transform root,ValoriaExternalAssetLibrary art)
        {
            // Replace six near-identical houses laid out as one prefab cluster with four differentiated
            // frontage masses. Gaps are deliberate courts/lanes, not empty reserve.
            var house=art.SlavicHouse;
            var shed=art.SlavicShed!=null?art.SlavicShed:house;

            Add(root,house,"west lower tall house",new Vector3(-11.85f,.34f,-3.45f),
                3.05f,3.45f,-12f,new Color(.91f,.86f,.76f,1f));
            Add(root,shed,"west lower workshop",new Vector3(-16.70f,.34f,-2.55f),
                3.65f,2.65f,18f,new Color(.86f,.79f,.67f,1f));

            // Middle court remains open between the lower pair and the upper shelf.
            Add(root,house,"west middle gable",new Vector3(-13.35f,.38f,2.85f),
                2.75f,3.05f,11f,new Color(.88f,.82f,.72f,1f));
            Add(root,house,"west upper landmark house",new Vector3(-17.15f,1.20f,5.75f),
                3.25f,3.75f,-20f,new Color(.92f,.87f,.78f,1f));

            // Two short wall returns frame a lane/court instead of packing another house into it.
            var wall=art.SlavicStoneFence!=null?art.SlavicStoneFence:art.MasonryWall;
            Add(root,wall,"west quarter court wall south",new Vector3(-14.35f,.32f,-.10f),
                2.75f,.95f,84f,new Color(.80f,.78f,.72f,1f));
            Add(root,wall,"west quarter court wall north",new Vector3(-15.55f,.88f,4.45f),
                2.35f,1.05f,-8f,new Color(.78f,.76f,.70f,1f));

            // One rock plinth binds the elevated house to the shelf; no decorative ground overlay.
            Add(root,art.SlavicFlatRock,"west upper house rock plinth",new Vector3(-17.20f,.72f,5.70f),
                4.50f,1.15f,142f,new Color(.72f,.71f,.67f,1f));
        }

        static void BuildSecondaryShoulders(Transform root,ValoriaExternalAssetLibrary art)
        {
            // Sparse secondary frontages on the pan edges keep the city continuous without creating
            // another repeated housing cluster or filling future functional parcels.
            var wall=art.MasonryWall!=null?art.MasonryWall:art.SlavicStoneFence;
            Add(root,wall,"west lateral support face",new Vector3(-11.15f,.20f,.95f),
                4.10f,1.65f,92f,new Color(.76f,.73f,.66f,1f));
            Add(root,wall,"east lateral support face",new Vector3(11.30f,.20f,1.10f),
                4.20f,1.70f,88f,new Color(.76f,.73f,.66f,1f));

            Add(root,art.SlavicFlatRock,"west lateral rock return",new Vector3(-12.10f,.06f,2.90f),
                3.10f,1.25f,52f,new Color(.70f,.70f,.66f,1f));
            Add(root,art.SlavicFlatRock,"east lateral rock return",new Vector3(12.25f,.06f,3.00f),
                3.10f,1.25f,232f,new Color(.70f,.70f,.66f,1f));
        }

        static void Add(Transform root,GameObject source,string role,Vector3 ground,
            float footprint,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return;
            var go=ValoriaKit.BenchmarkPieceModulated("Valoria · SSR v1 · "+role,source,ground,
                footprint,maxHeight,Quaternion.Euler(0f,yaw,0f),tint);
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

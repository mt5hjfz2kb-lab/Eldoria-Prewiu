using UnityEngine;

// World Map gate refresh: replace stale runner capture without touching Valoria.
namespace Eldoria.Presentation
{
    /// <summary>
    /// Reusable visual-only route composition for the pulled-back world map.
    /// It combines a continuous worn-earth corridor with low-salience hard-surface
    /// landmarks so routes stay readable on mobile without owning gameplay topology.
    /// </summary>
    public static class WorldRouteKit
    {
        static readonly Color Mud=new Color(.34f,.27f,.19f,1f);
        static readonly Color EdgeStone=new Color(.50f,.49f,.45f,1f);
        static readonly Color FenceStone=new Color(.36f,.35f,.33f,1f);

        public static GameObject MarchRoute(string name,Vector3 center,float length,float width,float yawDegrees)
        {
            var root=new GameObject(name);
            root.transform.position=center;
            root.transform.rotation=Quaternion.Euler(0,yawDegrees,0);
            var art=ValoriaExternalAssetLibrary.Load();

            // Keep the certified continuous earth trail as the route body.
            var trail=ValoriaGroundKit.TrailStraight("Frontier · march trail",center,length,width,yawDegrees);
            if(trail!=null)trail.transform.SetParent(root.transform,true);

            // Repeated mud wear breaks the broad terrain field and makes the corridor legible
            // even when the underlying earth tones are intentionally close.
            if(art!=null&&art.SlavicMudFlat!=null)
            {
                for(int i=0;i<5;i++)
                {
                    float z=Mathf.Lerp(-length*.40f,length*.40f,i/4f);
                    float x=(i%2==0?-.12f:.11f);
                    var world=root.transform.TransformPoint(new Vector3(x,.025f,z));
                    var wear=ValoriaKit.BenchmarkPieceModulated(name+" · mud wear "+(i+1),
                        art.SlavicMudFlat,world,width*.72f,.09f,
                        root.transform.rotation*Quaternion.Euler(0,(i%2==0?-6f:7f),0),
                        Mud*(.94f+(i%3)*.025f));
                    StripColliders(wear); Parent(root,wear);
                }
            }

            // Alternating rock clusters create a route rhythm without a fence-like corridor.
            for(int i=0;i<5;i++)
            {
                float z=Mathf.Lerp(-length*.38f,length*.38f,i/4f);
                float side=i%2==0?-1f:1f;
                var world=root.transform.TransformPoint(new Vector3(side*width*.68f,.02f,z));
                var cluster=WorldResourceKit.RoadsideRockCluster(name+" · verge cluster "+(i+1),
                    world,.55f+(i%2)*.08f,yawDegrees+(i%2==0?-12f:17f));
                StripColliders(cluster); Parent(root,cluster);
            }

            // Two broken retaining fragments work as low-key landmarks at route transitions.
            if(art!=null&&art.SlavicStoneFence!=null)
            {
                foreach(var spec in new[]{
                    new Vector4(-width*.78f,-length*.31f,-18f,.82f),
                    new Vector4(width*.80f,length*.29f,14f,.76f)})
                {
                    var world=root.transform.TransformPoint(new Vector3(spec.x,.04f,spec.y));
                    var marker=ValoriaKit.BenchmarkPieceTinted(name+" · route marker",
                        art.SlavicStoneFence,world,width*spec.w,.42f,
                        root.transform.rotation*Quaternion.Euler(0,spec.z,0),FenceStone);
                    StripColliders(marker); Parent(root,marker);
                }
            }
            return root;
        }

        static void Parent(GameObject root,GameObject child)
        {
            if(root!=null&&child!=null)child.transform.SetParent(root.transform,true);
        }

        static void StripColliders(GameObject root)
        {
            if(root==null)return;
            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
        }
    }
}

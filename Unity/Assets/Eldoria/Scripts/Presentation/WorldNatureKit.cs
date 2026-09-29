using UnityEngine;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Reusable visual-only world-map nature compositions built from the strongest
    /// Slavic World environment pieces already licensed in the project.
    /// Gameplay hotspots and resource rules remain separate.
    /// </summary>
    public static class WorldNatureKit
    {
        static readonly Color PineTint=new Color(.60f,.70f,.58f,1f);
        static readonly Color BushTint=new Color(.34f,.46f,.33f,1f);
        static readonly Color MossTint=new Color(.58f,.68f,.52f,1f);
        static readonly Color RockTint=new Color(.67f,.67f,.62f,1f);

        public static GameObject ForestResourcePocket(string name,Vector3 center,float radius)
        {
            var root=new GameObject(name);
            root.transform.position=center;
            var art=ValoriaExternalAssetLibrary.Load();

            // Three height bands keep the pocket readable as forest from the 4X camera
            // without exposing a repeated single-tree stamp.
            var trees=new[]{
                new Vector4(-.72f,-.20f,.72f,18f),
                new Vector4(-.34f,.58f,.88f,-31f),
                new Vector4(.16f,-.56f,.66f,47f),
                new Vector4(.48f,.28f,.82f,103f),
                new Vector4(.76f,-.18f,.62f,151f),
                new Vector4(-.62f,.42f,.58f,218f),
                new Vector4(.02f,.16f,.76f,279f)
            };
            for(int i=0;i<trees.Length;i++)
            {
                var t=trees[i];
                var p=center+new Vector3(t.x*radius,.02f,t.y*radius);
                var prefab=(i%3==0&&art!=null&&art.SlavicTreeTall!=null)?art.SlavicTreeTall:(art!=null?art.SlavicTree:null);
                var tree=ValoriaKit.BenchmarkPieceModulated(name+" · pine "+(i+1),prefab,p,
                    radius*(.34f+t.z*.10f),radius*(1.05f+t.z*.34f),Quaternion.Euler(0,t.w,0),PineTint*(.92f+(i%3)*.035f));
                StripColliders(tree);
            }

            var shrubs=new[]{
                new Vector3(-.78f,.05f,.42f),new Vector3(-.45f,.05f,-.52f),
                new Vector3(.58f,.05f,.48f),new Vector3(.72f,.05f,-.42f),
                new Vector3(.08f,.05f,.68f)
            };
            for(int i=0;i<shrubs.Length;i++)
            {
                var p=center+new Vector3(shrubs[i].x*radius,.02f,shrubs[i].z*radius);
                var bush=ValoriaKit.BenchmarkPieceModulated(name+" · undergrowth "+(i+1),
                    art!=null?art.SlavicBush:null,p,radius*.32f,radius*.23f,
                    Quaternion.Euler(0,37f+i*61f,0),BushTint*(.94f+(i%2)*.05f));
                StripColliders(bush);
            }

            for(int i=0;i<3;i++)
            {
                float a=(25f+i*117f)*Mathf.Deg2Rad;
                var p=center+new Vector3(Mathf.Cos(a)*radius*.62f,.015f,Mathf.Sin(a)*radius*.62f);
                var moss=ValoriaKit.BenchmarkPieceModulated(name+" · moss "+(i+1),
                    art!=null?art.SlavicMoss:null,p,radius*.42f,radius*.075f,
                    Quaternion.Euler(0,i*74f,0),MossTint);
                StripColliders(moss);
            }

            var log=ValoriaKit.BenchmarkPiece(name+" · harvest timber",art!=null?art.Firewood:null,
                center+new Vector3(radius*.55f,.08f,-radius*.08f),radius*.48f,radius*.26f,Quaternion.Euler(0,-24f,0));
            StripColliders(log);

            var rock=ValoriaKit.BenchmarkPieceModulated(name+" · buried rock",art!=null?art.SlavicFlatRock:null,
                center+new Vector3(-radius*.62f,.01f,-radius*.50f),radius*.40f,radius*.16f,
                Quaternion.Euler(0,21f,0),RockTint);
            StripColliders(rock);
            return root;
        }

        public static GameObject ForestEdgeCluster(string name,Vector3 center,float span,float yaw=0f)
        {
            var root=new GameObject(name);
            root.transform.position=center;
            root.transform.rotation=Quaternion.Euler(0,yaw,0);
            var art=ValoriaExternalAssetLibrary.Load();
            for(int i=0;i<4;i++)
            {
                float x=(-.5f+i/3f)*span;
                var world=root.transform.TransformPoint(new Vector3(x,.02f,(i%2==0?.14f:-.10f)*span));
                var tree=ValoriaKit.BenchmarkPieceModulated(name+" · edge pine "+(i+1),
                    art!=null?(i%2==0?art.SlavicTreeTall:art.SlavicTree):null,
                    world,span*.22f,span*(.64f+(i%3)*.09f),
                    root.transform.rotation*Quaternion.Euler(0,i*49f,0),PineTint);
                StripColliders(tree);
            }
            return root;
        }

        static void StripColliders(GameObject root)
        {
            if(root==null)return;
            foreach(var c in root.GetComponentsInChildren<Collider>(true))c.enabled=false;
        }
    }
}

using UnityEngine;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Reusable visual-only hard-surface world resource compositions.
    /// Gameplay targets remain independent from these visuals.
    /// </summary>
    public static class WorldResourceKit
    {
        static readonly Color QuarryStone=new Color(.52f,.50f,.46f,1f);
        static readonly Color QuarryDark=new Color(.34f,.33f,.31f,1f);
        static readonly Color QuarryDust=new Color(.46f,.40f,.32f,1f);

        public static GameObject QuarryResourcePocket(string name,Vector3 center,float radius)
        {
            var root=new GameObject(name);
            root.transform.position=center;
            var art=ValoriaExternalAssetLibrary.Load();

            // Back cut: one dominant rock face plus asymmetrical supporting masses.
            ValoriaKit.RockCluster(name+" · quarry face",center+new Vector3(.20f,.02f,.35f),radius*.50f,12);
            ValoriaKit.RockCluster(name+" · spoil left",center+new Vector3(-radius*.56f,.02f,-radius*.12f),radius*.28f,6);
            ValoriaKit.RockCluster(name+" · spoil right",center+new Vector3(radius*.55f,.02f,-radius*.34f),radius*.24f,5);

            var boulder=ValoriaKit.BenchmarkPieceModulated(name+" · authored boulder",
                art!=null?art.SlavicBoulder:null,center+new Vector3(radius*.48f,.04f,radius*.22f),
                radius*.50f,radius*.42f,Quaternion.Euler(0,23f,0),QuarryStone);
            StripColliders(boulder); Parent(root,boulder);

            var ledge=ValoriaKit.BenchmarkPieceModulated(name+" · authored ledge",
                art!=null?art.SlavicFlatRock:null,center+new Vector3(-radius*.28f,.03f,radius*.46f),
                radius*.72f,radius*.22f,Quaternion.Euler(0,-17f,0),QuarryStone*.94f);
            StripColliders(ledge); Parent(root,ledge);

            var wall=ValoriaKit.BenchmarkPieceTinted(name+" · retaining edge",
                art!=null?art.SlavicStoneFence:null,center+new Vector3(-radius*.50f,.08f,-radius*.58f),
                radius*.86f,radius*.34f,Quaternion.Euler(0,8f,0),QuarryDark);
            StripColliders(wall); Parent(root,wall);

            // Ground wear is subtle: it should blend into terrain, not look like a placed decal.
            for(int i=0;i<2;i++)
            {
                var mud=ValoriaKit.BenchmarkPieceModulated(name+" · dust wear "+(i+1),
                    art!=null?art.SlavicMudFlat:null,
                    center+new Vector3((i==0?-.18f:.26f)*radius,.018f,(-.34f+i*.24f)*radius),
                    radius*.72f,radius*.08f,Quaternion.Euler(0,i==0?-11f:19f,0),QuarryDust);
                StripColliders(mud); Parent(root,mud);
            }
            return root;
        }

        public static GameObject RoadsideRockCluster(string name,Vector3 center,float radius,float yaw=0f)
        {
            var root=new GameObject(name);
            root.transform.position=center;
            root.transform.rotation=Quaternion.Euler(0,yaw,0);
            var art=ValoriaExternalAssetLibrary.Load();
            var a=ValoriaKit.BenchmarkPieceModulated(name+" · flat rock",art!=null?art.SlavicFlatRock:null,
                center,radius*.76f,radius*.22f,Quaternion.Euler(0,yaw,0),QuarryStone);
            StripColliders(a); Parent(root,a);
            var b=ValoriaKit.BenchmarkPieceModulated(name+" · boulder",art!=null?art.SlavicBoulder:null,
                center+root.transform.right*radius*.36f+root.transform.forward*radius*.10f,
                radius*.44f,radius*.38f,Quaternion.Euler(0,yaw+31f,0),QuarryStone*.92f);
            StripColliders(b); Parent(root,b);
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

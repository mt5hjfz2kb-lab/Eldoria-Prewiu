using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Phase C only: authored starter geometry in the real current Valoria frame.
    // Materials/final-look/camera are deliberately left for later gates.
    public static class ValoriaProductionArtResetV1
    {
        public const string RootName="Valoria · Production Art Reset v1";
        public static int Pieces;

        public static void Apply(Transform canonicalRoot,PlayerState state)
        {
            if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);Pieces=0;

            var wall=Load("Valoria_WallSegment_v1");
            var gate=Load("Valoria_MainGate_v1");
            var tower=Load("Valoria_Tower_v1");
            var house=Load("Valoria_CivicHouse_v1");
            var workshop=Load("Valoria_Workshop_v1");

            // Replace only two visibly provisional/support residential shells.
            SuppressVisualRoot("VPD · upper dwelling");
            SuppressVisualRoot("VPD · rescued upper civil residence");

            // Entry cell around the certified lower entrance; visual-only.
            Piece(root,gate,"main gate",new Vector3(0f,.14f,-8.95f),4.45f,3.85f,0f);
            Piece(root,tower,"west entry tower",new Vector3(-3.15f,.13f,-8.25f),2.25f,4.05f,4f);
            Piece(root,tower,"east entry tower",new Vector3(3.15f,.13f,-8.25f),2.25f,4.05f,-4f);
            Piece(root,wall,"west entry curtain",new Vector3(-6.15f,.13f,-7.75f),3.65f,2.55f,7f);
            Piece(root,wall,"east entry curtain",new Vector3(6.15f,.13f,-7.75f),3.65f,2.55f,-7f);

            // Existing upper civic footprints, now using the authored source family.
            Piece(root,house,"upper west civic house",new Vector3(-6.05f,2.76f,7.15f),3.15f,3.55f,-12f);
            Piece(root,house,"upper east civic house",new Vector3(5.15f,2.76f,7.15f),3.05f,3.45f,8f);

            // One secondary authored workshop in a non-gameplay civic gap.
            Piece(root,workshop,"lower civic workshop",new Vector3(3.55f,.40f,-.65f),2.75f,2.65f,-12f);

            StripGameplay(root.gameObject);
        }

        static GameObject Load(string n)
        {
            var x=Resources.Load<GameObject>("Valoria/ProductionArt/StarterFamily/"+n);
            if(x==null)throw new InvalidOperationException("Missing production art starter resource "+n);
            return x;
        }

        static void SuppressVisualRoot(string rootName)
        {
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t==null||t.name!=rootName)continue;
                foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;
            }
        }

        static void Piece(Transform root,GameObject src,string role,Vector3 p,float footprint,float height,float yaw)
        {
            var go=ValoriaKit.BenchmarkPiece("Valoria · Production Art · "+role,src,p,footprint,height,Quaternion.Euler(0,yaw,0));
            if(go==null)throw new InvalidOperationException("Failed production piece "+role);
            go.transform.SetParent(root,true);ApplyProductionMaterials(go);StripGameplay(go);Pieces++;
        }

        static void StripGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }
    }
}

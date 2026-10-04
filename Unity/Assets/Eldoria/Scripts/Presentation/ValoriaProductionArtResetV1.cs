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
            Piece(root,gate,"main gate",new Vector3(0f,.14f,-8.72f),3.35f,2.75f,0f);
            Piece(root,tower,"west entry tower",new Vector3(-2.95f,.13f,-8.30f),1.72f,2.95f,4f);
            Piece(root,tower,"east entry tower",new Vector3(2.95f,.13f,-8.30f),1.72f,2.95f,-4f);
            Piece(root,wall,"west entry curtain",new Vector3(-5.25f,.13f,-8.02f),2.55f,1.55f,5f);
            Piece(root,wall,"east entry curtain",new Vector3(5.25f,.13f,-8.02f),2.55f,1.55f,-5f);

            // Existing upper civic footprints, now using the authored source family.
            Piece(root,house,"upper west civic house",new Vector3(-5.80f,2.76f,6.95f),2.42f,2.95f,-12f);
            Piece(root,house,"upper east civic house",new Vector3(5.25f,2.76f,6.95f),2.38f,2.90f,8f);

            // One secondary authored workshop in a non-gameplay civic gap.
            Piece(root,workshop,"lower civic workshop",new Vector3(4.15f,.24f,-2.05f),2.15f,2.15f,-12f);

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
            go.transform.SetParent(root,true);ApplyNeutralPreview(go);StripGameplay(go);Pieces++;
        }

        static void ApplyNeutralPreview(GameObject go)
        {
            // Geometry proof only: neutral diagnostic colors keep imported glTF from reading as
            // blown-out white. This is not the phase-D production material stack.
            int baseColor=Shader.PropertyToID("_BaseColor");
            int color=Shader.PropertyToID("_Color");
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats=r.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var m=mats[i]; if(m==null)continue;
                    string n=(m.name??"").ToLowerInvariant();
                    Color tint=
                        n.Contains("slate")||n.Contains("roof") ? new Color(.13f,.18f,.22f,1f) :
                        n.Contains("timber")||n.Contains("wood") ? new Color(.24f,.14f,.075f,1f) :
                        n.Contains("metal") ? new Color(.24f,.23f,.21f,1f) :
                        n.Contains("blue")||n.Contains("herald") ? new Color(.08f,.24f,.52f,1f) :
                        n.Contains("window") ? new Color(.63f,.28f,.08f,1f) :
                        n.Contains("plaster") ? new Color(.61f,.51f,.38f,1f) :
                        n.Contains("dark") ? new Color(.34f,.31f,.27f,1f) :
                        new Color(.52f,.46f,.38f,1f);
                    var block=new MaterialPropertyBlock();
                    r.GetPropertyBlock(block,i);
                    block.SetColor(baseColor,tint);
                    block.SetColor(color,tint);
                    r.SetPropertyBlock(block,i);
                }
            }
        }

        static void StripGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }
    }
}

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
            Piece(root,gate,"main gate",new Vector3(0f,.14f,-8.72f),2.75f,2.25f,0f);
            Piece(root,tower,"west entry tower",new Vector3(-2.72f,.13f,-8.34f),1.38f,2.35f,4f);
            Piece(root,tower,"east entry tower",new Vector3(2.72f,.13f,-8.34f),1.38f,2.35f,-4f);
            Piece(root,wall,"west entry curtain",new Vector3(-4.85f,.13f,-8.08f),1.88f,1.08f,5f);
            Piece(root,wall,"east entry curtain",new Vector3(4.85f,.13f,-8.08f),1.88f,1.08f,-5f);

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
            go.transform.SetParent(root,true);ApplyProductionMaterials(go);StripGameplay(go);Pieces++;
        }

        static void ApplyProductionMaterials(GameObject go)
        {
            var stone=ValoriaKit.DetailedSurfaceMaterial(new Color(.50f,.42f,.33f,1f),"stone",new Vector2(2.1f,2.1f),1.12f);
            var darkStone=ValoriaKit.DetailedSurfaceMaterial(new Color(.32f,.29f,.25f,1f),"stone",new Vector2(2.5f,2.5f),1.05f);
            var timber=ValoriaKit.DetailedSurfaceMaterial(new Color(.17f,.09f,.045f,1f),"wood",new Vector2(1.8f,1.8f),.96f);
            var slate=ValoriaKit.DetailedSurfaceMaterial(new Color(.075f,.115f,.17f,1f),"slate",new Vector2(2.0f,2.0f),1.08f);
            var plaster=ValoriaKit.DetailedSurfaceMaterial(new Color(.58f,.46f,.32f,1f),"stone",new Vector2(3.0f,3.0f),.42f);
            var blue=Solid("Eldoria Heraldry Blue",new Color(.035f,.13f,.36f,1f),.24f,0f);
            var warm=Solid("Eldoria Warm Window",new Color(.82f,.29f,.055f,1f),.38f,0f);
            var metal=Solid("Eldoria Metal Accent",new Color(.20f,.15f,.075f,1f),.44f,.35f);
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                string on=(r.gameObject.name??"").ToLowerInvariant();
                var src=r.sharedMaterials;var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    string mn=src[i]!=null?(src[i].name??"").ToLowerInvariant():"";
                    string n=on+"|"+mn;
                    if(n.Contains("roof")||n.Contains("slate"))dst[i]=slate;
                    else if(n.Contains("timber")||n.Contains("wood")||n.Contains("door")||n.Contains("frame")||n.Contains("post")||n.Contains("beam")||n.Contains("bench"))dst[i]=timber;
                    else if(n.Contains("plaster")||n.Contains("upper floor")||n.Contains("annex")||n.Contains("dormer body"))dst[i]=plaster;
                    else if(n.Contains("banner")||n.Contains("herald"))dst[i]=blue;
                    else if(n.Contains("window"))dst[i]=warm;
                    else if(n.Contains("metal")||n.Contains("finial"))dst[i]=metal;
                    else if(n.Contains("plinth")||n.Contains("shadow")||n.Contains("chimney"))dst[i]=darkStone;
                    else dst[i]=stone;
                }
                r.sharedMaterials=dst;
            }
        }

        static Material Solid(string name,Color color,float smooth,float metallic)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader){name=name};
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Color"))m.SetColor("_Color",color);
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smooth);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);
            return m;
        }

        static void ApplyNeutralPreview(GameObject go)
        {
            // Phase-C clay/role preview. glTFast materials use shader-specific properties, so
            // MaterialPropertyBlock tinting is not reliable here. Replace only the candidate
            // instance materials with deterministic URP/Lit diagnostics. Phase D later restores
            // texture/mask-driven production surfaces.
            var lit=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=r.sharedMaterials;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    var sm=src[i];
                    string n=(sm!=null?sm.name:"").ToLowerInvariant();
                    Color tint=
                        n.Contains("slate")||n.Contains("roof") ? new Color(.16f,.20f,.24f,1f) :
                        n.Contains("timber")||n.Contains("wood") ? new Color(.28f,.17f,.09f,1f) :
                        n.Contains("metal") ? new Color(.25f,.24f,.22f,1f) :
                        n.Contains("blue")||n.Contains("herald") ? new Color(.10f,.28f,.55f,1f) :
                        n.Contains("window") ? new Color(.56f,.24f,.07f,1f) :
                        n.Contains("plaster") ? new Color(.58f,.48f,.35f,1f) :
                        n.Contains("dark") ? new Color(.34f,.31f,.27f,1f) :
                        new Color(.53f,.46f,.37f,1f);
                    var m=new Material(lit){name="Valoria Phase C Preview · "+n};
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",n.Contains("metal")?.22f:.035f);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",n.Contains("metal")?.12f:0f);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static void StripGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }
    }
}

using System;
using Eldoria.Domain;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.Presentation
{
    // Reset-owned visual source: dedicated Unity stop gate is authoritative for C/D/E/F iteration.
    // Materials/final-look/camera are deliberately left for later gates.
    public static class ValoriaProductionArtResetV1
    {
        public const string RootName="Valoria · Production Art Reset v1";
        public static int Pieces;
        public const string CandidateFamily="StarterFamilyV2";
        public const string CandidateSuffix="_v2";

        public static void Apply(Transform canonicalRoot,PlayerState state)
        {
            if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName).transform;root.SetParent(canonicalRoot,true);Pieces=0;

            var wall=Load("Valoria_WallSegment"+CandidateSuffix);
            var gate=Load("Valoria_MainGate"+CandidateSuffix);
            var tower=Load("Valoria_Tower"+CandidateSuffix);
            var house=Load("Valoria_CivicHouse"+CandidateSuffix);
            var workshop=Load("Valoria_Workshop"+CandidateSuffix);

            // Replace only two visibly provisional/support residential shells.
            SuppressVisualRoot("VPD · upper dwelling");
            SuppressVisualRoot("VPD · rescued upper civil residence");

            // Entry cell around the certified lower entrance; visual-only.
            Piece(root,gate,"main gate",new Vector3(0f,.14f,-8.55f),5.10f,3.65f,0f);
            Piece(root,tower,"west entry tower",new Vector3(-4.05f,.13f,-8.22f),2.45f,3.85f,4f);
            Piece(root,tower,"east entry tower",new Vector3(4.05f,.13f,-8.22f),2.45f,3.85f,-4f);
            Piece(root,wall,"west entry curtain",new Vector3(-6.75f,.13f,-7.95f),3.25f,1.85f,5f);
            Piece(root,wall,"east entry curtain",new Vector3(6.75f,.13f,-7.95f),3.25f,1.85f,-5f);

            // Existing upper civic footprints, now using the authored source family.
            Piece(root,house,"upper west civic house",new Vector3(-5.80f,2.76f,6.75f),3.20f,3.55f,-12f);
            Piece(root,house,"upper east civic house",new Vector3(5.25f,2.76f,6.75f),3.15f,3.50f,8f);

            // One secondary authored workshop in a non-gameplay civic gap.
            Piece(root,workshop,"lower civic workshop",new Vector3(4.45f,.24f,-2.15f),3.15f,2.75f,-12f);

            StripGameplay(root.gameObject);
        }

        static GameObject Load(string n)
        {
            var x=Resources.Load<GameObject>("Valoria/ProductionArt/"+CandidateFamily+"/"+n);
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
            go.transform.SetParent(root,true);PreserveSourceMaterials(go);StripGameplay(go);Pieces++;
        }

        static void PreserveSourceMaterials(GameObject go)
        {
            // Starter v2 is a source-reauthoring proof: donor/source materials are intentionally
            // preserved so geometry richness and authored surface response are judged together.
            // Phase D may normalize them only after this family clears the zoom9/mobile stop gate.
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                r.receiveShadows=true;
                r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            }
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
            // Phase-C geometry proof only. Some imported glTF shaders ignore URP property blocks,
            // so replace them with deterministic neutral URP/Lit diagnostic materials. Phase D
            // later replaces this entire preview path with the production PBR material stack.
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=r.sharedMaterials;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    var m=src[i];
                    string n=(m!=null?m.name:"").ToLowerInvariant();
                    string role=
                        n.Contains("slate")||n.Contains("roof") ? "slate" :
                        n.Contains("timber")||n.Contains("wood") ? "timber" :
                        n.Contains("metal") ? "metal" :
                        n.Contains("blue")||n.Contains("herald") ? "blue" :
                        n.Contains("window") ? "window" :
                        n.Contains("plaster") ? "plaster" :
                        n.Contains("dark") ? "stone-dark" : "stone";
                    Color tint=
                        role=="slate" ? new Color(.12f,.17f,.22f,1f) :
                        role=="timber" ? new Color(.25f,.14f,.075f,1f) :
                        role=="metal" ? new Color(.22f,.21f,.19f,1f) :
                        role=="blue" ? new Color(.07f,.22f,.48f,1f) :
                        role=="window" ? new Color(.66f,.28f,.07f,1f) :
                        role=="plaster" ? new Color(.58f,.49f,.37f,1f) :
                        role=="stone-dark" ? new Color(.31f,.29f,.25f,1f) :
                        new Color(.50f,.44f,.36f,1f);
                    var nm=new Material(shader){name="Valoria C3 Neutral · "+role};
                    if(nm.HasProperty("_BaseColor"))nm.SetColor("_BaseColor",tint);
                    if(nm.HasProperty("_Color"))nm.SetColor("_Color",tint);
                    if(nm.HasProperty("_Metallic"))nm.SetFloat("_Metallic",role=="metal"?.12f:0f);
                    if(nm.HasProperty("_Smoothness"))nm.SetFloat("_Smoothness",role=="metal"?.16f:role=="window"?.20f:.035f);
                    dst[i]=nm;
                }
                r.sharedMaterials=dst;
                r.SetPropertyBlock(null);
            }
        }

        static void StripGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }
    }
}

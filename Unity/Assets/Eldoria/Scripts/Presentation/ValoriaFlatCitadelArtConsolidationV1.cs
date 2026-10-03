using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.Presentation
{
    /// <summary>
    /// Large-block art consolidation applied on top of the accepted Flat Citadel Production Uplift.
    /// Macrocomposition and gameplay authority remain untouched.
    /// </summary>
    public static class ValoriaFlatCitadelArtConsolidationV1
    {
        public const string RootName = "Valoria · Flat Citadel Art Consolidation v1";
        public static int HiddenUpliftWallRenderers { get; private set; }
        public static int ConsolidatedWallModules { get; private set; }
        public static int ReservedParcels { get; private set; }
        public static int MaterialsConsolidated { get; private set; }
        public static int BastionInterfaceModules { get; private set; }

        static readonly Color Stone = new Color(.62f,.60f,.55f,1f);
        static readonly Color StoneDark = new Color(.49f,.48f,.45f,1f);
        static readonly Color StoneLight = new Color(.70f,.68f,.63f,1f);
        static readonly Color Earth = new Color(.41f,.34f,.24f,1f);
        static readonly Color Wood = new Color(.42f,.29f,.18f,1f);
        static readonly Color Blue = new Color(.16f,.28f,.44f,1f);

        public static void Apply(Transform canonicalRoot, PlayerState state)
        {
            if(canonicalRoot==null)throw new ArgumentNullException(nameof(canonicalRoot));

            var old=GameObject.Find(RootName);
            if(old!=null)Object.DestroyImmediate(old);

            var root=new GameObject(RootName).transform;
            root.SetParent(canonicalRoot,true);

            var uplift=GameObject.Find(ValoriaFlatCitadelProductionUpliftV1.RootName);
            if(uplift==null)throw new InvalidOperationException("Art Consolidation requires the accepted Flat Citadel Production Uplift visual root.");

            HiddenUpliftWallRenderers=HideUpliftOuterWall(uplift.transform);
            HideReservedPlotOccupants(uplift.transform);

            BuildProgressionReservations(root);
            BuildWallContinuityBase(root);
            BuildConsolidatedWall(root);
            BuildBastionArchitecturalInterface(root);
            BuildGroundEdgeIntegration(root);
            BuildPerimeterLife(root);
            ApplyAtmosphere();
            MaterialsConsolidated=ConsolidateMaterialLanguage(uplift.transform,root);

            DisableGameplay(root.gameObject);
        }

        static int HideUpliftOuterWall(Transform uplift)
        {
            int count=0;
            foreach(var r in uplift.GetComponentsInChildren<Renderer>(true))
            {
                string n=Chain(r.transform);
                bool outer=
                    n.Contains("flat citadel production · main gate")||
                    n.Contains("flat citadel production · gate tower")||
                    n.Contains("flat citadel production · front curtain")||
                    n.Contains("flat citadel production · west curtain")||
                    n.Contains("flat citadel production · east curtain")||
                    n.Contains("flat citadel production · rear curtain")||
                    n.Contains("flat citadel production · west mid tower")||
                    n.Contains("flat citadel production · east mid tower")||
                    n.Contains("flat citadel production · corner tower")||
                    n.Contains("flat citadel production · gate foundation")||
                    n.Contains("flat citadel production · wall banner");
                if(!outer)continue;
                r.enabled=false;
                count++;
            }
            return count;
        }

        static void HideReservedPlotOccupants(Transform uplift)
        {
            // R4 / R5 are progression parcels. Early proof cottages are not allowed to consume them.
            foreach(var r in uplift.GetComponentsInChildren<Renderer>(true))
            {
                string n=Chain(r.transform);
                if(n.Contains("cottage west")||n.Contains("cottage east")||
                   n.Contains("rebuilt home"))
                    r.enabled=false;
            }
        }

        static void BuildProgressionReservations(Transform root)
        {
            ReservedParcels=0;
            var padMat=ValoriaKit.DetailedSurfaceMaterial(Earth,"earth",new Vector2(2.5f,2.5f),.92f);

            BuildReservedParcel(root,"R4 · Cantera future plot",new Vector3(-6.25f,.125f,3.20f),new Vector3(4.75f,.055f,3.85f),padMat);
            BuildReservedParcel(root,"R5 · Forja future plot",new Vector3( 6.25f,.125f,3.15f),new Vector3(4.55f,.055f,3.75f),padMat);
            BuildReservedParcel(root,"R6 · Hospital future plot",new Vector3( 2.70f,.125f,-4.35f),new Vector3(3.85f,.055f,3.25f),padMat);

            // Construction stakes communicate intentional vacancy while remaining trivially removable.
            foreach(var p in new[]{
                new Vector3(-8.15f,.17f,1.65f),new Vector3(-4.35f,.17f,1.65f),new Vector3(-8.15f,.17f,4.75f),new Vector3(-4.35f,.17f,4.75f),
                new Vector3( 4.45f,.17f,1.65f),new Vector3( 8.05f,.17f,1.65f),new Vector3( 4.45f,.17f,4.65f),new Vector3( 8.05f,.17f,4.65f),
                new Vector3( 1.15f,.17f,-5.65f),new Vector3( 4.25f,.17f,-5.65f),new Vector3(1.15f,.17f,-3.05f),new Vector3(4.25f,.17f,-3.05f)})
                BuildStake(root,p);
        }

        static void BuildReservedParcel(Transform root,string role,Vector3 p,Vector3 size,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="Valoria · Art Consolidation · "+role;
            go.transform.SetParent(root,true);
            go.transform.position=p;
            go.transform.localScale=size;
            var r=go.GetComponent<Renderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;
            var c=go.GetComponent<Collider>();if(c!=null)Object.DestroyImmediate(c);
            ReservedParcels++;
        }

        static void BuildStake(Transform root,Vector3 p)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name="Valoria · Art Consolidation · removable construction stake";
            go.transform.SetParent(root,true);
            go.transform.position=p;
            go.transform.localScale=new Vector3(.045f,.34f,.045f);
            var r=go.GetComponent<Renderer>();
            r.sharedMaterial=ValoriaKit.DetailedSurfaceMaterial(Wood,"wood",new Vector2(1.2f,1.2f),.95f);
            var c=go.GetComponent<Collider>();if(c!=null)Object.DestroyImmediate(c);
        }

        static void BuildWallContinuityBase(Transform root)
        {
            var mat=ValoriaKit.DetailedSurfaceMaterial(new Color(.61f,.59f,.55f,1f),"stone",new Vector2(2.25f,2.25f),1.0f);

            // Low continuous masonry establishes one defensive ring. Authored modules above it carry the silhouette.
            // Openings are intentional: main gate + west/east future expansion interfaces.
            AddWallBase(root,"front west base",new Vector3(-6.25f,.36f,-6.28f),new Vector3(7.05f,.70f,.54f),mat);
            AddWallBase(root,"front east base",new Vector3( 6.25f,.36f,-6.28f),new Vector3(7.05f,.70f,.54f),mat);

            AddWallBase(root,"west lower base",new Vector3(-9.46f,.36f,-1.45f),new Vector3(.54f,.70f,9.15f),mat);
            AddWallBase(root,"west upper base",new Vector3(-9.46f,.36f, 6.75f),new Vector3(.54f,.70f,5.10f),mat);
            AddWallBase(root,"east lower base",new Vector3( 9.46f,.36f,-1.45f),new Vector3(.54f,.70f,9.15f),mat);
            AddWallBase(root,"east upper base",new Vector3( 9.46f,.36f, 6.75f),new Vector3(.54f,.70f,5.10f),mat);

            AddWallBase(root,"rear base",new Vector3(0f,.34f,9.26f),new Vector3(18.45f,.66f,.54f),mat);
        }

        static void AddWallBase(Transform root,string role,Vector3 p,Vector3 size,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="Valoria · Art Consolidation · "+role;
            go.transform.SetParent(root,true);
            go.transform.position=p;
            go.transform.localScale=size;
            var r=go.GetComponent<Renderer>();r.sharedMaterial=mat;r.receiveShadows=true;
            var col=go.GetComponent<Collider>();if(col!=null)Object.DestroyImmediate(col);
        }

        static void BuildConsolidatedWall(Transform root)
        {
            ConsolidatedWallModules=0;
            var wall=Resources.Load<GameObject>("Valoria/Stone_Wall");
            var tower=Resources.Load<GameObject>("Valoria/Stone_Tower");
            var gate=Resources.Load<GameObject>("Valoria/Stone_Gate");
            if(wall==null||tower==null||gate==null)
                throw new InvalidOperationException("Consolidated wall requires canonical Stone_Wall / Stone_Tower / Stone_Gate.");

            // Hierarchy A: one credible main gatehouse. Tower pair is lower than the old uplift so it frames,
            // rather than competes with, the Hero Bastion.
            AddModule(root,gate,"main gatehouse",new Vector3(0f,.10f,-6.48f),4.45f,3.15f,0f,StoneLight);
            AddModule(root,tower,"main gate west tower",new Vector3(-3.10f,.09f,-6.18f),2.15f,3.02f,4f,Stone);
            AddModule(root,tower,"main gate east tower",new Vector3( 3.10f,.09f,-6.18f),2.08f,2.92f,-4f,Stone);
            AddBanner(root,new Vector3(-1.55f,2.20f,-6.56f),.86f);
            AddBanner(root,new Vector3( 1.55f,2.15f,-6.56f),.82f);

            // Hierarchy B: long quiet curtain groups. Each group is a wall + buttress pair rather than
            // a rhythm of identical wall/tower/wall/tower modules.
            AddCurtainGroup(root,"front west outer",new Vector3(-7.35f,.08f,-6.26f),4.60f,0f,false);
            AddCurtainGroup(root,"front east outer",new Vector3( 7.35f,.08f,-6.26f),4.60f,0f,true);

            AddCurtainGroup(root,"west lower",new Vector3(-9.45f,.08f,-3.15f),4.45f,90f,false);
            AddCurtainGroup(root,"west upper",new Vector3(-9.45f,.08f, 7.15f),3.65f,90f,true);
            AddCurtainGroup(root,"east lower",new Vector3( 9.45f,.08f,-3.15f),4.45f,90f,true);
            AddCurtainGroup(root,"east upper",new Vector3( 9.45f,.08f, 7.15f),3.65f,90f,false);

            // XW / XE progression seams: side gates are deliberate future extension interfaces.
            // Today they read as controlled postern/repair gates; later they can become district streets.
            AddModule(root,gate,"XW future expansion gate",new Vector3(-9.48f,.09f,3.65f),3.15f,2.45f,90f,Stone);
            AddModule(root,gate,"XE future expansion gate",new Vector3( 9.48f,.09f,3.65f),3.15f,2.45f,90f,Stone);

            // Rear ring stays deliberately low so it never becomes a skyline competitor.
            AddCurtainGroup(root,"rear west",new Vector3(-5.45f,.08f,9.25f),6.10f,0f,false,1.55f);
            AddCurtainGroup(root,"rear east",new Vector3( 5.45f,.08f,9.25f),6.10f,0f,true,1.55f);

            // Only two rear corner towers. Front corners are articulated by buttresses instead of four more towers.
            AddModule(root,tower,"rear west watchtower",new Vector3(-9.25f,.09f,9.05f),1.90f,2.70f,18f,StoneDark);
            AddModule(root,tower,"rear east watchtower",new Vector3( 9.25f,.09f,9.05f),1.85f,2.62f,-18f,StoneDark);

            AddGroundTransition(root,new Vector3(-4.65f,.07f,-6.48f),new Vector3(1.85f,.12f,1.05f));
            AddGroundTransition(root,new Vector3( 4.65f,.07f,-6.48f),new Vector3(1.85f,.12f,1.05f));
            AddGroundTransition(root,new Vector3(-9.48f,.07f,3.65f),new Vector3(1.05f,.12f,2.2f));
            AddGroundTransition(root,new Vector3( 9.48f,.07f,3.65f),new Vector3(1.05f,.12f,2.2f));
        }

        static void AddCurtainGroup(Transform root,string role,Vector3 p,float span,float yaw,bool mirror,float maxHeight=1.72f)
        {
            var wall=Resources.Load<GameObject>("Valoria/Stone_Wall");
            if(wall==null)return;
            AddModule(root,wall,role+" curtain",p,span,maxHeight,yaw,Stone);

            // Low pilaster modules break silhouette and repetition without introducing another tower.
            Vector3 axis=Mathf.Abs(Mathf.DeltaAngle(yaw,90f))<25f?Vector3.forward:Vector3.right;
            for(int i=-1;i<=1;i+=2)
            {
                var bp=p+axis*(span*.37f*i);
                AddModule(root,wall,role+" pilaster "+i,bp,.82f,Mathf.Min(1.34f,maxHeight*.86f),yaw,StoneDark);
            }
        }

        static void AddGroundTransition(Transform root,Vector3 p,Vector3 size)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="Valoria · Art Consolidation · wall ground transition";
            go.transform.SetParent(root,true);
            go.transform.position=p;
            go.transform.localScale=size;
            go.GetComponent<Renderer>().sharedMaterial=ValoriaKit.DetailedSurfaceMaterial(new Color(.48f,.44f,.36f,1f),"earth",new Vector2(2.0f,2.0f),.95f);
            var c=go.GetComponent<Collider>();if(c!=null)Object.DestroyImmediate(c);
        }

        static void BuildBastionArchitecturalInterface(Transform root)
        {
            BastionInterfaceModules=0;
            var wall=Resources.Load<GameObject>("Valoria/Stone_Wall");
            if(wall==null)return;

            // Split retaining masses occupy the rock-facing shoulders while the central stair remains open.
            var apronMat=ValoriaKit.DetailedSurfaceMaterial(new Color(.57f,.55f,.51f,1f),"stone",new Vector2(1.85f,1.85f),1.0f);
            foreach(float x in new[]{-2.55f,2.55f})
            {
                var apron=GameObject.CreatePrimitive(PrimitiveType.Cube);
                apron.name="Valoria · Art Consolidation · Bastion retaining mass";
                apron.transform.SetParent(root,true);
                apron.transform.position=new Vector3(x,.58f,4.95f);
                apron.transform.localScale=new Vector3(2.85f,1.08f,1.05f);
                apron.GetComponent<Renderer>().sharedMaterial=apronMat;
                var apronCol=apron.GetComponent<Collider>();if(apronCol!=null)Object.DestroyImmediate(apronCol);
                BastionInterfaceModules++;
            }

            AddBastionFacing(root,wall,"west retaining face",new Vector3(-2.70f,.14f,4.58f),3.35f,1.92f,0f);
            AddBastionFacing(root,wall,"east retaining face",new Vector3( 2.70f,.14f,4.58f),3.35f,1.92f,0f);
            AddBastionFacing(root,wall,"west return",new Vector3(-3.72f,.14f,6.15f),2.95f,1.55f,90f);
            AddBastionFacing(root,wall,"east return",new Vector3( 3.72f,.14f,6.15f),2.95f,1.55f,90f);

            // Flared stair and landing bridge plaza -> Bastion. Deliberately architectural, not geological.
            var stairMat=ValoriaKit.DetailedSurfaceMaterial(new Color(.62f,.60f,.56f,1f),"stone",new Vector2(1.7f,1.7f),1.0f);
            for(int i=0;i<8;i++)
            {
                float t=i/7f;
                float width=Mathf.Lerp(4.45f,3.35f,t);
                var step=GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name="Valoria · Art Consolidation · Bastion processional step "+i;
                step.transform.SetParent(root,true);
                step.transform.position=new Vector3(0,.18f+t*.70f,3.72f+t*1.42f);
                step.transform.localScale=new Vector3(width,.105f,.62f);
                step.GetComponent<Renderer>().sharedMaterial=stairMat;
                var col=step.GetComponent<Collider>();if(col!=null)Object.DestroyImmediate(col);
            }

            // Side plinths become visual anchors for future civic detail without occupying upper-city reserve.
            foreach(float x in new[]{-3.55f,3.55f})
            {
                var p=GameObject.CreatePrimitive(PrimitiveType.Cube);
                p.name="Valoria · Art Consolidation · Bastion stair plinth";
                p.transform.SetParent(root,true);
                p.transform.position=new Vector3(x,.42f,4.85f);
                p.transform.localScale=new Vector3(.72f,.76f,.82f);
                p.GetComponent<Renderer>().sharedMaterial=apronMat;
                var col=p.GetComponent<Collider>();if(col!=null)Object.DestroyImmediate(col);
                BastionInterfaceModules++;
            }
        }

        static void AddBastionFacing(Transform root,GameObject source,string role,Vector3 p,float span,float maxHeight,float yaw)
        {
            AddModule(root,source,"Bastion "+role,p,span,maxHeight,yaw,Stone);
            BastionInterfaceModules++;
        }

        static void BuildGroundEdgeIntegration(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            // Organic ground transitions only; no mature trees or permanent dressing enters reserved parcels.
            if(art.SlavicMoss!=null)
            {
                foreach(var s in new[]{
                    new Vector4(-8.6f,-5.85f, 12f,1.30f),new Vector4(-9.95f,-.35f, 76f,1.10f),
                    new Vector4(-8.85f, 8.35f,124f,1.20f),new Vector4( 8.80f, 8.30f,214f,1.20f),
                    new Vector4( 9.95f,-.25f,286f,1.10f),new Vector4( 8.55f,-5.82f,336f,1.30f)})
                {
                    var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Art Consolidation · wall meadow seam",
                        art.SlavicMoss,new Vector3(s.x,.08f,s.y),s.w,.09f,Quaternion.Euler(0,s.z,0),new Color(.48f,.58f,.39f,1f));
                    if(go==null)continue;
                    go.transform.SetParent(root,true);
                    foreach(var col in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
                }
            }
            if(art.SlavicMudFlat!=null)
            {
                foreach(var s in new[]{
                    new Vector4(-4.25f,-6.28f,14f,1.15f),new Vector4(4.25f,-6.28f,194f,1.15f),
                    new Vector4(-9.38f,3.65f,86f,1.05f),new Vector4(9.38f,3.65f,266f,1.05f)})
                {
                    var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Art Consolidation · gate earth seam",
                        art.SlavicMudFlat,new Vector3(s.x,.075f,s.y),s.w,.08f,Quaternion.Euler(0,s.z,0),new Color(.55f,.47f,.34f,1f));
                    if(go==null)continue;
                    go.transform.SetParent(root,true);
                    foreach(var col in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
                }
            }
        }

        static void BuildPerimeterLife(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return;

            if(art.SlavicBush!=null)
            {
                foreach(var s in new[]{
                    new Vector4(-10.75f,-5.15f, 18f,.78f),new Vector4(-11.10f,-2.55f, 56f,.70f),
                    new Vector4(-10.70f, 7.10f,104f,.74f),new Vector4(-7.35f,10.25f,142f,.72f),
                    new Vector4( 7.25f,10.20f,218f,.72f),new Vector4(10.70f,7.10f,256f,.74f),
                    new Vector4(11.10f,-2.50f,302f,.70f),new Vector4(10.75f,-5.05f,338f,.78f)})
                {
                    var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Art Consolidation · perimeter low vegetation",
                        art.SlavicBush,new Vector3(s.x,.07f,s.y),s.w,.58f,Quaternion.Euler(0,s.z,0),new Color(.50f,.62f,.42f,1f));
                    if(go==null)continue;
                    go.transform.SetParent(root,true);
                    foreach(var col in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
                }
            }

            if(art.SlavicBoulder!=null)
            {
                foreach(var s in new[]{
                    new Vector4(-10.60f,-6.15f,24f,.82f),new Vector4(-10.55f,8.80f,78f,.68f),
                    new Vector4(10.55f,8.75f,206f,.68f),new Vector4(10.60f,-6.10f,214f,.82f)})
                {
                    var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Art Consolidation · perimeter low boulder",
                        art.SlavicBoulder,new Vector3(s.x,.05f,s.y),s.w,.52f,Quaternion.Euler(0,s.z,0),new Color(.52f,.52f,.48f,1f));
                    if(go==null)continue;
                    go.transform.SetParent(root,true);
                    foreach(var col in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(col);
                }
            }
        }

        static void ApplyAtmosphere()
        {
            // Camera-contained haze softens the empty far field without reintroducing a panoramic background.
            RenderSettings.fog=true;
            RenderSettings.fogMode=FogMode.Linear;
            RenderSettings.fogColor=new Color(.48f,.53f,.54f,1f);
            RenderSettings.fogStartDistance=43f;
            RenderSettings.fogEndDistance=68f;
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.55f,.58f,.58f,1f);
            RenderSettings.ambientEquatorColor=new Color(.43f,.44f,.41f,1f);
            RenderSettings.ambientGroundColor=new Color(.28f,.29f,.25f,1f);
            RenderSettings.ambientIntensity=.88f;
        }

        static int ConsolidateMaterialLanguage(Transform uplift,Transform additions)
        {
            int count=0;
            var roots=new[]{uplift,additions};
            foreach(var rr in roots)
            foreach(var r in rr.GetComponentsInChildren<Renderer>(true))
            {
                if(r==null||!r.enabled)continue;
                string chain=Chain(r.transform);
                bool stone=chain.Contains("bastion")||chain.Contains("wall")||chain.Contains("gate")||chain.Contains("tower")||chain.Contains("plaza")||chain.Contains("road");
                bool saw=chain.Contains("aserradero");
                bool barracks=chain.Contains("cuartel");
                bool granary=chain.Contains("granero");
                bool functional=saw||barracks||granary;
                if(!stone&&!functional)continue;

                var mats=r.sharedMaterials;
                var next=new Material[mats.Length];
                bool changed=false;
                for(int i=0;i<mats.Length;i++)
                {
                    var old=mats[i];
                    if(old==null){next[i]=null;continue;}
                    var m=new Material(old){name="Valoria Consolidated · "+old.name};
                    Color target=stone?Stone:(saw?new Color(.76f,.66f,.52f,1f):(barracks?new Color(.65f,.64f,.60f,1f):new Color(.72f,.66f,.51f,1f)));
                    foreach(string prop in new[]{"_BaseColor","_Color","_BaseColorFactor","_Primary_Color"})
                    {
                        if(!m.HasProperty(prop))continue;
                        try
                        {
                            var original=m.GetColor(prop);
                            var blended=Color.Lerp(original,target,stone?.20f:.26f);
                            blended.a=original.a;
                            m.SetColor(prop,blended);
                        }catch{}
                    }
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))
                    {
                        float current=.08f;
                        try{current=m.GetFloat("_Smoothness");}catch{}
                        m.SetFloat("_Smoothness",Mathf.Clamp(current,.025f,.14f));
                    }
                    if(m.HasProperty("_BumpScale"))
                    {
                        float current=1f;
                        try{current=m.GetFloat("_BumpScale");}catch{}
                        m.SetFloat("_BumpScale",Mathf.Clamp(current,.65f,1.15f));
                    }
                    next[i]=m;changed=true;
                }
                if(changed){r.sharedMaterials=next;count++;}
            }
            return count;
        }

        static void AddModule(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw,Color tint)
        {
            var go=ValoriaKit.BenchmarkPieceModulated("Valoria · Art Consolidation · "+role,source,ground,footprint,maxHeight,
                Quaternion.Euler(0,yaw,0),tint);
            if(go==null)throw new InvalidOperationException("Failed wall/interface module: "+role);
            go.transform.SetParent(root,true);
            NormalizeImportedStone(go,tint);
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            ConsolidatedWallModules++;
        }

        static void NormalizeImportedStone(GameObject go,Color tint)
        {
            var lit=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            if(lit==null)return;

            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=renderer.sharedMaterials;
                var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    var old=src[i];
                    if(old==null){dst[i]=null;continue;}

                    Texture baseMap=null,normal=null;
                    Vector2 scale=Vector2.one,offset=Vector2.zero;
                    foreach(string prop in new[]{"_Texture","_BaseMap","_MainTex","_BaseColorTexture","baseColorTexture","_Albedo"})
                    {
                        if(!old.HasProperty(prop)||old.GetTexture(prop)==null)continue;
                        baseMap=old.GetTexture(prop);
                        try{scale=old.GetTextureScale(prop);offset=old.GetTextureOffset(prop);}catch{}
                        break;
                    }
                    foreach(string prop in new[]{"_BumpMap","_NormalMap","normalTexture"})
                        if(old.HasProperty(prop)&&old.GetTexture(prop)!=null){normal=old.GetTexture(prop);break;}

                    var m=new Material(lit){name="Valoria Art Consolidation stone · "+old.name};
                    if(baseMap!=null)
                    {
                        if(m.HasProperty("_BaseMap")){m.SetTexture("_BaseMap",baseMap);m.SetTextureScale("_BaseMap",scale);m.SetTextureOffset("_BaseMap",offset);}
                        if(m.HasProperty("_MainTex")){m.SetTexture("_MainTex",baseMap);m.SetTextureScale("_MainTex",scale);m.SetTextureOffset("_MainTex",offset);}
                    }
                    if(normal!=null&&m.HasProperty("_BumpMap"))
                    {
                        m.SetTexture("_BumpMap",normal);
                        m.SetFloat("_BumpScale",.92f);
                        m.EnableKeyword("_NORMALMAP");
                    }

                    var baseColor=Color.Lerp(new Color(.73f,.70f,.64f,1f),tint,.22f);
                    baseColor.a=1f;
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",baseColor);
                    if(m.HasProperty("_Color"))m.SetColor("_Color",baseColor);
                    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",0f);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.035f);
                    if(m.HasProperty("_SpecularHighlights"))m.SetFloat("_SpecularHighlights",1f);
                    if(m.HasProperty("_EnvironmentReflections"))m.SetFloat("_EnvironmentReflections",1f);
                    dst[i]=m;
                }
                renderer.sharedMaterials=dst;
            }
        }

        static void AddBanner(Transform root,Vector3 p,float scale)
        {
            var canonical=root.parent;
            int before=canonical!=null?canonical.childCount:0;
            ValoriaKit.Banner("Valoria · Art Consolidation · wall heraldry",p,new Vector3(.42f*scale,1.22f*scale,.06f),Blue);
            if(canonical==null)return;
            for(int i=canonical.childCount-1;i>=0;i--)
            {
                var child=canonical.GetChild(i);
                if(child==root)continue;
                if(child.name.Contains("Art Consolidation")&&child.name.Contains("heraldry"))
                    child.SetParent(root,true);
            }
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }
    }
}

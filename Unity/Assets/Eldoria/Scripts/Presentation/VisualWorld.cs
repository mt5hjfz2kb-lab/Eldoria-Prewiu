using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.Presentation
{
    // Original procedural study: provisional geometry/materials, no inherited web art or unlicensed assets.
    public static class VisualWorld
    {
        static readonly Color Stone = new Color(.34f,.36f,.37f), Deep = new Color(.10f,.12f,.13f);
        static readonly Color WarmStone = new Color(.46f,.43f,.37f), Timber = new Color(.24f,.16f,.11f);
        static readonly Color Earth = new Color(.22f,.19f,.15f), Pine = new Color(.10f,.18f,.14f);
        static readonly Color Amber = new Color(.96f,.53f,.22f), Violet = new Color(.57f,.19f,.91f);
        public static void Create(bool city, PlayerState state)
        {
            // Keep the fixed gameplay camera singular even if PlayMode/test scene transitions
            // invoke world creation more than once during the same session.
            foreach(var oldCamera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if(oldCamera.gameObject.name=="Isometric camera")
                    UnityEngine.Object.DestroyImmediate(oldCamera.gameObject);

            RenderSettings.ambientMode = AmbientMode.Flat;
            // First OWNER I-II human pass showed that the certified neutral-overcast rig
            // reads materially too dark in motion. Lift ambient/fog values without
            // flattening the stone/timber hierarchy or removing the corruption contrast.
            RenderSettings.ambientLight = city?new Color(.82f,.82f,.80f):new Color(.94f,.92f,.86f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = city?new Color(.69f,.69f,.67f):new Color(.68f,.68f,.63f);
            RenderSettings.fogStartDistance=city?30:36; RenderSettings.fogEndDistance=city?68:110;
            var cameraGo = new GameObject("Isometric camera");
            var camera = cameraGo.AddComponent<Camera>(); camera.orthographic=true;
            camera.orthographicSize = city ? 10.2f : 14;
            camera.backgroundColor = RenderSettings.fogColor; camera.clearFlags=CameraClearFlags.SolidColor;
            cameraGo.tag="MainCamera";
            cameraGo.transform.position = city ? new Vector3(18.2f,14.6f,-25.8f) : new Vector3(20,24,-21);
            cameraGo.transform.LookAt(city ? new Vector3(0,3.15f,5.8f) : new Vector3(0,0,1));
            var sun = new GameObject("Valoria · amber dusk").AddComponent<Light>();
            sun.type=LightType.Directional; sun.color=city?new Color(1.0f,.97f,.90f):new Color(1.0f,.95f,.86f);
            sun.intensity=city?1.22f:2.30f;
            sun.transform.rotation=Quaternion.Euler(city?52f:48f,city?-25f:-32f,0); sun.shadows=LightShadows.Soft; sun.shadowStrength=city?.43f:.46f;
            var worldGround=Box("World ground",new Vector3(0,-.7f,city?4:0),city?new Vector3(66,1.2f,62):new Vector3(34,1.2f,30),
                city?new Color(.285f,.265f,.215f):Earth);
            if(!city) worldGround.GetComponent<Renderer>().enabled=false;
            if(city)
            {
                // Art Pass 1: preserve the full collision/support envelope but remove the visible
                // board-like slab edge from the authored camera. The visible valley floor is a
                // shallow irregular sheet, so zoom 19 no longer exposes a giant rectangular plinth.
                worldGround.GetComponent<Renderer>().enabled=false;
                IrregularGround("Valoria · valley floor",new Vector3(0,-.08f,4.0f),200f,180f,new Color(.305f,.295f,.265f));
            }
            // Frontier composition is authored inside Frontier(). Do not surround it with
            // box-cliff or box-road primitives: the official owner captures showed those
            // debug-like masses dominating the world wedge.
            if(city)
            {
                // Keep nearby terrain controlled, but give the valley a real distant horizon.
                // Preserve the authored mountain atlas and mute it into the fog instead of flattening it to clay.
                ValoriaKit.TerrainPieceTinted("SM_Mountains_11","Valoria · distant mountain west",
                    new Vector3(-15.5f,-2.2f,26.5f),9.4f,5.8f,Quaternion.Euler(0,18f,0),
                    new Color(.31f,.34f,.34f,1f));
                ValoriaKit.TerrainPieceTinted("SM_Mountains_11","Valoria · distant mountain centre",
                    new Vector3(-1.0f,-2.5f,28.0f),10.2f,6.2f,Quaternion.Euler(0,-7f,0),
                    new Color(.29f,.32f,.33f,1f));
                ValoriaKit.TerrainPieceTinted("SM_Mountains_11","Valoria · distant mountain east",
                    new Vector3(15.0f,-2.3f,26.8f),9.0f,5.6f,Quaternion.Euler(0,-24f,0),
                    new Color(.31f,.33f,.33f,1f));
                ValoriaKit.RockCluster("Ruined imperial arch fall",new Vector3(-8.1f,-.12f,17.5f),.82f,7);
                // A restrained rear ridge breaks the valley edge while preserving a clear Bastion silhouette.
                for(int i=0;i<16;i++)
                {
                    float x=-17.0f+i*2.25f;
                    if(Mathf.Abs(x)<4.2f)continue;
                    float z=15.0f+(i%3)*1.25f;
                    ValoriaKit.PineTree("Valoria · rear pine",new Vector3(x,0,z),.58f+(i%4)*.07f);
                    if(i%3==0)ValoriaKit.RockCluster("Valoria · rear ridge rock",new Vector3(x+.7f,-.10f,z+.6f),.72f,5);
                }
                PlayableDistrictSkeleton(state);
            }
            else Frontier(state);
            if(city)ValoriaScar(new Vector3(14.8f,.05f,15.6f));
            // Frontier() owns its restrained Breach scar. Do not stack the old neon shard portal
            // over it; that read as a prototype prop in the official I-II captures.

            // Procedural primitives are created at the origin and then positioned/scaled.
            // Force the physics world to ingest those transforms before any same-frame
            // world click or PlayMode gate queries Collider.bounds / raycasts.
            Physics.SyncTransforms();
        }
        static void PlayableDistrictSkeleton(PlayerState state)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            var rescuedResidential=Resources.Load<GameObject>("Valoria/Rescued/ResidentialTerraceRock");
            var rescuedSeam=Resources.Load<GameObject>("Valoria/Rescued/RockTerrainSeamFiller");
            // VALORIA PLAYABLE DISTRICT v1 — ART PASS 1
            // Frozen topology: continuous terrain -> L0 -> street -> vertical link -> L1 -> plots -> buildings.
            // Visual treatment may overlap/bury supports, but it must never redefine circulation or hotspot footprints.
            var ground=ValoriaKit.Block("VPD · continuous terrain",new Vector3(0,-.18f,1.0f),
                new Vector3(22f,.50f,25f),new Color(.29f,.27f,.22f));
            ground.GetComponent<Renderer>().enabled=false;

            // Organic core mountain/ground skin over the frozen collision base.
            IrregularGround("VPD · inhabited mountain floor",new Vector3(0,.075f,.5f),23.8f,26.4f,new Color(.30f,.285f,.245f));
            IrregularGround("VPD · lower terrace earth",new Vector3(0,.105f,-3.5f),18.4f,11.8f,new Color(.355f,.325f,.265f));
            IrregularGround("VPD · upper terrace earth",new Vector3(0,2.405f,7.35f),13.9f,8.5f,new Color(.345f,.325f,.285f));

            // Planta 0 collision/interaction floor remains exact, but its rectangular renderers are hidden.
            var l0=ValoriaKit.Block("VPD · L0 civic floor",new Vector3(0,.12f,-3.2f),
                new Vector3(17.5f,.28f,10.5f),new Color(.37f,.34f,.28f));
            l0.GetComponent<Renderer>().enabled=false;
            var street=ValoriaKit.Block("VPD · L0 main street",new Vector3(0,.30f,-3.0f),
                new Vector3(3.4f,.16f,10.8f),new Color(.50f,.47f,.40f));
            street.GetComponent<Renderer>().enabled=false;
            var westPlot=ValoriaKit.Block("VPD · L0 west plot",new Vector3(-7.0f,.31f,-2.8f),
                new Vector3(5.1f,.18f,5.2f),new Color(.33f,.30f,.24f));
            westPlot.GetComponent<Renderer>().enabled=false;
            var eastPlot=ValoriaKit.Block("VPD · L0 east plot",new Vector3(7.0f,.31f,-4.0f),
                new Vector3(5.1f,.18f,5.2f),new Color(.33f,.30f,.24f));
            eastPlot.GetComponent<Renderer>().enabled=false;
            var apron=ValoriaKit.Block("VPD · lower entry apron",new Vector3(0,.30f,-8.35f),
                new Vector3(6.6f,.14f,2.0f),new Color(.43f,.40f,.34f));
            apron.GetComponent<Renderer>().enabled=false;

            // Ground Kit v1 current-main validation checkpoint.
            // Ground Kit v1: visual-only reusable skins over the frozen certified circulation.
            // Gameplay floors/hotspots underneath remain authoritative.
            ValoriaGroundKit.StreetStraight("VPD · GroundKit main street",
                new Vector3(0,.405f,-3.05f),10.55f,3.18f,0f);
            ValoriaGroundKit.StreetBlendWidening("VPD · GroundKit entry widening",
                new Vector3(0,.407f,-8.25f),6.65f,2.35f,0f);

            // Standard functional-plot terrace skins use the same reusable ground language.
            ValoriaGroundKit.TerraceFloor("VPD · GroundKit west workshop terrace",
                new Vector3(-7.0f,.367f,-2.8f),5.65f,5.8f,-1.2f);
            ValoriaGroundKit.TerraceFloor("VPD · GroundKit east military terrace",
                new Vector3(7.0f,.367f,-4.0f),5.7f,5.75f,1.0f);
            ValoriaGroundKit.GroundSeam("VPD · GroundKit west plot seam",
                new Vector3(-4.45f,.355f,-2.15f),1.65f,4.4f,5f);
            ValoriaGroundKit.GroundSeam("VPD · GroundKit east plot seam",
                new Vector3(4.45f,.355f,-3.35f),1.65f,4.4f,-6f);

            // Real continuous 0 -> 1 connection: twelve visible treads, no fused architectural dependency.
            const int steps=12;
            const float rise=.18f;
            const float depth=.52f;
            for(int i=0;i<steps;i++)
            {
                float y=.39f+i*rise;
                float z=.15f+i*depth;
                float w=3.32f-(i%4==0?.10f:0f);
                var tread=ValoriaKit.Block("VPD · vertical stair "+(i+1),new Vector3((i%3==0?.025f:0f),y,z),
                    new Vector3(w,.18f,depth+.07f),new Color(.47f,.44f,.37f));
                if(i%4==1)tread.transform.rotation=Quaternion.Euler(0,.7f,0);
            }

            // Stair cheeks are deliberately low and intermittent: they integrate the route into rock
            // without hiding the twelve-tread read from any official zoom.
            for(int i=0;i<6;i++)
            {
                float z=.55f+i*1.0f;
                float y=.34f+i*.34f;
                float side=(i%2==0?-1f:1f);
                ValoriaKit.RockCluster("VPD · stair shoulder rock",new Vector3(side*2.12f,y-.16f,z),.52f,4);
            }

            float l1=2.55f;
            var landing=ValoriaKit.Block("VPD · L1 landing",new Vector3(0,l1,6.7f),
                new Vector3(7.0f,.30f,4.2f),new Color(.42f,.39f,.33f));
            landing.GetComponent<Renderer>().enabled=false;
            var l1West=ValoriaKit.Block("VPD · L1 west plot",new Vector3(-5.2f,l1,7.1f),
                new Vector3(4.2f,.28f,4.8f),new Color(.34f,.31f,.27f));
            l1West.GetComponent<Renderer>().enabled=false;
            var l1East=ValoriaKit.Block("VPD · L1 east plot",new Vector3(5.2f,l1,7.1f),
                new Vector3(4.2f,.28f,4.8f),new Color(.34f,.31f,.27f));
            l1East.GetComponent<Renderer>().enabled=false;
            ValoriaGroundKit.StreetBlendWidening("VPD · GroundKit L1 landing",
                new Vector3(0,l1+.17f,6.65f),7.55f,4.9f,0f);
            ValoriaGroundKit.TerraceFloor("VPD · GroundKit L1 west terrace",
                new Vector3(-5.2f,l1+.16f,7.1f),4.75f,5.25f,-1.5f);
            ValoriaGroundKit.TerraceFloor("VPD · GroundKit L1 east terrace",
                new Vector3(5.2f,l1+.16f,7.1f),4.75f,5.25f,1.5f);

            // Frozen support volumes stay as invisible structural/collision mass. Visible containment
            // is rebuilt as rock + masonry fragments so Planta 1 belongs to the same inhabited mountain.
            var supportWest=ValoriaKit.Block("VPD · L1 support west",new Vector3(-5.2f,1.20f,7.2f),
                new Vector3(4.3f,2.35f,4.9f),new Color(.25f,.25f,.23f));
            var supportCentre=ValoriaKit.Block("VPD · L1 support centre",new Vector3(0,1.20f,7.0f),
                new Vector3(7.2f,2.35f,4.4f),new Color(.26f,.26f,.24f));
            var supportEast=ValoriaKit.Block("VPD · L1 support east",new Vector3(5.2f,1.20f,7.2f),
                new Vector3(4.3f,2.35f,4.9f),new Color(.25f,.25f,.23f));
            supportWest.GetComponent<Renderer>().enabled=false;
            supportCentre.GetComponent<Renderer>().enabled=false;
            supportEast.GetComponent<Renderer>().enabled=false;

            foreach(var p in new[]{
                new Vector3(-7.0f,.36f,5.2f),new Vector3(-5.5f,.62f,4.9f),new Vector3(-3.8f,.45f,5.0f),
                new Vector3(3.8f,.45f,5.0f),new Vector3(5.5f,.62f,4.9f),new Vector3(7.0f,.36f,5.2f),
                new Vector3(-7.2f,.45f,8.8f),new Vector3(-4.9f,.58f,9.2f),
                new Vector3(4.9f,.58f,9.2f),new Vector3(7.2f,.45f,8.8f)})
            {
                var authored=ValoriaKit.BenchmarkPieceTinted("VPD · authored retaining rock",
                    art!=null?art.SlavicFlatRock:null,p,2.25f,1.20f,
                    Quaternion.Euler(0,(p.x>0?27f:-29f)+(p.z>7f?18f:0f),0),
                    new Color(.30f,.305f,.285f,1f));
                if(authored==null)ValoriaKit.RockCluster("VPD · inhabited retaining rock",p,.78f,5);
            }
            ValoriaGroundKit.RetainingEdge("VPD · GroundKit L1 retaining edge",
                new Vector3(0,.44f,4.88f),10.6f,.78f,0f);
            for(int i=0;i<5;i++)
            {
                float x=-4.4f+i*2.2f;
                ValoriaKit.BenchmarkPieceTinted("VPD · retaining stone face",
                    art!=null?art.SlavicStoneFence:null,
                    new Vector3(x,1.58f,4.67f+(i%2)*.08f),1.82f,1.30f,
                    Quaternion.Euler(0,(i%2==0?2f:-2f),0),ValoriaKit.OldStone*.90f);
            }

            // ART PASS 2 — dedicated architecture inside the frozen plot envelopes.
            // Visual architecture stays separate from the certified interaction volumes below.
            ValoriaKit.SawmillArchitecture("Aserradero",new Vector3(-7.0f,.40f,-2.8f),state.SawmillLevel>0,Glow);
            ValoriaKit.BenchmarkPieceTinted("VPD · rescued seam west",rescuedSeam,
                new Vector3(-6.15f,.20f,-2.55f),3.10f,1.65f,Quaternion.Euler(0,28f,0),
                new Color(.18f,.19f,.18f,1f));
            TagVisibleHotspots("Aserradero","sawmill");
            var mill=ValoriaKit.Block("Aserradero · target",new Vector3(-6.55f,1.68f,-3.75f),
                new Vector3(3.75f,2.25f,1.15f),new Color(.2f,.2f,.2f));
            mill.AddComponent<WorldHotspot>().Id="sawmill"; mill.GetComponent<Renderer>().enabled=false;

            ValoriaKit.BarracksArchitecture("Cuartel",new Vector3(7.0f,.40f,-4.0f),state.BarracksLevel>0,Glow);
            ValoriaKit.BenchmarkPieceTinted("VPD · rescued seam east",rescuedSeam,
                new Vector3(5.85f,.20f,-3.75f),2.95f,1.55f,Quaternion.Euler(0,205f,0),
                new Color(.18f,.19f,.18f,1f));
            TagVisibleHotspots("Cuartel","barracks");
            var barracks=ValoriaKit.Block("Cuartel · target",new Vector3(7.55f,1.68f,-5.05f),
                new Vector3(3.85f,2.30f,1.15f),new Color(.2f,.2f,.2f));
            barracks.AddComponent<WorldHotspot>().Id="barracks";
            barracks.GetComponent<Renderer>().enabled=false;
            // Canonical progression contract: the Cuartel interaction is a Bastion II unlock.
            // Create the target already in its authoritative state so no frame can leak a clickable
            // future building before ValoriaProgressionVisualGuard performs its periodic refresh.
            var barracksTargetCollider=barracks.GetComponent<Collider>();
            if(barracksTargetCollider!=null)barracksTargetCollider.enabled=state.BastionLevel>=2;

            ValoriaKit.BastionCore("Bastion",new Vector3(0,l1+.45f,7.25f),Glow);
            TagVisibleHotspots("Bastion","bastion");
            var bastion=ValoriaKit.Block("Bastion · target",new Vector3(.75f,l1+2.45f,5.95f),
                new Vector3(5.4f,4.7f,1.35f),new Color(.2f,.2f,.2f));
            bastion.AddComponent<WorldHotspot>().Id="bastion"; bastion.GetComponent<Renderer>().enabled=false;

            // Civil/economic upper plot: promote rescued certified geometry as visual-only art.
            // Gameplay topology remains the invisible certified plot and independent route/hotspots.
            ValoriaKit.BenchmarkPieceTinted("VPD · rescued seam residential",rescuedSeam,
                new Vector3(-6.05f,l1+.15f,7.15f),3.55f,1.75f,Quaternion.Euler(0,102f,0),
                new Color(.18f,.19f,.18f,1f));
            var rescuedCivil=ValoriaKit.BenchmarkPiece("VPD · rescued upper civil residence",rescuedResidential,
                new Vector3(-6.05f,l1+.34f,7.15f),4.20f,4.45f,Quaternion.Euler(0,-12f,0));
            if(rescuedCivil!=null)
            {
                StyleRescuedResidential(rescuedCivil);
                Glow("VPD · rescued residence hearth",new Vector3(-6.15f,l1+1.55f,6.15f),Amber,.78f,2.6f);
            }
            else ValoriaKit.House("VPD · upper civil house fallback",new Vector3(-5.15f,l1+.34f,7.15f),
                new Vector3(2.7f,1.35f,2.4f),true,Glow);
            ValoriaKit.House("VPD · upper dwelling",new Vector3(5.15f,l1+.34f,7.15f),
                new Vector3(2.55f,1.30f,2.25f),true,Glow);

            // Long-term master envelope remains intact and continues to define the panning future footprint.
            MasterEnvelopeGraybox(l1,art,rescuedSeam,state.BastionLevel>=3);

            var gate=ValoriaKit.Block("Puerta · ir al mundo",new Vector3(0,1.05f,-9.25f),
                new Vector3(3.0f,2.1f,.50f),ValoriaKit.Timber);
            gate.AddComponent<WorldHotspot>().Id="gate"; gate.GetComponent<Renderer>().enabled=false;

            // Authored rock band turns the near camera-facing earth cut into a mountain edge.
            for(int i=0;i<8;i++)
            {
                float x=-10.2f+i*2.9f;
                var p=new Vector3(x,-.18f,-9.1f+(i%2)*.32f);
                var authored=ValoriaKit.BenchmarkPieceTinted("VPD · lower cliff authored rock",
                    art!=null?art.SlavicFlatRock:null,p,2.85f,1.45f,
                    Quaternion.Euler(0,12f+i*31f,0),new Color(.295f,.29f,.265f,1f));
                if(authored==null)ValoriaKit.RockCluster("VPD · lower cliff fallback",p,.88f,5);
            }
            foreach(var p in new[]{new Vector3(-10.4f,-.10f,-7.5f),new Vector3(10.1f,-.08f,-7.25f)})
                ValoriaKit.BenchmarkPieceTinted("VPD · lower cliff boulder",
                    art!=null?art.SlavicBoulder:null,p,2.45f,1.65f,
                    Quaternion.Euler(0,p.x>0?28f:-24f,0),ValoriaKit.OldStone*.84f);

            // Mountain seams frame, rather than define, circulation.
            foreach(var p in new[]{
                new Vector3(-9.0f,-.10f,5.2f),new Vector3(9.0f,-.10f,5.6f),
                new Vector3(-9.3f,-.08f,-6.3f),new Vector3(9.4f,-.08f,-6.1f),
                new Vector3(-8.7f,-.05f,9.6f),new Vector3(8.8f,-.05f,9.4f)})
                ValoriaKit.RockCluster("VPD · terrain seam",p,.80f,5);
            for(int i=0;i<8;i++)
            {
                float x=(i%2==0?-1f:1f)*(9.2f+(i%3)*.55f);
                float z=-6.6f+i*2.35f;
                ValoriaKit.PineTree("VPD · edge pine",new Vector3(x,.02f,z),.58f+(i%3)*.06f);
            }
            Hero(new Vector3(-1.4f,.34f,-5.0f),1.0f);
            int visibleArchers=state.BastionLevel>=2?4:3;
            for(int i=0;i<visibleArchers;i++) Archer(new Vector3(2.0f+(i%2)*.62f,.34f,-4.5f+(i/2)*.62f));
            Glow("VPD · Bastion warmth",new Vector3(0,l1+3.4f,6.2f),Amber,1.35f,6.0f);

            // Apply the canonical visual/interaction progression before the first rendered frame.
            // The persistent guard will keep it synchronized afterwards, but initial creation must
            // already be correct so future buildings never flash or accept clicks early.
            ValoriaProgressionVisualGuard.Apply(state);
        }

        static void MasterEnvelopeGraybox(float l1,ValoriaExternalAssetLibrary art,GameObject rescuedSeam,bool showGranary)
        {
            var reserve=new Color(.255f,.245f,.215f);
            var route=new Color(.39f,.37f,.32f);
            var upper=new Color(.30f,.29f,.27f);

            // The expansion envelope remains physically reserved, but Art Pass 1 no longer exposes
            // the proof slabs as final-looking rectangular boards. Their colliders/names/positions stay exact.
            var west=ValoriaKit.Block("VPD · master west district",new Vector3(-16.0f,.10f,1.4f),
                new Vector3(10.0f,.22f,15.0f),reserve);
            var westRoute=ValoriaKit.Block("VPD · master west route",new Vector3(-11.1f,.27f,-1.2f),
                new Vector3(7.0f,.14f,2.4f),route);
            var east=ValoriaKit.Block("VPD · master east district",new Vector3(16.0f,.10f,1.8f),
                new Vector3(10.0f,.22f,15.0f),reserve);
            var eastRoute=ValoriaKit.Block("VPD · master east route",new Vector3(11.1f,.27f,-1.0f),
                new Vector3(7.0f,.14f,2.4f),route);
            var civic=ValoriaKit.Block("VPD · master upper civic reserve",new Vector3(0,l1-.08f,15.4f),
                new Vector3(13.5f,.22f,7.0f),upper);
            var civicLink=ValoriaKit.Block("VPD · master upper civic link",new Vector3(0,l1+.02f,11.2f),
                new Vector3(4.0f,.14f,4.0f),route);
            var future=ValoriaKit.Block("VPD · master future reserve",new Vector3(0,.08f,24.0f),
                new Vector3(19.0f,.20f,7.0f),reserve*.92f);
            var westApron=ValoriaKit.Block("VPD · master west terrain apron",new Vector3(-24.0f,-.15f,4.0f),
                new Vector3(7.0f,.38f,25.0f),new Color(.27f,.255f,.22f));
            var eastApron=ValoriaKit.Block("VPD · master east terrain apron",new Vector3(24.0f,-.15f,4.0f),
                new Vector3(7.0f,.38f,25.0f),new Color(.27f,.255f,.22f));

            foreach(var slab in new[]{west,westRoute,east,eastRoute,civic,civicLink,future,westApron,eastApron})
                slab.GetComponent<Renderer>().enabled=false;

            // Organic visual skins preserve the same planning envelope while reading as one valley/mountain.
            IrregularGround("VPD · west expansion terrain",new Vector3(-16.0f,.23f,1.4f),11.2f,16.6f,new Color(.285f,.272f,.235f));
            IrregularGround("VPD · east expansion terrain",new Vector3(16.0f,.23f,1.8f),11.2f,16.6f,new Color(.285f,.272f,.235f));
            IrregularGround("VPD · upper civic mountain shelf",new Vector3(0,l1+.05f,15.4f),14.8f,8.0f,new Color(.315f,.302f,.275f));
            IrregularGround("VPD · future valley shelf",new Vector3(0,.18f,24.0f),20.5f,8.2f,new Color(.275f,.265f,.235f));
            IrregularGround("VPD · west authored apron",new Vector3(-24.0f,.02f,4.0f),8.2f,26.2f,new Color(.275f,.262f,.225f));
            IrregularGround("VPD · east authored apron",new Vector3(24.0f,.02f,4.0f),8.2f,26.2f,new Color(.275f,.262f,.225f));

            // Expansion routes stay legible but avoid the single rectangular-strip silhouette.
            for(int side=-1;side<=1;side+=2)
            {
                for(int i=0;i<5;i++)
                {
                    float x=side*(8.3f+i*1.45f);
                    float z=-1.1f+(i%2==0?-.12f:.12f);
                    var piece=ValoriaKit.Block("VPD · future route stone",new Vector3(x,.335f,z),
                        new Vector3(1.72f,.075f,2.08f),new Color(.39f,.37f,.32f));
                    piece.transform.rotation=Quaternion.Euler(0,side*(i%2==0?3f:-2f),0);
                }
            }
            for(int i=0;i<4;i++)
            {
                float z=10.3f+i*1.0f;
                var piece=ValoriaKit.Block("VPD · civic approach stone",new Vector3(0,l1+.10f,z),
                    new Vector3(3.65f,.075f,1.18f),new Color(.39f,.37f,.32f));
                piece.transform.rotation=Quaternion.Euler(0,(i%2==0?1.5f:-1.5f),0);
            }

            // FIRST EXPANSION PRODUCTION SLICE — west rebuilders quarter.
            // This is visual city growth inside the already-reserved Master Envelope. It does not
            // create new gameplay requirements or move the certified district/camera topology.
            IrregularGround("VPD · west rebuilders terrace",new Vector3(-14.7f,.34f,1.25f),8.7f,12.2f,new Color(.315f,.292f,.245f));
            IrregularGround("VPD · west rebuilders upper shelf",new Vector3(-15.9f,1.18f,5.55f),6.1f,4.7f,new Color(.325f,.305f,.265f));

            // Stone path fragments visually continue the Aserradero/work-district language into the reserve.
            for(int i=0;i<6;i++)
            {
                float x=-9.7f-i*1.36f;
                float z=-1.0f+(i%2==0?-.10f:.10f);
                var slab=ValoriaKit.Block("VPD · west rebuilders route "+(i+1),new Vector3(x,.355f,z),
                    new Vector3(1.62f,.08f,1.92f),new Color(.40f,.375f,.32f));
                slab.transform.rotation=Quaternion.Euler(0,(i%2==0?-4f:3f),0);
                ValoriaKit.BenchmarkPieceTinted("VPD · west rebuilders cobble "+(i+1),
                    art!=null?art.SlavicCobbleRoad:null,new Vector3(x,.405f,z),1.58f,.14f,
                    Quaternion.Euler(0,(i%2==0?-4f:3f),0),ValoriaKit.WarmStone*.80f);
            }

            // Lower inhabited/work frontage. Small footprints and staggered placement preserve route readability.
            foreach(var home in new[]{
                new Vector3(-12.0f,.34f,-3.25f),new Vector3(-15.25f,.36f,-3.05f),
                new Vector3(-13.25f,.36f,2.45f),new Vector3(-17.35f,.38f,2.15f)})
                ValoriaKit.House("VPD · west rebuilders home",home,new Vector3(2.05f,1.18f,1.78f),true,Glow);

            // Bastion III food-economy growth: replace only the non-authoritative placeholder
            // house on the reserved west plot. The imported visual owns no gameplay collider/hotspot.
            if(showGranary)
                ValoriaKit.GranaryArchitecture("Granero",new Vector3(-17.2f,.34f,-3.0f),true,Glow);
            else
                ValoriaKit.House("VPD · west rebuilders granary placeholder",new Vector3(-18.05f,.35f,-2.15f),
                    new Vector3(2.05f,1.18f,1.78f),true,Glow);

            foreach(var yard in new[]{
                new Vector3(-11.55f,.24f,3.95f),new Vector3(-18.15f,.24f,4.45f)})
            {
                IrregularGround("VPD · west rebuilders work court",yard,3.35f,2.55f,new Color(.285f,.255f,.205f));
                ValoriaKit.BenchmarkPiece("VPD · west rebuilders firewood",art!=null?art.Firewood:null,
                    yard+new Vector3(-.65f,.13f,.08f),1.05f,.72f,Quaternion.Euler(0,22f,0));
                ValoriaKit.BenchmarkPieceTinted("VPD · west rebuilders fence",art!=null?art.SlavicStoneFence:null,
                    yard+new Vector3(.72f,.08f,.60f),1.55f,.78f,Quaternion.Euler(0,-8f,0),ValoriaKit.OldStone*.86f);
            }

            // Upper shelf makes the extension read vertically instead of as a flat suburb.
            ValoriaKit.House("VPD · west rebuilders upper dwelling",new Vector3(-15.8f,1.28f,5.55f),
                new Vector3(2.35f,1.28f,2.0f),true,Glow);
            ValoriaKit.House("VPD · west rebuilders upper dwelling",new Vector3(-18.05f,1.18f,6.05f),
                new Vector3(1.95f,1.12f,1.70f),true,Glow);

            // Reuse the certified seam family only as visual geology/burial. No collider or route depends on it.
            ValoriaKit.BenchmarkPieceTinted("VPD · west rebuilders rescued seam A",rescuedSeam,
                new Vector3(-11.05f,.18f,1.15f),2.65f,1.42f,Quaternion.Euler(0,74f,0),
                new Color(.18f,.19f,.18f,1f));
            ValoriaKit.BenchmarkPieceTinted("VPD · west rebuilders rescued seam B",rescuedSeam,
                new Vector3(-17.95f,.28f,5.05f),2.90f,1.50f,Quaternion.Euler(0,148f,0),
                new Color(.18f,.19f,.18f,1f));

            // Sparse skyline markers and vegetation tie the new quarter back into the same mountain-city silhouette.
            ValoriaKit.Banner("VPD · west rebuilders banner",new Vector3(-14.55f,2.25f,-.55f),
                new Vector3(.42f,1.45f,.07f),new Color(.18f,.30f,.43f));
            foreach(var p in new[]{
                new Vector3(-20.0f,.02f,-.3f),new Vector3(-19.5f,.02f,3.3f),
                new Vector3(-12.2f,.02f,6.6f)})
                ValoriaKit.PineTree("VPD · west rebuilders pine",p,.56f);

            // Edge geology establishes depth without consuming future building plots.
            foreach(var p in new[]{
                new Vector3(-20.2f,-.02f,-5.6f),new Vector3(-20.6f,.02f,7.8f),
                new Vector3(20.2f,-.02f,-5.4f),new Vector3(20.6f,.02f,8.0f),
                new Vector3(-6.7f,2.40f,17.4f),new Vector3(6.7f,2.40f,17.2f)})
                ValoriaKit.RockCluster("VPD · expansion edge geology",p,.92f,6);
        }

        static void TagVisibleHotspots(string prefix,string id)
        {
            foreach(var collider in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if(!collider.gameObject.name.StartsWith(prefix,System.StringComparison.Ordinal))continue;
                var hotspot=collider.GetComponent<WorldHotspot>();
                if(hotspot==null)hotspot=collider.gameObject.AddComponent<WorldHotspot>();
                hotspot.Id=id;
            }
        }

        static void City(PlayerState state)
        {
            var art=ValoriaExternalAssetLibrary.Load();

            // Keep the outer valley quiet. Flat colour islands made the city read like a board-game map;
            // use only sparse authored rock silhouettes and tree groups around the playable plateau.
            for(int i=0;i<12;i++)
            {
                float angle=(i*137f+18f)*Mathf.Deg2Rad;
                float radius=13.2f+(i%4)*2.25f;
                var p=new Vector3(Mathf.Cos(angle)*radius,.012f,1.0f+Mathf.Sin(angle)*radius*.78f);
                ValoriaKit.BenchmarkPieceTinted("Valoria · outer flat rock",art!=null?art.SlavicFlatRock:null,
                    p,1.85f+(i%3)*.28f,.48f+(i%2)*.08f,
                    Quaternion.Euler(0,i*41%360,0),new Color(.28f,.29f,.27f,1f));
                if(i%2==0)ValoriaKit.PineTree("Valoria · outer pine",p+new Vector3((i%3-1)*1.8f,0,1.8f),.72f+(i%3)*.10f);
            }

            // Close the rear horizon with restrained authored rock/tree silhouettes.
            // This gives Valoria a valley wall without returning to the oversized imported mountain blobs.
            for(int i=0;i<9;i++)
            {
                float x=-15.5f+i*3.85f;
                float z=18.2f+(i%3)*1.15f;
                var p=new Vector3(x,-.06f,z);
                if(ValoriaKit.BenchmarkPieceTinted("Valoria · rear ridge stone",art!=null?art.SlavicFlatRock:null,
                    p,2.80f+(i%2)*.35f,.62f+(i%3)*.08f,Quaternion.Euler(0,17+i*33,0),
                    new Color(.26f,.28f,.27f,1f))==null)
                    ValoriaKit.RockCluster("Valoria · rear ridge fallback",p,.85f,5);
                if(i%2==0)
                    ValoriaKit.PineTree("Valoria · ridge pine",p+new Vector3((i%3-1)*1.15f,0,-1.15f),.68f+(i%3)*.07f);
            }

            // Visual Bible production pass 02. Layout is now expressed through reusable modules
            // so authored prefabs can later replace them without changing gameplay coordinates.

            // Clifftop city terraces. Overlapping low rock shelves replace the old rectangular slabs:
            // gameplay elevations stay identical, but the visible perimeter no longer reads like a board.
            ValoriaKit.Cylinder("Valoria · upper terrace core",new Vector3(-.4f,.67f,4.25f),
                new Vector3(7.6f,.57f,5.05f),new Color(.30f,.29f,.25f),Quaternion.identity);
            ValoriaKit.Cylinder("Valoria · upper terrace west",new Vector3(-5.25f,.58f,4.55f),
                new Vector3(3.55f,.49f,4.10f),new Color(.28f,.28f,.24f),Quaternion.identity);
            ValoriaKit.Cylinder("Valoria · upper terrace east",new Vector3(5.25f,.60f,4.20f),
                new Vector3(3.60f,.50f,4.00f),new Color(.29f,.28f,.24f),Quaternion.identity);
            ValoriaKit.Cylinder("Valoria · lower terrace core",new Vector3(0,-.18f,-3.25f),
                new Vector3(8.75f,.32f,3.25f),new Color(.28f,.25f,.20f),Quaternion.identity);
            ValoriaKit.Cylinder("Valoria · lower terrace west",new Vector3(-5.55f,-.20f,-2.85f),
                new Vector3(3.85f,.30f,2.85f),new Color(.27f,.24f,.20f),Quaternion.identity);
            ValoriaKit.Cylinder("Valoria · lower terrace east",new Vector3(5.35f,-.20f,-3.05f),
                new Vector3(3.95f,.30f,2.90f),new Color(.27f,.24f,.20f),Quaternion.identity);
            ValoriaKit.RockCluster("Valoria plateau edge west",new Vector3(-10.0f,-.25f,1.8f),1.25f,10);
            ValoriaKit.RockCluster("Valoria plateau edge east",new Vector3(10.0f,-.25f,2.5f),1.20f,10);
            foreach(var q in new[]{
                new Vector3(-8.8f,.05f,-.4f),new Vector3(-9.0f,.06f,3.2f),new Vector3(-8.2f,.05f,6.9f),
                new Vector3(8.7f,.05f,-.6f),new Vector3(9.0f,.06f,3.0f),new Vector3(8.3f,.05f,6.8f)})
                ValoriaKit.BenchmarkPieceTinted("Valoria · terrace edge flat rock",art!=null?art.SlavicFlatRock:null,
                    q,2.65f,.58f,Quaternion.Euler(0,(q.x>0?31:-27),0),new Color(.31f,.31f,.28f,1f));
            for(int i=0;i<7;i++)
            {
                var p=new Vector3(-9+i*3f,-.62f,-6.0f+(i%2)*.22f);
                if(ValoriaKit.BenchmarkPieceTinted("Valoria · lower cliff stone",art!=null?art.SlavicFlatRock:null,
                    p,2.9f+(i%3)*.22f,.66f+(i%2)*.08f,Quaternion.Euler(0,17+i*29,0),
                    new Color(.33f,.32f,.28f,1f))==null)
                    ValoriaKit.RockCluster("Valoria · lower cliff fallback",p,.90f,5);
            }
            for(int i=0;i<6;i++)
            {
                float x=-8.1f+i*3.25f;
                var p=new Vector3(x,-.28f,9.45f+(i%2)*.22f);
                if(ValoriaKit.BenchmarkPieceTinted("Upper retaining rock",art!=null?art.SlavicFlatRock:null,
                    p,2.75f+(i%2)*.22f,.60f,Quaternion.Euler(0,21+i*31,0),
                    new Color(.29f,.30f,.28f,1f))==null)
                    ValoriaKit.RockCluster("Upper retaining rock fallback",p,.85f,5);
            }
            ValoriaKit.RockCluster("Valoria cliff rocks west",new Vector3(-8.6f,-.15f,-5.75f),1.45f,7);
            ValoriaKit.RockCluster("Valoria cliff rocks centre",new Vector3(-.6f,-.18f,-6.15f),1.30f,8);
            ValoriaKit.RockCluster("Valoria cliff rocks east",new Vector3(8.2f,-.12f,-5.8f),1.40f,7);
            foreach(var q in new[]{new Vector3(-8.8f,-.42f,-5.55f),new Vector3(-4.8f,-.48f,-6.0f),
                new Vector3(4.7f,-.45f,-5.95f),new Vector3(8.7f,-.40f,-5.45f)})
                ValoriaKit.BenchmarkPieceTinted("Valoria · authored cliff boulder",art!=null?art.SlavicBoulder:null,
                    q,2.35f,1.55f,Quaternion.Euler(0,(q.x>0?28:-24),0),ValoriaKit.OldStone*.82f);
            ValoriaKit.RockCluster("Valoria upper outcrop west",new Vector3(-9.0f,.02f,7.3f),1.05f,5);
            ValoriaKit.RockCluster("Valoria upper outcrop east",new Vector3(8.8f,.02f,7.9f),1.00f,5);

            // Authored ground transitions stitch the playable shelves into the valley floor.
            foreach(var q in new[]{
                new Vector3(-6.8f,.03f,-5.0f),new Vector3(-3.4f,.04f,-5.7f),new Vector3(3.6f,.04f,-5.5f),
                new Vector3(6.9f,.03f,-4.9f),new Vector3(-7.2f,.70f,2.2f),new Vector3(7.1f,.70f,2.6f)})
                ValoriaKit.BenchmarkPieceModulated("Valoria · mud transition",art!=null?art.SlavicMudFlat:null,
                    q,3.25f,.18f,Quaternion.Euler(0,(int)(q.x*17f)%360,0),new Color(.62f,.54f,.44f,1f));
            foreach(var q in new[]{
                new Vector3(-7.7f,.76f,5.6f),new Vector3(-4.6f,.75f,8.0f),new Vector3(4.8f,.75f,7.8f),
                new Vector3(7.6f,.74f,5.8f),new Vector3(-8.3f,.12f,-1.6f),new Vector3(8.1f,.12f,-1.4f)})
                ValoriaKit.BenchmarkPieceModulated("Valoria · moss seam",art!=null?art.SlavicMoss:null,
                    q,1.45f,.32f,Quaternion.Euler(0,(int)(q.z*23f)%360,0),new Color(.40f,.53f,.38f,1f));

            // Capture review: long procedural retaining slabs read as dark blockout from side angles.
            // Build the terrace edges from authored masonry modules instead, keeping short fallbacks only for safety.
            foreach(float side in new[]{-1f,1f})
            {
                for(int i=0;i<2;i++)
                {
                    var p=new Vector3(side*8.15f,.16f,1.9f+i*3.45f);
                    if(ValoriaKit.BenchmarkPieceModulated("Valoria · authored retaining wall",art!=null?art.MasonryWall:null,
                        p,3.65f,2.15f,Quaternion.Euler(0,90f,0),new Color(.52f,.53f,.51f,1f))==null)
                        ValoriaKit.Wall("Valoria retaining fallback",p+new Vector3(0,.90f,0),
                            new Vector3(.72f,1.65f,3.25f),ValoriaKit.OldStone*.72f,false);
                }
            }
            ValoriaKit.RockCluster("Old palace rubble west",new Vector3(-8.4f,.15f,6.2f),1.15f,10);
            ValoriaKit.RockCluster("Old palace rubble east",new Vector3(8.1f,.15f,6.6f),1.05f,9);

            // Dead-imperial rubble remains at the rear without eclipsing the living Bastion.
            ValoriaKit.RockCluster("Imperial collapse west",new Vector3(-7.4f,.02f,10.2f),1.25f,10);
            ValoriaKit.RockCluster("Imperial collapse east",new Vector3(7.0f,.02f,11.0f),1.15f,9);

            // Signature Bastion: fortress built inside a dead palace.
            ValoriaKit.BastionCore("Bastion",new Vector3(0,1.32f,5.25f),Glow);

            // Main central route from foreground to fortress. Use one coherent Slavic stone family
            // for the civic approach, palette-normalised to Eldoria instead of raw package colours.
            var lowerGateArt=ValoriaKit.BenchmarkPieceTinted("Valoria · lower stone gate",art!=null?art.SlavicRockGate:null,
                new Vector3(0,.12f,-4.65f),5.0f,3.3f,Quaternion.identity,ValoriaKit.WarmStone*.90f);
            if(lowerGateArt==null)
                ValoriaKit.Wall("Valoria · lower entrance",new Vector3(0,1.25f,-4.65f),new Vector3(5.4f,2.35f,.75f),ValoriaKit.WarmStone*.88f,false);
            var gate=ValoriaKit.Block("Puerta · ir al mundo",new Vector3(0,1.05f,-4.82f),new Vector3(2.25f,2.0f,.34f),ValoriaKit.Timber);
            gate.AddComponent<WorldHotspot>().Id="gate";
            gate.GetComponent<Renderer>().enabled=false;
            for(int i=0;i<10;i++)
            {
                var p=new Vector3(Mathf.Sin(i*.31f)*.22f,.20f,-10.5f+i*1.22f);
                if(ValoriaKit.BenchmarkPieceTinted("Valoria · authored cobble route",art!=null?art.SlavicCobbleRoad:null,
                    p,2.55f,.22f,Quaternion.Euler(0,(i%3-1)*5,0),ValoriaKit.WarmStone*.82f)==null)
                {
                    var road=ValoriaKit.Block("Valoria · worn stone route",p,new Vector3(2.35f,.08f,.92f),ValoriaKit.WarmStone*.84f);
                    road.transform.rotation=Quaternion.Euler(0,(i%3-1)*5,0);
                }
            }
            ValoriaKit.Stair("Bastion stair",new Vector3(0,.34f,.65f),9,3.1f,.18f,.44f,ValoriaKit.WarmStone*.74f);
            ValoriaKit.Rubble("Gate rubble",new Vector3(-3.4f,.22f,-3.8f),1.0f,6);

            // Left: work district / Sawmill.
            ValoriaKit.Block("Sawmill yard",new Vector3(-6.35f,.18f,-1.7f),new Vector3(5.0f,.22f,4.3f),ValoriaKit.Earth*.98f);
            for(int i=0;i<2;i++)
                ValoriaKit.BenchmarkPieceTinted("Sawmill · stone yard edge",art!=null?art.SlavicStoneFence:null,
                    new Vector3(-8.55f+i*4.35f,.16f,-3.45f),2.15f,1.15f,Quaternion.identity,ValoriaKit.OldStone*.90f);
            ValoriaKit.House("Sawmill",new Vector3(-6.4f,.42f,-1.45f),new Vector3(3.5f,1.65f,2.65f),state.SawmillLevel>0,Glow);
            var mill=ValoriaKit.Block("Aserradero · interacción",new Vector3(-6.4f,1.18f,-1.45f),
                new Vector3(3.05f,1.45f,2.35f),state.SawmillLevel>0?new Color(.34f,.25f,.17f):new Color(.18f,.18f,.17f));
            mill.AddComponent<WorldHotspot>().Id="sawmill";
            mill.GetComponent<Renderer>().enabled=false;
            ValoriaKit.BenchmarkPiece("Aserradero · leña",art!=null?art.Firewood:null,
                new Vector3(-8.2f,.31f,-2.7f),1.25f,.9f,Quaternion.Euler(0,18,0));
            ValoriaKit.Scaffold("Sawmill scaffold",new Vector3(-8.1f,1.6f,.05f),new Vector3(1.5f,2.8f,1.2f));
            if(state.SawmillLevel>0)
            {
                Glow("Sawmill fire",new Vector3(-5.45f,1.65f,-1.9f),Amber,1.55f,4.0f);
                for(int i=0;i<5;i++)
                    ValoriaKit.Cylinder("Log stack",new Vector3(-8.6f+i*.42f,.36f,-2.8f),new Vector3(.25f,1.75f,.25f),
                        new Color(.27f,.17f,.10f),Quaternion.Euler(90,0,0));
            }

            // Right: military district grows into a readable Bastion II objective.
            ValoriaKit.House("Early barracks",new Vector3(6.0f,.42f,-1.55f),new Vector3(3.4f,1.7f,2.75f),state.BarracksLevel>0,Glow);
            var barracks=ValoriaKit.Block("Cuartel · interacción",new Vector3(6.0f,1.18f,-1.55f),
                new Vector3(3.0f,1.40f,2.35f),state.BarracksLevel>0?new Color(.36f,.34f,.30f):new Color(.20f,.20f,.19f));
            barracks.AddComponent<WorldHotspot>().Id="barracks";
            barracks.GetComponent<Renderer>().enabled=false;
            if(state.BastionLevel>=2 && state.BarracksLevel==0)
                ValoriaKit.Scaffold("Cuartel scaffold",new Vector3(7.75f,1.55f,-.55f),new Vector3(1.35f,2.7f,1.1f));
            if(state.BarracksLevel>0)
            {
                Glow("Barracks forge light",new Vector3(6.8f,1.4f,-2.15f),Amber,1.05f,3.4f);
                ValoriaKit.Banner("Barracks banner",new Vector3(5.15f,2.0f,-2.75f),new Vector3(.55f,1.65f,.08f),new Color(.16f,.25f,.34f));
            }
            ValoriaKit.House("Granary",new Vector3(4.55f,.40f,-4.0f),new Vector3(2.75f,1.5f,2.15f),true,Glow);
            ValoriaKit.Block("Training yard",new Vector3(7.05f,.17f,-4.0f),new Vector3(3.4f,.16f,2.5f),ValoriaKit.Earth*.94f);
            for(int i=0;i<2;i++)
                ValoriaKit.BenchmarkPieceTinted("Training yard · stone edge",art!=null?art.SlavicStoneFence:null,
                    new Vector3(5.65f+i*2.8f,.14f,-5.15f),1.55f,.95f,Quaternion.identity,ValoriaKit.OldStone*.88f);

            // Sparse but readable settlement around the Bastion. Keep all structures in one authored kit.
            foreach(var shelter in new[]{new Vector3(-3.45f,.30f,-3.35f),new Vector3(2.7f,.30f,-2.75f),
                new Vector3(-4.1f,.30f,.05f),new Vector3(3.85f,.30f,.20f)})
                ValoriaKit.House("Rebuilder shelter",shelter,new Vector3(1.9f,1.1f,1.65f),true,Glow);
            ValoriaKit.House("Valoria cottage west",new Vector3(-6.0f,.30f,2.0f),new Vector3(2.2f,1.25f,1.85f),true,Glow);
            ValoriaKit.House("Valoria cottage east",new Vector3(6.0f,.30f,2.15f),new Vector3(2.2f,1.25f,1.85f),true,Glow);

            // Inhabited lower-town layer. Keep the central approach readable, but remove the empty-diorama
            // read by overlapping small rebuilt homes, work courts and authored props into both flanks.
            foreach(var home in new[]{
                new Vector3(-8.35f,.26f,-4.25f),new Vector3(-8.05f,.28f,4.25f),
                new Vector3(8.15f,.27f,-4.35f),new Vector3(7.85f,.28f,4.45f)})
                ValoriaKit.House("Valoria lower-town home",home,new Vector3(1.75f,1.05f,1.48f),true,Glow);
            foreach(var yard in new[]{
                new Vector3(-4.75f,.17f,-4.95f),new Vector3(4.95f,.17f,-5.05f)})
            {
                ValoriaKit.Block("Valoria work court",yard,new Vector3(2.35f,.10f,1.55f),ValoriaKit.Earth*.92f);
                ValoriaKit.BenchmarkPiece("Valoria work court · firewood",art!=null?art.Firewood:null,
                    yard+new Vector3(-.72f,.12f,.08f),.90f,.62f,Quaternion.Euler(0,24f,0));
                ValoriaKit.BenchmarkPieceTinted("Valoria work court · fence",art!=null?art.SlavicStoneFence:null,
                    yard+new Vector3(.75f,.05f,.52f),1.40f,.72f,Quaternion.Euler(0,8f,0),ValoriaKit.OldStone*.88f);
            }
            foreach(var bush in new[]{
                new Vector3(-9.15f,.02f,-2.0f),new Vector3(-7.35f,.02f,6.0f),
                new Vector3(9.1f,.02f,-2.15f),new Vector3(7.15f,.02f,6.15f),
                new Vector3(-4.6f,.72f,5.25f),new Vector3(4.75f,.72f,5.35f)})
                ValoriaKit.BenchmarkPieceModulated("Valoria · authored undergrowth",art!=null?art.SlavicBush:null,
                    bush,1.10f,.72f,Quaternion.Euler(0,(int)(bush.x*29f)%360,0),
                    new Color(.42f,.58f,.43f,1f));

            // The imported Slavic vegetation was visually incompatible in URP (white/yellow blow-out).
            // Use one restrained dark-pine family until a production vegetation set is selected.
            for(int i=0;i<26;i++)
            {
                float z=-10.5f+(i*19%31)*.92f;
                float x=(i%2==0?-1f:1f)*(10.35f+(i*7%7)*.58f);
                ValoriaKit.PineTree("Valoria pine",new Vector3(x,0,z),.66f+(i%4)*.07f);
            }
            for(int i=0;i<8;i++)
            {
                float z=-5.8f+(i*11%17)*.82f;
                float x=(i%2==0?-1f:1f)*(8.75f+(i*5%4)*.38f);
                ValoriaKit.PineTree("Valoria inner pine",new Vector3(x,0,z),.54f+(i%3)*.06f);
            }
            ValoriaKit.RockCluster("Valoria roadside rocks",new Vector3(-3.7f,.05f,-1.35f),.58f,4);
            ValoriaKit.RockCluster("Valoria barracks rocks",new Vector3(7.5f,.05f,-1.15f),.52f,4);

            Hero(new Vector3(-1.7f,0,-1.9f),1.0f);
            int visibleArchers=state.BastionLevel>=2?4:3;
            for(int i=0;i<visibleArchers;i++)Archer(new Vector3(2.0f+(i%4)*.67f,0,-2.8f+(i/4)*.68f));
            ValoriaKit.Banner("Lower town banner west",new Vector3(-2.15f,2.0f,-4.58f),
                new Vector3(.42f,1.35f,.06f),new Color(.18f,.32f,.48f));
            ValoriaKit.Banner("Lower town banner east",new Vector3(2.15f,2.0f,-4.58f),
                new Vector3(.42f,1.35f,.06f),new Color(.18f,.32f,.48f));
            Glow("Gate torch L",new Vector3(-2.85f,2.0f,-4.4f),Amber,1.25f,3.0f);
            Glow("Gate torch R",new Vector3(2.85f,2.0f,-4.4f),Amber,1.25f,3.0f);
            Glow("Bastion inhabited warmth",new Vector3(0,4.15f,3.45f),Amber,1.45f,7.2f);
        }
        static void StyleRescuedResidential(GameObject root)
        {
            if(root==null)return;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)
                {
                    var source=mats[i];
                    var lower=(source!=null?source.name:"").ToLowerInvariant();
                    Color color;
                    if(lower.Contains("stone")||(!lower.Contains("rock")&&!lower.Contains("roof")&&!lower.Contains("timber")&&i==0))
                        color=new Color(.43f,.36f,.27f,1f);
                    else if(lower.Contains("rock")||i==1)
                        color=new Color(.15f,.16f,.15f,1f);
                    else if(lower.Contains("roof")||i==2)
                        color=new Color(.085f,.095f,.105f,1f);
                    else if(lower.Contains("timber")||i==3)
                        color=new Color(.34f,.18f,.075f,1f);
                    else color=new Color(.25f,.24f,.21f,1f);
                    mats[i]=ValoriaKit.Material(color);
                }
                renderer.sharedMaterials=mats;
            }
        }

        static void Frontier(PlayerState state)
        {
            var art=ValoriaExternalAssetLibrary.Load();

            // I-II production corridor: one authored route from Valoria into resources and corruption.
            // Keep gameplay hotspots independent from the visual dressing so art never owns rules.
            // World-map visual base extends well beyond the interactive corridor so the official
            // camera never exposes a tabletop edge. Gameplay topology remains unchanged.
            IrregularGround("Frontier · valley floor",new Vector3(0,-.04f,1.0f),52.0f,44.0f,new Color(.155f,.145f,.118f),"earth");
            IrregularGround("Frontier · playable earth",new Vector3(0,.02f,1.0f),27.0f,22.0f,new Color(.175f,.165f,.135f),"earth");
            IrregularGround("Frontier · Valoria approach",new Vector3(0,.07f,-7.0f),8.2f,5.2f,new Color(.205f,.180f,.135f),"earth");

            // Low relief around the playable wedge gives Frontier a world horizon instead of a
            // floating board. These are visual-only terrain silhouettes with no gameplay collision.
            ValoriaKit.TerrainPieceTinted("SM_Hills_01","Frontier · west ridge",
                new Vector3(-17.0f,-.45f,3.5f),9.5f,3.4f,Quaternion.Euler(0,24f,0),new Color(.30f,.31f,.27f,1f));
            ValoriaKit.TerrainPieceTinted("SM_Hills_01","Frontier · east ridge",
                new Vector3(17.2f,-.50f,4.2f),9.8f,3.5f,Quaternion.Euler(0,-31f,0),new Color(.29f,.30f,.27f,1f));
            ValoriaKit.TerrainPieceTinted("SM_Cliffs_01","Frontier · north cliff west",
                new Vector3(-9.5f,-.65f,15.0f),8.8f,4.4f,Quaternion.Euler(0,17f,0),new Color(.31f,.31f,.29f,1f));
            ValoriaKit.TerrainPieceTinted("SM_Cliffs_03","Frontier · north cliff east",
                new Vector3(9.4f,-.70f,15.3f),9.0f,4.6f,Quaternion.Euler(0,-21f,0),new Color(.30f,.30f,.29f,1f));
            ValoriaKit.TerrainPieceTinted("SM_Terrain_03","Frontier · south terrain transition",
                new Vector3(0f,-.58f,-15.0f),12.0f,2.4f,Quaternion.Euler(0,8f,0),new Color(.32f,.30f,.25f,1f));

            // World Route Kit v1: one continuous, mobile-readable corridor from Valoria
            // toward the threat. Visual-only dressing never owns march topology or rules.
            WorldRouteKit.MarchRoute("Frontier · march route kit",new Vector3(0,.13f,-1.0f),12.4f,2.60f,0f);

            // Forest resource pocket. The invisible hotspot remains the only gameplay target.
            IrregularGround("Frontier · forest earth",new Vector3(-6.3f,.04f,1.1f),5.7f,5.25f,new Color(.145f,.175f,.125f),"earth");
            var grove=Cylinder("Bosque de Valoria · recolectar",new Vector3(-6.2f,1.35f,1.0f),
                new Vector3(2.35f,2.7f,2.35f),new Color(.18f,.22f,.18f),Quaternion.identity);
            grove.AddComponent<WorldHotspot>().Id="forest-valoria";
            grove.GetComponent<Renderer>().enabled=false;
            for(int i=0;i<10;i++)
            {
                float x=-8.5f+(i*17%9)*.62f;
                float z=-1.15f+(i*23%8)*.68f;
                ValoriaKit.PineTree("Frontier · forest pine",new Vector3(x,.02f,z),.58f+(i%4)*.08f);
            }
            foreach(var p in new[]{
                new Vector3(-8.4f,.04f,2.8f),new Vector3(-6.9f,.04f,3.55f),
                new Vector3(-4.7f,.04f,2.65f),new Vector3(-7.6f,.04f,-.55f)})
                ValoriaKit.BenchmarkPieceModulated("Frontier · undergrowth",art!=null?art.SlavicBush:null,
                    p,1.0f,.68f,Quaternion.Euler(0,(int)(p.x*31f)%360,0),new Color(.35f,.49f,.36f,1f));
            ValoriaKit.BenchmarkPiece("Frontier · stacked timber",art!=null?art.Firewood:null,
                new Vector3(-4.25f,.12f,.45f),1.25f,.78f,Quaternion.Euler(0,-18f,0));
            Glow("Frontier · lumber warmth",new Vector3(-4.35f,.72f,.35f),Amber,.68f,2.6f);
            // Capture review rejected the imported tall-tree silhouettes: they read bare/corrupted.
            // Keep Frontier's forest language evergreen and reserve Slavic for undergrowth/moss/ground props.
            foreach(var p in new[]{
                new Vector3(-8.65f,.03f,1.95f),new Vector3(-6.1f,.03f,3.35f),
                new Vector3(-4.65f,.03f,1.65f),new Vector3(-5.55f,.03f,-.25f),
                new Vector3(-7.55f,.03f,3.0f)})
                ValoriaKit.PineTree("Frontier · tall evergreen",p,.78f);
            foreach(var p in new[]{new Vector3(-7.8f,.05f,1.2f),new Vector3(-5.6f,.05f,2.5f)})
                ValoriaKit.BenchmarkPieceModulated("Frontier · forest moss",art!=null?art.SlavicMoss:null,
                    p,1.7f,.28f,Quaternion.Euler(0,(int)(p.x*29f)%360,0),new Color(.52f,.62f,.48f,1f));
            foreach(var p in new[]{new Vector3(-6.9f,.025f,.55f),new Vector3(-5.15f,.025f,1.35f)})
                ValoriaKit.BenchmarkPieceModulated("Frontier · forest floor mud",art!=null?art.SlavicMudFlat:null,
                    p,2.35f,.12f,Quaternion.Euler(0,(int)(p.z*53f)%360,0),new Color(.48f,.42f,.32f,1f));

            // Quarry identity is staged visually now so the incoming authoritative gather mechanic
            // can attach without another art pass. No hotspot is added here until gameplay owns it.
            IrregularGround("Frontier · quarry shelf",new Vector3(6.3f,.05f,-2.2f),5.7f,4.9f,new Color(.205f,.198f,.180f),"stone");
            WorldResourceKit.QuarryResourcePocket("Frontier · quarry resource kit",new Vector3(6.35f,.02f,-2.15f),2.65f);
            var quarryMarker=Box("Cantera de Valoria · visual reserve",new Vector3(6.5f,.28f,-2.1f),
                new Vector3(3.7f,.18f,3.4f),new Color(.20f,.20f,.19f));
            quarryMarker.GetComponent<Renderer>().enabled=false;
            var qCol=quarryMarker.GetComponent<Collider>();if(qCol!=null)Object.Destroy(qCol);

            // Forest/road framing without random prototype scatter.
            foreach(var p in new[]{
                new Vector3(-11.2f,.02f,-4.5f),new Vector3(-10.8f,.02f,5.6f),
                new Vector3(10.8f,.02f,-4.7f),new Vector3(10.5f,.02f,4.5f),
                new Vector3(-9.7f,.02f,8.3f),new Vector3(8.9f,.02f,8.0f)})
                ValoriaKit.PineTree("Frontier · ridge pine",p,.64f);
            foreach(var p in new[]{
                new Vector3(-10.5f,.04f,1.8f),new Vector3(10.2f,.04f,1.2f),
                new Vector3(-3.7f,.04f,7.2f),new Vector3(3.6f,.04f,7.0f)})
                ValoriaKit.RockCluster("Frontier · route geology",p,.68f,5);

            // The route becomes visibly colder/corrupted before the encounter, so the threat reads
            // as territory rather than an isolated dark model.
            IrregularGround("Frontier · corrupted shelf",new Vector3(6.0f,.045f,4.1f),5.8f,5.2f,new Color(.165f,.155f,.175f),"slate");
            Glow("Frontier · road lantern west",new Vector3(-1.55f,1.05f,-4.9f),Amber,.72f,2.8f);
            Glow("Frontier · road lantern east",new Vector3(1.45f,1.00f,-3.4f),Amber,.64f,2.5f);

            // Enemy target stays an invisible gameplay volume; visible threat is built as authored
            // dark-fantasy silhouette around it rather than exposing a primitive sphere.
            string enemyId=state.BastionLevel>=2?"engendro-valoria":"corrupt-scout";
            bool defeated=state.BastionLevel>=2?state.EngendroDefeated:state.ScoutDefeated;
            var enemy=Sphere(state.BastionLevel>=2?"Engendro de la Brecha · target":"Explorador corrupto · target",
                new Vector3(5.0f,1.0f,3.15f),state.BastionLevel>=2?new Vector3(1.90f,2.70f,1.90f):new Vector3(1.45f,2.2f,1.45f),
                Color.clear);
            enemy.AddComponent<WorldHotspot>().Id=enemyId;
            enemy.GetComponent<Renderer>().enabled=false;

            if(state.BastionLevel>=2)
            {
                var torso=Sphere("Engendro · corrupted torso",new Vector3(5.0f,1.18f,3.15f),
                    new Vector3(1.45f,1.10f,1.28f),defeated?Stone*.38f:new Color(.18f,.16f,.20f));
                var tc=torso.GetComponent<Collider>();if(tc!=null)Object.Destroy(tc);
                foreach(float x in new[]{-1f,1f})
                {
                    var shoulder=Sphere("Engendro · stone shoulder",new Vector3(5.0f+x*.68f,1.48f,3.02f),
                        new Vector3(.88f,.76f,.82f),defeated?Stone*.34f:new Color(.16f,.17f,.19f));
                    var shc=shoulder.GetComponent<Collider>();if(shc!=null)Object.Destroy(shc);
                    var limb=Cylinder("Engendro · forelimb",new Vector3(5.0f+x*.83f,.68f,2.78f),
                        new Vector3(.20f,.98f,.20f),defeated?Stone*.32f:new Color(.13f,.14f,.15f),Quaternion.Euler(22f,0,x*24f));
                    var lc=limb.GetComponent<Collider>();if(lc!=null)Object.Destroy(lc);
                }
                var spine=Sphere("Engendro · fractured spine",new Vector3(5.0f,1.72f,3.18f),
                    new Vector3(.86f,.62f,1.08f),defeated?Stone*.35f:new Color(.12f,.13f,.15f));
                var spc=spine.GetComponent<Collider>();if(spc!=null)Object.Destroy(spc);
                if(!defeated) Glow("Engendro · corruption core",new Vector3(5.0f,1.28f,2.50f),new Color(.48f,.22f,.62f),.72f,2.8f);
            }
            else
            {
                var body=Cylinder("Explorador corrupto · body",new Vector3(5.0f,1.0f,3.15f),
                    new Vector3(.48f,1.45f,.44f),defeated?Stone*.36f:new Color(.17f,.18f,.19f),Quaternion.identity);
                var bc=body.GetComponent<Collider>();if(bc!=null)Object.Destroy(bc);
                var hood=Sphere("Explorador corrupto · hood",new Vector3(5.0f,2.02f,3.15f),
                    new Vector3(.56f,.52f,.54f),defeated?Stone*.34f:new Color(.12f,.13f,.16f));
                var hc=hood.GetComponent<Collider>();if(hc!=null)Object.Destroy(hc);
                var spear=Box("Explorador corrupto · spear",new Vector3(5.55f,1.25f,3.05f),
                    new Vector3(.10f,2.60f,.10f),new Color(.25f,.20f,.16f));
                spear.transform.rotation=Quaternion.Euler(0,0,-8f);
                var sc=spear.GetComponent<Collider>();if(sc!=null)Object.Destroy(sc);
                if(!defeated)Glow("Explorador corrupto · violet mark",new Vector3(5.0f,1.25f,2.72f),Violet,.62f,2.1f);
            }

            // Ruined imperial watchpost and distant Breach scar frame the threat direction.
            if(ValoriaKit.BenchmarkPieceModulated("Frontier · broken imperial watchpost",art!=null?art.RuinedTower:null,
                new Vector3(7.1f,.08f,5.25f),2.65f,3.65f,Quaternion.Euler(0,-14f,0),new Color(.46f,.47f,.45f,1f))==null)
            {
                var tower=Box("Frontier · broken watchpost fallback",new Vector3(7.1f,1.05f,5.25f),
                    new Vector3(2.15f,2.10f,1.65f),Stone*.52f);
                var wc=tower.GetComponent<Collider>();if(wc!=null)Object.Destroy(wc);
            }
            ValoriaScar(new Vector3(10.0f,.01f,8.0f));

            // Party staging stays readable at the road mouth.
            Hero(new Vector3(-1.35f,.02f,-6.0f),.90f);
            for(int i=0;i<4;i++) Archer(new Vector3(-.65f+i*.55f,.02f,-7.0f));
            if(state.March.Phase!="idle")
            {
                for(int i=0;i<4;i++)
                {
                    float t=(i+1)/5f;
                    var marker=Sphere("March trail "+i,new Vector3(Mathf.Lerp(0f,4.1f,t),.20f,Mathf.Lerp(-4.8f,2.2f,t)),
                        new Vector3(.22f,.06f,.22f),Amber*.72f);
                    var mc=marker.GetComponent<Collider>();if(mc!=null)Object.Destroy(mc);
                    marker.AddComponent<BreachPulse>().Speed=1.1f+i*.12f;
                }
            }
        }
        static void House(string name,Vector3 p,Vector3 size,bool lit)
        {
            Box(name+" · stone base",p+new Vector3(0,size.y*.45f,0),size,WarmStone*.72f);
            GableRoof(name+" · roof",p+new Vector3(0,size.y+0.35f,0),new Vector3(size.x*1.12f,.8f,size.z*1.16f),Timber);
            Box(name+" · door",p+new Vector3(0,.75f,-size.z*.52f),new Vector3(.48f,1.25f,.16f),Timber*.82f);
            if(lit) Glow(name+" · hearth",p+new Vector3(.45f,1.0f,-size.z*.58f),Amber,.85f,2.7f);
        }
        static GameObject GableRoof(string name,Vector3 p,Vector3 s,Color c)
        {
            var go=new GameObject(name);
            go.transform.position=p;
            var mf=go.AddComponent<MeshFilter>();
            var mr=go.AddComponent<MeshRenderer>();
            float x=s.x*.5f,z=s.z*.5f,h=s.y;
            var mesh=new Mesh();
            mesh.vertices=new[]{
                new Vector3(-x,0,-z),new Vector3(x,0,-z),new Vector3(0,h,-z),
                new Vector3(-x,0,z),new Vector3(0,h,z),new Vector3(x,0,z)
            };
            mesh.triangles=new[]{
                0,1,2, 3,4,5,
                0,2,4, 0,4,3,
                1,5,4, 1,4,2
            };
            mesh.RecalculateNormals();
            mf.sharedMesh=mesh; mr.sharedMaterial=Mat(c);
            return go;
        }
        static void Arch(string name,Vector3 center,float radius,float thickness,int blocks,Color color)
        {
            // Upper semicircle plus two massive piers.
            for(int i=0;i<blocks;i++)
            {
                float t=i/(float)(blocks-1);
                float a=Mathf.Lerp(20f,160f,t)*Mathf.Deg2Rad;
                var p=center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0);
                var b=Box(name+" · voussoir",p,new Vector3(thickness,1.05f,1.65f),color);
                b.transform.rotation=Quaternion.Euler(0,0,90f-Mathf.Rad2Deg*a);
            }
            Box(name+" · left pier",center+new Vector3(-radius+.45f,1.9f,0),new Vector3(thickness*1.25f,4.2f,1.9f),color*.95f);
            Box(name+" · right pier",center+new Vector3(radius-.45f,1.9f,0),new Vector3(thickness*1.25f,4.2f,1.9f),color*.95f);
        }
        static void Scaffold(Vector3 p,Vector3 s)
        {
            Color wood=new Color(.27f,.18f,.11f);
            float hx=s.x*.5f,hz=s.z*.5f;
            foreach(float x in new[]{-hx,hx})
                foreach(float z in new[]{-hz,hz})
                    Box("Scaffold post",p+new Vector3(x,0,z),new Vector3(.12f,s.y,.12f),wood);
            for(int level=0;level<3;level++)
            {
                float y=-s.y*.45f+level*s.y*.45f;
                Box("Scaffold rail",p+new Vector3(0,y,-hz),new Vector3(s.x,.10f,.10f),wood);
                Box("Scaffold rail",p+new Vector3(0,y,hz),new Vector3(s.x,.10f,.10f),wood);
            }
        }
        static void PineTree(Vector3 p,float scale)
        {
            Cylinder("Pine trunk",p+Vector3.up*1.25f*scale,new Vector3(.16f,1.3f,.16f)*scale,new Color(.22f,.15f,.10f),Quaternion.identity);
            for(int i=0;i<3;i++)
            {
                float y=(1.4f+i*.65f)*scale;
                Cylinder("Pine crown",p+Vector3.up*y,new Vector3((1.15f-i*.18f)*scale,.65f*scale,(1.15f-i*.18f)*scale),Pine*(.9f+i*.05f),Quaternion.identity);
            }
        }
        static void Tower(Vector3 p,float height)
        {
            Cylinder("Imperial tower",p+Vector3.up*(height/2),new Vector3(1.8f,height,1.8f),Stone*.8f,Quaternion.identity);
            Cylinder("Tower rim",p+Vector3.up*(height+.1f),new Vector3(2.2f,.45f,2.2f),Stone,Quaternion.identity);
            Cylinder("Slate roof",p+Vector3.up*(height+.45f),new Vector3(1.6f,.3f,1.6f),Deep,Quaternion.identity);
        }
        static void Tree(Vector3 p,bool living)
        {
            var trunk=Cylinder("Border tree",p+new Vector3(0,1.2f,0),new Vector3(.22f,2.4f,.22f),new Color(.24f,.21f,.2f),Quaternion.Euler(0,0,12));
            var branch=Box("Branch",p+new Vector3(.35f,2.3f,0),new Vector3(1,.12f,.12f),new Color(.25f,.23f,.2f)); branch.transform.rotation=Quaternion.Euler(0,0,28);
            if(living) Sphere("Ash needles",p+new Vector3(0,2.8f,0),new Vector3(1.3f,.85f,1.1f),new Color(.17f,.25f,.22f));
        }
        static void Hero(Vector3 p,float scale)
        {
            var body=Cylinder("Sir Aldric · guardian",p+Vector3.up*.95f*scale,new Vector3(.7f,1.65f,.58f)*scale,
                new Color(.34f,.40f,.47f),Quaternion.identity);
            Sphere("Aldric helm",p+Vector3.up*2.05f*scale,Vector3.one*.58f*scale,Stone*.78f);
            Box("Aldric cloak",p+new Vector3(0,.95f,.38f)*scale,new Vector3(.8f,1.3f,.13f)*scale,new Color(.29f,.14f,.16f));
            Box("Aldric shield",p+new Vector3(-.57f,1.05f,-.1f)*scale,new Vector3(.2f,.9f,.65f)*scale,new Color(.50f,.40f,.27f));
        }
        static void Archer(Vector3 p)
        {
            Cylinder("Archer silhouette",p+Vector3.up*.7f,new Vector3(.27f,1.2f,.28f),new Color(.31f,.33f,.33f),Quaternion.identity);
            Sphere("Archer hood",p+Vector3.up*1.48f,Vector3.one*.34f,new Color(.20f,.24f,.24f));
            Cylinder("Bow",p+new Vector3(.33f,.88f,0),new Vector3(.07f,1.2f,.07f),new Color(.55f,.39f,.2f),Quaternion.Euler(0,0,12));
        }
        static void Rift(Vector3 p)
        {
            for(int i=0;i<5;i++)
            {
                var shard=Box("Fractured Breach shard",p+new Vector3((i-2)*.77f,1.15f+i%2*.5f,i%2*.4f),
                    new Vector3(.28f,2.2f+i%2, .27f),Violet * (i%2==0?1:.6f));
                shard.transform.rotation=Quaternion.Euler(13,i*31,(i-2)*12);
                shard.AddComponent<BreachPulse>().Speed=.9f+i*.17f;
            }
            Glow("Breach wound",p+new Vector3(0,1.2f,0),Violet,2.4f,8);
            Cylinder("Corruption scar",p+new Vector3(0,.07f,0),new Vector3(4,.08f,3),new Color(.25f,.08f,.32f),Quaternion.identity);
        }
        static void ValoriaScar(Vector3 p)
        {
            // A wound in the soil behind the city: a cracked, stained surface rather than a portal prop.
            IrregularGround("Brecha · burned earth",p,5.2f,3.8f,new Color(.10f,.105f,.115f));
            for(int i=0;i<5;i++)
            {
                float x=-2.0f+i*.96f;
                var fissure=Box("Brecha · buried fracture",p+new Vector3(x,.055f,Mathf.Sin(i*.8f)*.40f),
                    new Vector3(.07f,.025f,1.6f+(i%2)*.7f),new Color(.26f,.13f,.29f));
                fissure.transform.rotation=Quaternion.Euler(0,18+i*13,0);
            }
            var library=ValoriaExternalAssetLibrary.Load();
            ValoriaKit.RockCluster("Brecha · collapsed watchtower",p+new Vector3(2.25f,.02f,1.25f),1.15f,14);
            ValoriaKit.RockCluster("Brecha · displaced rock",p+new Vector3(-2.1f,0,.9f),1.0f,12);
            Glow("Brecha · restrained violet glow",p+new Vector3(.1f,.55f,0),new Color(.48f,.26f,.56f),.65f,3.6f);
        }
        static void IrregularGround(string name,Vector3 center,float width,float depth,Color color)
            => IrregularGround(name,center,width,depth,color,null);

        static void IrregularGround(string name,Vector3 center,float width,float depth,Color color,string surfaceKind)
        {
            const int sides=11;
            var vertices=new Vector3[sides+1];var triangles=new int[sides*3];var uv=new Vector2[sides+1];
            vertices[0]=Vector3.zero;uv[0]=new Vector2(.5f,.5f);
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides;
                float wobble=.82f+(i*19%7)*.055f;
                vertices[i+1]=new Vector3(Mathf.Cos(a)*width*.5f*wobble,0,Mathf.Sin(a)*depth*.5f*wobble);
                uv[i+1]=new Vector2(vertices[i+1].x/Mathf.Max(.01f,width)+.5f,
                    vertices[i+1].z/Mathf.Max(.01f,depth)+.5f);
                triangles[i*3]=0;triangles[i*3+1]=(i+1)%sides+1;triangles[i*3+2]=i+1;
            }
            var go=new GameObject(name);go.transform.position=center;
            var mesh=new Mesh { name=name+" mesh",vertices=vertices,triangles=triangles,uv=uv };
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;

            // Large terrain sheets previously had no UVs, so the generated ground texture could
            // not read at all. Keep the texture's own colour, neutralise the second material tint,
            // and tile by world size so earth/stone breakup remains visible from the mobile camera.
            var tiling=new Vector2(Mathf.Max(4f,width/2.0f),Mathf.Max(4f,depth/2.0f));
            var material=string.IsNullOrEmpty(surfaceKind)
                ? new Material(Mat(color)){name="Eldoria ground · "+ColorUtility.ToHtmlStringRGB(color)}
                : ValoriaKit.SurfaceMaterial(color,surfaceKind,tiling);
            if(string.IsNullOrEmpty(surfaceKind))
            {
                if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",Color.white);
                if(material.HasProperty("_Color"))material.SetColor("_Color",Color.white);
                if(material.HasProperty("_BaseMap"))material.SetTextureScale("_BaseMap",tiling);
                else if(material.HasProperty("_MainTex"))material.SetTextureScale("_MainTex",tiling);
                if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.03f);
            }
            go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        static void Glow(string name,Vector3 p,Color color,float intensity,float range)
        {
            var light=new GameObject(name).AddComponent<Light>(); light.type=LightType.Point;
            light.transform.position=p; light.color=color; light.intensity=intensity; light.range=range;
            light.gameObject.AddComponent<BreachPulse>().Speed=color==Violet?.8f:1.3f;
        }
        static Material Mat(Color color)=>ValoriaKit.Material(color);
        static GameObject Shape(string name,PrimitiveType type,Vector3 p,Vector3 scale,Color color)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.position=p;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=Mat(color);return go;
        }
        static GameObject Box(string n,Vector3 p,Vector3 s,Color c)=>Shape(n,PrimitiveType.Cube,p,s,c);
        static GameObject Cylinder(string n,Vector3 p,Vector3 s,Color c,Quaternion q)
        {var go=Shape(n,PrimitiveType.Cylinder,p,s,c);go.transform.rotation=q;return go;}
        static GameObject Sphere(string n,Vector3 p,Vector3 s,Color c)=>Shape(n,PrimitiveType.Sphere,p,s,c);
    }
    public sealed class WorldHotspot:MonoBehaviour { public string Id; }
    public sealed class BreachPulse:MonoBehaviour
    {
        public float Speed=1;
        private Vector3 basis;private Light point;
        void Awake(){basis=transform.localScale;point=GetComponent<Light>();}
        void Update(){float s=1+.08f*Mathf.Sin(Time.time*Speed*2);transform.localScale=basis*s;
            if(point!=null)point.intensity=Mathf.Max(.8f,point.intensity+Mathf.Sin(Time.time*Speed)*.001f);}
    }
}

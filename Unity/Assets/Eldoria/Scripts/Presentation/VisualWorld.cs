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
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = city?new Color(.80f,.82f,.81f):new Color(.76f,.75f,.72f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = city?new Color(.56f,.58f,.58f):new Color(.46f,.46f,.44f);
            RenderSettings.fogStartDistance=city?36:48; RenderSettings.fogEndDistance=city?108:140;
            var cameraGo = new GameObject("Isometric camera");
            var camera = cameraGo.AddComponent<Camera>(); camera.orthographic=true;
            camera.orthographicSize = city ? 9.7f : 14;
            camera.backgroundColor = RenderSettings.fogColor; camera.clearFlags=CameraClearFlags.SolidColor;
            cameraGo.tag="MainCamera";
            cameraGo.transform.position = city ? new Vector3(16.6f,16.8f,-23.2f) : new Vector3(20,24,-21);
            cameraGo.transform.LookAt(city ? new Vector3(0,2.25f,3.7f) : new Vector3(0,0,1));
            var sun = new GameObject("Valoria · amber dusk").AddComponent<Light>();
            sun.type=LightType.Directional; sun.color=city?new Color(1.0f,.80f,.62f):new Color(1.0f,.93f,.82f);
            sun.intensity=city?2.55f:1.9f;
            sun.transform.rotation=Quaternion.Euler(50,-32,0); sun.shadows=LightShadows.Soft; sun.shadowStrength=city?.48f:.55f;
            Box("World ground",new Vector3(0,-.7f,city?4:0),city?new Vector3(66,1.2f,62):new Vector3(34,1.2f,30),
                city?new Color(.27f,.29f,.26f):Earth);
            // Layered rock masses create a believable playable plateau instead of a flat board.
            for(int i=0;i<(city?0:11);i++)
            {
                float x=-15+i*3.05f;
                float h=1.4f+(i%4)*.55f;
                Box("Cliff rim",new Vector3(x,-.05f,12.2f-(i%2)*.55f),new Vector3(3.3f,h,3.1f),Stone*.68f);
            }
            for(int i=0;i<(city?0:8);i++)
            {
                var rock=Box("Old imperial road",new Vector3(-1.2f+(i%2)*.28f,-.02f,-12+i*3.15f),new Vector3(2.7f,.16f,1.7f),WarmStone*.78f);
                rock.transform.rotation=Quaternion.Euler(0,(i%2==0?7:-8),0);
            }
            if(city)
            {
                // Capture review: nearby imported terrain read as oversized clay blobs.
                // Keep only distant silhouettes; the playable city uses controlled terraces and rock edges.
                ValoriaKit.TerrainPiece("SM_Mountains_11","Valoria mountain backdrop west",
                    new Vector3(-18.0f,-2.4f,20.5f),new Vector3(.13f,.13f,.13f),Quaternion.Euler(0,18,0));
                ValoriaKit.TerrainPiece("SM_Mountains_11","Valoria mountain backdrop east",
                    new Vector3(16.5f,-2.6f,21.0f),new Vector3(.12f,.12f,.12f),Quaternion.Euler(0,-23,0));
                ValoriaKit.BrokenArch("Ruined imperial arch",new Vector3(-7.4f,.25f,15.4f),
                    5.15f,1.55f,ValoriaKit.OldStone*.74f);
                ValoriaKit.RockCluster("Ruined imperial arch fall",new Vector3(-3.2f,.04f,14.7f),1.45f,14);
                City(state);
            }
            else Frontier(state);
            if(city)ValoriaScar(new Vector3(11.8f,.05f,10.4f));
            else Rift(new Vector3(9,1,8));
        }
        static void City(PlayerState state)
        {
            var art=ValoriaExternalAssetLibrary.Load();

            // Break the giant repeating ground plane into irregular earth/moss fields.
            // These stay outside the central civic route so gameplay readability remains intact.
            for(int i=0;i<18;i++)
            {
                float angle=(i*137f+18f)*Mathf.Deg2Rad;
                float radius=12.2f+(i%4)*2.15f;
                var p=new Vector3(Mathf.Cos(angle)*radius,.012f,1.0f+Mathf.Sin(angle)*radius*.78f);
                var c=i%3==0?new Color(.19f,.23f,.17f):
                    i%3==1?new Color(.29f,.265f,.215f):new Color(.245f,.255f,.205f);
                IrregularGround("Valoria · outer terrain patch",p,5.4f+(i%3)*1.1f,4.0f+(i%4)*.75f,c);
                if(i%3==0)
                    ValoriaKit.BenchmarkPieceTinted("Valoria · outer boulder",art!=null?art.SlavicBoulder:null,
                        p+new Vector3((i%2==0?1.2f:-1.1f),.03f,.6f),1.7f+(i%2)*.45f,1.15f,
                        Quaternion.Euler(0,i*41%360,0),ValoriaKit.OldStone*.78f);
            }

            // Visual Bible production pass 02. Layout is now expressed through reusable modules
            // so authored prefabs can later replace them without changing gameplay coordinates.

            // Clifftop city terraces.
            ValoriaKit.Block("Valoria · upper terrace",new Vector3(0,.38f,4.0f),new Vector3(17,.78f,11),new Color(.31f,.30f,.27f));
            ValoriaKit.Block("Valoria · lower terrace",new Vector3(0,-.03f,-3.4f),new Vector3(19,.34f,7.0f),new Color(.29f,.27f,.22f));
            ValoriaKit.RockCluster("Valoria plateau edge west",new Vector3(-10.0f,-.25f,1.8f),1.25f,10);
            ValoriaKit.RockCluster("Valoria plateau edge east",new Vector3(10.0f,-.25f,2.5f),1.20f,10);
            for(int i=0;i<7;i++)
                ValoriaKit.Block("Cliff face",new Vector3(-9+i*3f,-.70f,-6.0f),new Vector3(3.15f,1.85f,1.55f),ValoriaKit.OldStone*.88f);
            for(int i=0;i<6;i++)
            {
                float x=-8.1f+i*3.25f;
                ValoriaKit.Block("Upper retaining rock",new Vector3(x,-.32f,9.3f+(i%2)*.25f),new Vector3(3.5f,1.1f,1.35f),ValoriaKit.OldStone*.80f);
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

            // Keep the perimeter visually continuous. Loose wall/tower prefabs were removed after capture review.
            ValoriaKit.Wall("Valoria west retaining wall",new Vector3(-8.2f,1.05f,3.7f),new Vector3(1.0f,2.0f,6.8f),ValoriaKit.OldStone*.92f,false);
            ValoriaKit.Wall("Valoria east retaining wall",new Vector3(8.2f,1.05f,3.7f),new Vector3(1.0f,2.0f,6.8f),ValoriaKit.OldStone*.92f,false);
            ValoriaKit.RockCluster("Old palace rubble west",new Vector3(-8.4f,.15f,6.2f),1.15f,10);
            ValoriaKit.RockCluster("Old palace rubble east",new Vector3(8.1f,.15f,6.6f),1.05f,9);

            // Dead-imperial rubble remains at the rear without eclipsing the living Bastion.
            ValoriaKit.RockCluster("Imperial collapse west",new Vector3(-7.4f,.02f,10.2f),1.25f,10);
            ValoriaKit.RockCluster("Imperial collapse east",new Vector3(7.0f,.02f,11.0f),1.15f,9);

            // Signature Bastion: fortress built inside a dead palace.
            ValoriaKit.BastionCore("Bastion",new Vector3(0,.78f,5.0f),Glow);

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
            ValoriaKit.Stair("Bastion stair",new Vector3(0,.28f,.55f),7,2.9f,.16f,.42f,ValoriaKit.WarmStone*.74f);
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

            // The imported Slavic vegetation was visually incompatible in URP (white/yellow blow-out).
            // Use one restrained dark-pine family until a production vegetation set is selected.
            for(int i=0;i<34;i++)
            {
                float z=-11f+(i*19%31)*.92f;
                float x=(i%2==0?-1f:1f)*(9.6f+(i*7%8)*.62f);
                ValoriaKit.PineTree("Valoria pine",new Vector3(x,0,z),.72f+(i%4)*.08f);
            }
            for(int i=0;i<12;i++)
            {
                float z=-7.5f+(i*11%19)*.82f;
                float x=(i%2==0?-1f:1f)*(7.9f+(i*5%5)*.42f);
                ValoriaKit.PineTree("Valoria inner pine",new Vector3(x,0,z),.58f+(i%3)*.07f);
            }
            ValoriaKit.RockCluster("Valoria roadside rocks",new Vector3(-3.7f,.05f,-1.35f),.58f,4);
            ValoriaKit.RockCluster("Valoria barracks rocks",new Vector3(7.5f,.05f,-1.15f),.52f,4);

            Hero(new Vector3(-1.7f,0,-1.9f),1.0f);
            int visibleArchers=state.BastionLevel>=2?4:3;
            for(int i=0;i<visibleArchers;i++)Archer(new Vector3(2.0f+(i%4)*.67f,0,-2.8f+(i/4)*.68f));
            Glow("Gate torch L",new Vector3(-2.85f,2.0f,-4.4f),Amber,1.25f,3.0f);
            Glow("Gate torch R",new Vector3(2.85f,2.0f,-4.4f),Amber,1.25f,3.0f);
            Glow("Bastion inhabited warmth",new Vector3(0,3.65f,3.2f),Amber,1.35f,6.5f);
        }
        static void Frontier(PlayerState state)
        {
            for(int i=0;i<15;i++)
            {
                float x=-15+(i*37%29),z=-11+(i*13%25);
                if(x>-3 && x<3) continue;
                Tree(new Vector3(x,0,z),i%3==0);
            }
            Box("Forest reserve",new Vector3(-6,.1f,1),new Vector3(4.6f,.32f,4),new Color(.15f,.22f,.19f));
            var grove=Cylinder("Bosque de Valoria · recolectar",new Vector3(-6,1.4f,1),new Vector3(2,2.8f,2),new Color(.20f,.28f,.23f),Quaternion.identity);
            grove.AddComponent<WorldHotspot>().Id="forest-valoria";
            for(int i=0;i<5;i++) Tree(new Vector3(-8+(i%3)*1.4f,0,-.1f+(i/3)*2),true);
            string enemyId=state.BastionLevel>=2?"engendro-valoria":"corrupt-scout";
            bool defeated=state.BastionLevel>=2?state.EngendroDefeated:state.ScoutDefeated;
            var enemy=Sphere(state.BastionLevel>=2?"Engendro de la Brecha":"Explorador corrupto",
                new Vector3(5,1,3),state.BastionLevel>=2?new Vector3(1.75f,2.55f,1.75f):new Vector3(1.35f,2.1f,1.35f),
                defeated?Stone*.5f:Violet*(state.BastionLevel>=2?.66f:.52f));
            enemy.AddComponent<WorldHotspot>().Id=enemyId;
            if(state.BastionLevel>=2 && !defeated)
            {
                Box("Engendro carapace",new Vector3(5,1.35f,3),new Vector3(2.1f,.55f,1.55f),Deep*.75f);
                Glow("Engendro corruption core",new Vector3(5,1.45f,2.55f),Violet,1.5f,4.2f);
            }
            Box("Broken watchpost",new Vector3(5,.8f,4.6f),new Vector3(2.5f,1.7f,1.3f),Stone*.65f);
            Hero(new Vector3(0,0,-6),.9f);
            for(int i=0;i<4;i++) Archer(new Vector3(-1.3f+i*.75f,0,-7));
            if(state.March.Phase!="idle")
            {
                var marker=Sphere("March signal",new Vector3(0,.55f,-2),new Vector3(.9f,.25f,.9f),Amber);
                marker.AddComponent<BreachPulse>().Speed=1.5f;
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
        {
            const int sides=11;
            var vertices=new Vector3[sides+1];var triangles=new int[sides*3];
            vertices[0]=Vector3.zero;
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides;
                float wobble=.82f+(i*19%7)*.055f;
                vertices[i+1]=new Vector3(Mathf.Cos(a)*width*.5f*wobble,0,Mathf.Sin(a)*depth*.5f*wobble);
                triangles[i*3]=0;triangles[i*3+1]=(i+1)%sides+1;triangles[i*3+2]=i+1;
            }
            var go=new GameObject(name);go.transform.position=center;
            var mesh=new Mesh { name=name+" mesh",vertices=vertices,triangles=triangles };
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh=mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial=Mat(color);
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

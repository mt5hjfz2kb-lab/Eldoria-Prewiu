// ISOLATED FIRST VALORIA LAYERS: preserved master-plan geometry, not canonical player Valoria.
// Output remains GREYBOX / SUPPORT; does not edit SHARP scene, gameplay, PlayerState or LocalGateway.
// Unity 6000.3.23f1, URP 17.3. -executeMethod Eldoria.ValoriaLayered.Editor.ValoriaLayeredMasterSceneBuilder.Build
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.ValoriaLayered.Editor
{
    public static class ValoriaLayeredMasterSceneBuilder
    {
        const string Root = "Assets/Eldoria/ArtTests/ValoriaLayeredConstruction";
        const string LayoutPath = Root + "/Layout/certified-legacy-skeleton.json";
        const string GeneratedPath = Root + "/Generated";
        const string ScenePath = Root + "/Valoria_MasterPlan_Layers_ABC.unity";
        const string ReferenceSha = "8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689";
        static readonly string EvidenceDir = Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", "..", "docs", "evidence", "valoria-layered-construction-20261016"));
        static Material soil, road, plot, upper, stone, reserve, water, timber;
        static Transform root, layerA, layerB, layerC, support;
        static Camera camera;
        static Plan plan;
        static int zoneCount, stairCount, gatePartCount;
        [Serializable] sealed class Plan
        {
            public string classification, units;
            public int expected_footprint_count;
            public Footprint[] footprints;
            public Stair stair;
            public QaCamera approved_qa_camera;
        }
        [Serializable] sealed class Footprint
        {
            public string id, kind;
            public float[] center, size;
            public int source_line;
        }
        [Serializable] sealed class Stair
        {
            public int count;
            public float[] first_center;
            public float rise, run, approx_width, step_height, step_depth;
        }
        [Serializable] sealed class QaCamera
        {
            public float[] position, target, zoom_sizes;
        }
        [Serializable] sealed class Result
        {
            public string scene, unity, classification, source_layout, camera_authority, visual_authority;
            public string bridge_status, gameplay_status, art_status;
            public bool source_topology_preserved, bridge_continuous_geometry, is_canonical, physical_mobile_tested;
            public int footprints, stairs, gate_greybox_parts, raycasts_passed;
            public float bridge_deck_top, bank_top, entry_road_top, stair_first_top, stair_last_top, upper_floor_top;
        }
        [MenuItem("Eldoria/Valoria/Build isolated master-plan layers ABC")]
        public static void Build()
        {
            try
            {
                BuildInternal();
                Debug.Log("VALORIA_LAYERED_MASTER_ABC_COMPLETE");
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("VALORIA_LAYERED_MASTER_ABC_FAILED " + e);
                EditorApplication.Exit(1);
            }
        }
        static void Check(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("VALORIA_MASTER_GATE: " + message);
        }
        static Vector3 V(float[] p) { Check(p != null && p.Length == 3, "Expected XYZ"); return new Vector3(p[0],p[1],p[2]); }
        static Material Mat(string name, Color color)
        {
            var path = GeneratedPath + "/" + name + ".mat";
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            m.name = name;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            else m.color = color;
            AssetDatabase.CreateAsset(m,path);
            return m;
        }
        static Transform Group(string name,Transform parent)
        {
            var t=new GameObject(name).transform;
            t.SetParent(parent,false);
            return t;
        }
        static GameObject Box(string name,Vector3 centre,Vector3 scale,Material mat,Transform parent,bool visible=true,bool trigger=false)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name=name;
            g.transform.SetParent(parent,false);
            g.transform.position=centre;
            g.transform.localScale=scale;
            var renderer=g.GetComponent<MeshRenderer>();
            renderer.sharedMaterial=mat;
            renderer.enabled=visible;
            var collider=g.GetComponent<BoxCollider>();
            collider.isTrigger=trigger;
            return g;
        }
        static Footprint Get(string id)
        {
            foreach (var f in plan.footprints) if (f.id == id) return f;
            throw new InvalidOperationException("Missing certified footprint: "+id);
        }
        static float Top(Footprint f) {return f.center[1]+f.size[1]*.5f;}
        static void BuildInternal()
        {
            Check(File.Exists(LayoutPath), "Missing recovered layout");
            plan=JsonUtility.FromJson<Plan>(File.ReadAllText(LayoutPath));
            Check(plan != null && plan.footprints != null && plan.footprints.Length == 22, "Expected 22 source footprints");
            Check(plan.expected_footprint_count == 22 && plan.stair.count == 12,"Source footprint / stair count mismatch");
            Check(plan.classification == "LEGACY_QA_CERTIFIED_TOPOLOGY_NOT_CANONICAL_SHARP_GEOMETRY","Invalid source authority");
            Check(Mathf.Abs(Get("VPD · master west district").center[0]+16f)<.001f &&
                  Mathf.Abs(Get("VPD · master east district").center[0]-16f)<.001f,"Expansion reserves changed");
            Check(Mathf.Abs(Get("VPD · L1 landing").center[1]-2.55f)<.001f,"Upper topology changed");
            Directory.CreateDirectory(GeneratedPath);
            Directory.CreateDirectory(EvidenceDir);
            if (AssetDatabase.IsValidFolder(GeneratedPath)) AssetDatabase.DeleteAsset(GeneratedPath);
            Directory.CreateDirectory(GeneratedPath);
            AssetDatabase.Refresh();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            root=Group("VALORIA / MASTER PLAN / isolated and greybox ONLY",null);
            layerA=Group("LAYER A - terrain and supports",root);
            layerB=Group("LAYER B - roads staircase reversible bridge",root);
            layerC=Group("LAYER C - architectural blockout, not production art",root);
            support=Group("SUPPORT - plot volumes and technical colliders",root);
            soil=Mat("ground_greybox",new Color(.33f,.34f,.25f));
            road=Mat("road_greybox",new Color(.57f,.52f,.43f));
            plot=Mat("plot_reserves_greybox",new Color(.47f,.47f,.38f));
            upper=Mat("upper_terrace_greybox",new Color(.40f,.43f,.33f));
            stone=Mat("stone_volume_greybox",new Color(.53f,.54f,.51f));
            reserve=Mat("future_reserve_greybox",new Color(.36f,.38f,.33f));
            water=Mat("river_provisional",new Color(.10f,.27f,.36f));
            timber=Mat("timber_greybox",new Color(.34f,.24f,.15f));
            BuildGroundAndExistingZones();
            BuildCirculationAndBridge();
            BuildGateAndMassing();
            BuildCameraAndLighting();
            Physics.SyncTransforms();
            int raycasts=CheckRouteRaycasts();
            AssetDatabase.SaveAssets();
            Check(EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),ScenePath,true),"Failed to save actual .unity scene");
            Capture("master-home-19",19,1536,1024);
            Capture("master-home-12",12,1536,1024);
            Capture("master-home-9",9,1536,1024);
            Capture("master-portrait-12",12,390,844);
            Capture("master-west-pan-12",12,1280,720,new Vector3(-13.5f,0,0));
            Capture("master-east-pan-12",12,1280,720,new Vector3(13.5f,0,0));
            var route=Get("VPD · lower entry apron");
            var landing=Get("VPD · L1 landing");
            var lastY=plan.stair.first_center[1]+(plan.stair.count-1)*plan.stair.rise+plan.stair.step_height*.5f;
            var result=new Result {
                scene=ScenePath,unity=Application.unityVersion,
                classification="GREYBOX / SUPPORT / TEMPORARY",
                source_layout=LayoutPath,camera_authority="HISTORICAL_ORTHOGRAPHIC_QA_NOT_SHARP",
                visual_authority=ReferenceSha,bridge_status="PROVISIONAL_NOT_CERTIFIED",
                gameplay_status="NOT_TESTED_NO_FAKE_HOTSPOTS",art_status="NOT_APPROVED",
                source_topology_preserved=true,bridge_continuous_geometry=true,
                is_canonical=false,physical_mobile_tested=false,
                footprints=zoneCount,stairs=stairCount,gate_greybox_parts=gatePartCount,
                raycasts_passed=raycasts,
                bridge_deck_top=.38f,bank_top=.38f,entry_road_top=Top(route),
                stair_first_top=plan.stair.first_center[1]+plan.stair.step_height*.5f,
                stair_last_top=lastY,upper_floor_top=Top(landing)
            };
            File.WriteAllText(Path.Combine(EvidenceDir,"first-native-gate.json"),JsonUtility.ToJson(result,true));
            Debug.Log("VALORIA_LAYERED_ABC_NATIVE_SOURCE zones="+zoneCount+" stairs="+stairCount+" rays="+raycasts+
                      " upper="+Top(landing)+" first="+result.stair_first_top+" last="+lastY);
        }
        static void BuildGroundAndExistingZones()
        {
            // The certified floors/colliders are taken verbatim from the legacy QA source.
            // A larger lower rock substrate below them provides actual support, not a floating display slab.
            Box("GREYBOX continuous rocky substrate",new Vector3(0,-.37f,8),new Vector3(57f,.86f,42f),soil,layerA);
            Box("SUPPORT upper civic underfill",new Vector3(0,1.14f,15.4f),new Vector3(13.5f,2.24f,7f),stone,layerA);
            Box("SUPPORT civic link underfill",new Vector3(0,1.16f,11.2f),new Vector3(4f,2.30f,4f),stone,layerA);
            foreach(var f in plan.footprints)
            {
                Transform parent=layerA;Material m=soil;
                bool visible=true,trigger=false;
                if(f.kind=="gate_hotspot_volume"){parent=support;visible=false;trigger=true;m=stone;}
                else if(f.kind=="support"){parent=layerA;visible=true;m=stone;}
                else if(f.kind.StartsWith("route")){parent=layerB;m=road;}
                else if(f.kind.StartsWith("plot")){parent=support;trigger=true;m=plot;}
                else if(f.kind.StartsWith("reserve")){parent=support;trigger=true;m=f.kind=="reserve_upper"?upper:reserve;}
                else if(f.kind=="floor_upper"){m=upper;}
                Box((trigger?"SUPPORT/RESERVE ":"GREYBOX ")+f.id,V(f.center),V(f.size),m,parent,visible,trigger);
                zoneCount++;
            }
            // The upper landing and its two small plots retain the exact certified Y.
            // Existing source underfloor support blocks were copied above, so the L1 plaza is not floating.
            Box("PROVISIONAL river under gate approach",new Vector3(0,-1.90f,-16f),
                new Vector3(35f,.12f,6.1f),water,layerA);
            Box("TEMPORARY far shore ground, noncertified extension",new Vector3(0,-.32f,-23.1f),
                new Vector3(18f,1.4f,7f),soil,layerA);
        }
        static void BuildCirculationAndBridge()
        {
            // Exactly 12 certified steps; the original source defines all centres in a loop.
            for(int i=0;i<plan.stair.count;i++)
            {
                var centre=new Vector3(plan.stair.first_center[0],plan.stair.first_center[1]+i*plan.stair.rise,
                                       plan.stair.first_center[2]+i*plan.stair.run);
                Box("GREYBOX certified stair step "+(i+1),centre,
                   new Vector3(plan.stair.approx_width,plan.stair.step_height,plan.stair.step_depth),
                   road,layerB);
                stairCount++;
            }
            // Bridge coordinates NOT part of certified original: isolated/reversible example only.
            // Both ends meet genuine horizontal walkable surfaces at Y=0.38 with 0 height jump.
            const float top=.38f,thickness=.28f,nearZ=-9.35f,farZ=-19.65f;
            Box("TEMPORARY bridge / continuous deck and collider",
                new Vector3(0,top-thickness*.5f,(nearZ+farZ)*.5f),
                new Vector3(4.5f,thickness,nearZ-farZ),road,layerB);
            Box("TEMPORARY far shore approach top flush with bridge",
                new Vector3(0,top-.10f,-21.2f),new Vector3(5f,.2f,3.2f),road,layerB);
            for(int side=-1;side<=1;side+=2)
            {
                Box("GREYBOX bridge parapet "+side,new Vector3(side*2.08f,.68f,-14.5f),
                    new Vector3(.35f,.6f,10.3f),stone,layerB);
                for(int z=-19;z<=-10;z+=3)
                    Box("GREYBOX bridge pier "+side+" z"+z,new Vector3(side*1.8f,-.78f,z),
                        new Vector3(.9f,2.32f,.9f),stone,layerA);
            }
            var entry=Get("VPD · lower entry apron");
            Check(Mathf.Abs(Top(entry)-.37f)<.001f,"Entry apron moved");
            Check(Mathf.Abs(top-(entry.center[1]+entry.size[1]*.5f))<.03f,"Bridge vs road discontinuity");
            Check(Mathf.Abs((top-.1f+.2f*.5f)-top)<.001f,"Far bank bridge height discontinuity");
            Check(nearZ>=entry.center[2]-entry.size[2]*.5f-.01f,"Bridge misses certified apron");
        }
        static void BuildGateAndMassing()
        {
            // All architecture in this phase is explicitly GREYBOX, even if it resembles masonry.
            // Do not mark it PRODUCTION ART SOURCE or attach fake WorldHotspots.
            for(int side=-1;side<=1;side+=2)
            {
                Box("GREYBOX lower gate tower "+side,new Vector3(side*3.30f,2.0f,-9.25f),
                    new Vector3(2.55f,4f,2.55f),stone,layerC);gatePartCount++;
                Box("GREYBOX lower gate wall "+side,new Vector3(side*6.70f,1.45f,-9.25f),
                    new Vector3(4.6f,2.9f,1.3f),stone,layerC);gatePartCount++;
                Box("GREYBOX gate base "+side,new Vector3(side*3.3f,.14f,-9.25f),
                    new Vector3(3f,.28f,2.85f),stone,layerC);gatePartCount++;
            }
            Box("GREYBOX gate arch lintel / passable below",new Vector3(0,4.20f,-9.25f),
                new Vector3(4.1f,.8f,2.3f),stone,layerC);gatePartCount++;
            // Non-physical gate leaf is a visible placeholder. Selection of city->Frontier
            // still belongs exclusively to the existing game's hotspot/collider authority.
            var leaf=Box("TEMPORARY gate leaf (visual only, no collider)",
                new Vector3(0,1.48f,-9.65f),new Vector3(2.7f,2.9f,.18f),timber,layerC);
            leaf.GetComponent<Collider>().enabled=false;gatePartCount++;
            Box("GREYBOX Bastion hero reservation, silhouette only",new Vector3(0,4.48f,7.25f),
                new Vector3(5.5f,3.5f,4.2f),stone,layerC);
            Box("GREYBOX upper fortified entry reservation",new Vector3(0,4.10f,10.8f),
                new Vector3(5.3f,2.8f,1.1f),stone,layerC);
        }
        static void BuildCameraAndLighting()
        {
            camera=new GameObject("ORTHOGRAPHIC HISTORICAL QA CAMERA - NOT SHARP runtime").AddComponent<Camera>();
            camera.transform.SetParent(root,false);
            camera.transform.position=V(plan.approved_qa_camera.position);
            camera.transform.LookAt(V(plan.approved_qa_camera.target));
            camera.orthographic=true;
            camera.orthographicSize=19f;
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.27f,.37f,.45f);
            camera.nearClipPlane=.1f;camera.farClipPlane=300f;
            camera.tag="MainCamera";
            var sun=new GameObject("GREYBOX viewing light").AddComponent<Light>();
            sun.transform.SetParent(root,false);
            sun.type=LightType.Directional;
            sun.intensity=1.2f;
            sun.shadows=LightShadows.Soft;
            sun.transform.rotation=Quaternion.Euler(50,210,0);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.60f,.62f,.65f);
            RenderSettings.ambientGroundColor=new Color(.30f,.29f,.25f);
        }
        static int CheckRouteRaycasts()
        {
            var probes=new List<Vector3> {
                new Vector3(0,.38f,-20.8f),new Vector3(0,.38f,-18f),
                new Vector3(0,.38f,-14f),new Vector3(0,.38f,-10.3f),
                new Vector3(0,.38f,-8.35f),new Vector3(0,.38f,-3f),
                new Vector3(0,2.70f,6.7f)
            };
            for(int i=0;i<plan.stair.count;i++)
                probes.Add(new Vector3(0,plan.stair.first_center[1]+plan.stair.rise*i+plan.stair.step_height*.5f,
                    plan.stair.first_center[2]+plan.stair.run*i));
            int passed=0;
            foreach(var p in probes)
            {
                RaycastHit hit;
                // Ignore plot trigger reservations: they are not traversable ground.
                Check(Physics.Raycast(p+Vector3.up*2,Vector3.down,out hit,4f,~0,QueryTriggerInteraction.Ignore),
                      "Missing collider under certified/temporary circulation at "+p);
                Check(Mathf.Abs(hit.point.y-p.y)<.28f,"Unstable route surface at "+p+" hit="+hit.point);
                passed++;
            }
            return passed;
        }
        static void Capture(string name,float size,int width,int height,Vector3 shift=default(Vector3))
        {
            var oldPos=camera.transform.position;float oldSize=camera.orthographicSize;var oldRt=camera.targetTexture;
            var active=RenderTexture.active;
            var rt=new RenderTexture(width,height,24);
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.transform.position+=shift;
                camera.orthographicSize=size;
                camera.targetTexture=rt;
                camera.Render();
                RenderTexture.active=rt;
                texture.ReadPixels(new Rect(0,0,width,height),0,0);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(EvidenceDir,name+".png"),texture.EncodeToPNG());
            }
            finally
            {
                camera.transform.position=oldPos;camera.orthographicSize=oldSize;
                camera.targetTexture=oldRt;
                RenderTexture.active=active;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}

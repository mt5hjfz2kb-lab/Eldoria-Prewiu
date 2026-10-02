using System;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Integrated whole-frame comparison over the current strongest stack.
    public static class ValoriaStrongestCompositeGateV2
    {
        const string Folder="ValoriaStrongestCompositeV2Captures";

        static readonly (string id,string path,Vector3 p,float yaw,float span,float height)[] PremiumSpecs={
            ("town_house_01","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_01a_PRE.prefab",new Vector3(-7.45f,.42f,-2.55f),12f,2.75f,3.35f),
            ("town_house_02","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_02a_PRE.prefab",new Vector3(7.35f,.42f,-3.05f),190f,2.70f,3.25f),
            ("town_house_03c","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03c_PRE.prefab",new Vector3(-5.25f,.72f,2.95f),18f,2.35f,2.85f),
            ("admin_01a","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Administrative/EA03_Town_Building_Administrative _01a_PRE.prefab",new Vector3(5.15f,.72f,3.00f),174f,2.45f,2.90f),
            ("town_house_03a","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03a_PRE.prefab",new Vector3(-4.15f,1.48f,5.15f),12f,2.15f,2.65f),
            ("town_house_03b","Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/EA03_Town_House_Comp_03b_PRE.prefab",new Vector3(4.10f,1.48f,5.20f),188f,2.15f,2.65f)
        };

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;
            ProductionVisualIntegration.TerrainTerraceEnabled=true;
            ProductionVisualIntegration.SurfaceCellEnabled=false;
            ProductionVisualIntegration.ProductionCellEnabled=false;
            ProductionVisualIntegration.CoherentCastleProofEnabled=false;
            ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
            ProductionVisualIntegration.CompactFootprintReframeEnabled=true;

            AssetVisualUpliftPassV1.Enabled=false;
            AssetLibraryReprocessingPassV1.Enabled=true;
            MidTierDistrictProduction.Enabled=true;
            ValoriaFullFrameArchitectureBatchV1.Enabled=true;
            ValoriaFullFrameForegroundEdgePassV1.Enabled=true;
            ValoriaOpenValleyCompositionV1.Enabled=true;
            ValoriaReferenceConvergencePassV2.Enabled=false;
            ValoriaInCitySurfacePassV1.Enabled=false;
            ValoriaStairLandingIntegrationV1.Enabled=false;
            ValoriaFullFrameConvergenceIteration1.Enabled=false;
            ValoriaFullFrameConvergenceIteration2.Enabled=false;
            ValoriaBenchmarkCompositeV2.Enabled=false;
            ValoriaEnvironmentUpliftV1.Enabled=false;
            ValoriaArchitectureCoherenceV1.Enabled=false;
            ValoriaBackplateCandidateV1.Enabled=false;
            ValoriaCliffIslandReframeV1.Enabled=false;
            ValoriaCliffIslandCleanupV2.Enabled=false;
            ValoriaResidualCleanupV1.Enabled=false;
            ValoriaMaterialResidueCleanupV2.Enabled=false;
            ValoriaFullFrameArtifactCleanupV1.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var c=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(c==null||root==null)throw new Exception("Valoria capture prerequisites missing.");

            var p=new Vector3(18.2f,14.6f,-25.8f);
            var t=new Vector3(0f,3.35f,5.6f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            BuildStrongestBase(root.transform,state,c);
            SaveSet(c,"before",p,t);

            int suppressed=SuppressDisconnectedResidue();
            int premium=ReplaceSecondaryArchitecture(root.transform);
            int ruins=AddBuriedSideRuins(root.transform);

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Strongest Composite v2 altered gameplay signature.");

            SaveSet(c,"after",p,t);
            File.WriteAllText(Folder+"/evidence.json",$"{{\n"+
                $"  \"collider_hotspot_signature_equal\": true,\n"+
                $"  \"disconnected_renderers_suppressed\": {suppressed},\n"+
                $"  \"premium_secondary_loaded\": {premium},\n"+
                $"  \"buried_side_ruin_pieces\": {ruins},\n"+
                $"  \"background\": \"Kiara 3 Morning CC0\",\n"+
                $"  \"existing_assets_only\": true,\n"+
                $"  \"tripo_credits\": 0\n"+
                $"}}\n");

            Debug.Log("VALORIA_STRONGEST_COMPOSITE_V2_GATE=PASS");
            EditorApplication.Exit(0);
        }

        static void BuildStrongestBase(Transform root,PlayerState state,Camera c)
        {
            ValoriaBenchmarkCompositeV2.Enabled=true;ValoriaBenchmarkCompositeV2.Build(root,state);
            ValoriaEnvironmentUpliftV1.Enabled=true;ValoriaEnvironmentUpliftV1.Build(root,state);
            ValoriaArchitectureCoherenceV1.Enabled=true;ValoriaArchitectureCoherenceV1.Build(root,state);
            ValoriaCliffIslandReframeV1.Enabled=true;ValoriaCliffIslandReframeV1.Build(root,state);
            ValoriaBackplateCandidateV1.Enabled=true;
            if(!ValoriaBackplateCandidateV1.Build(root,c,"kiara3_1"))throw new Exception("Kiara 3 backplate unavailable.");
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
            ValoriaCliffIslandCleanupV2.Enabled=true;ValoriaCliffIslandCleanupV2.Build(root,state);
            ValoriaResidualCleanupV1.Enabled=true;ValoriaResidualCleanupV1.Build(root,state);
            ValoriaMaterialResidueCleanupV2.Enabled=true;ValoriaMaterialResidueCleanupV2.Build(root,state);
            ValoriaFullFrameArtifactCleanupV1.Enabled=true;ValoriaFullFrameArtifactCleanupV1.Build(root,state);
        }

        static int SuppressDisconnectedResidue()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                string chain=Chain(r.transform);

                if(chain.Contains("backplate")||chain.Contains("bastion")||
                   chain.Contains("aserradero")||chain.Contains("cuartel")||
                   chain.Contains("granary")||chain.Contains("granero"))continue;

                bool presentation=chain.Contains("valoria ·")||chain.Contains("vpd ·")||
                    chain.Contains("mid-tier")||chain.Contains("assetlibrary");
                if(!presentation)continue;

                bool farOuter=Mathf.Abs(b.center.x)>9.6f||b.center.z>10.9f||b.center.z<-8.2f;
                bool detachedSmall=Mathf.Abs(b.center.x)>8.3f &&
                    b.size.y<4.5f && Mathf.Max(b.size.x,b.size.z)<6.5f;

                if(farOuter||detachedSmall)
                {
                    r.enabled=false;
                    count++;
                }
            }
            return count;
        }

        static int ReplaceSecondaryArchitecture(Transform root)
        {
            HideFamily("Valoria · Mid-Tier District v1 · production visual only");
            HideFamily("Valoria · Full Frame Architecture Batch v1");

            var proof=new GameObject("Valoria · Strongest v2 · premium secondary family").transform;
            proof.SetParent(root,true);
            int loaded=0;

            foreach(var s in PremiumSpecs)
            {
                var src=AssetDatabase.LoadAssetAtPath<GameObject>(s.path);
                if(src==null)continue;
                var go=(GameObject)PrefabUtility.InstantiatePrefab(src);
                if(go==null)continue;
                go.name="Valoria · Strongest v2 · premium secondary · "+s.id;
                go.transform.rotation=Quaternion.Euler(0f,s.yaw,0f);
                Fit(go,s.p,s.span,s.height);
                Neutralize(go);
                go.transform.SetParent(proof,true);
                DisableGameplay(go);
                loaded++;
            }
            return loaded;
        }

        static int AddBuriedSideRuins(Transform root)
        {
            var art=ValoriaExternalAssetLibrary.Load();
            if(art==null)return 0;
            var proof=new GameObject("Valoria · Strongest v2 · buried side ruins").transform;
            proof.SetParent(root,true);
            int count=0;

            count+=Add(proof,art.MegaHalfGate,"west half arch",new Vector3(-8.45f,.32f,6.00f),2.55f,3.90f,28f,new Color(.58f,.56f,.51f,1f));
            count+=Add(proof,art.MegaDestroyedTower,"west broken tower",new Vector3(-9.20f,.18f,7.35f),1.90f,3.35f,42f,new Color(.52f,.51f,.48f,1f));
            count+=Add(proof,art.MegaHalfGate,"east half arch",new Vector3(8.55f,.30f,6.10f),2.60f,4.00f,205f,new Color(.58f,.56f,.51f,1f));
            count+=Add(proof,art.MegaDestroyedTower,"east broken tower",new Vector3(9.25f,.16f,7.45f),1.85f,3.25f,222f,new Color(.52f,.51f,.48f,1f));

            var rock=Resources.Load<GameObject>("WorldInventory/Rock02");
            if(rock!=null)
            {
                count+=Add(proof,rock,"west burial",new Vector3(-8.70f,.06f,5.55f),2.10f,.90f,18f,new Color(.39f,.39f,.35f,1f));
                count+=Add(proof,rock,"east burial",new Vector3(8.80f,.06f,5.65f),2.10f,.90f,198f,new Color(.39f,.39f,.35f,1f));
            }
            return count;
        }

        static int Add(Transform root,GameObject source,string role,Vector3 ground,float footprint,float maxHeight,float yaw,Color tint)
        {
            if(source==null)return 0;
            var go=ValoriaKit.BenchmarkPieceIntegrated("Valoria · Strongest v2 · "+role,source,ground,footprint,maxHeight,
                Quaternion.Euler(0f,yaw,0f),tint);
            if(go==null)return 0;
            go.transform.SetParent(root,true);
            DisableGameplay(go);
            return 1;
        }

        static void HideFamily(string rootName)
        {
            var go=GameObject.Find(rootName);
            if(go==null)return;
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.enabled=false;
            foreach(var l in go.GetComponentsInChildren<Light>(true))l.enabled=false;
        }

        static void Fit(GameObject go,Vector3 ground,float span,float maxHeight)
        {
            var rs=go.GetComponentsInChildren<Renderer>(true);
            if(rs.Length==0)return;
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            float scale=Mathf.Min(span/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z)),
                                  maxHeight/Mathf.Max(.001f,b.size.y));
            go.transform.localScale*=scale;
            rs=go.GetComponentsInChildren<Renderer>(true);
            b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            go.transform.position+=ground-new Vector3(b.center.x,b.min.y,b.center.z);
        }

        static void Neutralize(GameObject go)
        {
            foreach(var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var src=r.sharedMaterials;var dst=new Material[src.Length];
                for(int i=0;i<src.Length;i++)
                {
                    if(src[i]==null){dst[i]=null;continue;}
                    var m=new Material(src[i]){name="Valoria Strongest v2 · "+src[i].name};
                    string n=(r.name+" "+src[i].name).ToLowerInvariant();
                    Color tint=(n.Contains("roof")||n.Contains("tile")||n.Contains("shingle"))
                        ?new Color(.34f,.37f,.38f,1f)
                        :(n.Contains("wood")||n.Contains("beam")||n.Contains("timber"))
                            ?new Color(.36f,.28f,.21f,1f)
                            :new Color(.67f,.63f,.55f,1f);
                    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",tint);
                    else if(m.HasProperty("_Color"))m.SetColor("_Color",tint);
                    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.035f);
                    dst[i]=m;
                }
                r.sharedMaterials=dst;
            }
        }

        static void DisableGameplay(GameObject go)
        {
            foreach(var c in go.GetComponentsInChildren<Collider>(true))c.enabled=false;
            foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
            foreach(var b in go.GetComponentsInChildren<MonoBehaviour>(true))if(!(b is WorldHotspot))b.enabled=false;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
        }

        static void SaveSet(Camera c,string tag,Vector3 p,Vector3 t)
        {
            Save(c,Folder+"/"+tag+"-19.png",p,t,19f,1280,720);
            Save(c,Folder+"/"+tag+"-12.png",p,t,12f,1280,720);
            Save(c,Folder+"/"+tag+"-9.png",p,t,9f,1280,720);
            Save(c,Folder+"/"+tag+"-production.png",p,t,9.1f,1280,720);
            ValoriaBackplateCandidateV1.FitAspect(390f/844f);
            Save(c,Folder+"/"+tag+"-mobile.png",p,t,9.1f,390,844);
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
        }

        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try{
                c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
                File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
            } finally {
                c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>
    /// Builds a THIRD, isolated premium-visual TEST scene for Granero mobile readability. Does NOT overwrite
    /// the original City2D scene, production Valoria, saves or business rules.
    /// Uses existing CityBuilding tiers and read-only stage bridge unchanged.
    /// The Cantera is premium; its built state remains an isolated QA fixture.
    /// </summary>
    public static class City2DPremiumPreviewV3
    {
        const string Root="Assets/Experimentos/Ciudad2D/";
        const string OldScene=Root+"Experimento_Ciudad2D.unity";
        public const string PremiumScene=Root+"QA/Experimento_Ciudad2D_PremiumV3.unity";
        const string Output="Ciudad2DPremiumPlayableV3Evidence";

        [MenuItem("Eldoria/Experimentos/Ciudad2D/Crear preview jugable Premium V3 (Granero legible)")]
        public static void Build()
        {
            BuildCore(false);
        }

        [MenuItem("Eldoria/Experimentos/Ciudad2D/Probar preview Premium V3 Granero (NO aprobado)")]
        public static void Capture()
        {
            BuildCore(true);
        }

        static void BuildCore(bool capture)
        {
            var previous=EditorSceneManager.GetSceneManagerSetup();
            if(!UnityEngine.Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            try
            {
                if(UnityEngine.Application.isBatchMode)
                {
                    var scratch=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                    if(!EditorSceneManager.SaveScene(scratch,Root+"QA/PremiumPreviewV3Scratch.unity"))
                        throw new InvalidOperationException("Cannot save safe scratch scene");
                }
                City2DSceneBuilder.Rebuild();
                var source=EditorSceneManager.OpenScene(OldScene,OpenSceneMode.Single);
                if(!source.IsValid()||!source.isLoaded)
                    throw new InvalidOperationException("Original sandbox unavailable");
                if(AssetDatabase.LoadAssetAtPath<SceneAsset>(PremiumScene)!=null)
                    throw new InvalidOperationException("Never overwrite a previous premium scene version");
                if(!EditorSceneManager.SaveScene(source,PremiumScene,true))
                    throw new InvalidOperationException("Unable to copy the original City2D scene");
                var scene=EditorSceneManager.OpenScene(PremiumScene,OpenSceneMode.Single);
                var bridge=UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
                if(bridge==null||bridge.IsLiveBound)
                    throw new InvalidOperationException("No read-only premium scene bridge");
                var buildings=new Dictionary<string,CityBuilding>(StringComparer.Ordinal);
                foreach(var item in UnityEngine.Object.FindObjectsByType<CityBuilding>(
                    FindObjectsSortMode.None))
                    if(item.gameObject.scene==scene)buildings.Add(item.BuildingId,item);
                if(buildings.Count!=6)
                    throw new InvalidOperationException("Original six building interactions altered");

                Swap(buildings,"cuartel","Cuartel_Premium_V2.png",
                    "Cuartel_Premium_ShadowV2.png",1f);
                Swap(buildings,"aserradero","Aserradero_Premium_V2.png",
                    "Aserradero_Premium_V2_Shadow.png",2f);
                // Granero only: preserve original RGBA and independent shadow. Test 15%
                // larger silhouette and 0.23 leftward world offset in real Unity.
                // Candidate evaluation, not an accepted art change.\n                Swap(buildings,"granero","Granero_Premium_V1.png",
                    "Granero_Premium_V1_Shadow.png",2f,1.15f);
                Swap(buildings,"cantera","Cantera_Premium_V1.png",
                    "Cantera_Premium_V1_Shadow.png",2f);

                // Granary has no heating chimney. Avoid fake roof smoke.
                // Sawmill/Cuartel have baked smoke already: retain just one
                // subtle dynamic plume each, not two overlapping wisps.
                foreach(var id in new[]{"smoke_cuartel_2","smoke_aserradero_2",
                                        "smoke_granero_1","smoke_granero_2"})
                {
                    var obj=GameObject.Find("AmbienteVivo_"+id);
                    if(obj==null)throw new InvalidOperationException("Missing original smoke "+id);
                    obj.SetActive(false);
                }
                bridge.RebindSceneBuildings();
                bridge.SetDemoBastionLevel(1);
                if(bridge.RegisteredBuildingCount!=6||
                   bridge.RegisteredAmbientEffectCount!=6 ||
                   UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(FindObjectsSortMode.None).Length!=4)
                    throw new InvalidOperationException("Premium preview ambient plot binding corrupt");
                foreach(var gate in UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                    FindObjectsSortMode.None)) gate.Refresh();
                if(!EditorSceneManager.SaveScene(scene,PremiumScene))
                    throw new InvalidOperationException("Unable to save NEW premium scene");
                if(capture)CaptureEvidence(scene,bridge);
                Debug.Log("[Ciudad2D] PREMIUM_V3_GRANERO_READABILITY_CANDIDATE_NOT_APPROVED");
            }
            finally
            {
                if(!UnityEngine.Application.isBatchMode && previous!=null && previous.Length>0)
                    EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }

        static void Swap(Dictionary<string,CityBuilding> items,string id,
            string candidateName,string shadowName,float shadowScale,float mobileLegibilityScale=1f)
        {
            var owner=items[id];
            var renderer=owner.GetComponent<SpriteRenderer>();
            if(renderer==null)throw new InvalidOperationException("Missing original building renderer");
            var old=renderer.sprite;
            var originalScale=owner.transform.localScale;
            var originalPosition=owner.transform.position;
            string folder=Root+"Sprites/Candidates/";
            var art=AssetDatabase.LoadAssetAtPath<Sprite>(folder+candidateName);
            var shadow=AssetDatabase.LoadAssetAtPath<Sprite>(folder+shadowName);
            if(old==null||art==null||shadow==null)
                throw new InvalidOperationException("Premium source or shadow missing: "+id);
            var before=AlphaFoot(old);var after=AlphaFoot(art);
            float oldWidth=before.width/old.pixelsPerUnit*originalScale.x;
            float replacementScale=oldWidth/(after.width/art.pixelsPerUnit)*mobileLegibilityScale;
            float previousFoot=before.minimumY/old.pixelsPerUnit*originalScale.x;
            float newFoot=after.minimumY/art.pixelsPerUnit*replacementScale;
            owner.Configure(id,owner.DisplayName,1,
                new[]{new CityBuilding.VisualTier(1,art,replacementScale)});
            owner.transform.position=originalPosition+
                new Vector3(id=="granero" ? -0.23f : 0f,previousFoot-newFoot,0f);

            var obj=new GameObject("SombraPremium_"+id);
            obj.transform.position=owner.transform.position;
            obj.transform.localScale=Vector3.one*replacementScale*shadowScale;
            var sh=obj.AddComponent<SpriteRenderer>();
            sh.sprite=shadow;sh.sortingOrder=renderer.sortingOrder-1;
            obj.AddComponent<City2DPremiumShadowGate>().Configure(owner);
            if(obj.GetComponent<Collider2D>()!=null)
                throw new InvalidOperationException("Shadow blocks input");
            // The old foliage props concealed fake rectangular platforms.
            // Do not reuse them to disguise the new art.
            for(int i=0;i<2;i++)
            {
                var seam=GameObject.Find("NaturalizarPlinto_"+id+"_"+i);
                if(seam!=null)seam.SetActive(false);
            }
        }

        struct Bounds { public float width,minimumY; }
        static Bounds AlphaFoot(Sprite sprite)
        {
            string path=AssetDatabase.GetAssetPath(sprite);
            if(!path.StartsWith(Root,StringComparison.Ordinal))
                throw new InvalidOperationException("Non-City2D visual source");
            string physical=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                "..",path.Replace('/',Path.DirectorySeparatorChar)));
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                if(!ImageConversion.LoadImage(texture,File.ReadAllBytes(physical),false))
                    throw new InvalidOperationException("Cannot measure alpha foot: "+path);
                var px=texture.GetPixels32();
                int minX=texture.width,maxX=-1,bottom=texture.height;
                for(int y=0;y<texture.height;y++)
                for(int x=0;x<texture.width;x++)
                    if(px[y*texture.width+x].a>32)
                    {
                        minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);
                        bottom=Math.Min(bottom,y);
                    }
                if(maxX<minX)throw new InvalidOperationException("Alpha silhouette empty");
                return new Bounds{width=maxX-minX+1,minimumY=bottom};
            }
            finally{UnityEngine.Object.DestroyImmediate(texture);}
        }

        static void CaptureEvidence(Scene scene,City2DStateBridge bridge)
        {
            var camera=Camera.main;
            var fit=camera!=null?camera.GetComponent<City2DCameraFit>():null;
            if(camera==null||fit==null||!camera.orthographic)
                throw new InvalidOperationException("Premium capture lost ortho camera");
            var folder=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..",Output));
            Directory.CreateDirectory(folder);
            var report=Path.Combine(folder,"evidence.json");
            if(File.Exists(report))File.Delete(report);
            var records=new List<string>();
            foreach(int stage in new[]{1,3,5})
            {
                bridge.SetDemoBastionLevel(stage);
                foreach(var shadow in UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                    FindObjectsSortMode.None)) shadow.Refresh();
                var resolutions=stage==5
                    ?new[]{new[]{390,844},new[]{1170,2532},new[]{844,390}}
                    :new[]{new[]{390,844}};
                foreach(var size in resolutions)
                {
                    fit.SetEvidenceViewport(size[0],size[1]);
                    fit.SetEvidenceFocus(Vector2.zero);
                    string file="premium_bastion"+stage+"_"+size[0]+"x"+size[1]+".png";
                    var bytes=Render(camera,size[0],size[1]);
                    File.WriteAllBytes(Path.Combine(folder,file),bytes);
                    records.Add("{\"file\":\""+file+"\",\"sha256\":\""+Sha(bytes)+"\","+
                        "\"width\":"+size[0]+",\"height\":"+size[1]+",\"stage\":"+stage+"}");
                }
            }
            bridge.SetDemoBastionLevel(5);
            var premiumBuildings=new Dictionary<string,CityBuilding>(StringComparer.Ordinal);
            foreach(var item in UnityEngine.Object.FindObjectsByType<CityBuilding>(FindObjectsSortMode.None))
                if(item.gameObject.scene==scene) premiumBuildings[item.BuildingId]=item;
            foreach(var id in new[]{"cuartel","aserradero","granero","cantera"})
            {
                fit.SetEvidenceViewport(1170,2532);
                fit.SetEvidenceZoom(1.75f);
                fit.SetEvidenceFocus((Vector2)premiumBuildings[id].transform.position);
                string file="premium_bastion5_1170x2532_"+id+"_close.png";
                byte[] bytes=Render(camera,1170,2532);
                File.WriteAllBytes(Path.Combine(folder,file),bytes);
                records.Add("{\"file\":\""+file+"\",\"sha256\":\""+Sha(bytes)+"\","+
                    "\"width\":1170,\"height\":2532,\"stage\":5}");
            }
            // Canonical stage V keeps Cantera Available, not Built. Preserve
            // that true screenshot above. Add TWO clearly labeled QA frames
            // for the valid post-build state without changing progression.
            var quarry=premiumBuildings["cantera"];
            if(quarry.PlotPhase!=City2DPlotPhase.Available ||
               quarry.GetComponent<SpriteRenderer>().enabled)
                throw new InvalidOperationException("Stage V quarry must be Available before QA fixture");
            quarry.SetAppearance(new City2DPlotView(City2DPlotPhase.Built,1,5));
            foreach(var shade in UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                FindObjectsSortMode.None)) shade.Refresh();
            if(!quarry.GetComponent<SpriteRenderer>().enabled)
                throw new InvalidOperationException("Built quarry QA sprite did not appear");
            fit.SetEvidenceViewport(390,844);
            fit.SetEvidenceFocus(Vector2.zero);
            string builtFile="premium_bastion5_quarry_built_390x844.png";
            var builtBytes=Render(camera,390,844);
            File.WriteAllBytes(Path.Combine(folder,builtFile),builtBytes);
            records.Add("{\"file\":\""+builtFile+"\",\"sha256\":\""+Sha(builtBytes)+"\","+
                "\"width\":390,\"height\":844,\"stage\":5,\"quarry_built_qa_only\":true}");
            fit.SetEvidenceViewport(1170,2532);
            fit.SetEvidenceZoom(1.75f);
            fit.SetEvidenceFocus((Vector2)quarry.transform.position);
            builtFile="premium_bastion5_quarry_built_detail_1170x2532.png";
            builtBytes=Render(camera,1170,2532);
            File.WriteAllBytes(Path.Combine(folder,builtFile),builtBytes);
            records.Add("{\"file\":\""+builtFile+"\",\"sha256\":\""+Sha(builtBytes)+"\","+
                "\"width\":1170,\"height\":2532,\"stage\":5,\"quarry_built_qa_only\":true}");
            if(records.Count!=11)throw new InvalidOperationException("Eleven source-verified camera frames expected");
            var json="{\"engine\":\"Unity\",\"mode\":\"Editor_RenderTexture\","+
                "\"source_scene\":\"Experimento_Ciudad2D_PremiumV3.unity\","+
                "\"isolate_only\":true,\"plot_state_read_only\":true,"+
                "\"four_premium_candidates\":[\"cuartel\",\"aserradero\",\"granero\",\"cantera\"],"+
                "\"cantera_premium\":true,\"quarry_unbuilt_at_default_stage5\":true,"+
                "\"quarry_built_qa_fixture_only\":true,\"bastion_I_1_to_4_V_at_5\":true,"+
                "\"shadows_independent_built_gated\":true,"+
                "\"ambient_smoke_sources_after_deduplication\":2,"+
                "\"gameplay_modified\":false,\"main_modified\":false,"+
                "\"art_approved\":false,\"iPhone_physical_test\":\"PENDING\","+
                "\"captures\":["+string.Join(",",records)+"]}\n";
            File.WriteAllText(report,json,Encoding.UTF8);
            Debug.Log("[Ciudad2D] PREMIUM_PREVIEW_11_TRUE_UNITY_RENDER_FRAMES_NO_PLAYMODE_CERTIFICATION");
        }
        static byte[] Render(Camera camera,int w,int h)
        {
            var oldRT=RenderTexture.active;
            var oldTarget=camera.targetTexture;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            Texture2D tex=null;
            try
            {
                rt.Create();camera.targetTexture=rt;camera.aspect=(float)w/h;
                camera.Render();RenderTexture.active=rt;
                tex=new Texture2D(w,h,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
                var bytes=tex.EncodeToPNG();
                if(bytes==null||bytes.Length<8000)throw new InvalidOperationException("Empty real Unity frame");
                return bytes;
            }
            finally
            {
                camera.targetTexture=oldTarget;RenderTexture.active=oldRT;
                if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);
                rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
        static string Sha(byte[] raw)
        {
            using(var hash=SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(raw)).Replace("-","").ToLowerInvariant();
        }
    }
}
#endif

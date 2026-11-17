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
    /// True native three-way comparison; all work is in the isolated sandbox.
    /// A=32 original props and old Cuartel, B=23 props old Cuartel,
    /// C=23 props premium Cuartel with separate shadow.
    /// No changes to PlayerState, construction policy, BuildSettings, or main.
    /// </summary>
    public static class City2DQualityV3Gallery
    {
        const string Root="Assets/Experimentos/Ciudad2D/";
        const string Spr=Root+"Sprites/";
        const string Candidate=Spr+"Candidates/Cuartel_Premium_V2.png";
        const string Shadow=Spr+"Candidates/Cuartel_Premium_ShadowV2.png";
        const string SourceA=Root+"ArtBible/city_layout_32decor_control_A_20261117.json";
        const string Output="Ciudad2DQualityV3Evidence";
        const string SawmillCandidate=Spr+"Candidates/Aserradero_Premium_V1.png";
        const string SawmillShadow=Spr+"Candidates/Aserradero_Premium_ShadowV1.png";
        const string SawmillSha="4fa333a21db0150179b6dd1158b559623d52765de90c82cc8b807ea30eaffa9e";
        const string SawmillShadowSha="900eeab27836e647671820287a38fefb0050cea64bf991f53ad01bcc27a4ae7c";
        const string ExactCandidateSha="0dcb8edb8237621f13893893edce20fec51deb1248df8bcbc3a89ea3990b4062";
        const string ExactShadowSha="9d128fe0a7d0c229c1c3b14b098eaef17e6df79ac557ed283a3893bb4a22c5a6";
        static readonly HashSet<string> Removed=new HashSet<string>(StringComparer.Ordinal)
            {"decor_01","decor_02","decor_20","decor_21","decor_22",
             "decor_23","decor_24","decor_27","decor_30"};

        [Serializable] sealed class LegacyLayout { public Decor[] decor; }
        [Serializable] sealed class Decor
        {
            public string id,filename;
            public float x,y,scale;
            public int min_bastion,max_bastion;
        }
        struct Extents { public int Width, MinimumY; }
        static Extents VisibleExtents(Sprite sprite)
        {
            string physical=AssetPath(sprite);
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                if(!tex.LoadImage(File.ReadAllBytes(physical),false))
                    throw new InvalidOperationException("Cannot decode "+sprite.name);
                var pixels=tex.GetPixels32();
                int left=tex.width,right=-1,bottom=tex.height;
                for(int y=0;y<tex.height;y++)
                for(int x=0;x<tex.width;x++)
                    if(pixels[y*tex.width+x].a>32)
                    {
                        left=Math.Min(left,x);right=Math.Max(right,x);
                        bottom=Math.Min(bottom,y);
                    }
                if(right<left)throw new InvalidOperationException("Invisible candidate source");
                return new Extents {Width=right-left+1,MinimumY=bottom};
            }
            finally { UnityEngine.Object.DestroyImmediate(tex); }
        }
        static string AssetPath(Sprite sprite)
        {
            string path=AssetDatabase.GetAssetPath(sprite);
            if(!path.StartsWith(Root,StringComparison.Ordinal))
                throw new InvalidOperationException("Outside sandbox: "+path);
            return Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                "..",path.Replace('/',Path.DirectorySeparatorChar)));
        }
        static void CheckSha(string relative,string expected)
        {
            string path=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                "..",relative));
            using(var sha=SHA256.Create())
            {
                string digest=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)))
                    .Replace("-","").ToLowerInvariant();
                if(digest!=expected)throw new InvalidOperationException(
                    "Premature art source change "+relative);
            }
        }
        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Premium V3 A-B-C-D nativo")]
        public static void Capture()
        {
            if(!UnityEngine.Application.isBatchMode &&
               !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            var previous=EditorSceneManager.GetSceneManagerSetup();
            var outDir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..",Output));
            Directory.CreateDirectory(outDir);
            string manifest=Path.Combine(outDir,"evidence.json");
            if(File.Exists(manifest))File.Delete(manifest);
            CheckSha(Candidate,ExactCandidateSha);
            CheckSha(Shadow,ExactShadowSha);
            CheckSha(SawmillCandidate,SawmillSha);
            CheckSha(SawmillShadow,SawmillShadowSha);
            var premium=AssetDatabase.LoadAssetAtPath<Sprite>(Candidate);
            var shadow=AssetDatabase.LoadAssetAtPath<Sprite>(Shadow);
            var premiumSawmill=AssetDatabase.LoadAssetAtPath<Sprite>(SawmillCandidate);
            var sawmillShadow=AssetDatabase.LoadAssetAtPath<Sprite>(SawmillShadow);
            if(premium==null||shadow==null || premium.texture.width!=shadow.texture.width
                ||premium.texture.height!=shadow.texture.height)
                throw new InvalidOperationException("Premium art/shadow import failed");
            if(premiumSawmill==null||sawmillShadow==null ||
                premiumSawmill.texture.width!=sawmillShadow.texture.width ||
                premiumSawmill.texture.height!=sawmillShadow.texture.height)
                throw new InvalidOperationException("Premium sawmill/shadow pair import failed");
            try
            {
                if(UnityEngine.Application.isBatchMode)
                {
                    var scratch=EditorSceneManager.NewScene(
                        NewSceneSetup.EmptyScene,NewSceneMode.Single);
                    if(!EditorSceneManager.SaveScene(scratch,Root+"QA/QualityV3_Scratch.unity"))
                        throw new InvalidOperationException("Scratch scene could not be prepared");
                }
                City2DSceneBuilder.Rebuild();
                var scene=EditorSceneManager.OpenScene(City2DSceneBuilder.ScenePath,OpenSceneMode.Single);
                if(!scene.IsValid()||!scene.isLoaded)throw new InvalidOperationException("Scene not loaded");
                var cam=Camera.main;
                var fit=cam!=null?cam.GetComponent<City2DCameraFit>():null;
                var bridge=UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
                var terrain=GameObject.Find("Terreno_Valoria")?.GetComponent<SpriteRenderer>();
                if(cam==null||fit==null||bridge==null||bridge.IsLiveBound||terrain==null)
                    throw new InvalidOperationException("Not genuine original scene and read-only state");
                bridge.RebindSceneBuildings();
                if(bridge.RegisteredBuildingCount!=6||bridge.RegisteredAmbientEffectCount!=10)
                    throw new InvalidOperationException("Core city mechanics/ambience changed");

                CityBuilding cuartel=null, sawmill=null;
                foreach(var component in UnityEngine.Object.FindObjectsByType<CityBuilding>(
                    FindObjectsSortMode.None))
                    if(component.BuildingId=="cuartel")cuartel=component;
                    else if(component.BuildingId=="aserradero")sawmill=component;
                if(cuartel==null||sawmill==null)throw new InvalidOperationException("Missing original Cuartel/Sawmill");
                var render=cuartel.GetComponent<SpriteRenderer>();
                var oldArt=render.sprite;
                var oldExt=VisibleExtents(oldArt);
                var newExt=VisibleExtents(premium);
                var shadowObj=new GameObject("QA_ONLY_CuartelPremium_ContactShadow");
                shadowObj.transform.position=cuartel.transform.position;
                var shade=shadowObj.AddComponent<SpriteRenderer>();
                shade.sprite=shadow;
                shade.sortingOrder=render.sortingOrder-1;
                shade.enabled=false;
                var sawmillRender=sawmill.GetComponent<SpriteRenderer>();
                var oldSawmillArt=sawmillRender.sprite;
                var oldSawmillExtent=VisibleExtents(oldSawmillArt);
                var premiumSawmillExtent=VisibleExtents(premiumSawmill);
                var sawmillShadeObj=new GameObject("QA_ONLY_PremiumSawmillContactShadow");
                var sawmillShade=sawmillShadeObj.AddComponent<SpriteRenderer>();
                sawmillShade.sprite=sawmillShadow;
                sawmillShade.sortingOrder=sawmillRender.sortingOrder-1;
                sawmillShade.enabled=false;

                var legacy=AssetDatabase.LoadAssetAtPath<TextAsset>(SourceA);
                var config=legacy!=null?JsonUtility.FromJson<LegacyLayout>(legacy.text):null;
                if(config?.decor==null||config.decor.Length!=32)
                    throw new InvalidOperationException("Missing exact 32-prop control A");
                var restored=new List<City2DAmbientGating>();
                foreach(var spec in config.decor)
                {
                    if(!Removed.Contains(spec.id))continue;
                    if(GameObject.Find("Atrezzo_"+spec.id)!=null)
                        throw new InvalidOperationException("Density reduction was not applied: "+spec.id);
                    var obj=new GameObject("QA_ControlA_"+spec.id);
                    obj.transform.position=new Vector3(
                        (spec.x/941f-.5f)*terrain.sprite.bounds.size.x,
                        (.5f-spec.y/1672f)*terrain.sprite.bounds.size.y,-.05f);
                    obj.transform.localScale=Vector3.one*spec.scale;
                    var sr=obj.AddComponent<SpriteRenderer>();
                    sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Spr+spec.filename);
                    if(sr.sprite==null)throw new InvalidOperationException("Control prop missing");
                    sr.sortingOrder=-Mathf.RoundToInt(
                        (.5f-spec.y/1672f)*terrain.sprite.bounds.size.y*100f);
                    restored.Add(obj.AddComponent<City2DAmbientGating>());
                    restored[restored.Count-1].Configure(spec.min_bastion,spec.max_bastion);
                }
                if(restored.Count!=9)throw new InvalidOperationException("Nine control props not restored");

                var records=new List<string>();
                var modes=new[]{"A_32_original","B_23_quiet","C_23_premium","D_23_cuartel_sawmill"};
                foreach(int stage in new[]{1,3,5})
                {
                    bridge.SetDemoBastionLevel(stage);
                    var originalPosition=cuartel.transform.position;
                    var originalScale=cuartel.transform.localScale;
                    var actualOriginal=render.sprite;
                    var sawmillOrigin=sawmill.transform.position;
                    var sawmillScale=sawmill.transform.localScale;
                    foreach(string mode in modes)
                    {
                        foreach(var gate in restored)
                        {
                            gate.Apply(stage);
                            gate.GetComponent<SpriteRenderer>().enabled=false; // Then restore only true A scene as staged.
                        }
                        // The imported stage windows still govern control props.
                        if(mode=="A_32_original")
                            for(int i=0;i<restored.Count;i++)
                            {
                                var spec=FindSpecFor(restored[i].gameObject.name,config);
                                restored[i].GetComponent<SpriteRenderer>().enabled=
                                    stage>=spec.min_bastion &&
                                    (spec.max_bastion==0||stage<=spec.max_bastion);
                            }

                        bool candidate=mode=="C_23_premium"||mode=="D_23_cuartel_sawmill";
                        render.sprite=candidate?premium:actualOriginal;
                        if(candidate)
                        {
                            float visibleWidth=oldExt.Width/
                                actualOriginal.pixelsPerUnit*originalScale.x;
                            float multiplier=visibleWidth/
                                (newExt.Width/premium.pixelsPerUnit);
                            cuartel.transform.localScale=Vector3.one*multiplier;
                            float prevGround=oldExt.MinimumY/
                                actualOriginal.pixelsPerUnit*originalScale.x;
                            float nextGround=newExt.MinimumY/
                                premium.pixelsPerUnit*multiplier;
                            cuartel.transform.position=originalPosition+
                                new Vector3(0f,prevGround-nextGround,0f);
                            shadowObj.transform.position=cuartel.transform.position;
                            shadowObj.transform.localScale=cuartel.transform.localScale;
                            // No shadow of a building that is not yet built.
                            shade.enabled=render.enabled;
                        }
                        else
                        {
                            cuartel.transform.position=originalPosition;
                            cuartel.transform.localScale=originalScale;
                            shade.enabled=false;
                        }

                        // D changes only one additional sprite, A/B/C remain strict controls.
                        bool sawmillCandidate=mode=="D_23_cuartel_sawmill";
                        sawmillRender.sprite=sawmillCandidate?premiumSawmill:oldSawmillArt;
                        if(sawmillCandidate && sawmillRender.enabled)
                        {
                            float width=oldSawmillExtent.Width/oldSawmillArt.pixelsPerUnit*sawmillScale.x;
                            float mult=width/(premiumSawmillExtent.Width/premiumSawmill.pixelsPerUnit);
                            sawmill.transform.localScale=Vector3.one*mult;
                            float oldBase=oldSawmillExtent.MinimumY/oldSawmillArt.pixelsPerUnit*sawmillScale.x;
                            float newBase=premiumSawmillExtent.MinimumY/premiumSawmill.pixelsPerUnit*mult;
                            sawmill.transform.position=sawmillOrigin+new Vector3(0,oldBase-newBase,0);
                            sawmillShadeObj.transform.position=sawmill.transform.position;
                            sawmillShadeObj.transform.localScale=sawmill.transform.localScale;
                            sawmillShade.enabled=true;
                        }
                        else
                        {
                            sawmill.transform.position=sawmillOrigin;
                            sawmill.transform.localScale=sawmillScale;
                            sawmillShade.enabled=false;
                        }

                        int[][] views=stage==5
                            ?new[]{new[]{390,844},new[]{1170,2532},new[]{844,390}}
                            :stage==3?new[]{new[]{390,844},new[]{1170,2532}}
                            :new[]{new[]{390,844}};
                        foreach(var wh in views)
                        {
                            fit.SetEvidenceViewport(wh[0],wh[1]);
                            fit.SetEvidenceFocus(Vector2.zero); // locked, identical camera before every A/B/C/D
                            Save(outDir,records,cam,mode,stage,wh[0],wh[1],"standard");
                        }
                        if(stage==5)
                        {
                            fit.SetEvidenceViewport(1170,2532);
                            fit.SetEvidenceZoom(1.75f);
                            fit.SetEvidenceFocus((Vector2)cuartel.transform.position);
                            Save(outDir,records,cam,mode,stage,1170,2532,"cuartel_close");
                            fit.SetEvidenceViewport(1170,2532);
                            fit.SetEvidenceZoom(1.75f);
                            fit.SetEvidenceFocus((Vector2)sawmill.transform.position);
                            Save(outDir,records,cam,mode,stage,1170,2532,"aserradero_close");
                        }
                    }
                    render.sprite=actualOriginal;
                    cuartel.transform.position=originalPosition;
                    cuartel.transform.localScale=originalScale;
                    sawmillRender.sprite=oldSawmillArt;
                    sawmill.transform.position=sawmillOrigin;
                    sawmill.transform.localScale=sawmillScale;
                }
                // Note: stage II/IV use the unchanged Bastion I sprite by design.
                string report="{\"engine\":\"Unity\",\"mode\":\"Editor_RenderTexture\","+
                    "\"source_scene\":\"Experimento_Ciudad2D.unity\","+
                    "\"art_comparison_only\":true,\"modes\":[\""+
                    string.Join("\",\"",modes)+"\"],"+
                    "\"removed_duplicate_background_props\":9,"+
                    "\"legacy_original_props\":32,\"quieter_props\":23,"+
                    "\"premium_cuartel_original_sha256\":\""+ExactCandidateSha+"\","+
                    "\"shadow_separate\":true,\"shadow_source_sha256\":\""+ExactShadowSha+"\","+
                    "\"sawmill_candidate_sha256\":\""+SawmillSha+"\","+
                    "\"sawmill_shadow_sha256\":\""+SawmillShadowSha+"\","+
                    "\"main_modified\":false,\"gameplay_modified\":false,"+
                    "\"bastion_I_1_to_4_V_5_unchanged\":true,"+
                    "\"playmode_candidate_pointer_qa\":false,"+
                    "\"art_approved\":false,\"physical_iphone_test\":\"PENDING\","+
                    "\"captures\":["+string.Join(",",records)+"]}\n";
                File.WriteAllText(manifest,report,Encoding.UTF8);
                Debug.Log("[Ciudad2D] QUALITY_V3_NATIVE_A_B_C_D_32_FRAMES_ART_NOT_APPROVED");
            }
            finally
            {
                if(!UnityEngine.Application.isBatchMode && previous!=null && previous.Length>0)
                    EditorSceneManager.RestoreSceneManagerSetup(previous);
            }
        }
        static Decor FindSpecFor(string name,LegacyLayout source)
        {
            string id=name.Replace("QA_ControlA_","");
            foreach(var item in source.decor)if(item.id==id)return item;
            throw new InvalidOperationException("Control decor metadata missing");
        }
        static void Save(string root,List<string> list,Camera cam,string mode,
            int stage,int w,int h,string closeup)
        {
            var previous=RenderTexture.active;
            var targetBefore=cam.targetTexture;
            var target=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            Texture2D tex=null;
            try
            {
                target.Create();cam.targetTexture=target;cam.aspect=(float)w/h;
                cam.Render();RenderTexture.active=target;
                tex=new Texture2D(w,h,TextureFormat.RGB24,false);
                tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
                byte[] binary=tex.EncodeToPNG();
                if(binary==null||binary.Length<8000)
                    throw new InvalidOperationException("Empty actual Unity frame");
                string file=mode+"_bastion"+stage+"_"+w+"x"+h+"_"+closeup+".png";
                File.WriteAllBytes(Path.Combine(root,file),binary);
                using(var sha=SHA256.Create())
                {
                    var digest=BitConverter.ToString(sha.ComputeHash(binary))
                        .Replace("-","").ToLowerInvariant();
                    list.Add("{\"file\":\""+file+"\",\"sha256\":\""+digest+
                        "\",\"width\":"+w+",\"height\":"+h+",\"stage\":"+stage+"}");
                }
            }
            finally
            {
                cam.targetTexture=targetBefore;RenderTexture.active=previous;
                if(tex!=null)UnityEngine.Object.DestroyImmediate(tex);
                target.Release();UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
#endif

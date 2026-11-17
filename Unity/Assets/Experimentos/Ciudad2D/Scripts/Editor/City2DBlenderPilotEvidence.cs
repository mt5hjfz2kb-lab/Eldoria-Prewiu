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
    /// Exactly matched Unity A/B screenshots: current city versus the two
    /// Blender-generated color+shadow PNGs. Never modifies production scenes,
    /// PlayerState, original art, Build Settings or colliders.
    /// A true sprite != a flat 3D mesh rendered at runtime.
    /// </summary>
    public static class City2DBlenderPilotEvidence
    {
        private const string Base = City2DSceneBuilder.Folder + "Sprites/BlenderPilot/";
        private const string OutputDir = "Ciudad2DBlenderPilotEvidence";
        private sealed class Candidate
        {
            public string Name;
            public CityBuilding Building;
            public SpriteRenderer Original;
            public GameObject ColorGo, ShadowGo;
            public SpriteRenderer Color, Shadow;
            public float WorldFootWidth;
            public int OriginalSorting;
        }

        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Blender Cuartel+Aserradero A/B Unity")]
        public static void Capture()
        {
            string root = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", OutputDir));
            Directory.CreateDirectory(root);
            string manifestPath = Path.Combine(root, "evidence.json");
            if (File.Exists(manifestPath)) File.Delete(manifestPath);
            if (!UnityEngine.Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var oldSetup = EditorSceneManager.GetSceneManagerSetup();
            if (UnityEngine.Application.isBatchMode)
            {
                var scratch = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                if (!EditorSceneManager.SaveScene(scratch, City2DSceneBuilder.Folder+"QA/City2D_BlenderScratch.unity"))
                    throw new InvalidOperationException("Scratch scene could not be saved");
            }
            var shots = new List<string>();
            var previews = new List<Candidate>();
            try
            {
                // Always reproduce exact experimental scene from pinned SHA.
                City2DSceneBuilder.Rebuild();
                var loaded = EditorSceneManager.OpenScene(City2DSceneBuilder.ScenePath, OpenSceneMode.Single);
                if (!loaded.IsValid() || !loaded.isLoaded)
                    throw new InvalidOperationException("Blender A/B cannot load isolated Unity scene");
                var camera = Camera.main;
                var cameraFit = camera != null ? camera.GetComponent<City2DCameraFit>() : null;
                var bridge = UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
                if (camera == null || cameraFit == null || bridge == null)
                    throw new InvalidOperationException("A/B missing real scene camera, Fit or state bridge");
                bridge.RebindSceneBuildings();
                bridge.SetDemoBastionLevel(5);
                if (bridge.RegisteredBuildingCount != 6 || bridge.IsLiveBound)
                    throw new InvalidOperationException("A/B source state not isolated and read-only");

                foreach (var name in new[] {"cuartel","aserradero"})
                {
                    var p = FindBuilding(name);
                    if (p == null || p.PlotPhase != City2DPlotPhase.Built)
                        throw new InvalidOperationException("Chapter V did not build intended " + name);
                    var original=p.GetComponent<SpriteRenderer>();
                    if (original==null||original.sprite==null||!original.enabled)
                        throw new InvalidOperationException("Missing original renderer: "+name);
                    string prefix=name=="cuartel"?"Cuartel":"Aserradero";
                    var color=AssetDatabase.LoadAssetAtPath<Sprite>(Base+prefix+"_Ready.png");
                    var shadow=AssetDatabase.LoadAssetAtPath<Sprite>(Base+prefix+"_ShadowReady.png");
                    if(color==null||shadow==null)
                        throw new InvalidOperationException("Blender two-layer sprite pair missing: "+prefix);
                    if (color.texture.width != shadow.texture.width ||
                        color.texture.height != shadow.texture.height ||
                        color.pivot != shadow.pivot)
                        throw new InvalidOperationException("Blender color/shadow registration mismatch: "+prefix);
                    var trial = new Candidate {
                        Name=prefix,Building=p,Original=original,
                        WorldFootWidth=original.sprite.bounds.size.x * p.transform.lossyScale.x,
                        OriginalSorting=original.sortingOrder
                    };
                    trial.ColorGo=new GameObject("Blender_B_"+name);
                    trial.ColorGo.transform.position=p.transform.position;
                    trial.Color=trial.ColorGo.AddComponent<SpriteRenderer>();
                    trial.Color.sprite=color;
                    trial.Color.sortingOrder=original.sortingOrder;
                    // A transparent 1050px canvas is not a 1050px-wide building.
                    // Match visible alpha support to the unmodified 860px
                    // original, verified against source SHA and Blender bbox:
                    // Cuartel 845/860 vs 782/1050; Aserradero 849/860 vs 722/1050.
                    float oldVisibleRatio = name=="cuartel" ? 845f/860f : 849f/860f;
                    float blenderVisibleRatio = name=="cuartel" ? 782f/1050f : 722f/1050f;
                    float apparentWidthCorrection = oldVisibleRatio/blenderVisibleRatio;
                    trial.ColorGo.transform.localScale=Vector3.one *
                        (trial.WorldFootWidth / Mathf.Max(0.001f,color.bounds.size.x)) * apparentWidthCorrection;
                    trial.Color.enabled=false;
                    trial.ShadowGo=new GameObject("Blender_Shadow_"+name);
                    trial.ShadowGo.transform.position=p.transform.position;
                    trial.Shadow=trial.ShadowGo.AddComponent<SpriteRenderer>();
                    trial.Shadow.sprite=shadow;
                    trial.Shadow.sortingOrder=original.sortingOrder-1;
                    trial.ShadowGo.transform.localScale=trial.ColorGo.transform.localScale;
                    trial.Shadow.enabled=false;
                    // Game's original collider remains attached to original p,
                    // independent of the review sprites/footprint.
                    var collider=p.GetComponent<BoxCollider2D>();
                    if(collider==null||!collider.enabled)
                        throw new InvalidOperationException("Missing original physical hotspot: "+name);
                    previews.Add(trial);
                    Debug.Log("[Ciudad2D] BLENDER_AB_READY "+name+
                        " source="+original.sprite.name+
                        " replacement="+color.name+
                        " worldFootWidth="+trial.WorldFootWidth+
                        " scale="+trial.ColorGo.transform.localScale.x);
                }
                var presets = new [] {
                    new { Width=390, Height=844, Zoom=1f },
                    new { Width=1170, Height=2532, Zoom=1f },
                    new { Width=844, Height=390, Zoom=1f },
                    new { Width=1170, Height=2532, Zoom=1.42f }
                };
                var outputs = new Dictionary<string,byte[]>();
                string[] modes={"A_original","B_cuartel","B_aserradero","B_both"};
                foreach(var mode in modes)
                {
                    foreach(var c in previews)
                    {
                        bool enabled=(mode=="B_both"||
                           (mode=="B_cuartel"&&c.Name=="Cuartel")||
                           (mode=="B_aserradero"&&c.Name=="Aserradero"));
                        c.Original.enabled=!enabled;
                        c.Color.enabled=enabled;
                        c.Shadow.enabled=enabled;
                        if (c.Building.GetComponent<BoxCollider2D>().enabled != true)
                            throw new InvalidOperationException("Comparison changed original collider");
                    }
                    foreach(var v in presets)
                    {
                        cameraFit.SetEvidenceViewport(v.Width,v.Height);
                        if (v.Zoom>1.001f) cameraFit.SetEvidenceZoom(v.Zoom);
                        string key=mode+"_"+v.Width+"x"+v.Height+(v.Zoom>1.001f?"_zoom":"");
                        byte[] png=Render(camera,v.Width,v.Height);
                        outputs[key]=png;
                        string file=key+".png";
                        File.WriteAllBytes(Path.Combine(root,file),png);
                        shots.Add("    {\"file\":\""+file+"\",\"mode\":\""+mode+
                            "\",\"width\":"+v.Width+",\"height\":"+v.Height+
                            ",\"sha256\":\""+Sha(png)+"\"}");
                    }
                    // Reset zoom before the next phase: exact matched cameras.
                    cameraFit.SetEvidenceZoom(1f);
                }
                foreach(var v in presets)
                {
                    string dimension=v.Width+"x"+v.Height+(v.Zoom>1.001f?"_zoom":"");
                    byte[] basePng=outputs["A_original_"+dimension];
                    byte[] bothPng=outputs["B_both_"+dimension];
                    // Pixel content must differ; this is visibility, not quality.
                    if(Sha(basePng)==Sha(bothPng))
                        throw new InvalidOperationException("Blender B invisible; A/B identical "+dimension);
                }
                string json="{\n"+
                    "  \"engine\":\"Unity\",\n"+
                    "  \"mode\":\"Editor_RenderTexture_A_B_not_PlayMode\",\n"+
                    "  \"model_source\":\"Blender kit-based authoring, two building variants\",\n"+
                    "  \"source_lore\":\"Valoria, age I-V\",\n"+
                    "  \"comparison_camera_locked\":true,\n"+
                    "  \"two_building_layers\":true,\n"+
                    "  \"unchanged_unity_hotspots\":true,\n"+
                    "  \"progression_real_read_only\":true,\n"+
                    "  \"gameplay_or_save_modified\":false,\n"+
                    "  \"main_changed\":false,\n"+
                    "  \"art_approved\":false,\n"+
                    "  \"device_qa\":\"PENDING\",\n"+
                    "  \"captures\":[\n"+string.Join(",\n",shots)+"\n  ]\n}\n";
                File.WriteAllText(manifestPath,json,Encoding.UTF8);
                Debug.Log("[Ciudad2D] BLENDER_A_B_REAL_UNITY_RENDER_PASS "+shots.Count);
            }
            finally
            {
                foreach(var c in previews)
                {
                    if(c.Original!=null)c.Original.enabled=true;
                    if(c.ColorGo!=null)UnityEngine.Object.DestroyImmediate(c.ColorGo);
                    if(c.ShadowGo!=null)UnityEngine.Object.DestroyImmediate(c.ShadowGo);
                }
                if(!UnityEngine.Application.isBatchMode && oldSetup!=null && oldSetup.Length>0)
                    EditorSceneManager.RestoreSceneManagerSetup(oldSetup);
            }
        }
        private static CityBuilding FindBuilding(string id)
        {
            foreach(var b in UnityEngine.Object.FindObjectsByType<CityBuilding>(FindObjectsSortMode.None))
                if(b.BuildingId==id)return b;
            return null;
        }
        private static string Sha(byte[] bytes)
        {
            using(var sha=SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();
        }
        private static byte[] Render(Camera cam,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var oldTexture=cam.targetTexture;
            var oldActive=RenderTexture.active;
            Texture2D readback=null;
            try
            {
                rt.Create();cam.targetTexture=rt;cam.aspect=(float)w/h;cam.Render();
                RenderTexture.active=rt;
                readback=new Texture2D(w,h,TextureFormat.RGB24,false);
                readback.ReadPixels(new Rect(0,0,w,h),0,0);readback.Apply();
                var bytes=readback.EncodeToPNG();
                if(bytes==null||bytes.Length<8000)throw new InvalidOperationException("Empty A/B screenshot");
                return bytes;
            }
            finally
            {
                cam.targetTexture=oldTexture;RenderTexture.active=oldActive;
                if(readback!=null)UnityEngine.Object.DestroyImmediate(readback);
                rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
#endif

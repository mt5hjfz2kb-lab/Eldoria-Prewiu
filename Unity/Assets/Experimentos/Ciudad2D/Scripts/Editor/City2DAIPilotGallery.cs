#if UNITY_EDITOR
using System;
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
    /// Separate A/B comparison STAGE. It cannot claim art approval, edit saves,
    /// change gameplay or use an unreviewed image as the actual city asset.
    /// Real Unity captures become possible only when three verified candidate
    /// RGBA sprites have been deliberately copied into Sprites/AIPilot/.
    /// </summary>
    public static class City2DAIPilotGallery
    {
        private const string Root = "Assets/Experimentos/Ciudad2D/";
        private const string SpriteRoot = Root + "Sprites/";
        private const string CandidateRoot = SpriteRoot + "AIPilot/";
        private const string OutFolder = "Ciudad2DAIPilotEvidence";
        private static readonly string[] Candidates =
            { "Forja_Guardia_RGBA.png", "Observatorio_Brecha_RGBA.png",
              "Archivo_Cartografos_RGBA.png" };
        private static readonly string[] Originals =
            { "Cuartel.png", "Aserradero.png", "Granero.png" };
        private static readonly Vector2[] Slots =
            { new Vector2(206,547), new Vector2(172,964), new Vector2(751,962) };
        private static readonly float[] OriginalScales =
            { .27558837f, .310636f, .307713f };

        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Arte IA / Comparar tres nuevos con maestros")]
        public static void Capture() { CaptureForMode(false); }

        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Arte IA / Comparar Forja V3 con originales")]
        public static void CaptureForjaV3() { CaptureForMode(true); }

        // Run the exact same camera and ground for the existing three-building
        // baseline and the new isolated V3 test. The game's city never changes.
        private static void CaptureForMode(bool forgeV3)
        {
            if (!UnityEngine.Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var previousSetup=EditorSceneManager.GetSceneManagerSetup();
            var folderName=forgeV3?"Ciudad2DForjaV3Evidence":OutFolder;
            var exportPath=Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath,"..",folderName));
            Directory.CreateDirectory(exportPath);
            var report=Path.Combine(exportPath,"evidence.json");
            if(File.Exists(report)) File.Delete(report);
            var terrain=AssetDatabase.LoadAssetAtPath<Sprite>(
                SpriteRoot+"Retina/Terreno_Organico.png");
            if(terrain==null) throw new InvalidOperationException(
                "Valoria approved ground candidate absent. Never invent a ground.");

            Sprite[] candidate=new Sprite[3],source=new Sprite[3];
            Footprint[] candidatePixels=new Footprint[3],sourcePixels=new Footprint[3];
            for(int i=0;i<3;i++)
            {
                source[i]=AssetDatabase.LoadAssetAtPath<Sprite>(
                    SpriteRoot+Originals[i]);
                var candidateName=forgeV3&&i==0
                    ?"Versions/Forja_Guardia_RGBA_v3.png":Candidates[i];
                candidate[i]=AssetDatabase.LoadAssetAtPath<Sprite>(
                    CandidateRoot+candidateName);
                if(source[i]==null || candidate[i]==null) throw new
                    InvalidOperationException("AI A/B not ready: THREE individual " +
                        "candidate sprites required, missing "+
                        (forgeV3&&i==0?"Forja V3":Candidates[i]));
                ValidateSprite(candidate[i],Candidates[i]);
                sourcePixels[i]=MeasureFootprint(source[i]);
                candidatePixels[i]=MeasureFootprint(candidate[i]);
            }
            var bastion=AssetDatabase.LoadAssetAtPath<Sprite>(
                SpriteRoot+"Bastion_Nivel05.png");
            if(bastion==null) throw new InvalidOperationException("Bastion master missing");

            try
            {
                var stage=EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var ground=new GameObject("Valoria_Terrain_Immutable").AddComponent<SpriteRenderer>();
                ground.sprite=terrain;
                ground.sortingOrder=-10000;
                var cameraGO=new GameObject("ArtBible_LockedCamera");
                cameraGO.tag="MainCamera";
                cameraGO.transform.position=new Vector3(0f,0f,-10f);
                var cam=cameraGO.AddComponent<Camera>();
                cam.orthographic=true;
                cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=new Color(.11f,.16f,.20f,1f);
                var fit=cameraGO.AddComponent<City2DCameraFit>();
                fit.SetGround(ground);
                var bastionObj=NewSprite("MAESTRO_BastionV",bastion,
                    470,824,.47f,terrain,-10);
                var art=new SpriteRenderer[3];
                for(int i=0;i<3;i++)
                    art[i]=NewSprite("PROOF_ONLY_"+Candidates[i],source[i],
                        Slots[i].x,Slots[i].y,OriginalScales[i],terrain,
                        -Mathf.RoundToInt((.5f-Slots[i].y/1672f)*terrain.bounds.size.y*100f));
                var evidence=new System.Collections.Generic.List<string>();
                var originalsPosition=new Vector3[3];
                for(int i=0;i<3;i++) originalsPosition[i]=art[i].transform.position;
                foreach(var mode in new[]{"A_original","B_new_ai"})
                {
                    for(int i=0;i<3;i++)
                    {
                        bool isOriginal=mode=="A_original";
                        art[i].sprite=isOriginal?source[i]:candidate[i];
                        var current=isOriginal?sourcePixels[i]:candidatePixels[i];
                        // Compare visible, alpha-backed silhouettes, not padded PNG canvas.
                        float worldVisibleWidth=sourcePixels[i].Width /
                            source[i].pixelsPerUnit*OriginalScales[i];
                        float scale=worldVisibleWidth /
                            Mathf.Max(.001f,current.Width/art[i].sprite.pixelsPerUnit);
                        art[i].transform.localScale=Vector3.one*scale;
                        // Baseline A is immutable; only move B to share its visible
                        // foot-ground anchor. Padding in RGBA is NOT a raised plinth.
                        float sourceFoot=sourcePixels[i].MinimumY /
                            source[i].pixelsPerUnit*OriginalScales[i];
                        float targetFoot=current.MinimumY /
                            art[i].sprite.pixelsPerUnit*scale;
                        art[i].transform.position=originalsPosition[i]+
                            new Vector3(0f,sourceFoot-targetFoot,0f);
                    }
                    foreach(var wh in new[]{new[]{390,844},new[]{1170,2532},
                                             new[]{844,390}})
                    {
                        fit.SetEvidenceViewport(wh[0],wh[1]);
                        string file=mode+"_"+wh[0]+"x"+wh[1]+".png";
                        var bytes=CaptureFrame(cam,wh[0],wh[1]);
                        File.WriteAllBytes(Path.Combine(exportPath,file),bytes);
                        evidence.Add("{\"file\":\""+file+"\",\"sha256\":\""+
                            Sha(bytes)+"\",\"width\":"+wh[0]+",\"height\":"+wh[1]+"}");
                    }
                    fit.SetEvidenceViewport(1170,2532);
                    fit.SetEvidenceZoom(1.42f);
                    string closeup=mode+"_1170x2532_zoom.png";
                    var detail=CaptureFrame(cam,1170,2532);
                    File.WriteAllBytes(Path.Combine(exportPath,closeup),detail);
                    evidence.Add("{\"file\":\""+closeup+"\",\"sha256\":\""+
                        Sha(detail)+"\",\"width\":1170,\"height\":2532,\"zoom\":1.42}");
                }
                string result="{\"engine\":\"Unity\",\"mode\":\"Editor_RenderTexture\","+
                    "\"gallery_only\":true,\"three_new_buildings_present\":true,"+
                    "\"forja_variant\":\""+(forgeV3?"V3":"V1")+"\","+
                    "\"master_art_unchanged\":true,"+
                    "\"camera_and_terrain_locked\":true,\"visible_alpha_width_aligned\":true,"+
                    "\"alpha_ground_foot_aligned_to_original\":true,\"main_modified\":false,"+
                    "\"gameplay_or_save_modified\":false,\"art_approved\":false,"+
                    "\"playmode_interaction_not_certified_in_gallery\":true,"+
                    "\"captures\":["+string.Join(",",evidence)+"]}\n";
                File.WriteAllText(report,result,Encoding.UTF8);
                EditorSceneManager.SaveScene(stage,Root+"QA/"+
                    (forgeV3?"Valoria_ArtBible_Sandbox_ForjaV3.unity":"Valoria_ArtBible_Sandbox.unity"));
                Debug.Log("[Ciudad2D] AI_GALLERY_8_NATIVE_CAPTURES_READY_ART_NOT_APPROVED"+(forgeV3?"_FORJA_V3":""));
            }
            finally
            {
                if(!UnityEngine.Application.isBatchMode &&
                   previousSetup!=null&&previousSetup.Length>0)
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }

        private struct Footprint
        {
            public float Width;
            public float MinimumY;
        }
        private static Footprint MeasureFootprint(Sprite sprite)
        {
            // Read source PNG into temporary memory. Never change Unity importer or
            // replace the master; the actual alpha silhouette controls the pivot.
            string asset=AssetDatabase.GetAssetPath(sprite);
            string file=Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath,"..",asset));
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                if(!tex.LoadImage(File.ReadAllBytes(file),false))
                    throw new InvalidOperationException("Unable to decode art PNG "+asset);
                var pixels=tex.GetPixels32();
                int minimumX=tex.width,maximumX=-1,minimumY=tex.height;
                for(int y=0;y<tex.height;y++)
                    for(int x=0;x<tex.width;x++)
                    {
                        if(pixels[y*tex.width+x].a<=32) continue;
                        minimumX=Math.Min(minimumX,x);
                        maximumX=Math.Max(maximumX,x);
                        minimumY=Math.Min(minimumY,y);
                    }
                if(maximumX<minimumX || minimumY>=tex.height)
                    throw new InvalidOperationException("Empty alpha silhouette "+asset);
                return new Footprint { Width=maximumX-minimumX+1,
                                       MinimumY=minimumY };
            }
            finally { UnityEngine.Object.DestroyImmediate(tex); }
        }

        private static void ValidateSprite(Sprite sprite,string name)
        {
            var tex=sprite.texture;
            if(tex.width<512||tex.height<512)
                throw new InvalidOperationException("Not enough source pixels: "+name);
            if(sprite.pivot.y>sprite.rect.height*.12f)
                throw new InvalidOperationException("Foot pivot not bottom anchored: "+name);
            string asset=AssetDatabase.GetAssetPath(sprite);
            if(!asset.StartsWith(CandidateRoot,StringComparison.Ordinal))
                throw new InvalidOperationException("Only candidate isolation folder permitted");
        }
        private static SpriteRenderer NewSprite(
            string label,Sprite art,float px,float py,float scale,Sprite terrain,int order)
        {
            var go=new GameObject(label);
            go.transform.position=new Vector3(
                (px/941f-.5f)*terrain.bounds.size.x,
                (.5f-py/1672f)*terrain.bounds.size.y,-.05f);
            go.transform.localScale=Vector3.one*scale;
            var visual=go.AddComponent<SpriteRenderer>();
            visual.sprite=art;
            visual.sortingOrder=order;
            return visual;
        }
        private static byte[] CaptureFrame(Camera cam,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;
            var previousTarget=cam.targetTexture;
            Texture2D copy=null;
            try
            {
                rt.Create();cam.targetTexture=rt;cam.aspect=(float)w/h;
                cam.Render();RenderTexture.active=rt;
                copy=new Texture2D(w,h,TextureFormat.RGB24,false);
                copy.ReadPixels(new Rect(0,0,w,h),0,0);copy.Apply();
                var bytes=copy.EncodeToPNG();
                if(bytes==null||bytes.Length<8000)
                    throw new InvalidOperationException("Empty Unity A/B image");
                return bytes;
            }
            finally
            {
                cam.targetTexture=previousTarget;
                RenderTexture.active=previous;
                if(copy!=null)UnityEngine.Object.DestroyImmediate(copy);
                rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
        private static string Sha(byte[] bytes)
        {
            using(var sha=SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-","").ToLowerInvariant();
        }
    }
}
#endif

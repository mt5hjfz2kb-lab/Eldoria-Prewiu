using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Preview-only size budget. Operates on copied, generated Unity scene
    /// assets in the Windows CI worktree; it never changes main or approved GLBs.
    /// </summary>
    public static class ValoriaFirstZoneWebGLBudget
    {
        const string Generated="Assets/Eldoria/Experimental/ValoriaFinishV1/Generated";
        const string Optimized="Assets/Eldoria/Experimental/ValoriaFinishV1/WebGLBudgetTextures";

        public static void PrepareGeneratedMaterials(int maxTextureSize)
        {
            if(maxTextureSize<128||maxTextureSize>1024)
                throw new ArgumentOutOfRangeException(nameof(maxTextureSize));
            if(!AssetDatabase.IsValidFolder(Generated))
                throw new Exception("Generated Unity first-zone assets are missing");
            if(!AssetDatabase.IsValidFolder(Optimized))
                AssetDatabase.CreateFolder("Assets/Eldoria/Experimental/ValoriaFinishV1",
                    "WebGLBudgetTextures");
            var cached=new Dictionary<Texture2D,Texture2D>();
            int bindings=0;
            long originalPixels=0, reducedPixels=0;
            var guids=AssetDatabase.FindAssets("t:Material",new[]{Generated});
            foreach(string guid in guids)
            {
                string matPath=AssetDatabase.GUIDToAssetPath(guid);
                var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if(mat==null)continue;
                bool changed=false;
                // Preview-only deglare: prevent overexposed near-white architectural surfaces.
                // Preserve material textures and color differences for the real art review.
                foreach(var colorProperty in new[]{"_BaseColor","_Color"})
                {
                    if(!mat.HasProperty(colorProperty))continue;
                    var c=mat.GetColor(colorProperty);
                    float peak=Math.Max(c.r,Math.Max(c.g,c.b));
                    if(peak>.78f)
                    {
                        float factor=.78f/peak;
                        mat.SetColor(colorProperty,new Color(c.r*factor*.95f,c.g*factor*.94f,c.b*factor*.90f,c.a));
                        changed=true;
                    }
                }
                foreach(string property in mat.GetTexturePropertyNames())
                {
                    var source=mat.GetTexture(property) as Texture2D;
                    if(source==null||Math.Max(source.width,source.height)<=maxTextureSize)
                        continue;
                    if(!cached.TryGetValue(source,out var replacement))
                    {
                        int longest=Math.Max(source.width,source.height);
                        int width=Math.Max(1,Mathf.RoundToInt(source.width*
                            ((float)maxTextureSize/longest)));
                        int height=Math.Max(1,Mathf.RoundToInt(source.height*
                            ((float)maxTextureSize/longest)));
                        var temp=RenderTexture.GetTemporary(width,height,0,
                            RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default);
                        var oldRT=RenderTexture.active;
                        try
                        {
                            Graphics.Blit(source,temp);
                            RenderTexture.active=temp;
                            replacement=new Texture2D(width,height,
                                TextureFormat.RGBA32,true,false);
                            replacement.ReadPixels(new Rect(0,0,width,height),0,0);
                            replacement.Apply(true,true);
                            replacement.name="WebGL 3D zone "+source.name;
                            replacement.filterMode=FilterMode.Trilinear;
                            replacement.wrapMode=source.wrapMode;
                            string token=AssetDatabase.AssetPathToGUID(
                                AssetDatabase.GetAssetPath(source));
                            if(string.IsNullOrEmpty(token))
                                token=source.GetInstanceID().ToString();
                            string dest=Optimized+"/"+token+".asset";
                            if(AssetDatabase.LoadAssetAtPath<Texture2D>(dest)!=null)
                                AssetDatabase.DeleteAsset(dest);
                            AssetDatabase.CreateAsset(replacement,dest);
                            cached[source]=replacement;
                            originalPixels+=(long)source.width*source.height*4;
                            reducedPixels+=(long)width*height*4;
                        }
                        finally
                        {
                            RenderTexture.active=oldRT;
                            RenderTexture.ReleaseTemporary(temp);
                        }
                    }
                    mat.SetTexture(property,replacement);
                    bindings++;changed=true;
                }
                if(changed)EditorUtility.SetDirty(mat);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("ELDORIA_FIRST_ZONE_TEXTURE_BUDGET original_bytes="+
                originalPixels+" reduced_bytes="+reducedPixels+
                " unique_textures="+cached.Count+" material_slots="+bindings);
            if(bindings==0)
                throw new Exception("Texture budget found zero linked material textures");
        }

        public static IDisposable HideUnusedArtTrialStreamingAssets()
        {
            var source=UnityEngine.Application.streamingAssetsPath;
            string backup=Path.Combine(Path.GetDirectoryName(
                UnityEngine.Application.dataPath),"Temp/ValoriaFirstZoneExcludedArtTrials");
            string[] names={"ValoriaFamilyV3","ValoriaLocalTalus",
                "ValoriaStructuralCliff","ValoriaCoherentCliffV4",
                "ValoriaCoherentCliffV5"};
            Directory.CreateDirectory(backup);
            var moved=new List<(string source,string backup)>();
            foreach(var name in names)
            {
                string original=Path.Combine(source,name);
                string temporary=Path.Combine(backup,name);
                if(Directory.Exists(temporary)&&!Directory.Exists(original))
                    Directory.Move(temporary,original); // recover interrupted build
                if(!Directory.Exists(original))continue;
                if(Directory.Exists(temporary))
                    throw new Exception("Conflicting art-trial backup "+temporary);
                Directory.Move(original,temporary);
                moved.Add((original,temporary));
            }
            AssetDatabase.Refresh();
            Debug.Log("ELDORIA_FIRST_ZONE_STREAMING_TRIAL_FOLDERS_EXCLUDED "+
                moved.Count);
            return new RestoreFolders(moved);
        }

        sealed class RestoreFolders:IDisposable
        {
            readonly List<(string source,string backup)> moved;
            public RestoreFolders(List<(string source,string backup)> folders)
                {moved=folders;}
            public void Dispose()
            {
                foreach(var item in moved)
                {
                    if(Directory.Exists(item.source))
                    {
                        if(Directory.GetFileSystemEntries(item.source).Length!=0)
                            throw new IOException("Streaming trial destination not empty "+
                                item.source);
                        Directory.Delete(item.source);
                    }
                    if(Directory.Exists(item.backup))
                        Directory.Move(item.backup,item.source);
                }
                AssetDatabase.Refresh();
            }
        }
    }
}

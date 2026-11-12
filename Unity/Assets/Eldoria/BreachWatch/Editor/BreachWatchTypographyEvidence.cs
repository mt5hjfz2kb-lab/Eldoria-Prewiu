#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.BreachWatch.Editor
{
    public static class BreachWatchTypographyEvidence
    {
        static bool inkMaskCalibrated;
        [Serializable] public sealed class Metric
        {
            public string path,text,role,font;
            public int size,configuredSize,generatorFontSizeUsed,visibleCharacters,expectedCharacters;
            public int gpuVisiblePixels,gpuOutsidePixels;
            public float preferredWidth,preferredHeight,pixelsPerUnit;
            public Rect box,ink;
            public Vector3 localScale;
            public bool glyphsPresent,generatedQuadOutside,inkOutside;
        }
        [Serializable] sealed class Proof
        {
            public string method="SAME_FRAME_IMPORTED_UGUI_FONT_AB_NOT_ART_APPROVAL";
            public string source=Environment.GetEnvironmentVariable("GITHUB_SHA");
            public string baselineFile,baselineSha256,candidateFile,candidateSha256;
            public int width,height;
            public bool restored,artApproved=false;
            public bool isolatedInkMaskCalibrated;
            public List<Metric> before,after;
            public List<string> violations=new List<string>();
        }
        sealed class State
        {
            public Text Text;public Font Font;public int Size,Min,Max;
            public FontStyle Style;public TextAnchor Alignment;
            public Vector3 Scale;public Vector2 Room;public bool BestFit;public string Value;
            public State(Text t){Text=t;Font=t.font;Size=t.fontSize;Style=t.fontStyle;
                Alignment=t.alignment;Scale=t.rectTransform.localScale;
                Room=t.rectTransform.sizeDelta;BestFit=t.resizeTextForBestFit;Min=t.resizeTextMinSize;
                Max=t.resizeTextMaxSize;Value=t.text;}
            public bool Same()=>!Text||(Text.font==Font&&Text.fontSize==Size&&
                Text.fontStyle==Style&&Text.alignment==Alignment&&
                Text.rectTransform.localScale==Scale&&Text.rectTransform.sizeDelta==Room&&Text.resizeTextForBestFit==BestFit&&
                Text.resizeTextMinSize==Min&&Text.resizeTextMaxSize==Max&&Text.text==Value);
        }
        static string Digest(byte[] bytes)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        static string PathOf(Transform item,Transform root)
        {string path=item.name;while(item.parent&&item.parent!=root){item=item.parent;path=item.name+"/"+path;}return path;}

        // Font atlas quads include transparent padding and are not visible
        // ink. Confirm suspicious bounds using the original GPU Text mesh,
        // isolated from every other Graphic and world Renderer. Black drop
        // shadows are retained but excluded from the bright-glyph mask.
        static void VisibleInk(Text text,Canvas canvas,Camera camera,RenderTexture target,
            int width,int height,out int visible,out int outside,bool displacedBounds=false)
        {
            visible=outside=0;
            // World-space building labels live on other Canvases. Disabling
            // only the Vigilia descendants leaves their pixels in the mask.
            var graphics=UnityEngine.Object.FindObjectsByType<Graphic>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            var enabled=graphics.Select(x=>x.enabled).ToArray();
            var renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            var rendered=renderers.Select(x=>x.enabled).ToArray();
            var oldFlags=camera.clearFlags;var oldColor=camera.backgroundColor;
            var oldActive=RenderTexture.active;
            var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
            var corners=new Vector3[4];text.rectTransform.GetWorldCorners(corners);
            var points=corners.Select(x=>camera.WorldToViewportPoint(x)).ToArray();
            var bounds=Rect.MinMaxRect(points.Min(x=>x.x)*width-2f,points.Min(x=>x.y)*height-2f,
                points.Max(x=>x.x)*width+2f,points.Max(x=>x.y)*height+2f);
            if(displacedBounds)bounds.x-=width*2f;
            try
            {
                for(int i=0;i<graphics.Length;i++)graphics[i].enabled=graphics[i]==text;
                foreach(var renderer in renderers)renderer.enabled=false;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                var pixels=image.GetPixels32();
                for(int y=0;y<height;y++)for(int x=0;x<width;x++)
                {
                    var pixel=pixels[y*width+x];
                    if(pixel.a<24||pixel.r+pixel.g+pixel.b<100)continue;
                    visible++;if(!bounds.Contains(new Vector2(x+.5f,y+.5f)))outside++;
                }
            }
            finally
            {
                for(int i=0;i<graphics.Length;i++)if(graphics[i])graphics[i].enabled=enabled[i];
                for(int i=0;i<renderers.Length;i++)if(renderers[i])renderers[i].enabled=rendered[i];
                camera.clearFlags=oldFlags;camera.backgroundColor=oldColor;
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=oldActive;
                UnityEngine.Object.DestroyImmediate(image);
            }
        }

        static List<Metric> Measure(Canvas canvas,Camera camera,RenderTexture target,int width,int height)
        {
            var result=new List<Metric>();
            foreach(var text in canvas.GetComponentsInChildren<Text>(false))
            {
                if(!text.isActiveAndEnabled||string.IsNullOrEmpty(text.text)||text.color.a<.01f)continue;
                string plain=text.supportRichText?Regex.Replace(text.text,"<[^>]*>",""):text.text;
                int size=text.fontSize;
                bool present=text.font;
                if(text.font)
                {
                    foreach(char c in plain)
                        if(!char.IsWhiteSpace(c)&&!text.font.HasCharacter(c))
                            present=false;
                }
                var box=text.rectTransform.rect;
                // Inspect the exact cached mesh used by uGUI, without
                // regenerating BestFit at a guessed logical/pixel size.
                var verts=text.cachedTextGenerator.verts;
                float ppu=text.pixelsPerUnit;
                float x0=float.PositiveInfinity,y0=float.PositiveInfinity;
                float x1=float.NegativeInfinity,y1=float.NegativeInfinity;
                // uGUI appends an unused final quad. Ignore zero-area whitespace.
                for(int i=0;i+3<verts.Count-4;i+=4)
                {
                    var a=verts[i].position/ppu;var b=verts[i+2].position/ppu;
                    if(Mathf.Abs(a.x-b.x)<.01f||Mathf.Abs(a.y-b.y)<.01f)continue;
                    for(int j=0;j<4;j++)
                    {var v=verts[i+j].position/ppu;x0=Mathf.Min(x0,v.x);x1=Mathf.Max(x1,v.x);
                        y0=Mathf.Min(y0,v.y);y1=Mathf.Max(y1,v.y);}
                }
                bool ink= !float.IsInfinity(x0);
                var bounds=ink?Rect.MinMaxRect(x0,y0,x1,y1):Rect.zero;
                const float tolerance=1.5f;
                bool suspect=ink&&(bounds.xMin<box.xMin-tolerance||bounds.xMax>box.xMax+tolerance||
                    bounds.yMin<box.yMin-tolerance||bounds.yMax>box.yMax+tolerance);
                int visible=0,outside=0;
                if(suspect)VisibleInk(text,canvas,camera,target,width,height,out visible,out outside);
                result.Add(new Metric {path=PathOf(text.transform,canvas.transform),text=text.text,
                    role=BreachWatchTypography.ResolveRole(text).ToString(),font=text.font?text.font.name:"MISSING",
                    size=size,configuredSize=text.fontSize,generatorFontSizeUsed=text.cachedTextGenerator.fontSizeUsedForBestFit,
                    box=box,ink=bounds,pixelsPerUnit=ppu,localScale=text.rectTransform.localScale,
                    preferredWidth=text.preferredWidth,preferredHeight=text.preferredHeight,
                    visibleCharacters=text.cachedTextGenerator.characterCountVisible,
                    expectedCharacters=plain.Length,glyphsPresent=present,
                    generatedQuadOutside=suspect,gpuVisiblePixels=visible,gpuOutsidePixels=outside,
                    inkOutside=outside>6});
            }
            return result;
        }

        public static void Capture(Canvas canvas,Camera camera,RenderTexture target,
            string baselineFile,int width,int height)
        {
            if(Environment.GetEnvironmentVariable("VIGILIA_TYPOGRAPHY_AB")!="1")return;
            if(!inkMaskCalibrated)
            {
                var probe=canvas.GetComponentsInChildren<Text>(false).FirstOrDefault(x=>
                    x.isActiveAndEnabled&&x.name=="Actual quantity"&&!string.IsNullOrWhiteSpace(x.text));
                if(!probe)throw new InvalidOperationException("No genuine visible resource text to calibrate glyph mask");
                VisibleInk(probe,canvas,camera,target,width,height,out int visible,out int outside);
                VisibleInk(probe,canvas,camera,target,width,height,out int negativeVisible,out int negativeOutside,true);
                if(visible<8||outside>6||negativeVisible<8||negativeOutside!=negativeVisible)
                    throw new InvalidOperationException("GPU glyph mask calibration failed: positive="+visible+"/"+outside+
                        " negative="+negativeVisible+"/"+negativeOutside);
                inkMaskCalibrated=true;
            }
            if(!BreachWatchTypography.IsOptedIn("https://qa.invalid/?x=1&"+BreachWatchTypography.QueryFlag)||
               BreachWatchTypography.IsOptedIn("https://qa.invalid/#?"+BreachWatchTypography.QueryFlag)||
               BreachWatchTypography.IsOptedIn("https://qa.invalid/?eldoria_typography=source-sans30")||
               BreachWatchTypography.IsOptedIn("https://qa.invalid/"))
                throw new InvalidOperationException("Typography opt-in must require the exact query parameter");
            var snapshots=canvas.GetComponentsInChildren<Text>(true).Select(x=>new State(x)).ToArray();
            var component=canvas.GetComponent<BreachWatchTypography>();
            bool created=!component;if(created)component=canvas.gameObject.AddComponent<BreachWatchTypography>();
            if(component.CandidateEnabled)throw new InvalidOperationException("A/B baseline is already opted in");
            var proof=new Proof {isolatedInkMaskCalibrated=inkMaskCalibrated,baselineFile=Path.GetFileName(baselineFile),
                baselineSha256=Digest(File.ReadAllBytes(baselineFile)),width=width,height=height,
                before=Measure(canvas,camera,target,width,height)};
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            var oldActive=RenderTexture.active;
            string output=Path.Combine(Path.GetDirectoryName(baselineFile),"typography-ab");
            Directory.CreateDirectory(output);
            try
            {
                if(!component.SetCandidate(true))throw new InvalidOperationException("Required imported font absent");
                Canvas.ForceUpdateCanvases();camera.Render();Canvas.ForceUpdateCanvases();camera.Render();
                proof.after=Measure(canvas,camera,target,width,height);
                RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                proof.candidateFile=Path.GetFileNameWithoutExtension(baselineFile)+"-source-sans3.png";
                byte[] bytes=image.EncodeToPNG();File.WriteAllBytes(Path.Combine(output,proof.candidateFile),bytes);
                proof.candidateSha256=Digest(bytes);
                foreach(var item in proof.after)
                {
                    if(!item.glyphsPresent)proof.violations.Add(item.path+": missing imported glyph");
                    if(item.inkOutside)proof.violations.Add(item.path+": actual GPU glyph ink outside own rectangle");
                }
            }
            finally
            {
                component.Restore();
                proof.restored=snapshots.All(x=>x.Same());
                if(created)UnityEngine.Object.DestroyImmediate(component);
                Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=oldActive;
                UnityEngine.Object.DestroyImmediate(image);
                File.WriteAllText(Path.Combine(output,Path.GetFileNameWithoutExtension(baselineFile)+".json"),
                    JsonUtility.ToJson(proof,true));
            }
            if(!proof.restored)throw new InvalidOperationException("Typography A/B did not restore all source Text properties");
            // Preserve every orientation before the workflow rejects any
            // candidate with clipped/missing glyphs; never hide failed images.
            if(proof.violations.Count>0)Debug.LogWarning("VIGILIA_TYPOGRAPHY_CANDIDATE_REJECTED "+string.Join("; ",proof.violations));
        }
    }
}
#endif

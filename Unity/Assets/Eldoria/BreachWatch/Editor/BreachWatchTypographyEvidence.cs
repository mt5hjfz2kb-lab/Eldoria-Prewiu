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
        [Serializable] public sealed class Metric
        {
            public string path,text,role,font;
            public int size,configuredSize,visibleCharacters,expectedCharacters;
            public float preferredWidth,preferredHeight,pixelsPerUnit;
            public Rect box,ink;
            public Vector3 localScale;
            public bool glyphsPresent,inkOutside;
        }
        [Serializable] sealed class Proof
        {
            public string method="SAME_FRAME_IMPORTED_UGUI_FONT_AB_NOT_ART_APPROVAL";
            public string source=Environment.GetEnvironmentVariable("GITHUB_SHA");
            public string baselineFile,baselineSha256,candidateFile,candidateSha256;
            public int width,height;
            public bool restored,artApproved=false;
            public List<Metric> before,after;
            public List<string> violations=new List<string>();
        }
        sealed class State
        {
            public Text Text;public Font Font;public int Size,Min,Max;
            public FontStyle Style;public TextAnchor Alignment;
            public Vector3 Scale;public bool BestFit;public string Value;
            public State(Text t){Text=t;Font=t.font;Size=t.fontSize;Style=t.fontStyle;
                Alignment=t.alignment;Scale=t.rectTransform.localScale;
                BestFit=t.resizeTextForBestFit;Min=t.resizeTextMinSize;
                Max=t.resizeTextMaxSize;Value=t.text;}
            public bool Same()=>!Text||(Text.font==Font&&Text.fontSize==Size&&
                Text.fontStyle==Style&&Text.alignment==Alignment&&
                Text.rectTransform.localScale==Scale&&Text.resizeTextForBestFit==BestFit&&
                Text.resizeTextMinSize==Min&&Text.resizeTextMaxSize==Max&&Text.text==Value);
        }
        static string Digest(byte[] bytes)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
        static string PathOf(Transform item,Transform root)
        {string path=item.name;while(item.parent&&item.parent!=root){item=item.parent;path=item.name+"/"+path;}return path;}

        static List<Metric> Measure(Canvas canvas)
        {
            var result=new List<Metric>();
            foreach(var text in canvas.GetComponentsInChildren<Text>(false))
            {
                if(!text.isActiveAndEnabled||string.IsNullOrEmpty(text.text)||text.color.a<.01f)continue;
                string plain=text.supportRichText?Regex.Replace(text.text,"<[^>]*>",""):text.text;
                // fontSizeUsedForBestFit is the generator's scaled pixel
                // size. Feeding it back as logical fontSize scales it twice
                // on Retina/portrait canvases and invents clipping.
                int size=text.resizeTextForBestFit?Mathf.RoundToInt(
                    text.cachedTextGenerator.fontSizeUsedForBestFit/text.pixelsPerUnit):text.fontSize;
                if(size<=0)size=text.fontSize;
                bool present=text.font;
                if(text.font)
                {
                    text.font.RequestCharactersInTexture(plain,size,text.fontStyle);
                    foreach(char c in plain)
                        if(!char.IsWhiteSpace(c)&&!text.font.GetCharacterInfo(c,out var unused,size,text.fontStyle))
                            present=false;
                }
                var box=text.rectTransform.rect;
                var settings=text.GetGenerationSettings(box.size);
                settings.verticalOverflow=VerticalWrapMode.Overflow;
                // Use the actual imported font and the actual BestFit size,
                // but do not permit the measurement to hide truncated lines.
                settings.resizeTextForBestFit=false;settings.fontSize=size;
                var generator=new TextGenerator();generator.Populate(text.text,settings);
                var verts=generator.verts;
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
                result.Add(new Metric {path=PathOf(text.transform,canvas.transform),text=text.text,
                    role=BreachWatchTypography.ResolveRole(text).ToString(),font=text.font?text.font.name:"MISSING",
                    size=size,configuredSize=text.fontSize,box=box,ink=bounds,pixelsPerUnit=ppu,localScale=text.rectTransform.localScale,
                    preferredWidth=text.preferredWidth,preferredHeight=text.preferredHeight,
                    visibleCharacters=text.cachedTextGenerator.characterCountVisible,
                    expectedCharacters=plain.Length,glyphsPresent=present,
                    inkOutside=ink&&(bounds.xMin<box.xMin-tolerance||bounds.xMax>box.xMax+tolerance||
                        bounds.yMin<box.yMin-tolerance||bounds.yMax>box.yMax+tolerance)});
                ((IDisposable)generator).Dispose();
            }
            return result;
        }

        public static void Capture(Canvas canvas,Camera camera,RenderTexture target,
            string baselineFile,int width,int height)
        {
            if(Environment.GetEnvironmentVariable("VIGILIA_TYPOGRAPHY_AB")!="1")return;
            if(!BreachWatchTypography.IsOptedIn("https://qa.invalid/?x=1&"+BreachWatchTypography.QueryFlag)||
               BreachWatchTypography.IsOptedIn("https://qa.invalid/#?"+BreachWatchTypography.QueryFlag)||
               BreachWatchTypography.IsOptedIn("https://qa.invalid/?eldoria_typography=source-sans30")||
               BreachWatchTypography.IsOptedIn("https://qa.invalid/"))
                throw new InvalidOperationException("Typography opt-in must require the exact query parameter");
            var snapshots=canvas.GetComponentsInChildren<Text>(true).Select(x=>new State(x)).ToArray();
            var component=canvas.GetComponent<BreachWatchTypography>();
            bool created=!component;if(created)component=canvas.gameObject.AddComponent<BreachWatchTypography>();
            if(component.CandidateEnabled)throw new InvalidOperationException("A/B baseline is already opted in");
            var proof=new Proof {baselineFile=Path.GetFileName(baselineFile),
                baselineSha256=Digest(File.ReadAllBytes(baselineFile)),width=width,height=height,
                before=Measure(canvas)};
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            var oldActive=RenderTexture.active;
            string output=Path.Combine(Path.GetDirectoryName(baselineFile),"typography-ab");
            Directory.CreateDirectory(output);
            try
            {
                if(!component.SetCandidate(true))throw new InvalidOperationException("Required imported font absent");
                Canvas.ForceUpdateCanvases();camera.Render();Canvas.ForceUpdateCanvases();camera.Render();
                proof.after=Measure(canvas);
                RenderTexture.active=target;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                proof.candidateFile=Path.GetFileNameWithoutExtension(baselineFile)+"-source-sans3.png";
                byte[] bytes=image.EncodeToPNG();File.WriteAllBytes(Path.Combine(output,proof.candidateFile),bytes);
                proof.candidateSha256=Digest(bytes);
                foreach(var item in proof.after)
                {
                    if(!item.glyphsPresent)proof.violations.Add(item.path+": missing imported glyph");
                    if(item.inkOutside)proof.violations.Add(item.path+": generated glyph ink outside own rectangle");
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

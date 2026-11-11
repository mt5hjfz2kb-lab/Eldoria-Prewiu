#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Eldoria.Domain;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.Issue186Art.Editor
{
    /// <summary>
    /// Native Unity 6000 import + independent UI component proof, on a disposable
    /// EMPTY scene. It does not import the PR173 HUD nor change user saves.
    /// Never conflate this with visual approval in the real campaign.
    /// </summary>
    public static class Issue186NativeEvidence
    {
        const string Folder = "Issue186NativeEvidence";
        const int UiLayer = 5;
        [Serializable] sealed class CaptureInfo
        {
            public string file, sha256, description;
            public int width, height, bytes, actualResourceSprites;
            public bool indicatorReady, newIndicatorReady, firstUsePointer,
                firstUseCompleted, pointerLeavesCtaCenterClear, realUiClick,
                visiblePixels, renderTargetCorrect, noOverlappingCards;
        }
        [Serializable] sealed class Report
        {
            public string status, sourceSha, limitation, error;
            public int captures, realImportedSprites, pointerRaycastHits;
            public bool allResourcesImported, artApproved, actualCityTested, physicalIphoneTested;
            public CaptureInfo[] images;
        }
        static void Need(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("ISSUE186_NATIVE_FAIL " + message);
        }
        static string SHA(string file)
        {
            using(var hash = SHA256.Create())
            using(var stream = File.OpenRead(file))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
        }
        static RectTransform Element(Transform parent,string name,Vector2 center,
            Vector2 size, params Type[] components)
        {
            var types = new List<Type> { typeof(RectTransform) };
            foreach(var type in components)types.Add(type);
            var go = new GameObject(name,types.ToArray());
            go.layer=UiLayer;
            go.transform.SetParent(parent,false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.pivot=new Vector2(.5f,.5f);
            rect.sizeDelta=size;
            rect.anchoredPosition=center;
            return rect;
        }
        static Image Paint(Transform parent,string name,Vector2 center,
            Vector2 size,Color tint)
        {
            var rect=Element(parent,name,center,size,typeof(Image));
            var img=rect.GetComponent<Image>();
            img.color=tint;
            img.raycastTarget=false;
            return img;
        }
        static Text Caption(Transform parent,string name,string caption,
            Vector2 center,Vector2 size,int fontSize,Color color,Font font)
        {
            var rect=Element(parent,name,center,size,typeof(Text));
            var t=rect.GetComponent<Text>();
            t.font=font;t.fontSize=fontSize;
            t.color=color;t.text=caption;t.alignment=TextAnchor.MiddleCenter;
            t.horizontalOverflow=HorizontalWrapMode.Overflow;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.raycastTarget=false;
            return t;
        }
        static bool portraitSize(int width,int height) => width < height;

        static Camera SetupCamera(GameObject holder,int width,int height,
            RenderTexture rt)
        {
            holder.layer=UiLayer;
            var camera=holder.AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.023f,.023f,.028f,1f);
            camera.orthographic=true;
            camera.orthographicSize=height*.005f;
            camera.transform.position=new Vector3(0f,0f,-25f);
            camera.nearClipPlane=.1f;
            camera.farClipPlane=100f;
            camera.aspect=width/(float)height;
            camera.cullingMask=1<<UiLayer;
            camera.targetTexture=rt;
            if(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline!=null)
            {
                var extra=Type.GetType(
                    "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
                Need(extra!=null,"URP package present but additional camera component type unavailable");
                holder.AddComponent(extra);
            }
            return camera;
        }
        // Even if assets import correctly, their use must be evident on
        // actual Unity-rendered cards and the two real attention states.
        static CaptureInfo CaptureOne(string folder,int width,int height,Font font,
            ref int raycastHits)
        {
            var created=new List<GameObject>();
            RenderTexture rt=null;
            Texture2D readback=null;
            RenderTexture previous=RenderTexture.active;
            try
            {
                rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
                rt.Create();
                var cameraGO=new GameObject("Issue186 disposable render camera");
                created.Add(cameraGO);
                var cam=SetupCamera(cameraGO,width,height,rt);
                var canvasGO=new GameObject("Issue186 disposable Canvas",
                    typeof(RectTransform),typeof(Canvas),
                    typeof(CanvasScaler),typeof(GraphicRaycaster));
                created.Add(canvasGO);
                canvasGO.layer=UiLayer;
                var canvas=canvasGO.GetComponent<Canvas>();
                canvas.renderMode=RenderMode.WorldSpace;
                canvas.worldCamera=cam;
                canvas.sortingOrder=15;
                var root=(RectTransform)canvasGO.transform;
                root.sizeDelta=new Vector2(width,height);
                root.localScale=Vector3.one*.01f;
                Paint(root,"isolated slate",Vector2.zero,
                    new Vector2(width,height),new Color(.071f,.054f,.06f,1f));
                Paint(root,"header carved background",
                    new Vector2(0,height*.5f-30),new Vector2(width*.98f,58f),
                    new Color(.14f,.057f,.058f,1f));
                // Guard the headline from BOTH the unread medallion at
                // left and the real claim button at right. The previous
                // centered 72% title overlapped the CTA on 390px phones.
                float claimCenterX=width*.5f-(portraitSize(width,height)?70f:90f);
                float claimWidth=portraitSize(width,height)?92f:118f;
                float titleLeft=-width*.5f+70f;
                float titleRight=claimCenterX-claimWidth*.5f-16f;
                float titleWidth=titleRight-titleLeft;
                Need(titleWidth>150f,"Not enough headline space beside action and alert");
                int titleSize=width<=480?17:width<=900?21:28;
                var heading=Caption(root,"real native heading","RECURSOS DE VALORIA",
                    new Vector2((titleLeft+titleRight)*.5f,height*.5f-30),
                    new Vector2(titleWidth,45),titleSize,
                    new Color(.93f,.79f,.47f,1),font);
                // UI Text must fit the measured box in genuine Unity, not
                // silently paint across the claim action via Overflow.
                while(heading.preferredWidth>titleWidth-8f && heading.fontSize>12)
                    heading.fontSize--;
                Need(heading.preferredWidth<=titleWidth-8f,
                    "Actual native title still overlaps the claim action");

                bool portrait=width<height;
                // Every row is a complete resource family: 100 / 1K / 10K /
                // 100K. Three columns on a phone mixed the last WOOD tier
                // with STONE, making the quantity progression misleading.
                const int columns=4;
                const int rows=3;
                float availableWidth=width*(portrait?.93f:.91f);
                float gapX=portrait?7f:12f;
                float slotWidth=Mathf.Min(portrait?220:235,
                    (availableWidth-(columns-1)*gapX)/columns);
                // Actual asset-size study, not misleading 200px blowups:
                // art direction requires legibility between 48 and 96 px.
                float icon=Mathf.Min(96f,slotWidth-8f,
                    Mathf.Max(48f,(height-(portrait?170f:100f))/rows-30f));
                Need(icon>=48f && icon<=96f,
                    "Viewport cannot display reward items at the approved 48-96px study size");
                float rowH=icon+23f;
                float gapY=portrait?10f:7f;
                float gridHeight=rows*rowH+(rows-1)*gapY;
                // Align the entire comparison grid between the fixed header
                // and mobile safe bottom. Old top-stacking left huge empty
                // lower space in portrait while crowding the header.
                float contentTop=height*.5f-74f;
                float contentBottom=-height*.5f+25f;
                Need(gridHeight<=contentTop-contentBottom,
                    "No vertical room for 3 coherent resource rows: "+width+"x"+height);
                float midY=(contentTop+contentBottom)*.5f;
                float firstCenter=midY+(gridHeight-rowH)*.5f;
                var rects=new List<Rect>();
                string[] kinds={"wood","stone","food"};
                int[] sizes={100,1000,10000,100000};
                int rendered=0;
                for(int i=0;i<12;i++)
                {
                    int col=i%columns, row=i/columns;
                    var x=(col-(columns-1)*.5f)*(slotWidth+gapX);
                    var y=firstCenter-row*(rowH+gapY);
                    var area=new Rect(x-slotWidth*.5f,y-rowH*.5f,
                        slotWidth,rowH);
                    foreach(var old in rects)
                        Need(!area.Overlaps(old),
                            "Overlapping card geometry "+width+"x"+height);
                    rects.Add(area);
                    Paint(root,"dark cartridge "+i,
                        new Vector2(x,y),new Vector2(slotWidth-2,rowH-2),
                        new Color(.14f,.094f,.092f,1));
                    var iconImage=Paint(root,"actual imported packet "+i,
                        new Vector2(x,y+10),new Vector2(icon,icon),
                        Color.white);
                    string kind=kinds[i/sizes.Length];int amount=sizes[i%sizes.Length];
                    var quantity=Caption(root,"authoritative value "+i,"",
                        new Vector2(x,y-rowH*.5f+12),
                        new Vector2(slotWidth-5,22),
                        width<=480?12:16,Color.white,font);
                    Need(Issue186ArtKit.BindPacket(iconImage,quantity,kind,amount),
                        "Could not bind actual sprite "+kind+":"+amount);
                    Need(iconImage.sprite!=null && iconImage.sprite.name==
                        "pack_"+kind+"_"+ResourceRewardRules.TierLabel(amount).ToLowerInvariant(),
                        "Incorrect actual sprite on " + kind);
                    Need(quantity.text==ResourceRewardRules.TierLabel(amount),
                        "Value text not derived from actual denomination");
                    rendered++;
                }
                Need(firstCenter+rowH*.5f<=contentTop+.01f &&
                     firstCenter-(rows-1)*(rowH+gapY)-rowH*.5f>=contentBottom-.01f,
                    "Grid clipped near header/safe bottom "+width+"x"+height);
                var buttonRect=Element(root,"Actual button / Claim",
                    new Vector2(width*.5f-(portrait?70:90),
                        height*.5f-30),new Vector2(portrait?92:118,34),
                    typeof(Image),typeof(Button));
                var button=buttonRect.GetComponent<Button>();
                button.targetGraphic=buttonRect.GetComponent<Image>();
                buttonRect.GetComponent<Image>().color=
                    new Color(.35f,.17f,.09f,.95f);
                Caption(buttonRect,"Actual button label","RECLAMAR",
                    Vector2.zero,new Vector2(90,28),
                    width<=480?10:12,Color.white,font);
                var newBadge=Paint(root,"actual unread badge",
                    new Vector2(-width*.5f+43f,height*.5f-30f),
                    new Vector2(28f,28f),Color.white);
                Need(Issue186ArtKit.BindAttention(newBadge,AttentionLevel.New) &&
                    newBadge.gameObject.activeSelf &&
                    newBadge.sprite.name=="badge_new" && !newBadge.raycastTarget,
                    "Unread badge must be visually distinct and tap-through");
                var badge=Paint(root,"actual claimable badge",
                    new Vector2(width*.5f-24f,height*.5f-30),new Vector2(30,30),
                    Color.white);
                Need(Issue186ArtKit.BindAttention(badge,AttentionLevel.ActionReady) &&
                    badge.gameObject.activeSelf && badge.sprite.name=="badge_claimable",
                    "Claim badge does not reflect actionable event");
                var pointer=Paint(root,"actual tutorial pointer",Vector2.zero,
                    new Vector2(width<=480?72:90,width<=480?72:90),Color.white);
                var halo=Paint(root,"actual tutorial halo",Vector2.zero,
                    new Vector2(54,54),Color.white);
                // Actual interactive control; never show pointer without one.
                var progress=new GuidedTutorialProgress();
                bool pointed=Issue186ArtKit.BindFirstUsePointer(progress,
                    "issue186.claim","ClaimReward",true,canvas,buttonRect,pointer,halo);
                Need(pointed && pointer.gameObject.activeInHierarchy &&
                     !pointer.raycastTarget && !halo.raycastTarget,
                    "Pointer not anchored to a real interactable button");
                // Either the original RIGHT hand or a mirrored LEFT hand
                // is valid. Measure genuine transformed geometry rather than
                // assuming it always approaches from the right; that false
                // assumption rejected every near-edge mobile CTA.
                var handCorners=new Vector3[4];
                pointer.rectTransform.GetWorldCorners(handCorners);
                float handMinX=float.PositiveInfinity;
                float handMaxX=float.NegativeInfinity;
                foreach(var corner in handCorners)
                {
                    float x=root.InverseTransformPoint(corner).x;
                    handMinX=Mathf.Min(handMinX,x);
                    handMaxX=Mathf.Max(handMaxX,x);
                }
                float labelCoreLeft=buttonRect.anchoredPosition.x -
                    buttonRect.rect.width*.30f;
                float labelCoreRight=buttonRect.anchoredPosition.x +
                    buttonRect.rect.width*.30f;
                bool pointerClear=handMaxX<=labelCoreLeft ||
                    handMinX>=labelCoreRight;
                Need(pointerClear,
                    "Tutorial pointer overlaps center 60% of claim label");
                // This specific near-right-edge fixture MUST exercise the
                // mirrored fallback. Otherwise its unmirrored gauntlet would
                // be cropped by the device safe area, even if all images load.
                bool actuallyMirrored=pointer.rectTransform.localScale.x<0f;
                Need(actuallyMirrored,
                    "Near-edge mobile/desktop fixture did not mirror the hand");
                // Test the browser/device-independent UI event flow using a
                // real GraphicRaycaster, not an invented click coordinate.
                var evGO=new GameObject("Disposable event dispatch",
                    typeof(EventSystem),typeof(StandaloneInputModule));
                created.Add(evGO);
                var ev=evGO.GetComponent<EventSystem>();
                Canvas.ForceUpdateCanvases();
                var actualCenter=buttonRect.TransformPoint(buttonRect.rect.center);
                var cursor=RectTransformUtility.WorldToScreenPoint(cam,actualCenter);
                var pd=new PointerEventData(ev){position=cursor};
                var hits=new List<RaycastResult>();
                canvasGO.GetComponent<GraphicRaycaster>().Raycast(pd,hits);
                var realHit=hits.Find(h=>h.gameObject==buttonRect.gameObject);
                Need(realHit.gameObject==buttonRect.gameObject,
                    "No real raycast hit on Claim button");
                raycastHits++;
                bool action=false;
                button.onClick.AddListener(()=>action=true);
                pd.button=PointerEventData.InputButton.Left;
                // Drive the actual Button IPointerClickHandler using the hit
                // returned by Unity GraphicRaycaster. Calling onClick.Invoke()
                // would bypass the control and falsely pass a disabled button.
                ExecuteEvents.Execute<IPointerClickHandler>(
                    realHit.gameObject,pd,ExecuteEvents.pointerClickHandler);
                Need(action,"Unity UI click was not dispatched by the hit Button");
                // A successful Unity Button callback is NOT a successful
                // gateway command. The #186 gameplay bridge is still pending
                // while #181 owns LocalGateway. Never claim tutorial complete
                // merely because our art-only fixture dispatched a click.
                Need(!GuidedTutorialRules.ConfirmAction(progress,"issue186.claim",
                        "ClaimReward","ClaimReward",false) &&
                     !progress.Completed.Contains("issue186.claim"),
                    "UI-only tap falsely completed an unimplemented claim");
                Need(Issue186ArtKit.BindFirstUsePointer(progress,
                        "issue186.claim","ClaimReward",true,canvas,buttonRect,pointer,halo)
                     && pointer.gameObject.activeInHierarchy,
                    "Tutorial must remain visible until an authoritative game result");
                // Capture this honest pending state; source-only domain tests
                // independently exercise completion with actual success=true.

                Canvas.ForceUpdateCanvases();
                // Camera.Render() is not supported for this project's URP.
                // Unity 6 render requests render the full Base Camera stack to
                // the exact target outside the frame loop. Fail closed if not
                // supported instead of uploading a blank "evidence" screenshot.
                if (GraphicsSettings.currentRenderPipeline != null)
                {
                    var request = new RenderPipeline.StandardRequest
                    {
                        destination = rt
                    };
                    Need(RenderPipeline.SupportsRenderRequest(cam, request),
                        "URP camera does not support a StandardRequest");
                    RenderPipeline.SubmitRenderRequest(cam, request);
                    RenderPipeline.SubmitRenderRequest(cam, request);
                }
                else
                {
                    cam.Render();
                    cam.Render();
                }
                RenderTexture.active=rt;
                readback=new Texture2D(width,height,TextureFormat.RGB24,false);
                readback.ReadPixels(new Rect(0,0,width,height),0,0);
                readback.Apply(false);
                var pixels=readback.GetPixels32();
                int interesting=0;
                foreach(var pixel in pixels)
                    if(pixel.r>85 && pixel.g>50 && pixel.b<220)interesting++;
                Need(interesting>width*height*.008f,
                    "Real Unity render is suspiciously blank");
                string file="issue186-real-unity-"+width+"x"+height+".png";
                string full=Path.Combine(folder,file);
                File.WriteAllBytes(full,readback.EncodeToPNG());
                int bytes=(int)new FileInfo(full).Length;
                Need(bytes>8000,"Real native PNG is missing visual content");
                return new CaptureInfo{file=file,sha256=SHA(full),
                    description="Actual Unity uGUI sprites, value text and pointer / NOT integrated city",
                    width=width,height=height,bytes=bytes,actualResourceSprites=rendered,
                    indicatorReady=badge.gameObject.activeInHierarchy,
                    newIndicatorReady=newBadge.gameObject.activeInHierarchy,
                    firstUsePointer=pointed,firstUseCompleted=false,
                    pointerLeavesCtaCenterClear=pointerClear,realUiClick=action,
                    visiblePixels=interesting>width*height*.008f,
                    renderTargetCorrect=cam.targetTexture==rt,
                    noOverlappingCards=true};
            }
            finally
            {
                RenderTexture.active=previous;
                if(readback)Object.DestroyImmediate(readback);
                if(rt){rt.Release();Object.DestroyImmediate(rt);}
                foreach(var obj in created)
                    if(obj)Object.DestroyImmediate(obj);
            }
        }

        [MenuItem("Eldoria/Issue 186/Captura nativa a cuatro resoluciones")]
        public static void Capture()
        {
            var project=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            var output=Path.GetFullPath(Path.Combine(project,Folder));
            Need(Path.GetDirectoryName(output)==project &&
                 Path.GetFileName(output)==Folder,
                "Refuse output outside this isolated Unity proof directory");
            // Do not destroy an existing user's unsaved scene.
            if(!Application.isBatchMode && EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException(
                    "Save or close the active modified scene before using disposable art preview");
            var report=new Report{
                sourceSha=Environment.GetEnvironmentVariable("GITHUB_SHA")??"local",
                limitation="Isolated imported art render only; no campaign, save, phone or final art approval",
                status="RUNNING",artApproved=false,actualCityTested=false,
                physicalIphoneTested=false
            };
            if(Directory.Exists(output))Directory.Delete(output,true);
            Directory.CreateDirectory(output);
            var results=new List<CaptureInfo>();
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                Issue186ArtAudit.VerifyImportedArt();
                report.allResourcesImported=true;
                report.realImportedSprites=16;
                var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                Need(font!=null,"Unity 6000 native preview missing built-in legacy font");
                int hits=0;
                foreach(var size in new[]{(1600,900),(900,1600),(844,390),(390,844)})
                    results.Add(CaptureOne(output,size.Item1,size.Item2,font,ref hits));
                report.pointerRaycastHits=hits;
                report.captures=results.Count;
                report.images=results.ToArray();
                Need(results.Count==4 && hits==4 &&
                    results.TrueForAll(x=>x.actualResourceSprites==12 &&
                        x.indicatorReady && x.newIndicatorReady &&
                        x.firstUsePointer && !x.firstUseCompleted &&
                        x.pointerLeavesCtaCenterClear && x.realUiClick &&
                        x.noOverlappingCards),
                    "At least one real Unity resolution lacks UX/asset proof");
                report.status="UNITY_ISOLATED_IMPORT_PASS_ART_REVIEW_PENDING";
                File.WriteAllText(Path.Combine(output,"report.json"),
                    JsonUtility.ToJson(report,true));
                Debug.Log("ISSUE186_UNITY_ISOLATED_IMPORT_PASS: 4 actual GPU captures");
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                report.status="UNITY_ISOLATED_IMPORT_FAIL";
                report.error=e.ToString();
                report.images=results.ToArray();
                report.captures=results.Count;
                File.WriteAllText(Path.Combine(output,"report.json"),
                    JsonUtility.ToJson(report,true));
                Debug.LogException(e);
                if(Application.isBatchMode)EditorApplication.Exit(1);
                else throw;
            }
        }
    }
}
#endif

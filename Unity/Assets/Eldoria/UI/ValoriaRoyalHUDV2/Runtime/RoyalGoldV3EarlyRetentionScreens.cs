using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Two genuine, self-contained Royal Gold destinations. They share the
    /// EXISTING RoyalHUD safe area, EventSystem, gateway and persistent save.
    /// The shortcuts are genuinely absent until progress unlocks them.
    /// No Codex/Relicario destination and no extra Canvas or gameplay scene.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3EarlyRetentionScreens : MonoBehaviour
    {
        public enum Destination { Vigilants, Scouts }
        public RoyalHUD Hud { get; private set; }
        public bool IsOpen => screen && screen.activeSelf;
        public Destination OpenDestination { get; private set; }
        public RoyalButton VigilantsShortcut => vigilantShortcut;
        public RoyalButton ScoutsShortcut => scoutsShortcut;
        public GameObject VisibleScreen => IsOpen?screen:null;

        ICommandGateway gateway;
        Func<long> now;
        Action refreshed;
        RectTransform safe, screenRect, panelRoot, heroScene, infoCard, victoryBanner;
        CanvasGroup victoryGroup;
        RoyalText victoryTitle, victoryDetail;
        float victoryUntil, victoryStarted;
        RoyalButton vigilantShortcut, scoutsShortcut, back, mainAction, claimXp;
        readonly RoyalButton[] scoutActions=new RoyalButton[3];
        readonly RoyalText[] scoutCards=new RoyalText[3];
        readonly RoyalText[] scoutTitles=new RoyalText[3];
        // Miniatures are crops of the existing, real expedition map art:
        // four mission families share the same no-cost source texture.
        readonly RawImage[] scoutThumbnails=new RawImage[3];
        RawImage engendroPortrait;
        Texture2D engendroTexture;
        Texture2D engendroCutout;
        Rect engendroUvRect=new Rect(0f,0f,1f,1f);
        readonly RoyalText[] scoutActionLabels=new RoyalText[3];
        readonly RoyalGoldV3ScoutContractGraphic[] scoutArt=new RoyalGoldV3ScoutContractGraphic[3];
        readonly string[] scoutIds=new string[3];
        RoyalText heading, kicker, introduction, chapter, opponent, formation,
            reward, battleFeedback, passive, footer, mainActionLabel, xpLabel, backLabel;
        RoyalGoldV3EarlyRetentionArtwork sceneArtwork;
        RoyalGoldV3StageTrailGraphic chapterTrail;
        readonly RoyalText[] bossMilestoneLabels=new RoyalText[5];
        RoyalText currentMilestoneLabel;
        // Two original zero-cost painterly environments, with the hand-built
        // vector renderer as the graceful fallback in older installations.
        RawImage paintedScene;
        Texture2D paintedVigilants, paintedScouts;
        GameObject screen;
        float width=-1,height=-1,nextUpdate;
        float lastSafeWidth=-1,lastSafeHeight=-1;
        bool compactLandscape;
        struct TextDensityState
        {
            public int FontSize;
            public Vector2 SizeDelta;
            public Vector3 Scale;
        }
        readonly Dictionary<RoyalText,TextDensityState> textDensity=
            new Dictionary<RoyalText,TextDensityState>();
        void RestoreTextDensity()
        {
            foreach(var item in textDensity)
            {
                if(!item.Key)continue;
                item.Key.fontSize=item.Value.FontSize;
                item.Key.rectTransform.sizeDelta=item.Value.SizeDelta;
                item.Key.rectTransform.localScale=item.Value.Scale;
            }
            textDensity.Clear();
        }
        void ApplyTextDensity()
        {
            // uGUI generates dynamic glyphs using Canvas.scaleFactor, which
            // excludes an enlarged room transform. Request the larger glyph
            // and inversely scale its enlarged box: same visible geometry,
            // crisp native pixels rather than magnified 8-pixel letters.
            float density=panelRoot.localScale.x;
            if(!compactLandscape || density<=1f)return;
            foreach(var label in panelRoot.GetComponentsInChildren<RoyalText>(true))
            {
                var r=label.rectTransform;
                textDensity[label]=new TextDensityState{
                    FontSize=label.fontSize,SizeDelta=r.sizeDelta,Scale=r.localScale};
                var size=r.rect.size;
                label.fontSize=Mathf.CeilToInt(label.fontSize*density);
                r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x*density);
                r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y*density);
                r.localScale/=density;
            }
        }
        string notice="";
        static readonly Color Ivory=new Color(.96f,.91f,.80f,1);
        static readonly Color Gold=new Color(.87f,.70f,.40f,1);
        static readonly Color Ink=new Color(.13f,.18f,.23f,1);
        static readonly Color Muted=new Color(.76f,.76f,.73f,1);
        bool ready;

        public void Initialize(RoyalHUD canonicalHud,ICommandGateway commands,
            Func<long> utcTicks,Action onStateChanged)
        {
            if(Hud!=canonicalHud)
            {
                if(Hud){Hud.NarrativeOpening-=Close;Hud.SelectionChanged-=SelectionChanged;}
                Hud=canonicalHud;
                if(Hud){Hud.NarrativeOpening+=Close;Hud.SelectionChanged+=SelectionChanged;}
            }
            gateway=commands;
            now=utcTicks;
            refreshed=onStateChanged;
            nextUpdate=0;
        }
        void OnDestroy()
        {
            if(Hud){Hud.NarrativeOpening-=Close;Hud.SelectionChanged-=SelectionChanged;}
            if(engendroCutout)Destroy(engendroCutout);
        }
        // The reused Bastion-II render has a neutral studio backdrop.
        // Remove only bright low-chroma backdrop pixels ONCE, retaining the
        // original model, its violet corruption and all canonical gameplay.
        // No external service, shader, GPU readback or new art is required.
        static Texture2D TransparentEngendro(Texture2D source,out Rect portraitUv)
        {
            portraitUv=new Rect(0f,0f,1f,1f);
            if(!source)return null;
            try
            {
                var pixels=source.GetPixels32();
                int minX=source.width,minY=source.height,maxX=-1,maxY=-1;
                for(int i=0;i<pixels.Length;i++)
                {
                    var p=pixels[i];
                    int brightest=Math.Max(p.r,Math.Max(p.g,p.b));
                    int darkest=Math.Min(p.r,Math.Min(p.g,p.b));
                    int chroma=brightest-darkest;
                    int light=(p.r+p.g+p.b)/3;
                    if(chroma<25 && light>125)p.a=0;
                    else if(chroma<38 && light>93)
                        p.a=(byte)Mathf.Clamp((125-light)*8,0,255);
                    if(p.a>100)
                    {
                        int x=i%source.width,y=i/source.width;
                        minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);
                        minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);
                    }
                    pixels[i]=p;
                }
                if(maxX>=minX&&maxY>=minY)
                {
                    // Trim studio negative space so a small original model
                    // reads like the substantial enemy in the approved art.
                    // UV crop is reused by RawImage; source pixels stay owned
                    // by the unchanged canonical creature asset.
                    float dx=Mathf.Max(4f,(maxX-minX+1)*.16f);
                    float dy=Mathf.Max(4f,(maxY-minY+1)*.16f);
                    float left=Mathf.Clamp01((minX-dx)/source.width);
                    float bottom=Mathf.Clamp01((minY-dy)/source.height);
                    float right=Mathf.Clamp01((maxX+1f+dx)/source.width);
                    float top=Mathf.Clamp01((maxY+1f+dy)/source.height);
                    portraitUv=new Rect(left,bottom,Mathf.Max(.04f,right-left),
                        Mathf.Max(.04f,top-bottom));
                }
                var cutout=new Texture2D(source.width,source.height,
                    TextureFormat.RGBA32,false,false);
                cutout.name="engendro-lore-cutout";
                cutout.filterMode=FilterMode.Bilinear;
                cutout.wrapMode=TextureWrapMode.Clamp;
                cutout.SetPixels32(pixels);
                cutout.Apply(false,true);
                return cutout;
            }
            catch(Exception e)
            {
                Debug.LogWarning("Eldoria Engendro portrait fallback: "+e.Message);
                return null;
            }
        }
        void SelectionChanged(string selected)
        {
            if(!string.IsNullOrEmpty(selected))Close();
        }
        bool Blocked=>Hud==null||Hud.NarrativeOpen ||
            (Hud.Modal && Hud.Modal.gameObject.activeSelf);
        static RectTransform Rect(string name,Transform parent)
        {
            var g=new GameObject(name,typeof(RectTransform));
            g.transform.SetParent(parent,false);
            g.layer=parent.gameObject.layer;
            return (RectTransform)g.transform;
        }
        static void Stretch(RectTransform rect,float inset=0)
        {
            rect.anchorMin=Vector2.zero;
            rect.anchorMax=Vector2.one;
            rect.offsetMin=new Vector2(inset,inset);
            rect.offsetMax=new Vector2(-inset,-inset);
        }
        static void Pos(RectTransform r,Vector2 anchor,Vector2 pivot,
            float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;
            r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);
        }
        static void TL(RectTransform r,float x,float y,float w,float h)
            =>Pos(r,new Vector2(0,1),new Vector2(0,1),x,-y,w,h);
        static RoyalText Text(Transform parent,string id,int size,Color tint,bool bold=false)
        {
            var r=Rect(id,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var t=r.gameObject.AddComponent<RoyalText>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize=size;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            t.color=tint;t.raycastTarget=false;
            t.alignment=TextAnchor.MiddleLeft;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            return t;
        }
        static RectTransform Panel(Transform parent,string name,bool catches=false)
        {
            var r=Rect(name,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var plate=r.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            plate.Primary=false;plate.DecorativeCorners=true;
            plate.raycastTarget=catches;
            return r;
        }
        static RoyalButton Button(Transform parent,string name,string label,
            Action action,out RoyalText caption,bool primary=false,bool? battleStyle=null)
        {
            // Unity permits exactly one Graphic per GameObject. Choose the
            // final surface at creation instead of disabling one Graphic and
            // attempting to add a second incompatible component.
            var r=Rect(name,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            Graphic surface;
            if(battleStyle.HasValue)
            {
                var plate=r.gameObject.AddComponent<RoyalGoldV3RetentionActionGraphic>();
                plate.Battle=battleStyle.Value;
                surface=plate;
            }
            else
            {
                var plate=r.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
                plate.Primary=primary;
                plate.DecorativeCorners=false;
                surface=plate;
            }
            surface.raycastTarget=true;
            var b=r.gameObject.AddComponent<RoyalButton>();
            b.targetGraphic=surface;
            caption=Text(r,"Original gilded label",17,Ivory,true);
            caption.alignment=TextAnchor.MiddleCenter;
            Stretch(caption.rectTransform,5);
            caption.Put(label);
            b.onClick.AddListener(()=>action?.Invoke());
            return b;
        }
        static void DecorateShortcut(RoyalButton button,RoyalText caption,string source)
        {
            if(!button||!caption)return;
            Sprite sprite=Resources.Load<Sprite>(source);
            if(!sprite)return;
            var art=Rect("Approved Valoria heraldic emblem",button.transform);
            art.gameObject.AddComponent<CanvasRenderer>();
            var icon=art.gameObject.AddComponent<Image>();
            icon.sprite=sprite;
            icon.preserveAspect=true;
            icon.raycastTarget=false;
            Pos(art,new Vector2(0,.5f),new Vector2(0,.5f),8f,0f,37f,37f);
            var captionRect=caption.rectTransform;
            captionRect.offsetMin=new Vector2(45f,5f);
            captionRect.offsetMax=new Vector2(-5f,-5f);
            caption.fontSize=14;
            caption.SetAllDirty();
        }
        void EnsureView()
        {
            if(ready||!Hud)return;
            var safeArea=Hud.GetComponentInChildren<RoyalHudLayout>(true);
            safe=safeArea?safeArea.transform as RectTransform:null;
            if(!safe)return;
            vigilantShortcut=Button(safe,"Live Vigilants unlocked access",
                "VIGILANTES",()=>Open(Destination.Vigilants),out var a);
            scoutsShortcut=Button(safe,"Live Scouts unlocked access",
                "EXPLORADORES",()=>Open(Destination.Scouts),out var b);
            // Reuse genuine Eldoria's licensed recovered royal emblems, not
            // primitive substitutes. These images are ornamental siblings
            // inside the SAME actionable original RoyalButton.
            DecorateShortcut(vigilantShortcut,a,"EldoriaRetention/vigilants-compass");
            DecorateShortcut(scoutsShortcut,b,"EldoriaRetention/scouts-crest");
            // Never flash a locked action on the first Unity frame.
            vigilantShortcut.gameObject.SetActive(false);
            scoutsShortcut.gameObject.SetActive(false);
            // Shortcut belongs to the city HUD, NOT the Códice or the
            // bottom five-destination progression/navigation rail.
            screenRect=Rect("Early retention / independent destination",safe);
            screen=screenRect.gameObject;
            screenRect.gameObject.AddComponent<CanvasRenderer>();
            var curtain=screenRect.gameObject.AddComponent<Image>();
            curtain.color=new Color(.013f,.023f,.041f,.985f);
            curtain.raycastTarget=true;
            Stretch(screenRect,0);
            var panel=Panel(screenRect,"Independent royal manuscript room");
            panelRoot=panel;
            Stretch(panel,9);
            heading=Text(panel,"Page heading",32,Gold,true);
            kicker=Text(panel,"Chapter overline",17,Muted,true);
            introduction=Text(panel,"Mission introduction",18,Ivory);
            var art=Panel(panel,"Scenic illustration");
            heroScene=art;
            // Scenic trees stay inside the manuscript frame in either aspect ratio.
            art.gameObject.AddComponent<RectMask2D>();
            var picture=Rect("Original hand-drawn scenery",art);
            picture.gameObject.AddComponent<CanvasRenderer>();
            sceneArtwork=picture.gameObject.AddComponent<RoyalGoldV3EarlyRetentionArtwork>();
            sceneArtwork.raycastTarget=false;
            Stretch(picture,5);
            var painting=Rect("Original illustrated world / decorative only",art);
            painting.gameObject.AddComponent<CanvasRenderer>();
            paintedScene=painting.gameObject.AddComponent<RawImage>();
            paintedScene.raycastTarget=false;
            paintedScene.color=Color.white;
            Stretch(painting,5);
            // Original cinematic scenery adapted from the art direction
            // approved for both activities. Gameplay markers, labels and buttons
            // remain separate, live Unity controls layered above the painting.
            paintedVigilants=Resources.Load<Texture2D>("EldoriaRetention/vigilants-painterly-v2")
                ?? Resources.Load<Texture2D>("EldoriaRetention/vigilants-painterly");
            paintedScouts=Resources.Load<Texture2D>("EldoriaRetention/scouts-painterly-v2")
                ?? Resources.Load<Texture2D>("EldoriaRetention/scouts-painterly");
            // Keep the earlier zero-cost procedural texture as resilient fallback.
            if(!paintedVigilants)paintedVigilants=
                Resources.Load<Texture2D>("EldoriaRetention/vigilants-illustration");
            if(!paintedScouts)paintedScouts=
                Resources.Load<Texture2D>("EldoriaRetention/scouts-illustration");
            paintedScene.enabled=paintedVigilants!=null;
            // Decorative art stays BELOW the existing 25-stage route and
            // ABOVE the original fallback illustration, never intercepting taps.
            // A full 25-stage visual route; purely decorative, real
            // combat still goes through the single canonical action.
            var trail=Rect("Twenty-five connected stage medallions",art);
            trail.gameObject.AddComponent<CanvasRenderer>();
            chapterTrail=trail.gameObject.AddComponent<RoyalGoldV3StageTrailGraphic>();
            chapterTrail.raycastTarget=false;
            trail.anchorMin=new Vector2(.10f,.06f);
            trail.anchorMax=new Vector2(.90f,.54f);
            trail.offsetMin=trail.offsetMax=Vector2.zero;
            // Five numbered guardians and a current-stage marker share the
            // actual 25-step geometry, never an unrelated decorative path.
            for(int i=0;i<5;i++)
            {
                bossMilestoneLabels[i]=Text(trail,
                    "Guardian milestone "+((i+1)*5),16,Ivory,true);
                bossMilestoneLabels[i].alignment=TextAnchor.MiddleCenter;
                bossMilestoneLabels[i].Put(((i+1)*5).ToString());
            }
            currentMilestoneLabel=Text(trail,"Current live stage",17,Ink,true);
            currentMilestoneLabel.alignment=TextAnchor.MiddleCenter;
            // The stage-15 elite is the canonical Bastion-II Spawnling,
            // not an invented stone golem. Reuse its actual authored image.
            engendroTexture=Resources.Load<Texture2D>(
                "EldoriaRetention/engendro-portrait");
            engendroCutout=TransparentEngendro(engendroTexture,out engendroUvRect);
            var creature=Panel(heroScene,"Lore-approved Engendro portrait");
            // The original Breach creature is now a transparent silhouette:
            // do not surround it with a studio-like gray/gold square over the
            // living forest. It belongs IN the landscape, not on an ID card.
            var creaturePlate=creature.GetComponent<RoyalGoldV2FrameGraphic>();
            if(creaturePlate){creaturePlate.raycastTarget=false;creaturePlate.enabled=false;}
            var creatureArt=Rect("Engendro / authentic creature illustration",creature);
            creatureArt.gameObject.AddComponent<CanvasRenderer>();
            engendroPortrait=creatureArt.gameObject.AddComponent<RawImage>();
            engendroPortrait.raycastTarget=false;
            engendroPortrait.texture=engendroCutout?engendroCutout:engendroTexture;
            engendroPortrait.uvRect=engendroCutout?engendroUvRect:
                new Rect(0f,0f,1f,1f);
            Stretch(creatureArt,4f);
            creature.gameObject.SetActive(false);
            victoryBanner=Panel(heroScene,"Crónica Real / combat outcome gilded announcement");
            var victoryPlate=victoryBanner.GetComponent<RoyalGoldV2FrameGraphic>();
            victoryPlate.Primary=true;
            victoryPlate.raycastTarget=false;
            victoryTitle=Text(victoryBanner,"Original victory heading",25,Gold,true);
            victoryTitle.alignment=TextAnchor.MiddleCenter;
            victoryDetail=Text(victoryBanner,"Authoritative victory reward",16,Ivory);
            victoryDetail.alignment=TextAnchor.MiddleCenter;
            victoryGroup=victoryBanner.gameObject.AddComponent<CanvasGroup>();
            victoryGroup.alpha=0f;
            victoryGroup.blocksRaycasts=false;
            victoryGroup.interactable=false;
            victoryBanner.SetAsLastSibling();
            infoCard=Panel(panel,"Progression and expedition information");
            chapter=Text(infoCard,"Stage progression",23,Gold,true);
            opponent=Text(infoCard,"Real enemy and trait",19,Ivory);
            formation=Text(infoCard,"Actual available army",17,Muted);
            reward=Text(infoCard,"Authoritative first victory reward",17,Gold);
            passive=Text(infoCard,"Offline hero experience",18,Ivory);
            battleFeedback=Text(infoCard,"Last action result",17,Ivory);
            mainAction=Button(infoCard,"Battle against next unique stage",
                "COMBATIR",ExecuteStage,out mainActionLabel,true,true);
            claimXp=Button(infoCard,"Claim earned hero experience",
                "RECOGER EXPERIENCIA",CollectXp,out xpLabel);
            for(int i=0;i<3;i++)
            {
                int index=i;
                var cell=Panel(infoCard,"Royal scout order "+(i+1));
                // An illustrated mission CARD, with separate colored cloth
                // crest, aged parchment and a real wax seal. Backing is purely
                // visual: the existing action remains the one touch target.
                var backplate=cell.GetComponent<RoyalGoldV2FrameGraphic>();
                if(backplate){backplate.enabled=false;backplate.raycastTarget=false;}
                var artwork=Rect("Crónica Real / illuminated scout contract",cell);
                artwork.gameObject.AddComponent<CanvasRenderer>();
                scoutArt[i]=artwork.gameObject.AddComponent<RoyalGoldV3ScoutContractGraphic>();
                scoutArt[i].Kind=i;
                scoutArt[i].raycastTarget=false;
                Stretch(artwork,0);
                artwork.SetAsFirstSibling();
                var vignette=Rect("Mission map detail (visual only)",cell);
                vignette.gameObject.AddComponent<CanvasRenderer>();
                scoutThumbnails[i]=vignette.gameObject.AddComponent<RawImage>();
                scoutThumbnails[i].raycastTarget=false;
                scoutThumbnails[i].texture=paintedScouts;
                scoutThumbnails[i].uvRect=new Rect(.04f+i*.27f,.14f,.22f,.56f);
                scoutThumbnails[i].gameObject.SetActive(false);
                scoutTitles[i]=Text(cell,"Readable contract heading",22,Ink,true);
                scoutCards[i]=Text(cell,"Real scout order reward and status",18,Ink);
                scoutActions[i]=Button(cell,"Dispatch or claim scout "+(i+1),
                    "ENVIAR",()=>RunScout(index),out scoutActionLabels[i],true,false);
                scoutActions[i].transform.SetParent(cell,false);
                cell.name="Scout / live contract "+(i+1);
            }
            footer=Text(panel,"No artificial notifications",15,Muted);
            back=Button(panel,"Return to actual kingdom","VOLVER A VALORIA",
                Close,out backLabel);
            screen.SetActive(false);
            ready=true;
        }
        public void Open(Destination destination)
        {
            if(gateway==null||Blocked)return;
            EnsureView();
            if(!ready)return;
            var s=gateway.Snapshot();
            if(destination==Destination.Vigilants&&!EarlyRetentionRules.VigilantsUnlocked(s))return;
            if(destination==Destination.Scouts&&!EarlyRetentionRules.ScoutsUnlocked(s))return;
            OpenDestination=destination;
            notice="";
            victoryUntil=0f;
            if(victoryGroup)victoryGroup.alpha=0f;
            if(victoryBanner)victoryBanner.localScale=Vector3.one;
            screen.SetActive(true);
            screenRect.SetAsLastSibling();
            width=-1;height=-1;
            Layout();
            Render(true);
        }
        public void Close()
        {
            if(screen&&screen.activeSelf)screen.SetActive(false);
        }
        void Submit(string kind,string target)
        {
            if(gateway==null)return;
            var s=gateway.Snapshot();
            var cmd=new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,kind,target,s.Revision);
            var result=gateway.Execute(cmd);
            notice=result.Message;
            if(kind=="VigilantBattle" && result.Ok && IsOpen &&
               OpenDestination==Destination.Vigilants)
            {
                var report=gateway.Snapshot().LastBattleReport;
                if(report!=null && report.TargetId=="vigilant:"+target)
                {
                    victoryTitle.Put(report.Won?"VICTORIA":"DERROTA");
                    victoryTitle.color=report.Won?Gold:new Color(.93f,.51f,.41f,1f);
                    victoryDetail.Put(result.Message);
                    victoryStarted=Time.unscaledTime;
                    victoryUntil=victoryStarted+3.2f;
                    victoryGroup.alpha=1f;
                    victoryGroup.blocksRaycasts=false;
                }
            }
            if(result.Ok && IsOpen && OpenDestination==Destination.Scouts &&
               (kind=="ScoutDispatch"||kind=="ScoutClaim"))
            {
                // Small celebratory acknowledgement without inventing loot,
                // blocking the UI, or creating another management screen.
                victoryTitle.Put(kind=="ScoutClaim"?"ENCARGO COMPLETADO":
                    "EXPLORADORES EN CAMINO");
                victoryTitle.color=Gold;
                victoryDetail.Put(result.Message);
                victoryStarted=Time.unscaledTime;
                victoryUntil=victoryStarted+(kind=="ScoutClaim"?2.7f:1.9f);
                victoryGroup.alpha=1f;
                victoryGroup.blocksRaycasts=false;
            }
            refreshed?.Invoke();
            Render(true);
        }
        void ExecuteStage()
        {
            if(gateway==null)return;
            var s=gateway.Snapshot();
            int next=s.Retention==null?1:s.Retention.LastClearedStage+1;
            if(EarlyRetentionRules.StageUnlocked(s,next))Submit("VigilantBattle",next.ToString());
        }
        void CollectXp() => Submit("ClaimVigilantXp","aldric");
        void RunScout(int index)
        {
            if(index<0||index>=3||string.IsNullOrEmpty(scoutIds[index]))return;
            var s=gateway.Snapshot();
            var item=s.Retention?.Orders?.FirstOrDefault(x=>x.Id==scoutIds[index]);
            if(item==null||item.Claimed)return;
            if(item.StartedUtcTicks==0)Submit("ScoutDispatch",item.Id);
            else if(item.EndsUtcTicks<=now())Submit("ScoutClaim",item.Id);
        }
        // Run after a genuine native offscreen canvas resize, just as the
        // existing Management destination does. Never create a new canvas.
        public void SynchronizeLayout()
        {
            if(!IsOpen)return;
            Layout();
            Render(true);
        }
        void Update()
        {
            if(Hud==null||gateway==null||now==null)return;
            EnsureView();
            if(!ready)return;
            if(victoryGroup && victoryGroup.alpha>0f)
            {
                float remaining=victoryUntil-Time.unscaledTime;
                victoryGroup.alpha=Mathf.Clamp01(remaining*2f);
                // Gentle, frame-rate independent acknowledgement. The result
                // is always calculated and committed by the existing gateway;
                // this only animates its already-real gold-framed banner.
                float elapsed=Mathf.Max(0f,Time.unscaledTime-victoryStarted);
                float spring=1f-Mathf.Exp(-elapsed*11f);
                victoryBanner.localScale=Vector3.one*(.95f+.05f*spring);
            }
            if(Time.unscaledTime<nextUpdate)return;
            nextUpdate=Time.unscaledTime+.3f;
            var s=gateway.Snapshot();
            bool overlay=Blocked || !string.IsNullOrEmpty(Hud.SelectedBuildingId);
            bool allowV=EarlyRetentionRules.VigilantsUnlocked(s);
            bool allowS=EarlyRetentionRules.ScoutsUnlocked(s);
            if(vigilantShortcut.gameObject.activeSelf!=(!overlay&&allowV))
                vigilantShortcut.gameObject.SetActive(!overlay&&allowV);
            if(scoutsShortcut.gameObject.activeSelf!=(!overlay&&allowS))
                scoutsShortcut.gameObject.SetActive(!overlay&&allowS);
            if(IsOpen)
            {
                if(Blocked || (OpenDestination==Destination.Vigilants&&!allowV) ||
                   (OpenDestination==Destination.Scouts&&!allowS)){Close();return;}
                if(Mathf.Abs(safe.rect.width-lastSafeWidth)>1||Mathf.Abs(safe.rect.height-lastSafeHeight)>1)
                    Layout();
                Render(false);
            }
            else if(Mathf.Abs(safe.rect.width-lastSafeWidth)>1||Mathf.Abs(safe.rect.height-lastSafeHeight)>1)
                LayoutShortcuts();
        }
        void LayoutShortcuts()
        {
            if(!safe||!vigilantShortcut||!scoutsShortcut)return;
            float w=safe.rect.width,h=safe.rect.height;
            if(w<200||h<220)return;
            bool portrait=h>w;
            float navHeight=portrait?165f:118f;
            // Leave the existing five-medallion dock and object actions clear.
            float bw=portrait?Mathf.Min(158f,(w-26f)*.48f):175f;
            float bh=portrait?48f:50f;
            Pos((RectTransform)vigilantShortcut.transform,
                new Vector2(0,0),new Vector2(0,0),
                10f,navHeight+10f,bw,bh);
            Pos((RectTransform)scoutsShortcut.transform,
                new Vector2(0,0),new Vector2(0,0),
                16f+bw,navHeight+10f,bw,bh);
        }
        void Layout()
        {
            if(!ready||!safe)return;
            RestoreTextDensity();
            lastSafeWidth=safe.rect.width;lastSafeHeight=safe.rect.height;
            LayoutShortcuts();
            if(lastSafeWidth<200||lastSafeHeight<260)return;
            // RoyalScaler uses a 900-unit reference HEIGHT in production.
            // Give only this room a stable portrait width, leaving the shared
            // HUD, camera, scaler and safe-area/shortcut controls untouched.
            bool portrait=lastSafeHeight>lastSafeWidth;
            float pixelScale=Hud.GetComponent<Canvas>().scaleFactor;
            compactLandscape=!portrait &&
                ((UnityEngine.Application.isMobilePlatform && lastSafeWidth/lastSafeHeight>=1.55f) ||
                 (lastSafeWidth*pixelScale<1100f && lastSafeHeight*pixelScale<650f));
            float roomScale=portrait?Mathf.Min(1f,lastSafeWidth/800f):
                compactLandscape?lastSafeHeight/600f:1f;
            width=lastSafeWidth/roomScale;height=lastSafeHeight/roomScale;
            panelRoot.anchorMin=panelRoot.anchorMax=new Vector2(.5f,.5f);
            panelRoot.pivot=new Vector2(.5f,.5f);
            panelRoot.anchoredPosition=Vector2.zero;
            panelRoot.sizeDelta=new Vector2(width-18f,height-18f);
            panelRoot.localScale=Vector3.one*roomScale;
            // Every orientation-dependent font is assigned on each layout.
            // A phone capture/rotation must not leave its larger subtitle or
            // action labels inside the next landscape rectangles.
            kicker.fontSize=portrait?26:compactLandscape?20:18;
            mainActionLabel.fontSize=portrait?28:compactLandscape?22:17;
            xpLabel.fontSize=portrait?26:compactLandscape?20:17;
            float outer=9f;
            float pw=width-outer*2,ph=height-outer*2;
            var panel=panelRoot;
            float bodyX=portrait?18f:pw*.48f;
            float bodyWidth=portrait?pw-36f:pw*.49f-18f;
            // Two-line real destination title in portrait: do not force a long
            // Spanish heading into the narrow strip beside the Back button.
            // Keep the complete name and the actual tap target legible.
            if(portrait)
            {
                TL(heading.rectTransform,18f,12f,pw-200f,86f);
                heading.fontSize=36;
                TL(kicker.rectTransform,18f,104f,pw-36f,36f);
                TL((RectTransform)back.transform,pw-176f,14f,160f,88f);
                backLabel.Put("VOLVER");
                backLabel.fontSize=26;
            }
            else
            {
                TL(heading.rectTransform,20f,18f,pw-170f,44f);
                heading.fontSize=compactLandscape?26:34;
                TL(kicker.rectTransform,20f,61f,pw-35f,27f);
                TL((RectTransform)back.transform,pw-150f,compactLandscape?14f:23f,136f,compactLandscape?68f:46f);
                backLabel.Put("VOLVER");
                backLabel.fontSize=compactLandscape?20:17;
            }
            float imageTop=portrait?150f:108f;
            bool scoutsLandscape=!portrait && OpenDestination==Destination.Scouts;
            bool vigilantsLandscape=!portrait && OpenDestination==Destination.Vigilants;
            bool vigilantsPortrait=portrait && OpenDestination==Destination.Vigilants;
            // The approved reference is scenic-first. In wide views the landscape
            // or cartographer's desk spans the available width; actual missions
            // and the combat panel float over the LOWER portion of that artwork.
            // Portrait keeps the existing proven stacked, safe-area composition.
            // Vigilants has one encounter card: fill portrait with the world
            // instead of leaving a long dark strip before the battle button.
            // Scouts retains three full-size readable contracts beneath its map.
            float scenicHeight=vigilantsPortrait?Mathf.Max(220f,ph-840f):
                portrait?Mathf.Clamp(ph-988f,220f,880f):ph-155f;
            TL(heroScene,portrait?18f:20f,imageTop,
                portrait?pw-36f:pw-40f,scenicHeight);
            if(vigilantsLandscape)
            {
                chapterTrail.rectTransform.anchorMin=new Vector2(.04f,.13f);
                chapterTrail.rectTransform.anchorMax=new Vector2(.34f,.86f);
            }
            else
            {
                chapterTrail.rectTransform.anchorMin=new Vector2(.10f,.06f);
                chapterTrail.rectTransform.anchorMax=new Vector2(.90f,.54f);
            }
            chapterTrail.rectTransform.offsetMin=Vector2.zero;
            chapterTrail.rectTransform.offsetMax=Vector2.zero;
            if(engendroPortrait)
            {
                var frame=engendroPortrait.transform.parent as RectTransform;
                float w=Mathf.Min(heroScene.rect.width*(portrait?.48f:.37f),portrait?360f:330f);
                float h=Mathf.Min(heroScene.rect.height*.49f,portrait?360f:310f);
                float artAspect=engendroTexture?engendroUvRect.width*engendroTexture.width/
                    Mathf.Max(1f,engendroUvRect.height*engendroTexture.height):1f;
                if(w/h>artAspect)w=h*artAspect;else h=w/artAspect;
                TL(frame,portrait?16f:26f,
                    Mathf.Max(90f,heroScene.rect.height-h-19f),w,h);
            }
            if(victoryBanner)
            {
                // A genuine, readable moment of triumph: larger gilded
                // announcement, still only three seconds and never a modal.
                // The real gateway result supplies the reward text unchanged.
                float bannerWidth=Mathf.Min(heroScene.rect.width-24f,
                    portrait?620f:615f);
                float bannerHeight=208f;
                Pos(victoryBanner,new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                    0f,0f,bannerWidth,bannerHeight);
                TL(victoryTitle.rectTransform,15f,10f,bannerWidth-30f,
                    portrait?84f:90f);
                TL(victoryDetail.rectTransform,16f,portrait?100f:107f,
                    bannerWidth-32f,portrait?96f:89f);
                victoryTitle.fontSize=portrait?32:36;
                victoryDetail.fontSize=portrait?26:25;
            }
            // Keep the portrait manuscript above the fixed bottom footer.
            // The final scout card and claim-XP button previously extended
            // under this footer even though both were inside the safe area.
            if(portrait)
                TL(infoCard,bodyX,imageTop+scenicHeight+10f,
                    bodyWidth,ph-(imageTop+scenicHeight+58f));
            else if(compactLandscape)
                TL(infoCard,scoutsLandscape?18f:pw*.48f,imageTop+8f,
                    scoutsLandscape?pw-36f:pw*.50f-20f,ph-imageTop-56f);
            else if(scoutsLandscape)
                TL(infoCard,18f,ph*.49f,pw-36f,ph*.51f-61f);
            else
                TL(infoCard,pw*.37f,ph*.55f,pw*.61f,ph*.45f-55f);
            float iw=infoCard.rect.width,ih=infoCard.rect.height;
            if(iw<=0||ih<=0)return;
            if(vigilantsPortrait)
            {
                // Layout in canvas units must remain readable after the real
                // 390px capture scales the 800-unit portrait canvas by 0.4875.
                TL(chapter.rectTransform,14f,12f,iw-28f,44f);
                TL(opponent.rectTransform,14f,62f,iw-28f,78f);
                TL(formation.rectTransform,14f,148f,iw-28f,36f);
                TL(reward.rectTransform,14f,192f,iw-28f,72f);
                TL(passive.rectTransform,14f,272f,iw-28f,70f);
                TL(battleFeedback.rectTransform,14f,350f,iw-28f,40f);
                TL((RectTransform)mainAction.transform,14f,ih-200f,iw-28f,90f);
                TL((RectTransform)claimXp.transform,14f,ih-100f,iw-28f,88f);
                chapter.fontSize=32;opponent.fontSize=32;formation.fontSize=26;
                reward.fontSize=26;passive.fontSize=26;battleFeedback.fontSize=26;
            }
            else if(portrait)
            {
                // Scouts occupies three real contracts and keeps their original
                // touch-safe height in vertical and narrow phone layouts.
                TL(chapter.rectTransform,14f,12f,iw-28f,46f);
                TL(opponent.rectTransform,14f,52f,iw-28f,64f);
                TL(formation.rectTransform,14f,119f,iw-28f,34f);
                TL(reward.rectTransform,14f,156f,iw-28f,35f);
                TL(passive.rectTransform,14f,200f,iw-28f,58f);
                TL(battleFeedback.rectTransform,14f,Mathf.Max(182f,ih-184f),iw-28f,48f);
                TL((RectTransform)mainAction.transform,14f,Mathf.Max(236f,ih-114f),iw-28f,49f);
                TL((RectTransform)claimXp.transform,14f,Mathf.Max(288f,ih-62f),iw-28f,44f);
                chapter.fontSize=32;opponent.fontSize=26;formation.fontSize=26;
                reward.fontSize=22;passive.fontSize=21;battleFeedback.fontSize=19;
            }
            else if(vigilantsLandscape && compactLandscape)
            {
                TL(chapter.rectTransform,14f,10f,iw-28f,32f);
                TL(opponent.rectTransform,14f,46f,iw-28f,58f);
                TL(formation.rectTransform,14f,108f,iw-28f,28f);
                TL(reward.rectTransform,14f,144f,iw-28f,52f);
                TL(passive.rectTransform,14f,202f,iw-28f,50f);
                TL((RectTransform)mainAction.transform,iw*.51f,ih-80f,iw*.46f,68f);
                TL((RectTransform)claimXp.transform,14f,ih-80f,iw*.46f,68f);
                chapter.fontSize=24;opponent.fontSize=24;formation.fontSize=20;
                reward.fontSize=20;passive.fontSize=20;
            }
            else if(vigilantsLandscape)
            {
                // One compact, attractive mission card over the living route:
                // the encounter, rewards and actual combat button remain real.
                TL(chapter.rectTransform,20f,12f,iw-40f,37f);
                TL(opponent.rectTransform,20f,54f,iw-40f,64f);
                TL(formation.rectTransform,20f,122f,iw-40f,32f);
                TL(reward.rectTransform,20f,165f,iw-40f,38f);
                TL(passive.rectTransform,20f,208f,iw-40f,44f);
                TL(battleFeedback.rectTransform,20f,256f,iw-40f,24f);
                TL((RectTransform)mainAction.transform,iw*.51f,ih-61f,
                    iw*.46f,48f);
                TL((RectTransform)claimXp.transform,20f,ih-60f,
                    iw*.46f,46f);
                chapter.fontSize=27;opponent.fontSize=25;formation.fontSize=19;
                reward.fontSize=20;passive.fontSize=18;battleFeedback.fontSize=17;
            }
            else
            {
                // Scout-board header, visible above three portrait contracts.
                TL(chapter.rectTransform,16f,10f,iw-32f,41f);
                chapter.fontSize=27;
            }
            // The approved quest-board cards occupy the actual manuscript,
            // not three tiny 95-point rows floating at the top of a phone.
            // Scale all three naturally with their destination's usable height.
            float head=portrait?80f:57f; // Preserve the "ENCARGOS EN CURSO" royal heading.
            float spacing=Mathf.Clamp((ih-head-12f)/3f,65f,330f);
            for(int i=0;i<3;i++)
            {
                var cell=scoutCards[i].transform.parent as RectTransform;
                if(scoutsLandscape)
                {
                    // Three playable illustrated parchment contracts across
                    // the map, as in the approved image — not a list of timers.
                    float gap=10f;
                    float cardW=(iw-28f-gap*2f)/3f;
                    float cardH=Mathf.Max(110f,ih-70f);
                    TL(cell,14f+i*(cardW+gap),55f,cardW,cardH);
                    float inset=20f;
                    TL(scoutTitles[i].rectTransform,inset,13f,cardW-40f,33f);
                    TL((RectTransform)scoutThumbnails[i].transform,
                        inset,52f,cardW-40f,Mathf.Max(54f,cardH*.30f));
                    TL(scoutCards[i].rectTransform,inset,
                        63f+cardH*.30f,cardW-40f,compactLandscape?76f:58f);
                    TL((RectTransform)scoutActions[i].transform,
                        inset,cardH-(compactLandscape?83f:61f),cardW-40f,compactLandscape?68f:46f);
                    scoutTitles[i].fontSize=24;
                    scoutTitles[i].alignment=TextAnchor.MiddleCenter;
                    scoutCards[i].fontSize=19;
                    scoutCards[i].alignment=TextAnchor.MiddleCenter;
                    scoutActionLabels[i].fontSize=20;
                }
                else
                {
                    TL(cell,12f,head+i*spacing,iw-24f,spacing-5f);
                    float tab=Mathf.Clamp((iw-24f)*.14f,35f,65f);
                    float captionX=tab+22f;
                    float contentWidth=Mathf.Max(75f,iw-(portrait?186f:153f)-captionX);
                    float titleTop=Mathf.Max(13f,spacing*.18f);
                    TL(scoutTitles[i].rectTransform,captionX,titleTop,
                        contentWidth,Mathf.Min(portrait?72f:36f,spacing*(portrait?.34f:.24f)));
                    TL(scoutCards[i].rectTransform,captionX,titleTop+(portrait?76f:41f),
                        contentWidth,Mathf.Max(51f,spacing-titleTop-(portrait?82f:50f)));
                    TL((RectTransform)scoutActions[i].transform,
                        iw-(portrait?183f:150f),spacing*.5f-(portrait?44f:19f),
                        portrait?158f:120f,portrait?88f:40f);
                    scoutTitles[i].fontSize=portrait?30:21;
                    scoutTitles[i].alignment=TextAnchor.MiddleLeft;
                    scoutCards[i].fontSize=portrait?26:18;
                    scoutCards[i].alignment=TextAnchor.MiddleLeft;
                    scoutActionLabels[i].fontSize=portrait?26:18;
                }
                scoutThumbnails[i].gameObject.SetActive(scoutsLandscape &&
                    paintedScouts!=null);
            }
            TL(introduction.rectTransform,20f,100f,pw-40f,38f);
            footer.fontSize=portrait?26:compactLandscape?20:18;
            TL(footer.rectTransform,20f,ph-(portrait?50f:39f),pw-40f,portrait?40f:25f);
        }
        void Render(bool explicitRefresh)
        {
            if(!IsOpen||gateway==null||now==null)return;
            RestoreTextDensity();
            var s=gateway.Snapshot();
            var p=s.Retention??new EarlyRetentionState();
            bool vig=OpenDestination==Destination.Vigilants;
            sceneArtwork.Scouts=!vig;
            sceneArtwork.Chapter=Math.Max(1,(p.LastClearedStage/25)+1);
            if(paintedScene)
            {
                var source=vig?paintedVigilants:paintedScouts;
                paintedScene.enabled=source!=null;
                if(source && paintedScene.texture!=source)paintedScene.texture=source;
                if(source)
                {
                    // Cover the changing portrait/landscape panel without
                    // stretching masonry, terrain or the cartographer's map.
                    float panelRatio=Mathf.Max(.05f,heroScene.rect.width/
                        Mathf.Max(1f,heroScene.rect.height));
                    float pictureRatio=(float)source.width/source.height;
                    paintedScene.uvRect=panelRatio>pictureRatio
                        ?new Rect(0f,(1f-pictureRatio/panelRatio)*.5f,1f,
                            pictureRatio/panelRatio)
                        :new Rect((1f-panelRatio/pictureRatio)*.5f,0f,
                            panelRatio/pictureRatio,1f);
                }
            }
            chapterTrail.gameObject.SetActive(vig);
            if(vig)
            {
                int currentChapter=Math.Max(1,p.LastClearedStage/25+1);
                if(p.LastClearedStage>0 && p.LastClearedStage%25==0)
                    currentChapter=p.LastClearedStage/25+1;
                if(chapterTrail.Chapter!=currentChapter||chapterTrail.Cleared!=p.LastClearedStage)
                {
                    chapterTrail.Chapter=currentChapter;
                    chapterTrail.Cleared=p.LastClearedStage;
                    chapterTrail.SetVerticesDirty();
                }
            }
            int chapterProgress=p.LastClearedStage%25;
            if(sceneArtwork.ChapterProgress!=chapterProgress || explicitRefresh)
            {
                sceneArtwork.ChapterProgress=chapterProgress;
                sceneArtwork.SetVerticesDirty();
            }
            bool narrow=height>width && width<640f;
            heading.Put(vig?(narrow?"VIGILANTES":"CAMINO DE LOS VIGILANTES"):
                (narrow?"EXPLORADORES":"MESA DE EXPLORADORES"));
            kicker.Put(vig?(narrow?"CAMINO DE LOS VIGILANTES · PvE":
                "PRUEBAS DEL REINO · COMBATE AUTOMÁTICO"):
                (narrow?"MESA DE EXPLORADORES · ENCARGOS":
                "ENCARGOS REALES · REGRESOS PERSISTENTES"));
            introduction.gameObject.SetActive(false);
            // The board reserves a 57 px header; show its actual section title
            // instead of leaving a dead band above the three illustrated cards.
            chapter.gameObject.SetActive(true);
            opponent.gameObject.SetActive(vig);
            formation.gameObject.SetActive(vig);
            reward.gameObject.SetActive(vig);
            passive.gameObject.SetActive(vig);
            mainAction.gameObject.SetActive(vig);
            claimXp.gameObject.SetActive(vig);
            // Portrait players receive battle feedback through the existing
            // result banner; do not overlap compact XP/action controls.
            battleFeedback.gameObject.SetActive(vig && width>height && !compactLandscape);
            for(int i=0;i<3;i++)
            {
                scoutCards[i].transform.parent.gameObject.SetActive(!vig);
            }
            if(vig)
            {
                int stage=p.LastClearedStage+1;
                var info=EarlyRetentionRules.Stage(Math.Min(stage,10000));
                bool boss=info.Step%5==0;
                bool showEngendro=info.Step==15 && engendroTexture!=null;
                if(engendroPortrait)
                    engendroPortrait.transform.parent.gameObject.SetActive(showEngendro);
                if(showEngendro)
                {
                    // The real creature occupies the lower-left of the painted
                    // scene. Shift the 25 playable stage markers up so that
                    // none of them is hidden behind a decorative creature.
                    float sceneHeight=Mathf.Max(1f,heroScene.rect.height);
                    float portraitHeight=Mathf.Min(sceneHeight*.49f,
                        height>width?360f:310f);
                    float clearance=Mathf.Clamp(
                        (portraitHeight+32f)/sceneHeight,.27f,.64f);
                    bool wide=width>height;
                    chapterTrail.rectTransform.anchorMin=new Vector2(
                        wide?.07f:.10f,clearance);
                    chapterTrail.rectTransform.anchorMax=new Vector2(
                        wide?.34f:.90f,.95f);
                }
                else
                {
                    bool wide=width>height;
                    chapterTrail.rectTransform.anchorMin=wide
                        ?new Vector2(.04f,.13f):new Vector2(.10f,.06f);
                    chapterTrail.rectTransform.anchorMax=wide
                        ?new Vector2(.34f,.86f):new Vector2(.90f,.54f);
                }
                // Update labels only AFTER the Engendro-relative trail
                // anchors are chosen. Otherwise the captured stage-15 numbers
                // can retain coordinates from a different panel size.
                int liveStage=p.LastClearedStage%25+1;
                if(currentMilestoneLabel)
                {
                    currentMilestoneLabel.Put(liveStage.ToString());
                    currentMilestoneLabel.fontSize=height>width?28:compactLandscape?22:17;
                    currentMilestoneLabel.gameObject.SetActive(true);
                    var point=chapterTrail.StagePosition(liveStage);
                    Pos(currentMilestoneLabel.rectTransform,new Vector2(.5f,.5f),
                        new Vector2(.5f,.5f),point.x,point.y,40f,38f);
                }
                for(int i=0;i<bossMilestoneLabels.Length;i++)
                {
                    int bossStep=(i+1)*5;
                    var label=bossMilestoneLabels[i];
                    label.gameObject.SetActive(bossStep!=liveStage);
                    label.fontSize=height>width?26:compactLandscape?20:16;
                    var point=chapterTrail.StagePosition(bossStep);
                    Pos(label.rectTransform,new Vector2(.5f,.5f),
                        new Vector2(.5f,.5f),point.x,point.y,40f,38f);
                }
                if(sceneArtwork.BossEncounter!=boss)
                {
                    sceneArtwork.BossEncounter=boss;
                    sceneArtwork.SetVerticesDirty();
                }
                kicker.Put(width<550f
                    ?(boss?"GUARDIÁN · ":"PRUEBA · ")+info.Scene.ToUpperInvariant()
                    :info.Scene.ToUpperInvariant()+" · "+
                    (boss?"GUARDIÁN ESPECIAL":"COMBATE AUTOMÁTICO"));
                chapter.Put("CAPÍTULO "+info.Chapter+" · ETAPA "+info.Step+"/25");
                opponent.Put(info.Name+"\nRasgo: "+info.Trait);
                int archer=s.Available?.Total??0;
                formation.Put("Héroe: "+(s.PreparedHeroId=="lyra"&&s.ChapterThree.LyraRecruited?
                    "Lyra":"Aldric")+" · "+archer+" tropas disponibles");
                reward.Put("Al vencer: "+(30+stage*2)+" madera · "+(10+stage)+
                    " piedra · "+(20+stage/5*3)+" XP");
                int xp=EarlyRetentionRules.UnclaimedIdleXp(p,now());
                passive.Put("EXP. ACUMULADA: "+xp+" XP / máximo 8 h\n"+
                    "Maestría PvE: Aldric "+EarlyRetentionRules.HeroMasteryLevel(s,"aldric")+
                    " · Lyra "+EarlyRetentionRules.HeroMasteryLevel(s,"lyra"));
                mainAction.interactable=EarlyRetentionRules.StageUnlocked(s,stage)&&archer>0;
                // A single direct call to action, like the approved artwork.
                // The panel already explains that resolution is automatic.
                mainActionLabel.Put(mainAction.interactable?"COMBATIR":
                    "AÚN NO DISPONIBLE");
                claimXp.interactable=xp>0;
                xpLabel.Put(xp>0?"RECOGER "+xp+" XP":"EXPERIENCIA EN PROGRESO");
                battleFeedback.Put(string.IsNullOrEmpty(notice) && stage==1 ?
                    "ALDRIC · Es una prueba de preparación. Elige tus tropas y yo dirigiré el combate automático." :
                    notice);
                footer.Put("Cada victoria abre el siguiente tramo del camino.");
            }
            else
            {
                if(engendroPortrait)
                    engendroPortrait.transform.parent.gameObject.SetActive(false);
                chapter.Put("ENCARGOS EN CURSO");
                var list=p.Orders.Where(x=>x!=null&&!x.Claimed)
                    .OrderBy(x=>x.StartedUtcTicks>0?0:1)
                    .ThenBy(x=>x.Period).ThenBy(x=>x.Slot).Take(3).ToArray();
                for(int i=0;i<3;i++)
                {
                    var t=scoutCards[i];var a=scoutActions[i];var label=scoutActionLabels[i];
                    if(i>=list.Length)
                    {
                        if(scoutThumbnails[i])scoutThumbnails[i].enabled=false;
                        scoutTitles[i].Put("SIN ENCARGOS NUEVOS");
                        t.Put("Vuelve a consultar la mesa más tarde.");
                        a.interactable=false;label.Put("ESPERAR");scoutIds[i]="";
                        continue;
                    }
                    var order=list[i];
                    scoutIds[i]=order.Id;
                    // A real job type keeps the same miniature as board slots
                    // refill or reorder. The picture is decorative, not gameplay.
                    if(scoutThumbnails[i])
                    {
                        scoutThumbnails[i].enabled=paintedScouts!=null;
                        // Each real mission type gets a recognisable crop of
                        // the painted cartographer's map. A crop MUST preserve
                        // aspect ratio, rather than stretching a narrow slice
                        // of the source across a wide expedition card.
                        var thumbRect=scoutThumbnails[i].rectTransform.rect;
                        float aspect=Mathf.Max(.1f,thumbRect.width/
                            Mathf.Max(1f,thumbRect.height));
                        float artAspect=paintedScouts
                            ?(float)paintedScouts.width/paintedScouts.height:1f;
                        float cropW=.54f;
                        float cropH=Mathf.Min(.92f,cropW*artAspect/aspect);
                        float centerX=.24f+Mathf.Clamp(order.Kind,0,3)*.17f;
                        float centerY=.28f+Mathf.Clamp(order.Kind,0,3)*.15f;
                        scoutThumbnails[i].uvRect=new Rect(
                            Mathf.Clamp(centerX-cropW*.5f,0f,1f-cropW),
                            Mathf.Clamp(centerY-cropH*.5f,0f,1f-cropH),
                            cropW,cropH);
                        bool readyToClaim=order.StartedUtcTicks>0 &&
                            order.EndsUtcTicks<=now();
                        scoutThumbnails[i].color=readyToClaim
                            ?new Color(1f,.96f,.83f,1f)
                            :order.StartedUtcTicks>0
                                ?new Color(.82f,.88f,1f,1f):Color.white;
                    }
                    if(scoutArt[i])
                    {
                        bool completed=order.StartedUtcTicks>0&&order.EndsUtcTicks<=now();
                        bool underway=order.StartedUtcTicks>0&&!completed;
                        if(scoutArt[i].Kind!=order.Kind||
                           scoutArt[i].Completed!=completed||
                           scoutArt[i].InProgress!=underway)
                        {
                            scoutArt[i].Kind=order.Kind;
                            scoutArt[i].Completed=completed;
                            scoutArt[i].InProgress=underway;
                            scoutArt[i].SetVerticesDirty();
                        }
                    }
                    string state=order.StartedUtcTicks==0?"Disponible":
                        order.EndsUtcTicks<=now()?"Listo para reclamar":
                        "Regreso en "+TimeSpan.FromTicks(order.EndsUtcTicks-now()).ToString(@"hh\:mm\:ss");
                    scoutTitles[i].Put(EarlyRetentionRules.ScoutTitle(order.Kind));
                    t.Put(EarlyRetentionRules.ScoutRewardText(order.Kind)+"\n"+state);
                    a.interactable=order.StartedUtcTicks==0||order.EndsUtcTicks<=now();
                    label.Put(order.StartedUtcTicks==0?"ENVIAR":
                        order.EndsUtcTicks<=now()?"RECOGER":"EN CAMINO");
                }
                long due=EarlyRetentionRules.NextPeriodUtcTicks(now());
                footer.Put("Nuevos encargos: "+
                    TimeSpan.FromTicks(Math.Max(0,due-now())).ToString(@"hh\:mm\:ss")+
                    " · Máximo 6 pendientes");
            }
            ApplyTextDensity();
        }
    }
}

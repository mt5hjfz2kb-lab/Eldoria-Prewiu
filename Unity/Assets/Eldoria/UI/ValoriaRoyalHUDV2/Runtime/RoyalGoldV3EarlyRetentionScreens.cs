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
        readonly RoyalText[] scoutActionLabels=new RoyalText[3];
        readonly RoyalGoldV3ScoutContractGraphic[] scoutArt=new RoyalGoldV3ScoutContractGraphic[3];
        readonly string[] scoutIds=new string[3];
        RoyalText heading, kicker, introduction, chapter, opponent, formation,
            reward, battleFeedback, passive, footer, mainActionLabel, xpLabel, backLabel;
        RoyalGoldV3EarlyRetentionArtwork sceneArtwork;
        RoyalGoldV3StageTrailGraphic chapterTrail;
        // Two original zero-cost painterly environments, with the hand-built
        // vector renderer as the graceful fallback in older installations.
        RawImage paintedScene;
        Texture2D paintedVigilants, paintedScouts;
        GameObject screen;
        float width=-1,height=-1,nextUpdate;
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
            Action action,out RoyalText caption,bool primary=false)
        {
            var r=Panel(parent,name,true);
            var g=r.GetComponent<RoyalGoldV2FrameGraphic>();
            g.Primary=primary;g.DecorativeCorners=false;
            var b=r.gameObject.AddComponent<RoyalButton>();
            b.targetGraphic=g;
            caption=Text(r,"Original gilded label",17,Ivory,true);
            caption.alignment=TextAnchor.MiddleCenter;
            Stretch(caption.rectTransform,5);
            caption.Put(label);
            b.onClick.AddListener(()=>action?.Invoke());
            return b;
        }
        static void StyleRetentionAction(RoyalButton button,bool battle)
        {
            if(!button)return;
            var previous=button.GetComponent<RoyalGoldV2FrameGraphic>();
            if(previous){previous.raycastTarget=false;previous.enabled=false;}
            var frame=button.gameObject.AddComponent<RoyalGoldV3RetentionActionGraphic>();
            frame.Battle=battle;
            frame.raycastTarget=true;
            button.targetGraphic=frame;
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
            paintedVigilants=Resources.Load<Texture2D>("EldoriaRetention/vigilants-painterly");
            paintedScouts=Resources.Load<Texture2D>("EldoriaRetention/scouts-painterly");
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
            // The stage-15 elite is the canonical Bastion-II Spawnling,
            // not an invented stone golem. Reuse its actual authored image.
            engendroTexture=Resources.Load<Texture2D>(
                "EldoriaRetention/engendro-portrait");
            var creature=Panel(heroScene,"Lore-approved Engendro portrait");
            var creatureArt=Rect("Engendro / authentic creature illustration",creature);
            creatureArt.gameObject.AddComponent<CanvasRenderer>();
            engendroPortrait=creatureArt.gameObject.AddComponent<RawImage>();
            engendroPortrait.raycastTarget=false;
            engendroPortrait.texture=engendroTexture;
            engendroPortrait.uvRect=new Rect(0f,0f,1f,1f);
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
                "COMBATIR",ExecuteStage,out mainActionLabel,true);
            StyleRetentionAction(mainAction,true);
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
                    "ENVIAR",()=>RunScout(index),out scoutActionLabels[i],true);
                StyleRetentionAction(scoutActions[i],false);
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
                if(Mathf.Abs(safe.rect.width-width)>1||Mathf.Abs(safe.rect.height-height)>1)
                    Layout();
                Render(false);
            }
            else if(Mathf.Abs(safe.rect.width-width)>1||Mathf.Abs(safe.rect.height-height)>1)
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
            width=safe.rect.width;height=safe.rect.height;
            LayoutShortcuts();
            if(width<200||height<260)return;
            bool portrait=height>width;
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
                TL(heading.rectTransform,18f,12f,pw-135f,62f);
                heading.fontSize=22;
                TL(kicker.rectTransform,18f,78f,pw-36f,24f);
                TL((RectTransform)back.transform,pw-113f,22f,99f,44f);
                backLabel.Put("VOLVER");
                backLabel.fontSize=15;
            }
            else
            {
                TL(heading.rectTransform,20f,18f,pw-170f,44f);
                heading.fontSize=34;
                TL(kicker.rectTransform,20f,61f,pw-35f,27f);
                TL((RectTransform)back.transform,pw-150f,23f,136f,46f);
                backLabel.Put("VOLVER");
                backLabel.fontSize=17;
            }
            float imageTop=portrait?110f:108f;
            bool scoutsLandscape=!portrait && OpenDestination==Destination.Scouts;
            bool vigilantsLandscape=!portrait && OpenDestination==Destination.Vigilants;
            // The approved reference is scenic-first. In wide views the landscape
            // or cartographer's desk spans the available width; actual missions
            // and the combat panel float over the LOWER portion of that artwork.
            // Portrait keeps the existing proven stacked, safe-area composition.
            float scenicHeight=portrait?Mathf.Clamp(ph-640f,220f,880f):ph-155f;
            TL(heroScene,portrait?18f:20f,imageTop,
                portrait?pw-36f:pw-40f,scenicHeight);
            if(vigilantsLandscape)
            {
                chapterTrail.rectTransform.anchorMin=new Vector2(.04f,.13f);
                chapterTrail.rectTransform.anchorMax=new Vector2(.42f,.86f);
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
                float w=Mathf.Min(heroScene.rect.width*.30f,portrait?210f:235f);
                float h=Mathf.Min(heroScene.rect.height*.37f,portrait?170f:215f);
                TL(frame,portrait?16f:26f,
                    Mathf.Max(90f,heroScene.rect.height-h-19f),w,h);
            }
            if(victoryBanner)
            {
                float bannerWidth=Mathf.Min(heroScene.rect.width-18f,330f);
                float bannerHeight=84f;
                Pos(victoryBanner,new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                    0f,0f,bannerWidth,bannerHeight);
                TL(victoryTitle.rectTransform,12f,7f,bannerWidth-24f,35f);
                TL(victoryDetail.rectTransform,12f,44f,bannerWidth-24f,29f);
                victoryDetail.fontSize=portrait?14:16;
            }
            // Keep the portrait manuscript above the fixed bottom footer.
            // The final scout card and claim-XP button previously extended
            // under this footer even though both were inside the safe area.
            if(portrait)
                TL(infoCard,bodyX,imageTop+scenicHeight+10f,
                    bodyWidth,ph-(imageTop+scenicHeight+58f));
            else if(scoutsLandscape)
                TL(infoCard,18f,ph*.49f,pw-36f,ph*.51f-61f);
            else
                TL(infoCard,pw*.37f,ph*.55f,pw*.61f,ph*.45f-55f);
            float iw=infoCard.rect.width,ih=infoCard.rect.height;
            if(iw<=0||ih<=0)return;
            if(portrait)
            {
                // Compact, intentional vertical rhythm: the playable trail/map
                // remains the hero, while both actions stay fully above the footer.
                TL(chapter.rectTransform,14f,13f,iw-28f,36f);
                TL(opponent.rectTransform,14f,52f,iw-28f,47f);
                TL(formation.rectTransform,14f,101f,iw-28f,34f);
                TL(reward.rectTransform,14f,138f,iw-28f,35f);
                TL(passive.rectTransform,14f,182f,iw-28f,50f);
                TL(battleFeedback.rectTransform,14f,Mathf.Max(182f,ih-184f),iw-28f,48f);
                TL((RectTransform)mainAction.transform,14f,Mathf.Max(236f,ih-114f),iw-28f,49f);
                TL((RectTransform)claimXp.transform,14f,Mathf.Max(288f,ih-62f),iw-28f,44f);
                chapter.fontSize=26;opponent.fontSize=22;formation.fontSize=19;
                reward.fontSize=20;passive.fontSize=20;battleFeedback.fontSize=18;
            }
            else if(vigilantsLandscape)
            {
                // One compact, attractive mission card over the living route:
                // the encounter, rewards and actual combat button remain real.
                TL(chapter.rectTransform,20f,12f,iw-40f,37f);
                TL(opponent.rectTransform,20f,54f,iw-40f,55f);
                TL(formation.rectTransform,20f,110f,iw-40f,32f);
                TL(reward.rectTransform,20f,153f,iw-40f,38f);
                TL(passive.rectTransform,20f,196f,iw-40f,41f);
                TL(battleFeedback.rectTransform,20f,240f,iw-40f,30f);
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
            float head=57f; // Preserve the "ENCARGOS EN CURSO" royal heading.
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
                        63f+cardH*.30f,cardW-40f,58f);
                    TL((RectTransform)scoutActions[i].transform,
                        inset,cardH-61f,cardW-40f,46f);
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
                    float contentWidth=Mathf.Max(75f,iw-(portrait?115f:153f)-captionX);
                    float titleTop=Mathf.Max(13f,spacing*.18f);
                    TL(scoutTitles[i].rectTransform,captionX,titleTop,
                        contentWidth,Mathf.Min(36f,spacing*.24f));
                    TL(scoutCards[i].rectTransform,captionX,titleTop+41f,
                        contentWidth,Mathf.Max(51f,spacing-titleTop-50f));
                    TL((RectTransform)scoutActions[i].transform,
                        iw-(portrait?112f:150f),spacing*.5f-19f,
                        portrait?87f:120f,40f);
                    scoutTitles[i].fontSize=portrait?22:21;
                    scoutTitles[i].alignment=TextAnchor.MiddleLeft;
                    scoutCards[i].fontSize=portrait?19:18;
                    scoutCards[i].alignment=TextAnchor.MiddleLeft;
                    scoutActionLabels[i].fontSize=portrait?16:18;
                }
                scoutThumbnails[i].gameObject.SetActive(scoutsLandscape &&
                    paintedScouts!=null);
            }
            TL(introduction.rectTransform,20f,100f,pw-40f,38f);
            TL(footer.rectTransform,20f,ph-39f,pw-40f,25f);
        }
        void Render(bool explicitRefresh)
        {
            if(!IsOpen||gateway==null||now==null)return;
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
            bool narrow=safe.rect.height>safe.rect.width && safe.rect.width<640f;
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
            battleFeedback.gameObject.SetActive(vig);
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
                    float portraitHeight=Mathf.Min(sceneHeight*.37f,
                        height>width?170f:215f);
                    float clearance=Mathf.Clamp(
                        (portraitHeight+32f)/sceneHeight,.27f,.64f);
                    bool wide=width>height;
                    chapterTrail.rectTransform.anchorMin=new Vector2(
                        wide?.07f:.10f,clearance);
                    chapterTrail.rectTransform.anchorMax=new Vector2(
                        wide?.46f:.90f,.95f);
                }
                else
                {
                    bool wide=width>height;
                    chapterTrail.rectTransform.anchorMin=wide
                        ?new Vector2(.04f,.13f):new Vector2(.10f,.06f);
                    chapterTrail.rectTransform.anchorMax=wide
                        ?new Vector2(.42f,.86f):new Vector2(.90f,.54f);
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
                footer.Put("Nuevos encargos en "+
                    TimeSpan.FromTicks(Math.Max(0,due-now())).ToString(@"hh\:mm\:ss")+
                    " · Hasta seis pendientes acumulables");
            }
        }
    }
}

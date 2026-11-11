using System;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.DailyTreasures
{
    // Reversible opt-in uGUI presentation. Actual rewards and mail are saved
    // by LocalGateway BEFORE any ceremonial animation begins.
    [DisallowMultipleComponent]
    public sealed class DailyTreasurePanel : MonoBehaviour
    {
        // Source art direction: the actual PR173 Vigilia is charcoal stone,
        // restrained forged copper, parchment ivory and burgundy action.
        // NOT floral/purple-neon/fairy crystals or fake collectible currencies.
        static readonly Color Gold=new Color(.92f,.70f,.40f,1f);
        static readonly Color Ivory=new Color(.98f,.94f,.84f,1f);
        static readonly Color Muted=new Color(.83f,.81f,.76f,1f);
        static readonly Color Stone=new Color(.085f,.090f,.095f,1f);
        // Native real-city A/B run 42888085086: previous .91f dimmer
        // retained just ~15-19% of scenery outside the seven-day calendar.
        // Restore actual Valoria scenery, without changing the solid slate cards.
        const float CalendarBackdropOpacity=.70f;
        static readonly Color Oak=new Color(.28f,.12f,.10f,1f);
        ICommandGateway gateway;
        Func<long> now;
        Action refreshed;
        RectTransform root,entryRect,screen,cardSurface,calendar,ceremony,glowRect,raysRect,ambientRect;
        RectTransform originalSafeArea;
        RectTransform cachedCanonicalHeroRoom;
        // HeroesApprovedDestination.Initialize places its actual root directly
        // under the canonical HUD safe area. Probe only its immediate children
        // when the destination is not yet initialized: scanning every nested
        // card on each Android frame needlessly traverses the entire HUD.
        // This still detects the room in the SAME frame it appears.
        RectTransform cachedVigiliaBuildingPanel;
        float nextCanvasProbe;
        bool onVigiliaCanvas;
        public bool UsesVigiliaCanvas => onVigiliaCanvas;
        Button entryButton,claimButton,closeButton,skipButton;
        Image entryArtwork,entryGlow,entryNotice;
        Text entryNoticeNumber;
        bool entryIsAvailable;
        Text header,subhead,claimCaption,resultText,entryCaption;
        readonly Text[] dayHead=new Text[7],dayBody=new Text[7];
        readonly Image[] dayBack=new Image[7],sparks=new Image[18];
        Image rareDayGlow;
        bool rareDayAvailable;
        readonly RectTransform[] prizeRects=new RectTransform[4];
        readonly Vector2[] prizeHomes=new Vector2[4];
        Vector2 prizeFlightOrigin;
        readonly Image[] prizeIcons=new Image[4];
        readonly Text[] prizeNames=new Text[4],prizeAmounts=new Text[4];
        readonly CanvasGroup[] prizeGroups=new CanvasGroup[4];
        RectTransform prizeShelf;
        int prizeCount;
        float prizeRevealStarted;
        bool prizeAnimating;
        DailyTreasureRaysGraphic rays;
        readonly DailyTreasureRewardGlyph[] dayGlyph=new DailyTreasureRewardGlyph[7];
        readonly Image[] dayArtwork=new Image[7];
        static readonly string[] DayIllustrations={
            "resource_wood","resource_stone","resource_hourglass",
            "treasure_closed_512","resource_hourglass","resource_food",
            "treasure_closed_512"
        };
        DailyTreasureChestGraphic chest;
        Image illustratedClosed,illustratedOpen;
        bool hasAuthoredChestArt;
        public bool HasAuthoredChestArt => hasAuthoredChestArt;
        public int EarnedItemCardCount => prizeCount;
        public bool HasRevealedIllustratedEarnedItems
        {
            get
            {
                if(prizeCount<=0 || !prizeShelf ||
                   !prizeShelf.gameObject.activeInHierarchy)return false;
                for(int i=0;i<prizeCount;i++)
                    if(!prizeRects[i] || !prizeRects[i].gameObject.activeInHierarchy ||
                       !prizeIcons[i] || !prizeIcons[i].sprite ||
                       !prizeGroups[i] || prizeGroups[i].alpha<.98f)
                        return false;
                return true;
            }
        }
        Image glow,ambient;
        Image screenShade,mainFrameSurface;
        RectTransform milestoneBanner;
        Text milestoneBannerCaption;
        Image milestoneRule;
        DailyTreasureStageKind currentStage=DailyTreasureStageKind.Daily;
        string activeMilestoneId;
        public DailyTreasureStageKind ActiveMilestoneStage => currentStage;
        // The chest belongs to paid daily tributes, never to a hero unlock,
        // scout discovery, Vigilants report or Bastion development milestone.
        public bool DisplaysCeremonialChest =>
            ceremony && ceremony.gameObject.activeSelf &&
            (chest.gameObject.activeSelf || illustratedClosed.gameObject.activeSelf ||
             illustratedOpen.gameObject.activeSelf);
        RawImage milestoneScenery;
        RawImage milestoneCinematicBackdrop;
        Material ownedCinematicBackdropMaterial;
        Material ownedVigilantsReliefMaterial;
        RenderTexture capturedBastionScene;
        bool bastionCameraPending;
        int bastionCameraRetries;
        float nextBastionCameraTry;
        Image milestoneVignette;
        Image milestoneInsignia;
        // Separately painted alpha layers, behind official scene/hero identity.
        Image milestoneStageArt,milestoneOrnament;
        // The original EldoriaRetention atlas already represents four REAL
        // scout mission families. An earned discovery may paint the matching
        // actual persisted Kind on the existing blank parchment. Never use a
        // made-up map, guessed kind, fake order or second reward.
        RawImage milestoneDiscoveryIllustration;
        Material ownedScoutInkMaterial;
        int verifiedScoutKind=-1;
        public int VerifiedScoutDiscoveryKind => verifiedScoutKind;
        public bool HasCommittedScoutIllustration =>
            currentStage==DailyTreasureStageKind.Explorers &&
            verifiedScoutKind>=0 &&
            milestoneDiscoveryIllustration &&
            milestoneDiscoveryIllustration.texture &&
            milestoneDiscoveryIllustration.gameObject.activeInHierarchy;
        Vector2 milestoneStageRest;
        float milestoneStageRestOpacity;
        Sprite dailyFrameSprite;
        string milestoneArtworkResourceKey;
        public string MilestoneArtworkResourceKey => milestoneArtworkResourceKey;
        public bool HasDistinctMilestoneArt =>
            currentStage!=DailyTreasureStageKind.Daily &&
            milestoneStageArt && milestoneStageArt.sprite &&
            milestoneStageArt.gameObject.activeInHierarchy;
        // The hero is sourced only from the game's real hero inventory. Never
        // bake the imagined Lyra from a concept poster into a live milestone.
        Image milestoneLyraPortrait;
        RectTransform milestoneLyraViewport;
        bool lyraFallbackActive;
        float nextLyraArtProbe,stopLyraArtProbe,lyraArtRevealAt;
        Image bastionStoryInk;
        string milestoneSceneryKey;
        Rect milestoneSceneryBaseUv;
        public string MilestoneSceneryKey => milestoneSceneryKey;
        public bool DisplaysCanonicalLyraPortrait =>
            currentStage==DailyTreasureStageKind.Lyra &&
            milestoneLyraPortrait && milestoneLyraPortrait.sprite &&
            milestoneLyraPortrait.gameObject.activeInHierarchy;
        public bool HasMilestoneScenery =>
            milestoneScenery && milestoneScenery.texture &&
            milestoneScenery.gameObject.activeInHierarchy;
        DailyTreasureCeremonyTheme activeTheme=
            DailyTreasureCeremonyTheme.ForEvent(null,false);
        Texture2D glowTexture;
        Sprite glowSprite;
        bool suppressed,playing;
        float startTime,nextTick,lastWidth=-1,lastHeight=-1,noticeUntil;
        long lastPaintedUtcDay=-1;
        string claimNotice="";
        string ceremonialReceipt,ceremonialTitle;
        bool chestReward,chimePlayed;
        DailyTreasureCeremonyTier ceremonyTier=DailyTreasureCeremonyTier.Special;
        AudioSource originalSfx;
        AudioClip chestKnock,chestChime,chestGrandChime;
        readonly DailyTreasureMilestoneQueue milestoneQueue=
            new DailyTreasureMilestoneQueue();
        float nextMilestonePoll;
        bool sessionAutoCeremonyPlayed;

        public bool Suppressed
        {
            get => suppressed;
            set
            {
                if(suppressed==value)return;
                suppressed=value;
                if(suppressed && IsOpen)
                {
                    // A tutorial or a more important story room may appear
                    // while the player is looking at a daily prize. Close
                    // immediately, including audio: payments were already
                    // committed to LocalGateway, not tied to this Canvas.
                    // The informational receipt remains in the mailbox.
                    Close();
                }
                if(entryRect)
                    entryRect.gameObject.SetActive(!suppressed &&
                        !HasVisibleCanonicalHeroRoom() &&
                        !HasVisibleVigiliaBuildingPanel());
            }
        }
        public bool IsOpen => screen && screen.gameObject.activeSelf;
        public static DailyTreasurePanel Install(RectTransform safe)
        {
            var host=NewRect("Tesoros de Valoria / independent UI",safe);
            Stretch(host);
            var panel=host.gameObject.AddComponent<DailyTreasurePanel>();
            panel.Build();
            return panel;
        }
        public void Bind(ICommandGateway commands,Func<long> utcNow,Action onRefresh)
        {
            if(!ReferenceEquals(gateway,commands))
            {
                // Session baseline prevents playing a ceremony for offline
                // progress already recorded in the mailbox on startup.
                milestoneQueue.Reset(commands?.Snapshot());
                nextMilestonePoll=0f;
                sessionAutoCeremonyPlayed=false;
            }
            gateway=commands;now=utcNow;refreshed=onRefresh;nextTick=0;
        }
        static RectTransform NewRect(string name,Transform parent)
        {
            var obj=new GameObject(name,typeof(RectTransform));
            obj.transform.SetParent(parent,false);
            obj.layer=parent.gameObject.layer;
            return (RectTransform)obj.transform;
        }
        static void Stretch(RectTransform rect)
        {
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
        }
        static void Place(RectTransform r,float ax,float ay,float px,float py,
            float x,float y,float width,float height)
        {
            r.anchorMin=r.anchorMax=new Vector2(ax,ay);
            r.pivot=new Vector2(px,py);
            r.anchoredPosition=new Vector2(x,y);
            r.sizeDelta=new Vector2(width,height);
        }
        static Image Plate(RectTransform rect,Color tint,bool raycast=true)
        {
            var image=rect.gameObject.AddComponent<Image>();
            image.color=tint;
            image.raycastTarget=raycast;
            return image;
        }
        static DailyTreasureReliefGraphic Relief(RectTransform parent,bool major=false,
            bool primary=false)
        {
            var r=NewRect("Reversible forged bronze engraving / visual only",parent);
            Stretch(r);
            r.SetAsFirstSibling();
            var graphic=r.gameObject.AddComponent<DailyTreasureReliefGraphic>();
            graphic.Major=major;graphic.Primary=primary;
            graphic.raycastTarget=false;graphic.color=Color.white;
            return graphic;
        }
        // Reuse the EXACT standalone materials of Vigilia when PR173's
        // assets are installed: one basalt plate, one thin bronze-edged
        // resource card and one burgundy action. PR181 remains buildable and
        // visually complete with its own original nine-slice fallbacks.
        // Do not copy PR173 files, paste a whole HUD screenshot or layer
        // multiple ornate frames over one another.
        static string MatchingVigiliaSurface(string fallback)
        {
            switch(fallback)
            {
                case "valoria_frame_main_512":
                    // Vigilia profile_plate is a wide 4:1 identity cartouche.
                    // Stretching it into a tall 440x700 calendar distorts
                    // metal ornament; keep our real square nine-slice modal.
                    return null;
                case "valoria_frame_card_256":
                    return "VigiliaArt/PremiumSurfaces/slate_resource";
                case "valoria_frame_button_512x128":
                    return "VigiliaArt/PremiumSurfaces/burgundy_action";
                default:return null;
            }
        }
        static Sprite LoadUnifiedResourceEmblem(string fallback)
        {
            // Cropped historical 128px icons include HUD backdrops and
            // remnants of numeric labels. Prefer genuine standalone 256px
            // originals with clean transparent borders in both calendar and
            // committed-reward cards. Keep all existing images as fallbacks.
            string clean=(fallback=="resource_wood"?"resource_wood_clean_256":
                fallback=="resource_stone"?"resource_stone_clean_256":
                fallback=="resource_food"?"resource_food_clean_256":
                fallback=="resource_hourglass"?"resource_hourglass_clean_256":null);
            var painted=clean==null?null:
                Resources.Load<Sprite>("DailyTreasureArt/"+clean);
            if(painted)return painted;
            string owner=(fallback=="resource_wood"?"wood":
                fallback=="resource_stone"?"stone":
                fallback=="resource_food"?"food":null);
            var icon=owner==null?null:
                Resources.Load<Sprite>("VigiliaArt/ResourceIcons/"+owner);
            return icon?icon:Resources.Load<Sprite>(
                "DailyTreasureArt/"+fallback);
        }
        static bool ApplyAuthoredFrame(RectTransform rect,string resource)
        {
            var shared=MatchingVigiliaSurface(resource);
            var sprite=shared==null?null:Resources.Load<Sprite>(shared);
            if(!sprite)
                sprite=Resources.Load<Sprite>("DailyTreasureArt/"+resource);
            if(!sprite)return false;
            var target=rect.GetComponent<Image>();
            if(!target)return false;
            target.sprite=sprite;
            target.type=Image.Type.Sliced;
            target.fillCenter=true;
            target.color=Color.white;
            // Preserve the underlying action Button or modal's raycast target.
            return true;
        }
        static Text Label(Transform parent,string name,string value,
            int fontSize,Color tint,TextAnchor align)
        {
            var r=NewRect(name,parent);
            var t=r.gameObject.AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            // PR173 owns these licensed OFL typefaces. Reuse them only when
            // Vigilia is present; do not copy font binaries into PR181 or
            // introduce a second font package with conflicting licenses.
            bool display=name=="Reward heading" || name=="Day heading" ||
                name=="Claim label" || name=="Confirmed received items" ||
                name=="Real milestone category";
            var referenceFont=Resources.Load<Font>(display
                ? "VigiliaArt/Fonts/CormorantGaramond-SemiBold"
                : "VigiliaArt/Fonts/LibreBaskerville-Regular");
            if(referenceFont)t.font=referenceFont;
            else if(display)
            {
                var fallback=Resources.Load<Font>("EldoriaRetention/reference-serif");
                if(fallback)t.font=fallback;
            }
            t.fontSize=fontSize;t.color=tint;t.alignment=align;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Truncate;
            t.raycastTarget=false;t.text=value;t.supportRichText=false;
            return t;
        }
        static Button ActionButton(RectTransform rect,Color tint,Action action)
        {
            var image=Plate(rect,tint);
            var button=rect.gameObject.AddComponent<Button>();
            button.targetGraphic=image;
            var colors=button.colors;
            colors.normalColor=Color.white;
            colors.highlightedColor=new Color(1.15f,1.08f,.92f,1f);
            colors.pressedColor=new Color(.68f,.60f,.54f,1f);
            colors.disabledColor=new Color(.47f,.43f,.40f,.55f);
            button.colors=colors;
            button.onClick.AddListener(()=>action());
            return button;
        }
        void Build()
        {
            originalSfx=gameObject.AddComponent<AudioSource>();
            originalSfx.playOnAwake=false;originalSfx.spatialBlend=0f;
            originalSfx.volume=.34f;
            chestKnock=CreateFoleyClip("Valoria / iron and oak latch",
                DailyTreasureFoleyCue.Latch);
            chestChime=CreateFoleyClip("Valoria / bronze discovery",
                DailyTreasureFoleyCue.Discovery);
            chestGrandChime=CreateFoleyClip("Valoria / grand bronze ceremony",
                DailyTreasureFoleyCue.GrandDiscovery);
            root=(RectTransform)transform;
            originalSafeArea=root.parent as RectTransform;
            // No second EventSystem, Canvas, save or navigation router.
            entryRect=NewRect("Daily reward / optional medallion",root);
            entryButton=ActionButton(entryRect,Oak,OpenEntryOrDeferredMilestone);
            if(!ApplyAuthoredFrame(entryRect,"valoria_frame_button_512x128"))
                Relief(entryRect,false,true);
            // The owner-approved presentation identifies treasures by the
            // genuine illustrated coffer, never a rectangular text-only CTA.
            var halo=NewRect("Amber claimable glow / non-interactive",entryRect);
            entryGlow=Plate(halo,Color.clear,false);
            var miniature=NewRect("Actual Valoria chest medallion",entryRect);
            entryArtwork=Plate(miniature,Color.white,false);
            entryArtwork.sprite=Resources.Load<Sprite>(
                "DailyTreasureArt/treasure_closed_512");
            entryArtwork.preserveAspect=true;
            var badge=NewRect("A genuine daily tribute awaits",entryRect);
            entryNotice=Plate(badge,new Color(.49f,.18f,.12f,1f),false);
            entryNoticeNumber=Label(badge,"Real daily count","1",14,Ivory,
                TextAnchor.MiddleCenter);
            Stretch(entryNoticeNumber.rectTransform);
            entryCaption=Label(entryRect,"Daily available label",
                "TESORO",12,Gold,TextAnchor.MiddleCenter);
            var entryShadow=entryRect.gameObject.AddComponent<Shadow>();
            entryShadow.effectColor=new Color(.02f,.012f,.012f,.8f);
            entryShadow.effectDistance=new Vector2(2f,-3f);

            screen=NewRect("Tributo de los siete días / modal",root);
            Stretch(screen);
            screenShade=Plate(screen,new Color(.018f,.020f,.026f,CalendarBackdropOpacity));
            cardSurface=NewRect("Basalt and worn bronze reward parchment",screen);
            mainFrameSurface=Plate(cardSurface,Stone);
            if(!ApplyAuthoredFrame(cardSurface,"valoria_frame_main_512"))
                Relief(cardSurface,true);
            dailyFrameSprite=mainFrameSurface.sprite;
            // One forged frame for a story event (landscape only). Tall
            // layouts keep the genuine original nine-slice instead of
            // distorting wide metalwork or stacking multiple ornate borders.
            var narrativeFrame=NewRect(
                "Authored narrative single-frame ornament / no hitbox",cardSurface);
            milestoneOrnament=Plate(narrativeFrame,Color.white,false);
            milestoneOrnament.sprite=Resources.Load<Sprite>(
                "DailyTreasureCeremonies/shared_ornament");
            // The original 1376x768 bronze relief is framed into 740x370
            // and 643x355 landscapes. Simple mode stretches the rivets
            // unevenly (11.6% across axes). The genuine source has clear
            // 90px corner bands, imported at 200 PPU: UI 9-slice retains
            // 45px square corner geometry in Vigilia's 100px reference.
            // Only the transparent middle may resize. No copied ornament,
            // no overlaid second border and no new fictional art.
            if(milestoneOrnament.sprite &&
               milestoneOrnament.sprite.border.sqrMagnitude>0f)
                milestoneOrnament.type=Image.Type.Sliced;
            milestoneOrnament.raycastTarget=false;
            Stretch(milestoneOrnament.rectTransform);
            narrativeFrame.SetAsFirstSibling();
            milestoneOrnament.gameObject.SetActive(false);
            // ART DIRECTION: instead of a generic black right half, carry the
            // SAME genuine landscape through the whole ceremonial frame. A
            // subdued cinematic material protects the existing live text.
            // This only paints original assets, never alters any player scene.
            var atmosphere=NewRect(
                "Genuine story world / cinematic full-bleed atmosphere",
                cardSurface);
            milestoneCinematicBackdrop=atmosphere.gameObject
                .AddComponent<RawImage>();
            milestoneCinematicBackdrop.raycastTarget=false;
            var atmosphereShader=Resources.Load<Shader>(
                "DailyTreasureCeremonies/DailyNarrativeBackdrop");
            if(atmosphereShader && atmosphereShader.isSupported)
            {
                ownedCinematicBackdropMaterial=new Material(atmosphereShader)
                {name="Tesoros / one owned cinematic atmosphere"};
                milestoneCinematicBackdrop.material=ownedCinematicBackdropMaterial;
            }
            Stretch(atmosphere);
            atmosphere.offsetMin=new Vector2(12f,12f);
            atmosphere.offsetMax=new Vector2(-12f,-12f);
            // The SINGLE original bronze frame must paint ON TOP of the
            // atmosphere, no new decorative border or double rim.
            atmosphere.SetAsFirstSibling();
            milestoneCinematicBackdrop.gameObject.SetActive(false);
            var edge=cardSurface.gameObject.AddComponent<Shadow>();
            edge.effectColor=new Color(0f,0f,0f,.82f);
            edge.effectDistance=new Vector2(4f,-6f);
            header=Label(cardSurface,"Reward heading","TESOROS DE VALORIA",27,Gold,TextAnchor.MiddleCenter);
            subhead=Label(cardSurface,"Consecutive visits","",14,Muted,TextAnchor.MiddleCenter);
            closeButton=ActionButton(NewRect("Close tributes",cardSurface),Oak,Close);
            if(!ApplyAuthoredFrame((RectTransform)closeButton.transform,
                "valoria_frame_card_256"))
                Relief((RectTransform)closeButton.transform,false,false);
            var closeLabel=Label(closeButton.transform,"Accessible close","X",24,Ivory,TextAnchor.MiddleCenter);
            Stretch(closeLabel.rectTransform);
            calendar=NewRect("Seven genuine rewards",cardSurface);
            for(int i=0;i<7;i++)
            {
                var day=NewRect("Day "+(i+1)+" / non-clickable informational card",calendar);
                dayBack[i]=Plate(day,i==6?new Color(.27f,.20f,.11f,1f):Oak,false);
                if(!ApplyAuthoredFrame(day,"valoria_frame_card_256"))
                    Relief(day);
                var emblem=NewRect("Recognizable actual reward icon",day);
                dayGlyph[i]=emblem.gameObject.AddComponent<DailyTreasureRewardGlyph>();
                dayGlyph[i].Kind=i==0||i==3?DailyTreasureGlyphKind.Wood:
                    i==1?DailyTreasureGlyphKind.Stone:
                    i==5?DailyTreasureGlyphKind.Food:
                    i==6?DailyTreasureGlyphKind.Chest:DailyTreasureGlyphKind.Time;
                dayGlyph[i].raycastTarget=false;
                var iconSprite=LoadUnifiedResourceEmblem(DayIllustrations[i]);
                if(iconSprite)
                {
                    var painted=NewRect("Premium illustrated reward / no rays",day);
                    var icon=painted.gameObject.AddComponent<Image>();
                    icon.sprite=iconSprite;
                    icon.preserveAspect=true;
                    icon.raycastTarget=false;
                    dayArtwork[i]=icon;
                    dayGlyph[i].gameObject.SetActive(false);
                }
                dayHead[i]=Label(day,"Day heading","DÍA "+(i+1),17,Gold,TextAnchor.MiddleCenter);
                dayBody[i]=Label(day,"Actual reward contents","",15,Ivory,TextAnchor.MiddleCenter);
                Stretch(dayHead[i].rectTransform);
                Stretch(dayBody[i].rectTransform);
                Place(dayHead[i].rectTransform,.5f,1f,.5f,1f,0,-6,150,26);
            }
            claimButton=ActionButton(NewRect("Claim exact daily reward",cardSurface),Oak,OnPrimaryPressed);
            if(!ApplyAuthoredFrame((RectTransform)claimButton.transform,
                "valoria_frame_button_512x128"))
                Relief((RectTransform)claimButton.transform,false,true);
            claimCaption=Label(claimButton.transform,"Claim label","RECLAMAR TRIBUTO",19,Ivory,TextAnchor.MiddleCenter);
            Stretch(claimCaption.rectTransform);

            ceremony=NewRect("Saved reward / optional animated reveal",cardSurface);
            // This block has no reward-awarding logic. Existing REAL Eldoria
            // scenery is the focal illustration of narrative milestones.
            // The authored coffer belongs exclusively to earned daily gifts.
            var sceneryRect=NewRect("Genuine Eldoria milestone scenery / visual only",ceremony);
            milestoneScenery=sceneryRect.gameObject.AddComponent<RawImage>();
            milestoneScenery.raycastTarget=false;
            milestoneScenery.color=new Color(.58f,.53f,.48f,.26f);
            milestoneScenery.gameObject.SetActive(false);
            var veilRect=NewRect("Dim painterly setting for the earned chest",ceremony);
            milestoneVignette=Plate(veilRect,new Color(.028f,.023f,.027f,.26f),false);
            milestoneVignette.gameObject.SetActive(false);
            // The real 3D Lyra render is full-height and contains a large
            // transparent sky margin. Fitting the ENTIRE source into a 245px
            // panel made her face a tiny dark thumbnail. A real uGUI clip
            // viewport frames her upper body without editing or faking the
            // canonical portrait pixels; painted architecture stays in front.
            milestoneLyraViewport=NewRect(
                "Canonical Lyra / cinematic scene aperture",ceremony);
            milestoneLyraViewport.gameObject.AddComponent<RectMask2D>();
            var lyraPortraitRect=NewRect(
                "Canonical Lyra / actual recruited hero illustration",
                milestoneLyraViewport);
            milestoneLyraPortrait=Plate(lyraPortraitRect,Color.white,false);
            milestoneLyraPortrait.preserveAspect=true;
            milestoneLyraPortrait.gameObject.SetActive(false);
            // Original 1376px transparent compositions, not flattened
            // screenshots. Lyra's empty arch reveals the canonical figure;
            // Scouts, Vigilants and Bastion show only actual in-game identity.
            var paintedStage=NewRect(
                "Unique Eldoria event stage / transparent foreground",ceremony);
            milestoneStageArt=Plate(paintedStage,Color.white,false);
            milestoneStageArt.preserveAspect=true;
            milestoneStageArt.raycastTarget=false;
            milestoneStageArt.gameObject.SetActive(false);
            var authenticDiscovery=NewRect(
                "Actual claimed Scout Kind / feathered ink on original parchment",
                paintedStage);
            milestoneDiscoveryIllustration=authenticDiscovery.gameObject
                .AddComponent<RawImage>();
            milestoneDiscoveryIllustration.raycastTarget=false;
            // The source PNG contains an EMPTY parchment. Paint only in its
            // interior, respecting the compass and burnt edges.
            // Preserve the atlas's TRUE square vignette proportions:
            // art source ratio 1376/768, so .35 wide / .62 tall is ~= 1:1.
            // Stretching a quadrant 1.5x made the new painting unconvincing.
            // Remain inside original scroll and avoid its real brass compass.
            authenticDiscovery.anchorMin=new Vector2(.43f,.18f);
            authenticDiscovery.anchorMax=new Vector2(.78f,.80f);
            authenticDiscovery.offsetMin=authenticDiscovery.offsetMax=Vector2.zero;
            milestoneDiscoveryIllustration.gameObject.SetActive(false);
            // Authored source atlas uses four quadrants. The GPU shader
            // feathers atlas pixels as actual parchment INK rather than a
            // pasted opaque rectangular screenshot.
            var scoutInkShader=Resources.Load<Shader>(
                "DailyTreasureCeremonies/DailyScoutDiscoveryInk");
            if(scoutInkShader && scoutInkShader.isSupported)
            {
                ownedScoutInkMaterial=new Material(scoutInkShader)
                {
                    name="Tesoros / disposable canonical Scout ink"
                };
                milestoneDiscoveryIllustration.material=ownedScoutInkMaterial;
            }
            // Authentic original scouting / Vigilants crests already belong
            // to Eldoria; a restrained insignia strengthens each milestone
            // without inventing a new faction, reward, hero or building.
            var insigniaRect=NewRect(
                "Authentic Eldoria event insignia / visual only",ceremony);
            milestoneInsignia=Plate(insigniaRect,Color.white,false);
            milestoneInsignia.preserveAspect=true;
            // Keep the EXACT original real compass mark; make its pixels
            // forged rather than a blue sticker glued to painted iron.
            var heraldicShader=Resources.Load<Shader>(
                "DailyTreasureCeremonies/DailyHeraldicRelief");
            if(heraldicShader && heraldicShader.isSupported)
                ownedVigilantsReliefMaterial=new Material(heraldicShader)
                {name="Tesoros / original emblem as forged-bronze relief"};
            milestoneInsignia.gameObject.SetActive(false);
            // The real completed Bastion supplies the landscape. All FOUR
            // milestones now use the SAME locally confined, unframed basalt
            // reading ink for their committed mailbox text. The actual Eldoria
            // painting stays visible outside that text field; daily gifts
            // retain their distinct coffer and unchanged reward geometry.
            var cityInk=NewRect("Bastion / real-city milestone caption ink",ceremony);
            bastionStoryInk=Plate(cityInk,new Color(.045f,.049f,.052f,.82f),false);
            cityInk.gameObject.SetActive(false);
            milestoneBanner=NewRect("Milestone heraldic event banner / visual only",ceremony);
            // A literary heading is NOT a second clickable bronze button.
            // Keep the real event artwork unobstructed and legible.
            Plate(milestoneBanner,new Color(.050f,.048f,.046f,.58f),false);
            milestoneBannerCaption=Label(milestoneBanner,
                "Real milestone category","",19,Ivory,TextAnchor.MiddleCenter);
            Stretch(milestoneBannerCaption.rectTransform);
            var headingShadow=milestoneBannerCaption.gameObject.AddComponent<Shadow>();
            headingShadow.effectColor=new Color(.01f,.008f,.007f,.92f);
            headingShadow.effectDistance=new Vector2(1f,-2f);
            // One subtle event-specific underline, not a bright new frame.
            var ruleRect=NewRect("Event-specific forged-ink underline / visual only",
                milestoneBanner);
            milestoneRule=Plate(ruleRect,new Color(.84f,.63f,.40f,.72f),false);
            milestoneBanner.gameObject.SetActive(false);
            glowRect=NewRect("Warm radial light / visual only",ceremony);
            glow=Plate(glowRect,new Color(1f,.65f,.18f,0f),false);
            CreateGlow();
            entryGlow.sprite=glowSprite;
            entryGlow.color=new Color(1f,.69f,.25f,0f);
            // The actual seventh-day chest has an understated amber
            // anticipation effect without an extra downloaded texture.
            var rareHalo=NewRect("Seventh-day illustrated chest aura / no rays",
                dayBack[6].transform);
            rareHalo.SetAsFirstSibling();
            rareDayGlow=Plate(rareHalo,new Color(1f,.76f,.31f,0f),false);
            rareDayGlow.sprite=glowSprite;
            rareDayGlow.preserveAspect=true;
            ambientRect=NewRect("Subtle event-specific atmospheric halo / visual only",ceremony);
            ambient=Plate(ambientRect,Color.clear,false);
            ambient.sprite=glowSprite;
            ambient.preserveAspect=true;
            ambientRect.SetAsFirstSibling();
            raysRect=NewRect("Gentle original amber volumetric rays / visual only",ceremony);
            rays=raysRect.gameObject.AddComponent<DailyTreasureRaysGraphic>();
            rays.raycastTarget=false;
            rays.SetIntensity(0f);
            var chestRect=NewRect("Real animated chest / two-layer bronze oak graphic",ceremony);
            chest=chestRect.gameObject.AddComponent<DailyTreasureChestGraphic>();
            chest.raycastTarget=false;
            chest.color=Color.white;
            // Independent original illustrated sprites are optional until their
            // art-package files are actually imported and approved in Unity.
            // Keep an honest procedural fallback, never a fake flat screenshot.
            var closedSource=Resources.Load<Sprite>(
                "DailyTreasureArt/treasure_closed_512");
            var openSource=Resources.Load<Sprite>(
                "DailyTreasureArt/treasure_opened_512");
            hasAuthoredChestArt=closedSource && openSource;
            illustratedClosed=CreateIllustratedChest(ceremony,
                "Authored chest / closed",closedSource);
            illustratedOpen=CreateIllustratedChest(ceremony,
                "Authored chest / opened",openSource);
            illustratedClosed.gameObject.SetActive(false);
            illustratedOpen.gameObject.SetActive(false);
            resultText=Label(ceremony,"Confirmed received items","",21,Ivory,TextAnchor.MiddleCenter);

            // Real, individually illustrated resource cards. They are shown
            // only AFTER the LocalGateway has irrevocably credited the goods.
            // The same source icons appear in the daily calendar, and each
            // card uses the original premium nine-slice frame.
            prizeShelf=NewRect("Actually earned inventory reveal / no payout",ceremony);
            for(int i=0;i<prizeRects.Length;i++)
            {
                var card=NewRect("Earned item "+(i+1),prizeShelf);
                Plate(card,new Color(.18f,.14f,.13f,.95f),false);
                if(!ApplyAuthoredFrame(card,"valoria_frame_card_256"))
                    Relief(card);
                prizeRects[i]=card;
                prizeGroups[i]=card.gameObject.AddComponent<CanvasGroup>();
                prizeGroups[i].alpha=0f;
                prizeGroups[i].blocksRaycasts=false;
                prizeGroups[i].interactable=false;
                var iconRect=NewRect("Real award icon",card);
                prizeIcons[i]=Plate(iconRect,Color.white,false);
                prizeIcons[i].preserveAspect=true;
                prizeNames[i]=Label(card,"Reward name","",12,Gold,TextAnchor.MiddleCenter);
                prizeAmounts[i]=Label(card,"Actual paid count","",18,Ivory,TextAnchor.MiddleCenter);
                card.gameObject.SetActive(false);
            }
            prizeShelf.gameObject.SetActive(false);
            for(int i=0;i<sparks.Length;i++)
            {
                var spark=NewRect("Ember "+(i+1),ceremony);
                sparks[i]=Plate(spark,new Color(1f,.75f,.30f,0f),false);
                sparks[i].sprite=glowSprite;
                sparks[i].preserveAspect=true;
                Place(spark,.5f,.5f,.5f,.5f,0,0,
                    i%4==0?11f:6f,i%4==0?11f:6f);
            }
            skipButton=ActionButton(NewRect("Skip animation / keep earned items",ceremony),
                new Color(.23f,.19f,.17f,1f),SkipReveal);
            if(!ApplyAuthoredFrame((RectTransform)skipButton.transform,
                "valoria_frame_button_512x128"))
                Relief((RectTransform)skipButton.transform,false,false);
            var skipText=Label(skipButton.transform,"Skip caption","SALTAR",15,Gold,TextAnchor.MiddleCenter);
            Stretch(skipText.rectTransform);
            ceremony.gameObject.SetActive(false);
            screen.gameObject.SetActive(false);
            // The entry medallion is the ONLY control visible before the
            // calendar opens. It must have live geometry immediately.
            Layout();
        }
        static Image CreateIllustratedChest(Transform parent,string name,Sprite art)
        {
            var rect=NewRect(name,parent);
            var visual=rect.gameObject.AddComponent<Image>();
            visual.sprite=art;
            visual.preserveAspect=true;
            visual.raycastTarget=false;
            visual.color=Color.white;
            return visual;
        }
        static AudioClip CreateFoleyClip(string name,DailyTreasureFoleyCue cue)
        {
            // Original authored one-shot SFX, never music or externally copied
            // sound. Pre-render once per panel; no DSP or buffer allocations
            // in the animation frame loop.
            const int rate=DailyTreasureFoley.SampleRate;
            var data=DailyTreasureFoley.Render(cue,rate);
            var clip=AudioClip.Create(name,data.Length,1,rate,false);
            if(!clip.SetData(data,0))
            {
                DestroyOwned(clip);
                return null;
            }
            return clip;
        }
        void CreateGlow()
        {
            const int size=64;
            glowTexture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            glowTexture.name="valoria-treasure-glow-generated-once";
            glowTexture.wrapMode=TextureWrapMode.Clamp;
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=(x+0.5f-size*.5f)/(size*.5f);
                float v=(y+0.5f-size*.5f)/(size*.5f);
                float radius=Mathf.Sqrt(u*u+v*v);
                float a=Mathf.Pow(Mathf.Clamp01(1f-radius),2f);
                pixels[y*size+x]=new Color(1f,.78f,.34f,a);
            }
            glowTexture.SetPixels(pixels);glowTexture.Apply(false,true);
            glowSprite=Sprite.Create(glowTexture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100f);
            glow.sprite=glowSprite;
        }
        static void DestroyOwned(UnityEngine.Object asset)
        {
            if(!asset)return;
            // Native evidence creates a disposable editor scene. Destruction
            // must work there as well as in the real runtime without errors.
            if(UnityEngine.Application.isPlaying)UnityEngine.Object.Destroy(asset);
            else UnityEngine.Object.DestroyImmediate(asset);
        }
        void OnDestroy()
        {
            // The city can unload during a ceremony without calling Close().
            // A native RenderTexture allocated by the authenticated Bastion
            // camera must not survive that teardown on memory-limited iOS.
            if(capturedBastionScene)
            {
                if(milestoneScenery)milestoneScenery.texture=null;
                if(milestoneCinematicBackdrop)
                    milestoneCinematicBackdrop.texture=null;
                DailyTreasureBastionCamera.Release(capturedBastionScene);
                capturedBastionScene=null;
            }
            DestroyOwned(glowSprite);
            DestroyOwned(glowTexture);
            DestroyOwned(chestKnock);
            DestroyOwned(chestChime);
            DestroyOwned(chestGrandChime);
            DestroyOwned(ownedScoutInkMaterial);
            DestroyOwned(ownedCinematicBackdropMaterial);
            DestroyOwned(ownedVigilantsReliefMaterial);
        }
        bool PresentNextCommittedMilestone()
        {
            if(suppressed || IsOpen || HasVisibleCanonicalHeroRoom() ||
               HasVisibleVigiliaBuildingPanel() ||
               !milestoneQueue.TryPeek(out var milestone))return false;
            if(!PresentCommittedMilestone(milestone.Title,milestone.Detail,
                legendary:milestone.Legendary,eventId:milestone.EventId))
                return false;
            milestoneQueue.ConfirmPresented(milestone.EventId);
            sessionAutoCeremonyPlayed=true;
            return true;
        }

        void OpenEntryOrDeferredMilestone()
        {
            if(suppressed || gateway==null || now==null ||
               HasVisibleCanonicalHeroRoom() ||
               HasVisibleVigiliaBuildingPanel())return;
            // Daily-claim access always wins: do not hide a real reward behind
            // an unrelated celebration. Once claimed, the same embossed
            // medallion offers the next committed milestone on player request.
            bool claimAvailable=DailyTreasureRules.CanClaim(
                gateway.Snapshot().DailyTreasures,now());
            if(!claimAvailable && milestoneQueue.PendingCount>0 &&
               PresentNextCommittedMilestone())
                return;
            Open();
        }

        public void Open()
        {
            if(suppressed || gateway==null || now==null ||
               HasVisibleCanonicalHeroRoom() ||
               HasVisibleVigiliaBuildingPanel())return;
            // The chest modal must outrank the RoyalGold screens which are
            // siblings of this independent UI host inside the same safe area.
            // Reordering only the inner screen previously left it invisible
            // behind whichever kingdom room was last opened.
            root.SetAsLastSibling();
            screen.gameObject.SetActive(true);
            screen.SetAsLastSibling();
            // The entry medallion is NOT a second HUD behind the open modal.
            // Authentic native frames showed it poking outside the left frame.
            entryRect.gameObject.SetActive(false);
            ceremony.gameObject.SetActive(false);
            header.gameObject.SetActive(true);
            subhead.gameObject.SetActive(true);
            playing=false;
            ClearPrizeCards();
            claimNotice="";
            noticeUntil=0f;
            lastPaintedUtcDay=-1;
            calendar.gameObject.SetActive(true);
            claimButton.gameObject.SetActive(true);
            lastWidth=lastHeight=-1;
            Layout();
            Refresh();
        }
        public void Close()
        {
            // Leaving the ceremonial scene (or a tutorial taking focus) must
            // stop any half-played latch/chime. The reward is already saved;
            // no audio continues over the actual city or another room.
            if(originalSfx && originalSfx.isPlaying)originalSfx.Stop();
            if(screen)screen.gameObject.SetActive(false);
            if(entryRect)entryRect.gameObject.SetActive(!suppressed &&
                !HasVisibleCanonicalHeroRoom() &&
                !HasVisibleVigiliaBuildingPanel());
            currentStage=DailyTreasureStageKind.Daily;
            activeMilestoneId=null;
            if(screenShade)screenShade.color=new Color(.018f,.020f,.026f,CalendarBackdropOpacity);
            if(mainFrameSurface)
                mainFrameSurface.color=mainFrameSurface.sprite?Color.white:Stone;
            if(milestoneBanner)milestoneBanner.gameObject.SetActive(false);
            if(bastionStoryInk)bastionStoryInk.gameObject.SetActive(false);
            // Release the ONLY references owned by this optional ceremony to
            // large original painterly illustrations. Mobile WebGL must not
            // keep a 3MB+ scene texture alive just because a chest was opened.
            // The original shared Resources assets themselves are not deleted.
            if(milestoneScenery)
            {
                milestoneScenery.texture=null;
                milestoneScenery.gameObject.SetActive(false);
                milestoneSceneryKey=null;
            }
            if(milestoneCinematicBackdrop)
            {
                milestoneCinematicBackdrop.texture=null;
                milestoneCinematicBackdrop.gameObject.SetActive(false);
            }
            if(capturedBastionScene)
            {
                DailyTreasureBastionCamera.Release(capturedBastionScene);
                capturedBastionScene=null;
            }
            bastionCameraPending=false;
            bastionCameraRetries=0;
            nextBastionCameraTry=0f;
            if(milestoneVignette)
                milestoneVignette.gameObject.SetActive(false);
            if(milestoneInsignia)
            {
                milestoneInsignia.sprite=null;
                milestoneInsignia.material=null;
                milestoneInsignia.gameObject.SetActive(false);
            }
            if(milestoneLyraPortrait)
            {
                milestoneLyraPortrait.sprite=null;
                milestoneLyraPortrait.gameObject.SetActive(false);
                nextLyraArtProbe=stopLyraArtProbe=0f;
                lyraFallbackActive=false;
            }
            if(milestoneStageArt)
            {
                milestoneStageArt.sprite=null;
                milestoneStageArt.gameObject.SetActive(false);
                milestoneArtworkResourceKey=null;
            }
            if(milestoneOrnament)milestoneOrnament.gameObject.SetActive(false);
            if(milestoneDiscoveryIllustration)
            {
                milestoneDiscoveryIllustration.texture=null;
                milestoneDiscoveryIllustration.gameObject.SetActive(false);
                verifiedScoutKind=-1;
            }
            if(mainFrameSurface)
            {
                mainFrameSurface.sprite=dailyFrameSprite;
                mainFrameSurface.type=dailyFrameSprite?
                    Image.Type.Sliced:Image.Type.Simple;
                mainFrameSurface.color=dailyFrameSprite?Color.white:Stone;
            }
            if(header)header.gameObject.SetActive(true);
            if(subhead)subhead.gameObject.SetActive(true);
            ClearPrizeCards();
            playing=false;
        }
        void Refresh()
        {
            if(gateway==null||now==null||!IsOpen)return;
            var state=gateway.Snapshot();
            var daily=state.DailyTreasures??new DailyTreasureState();
            long today=DailyTreasureRules.UtcDay(now());
            lastPaintedUtcDay=today;
            var reward=DailyTreasureRules.Preview(daily,now());
            int upcoming=DailyTreasureRules.NextDay(daily);
            // If the user missed a local UTC day, show the actual broken
            // consecutive streak immediately without altering earned prizes.
            int consecutive=daily.LastClaimUtcDay>=0 &&
                today-daily.LastClaimUtcDay>1?0:
                Math.Max(0,daily.ConsecutiveDays);
            bool showNotice=!string.IsNullOrEmpty(claimNotice) &&
                Time.unscaledTime<noticeUntil;
            string streakInfo=reward.IsValid && reward.BonusFood>0
                ? "RACHA "+reward.Streak+" · +"+reward.BonusFood+" COMIDA HOY"
                : "RACHA "+consecutive+" · EXTRA EN "+
                    (DailyTreasureRules.NextConsecutiveBonusDay(consecutive)-
                     consecutive)+" DÍA(S)";
            subhead.text=showNotice?claimNotice:streakInfo;
            subhead.color=showNotice?new Color(1f,.68f,.48f,1f):Muted;
            for(int i=0;i<7;i++)
            {
                bool next=i+1==upcoming && reward.IsValid;
                var item=DailyTreasureRules.ForDay(i+1,next?reward.Streak:0);
                bool newCycle=daily.CycleDay==7 && upcoming==1 && reward.IsValid;
                bool collected=!newCycle && daily.CycleDay>0 && i+1<=daily.CycleDay;
                dayHead[i].text="DÍA "+(i+1)+(collected?"  OK":next?"  HOY":"");
                dayHead[i].color=next?Ivory:Gold;
                if(dayBack[i].sprite)
                    dayBack[i].color=next?Color.white:
                        collected?new Color(.62f,.62f,.59f,1f):
                        new Color(.83f,.80f,.75f,1f);
                else
                    dayBack[i].color=next?new Color(.42f,.29f,.16f,1f):
                        collected?new Color(.15f,.18f,.15f,1f):
                        i==6?new Color(.28f,.20f,.11f,1f):Oak;
                dayBody[i].text=ShortReward(item);
                dayBody[i].color=collected?Muted:Ivory;
                if(dayArtwork[i])
                    dayArtwork[i].color=collected?
                        new Color(.71f,.69f,.64f,.79f):Color.white;
            }
            rareDayAvailable=reward.IsValid && upcoming==7;
            if(rareDayGlow)rareDayGlow.color=new Color(1f,.78f,.32f,
                rareDayAvailable?.26f:.045f);
            claimButton.interactable=reward.IsValid;
            claimCaption.text=reward.IsValid?"RECLAMAR DÍA "+reward.CycleDay:
                daily.TotalClaims==int.MaxValue?"TRIBUTO NO DISPONIBLE":
                    NextRewardCountdown(now());
            // Refresh changes the contents AFTER the initial Layout in Open().
            // Loyalty can add a new second line on day three; reflow right
            // now, before the first player frame or native pointer, rather
            // than leaving the extra food painted over the bronze border.
            if(calendar && calendar.gameObject.activeInHierarchy)
                Layout();
        }
        static string NextRewardCountdown(long utcTicks)
        {
            int minutes=DailyTreasureRules.MinutesUntilNextUtcDay(utcTicks);
            return "SIGUIENTE EN "+(minutes/60).ToString("00")+":"+
                (minutes%60).ToString("00");
        }

        static string ShortReward(DailyTreasureReward reward)
        {
            // The seventh-day special prize contains FOUR actual rewards,
            // including one five-minute accelerator. In the first genuine
            // 1600x900 Unity capture the calendar only rendered three lines:
            // the fourth was clipped although it was correctly granted.
            // Keep all four promised items legible in a compact two-line
            // summary, rather than shrinking the font or hiding the prize.
            if(reward.CycleDay==7 && reward.Wood>0 &&
               reward.Stone>0 && reward.SpeedupFiveMinutes>0)
            {
                string top="Madera "+reward.Wood+" · Piedra "+reward.Stone;
                string bottom=(reward.Food>0?"Comida "+reward.Food+" · ":"")+
                    "5 min x"+reward.SpeedupFiveMinutes;
                return top+"\n"+bottom;
            }
            string text="";
            if(reward.Wood>0)text+="Madera "+reward.Wood+"\n";
            if(reward.Stone>0)text+="Piedra "+reward.Stone+"\n";
            if(reward.Food>0)text+="Comida "+reward.Food+"\n";
            if(reward.SpeedupOneMinute>0)text+="1 min x"+reward.SpeedupOneMinute+"\n";
            if(reward.SpeedupFiveMinutes>0)text+="5 min x"+reward.SpeedupFiveMinutes+"\n";
            return text.Trim();
        }
        void OnPrimaryPressed()
        {
            if(ceremony && ceremony.gameObject.activeSelf){Close();return;}
            Claim();
        }

        void ClearPrizeCards()
        {
            prizeCount=0;prizeAnimating=false;
            if(prizeShelf)prizeShelf.gameObject.SetActive(false);
            for(int i=0;i<prizeRects.Length;i++)
            {
                if(prizeRects[i])prizeRects[i].gameObject.SetActive(false);
                if(prizeGroups[i])prizeGroups[i].alpha=0f;
            }
        }
        void ConfigurePrizeCards(DailyTreasureReward reward)
        {
            ClearPrizeCards();
            var items=DailyTreasureRewardPresentation.FromCommittedReward(reward);
            prizeCount=items.Length;
            for(int i=0;i<prizeCount;i++)
            {
                var item=items[i];
                var sprite=LoadUnifiedResourceEmblem(item.Icon);
                // Never substitute an incorrect resource icon silently.
                prizeIcons[i].sprite=sprite;
                prizeIcons[i].color=sprite?Color.white:new Color(.6f,.6f,.6f,1f);
                prizeNames[i].text=item.Name;
                prizeNames[i].color=item.LoyaltyBonus?new Color(.98f,.70f,.34f,1f):Gold;
                prizeAmounts[i].text=item.Quantity;
                prizeRects[i].gameObject.SetActive(true);
                prizeGroups[i].alpha=0f;
            }
            if(prizeCount>0)Layout();
        }
        void SkipReveal()
        {
            // Skip means skip the presentation, including its remaining audio,
            // NEVER skip, re-award or reverse the already persisted items.
            if(originalSfx && originalSfx.isPlaying)originalSfx.Stop();
            ShowResult(true);
        }
        void PaintEarnedCards()
        {
            if(!prizeAnimating)return;
            bool finished=true;
            for(int i=0;i<prizeCount;i++)
            {
                // Exactly the same deterministic flight is evaluated by
                // Ubuntu tests and rendered by Unity: the gold-framed REAL
                // items emerge from the original illustrated chest without
                // generating any rewards in the presentation layer.
                var frame=DailyTreasurePrizeFlight.Sample(
                    Time.unscaledTime-prizeRevealStarted,i,chestReward,
                    prizeFlightOrigin.x,prizeFlightOrigin.y,
                    prizeHomes[i].x,prizeHomes[i].y);
                prizeGroups[i].alpha=frame.Opacity;
                prizeRects[i].anchoredPosition=new Vector2(frame.X,frame.Y);
                prizeRects[i].localScale=Vector3.one*frame.Scale;
                prizeRects[i].localRotation=Quaternion.Euler(0f,0f,frame.Angle);
                if(!frame.Finished)finished=false;
            }
            if(finished)
            {
                prizeAnimating=false;
                for(int i=0;i<prizeCount;i++)
                {
                    prizeRects[i].anchoredPosition=prizeHomes[i];
                    prizeRects[i].localScale=Vector3.one;
                    prizeRects[i].localRotation=Quaternion.identity;
                    prizeGroups[i].alpha=1f;
                }
            }
        }

        void Claim()
        {
            if(gateway==null||now==null || playing)return;
            var before=gateway.Snapshot();
            if(!DailyTreasureRules.CanClaim(before.DailyTreasures,now())){Refresh();return;}
            var command=new GameCommand(Guid.NewGuid().ToString("N"),
                before.PlayerId,"ClaimDailyTreasure","valoria",before.Revision);
            var result=gateway.Execute(command);
            if(!result.Ok)
            {
                // A full wallet or obsolete command must not look like a dead
                // button. The server/gateway is authoritative; no rewards are
                // granted by this view or its animation.
                claimNotice=string.IsNullOrWhiteSpace(result.Message)
                    ? "No se ha podido recoger el tributo":result.Message;
                noticeUntil=Time.unscaledTime+4.5f;
                Refresh();
                return;
            }
            claimNotice="";noticeUntil=0f;
            var after=gateway.Snapshot();
            var current=after.DailyTreasures;
            var reward=DailyTreasureRules.ForDay(current.CycleDay,current.ConsecutiveDays);
            refreshed?.Invoke();
            ConfigurePrizeCards(reward);
            Present("TRIBUTO DEL DÍA "+reward.CycleDay,
                DailyTreasureRules.Receipt(reward),reward.ShowChest,
                !reward.ShowChest?DailyTreasureCeremonyTier.Quiet:
                reward.CycleDay==7?DailyTreasureCeremonyTier.Legendary:
                DailyTreasureCeremonyTier.Special);
        }
        // Chapter battles and first-family discoveries happen while their own
        // room remains open. Queue the ceremony until the room/tutorial closes.
        // Verify the gateway's existing report first; queueing cannot pay.
        public bool QueueCommittedMilestone(string title,string confirmedResult,
            string receiptId,string receiptType,bool legendary,string eventId)
        {
            if(gateway==null || now==null)return false;
            return milestoneQueue.AddConfirmedReport(gateway.Snapshot(),
                receiptId,receiptType,eventId,title,confirmedResult,legendary);
        }

        // Called only after the game's authoritative reward/report has ALREADY
        // been committed by its existing command. Never calls Execute,
        // grants a bonus, opens the daily claim or invents a loot table.
        public bool PresentCommittedMilestone(string title,string confirmedResult,
            bool legendary=true,string eventId=null)
        {
            if(gateway==null || now==null || screen==null ||
               string.IsNullOrWhiteSpace(title) || suppressed ||
               HasVisibleCanonicalHeroRoom() ||
               HasVisibleVigiliaBuildingPanel())return false;
            if(IsOpen)return false;
            ClearPrizeCards();
            // Important victories are presented above the room whose real
            // gateway command just finished, without creating a second Canvas.
            root.SetAsLastSibling();
            screen.gameObject.SetActive(true);
            screen.SetAsLastSibling();
            entryRect.gameObject.SetActive(false);
            lastWidth=lastHeight=-1f;
            Layout();
            Present(title,confirmedResult,true,legendary?
                DailyTreasureCeremonyTier.Legendary:
                DailyTreasureCeremonyTier.Special,eventId);
            return true;
        }

        // Safe to reuse for milestone ceremonies: NEVER mints rewards.
        public void Present(string title,string alreadyReceived,bool showChest)
            =>Present(title,alreadyReceived,showChest,
                showChest?DailyTreasureCeremonyTier.Special:
                    DailyTreasureCeremonyTier.Quiet);

        public void Present(string title,string alreadyReceived,bool showChest,
            DailyTreasureCeremonyTier tier,string eventId=null)
        {
            if(!IsOpen)return;
            // A claim can transition to its own ceremony without an Update().
            entryRect.gameObject.SetActive(false);
            ceremonyTier=tier;
            activeTheme=DailyTreasureCeremonyTheme.ForEvent(
                eventId,tier==DailyTreasureCeremonyTier.Legendary);
            currentStage=DailyTreasureStageDirection.FromEvent(eventId);
            activeMilestoneId=eventId;
            string insigniaPath=currentStage==DailyTreasureStageKind.Explorers?
                "EldoriaRetention/scouts-crest":
                currentStage==DailyTreasureStageKind.Vigilants?
                "EldoriaRetention/vigilants-compass":null;
            milestoneInsignia.sprite=insigniaPath==null?null:
                Resources.Load<Sprite>(insigniaPath);
            milestoneInsignia.material=
                currentStage==DailyTreasureStageKind.Vigilants?
                    ownedVigilantsReliefMaterial:null;
            milestoneInsignia.gameObject.SetActive(milestoneInsignia.sprite!=null);
            if(milestoneBanner)
            {
                milestoneBanner.gameObject.SetActive(
                    currentStage!=DailyTreasureStageKind.Daily);
                switch(currentStage)
                {
                    case DailyTreasureStageKind.Lyra:
                        milestoneBannerCaption.text="LYRA · NUEVA ALIADA";
                        break;
                    case DailyTreasureStageKind.Explorers:
                        milestoneBannerCaption.text="PRIMER HALLAZGO";
                        break;
                    case DailyTreasureStageKind.Vigilants:
                        milestoneBannerCaption.text="CAPÍTULO SUPERADO";
                        break;
                    case DailyTreasureStageKind.Bastion:
                        milestoneBannerCaption.text="EL REINO CRECE";
                        break;
                }
                // Shared iron/bronze grammar: the color only marks context.
                // Violet belongs exclusively to the Breach, not Vigilants;
                // there is no full-frame neon recoloring of real scenery.
                if(milestoneRule)
                    milestoneRule.color=new Color(activeTheme.R,
                        activeTheme.G,activeTheme.B,.72f);
            }
            ceremonialTitle=title;ceremonialReceipt=alreadyReceived;
            // Milestones are real story notifications, not invented loot.
            // Keep their paced reveal, but reserve the authored two-pose
            // coffer exclusively for actual daily-resource claims.
            bool dailyCoffer=showChest &&
                currentStage==DailyTreasureStageKind.Daily;
            chestReward=dailyCoffer;
            // A focused cinematic view replaces the calendar's small header.
            // This gives the same illustrated chest room to shine even on
            // compact phones rather than squeezing it below an extra title.
            header.gameObject.SetActive(false);
            subhead.gameObject.SetActive(false);
            Layout();
            calendar.gameObject.SetActive(false);
            claimButton.gameObject.SetActive(false);
            prizeShelf.gameObject.SetActive(false);
            ceremony.gameObject.SetActive(true);
            resultText.text=showChest?ceremonialTitle:"TRIBUTO DE VALORIA";
            playing=showChest;
            prizeAnimating=false;
            prizeShelf.gameObject.SetActive(false);
            rays.SetIntensity(0f);
            chimePlayed=false;
            if(UnityEngine.Application.isPlaying && originalSfx && chestKnock && dailyCoffer)
            {
                if(originalSfx.isPlaying)originalSfx.Stop();
                originalSfx.PlayOneShot(chestKnock);
            }
            startTime=Time.unscaledTime;
            chest.gameObject.SetActive(dailyCoffer && !hasAuthoredChestArt);
            chest.SetOpening(0f);
            illustratedClosed.gameObject.SetActive(dailyCoffer && hasAuthoredChestArt);
            illustratedOpen.gameObject.SetActive(dailyCoffer && hasAuthoredChestArt);
            illustratedClosed.color=Color.white;
            illustratedOpen.color=new Color(1f,1f,1f,0f);
            illustratedClosed.rectTransform.localScale=Vector3.one;
            illustratedOpen.rectTransform.localScale=Vector3.one;
            glow.color=new Color(1f,.75f,.36f,dailyCoffer?0f:.16f);
            ambient.color=Color.clear;
            SetMilestoneScenery(eventId,showChest);
            // Real Bastion geometry is the protagonist, not an old invented
            // stone arch pasted on top. The previous Unity city screenshots
            // showed that decorative gate covering the actual built castle.
            // For this stage use ONLY the authenticated one-frame live camera;
            // if it is unavailable, keep the truthful unobstructed city.
            var stagePath=showChest &&
                currentStage!=DailyTreasureStageKind.Bastion?
                MilestoneArtResource(currentStage):null;
            var stageSprite=stagePath==null?null:Resources.Load<Sprite>(stagePath);
            milestoneStageArt.sprite=stageSprite;
            milestoneStageArt.gameObject.SetActive(stageSprite!=null);
            milestoneArtworkResourceKey=stageSprite?stagePath:null;
            // Confirmed first discovery ONLY. The authoritative mailbox
            // already proves the precise family/order ID and award receipt.
            // A concept or visual-only preview must never invent a Kind.
            verifiedScoutKind=showChest &&
                currentStage==DailyTreasureStageKind.Explorers?
                    FindActualCommittedScoutKind(eventId):-1;
            var scoutAtlas=verifiedScoutKind>=0 && ownedScoutInkMaterial?
                Resources.Load<Texture2D>(
                    "EldoriaRetention/scout-mission-vignettes-v4"):null;
            if(milestoneDiscoveryIllustration)
            {
                milestoneDiscoveryIllustration.texture=scoutAtlas;
                milestoneDiscoveryIllustration.gameObject.SetActive(scoutAtlas!=null);
                if(scoutAtlas)
                {
                    int kind=verifiedScoutKind;
                    var tile=new Rect((kind%2)*.5f+.01f,
                        (kind<2?.5f:0f)+.01f,.48f,.48f);
                    milestoneDiscoveryIllustration.uvRect=tile;
                    ownedScoutInkMaterial.SetVector("_AtlasUv",
                        new Vector4(tile.x,tile.y,tile.width,tile.height));
                    milestoneDiscoveryIllustration.color=Color.white;
                }
            }
            if(milestoneLyraPortrait)
            {
                // Prefer the original canonical illustration for this 2D
                // ceremony, then reuse an existing Heroes portrait if absent.
                var portrait=currentStage==DailyTreasureStageKind.Lyra &&
                    showChest?TryCanonicalLyraPortrait():null;
                // An isolated native fixture may not contain the Heroes UI
                // component. Use a sprite copied byte-for-byte from the
                // original approved 4.77MB Lyra GLB's model render; this is
                // an AUTHENTIC IDENTITY FALLBACK, not an invented heroine.
                // Replace it with live HeroesUI portrait as soon as ready.
                lyraFallbackActive=false;
                if(!portrait && currentStage==DailyTreasureStageKind.Lyra &&
                   showChest)
                {
                    portrait=Resources.Load<Sprite>(
                        "DailyTreasureCeremonies/canonical_lyra_bust");
                    lyraFallbackActive=portrait!=null;
                }
                milestoneLyraPortrait.sprite=portrait;
                milestoneLyraPortrait.gameObject.SetActive(portrait!=null);
                lyraArtRevealAt=Time.unscaledTime;
                nextLyraArtProbe=Time.unscaledTime+.7f;
                stopLyraArtProbe=Time.unscaledTime+5.4f;
            }
            // The original approved artwork now has category-specific stage
            // composition, not merely a recolored copy of the daily calendar.
            Layout();
            // A new milestone's FIRST visible frame must already have its
            // own poised entrance, not jump from full size to reduced size.
            if(showChest)PaintMilestoneStageEntrance(0f);
            skipButton.gameObject.SetActive(showChest);
            if(!showChest)ShowResult(true);
        }
        static string MilestoneScenerySource(string eventId)
        {
            // Reuse ONLY actual existing production art (not synthetic
            // characters, fake rewards or images rendered from an old UI).
            if(string.IsNullOrEmpty(eventId))return null;
            if(eventId.StartsWith("milestone:hero:lyra",
                StringComparison.Ordinal))
                return "EldoriaV41/heroes-hall-illustrated-v2";
            if(eventId.StartsWith("scout:",StringComparison.Ordinal) ||
               eventId.StartsWith("exploration:",StringComparison.Ordinal))
                return "EldoriaRetention/scouts-painterly-v2";
            if(eventId.StartsWith("vigilant:",StringComparison.Ordinal))
                return "EldoriaRetention/vigilants-painterly-v2";
            // For Bastion, the ACTUAL CITY must remain visible behind the
            // temporary modal: never replace the player's city with a
            // misleading artwork of a different building level.
            return null;
        }
        // This project keeps HeroesApprovedDestination in Assembly-CSharp;
        // DailyTreasures is an isolated asmdef that intentionally cannot
        // reference it directly. Resolve its public, already-bound ReadHeroes
        // once on a committed Lyra ceremony, not on Update()/each mobile frame.
        // Reflection reads ONLY the authoritative Lyra portrait from that
        // destination; it never creates/unlocks a hero or changes a save.
        static Sprite TryCanonicalLyraPortrait()
        {
            // Ceremony uses the original owner reference illustration. The
            // hero detail keeps its interactive 3D model; no model is loaded
            // or imported by this overlay. Transparent extraction provenance
            // is recorded beside the candidate source, not assumed approved.
            var illustration=Resources.Load<Sprite>(
                "DailyTreasureCeremonies/canonical_lyra_illustration");
            if(illustration)return illustration;
            var components=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            foreach(var component in components)
            {
                if(!component)continue;
                var typeName=component.GetType().FullName;
                try
                {
                    // The canonical hero loader validates the original
                    // PR127 GLB by SHA256 and caches its native render as a
                    // Sprite. Reuse that completed portrait without ever
                    // loading another 4.77 MB GLB in the reward overlay.
                    if(typeName=="Eldoria.HeroesUI.HeroesLyraModel")
                    {
                        var type=component.GetType();
                        var value=type.GetProperty("Ready")?.GetValue(component);
                        if(value is bool ready && ready)
                        {
                            var sprite=type.GetProperty("Portrait")
                                ?.GetValue(component) as Sprite;
                            if(sprite && sprite.rect.width>=256f &&
                               sprite.rect.height>=256f)return sprite;
                        }
                        continue;
                    }
                    if(typeName!="Eldoria.HeroesUI.HeroesApprovedDestination")
                        continue;
                    var field=component.GetType().GetField("ReadHeroes");
                    var reader=field?.GetValue(component) as Delegate;
                    var heroes=reader?.DynamicInvoke() as Array;
                    if(heroes==null)continue;
                    foreach(var hero in heroes)
                    {
                        if(hero==null)continue;
                        var type=hero.GetType();
                        var id=type.GetField("Id")?.GetValue(hero) as string;
                        if(!string.Equals(id,"lyra",
                            StringComparison.OrdinalIgnoreCase))continue;
                        // A portrait is art, not a replacement for a genuine
                        // 3D hero. Reject thumbnail upscaling and null states.
                        var image=type.GetField("Portrait")?.GetValue(hero)
                            as Sprite;
                        if(image && image.rect.width>=256f &&
                           image.rect.height>=256f)
                            return image;
                    }
                }
                catch(Exception ex)
                {
                    Debug.LogWarning("Daily treasures: canonical Lyra art "+
                        "lookup unavailable: "+ex.Message);
                }
            }
            return null;
        }


        // A one-frame Bastion portrait is 600x900; an authenticated landscape
        // lens is 960x540. RawImage Cover cannot turn one into the other:
        // after a physical device rotation it crops MOST of the real
        // building and makes genuine stonework appear blurred. Keep the
        // painted event and committed report, but reshoot only when the
        // orientation changes. Never rotate the gameplay camera or mint
        // another milestone/reward; release the previous GPU texture.
        void RefreshBastionLensForRotation(float width,float height)
        {
            if(currentStage!=DailyTreasureStageKind.Bastion ||
               !ceremony || !ceremony.gameObject.activeSelf ||
               !capturedBastionScene)return;
            bool requiredPortrait=height>width;
            bool renderedPortrait=
                capturedBastionScene.height>capturedBastionScene.width;
            if(requiredPortrait==renderedPortrait)return;
            var replacement=DailyTreasureBastionCamera.Capture(width,height);
            if(!replacement)
            {
                // Retain the actual previous image rather than inventing a
                // fallback castle or allocating/retrying once per frame.
                Debug.LogWarning("BASTION_ROTATION_RECAPTURE_UNAVAILABLE");
                return;
            }
            var previous=capturedBastionScene;
            capturedBastionScene=replacement;
            if(milestoneScenery)milestoneScenery.texture=replacement;
            if(milestoneCinematicBackdrop)
                milestoneCinematicBackdrop.texture=replacement;
            DailyTreasureBastionCamera.Release(previous);
        }

        void SetMilestoneScenery(string eventId,bool showChest)
        {
            var path=showChest?MilestoneScenerySource(eventId):null;
            Texture2D source=path==null?null:Resources.Load<Texture2D>(path);
            if(capturedBastionScene)
            {
                milestoneScenery.texture=null;
                DailyTreasureBastionCamera.Release(capturedBastionScene);
                capturedBastionScene=null;
            }
            bastionCameraPending=false;
            bastionCameraRetries=0;
            if(showChest && !string.IsNullOrEmpty(eventId) &&
               eventId.StartsWith("milestone:bastion:",StringComparison.Ordinal))
            {
                capturedBastionScene=DailyTreasureBastionCamera.Capture(
                    root.rect.width,root.rect.height);
                // The gameplay owner may apply a newly built Bastion tier
                // one frame AFTER the milestone arrives. Do not show a fake
                // painting or abandon the actual 3D hero for a black box.
                // Retry at most three times, only after scene setup and
                // at most every .35 seconds (never per-frame on mobile).
                bastionCameraPending=!capturedBastionScene;
                nextBastionCameraTry=Time.unscaledTime+.35f;
            }
            milestoneSceneryKey=capturedBastionScene?
                DailyTreasureBastionCamera.SceneKey:source?path:null;
            milestoneScenery.texture=capturedBastionScene?
                (Texture)capturedBastionScene:source;
            bool hasImage=milestoneScenery.texture!=null;
            // The real city MUST be visible behind the temporary event,
            // never replace it with an old flat screenshot if capture fails.
            // Its same live camera preview is permitted as a muted
            // atmospheric layer; no extra render or duplicate texture.
            if(milestoneCinematicBackdrop)
            {
                milestoneCinematicBackdrop.texture=milestoneScenery.texture;
                milestoneCinematicBackdrop.gameObject.SetActive(
                    hasImage && ownedCinematicBackdropMaterial);
                // Final cover UV is set by UpdateSceneryCrop/ Layout().
            }
            milestoneScenery.gameObject.SetActive(hasImage);
            milestoneVignette.gameObject.SetActive(hasImage);
            if(hasImage)
            {
                // Preserve the actual painting as the central illustration.
                // A strong dark tint previously made the original hall/map
                // nearly invisible; legibility belongs to the separate ink.
                milestoneScenery.color=Color.white;
                UpdateSceneryCrop();
            }
        }
        // Maps the ACTUAL claimed discovery to an existing painterly atlas
        // family. This deliberately duplicates NO reward or economy logic:
        // the only valid input is a committed mailbox report written by
        // LocalGateway and belonging to an existing order receipt.
        int FindActualCommittedScoutKind(string eventId)
        {
            const string prefix="scout:first:";
            if(!eventId?.StartsWith(prefix,StringComparison.Ordinal) ?? true)
                return -1;
            string orderId=eventId.Substring(prefix.Length);
            if(string.IsNullOrWhiteSpace(orderId))return -1;
            var player=gateway?.Snapshot();
            if(player?.Mailbox?.Messages==null ||
               player.Retention==null)return -1;
            foreach(var report in player.Mailbox.Messages)
            {
                if(report==null || report.Type!="exploration" ||
                   report.Category!="informes" ||
                   report.DiscoveryOrderId!=orderId ||
                   report.DiscoveryKind<0 || report.DiscoveryKind>3 ||
                   string.IsNullOrEmpty(report.RewardReceiptId) ||
                   report.EventId!="exploration:"+report.RewardReceiptId ||
                   (player.Retention.ScoutDiscoveryMask&
                       (1<<report.DiscoveryKind))==0)continue;
                return report.DiscoveryKind;
            }
            return -1;
        }
        // The actual game character/map/emblem/city is not baked into these
        // decorative aperture layers. Daily gifts never receive a stage.
        static string MilestoneArtResource(DailyTreasureStageKind kind)
        {
            switch(kind)
            {
                case DailyTreasureStageKind.Lyra:
                    return "DailyTreasureCeremonies/lyra_stage";
                case DailyTreasureStageKind.Explorers:
                    return "DailyTreasureCeremonies/explorers_stage";
                case DailyTreasureStageKind.Vigilants:
                    return "DailyTreasureCeremonies/vigilants_stage";
                case DailyTreasureStageKind.Bastion:
                    // Provenance-retained candidate remains on disk, but
                    // cannot masquerade as the player's actual building.
                    return null;
                default:return null;
            }
        }

        // Retain genuine painterly proportions in wide/portrait layouts.
        // Cover by cropping original pixels, never by compressing the world.
        static Rect ScenicCoverUv(Texture texture,Rect rect)
        {
            float source=(float)texture.width/Mathf.Max(1,texture.height);
            float target=rect.width/Mathf.Max(1f,rect.height);
            if(target<source)
            {
                float width=Mathf.Clamp01(target/source);
                return new Rect((1f-width)*.5f,0f,width,1f);
            }
            float height=Mathf.Clamp01(source/target);
            return new Rect(0f,(1f-height)*.5f,1f,height);
        }
        void UpdateSceneryCrop()
        {
            if(milestoneScenery && milestoneScenery.texture)
            {
                var rect=milestoneScenery.rectTransform.rect;
                if(rect.width>=10f && rect.height>=10f)
                {
                    milestoneScenery.uvRect=ScenicCoverUv(
                        milestoneScenery.texture,rect);
                    milestoneSceneryBaseUv=milestoneScenery.uvRect;
                }
            }
            if(milestoneCinematicBackdrop &&
               milestoneCinematicBackdrop.texture)
            {
                var rect=milestoneCinematicBackdrop.rectTransform.rect;
                if(rect.width>=10f && rect.height>=10f)
                {
                    var uv=ScenicCoverUv(
                        milestoneCinematicBackdrop.texture,rect);
                    milestoneCinematicBackdrop.uvRect=uv;
                    if(ownedCinematicBackdropMaterial)
                    {
                        // Ink position is evaluated in visible on-screen UV,
                        // never the source UV cropped during device rotation.
                        ownedCinematicBackdropMaterial.SetVector(
                            "_VisibleUv",new Vector4(
                                uv.x,uv.y,uv.width,uv.height));
                        bool stacked=!DailyTreasureStageDirection.Compose(
                            activeMilestoneId,root.rect.width,
                            root.rect.height).SideBySide;
                        ownedCinematicBackdropMaterial.SetFloat(
                            "_PortraitReading",stacked?1f:0f);
                    }
                }
            }
        }
        // Render the same authentic painting with a measured, category-
        // specific tint. Native 42818514510 crushed the *existing* Lyra
        // hall to ten color bins in portrait; no replacement art is used.
        static Color MilestonePaintTint(DailyTreasureStageLayout stage,
            float intensity=1f)
        {
            bool hallPortrait=stage.Kind==DailyTreasureStageKind.Lyra &&
                !stage.SideBySide;
            // The genuine close-up already carries PBR/real city lighting.
            // Do not crush the masonry and amber relief a second time.
            if(stage.Kind==DailyTreasureStageKind.Bastion)
                return new Color(.98f,.95f,.89f,.95f*intensity);
            return new Color(hallPortrait?.82f:.70f,
                hallPortrait?.79f:.68f,hallPortrait?.74f:.65f,
                stage.SceneryOpacity*intensity);
        }
        void PaintMilestoneAtmosphere(float elapsed,DailyTreasureRevealFrame frame)
        {
            if(!milestoneScenery || !milestoneScenery.texture ||
               !milestoneScenery.gameObject.activeInHierarchy ||
               root.rect.width<275f || root.rect.height<300f)
                return;
            // Crop and drift the ACTUAL original image with guaranteed UV
            // bounds. Painterly depth has no texture seams or procedural
            // imitation of the approved hero and world art.
            var crop=milestoneSceneryBaseUv;
            float freeX=Mathf.Max(0f,1f-crop.width);
            float freeY=Mathf.Max(0f,1f-crop.height);
            crop.x=Mathf.Clamp(crop.x+
                DailyTreasureStageDirection.SafePainterlyDrift(
                    elapsed,freeX,false),0f,freeX);
            crop.y=Mathf.Clamp(crop.y+
                DailyTreasureStageDirection.SafePainterlyDrift(
                    elapsed,freeY,true),0f,freeY);
            milestoneScenery.uvRect=crop;
            var stage=DailyTreasureStageDirection.Compose(
                activeMilestoneId,root.rect.width,root.rect.height);
            milestoneScenery.color=MilestonePaintTint(stage,
                .93f+.07f*frame.Glow);
        }
        void PaintMilestoneStageEntrance(float elapsed)
        {
            if(currentStage==DailyTreasureStageKind.Daily ||
               !milestoneStageArt || !milestoneStageArt.sprite ||
               !milestoneStageArt.gameObject.activeInHierarchy)return;
            var beat=DailyTreasureStageDirection.SampleEntrance(
                currentStage,elapsed);
            // Restore from Layout()'s real anchored geometry; never add
            // offsets to LAST frame's position or drift across rotations.
            milestoneStageArt.rectTransform.anchoredPosition=
                milestoneStageRest+new Vector2(beat.ShiftX,beat.ShiftY);
            milestoneStageArt.rectTransform.localScale=
                Vector3.one*beat.Scale;
            milestoneStageArt.color=new Color(1f,1f,1f,
                milestoneStageRestOpacity*beat.Opacity);
        }
        static string ConfirmedMilestoneSummary(string committedText)
        {
            if(string.IsNullOrWhiteSpace(committedText))
                return "Acontecimiento registrado";
            // One strong line, not a microscopic battle log in a ceremonial
            // frame. The complete existing report remains accessible in
            // the standard game mailbox, with no synthetic loot.
            string first=committedText.Replace("\r","")
                .Split('\n')[0].Trim();
            if(first.Length<=56)return first;
            int stop=first.LastIndexOf(' ',55);
            if(stop<29)stop=55;
            return first.Substring(0,stop).TrimEnd()+"…";
        }

        // Genuine Unity run 42805795367: the portrait Vigilants receipt
        // needs 110px of glyph lines inside an 84px text box. A ceremonial
        // panel must NEVER truncate a true report merely to preserve three
        // redundant sentences. Prefer the complete committed summary when
        // it fits the measured *actual font*; otherwise display the exact
        // event title and a concise mailbox pointer. The canonical mailbox
        // always retains the full report, and this UI never awards anything.
        // Re-evaluate after orientation changes rather than shrinking a
        // whole painting, inventing loot, or disabling the typography gate.
        void FitCommittedMilestoneReceipt()
        {
            if(prizeCount!=0 || currentStage==DailyTreasureStageKind.Daily ||
               playing || !ceremony.gameObject.activeSelf ||
               !claimButton.gameObject.activeSelf)return;
            var bounds=resultText.rectTransform.rect;
            float allowed=bounds.height+Mathf.Max(6f,bounds.height*.18f);
            resultText.text=ceremonialTitle+"\n"+
                ConfirmedMilestoneSummary(ceremonialReceipt)+
                "\nRegistrado en el buzón";
            if(resultText.preferredHeight>allowed)
                resultText.text=ceremonialTitle+"\nRegistrado en el buzón";
            if(resultText.preferredHeight>allowed)
                resultText.text=ceremonialTitle;
            // If an unusually long authentic title still fails, let the
            // independent native typography assertion report that defect.
        }

        void ShowResult(bool immediate=false)
        {
            playing=false;
            chest.SetOpening(1f);
            rays.SetIntensity(currentStage==DailyTreasureStageKind.Daily?
                (prizeCount>0 && !chestReward?.24f:.12f):0f);
            ambient.color=new Color(activeTheme.R,activeTheme.G,
                activeTheme.B,activeTheme.Strength*.28f);
            if(hasAuthoredChestArt)
            {
                illustratedClosed.color=new Color(1f,1f,1f,0f);
                illustratedOpen.color=Color.white;
                illustratedClosed.rectTransform.localScale=Vector3.one;
                illustratedOpen.rectTransform.localScale=Vector3.one;
            }
            if(prizeCount>0)
            {
                // The illustrated cards ARE the reward reveal; avoid putting
                // a wall of small text beneath an otherwise cinematic chest.
                // Landscape leaves only a small gap between the original
                // full-size illustrated chest and the illustrated prize row.
                // A forced two-line receipt there was vertically truncated.
                // The full claim details live in the mailbox and icon cards.
                bool portrait=root.rect.height>root.rect.width;
                resultText.text=portrait?
                    ceremonialTitle+"\nGuardado en tu inventario y buzón":
                    ceremonialTitle+" · GUARDADO";
                prizeShelf.gameObject.SetActive(true);
                prizeAnimating=!immediate;
                prizeRevealStarted=Time.unscaledTime;
                for(int i=0;i<prizeCount;i++)
                {
                    prizeGroups[i].alpha=immediate?1f:0f;
                    prizeRects[i].localScale=Vector3.one*(immediate?1f:.52f);
                    prizeRects[i].localRotation=Quaternion.identity;
                }
            }
            else
            {
                resultText.text=ceremonialTitle+"\n"+
                    ConfirmedMilestoneSummary(ceremonialReceipt)+
                    "\nRegistrado en el buzón";
                prizeAnimating=false;
            }
            claimButton.gameObject.SetActive(true);
            claimButton.interactable=true;
            claimCaption.text="CONTINUAR";
            // Once the cinematic result is visible, recompute only its
            // current composition; ordinary prizes have their own centered
            // illustrated-card layout rather than an empty chest slot.
            Layout();
            skipButton.gameObject.SetActive(false);
            glow.color=new Color(1f,.74f,.34f,
                currentStage==DailyTreasureStageKind.Daily?.27f:.08f);
            for(int i=0;i<sparks.Length;i++)
                sparks[i].color=new Color(1f,.75f,.30f,0f);
        }
        void Layout()
        {
            float w=((RectTransform)transform).rect.width;
            float h=((RectTransform)transform).rect.height;
            if(w<200||h<180)return;
            lastWidth=w;lastHeight=h;
            bool portrait=h>w;
            // Vigilia's landscape utility rail occupies the lower-left
            // 77–165 px band; use the verified free 175–230 px zone instead.
            // In portrait the tribute stays just above the five-medallion dock.
            // Vigilia's world objective and utility rail must remain readable.
            // The treasury medallion lives just above the dock in portrait,
            // and to the right of the objective in landscape.
            float entryX=onVigiliaCanvas&&!portrait?256f:14f;
            float entryY=onVigiliaCanvas?(portrait?103f:168f):
                (portrait?100f:83f);
            Place(entryRect,0f,0f,0f,0f,entryX,entryY,80f,72f);
            Place(entryGlow.rectTransform,.5f,.5f,.5f,.5f,
                0f,10f,83f,73f);
            Place(entryArtwork.rectTransform,.5f,.5f,.5f,.5f,
                0f,10f,68f,58f);
            Place(entryNotice.rectTransform,1f,1f,.5f,.5f,
                -1f,-2f,24f,24f);
            Place(entryCaption.rectTransform,.5f,0f,.5f,0f,
                0f,3f,72f,18f);
            // The approved authored chest looked small inside a 740x370
            // rectangle. Use a purposeful, responsive theatrical composition
            // while the chest opens, then a slightly wider compact receipt
            // that spends its area on REAL paid items instead of dead space.
            // Non-daily event stages retain their independent authored layout.
            bool openingChest=ceremony && ceremony.gameObject.activeSelf &&
                playing && chestReward && prizeCount>0 &&
                currentStage==DailyTreasureStageKind.Daily;
            bool paidChest=ceremony && ceremony.gameObject.activeSelf &&
                !playing && chestReward && prizeCount>0 &&
                currentStage==DailyTreasureStageKind.Daily;
            // An everyday 60-wood grant should not occupy the same vast
            // 740px parchment as a legendary four-item reveal. Give one or
            // two REAL paid item cards an intimate framed space, sized to
            // their content, with an actual 44px+ Continue control.
            bool focusedQuiet=ceremony && ceremony.gameObject.activeSelf &&
                prizeCount>0 && !chestReward;
            float quietWidth=portrait?
                (prizeCount<=1?336f:410f):
                (prizeCount<=1?340f:448f);
            float panelW=Mathf.Min(w-24f,focusedQuiet?quietWidth:
                portrait?440f:openingChest||paidChest?650f:740f);
            bool eventStage=currentStage!=DailyTreasureStageKind.Daily &&
                ceremony && ceremony.gameObject.activeSelf &&
                prizeCount==0 && w>=275f && h>=300f;
            var stage=eventStage?DailyTreasureStageDirection.Compose(
                activeMilestoneId,w,h):default(DailyTreasureStageLayout);
            var earnedTheatre=eventStage?
                DailyTreasureStageDirection.NarrativeTheatreFrame(w,h):
                default(DailyTreasureNarrativeFrame);
            if(eventStage)panelW=earnedTheatre.Width;
            // Orientation is also re-evaluated by the SAME native Editor
            // Layout() probe. A stale portrait source must never stand in
            // for the newly rotated view of the player's actual Bastion.
            if(eventStage && currentStage==DailyTreasureStageKind.Bastion)
                RefreshBastionLensForRotation(w,h);
            // Ordinary resources deserve the same illustrated materials but
            // not a giant EMPTY modal. An earned-item reveal uses a distinct,
            // compact luxury frame. The ceremonial coffer remains immersive.
            // Rotation can happen after an already-paid reward is revealed.
            // Update its caption to the CURRENT orientation without touching
            // the gateway, replaying the ceremony or losing the earned cards.
            if(prizeCount>0 && ceremony.gameObject.activeSelf && !playing &&
               claimButton.gameObject.activeSelf)
                resultText.text=portrait?
                    ceremonialTitle+"\nGuardado en tu inventario y buzón":
                    ceremonialTitle+" · GUARDADO";
            // Expand only the actual calendar where the viewport permits.
            // This gives seven real rewards a calmer hierarchy on desktops
            // and tablets, while the 390px landscape stays compact.
            bool roomyCalendar=ceremony && !ceremony.gameObject.activeSelf &&
                !portrait && h>=500f;
            float panelH=Mathf.Min(h-20f,portrait?
                (focusedQuiet?430f:openingChest?450f:700f):
                (focusedQuiet?330f:openingChest?300f:
                    roomyCalendar?420f:370f));
            if(eventStage)panelH=earnedTheatre.Height;
            // Real story art has grown 2.62x on desktop; retaining 17px
            // labels in a 1120px picture would feel cheap. Scale ONLY the
            // narrative title, official insignia and confirmed story text.
            // Daily buttons/rewards and all small phones remain unchanged.
            float narrativeScale=eventStage?Mathf.Clamp(
                Mathf.Min(panelW/(portrait?440f:740f),
                          panelH/(portrait?700f:370f)),
                1f,1.5f):1f;
            Place(cardSurface,.5f,.5f,.5f,.5f,0,0,panelW,panelH);
            bool liveBastionWorldThrough=eventStage &&
                currentStage==DailyTreasureStageKind.Bastion &&
                !HasMilestoneScenery;
            if(screenShade)screenShade.color=new Color(.018f,.020f,.026f,
                liveBastionWorldThrough?
                    DailyTreasureStageDirection.BastionWorldThroughBackdropOpacity:
                    eventStage?stage.BackdropOpacity:CalendarBackdropOpacity);
            bool singleFrame=eventStage && !portrait &&
                milestoneOrnament && milestoneOrnament.sprite;
            if(milestoneOrnament)
                milestoneOrnament.gameObject.SetActive(singleFrame);
            if(mainFrameSurface)
            {
                mainFrameSurface.sprite=singleFrame?null:dailyFrameSprite;
                mainFrameSurface.type=singleFrame?Image.Type.Simple:
                    dailyFrameSprite?Image.Type.Sliced:Image.Type.Simple;
                float bOpacity=liveBastionWorldThrough?
                    DailyTreasureStageDirection.BastionWorldThroughPlateOpacity:
                    DailyTreasureStageDirection.BastionPlateOpacity;
                if(singleFrame)
                    mainFrameSurface.color=new Color(Stone.r,Stone.g,Stone.b,
                        currentStage==DailyTreasureStageKind.Bastion?
                            bOpacity:.96f);
                else if(mainFrameSurface.sprite)
                    mainFrameSurface.color=eventStage &&
                        currentStage==DailyTreasureStageKind.Bastion?
                            new Color(1f,1f,1f,bOpacity):Color.white;
                else mainFrameSurface.color=Stone;
            }
            if(eventStage)
            {
                Place(milestoneScenery.rectTransform,.5f,.5f,.5f,.5f,
                    stage.SceneX,stage.SceneY,stage.SceneW,stage.SceneH);
                Place(milestoneVignette.rectTransform,.5f,.5f,.5f,.5f,
                    stage.SceneX,stage.SceneY,stage.SceneW,stage.SceneH);
                milestoneScenery.color=MilestonePaintTint(stage);
                milestoneVignette.color=new Color(.024f,.023f,.026f,
                    stage.SceneVeilOpacity);
                if(milestoneStageArt && milestoneStageArt.gameObject.activeSelf)
                {
                    // Painterly foreground is transparent and cannot invent
                    // a different Lyra, reward, or future Bastion city.
                    // Genuine Unity screenshot 43005395878 proved the
                    // first generic 1.72x/1.78x foregrounds obscured their
                    // painterly scenes and even covered receipt/Continue.
                    // Do not compensate with a tint: each DISTINCT physical
                    // subject must fit its actual scenic column. The Bastion
                    // arch belongs at the edge; the REAL BUILDING is the hero.
                    float artX=stage.SceneX,artY=stage.SceneY;
                    float artW=stage.SceneW,artH=stage.SceneH;
                    switch(currentStage)
                    {
                        case DailyTreasureStageKind.Explorers:
                            // Original 1376x768 RGBA: the unfilled parchment
                            // itself occupies ~70% of the source width. At
                            // .96x it concealed most of the REAL surveyed
                            // country in Unity 43005395878. Compose a smaller
                            // movable document to the RIGHT of the actual
                            // background, leaving geographic evidence at left
                            // and open sky ABOVE. Do not paint invented roads,
                            // settlements or reward tokens onto the scroll.
                            artW*=portrait?.72f:.78f;
                            artH*=portrait?.72f:.84f;
                            artX+=stage.SceneW*(portrait?.13f:.11f);
                            artY-=stage.SceneH*(portrait?.10f:.07f);
                            break;
                        case DailyTreasureStageKind.Vigilants:
                            // Original shield is only ~38% of its transparent
                            // source canvas; a .96x rectangle made the true
                            // forged metal tiny beside a 128px flat blue
                            // insignia. Enlarge the actual painted SHIELD (not
                            // a filtered/upscaled emblem), keeping its real
                            // visible central silhouette inside the scenic
                            // column even if alpha margins extend outside.
                            artW*=portrait?1.40f:1.45f;
                            // Source PNG's actual nontransparent bounds span
                            // 29..733 / 768 in Y. In tall UI the previous
                            // 1.03x shield sat under "CAPÍTULO SUPERADO" and
                            // also reached the confirmed mailbox report.
                            // Reserve ONE central heraldic zone BETWEEN
                            // title and receipt. No clipping or icon shrink.
                            artH*=portrait?.83f:1.06f;
                            artY-=portrait?stage.SceneH*.205f:0f;
                            break;
                        case DailyTreasureStageKind.Lyra:
                            artW*=portrait?.92f:.98f;
                            artH*=portrait?.83f:.94f;
                            break;
                        case DailyTreasureStageKind.Bastion:
                            // The central city is real gameplay geometry.
                            // Move the original alpha stonework to the EDGE
                            // as a slender architectural side ornament.
                            // The verified 43005395878 true-city A/B showed
                            // the enormous arch pasted OVER the actual
                            // building. Make it a SIDE DETAIL instead:
                            // reserve the central 38% of this scene for the
                            // real Bastion, in both orientations. The narrow
                            // ornament stays inside the panel (-.49..-.19w
                            // portrait, +.19..+.49w landscape) and retains
                            // its own fully original painted texture.
                            artW*=.30f;
                            artH*=portrait?.59f:.58f;
                            artX+=portrait?-stage.SceneW*.34f:
                                stage.SceneW*.34f;
                            artY+=stage.SceneH*.09f;
                            break;
                    }
                    if(currentStage==DailyTreasureStageKind.Explorers)
                    {
                        // Unity Image.preserveAspect letterboxes VERTICES
                        // but children retain the full container dimensions.
                        // The true committed Scout vignette is a CHILD of
                        // the original parchment: contain its parent to the
                        // exact sprite aspect BEFORE anchoring the ink.
                        // This fixes a 2x-tall sticker at 390x844 and a
                        // 37%-tall sticker at 844x390, unlike tint changes.
                        var original=milestoneStageArt.sprite.rect;
                        var image=DailyTreasureStageDirection.ContainOriginalArtwork(
                            artW,artH,original.width,original.height);
                        artW=image.Width;
                        artH=image.Height;
                    }
                    Place(milestoneStageArt.rectTransform,
                        .5f,.5f,.5f,.5f,artX,artY,artW,artH);
                    milestoneStageRest=milestoneStageArt.rectTransform.anchoredPosition;
                    milestoneStageRestOpacity=
                        currentStage==DailyTreasureStageKind.Bastion?.68f:.96f;
                    milestoneStageArt.color=new Color(1f,1f,1f,
                        milestoneStageRestOpacity);
                    milestoneStageArt.rectTransform.localScale=Vector3.one;
                }
                // The authoritative hero portrait is the focal subject of
                // Lyra's ceremony, with actual Eldoria hall as the depth layer.
                // Portrait crops are independent of text/hitboxes. Keep its
                // top edge below the native heading on both orientations.
                if(milestoneLyraPortrait.gameObject.activeSelf)
                {
                    float portraitWidth=stage.SideBySide?
                        Mathf.Min(stage.SceneW*.68f,245f):
                        Mathf.Min(stage.SceneW*.64f,245f);
                    float portraitHeight=stage.SideBySide?
                        Mathf.Min(stage.SceneH*.79f,255f):
                        Mathf.Min(stage.SceneH*.68f,267f);
                    float portraitX=stage.SceneX+
                        (stage.SideBySide?-stage.SceneW*.06f:0f);
                    float portraitY=stage.SceneY-
                        (stage.SideBySide?stage.SceneH*.12f:
                            stage.SceneH*.23f);
                    Place(milestoneLyraViewport,.5f,.5f,.5f,.5f,
                        portraitX,portraitY,portraitWidth,portraitHeight);
                    // Use the source's real aspect to distinguish tall GLB
                    // full-body portraits from already-tight Heroes UI busts.
                    // The tall canonical texture has empty space above Lyra:
                    // enlarge behind a RectMask2D and move the WHOLE image
                    // down so the face becomes the emotional focal point.
                    // No Texture2D duplication, Photoshop resize, new person,
                    // unbounded frame allocations or pixel re-painting.
                    var sourcePortrait=milestoneLyraPortrait.sprite;
                    bool fullBody=sourcePortrait &&
                        sourcePortrait.rect.height>=700f &&
                        sourcePortrait.rect.height/
                            Mathf.Max(1f,sourcePortrait.rect.width)>=1.2f;
                    bool originalIllustration=sourcePortrait &&
                        sourcePortrait.name=="canonical_lyra_illustration";
                    if(originalIllustration)
                    {
                        // This source has almost no empty space above the
                        // hair. Align its head within the aperture instead
                        // of reusing the old GLB render's empty-top crop.
                        float imageHeight=portraitHeight*1.85f;
                        float imageWidth=imageHeight*sourcePortrait.rect.width/
                            Mathf.Max(1f,sourcePortrait.rect.height);
                        Place(milestoneLyraPortrait.rectTransform,
                            .5f,.5f,.5f,.5f,0f,
                            portraitHeight*.46f-imageHeight*.5f,
                            imageWidth,imageHeight);
                    }
                    else if(fullBody)
                    {
                        Place(milestoneLyraPortrait.rectTransform,
                            .5f,.5f,.5f,.5f,0f,
                            -portraitHeight*.22f,
                            portraitWidth*1.60f,portraitHeight*2.05f);
                    }
                    else
                        Stretch(milestoneLyraPortrait.rectTransform);
                    milestoneLyraPortrait.color=new Color(
                        1f,.97f,.90f,.96f);
                }
                if(milestoneInsignia.gameObject.activeSelf)
                {
                    // Place the ACTUAL Vigiliants crest in the empty shield,
                    // instead of floating a second icon below the painting.
                    bool centerShield=currentStage==DailyTreasureStageKind.Vigilants &&
                        milestoneStageArt && milestoneStageArt.sprite;
                    // Official 128px crest is still blue, not bronze:
                    // show it as a smaller heraldic inset on the real
                    // large forged shield, never an enlarged flat sticker.
                    float insigniaSize=(centerShield?(portrait?64f:78f):
                        portrait?57f:65f)*narrativeScale;
                    Place(milestoneInsignia.rectTransform,.5f,.5f,.5f,.5f,
                        centerShield?stage.SceneX:stage.SceneX+stage.SceneW*.33f,
                        centerShield?stage.SceneY:stage.SceneY-stage.SceneH*.31f,
                        insigniaSize,insigniaSize);
                    milestoneInsignia.color=new Color(1f,.95f,.87f,.87f);
                }
                // Portrait has enough width for a readable literary heading,
                // but the top-right 100x45 SKIP control owns the upper band.
                // Put the story heading BELOW that hitbox rather than layering
                // another ornate button on the same location.
                // The Bastion milestone uses the ACTUAL built city; there is
                // no fictional chest or painted substitute for its level.
                // Other milestones put the game's authentic scenic painting
                // at the centre, never behind an unrelated daily coffer.
                bool cityStage=currentStage==DailyTreasureStageKind.Bastion;
                float headingWidth=cityStage?
                    Mathf.Min(panelW-132f,portrait?230f:300f):
                    portrait?Mathf.Min(panelW-116f,280f):
                    Mathf.Min(panelW*.58f,330f);
                float headingX=portrait?
                    (cityStage?-panelW*.5f+10f+headingWidth*.5f:
                     -Mathf.Min(26f,(panelW-headingWidth)*.20f)):
                    cityStage?-panelW*.5f+12f+headingWidth*.5f:
                    stage.SideBySide?stage.SceneX:-panelW*.10f;
                float headingY=cityStage?panelH*.405f:
                    portrait?panelH*.29f:panelH*.405f;
                Place(milestoneBanner,.5f,.5f,.5f,.5f,
                    headingX,headingY,headingWidth,
                    Mathf.Min(56f,40f*narrativeScale));
                Place(milestoneRule.rectTransform,.5f,0f,.5f,0f,
                    0f,2f,headingWidth-30f,2f);
                milestoneBannerCaption.fontSize=cityStage?
                    (portrait?17:16):portrait?18:19;
                milestoneBannerCaption.fontSize=Mathf.RoundToInt(
                    milestoneBannerCaption.fontSize*narrativeScale);
            }
            else
            {
                Place(milestoneScenery.rectTransform,.5f,.5f,.5f,.5f,
                    0f,0f,panelW-25f,panelH-28f);
                Place(milestoneVignette.rectTransform,.5f,.5f,.5f,.5f,
                    0f,0f,panelW-24f,panelH-26f);
            }
            // Preserve the Bastion scene, while protecting the actual
            // recorded story with a discrete unframed shadowed ink area.
            bool realCity=eventStage && currentStage==DailyTreasureStageKind.Bastion;
            // A previous 390x844 native shot devoted half the frame to
            // near-black emptiness. Keep REAL world pigments alive throughout
            // the stage and put contrast only behind factual receipt text.
            bastionStoryInk.gameObject.SetActive(eventStage);
            if(eventStage)
            {
                bastionStoryInk.color=new Color(.045f,.049f,.052f,
                    realCity?.82f:.67f);
                Place(bastionStoryInk.rectTransform,.5f,.5f,.5f,.5f,
                    stage.ReceiptX,stage.ReceiptY,
                    stage.ReceiptW+16f,stage.ReceiptH+12f);
            }
            UpdateSceneryCrop();
            // A real 390px portrait calendar asked the 27px title to
            // wrap to two 31px lines inside a 40px box. It also approached
            // Vigilia's close affordance on 900px tablets. Keep this title
            // in its OWN left-clear band and measure the actual imported
            // font in Unity; never truncate one of the two words or let
            // the title paint beneath the native close button.
            float safeHeadingWidth=Mathf.Max(180f,panelW-124f);
            Place(header.rectTransform,.5f,1f,.5f,1f,
                -28f,-10f,safeHeadingWidth,40f);
            header.fontSize=27;
            while(header.fontSize>17 &&
                  header.preferredWidth>safeHeadingWidth-8f)
                header.fontSize--;
            Place(subhead.rectTransform,.5f,1f,.5f,1f,0,-49f,panelW-44f,26f);
            Place((RectTransform)closeButton.transform,1f,1f,1f,1f,-8f,-8f,48f,48f);
            int cols=portrait?2:4,rows=portrait?4:2;
            float left=15f,gap=7f;
            float gridTop=82f,gridBottom=83f;
            float gridHeight=panelH-gridTop-gridBottom;
            float cellW=(panelW-2f*left-gap*(cols-1))/cols;
            float cellH=(gridHeight-gap*(rows-1))/rows;
            Place(calendar,.5f,.5f,.5f,.5f,0f,
                (-gridTop+gridBottom)*.5f,panelW,gridHeight);
            for(int i=0;i<7;i++)
            {
                int column=i%cols,row=i/cols;
                // Day seven is the ceremonial grand chest and deliberately
                // occupies the unused eighth slot: two columns, not tiny text.
                int span=i==6?2:1;
                float width=cellW*span+gap*(span-1);
                float x=-panelW*.5f+left+column*(cellW+gap)+width*.5f;
                float y=gridHeight*.5f-row*(cellH+gap)-cellH*.5f;
                Place(dayBack[i].rectTransform,.5f,.5f,.5f,.5f,x,y,width,cellH);
                bool roomy=cellH>=123.5f;
                Place(dayHead[i].rectTransform,.5f,1f,.5f,1f,0f,-5f,width-6f,25f);
                dayHead[i].fontSize=roomy?18:17;
                if(i==6)
                {
                    Place(dayGlyph[i].rectTransform,.5f,.5f,.5f,.5f,
                        -width*.5f+48f,-11f,
                        roomy?80f:73f,roomy?72f:57f);
                    if(dayArtwork[i])Place(dayArtwork[i].rectTransform,
                        .5f,.5f,.5f,.5f,-width*.5f+48f,-11f,
                        roomy?95f:85f,roomy?78f:65f);
                    if(rareDayGlow)Place(rareDayGlow.rectTransform,
                        .5f,.5f,.5f,.5f,-width*.5f+48f,-11f,
                        roomy?132f:122f,roomy?105f:96f);
                    Place(dayBody[i].rectTransform,.5f,.5f,.5f,.5f,
                        38f,-12f,width-116f,Mathf.Max(52f,cellH-39f));
                    // At 667x375 the double-width day-seven card is
                    // narrower; preserve two full prize lines without
                    // forcing unwanted wrap onto a third, clipped line.
                    dayBody[i].fontSize=!portrait && width<318f?13:
                        roomy?16:15;
                }
                else
                {
                    // Short 667x375 landscapes still need separate text/icon
                    // space; make small, bounded adjustments rather than shrink
                    // the entire Canvas or blur all text by scaling transforms.
                    bool tiny=cellH<96f;
                    // Real Unity 1600x900 screenshots showed Day 4 and Day 6
                    // two-line labels painted onto the bottom bronze border.
                    // Move both lines INSIDE the card rather than hiding
                    // their second resource, and reduce only the secondary
                    // icon when vertical space is genuinely constrained.
                    // The third-day loyalty bonus adds FOOD to the ordinary
                    // one-minute speedup; the calendar then has two entries
                    // even though it is not day four or six. Use the actual
                    // credited/previewed text to select safe two-line geometry.
                    bool twoResources=dayBody[i].text.IndexOf('\n')>=0;
                    bool compactDouble=twoResources && cellH<=103f;
                    if(tiny)Place(dayHead[i].rectTransform,.5f,1f,.5f,1f,
                        0f,-5f,cellW-6f,24f);
                    float iconY=roomy?10f:
                        compactDouble?3f:twoResources?4f:tiny?2f:3f;
                    Place(dayGlyph[i].rectTransform,.5f,.5f,.5f,.5f,
                        0f,iconY,roomy?56f:48f,
                        roomy?38f:compactDouble?23f:tiny?24f:28f);
                    if(dayArtwork[i])Place(dayArtwork[i].rectTransform,
                        .5f,.5f,.5f,.5f,0f,iconY,
                        roomy?65f:compactDouble?48f:tiny?51f:58f,
                        roomy?40f:compactDouble?23f:tiny?24f:28f);
                    float labelBottom=compactDouble?20f:
                        twoResources?27f:tiny?17f:18.5f;
                    float labelHeight=compactDouble?30f:
                        twoResources?36f:tiny?31f:33f;
                    Place(dayBody[i].rectTransform,.5f,.5f,.5f,.5f,
                        0f,-cellH*.5f+labelBottom,
                        cellW-10f,labelHeight);
                    dayBody[i].fontSize=compactDouble?12:
                        roomy?(twoResources?15:16):tiny?13:14;
                }
            }
            if(eventStage && currentStage==DailyTreasureStageKind.Bastion &&
               !portrait)
            {
                // Keep both the real built city AND its entrance in view.
                // The narrative and Continue occupy the left ink column.
                Place((RectTransform)claimButton.transform,
                    .5f,0f,.5f,0f,stage.ReceiptX,13f,
                    Mathf.Min(stage.ReceiptW,320f),54f);
            }
            else if(eventStage && stage.SideBySide)
            {
                // Native Scouts/Explorers 844x390: the centered 405px
                // CONTINUAR button intruded into the authentic painting.
                // Keep the single real action in the RIGHT-hand receipt
                // column; no opaque strip across the original map.
                Place((RectTransform)claimButton.transform,.5f,0f,.5f,0f,
                    stage.ReceiptX,13f,Mathf.Min(stage.ReceiptW,320f),54f);
            }
            else if(focusedQuiet)
                Place((RectTransform)claimButton.transform,.5f,.5f,.5f,.5f,
                    0f,portrait?-175f:-127f,
                    Mathf.Min(panelW-30f,portrait?325f:405f),54f);
            else
                Place((RectTransform)claimButton.transform,.5f,0f,.5f,0f,
                    0f,13f,Mathf.Min(panelW-30f,portrait?325f:405f),54f);
            Stretch(ceremony);
            // The ceremony now has its own top half; do not shrink the
            // original textured chest to a postage stamp on landscape phones.
            // The opening is a distinct, intimate theatre: a visibly larger
            // chest, a caption beneath it and no giant empty lower panel.
            // On completion the chest returns to the upper focal position
            // while the *actual paid* cards fill the newly available space.
            // Short 667x375 landscapes have only 355px of usable panel
            // height; keep a real 10px+ safety margin above the chest even
            // at the peak 3.5% animation scale, instead of clipping its lid.
            bool shortPaidLandscape=!portrait && h-20f<365f;
            float chestY=openingChest?(portrait?64f:42f):
                focusedQuiet?(portrait?64f:35f):
                prizeCount>0?(portrait?116f:shortPaidLandscape?84f:89f):
                (portrait?105f:87f);
            float artWidth=openingChest?(portrait?330f:330f):
                prizeCount>0?(portrait?284f:shortPaidLandscape?290f:300f):
                (portrait?294f:282f);
            float artHeight=openingChest?(portrait?265f:194f):
                prizeCount>0?(portrait?246f:shortPaidLandscape?164f:170f):
                (portrait?265f:175f);
            float chestX=eventStage?stage.ChestX:0f;
            if(eventStage)
            {
                chestY=stage.ChestY;
                artWidth=stage.ChestW;
                artHeight=stage.ChestH;
            }
            Place(glowRect,.5f,.5f,.5f,.5f,chestX,chestY,
                portrait?320f:335f,portrait?320f:335f);
            Place(ambientRect,.5f,.5f,.5f,.5f,chestX,chestY,
                portrait?385f:390f,portrait?385f:390f);
            Place(raysRect,.5f,.5f,.5f,.5f,chestX,chestY,
                portrait?330f:315f,portrait?330f:315f);
            Place(chest.rectTransform,.5f,.5f,.5f,.5f,chestX,chestY,
                prizeCount>0?artWidth*.88f:
                    eventStage?artWidth*.88f:(portrait?195f:165f),
                prizeCount>0?artHeight*.88f:
                    eventStage?artHeight*.88f:(portrait?150f:125f));
            // Full-cell transparent source sprites preserve the model's scale,
            // and do not stretch the illustrated ironwork on odd phone ratios.
            Place(illustratedClosed.rectTransform,.5f,.5f,.5f,.5f,
                chestX,chestY,artWidth,artHeight);
            Place(illustratedOpen.rectTransform,.5f,.5f,.5f,.5f,
                chestX,chestY,artWidth,artHeight);
            bool compactPrizePortrait=portrait && prizeCount>0 && panelH<680f;
            bool quietRewards=prizeCount>0 && !chestReward;
            // A very short portrait viewport cannot fit two receipt lines
            // BETWEEN the two-row earned-card shelf and the Continue button.
            // Keep the authentic full receipt in the mailbox and show a
            // single-line confirmation here, without changing the award.
            if(compactPrizePortrait && !playing &&
               ceremony.gameObject.activeSelf && claimButton.gameObject.activeSelf)
                resultText.text=ceremonialTitle+" · GUARDADO";
            if(prizeCount>0)
            {
                resultText.fontSize=openingChest?(portrait?20:18):
                    quietRewards?(portrait?19:17):
                    compactPrizePortrait?16:portrait?18:16;
                float captionY=openingChest?(portrait?-132f:-100f):
                    quietRewards?(portrait?-106f:-74f):
                    compactPrizePortrait?-217f:portrait?-232f:-15f;
                // Genuine Unity portrait receipt needed 66px for its two
                // actual lines (run 42776890262). The former 41px crop
                // hid the saved-inventory confirmation. At 72px its lower
                // edge is -142, 6px ABOVE the 54px Continue button top
                // (-148), while the prize shelf remains 46.5px above.
                // The real day-seven portrait rotation (run 42778827380)
                // needs 63px for its two-line caption; the previous 50px box
                // clipped it. 72px fits: at panelH=700, the bottom is -268,
                // 15px above Continue's top (-283), and its top (-196)
                // clears the two-row item shelf bottom (-194) by 2px.
                // Shorter portrait screens use a single-line 39px caption.
                // Preserve the landscape one-line 29px presentation.
                Place(resultText.rectTransform,.5f,.5f,.5f,.5f,
                    0f,captionY,panelW-45f,
                    openingChest?(portrait?45f:32f):
                    quietRewards?(portrait?72f:41f):
                    compactPrizePortrait?39f:portrait?72f:29f);
            }
            else
            {
                // Real chapter/hero/kingdom milestone: one large chest, then
                // the confirmed story (no made-up crystals or reward cards).
                resultText.fontSize=eventStage?
                    (portrait?19:17):(portrait?20:19);
                if(eventStage)
                    resultText.fontSize=Mathf.RoundToInt(
                        resultText.fontSize*narrativeScale);
                resultText.resizeTextForBestFit=eventStage;
                if(eventStage)
                {
                    // A rare long narrative must adapt within its actual
                    // measured Unity box, not truncate the player's milestone
                    // to preserve an arbitrary font size. Never below 14 px.
                    resultText.resizeTextMinSize=Mathf.RoundToInt(
                        (portrait?16:14)*narrativeScale);
                    resultText.resizeTextMaxSize=Mathf.RoundToInt(
                        (portrait?19:17)*narrativeScale);
                }
                if(eventStage)
                    Place(resultText.rectTransform,.5f,.5f,.5f,.5f,
                        stage.ReceiptX,stage.ReceiptY,
                        stage.ReceiptW,stage.ReceiptH);
                else
                    Place(resultText.rectTransform,.5f,.5f,.5f,.5f,0,
                        portrait?-148f:-71f,panelW-45f,portrait?141f:66f);
            }

            if(eventStage)FitCommittedMilestoneReceipt();

            // Four day-seven rewards need at least 65px per item, even
            // on small portrait screens. Raise the two-row shelf rather
            // than shrinking illustrated icons and text to unreadable sizes.
            float shelfHeight=quietRewards?(portrait?175f:137f):
                portrait?158f:76f;
            float shelfY=quietRewards?(portrait?64f:35f):
                portrait?-115f:-69f;
            Place(prizeShelf,.5f,.5f,.5f,.5f,0f,shelfY,
                panelW-34f,shelfHeight);
            int prizeCols=portrait && prizeCount>=3?2:Mathf.Max(1,prizeCount);
            int prizeRows=Mathf.Max(1,
                Mathf.CeilToInt(prizeCount/(float)prizeCols));
            float prizeGap=portrait?8f:6f;
            // Larger real receipts: two rewards can use spacious 190px
            // cards; four rewards use a balanced 2x2 portrait or nearly
            // full-width landscape row. Labels have room to render at >10px.
            float maxPaidCardW=prizeCount<=2?190f:portrait?170f:146f;
            float cardW=Mathf.Min(quietRewards?175f:maxPaidCardW,
                (panelW-38f-(prizeCols-1)*prizeGap)/prizeCols);
            float cardH=quietRewards?(portrait?129f:115f):
                prizeRows==2?72f:
                portrait?(compactPrizePortrait?82f:92f):72f;
            // Source of the animated cards is the visual chest opening; this
            // position is in shelf-local coordinates and follows layout size.
            prizeFlightOrigin=new Vector2(0f,
                chestY-shelfY-artHeight*.10f);
            for(int i=0;i<prizeRects.Length;i++)
            {
                int r=i/prizeCols,c=i%prizeCols;
                int itemsInRow=Mathf.Min(prizeCols,
                    Mathf.Max(0,prizeCount-r*prizeCols));
                float rowW=itemsInRow*cardW+
                    Mathf.Max(0,itemsInRow-1)*prizeGap;
                float x=-rowW*.5f+c*(cardW+prizeGap)+cardW*.5f;
                float y=prizeRows==1?0f:
                    r==0?cardH*.5f+prizeGap*.5f:
                         -cardH*.5f-prizeGap*.5f;
                Place(prizeRects[i],.5f,.5f,.5f,.5f,x,y,cardW,cardH);
                prizeHomes[i]=new Vector2(x,y);
                // Actual Unity captures exposed a serious presentation
                // failure: the 62px landscape cards rendered their icons but
                // clipped BOTH resource names and quantities. Their old
                // 10-16px label rectangles were shorter than font ascenders
                // and overlapped each other. No reward may appear blank.
                // Taller portrait cards retain a vertical composition;
                // short cards place the icon left and an independent two-line
                // receipt column right. Always allocate >=1.4x font height.
                bool tall=cardH>=110f;
                if(tall)
                {
                    Place(prizeIcons[i].rectTransform,.5f,.5f,.5f,.5f,
                        0f,cardH*.26f,53f,48f);
                    Place(prizeNames[i].rectTransform,.5f,.5f,.5f,.5f,
                        0f,-cardH*.08f,cardW-15f,24f);
                    Place(prizeAmounts[i].rectTransform,.5f,.5f,.5f,.5f,
                        0f,-cardH*.35f,cardW-15f,28f);
                    prizeNames[i].fontSize=12;
                    prizeAmounts[i].fontSize=20;
                }
                else
                {
                    float labelWidth=cardW-62f;
                    float labelX=cardW*.5f-labelWidth*.5f-7f;
                    Place(prizeIcons[i].rectTransform,.5f,.5f,.5f,.5f,
                        -cardW*.5f+27f,0f,43f,43f);
                    Place(prizeNames[i].rectTransform,.5f,.5f,.5f,.5f,
                        labelX,12f,labelWidth,27f);
                    Place(prizeAmounts[i].rectTransform,.5f,.5f,.5f,.5f,
                        labelX,-16f,labelWidth,26f);
                    prizeNames[i].fontSize=cardW>=170f?12:11;
                    prizeAmounts[i].fontSize=18;
                }
            }
            // Skip stays away from the receipt and is a genuine >=44px hit target.
            if(eventStage && stage.SideBySide)
                Place((RectTransform)skipButton.transform,
                    .5f,1f,.5f,1f,stage.SceneX,-66f,100f,45f);
            else
                Place((RectTransform)skipButton.transform,
                    1f,1f,1f,1f,-10f,-60f,100f,45f);
        }
        // PR173's approved Vigilia runs in a separate ScreenSpaceCamera
        // Canvas and deliberately disables the old Royal HUD's Canvas.
        // The treasure panel must therefore MOVE to its real safe-area
        // composition instead of remaining invisibly attached to legacy.
        // No direct PR173 assembly dependency, no second Canvas/GraphicRaycaster,
        // no duplicate gameplay gateway and no changes to PR173 itself.
        // Unity 6 treats '/' inside GameObject.Find as a hierarchy path.
        // Vigilia deliberately has '/' in its LITERAL root name, so Find
        // would return null and leave the coffer stranded on a disabled HUD.
        // Canvases are few; query them once per second, without taking a
        // dependency on PR173's assembly or mutating the other workstream.
        static Canvas FindLiteralVigiliaCanvas()
        {
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsInactive.Exclude,FindObjectsSortMode.None);
            foreach(var candidate in canvases)
                if(candidate && candidate.isActiveAndEnabled &&
                   candidate.gameObject.name=="VIGILIA / independent canvas")
                    return candidate;
            return null;
        }

        // The canonical Heroes room also contains a literal '/' in its
        // name and starts inactive. Search under the existing safe area,
        // caching its RectTransform once constructed; do not traverse the
        // whole world hierarchy every frame or open over real Lyra artwork.
        bool HasVisibleCanonicalHeroRoom()
        {
            if(!cachedCanonicalHeroRoom && originalSafeArea)
            {
                // Direct-child lookup mirrors the real HeroesApprovedDestination
                // parent contract. No recursive O(all HUD descendants) traversal,
                // managed buffer or allocation on a 60fps Android Update.
                for(int i=0;i<originalSafeArea.childCount;i++)
                {
                    var candidate=originalSafeArea.GetChild(i) as RectTransform;
                    if(candidate && candidate.gameObject.name==
                        "Heroes / canonical destination")
                    {
                        cachedCanonicalHeroRoom=candidate;
                        break;
                    }
                }
            }
            if(!cachedCanonicalHeroRoom ||
               !cachedCanonicalHeroRoom.gameObject.activeInHierarchy)
                return false;
            var parentCanvas=cachedCanonicalHeroRoom.GetComponentInParent<Canvas>();
            return parentCanvas && parentCanvas.isActiveAndEnabled;
        }

        // Vigilia owns the real selected-building cartouche on its
        // independent ScreenSpaceCamera safe area. The optional treasure
        // entry must yield while players examine or improve a building.
        // Search a direct named child only when migrated: no scene traversal,
        // duplicate Canvas, PR173 assembly reference, or synthetic artwork.
        bool HasVisibleVigiliaBuildingPanel()
        {
            if(!onVigiliaCanvas || !root || !root.parent)return false;
            if(!cachedVigiliaBuildingPanel ||
               cachedVigiliaBuildingPanel.parent!=root.parent)
                cachedVigiliaBuildingPanel=root.parent.Find(
                    "Independent selected-building cartouche") as RectTransform;
            if(!cachedVigiliaBuildingPanel ||
               !cachedVigiliaBuildingPanel.gameObject.activeInHierarchy)
                return false;
            var canvas=cachedVigiliaBuildingPanel.GetComponentInParent<Canvas>();
            return canvas && canvas.isActiveAndEnabled;
        }

        void SynchronizePresentationCanvas()
        {
            if(Time.unscaledTime<nextCanvasProbe)return;
            nextCanvasProbe=Time.unscaledTime+1f;

            var canvas=FindLiteralVigiliaCanvas();
            var safe=canvas?
                canvas.transform.Find("Safe composition") as RectTransform:null;
            if(safe)
            {
                if(root.parent!=safe)
                {
                    root.SetParent(safe,false);
                    Stretch(root);
                    root.SetAsLastSibling();
                    onVigiliaCanvas=true;
                    cachedVigiliaBuildingPanel=null;
                    // This canvas replaces the disabled legacy HUD immediately.
                    // Without an immediate geometry pass, the first real tap
                    // can intersect a stale hit target before Update() runs.
                    // Canvas is forced only on this rare parent migration.
                    Canvas.ForceUpdateCanvases();
                    lastWidth=lastHeight=-1f;
                    Layout();
                }
                return;
            }
            // The proof canvas can be disabled without destroying the scene.
            // Restore our original parent instead of leaving controls hidden.
            if(onVigiliaCanvas && originalSafeArea)
            {
                root.SetParent(originalSafeArea,false);
                Stretch(root);
                root.SetAsLastSibling();
                onVigiliaCanvas=false;
                cachedVigiliaBuildingPanel=null;
                Canvas.ForceUpdateCanvases();
                lastWidth=lastHeight=-1f;
                Layout();
            }
        }

#if UNITY_EDITOR
        // Editor-only proof of the actual reversible presentation migration.
        // This is not the production Vigilia canvas or a Unity visual approval.
        public void ProbePresentationCanvasForNativeEvidence()
        {
            nextCanvasProbe=0f;
            SynchronizePresentationCanvas();
        }

        public void ProbeOrientationLayoutForNativeEvidence()
        {
            // Exact same Layout() production method; callable from a native
            // Editor-only orientation test without Unity's player Update().
            lastWidth=lastHeight=-1f;
            Layout();
        }
        public bool ProbeHeroRoomVisibilityForNativeEvidence()
        {
            bool blocked=HasVisibleCanonicalHeroRoom();
            if(blocked && IsOpen)Close();
            if(entryRect)entryRect.gameObject.SetActive(!suppressed &&
                !blocked && !HasVisibleVigiliaBuildingPanel());
            return blocked;
        }
        public bool ProbeVigiliaBuildingForNativeEvidence()
        {
            bool blocked=HasVisibleVigiliaBuildingPanel();
            if(blocked && IsOpen)Close();
            if(entryRect)entryRect.gameObject.SetActive(!suppressed && !blocked);
            return blocked;
        }
#endif

        void TryRecoverBuiltBastionLens()
        {
            if(!bastionCameraPending ||
               currentStage!=DailyTreasureStageKind.Bastion ||
               !IsOpen || Time.unscaledTime<nextBastionCameraTry)return;
            nextBastionCameraTry=Time.unscaledTime+.35f;
            bastionCameraRetries++;
            var original=DailyTreasureBastionCamera.Capture(
                root.rect.width,root.rect.height);
            if(original)
            {
                capturedBastionScene=original;
                milestoneScenery.texture=original;
                milestoneSceneryKey=DailyTreasureBastionCamera.SceneKey;
                milestoneScenery.gameObject.SetActive(true);
                milestoneVignette.gameObject.SetActive(true);
                milestoneScenery.color=Color.white;
                if(milestoneCinematicBackdrop)
                {
                    milestoneCinematicBackdrop.texture=original;
                    milestoneCinematicBackdrop.gameObject.SetActive(
                        ownedCinematicBackdropMaterial!=null);
                }
                // The previous unloaded-city fallback was transparent.
                // Once original camera pixels arrive, restore the actual
                // planned frame, ink and cover UV for this orientation.
                bastionCameraPending=false;
                Layout();
                return;
            }
            if(bastionCameraRetries>=3)
            {
                // On an unbuilt/unloaded city never manufacture a false
                // monument. Its genuine original world remains visible
                // behind the ceremony, not an AI painting or paid coffer.
                bastionCameraPending=false;
                Debug.LogWarning(
                    "BASTION_REAL_LENS_STILL_UNAVAILABLE_AFTER_SCENE_SYNC");
            }
        }

        void Update()
        {
            if(!root)return;
            SynchronizePresentationCanvas();
            // The selected-building context must outrank a reward even if it
            // opened AFTER an already-paid ceremony; close only UI/Foley,
            // never the authoritative item or its mailbox receipt.
            bool vigiliaBuilding=HasVisibleVigiliaBuildingPanel();
            bool canonicalHero=HasVisibleCanonicalHeroRoom();
            bool sceneOwnedByOtherUi=vigiliaBuilding || canonicalHero;
            if(sceneOwnedByOtherUi && IsOpen)Close();
            // A hidden calendar must NOT also hide the sole entry geometry.
            // Layout is required on the very first frame, and again whenever
            // Vigilia's independent safe area or the device orientation moves.
            float currentWidth=root.rect.width,currentHeight=root.rect.height;
            if(Mathf.Abs(currentWidth-lastWidth)>1f ||
               Mathf.Abs(currentHeight-lastHeight)>1f)
                Layout();
            if(!sceneOwnedByOtherUi)TryRecoverBuiltBastionLens();
            if(gateway!=null && Time.unscaledTime>=nextMilestonePoll)
            {
                nextMilestonePoll=Time.unscaledTime+.65f;
                milestoneQueue.Observe(gateway.Snapshot());
                // Never interrupt a tutorial, building selection, paid chest
                // reveal or another room. In particular, Lyra's native
                // HeroesApprovedDestination introduction owns her genuine
                // character geometry; our reward coffer must NOT cover it.
                // First important achievement may receive one ceremonial
                // introduction per session. Additional genuine achievements
                // remain in the mailbox and can be opened on request via the
                // same gold medallion, avoiding autoplay chains after closing.
                if(!sessionAutoCeremonyPlayed)
                    PresentNextCommittedMilestone();
            }
            if(!IsOpen)
            {
                entryRect.gameObject.SetActive(!suppressed &&
                    !sceneOwnedByOtherUi);
                // A restrained, believable coffer glow, not cartoon bouncing.
                float shimmer=entryIsAvailable?
                    (.18f+.095f*Mathf.Sin(Time.unscaledTime*2.1f)):0f;
                entryGlow.color=new Color(1f,.70f,.30f,shimmer);
                entryRect.localScale=Vector3.one*
                    (entryIsAvailable?
                        1f+.008f*Mathf.Sin(Time.unscaledTime*2.1f):1f);
                if(gateway!=null && now!=null && Time.unscaledTime>=nextTick)
                {
                    nextTick=Time.unscaledTime+.8f;
                    entryIsAvailable=DailyTreasureRules.CanClaim(
                        gateway.Snapshot().DailyTreasures,now());
                    entryCaption.color=entryIsAvailable?Gold:Muted;
                    bool pending=milestoneQueue.PendingCount>0;
                    entryCaption.text=entryIsAvailable?"TESORO":
                        pending?"HITOS":"COFRE";
                    entryArtwork.color=entryIsAvailable||pending?Color.white:
                        new Color(.62f,.62f,.59f,.72f);
                    entryNoticeNumber.text=entryIsAvailable?"1":
                        Math.Min(99,milestoneQueue.PendingCount).ToString();
                    entryNotice.gameObject.SetActive(entryIsAvailable||pending);
                }
                return;
            }
            // Any open scene supersedes its own small entry medallion.
            // Restore the medallion only after Close() has finished.
            if(entryRect.gameObject.activeSelf)
                entryRect.gameObject.SetActive(false);
            if(calendar.gameObject.activeSelf && rareDayGlow)
            {
                float gentle=rareDayAvailable?
                    .22f+.085f*Mathf.Sin(Time.unscaledTime*1.72f):
                    .045f;
                rareDayGlow.color=new Color(1f,.78f,.32f,gentle);
            }
            if(!playing && calendar.gameObject.activeSelf && now!=null &&
                Time.unscaledTime>=nextTick)
            {
                nextTick=Time.unscaledTime+1.25f;
                long currentUtc=now();
                if(DailyTreasureRules.UtcDay(currentUtc)!=lastPaintedUtcDay)
                    Refresh();
                else if(!claimButton.interactable &&
                    gateway.Snapshot().DailyTreasures.TotalClaims<int.MaxValue)
                    claimCaption.text=NextRewardCountdown(currentUtc);
            }
            // The real Lyra GLB is loaded asynchronously by HeroesUI. The
            // earlier scenery remains visible while it completes; probe
            // a bounded five-second window, never trigger a second importer.
            if(currentStage==DailyTreasureStageKind.Lyra &&
               milestoneLyraPortrait &&
               (lyraFallbackActive || !milestoneLyraPortrait.sprite) &&
               Time.unscaledTime>=nextLyraArtProbe &&
               Time.unscaledTime<stopLyraArtProbe)
            {
                nextLyraArtProbe=Time.unscaledTime+.7f;
                var realArt=TryCanonicalLyraPortrait();
                if(realArt)
                {
                    milestoneLyraPortrait.sprite=realArt;
                    lyraFallbackActive=false;
                    milestoneLyraPortrait.gameObject.SetActive(true);
                    lyraArtRevealAt=Time.unscaledTime;
                    Layout();
                }
            }
            if(currentStage==DailyTreasureStageKind.Lyra &&
               milestoneLyraPortrait && milestoneLyraPortrait.sprite &&
               milestoneLyraPortrait.gameObject.activeSelf)
            {
                var tint=milestoneLyraPortrait.color;
                tint.a=.96f*Mathf.Clamp01(
                    (Time.unscaledTime-lyraArtRevealAt)/.42f);
                milestoneLyraPortrait.color=tint;
            }
            if(!playing)
            {
                PaintEarnedCards();
                if(!string.IsNullOrEmpty(claimNotice) &&
                    Time.unscaledTime>=noticeUntil)
                {
                    claimNotice="";
                    Refresh();
                }
                return;
            }
            float elapsed=Time.unscaledTime-startTime;
            if(elapsed>=DailyTreasureRevealTimeline.RevealPeakSeconds && !chimePlayed)
            {
                chimePlayed=true;
                if(UnityEngine.Application.isPlaying && originalSfx)
                {
                    var cue=ceremonyTier==DailyTreasureCeremonyTier.Legendary?
                        chestGrandChime:chestChime;
                    if(cue)originalSfx.PlayOneShot(cue);
                }
            }
            var frame=PaintCeremonyFrame(elapsed);
            if(frame.Finished)ShowResult();
        }

#if UNITY_EDITOR
        // Editor evidence uses the EXACT runtime animation formula, not a
        // mock GIF; this never invokes a reward/command or advances gameplay.
        public void SampleForNativeEvidence(float elapsed)
        {
            if(!IsOpen || !playing)
                throw new InvalidOperationException("No live ceremony to sample");
            PaintCeremonyFrame(elapsed);
        }

        // Original artwork's staggered chest-to-inventory flight should be
        // captured too. This test modifies only presentation transforms;
        // it NEVER issues an award or recreates the underlying player state.
        public void SamplePaidItemFlightForNativeEvidence(float elapsed)
        {
            if(!IsOpen || playing || prizeCount==0 ||
               !prizeShelf.gameObject.activeSelf)
                throw new InvalidOperationException(
                    "No real paid illustrated item cards available to sample");
            if(elapsed<0f)throw new ArgumentOutOfRangeException(nameof(elapsed));
            prizeAnimating=true;
            prizeRevealStarted=Time.unscaledTime-elapsed;
            PaintEarnedCards();
        }
#endif

        // The old chest ceremony instantly jumped from its opening frame
        // into the full inventory screen. Ease the actual uGUI geometry over
        // the already-authored 1.33-1.83s light-settle interval, making room
        // for the real saved rewards without a jump cut or another canvas.
        // Unity RectTransforms only: no new textures, no gameplay commands,
        // no expensive Canvas rebuild in the animation frame loop.
        void AnimateDailyCeremonyReflow(float progress)
        {
            if(!playing || !chestReward || prizeCount<=0 ||
               currentStage!=DailyTreasureStageKind.Daily)return;
            bool portrait=root.rect.height>root.rect.width;
            float h=root.rect.height;
            float openingH=Mathf.Min(h-20f,portrait?450f:300f);
            float paidH=Mathf.Min(h-20f,portrait?700f:370f);
            float t=Mathf.Clamp01(progress);
            var frameSize=cardSurface.sizeDelta;
            float sceneH=Mathf.Lerp(openingH,paidH,t);
            cardSurface.sizeDelta=new Vector2(frameSize.x,sceneH);
            // Skip must remain under the same real finger position while
            // the bronze frame expands. This action never awards currency.
            if(skipButton)
            {
                var skipRect=(RectTransform)skipButton.transform;
                skipRect.anchoredPosition=new Vector2(
                    skipRect.anchoredPosition.x,
                    -60f+(openingH-sceneH)*.5f);
            }
            bool compactLandscape=!portrait && paidH<365f;
            float y=Mathf.Lerp(portrait?64f:42f,
                portrait?116f:compactLandscape?84f:89f,t);
            float artW=Mathf.Lerp(330f,
                portrait?284f:compactLandscape?290f:300f,t);
            float artH=Mathf.Lerp(portrait?265f:194f,
                portrait?246f:compactLandscape?164f:170f,t);
            // Explicit owned references rather than new Image[] or
            // RectTransform[] allocations on every animation frame.
            if(illustratedClosed)
            {
                illustratedClosed.rectTransform.anchoredPosition=new Vector2(0f,y);
                illustratedClosed.rectTransform.sizeDelta=new Vector2(artW,artH);
            }
            if(illustratedOpen)
            {
                illustratedOpen.rectTransform.anchoredPosition=new Vector2(0f,y);
                illustratedOpen.rectTransform.sizeDelta=new Vector2(artW,artH);
            }
            if(chest)
            {
                chest.rectTransform.anchoredPosition=new Vector2(0f,y);
                chest.rectTransform.sizeDelta=new Vector2(artW*.88f,
                    artH*.88f);
            }
            if(glowRect)glowRect.anchoredPosition=new Vector2(0f,y);
            if(ambientRect)ambientRect.anchoredPosition=new Vector2(0f,y);
            if(raysRect)raysRect.anchoredPosition=new Vector2(0f,y);
            float resultY=Mathf.Lerp(portrait?-132f:-100f,
                portrait?(paidH<680f?-217f:-232f):-15f,t);
            resultText.rectTransform.anchoredPosition=new Vector2(0f,resultY);
        }

        DailyTreasureRevealFrame PaintCeremonyFrame(float elapsed)
        {
            var frame=DailyTreasureRevealTimeline.Sample(elapsed,ceremonyTier);
            AnimateDailyCeremonyReflow(frame.Text);
            chest.SetOpening(frame.Opened);
            if(hasAuthoredChestArt)
            {
                illustratedClosed.color=new Color(1f,1f,1f,frame.Closed);
                illustratedOpen.color=new Color(1f,1f,1f,frame.Opened);
                // Less physical motion than a bouncing chest: anticipation,
                // momentary shake and a 3.5% warm-release settle.
                illustratedClosed.rectTransform.localScale=
                    Vector3.one*(frame.Scale+frame.Shake);
                illustratedOpen.rectTransform.localScale=
                    Vector3.one*(frame.Scale-frame.Shake);
            }
            glow.color=new Color(1f,.77f,.35f,frame.Glow*.86f);
            PaintMilestoneAtmosphere(elapsed,frame);
            PaintMilestoneStageEntrance(elapsed);
            ambient.color=new Color(activeTheme.R,activeTheme.G,
                activeTheme.B,frame.Glow*activeTheme.Strength);
            rays.SetIntensity(currentStage==DailyTreasureStageKind.Daily?
                frame.Glow*.79f:0f);
            for(int i=0;i<sparks.Length;i++)
            {
                float angle=i*Mathf.PI*2f/sparks.Length;
                float age=Mathf.Max(0f,elapsed-.66f);
                float distance=age*(34f+(i%4)*13f);
                // Sparks must originate at the CURRENT authored chest, not
                // at an obsolete fixed Y from the old 740x370 theatre.
                // This also keeps Lyra, Scouts, Vigilants and Bastion effects
                // attached to the true category-specific chest placement.
                float baseY=illustratedClosed?
                    illustratedClosed.rectTransform.anchoredPosition.y:
                    chest.rectTransform.anchoredPosition.y;
                sparks[i].rectTransform.anchoredPosition=new Vector2(
                    Mathf.Cos(angle)*distance+Mathf.Sin(age*3.4f+i)*4f,
                    Mathf.Sin(angle)*distance+age*age*15f+baseY);
                sparks[i].color=new Color(1f,.68f+.025f*i,.24f,
                    currentStage==DailyTreasureStageKind.Daily?
                    Mathf.Clamp01(frame.Embers*(1f-(i%3)*.08f)):0f);
            }
            return frame;
        }
    }
}

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
        static readonly Color Gold=new Color(.91f,.72f,.38f,1f);
        static readonly Color Ivory=new Color(.97f,.92f,.82f,1f);
        static readonly Color Muted=new Color(.72f,.70f,.66f,1f);
        static readonly Color Stone=new Color(.115f,.115f,.13f,1f);
        static readonly Color Oak=new Color(.27f,.15f,.11f,1f);
        ICommandGateway gateway;
        Func<long> now;
        Action refreshed;
        RectTransform root,entryRect,screen,cardSurface,calendar,ceremony,glowRect;
        Button entryButton,claimButton,closeButton,skipButton;
        Text header,subhead,claimCaption,resultText,entryCaption;
        readonly Text[] dayHead=new Text[7],dayBody=new Text[7];
        readonly Image[] dayBack=new Image[7],sparks=new Image[12];
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
        Image glow;
        Texture2D glowTexture;
        Sprite glowSprite;
        bool suppressed,playing;
        float startTime,nextTick,lastWidth=-1,lastHeight=-1,noticeUntil;
        long lastPaintedUtcDay=-1;
        string claimNotice="";
        string ceremonialReceipt,ceremonialTitle;
        bool chestReward,chimePlayed;
        AudioSource originalSfx;
        AudioClip chestKnock,chestChime;

        public bool Suppressed
        {
            get => suppressed;
            set { suppressed=value; if(entryRect && (!screen || !screen.gameObject.activeSelf))
                    entryRect.gameObject.SetActive(!suppressed); }
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
        // Original authored black-iron and worn-bronze frames, supplied as
        // real Sprite textures (not a full-screen screenshot). Nine-slice keeps
        // all carved corners sharp while modal/card/button sizes adapt.
        static bool ApplyAuthoredFrame(RectTransform rect,string resource)
        {
            var sprite=Resources.Load<Sprite>("DailyTreasureArt/"+resource);
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
                name=="Claim label" || name=="Confirmed received items";
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
            originalSfx.playOnAwake=false;originalSfx.spatialBlend=0f;originalSfx.volume=.27f;
            chestKnock=MakeOriginalSound("Valoria / wooden lock",176f,.17f);
            chestChime=MakeOriginalSound("Valoria / discovery chime",540f,.62f);
            root=(RectTransform)transform;
            // No second EventSystem, Canvas, save or navigation router.
            entryRect=NewRect("Daily reward / optional medallion",root);
            entryButton=ActionButton(entryRect,Oak,Open);
            if(!ApplyAuthoredFrame(entryRect,"valoria_frame_button_512x128"))
                Relief(entryRect,false,true);
            entryCaption=Label(entryRect,"Daily available label","COFRE",15,Gold,TextAnchor.MiddleCenter);
            Stretch(entryCaption.rectTransform);
            var entryShadow=entryRect.gameObject.AddComponent<Shadow>();
            entryShadow.effectColor=new Color(.02f,.012f,.012f,.8f);
            entryShadow.effectDistance=new Vector2(2f,-3f);

            screen=NewRect("Tributo de los siete días / modal",root);
            Stretch(screen);
            Plate(screen,new Color(.018f,.020f,.026f,.91f));
            cardSurface=NewRect("Basalt and worn bronze reward parchment",screen);
            Plate(cardSurface,Stone);
            if(!ApplyAuthoredFrame(cardSurface,"valoria_frame_main_512"))
                Relief(cardSurface,true);
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
                var iconSprite=Resources.Load<Sprite>(
                    "DailyTreasureArt/"+DayIllustrations[i]);
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
            claimButton=ActionButton(NewRect("Claim exact daily reward",cardSurface),Oak,Claim);
            if(!ApplyAuthoredFrame((RectTransform)claimButton.transform,
                "valoria_frame_button_512x128"))
                Relief((RectTransform)claimButton.transform,false,true);
            claimCaption=Label(claimButton.transform,"Claim label","RECLAMAR TRIBUTO",19,Ivory,TextAnchor.MiddleCenter);
            Stretch(claimCaption.rectTransform);

            ceremony=NewRect("Saved reward / optional animated reveal",cardSurface);
            // This block has no reward-awarding logic.
            glowRect=NewRect("Warm radial light / visual only",ceremony);
            glow=Plate(glowRect,new Color(1f,.65f,.18f,0f),false);
            CreateGlow();
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
            for(int i=0;i<sparks.Length;i++)
            {
                var spark=NewRect("Ember "+(i+1),ceremony);
                sparks[i]=Plate(spark,new Color(1f,.75f,.30f,0f),false);
                Place(spark,.5f,.5f,.5f,.5f,0,0,5,5);
            }
            skipButton=ActionButton(NewRect("Skip animation / keep earned items",ceremony),
                new Color(.23f,.19f,.17f,1f),ShowResult);
            if(!ApplyAuthoredFrame((RectTransform)skipButton.transform,
                "valoria_frame_button_512x128"))
                Relief((RectTransform)skipButton.transform,false,false);
            var skipText=Label(skipButton.transform,"Skip caption","SALTAR",15,Gold,TextAnchor.MiddleCenter);
            Stretch(skipText.rectTransform);
            ceremony.gameObject.SetActive(false);
            screen.gameObject.SetActive(false);
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
        static AudioClip MakeOriginalSound(string name,float frequency,float duration)
        {
            // Low-memory original procedural foley, not a single arcade beep:
            // brief wood/iron latch impulse followed by soft detuned bronze tones.
            // Generated only once when the panel is instantiated.
            const int rate=22050;
            int count=Mathf.CeilToInt(rate*duration);
            bool impact=frequency<300f;
            var data=new float[count];
            for(int i=0;i<count;i++)
            {
                float t=i/(float)rate;
                float attack=Mathf.Min(1f,t*750f);
                float seed=Mathf.Sin((i+9)*78.233f)*43758.5453f;
                float noise=Mathf.Repeat(seed,1f)*2f-1f;
                float tone=impact
                    ? Mathf.Sin(t*frequency*2f*Mathf.PI)*Mathf.Exp(-t*23f)+
                      .32f*Mathf.Sin(t*frequency*.52f*2f*Mathf.PI)*Mathf.Exp(-t*16f)+
                      noise*.20f*Mathf.Exp(-t*62f)
                    : (Mathf.Sin(t*frequency*2f*Mathf.PI)+
                       .36f*Mathf.Sin(t*frequency*1.51f*2f*Mathf.PI)+
                       .15f*Mathf.Sin(t*frequency*2.07f*2f*Mathf.PI))*
                       Mathf.Exp(-t*5.3f);
                data[i]=Mathf.Clamp(tone*attack*(impact?.24f:.18f),-.80f,.80f);
            }
            var clip=AudioClip.Create(name,count,1,rate,false);
            clip.SetData(data,0);
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
            if(Application.isPlaying)UnityEngine.Object.Destroy(asset);
            else UnityEngine.Object.DestroyImmediate(asset);
        }
        void OnDestroy()
        {
            DestroyOwned(glowSprite);
            DestroyOwned(glowTexture);
            DestroyOwned(chestKnock);
            DestroyOwned(chestChime);
        }
        public void Open()
        {
            if(suppressed || gateway==null || now==null)return;
            screen.gameObject.SetActive(true);
            screen.SetAsLastSibling();
            ceremony.gameObject.SetActive(false);
            playing=false;
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
            if(screen)screen.gameObject.SetActive(false);
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
            claimButton.interactable=reward.IsValid;
            claimCaption.text=reward.IsValid?"RECLAMAR DÍA "+reward.CycleDay:
                "TRIBUTO YA RECOGIDO · REGRESA MAÑANA";
        }
        static string ShortReward(DailyTreasureReward reward)
        {
            string text="";
            if(reward.Wood>0)text+="Madera "+reward.Wood+"\n";
            if(reward.Stone>0)text+="Piedra "+reward.Stone+"\n";
            if(reward.Food>0)text+="Comida "+reward.Food+"\n";
            if(reward.SpeedupOneMinute>0)text+="1 min x"+reward.SpeedupOneMinute+"\n";
            if(reward.SpeedupFiveMinutes>0)text+="5 min x"+reward.SpeedupFiveMinutes+"\n";
            return text.Trim();
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
            Present("TRIBUTO DEL DÍA "+reward.CycleDay,
                DailyTreasureRules.Receipt(reward),reward.ShowChest);
        }
        // Called only after the game's authoritative reward/report has ALREADY
        // been committed by its existing command. Never calls Execute,
        // grants a bonus, opens the daily claim or invents a loot table.
        public bool PresentCommittedMilestone(string title,string confirmedResult)
        {
            if(gateway==null || now==null || screen==null ||
               string.IsNullOrWhiteSpace(title))return false;
            if(IsOpen)return false;
            screen.gameObject.SetActive(true);
            screen.SetAsLastSibling();
            lastWidth=lastHeight=-1f;
            Layout();
            Present(title,confirmedResult,true);
            return true;
        }

        // Safe to reuse for milestone ceremonies: NEVER mints rewards.
        public void Present(string title,string alreadyReceived,bool showChest)
        {
            if(!IsOpen)return;
            ceremonialTitle=title;ceremonialReceipt=alreadyReceived;chestReward=showChest;
            calendar.gameObject.SetActive(false);
            claimButton.gameObject.SetActive(false);
            ceremony.gameObject.SetActive(true);
            resultText.text=showChest?"EL COFRE DE LAS CENIZAS":"RECOMPENSA RECIBIDA";
            playing=showChest;
            chimePlayed=false;
            if(Application.isPlaying && originalSfx && chestKnock && showChest)
                originalSfx.PlayOneShot(chestKnock);
            startTime=Time.unscaledTime;
            chest.gameObject.SetActive(showChest && !hasAuthoredChestArt);
            chest.SetOpening(0f);
            illustratedClosed.gameObject.SetActive(showChest && hasAuthoredChestArt);
            illustratedOpen.gameObject.SetActive(showChest && hasAuthoredChestArt);
            illustratedClosed.color=Color.white;
            illustratedOpen.color=new Color(1f,1f,1f,0f);
            illustratedClosed.rectTransform.localScale=Vector3.one;
            illustratedOpen.rectTransform.localScale=Vector3.one;
            glow.color=new Color(1f,.75f,.36f,showChest?0f:.34f);
            skipButton.gameObject.SetActive(showChest);
            if(!showChest)ShowResult();
        }
        void ShowResult()
        {
            playing=false;
            chest.SetOpening(1f);
            if(hasAuthoredChestArt)
            {
                illustratedClosed.color=new Color(1f,1f,1f,0f);
                illustratedOpen.color=Color.white;
                illustratedClosed.rectTransform.localScale=Vector3.one;
                illustratedOpen.rectTransform.localScale=Vector3.one;
            }
            resultText.text=ceremonialTitle+"\n\n"+ceremonialReceipt+
                "\n\nGuardado · justificante en el buzón";
            skipButton.gameObject.SetActive(false);
            glow.color=new Color(1f,.74f,.34f,.27f);
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
            Place(entryRect,0f,0f,0f,0f,14f,portrait?100f:83f,76f,54f);
            float panelW=Mathf.Min(w-24f,portrait?440f:740f);
            float panelH=Mathf.Min(h-20f,portrait?700f:370f);
            Place(cardSurface,.5f,.5f,.5f,.5f,0,0,panelW,panelH);
            Place(header.rectTransform,.5f,1f,.5f,1f,0,-8f,panelW-64f,40f);
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
                Place(dayHead[i].rectTransform,.5f,1f,.5f,1f,0f,-5f,width-6f,25f);
                if(i==6)
                {
                    Place(dayGlyph[i].rectTransform,.5f,.5f,.5f,.5f,
                        -width*.5f+48f,-11f,73f,57f);
                    if(dayArtwork[i])Place(dayArtwork[i].rectTransform,
                        .5f,.5f,.5f,.5f,-width*.5f+48f,-11f,85f,65f);
                    Place(dayBody[i].rectTransform,.5f,.5f,.5f,.5f,
                        38f,-12f,width-116f,Mathf.Max(52f,cellH-39f));
                    dayBody[i].fontSize=15;
                }
                else
                {
                    // Short 667x375 landscapes still need separate text/icon
                    // space; make small, bounded adjustments rather than shrink
                    // the entire Canvas or blur all text by scaling transforms.
                    bool tiny=cellH<96f;
                    if(tiny)Place(dayHead[i].rectTransform,.5f,1f,.5f,1f,
                        0f,-5f,cellW-6f,24f);
                    Place(dayGlyph[i].rectTransform,.5f,.5f,.5f,.5f,
                        0f,tiny?2f:3f,48f,tiny?24f:28f);
                    if(dayArtwork[i])Place(dayArtwork[i].rectTransform,
                        .5f,.5f,.5f,.5f,0f,tiny?2f:3f,
                        tiny?51f:58f,tiny?31f:42f);
                    Place(dayBody[i].rectTransform,.5f,.5f,.5f,.5f,
                        0f,-cellH*.5f+(tiny?17f:18.5f),cellW-10f,tiny?31f:33f);
                    dayBody[i].fontSize=tiny?13:14;
                }
            }
            Place((RectTransform)claimButton.transform,.5f,0f,.5f,0f,
                0f,13f,Mathf.Min(panelW-30f,portrait?325f:405f),54f);
            Stretch(ceremony);
            Place(glowRect,.5f,.5f,.5f,.5f,0,10,portrait?290f:355f,portrait?290f:355f);
            Place(chest.rectTransform,.5f,.5f,.5f,.5f,0,portrait?55f:30f,
                portrait?195f:165f,portrait?150f:125f);
            // Full-cell transparent source sprites preserve the model's scale,
            // and do not stretch the illustrated ironwork on odd phone ratios.
            Place(illustratedClosed.rectTransform,.5f,.5f,.5f,.5f,
                0f,portrait?55f:10f,portrait?260f:235f,portrait?195f:170f);
            Place(illustratedOpen.rectTransform,.5f,.5f,.5f,.5f,
                0f,portrait?55f:10f,portrait?260f:235f,portrait?195f:170f);
            Place(resultText.rectTransform,.5f,.5f,.5f,.5f,0,
                portrait?-140f:-95f,panelW-45f,portrait?165f:118f);
            // Skip stays away from the receipt and is a genuine >=44px hit target.
            Place((RectTransform)skipButton.transform,1f,1f,1f,1f,-10f,-60f,100f,45f);
        }
        void Update()
        {
            if(!root)return;
            if(!IsOpen)
            {
                entryRect.gameObject.SetActive(!suppressed);
                if(gateway!=null && now!=null && Time.unscaledTime>=nextTick)
                {
                    nextTick=Time.unscaledTime+.8f;
                    bool available=DailyTreasureRules.CanClaim(
                        gateway.Snapshot().DailyTreasures,now());
                    entryCaption.color=available?Gold:Muted;
                    entryCaption.text=available?"TRIBUTO":"COFRE";
                }
                return;
            }
            float w=root.rect.width,h=root.rect.height;
            if(Mathf.Abs(w-lastWidth)>1f || Mathf.Abs(h-lastHeight)>1f)Layout();
            if(!playing && calendar.gameObject.activeSelf && now!=null &&
                Time.unscaledTime>=nextTick)
            {
                nextTick=Time.unscaledTime+1.25f;
                if(DailyTreasureRules.UtcDay(now())!=lastPaintedUtcDay)
                    Refresh();
            }
            if(!playing)
            {
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
                if(Application.isPlaying && originalSfx && chestChime)
                    originalSfx.PlayOneShot(chestChime);
            }
            var frame=DailyTreasureRevealTimeline.Sample(elapsed);
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
            for(int i=0;i<sparks.Length;i++)
            {
                float angle=i*Mathf.PI*2f/sparks.Length;
                float distance=Mathf.Max(0f,elapsed-.66f)*(34f+(i%3)*13f);
                sparks[i].rectTransform.anchoredPosition=new Vector2(
                    Mathf.Cos(angle)*distance,
                    Mathf.Sin(angle)*distance+18f);
                sparks[i].color=new Color(1f,.68f+.025f*i,.24f,
                    Mathf.Clamp01(frame.Embers*(1f-(i%3)*.08f)));
            }
            if(frame.Finished)ShowResult();
        }
    }
}

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
        DailyTreasureChestGraphic chest;
        Image glow;
        Texture2D glowTexture;
        Sprite glowSprite;
        bool suppressed,playing;
        float startTime,nextTick,lastWidth=-1,lastHeight=-1;
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
        static Text Label(Transform parent,string name,string value,
            int fontSize,Color tint,TextAnchor align)
        {
            var r=NewRect(name,parent);
            var t=r.gameObject.AddComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            chestKnock=MakeOriginalSound("Valoria / wooden lock",176f,.12f);
            chestChime=MakeOriginalSound("Valoria / discovery chime",540f,.23f);
            root=(RectTransform)transform;
            // No second EventSystem, Canvas, save or navigation router.
            entryRect=NewRect("Daily reward / optional medallion",root);
            entryButton=ActionButton(entryRect,Oak,Open);
            entryCaption=Label(entryRect,"Daily available label","COFRE",15,Gold,TextAnchor.MiddleCenter);
            Stretch(entryCaption.rectTransform);
            var entryBorder=entryRect.gameObject.AddComponent<Outline>();
            entryBorder.effectColor=Gold;entryBorder.effectDistance=new Vector2(2f,-2f);

            screen=NewRect("Tributo de los siete días / modal",root);
            Stretch(screen);
            Plate(screen,new Color(.018f,.020f,.026f,.91f));
            cardSurface=NewRect("Basalt and worn bronze reward parchment",screen);
            Plate(cardSurface,Stone);
            var edge=cardSurface.gameObject.AddComponent<Outline>();
            edge.effectColor=Gold;edge.effectDistance=new Vector2(2f,-2f);
            header=Label(cardSurface,"Reward heading","TESOROS DE VALORIA",27,Gold,TextAnchor.MiddleCenter);
            subhead=Label(cardSurface,"Consecutive visits","",16,Muted,TextAnchor.MiddleCenter);
            closeButton=ActionButton(NewRect("Close tributes",cardSurface),Oak,Close);
            var closeLabel=Label(closeButton.transform,"Accessible close","X",24,Ivory,TextAnchor.MiddleCenter);
            Stretch(closeLabel.rectTransform);
            calendar=NewRect("Seven genuine rewards",cardSurface);
            for(int i=0;i<7;i++)
            {
                var day=NewRect("Day "+(i+1)+" / non-clickable informational card",calendar);
                dayBack[i]=Plate(day,i==6?new Color(.27f,.20f,.11f,1f):Oak,false);
                dayHead[i]=Label(day,"Day heading","DÍA "+(i+1),17,Gold,TextAnchor.MiddleCenter);
                dayBody[i]=Label(day,"Actual reward contents","",15,Ivory,TextAnchor.MiddleCenter);
                Stretch(dayHead[i].rectTransform);
                Stretch(dayBody[i].rectTransform);
                Place(dayHead[i].rectTransform,.5f,1f,.5f,1f,0,-6,150,26);
            }
            claimButton=ActionButton(NewRect("Claim exact daily reward",cardSurface),Oak,Claim);
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
            resultText=Label(ceremony,"Confirmed received items","",21,Ivory,TextAnchor.MiddleCenter);
            for(int i=0;i<sparks.Length;i++)
            {
                var spark=NewRect("Ember "+(i+1),ceremony);
                sparks[i]=Plate(spark,new Color(1f,.75f,.30f,0f),false);
                Place(spark,.5f,.5f,.5f,.5f,0,0,5,5);
            }
            skipButton=ActionButton(NewRect("Skip animation / keep earned items",ceremony),
                new Color(.23f,.19f,.17f,1f),ShowResult);
            var skipText=Label(skipButton.transform,"Skip caption","SALTAR",15,Gold,TextAnchor.MiddleCenter);
            Stretch(skipText.rectTransform);
            ceremony.gameObject.SetActive(false);
            screen.gameObject.SetActive(false);
        }
        static AudioClip MakeOriginalSound(string name,float frequency,float duration)
        {
            const int rate=22050;
            int count=Mathf.CeilToInt(rate*duration);
            var data=new float[count];
            for(int i=0;i<count;i++)
            {
                float t=i/(float)rate;
                float fade=1f-i/(float)count;
                float vibration=Mathf.Sin(t*frequency*Mathf.PI*2f)+
                    .23f*Mathf.Sin(t*frequency*1.5f*Mathf.PI*2f);
                data[i]=vibration*fade*fade*.22f;
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
        void OnDestroy()
        {
            if(glowSprite)Destroy(glowSprite);
            if(glowTexture)Destroy(glowTexture);
            if(chestKnock)Destroy(chestKnock);
            if(chestChime)Destroy(chestChime);
        }
        public void Open()
        {
            if(suppressed || gateway==null || now==null)return;
            screen.gameObject.SetActive(true);
            screen.SetAsLastSibling();
            ceremony.gameObject.SetActive(false);
            playing=false;
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
            var reward=DailyTreasureRules.Preview(daily,now());
            int upcoming=DailyTreasureRules.NextDay(daily);
            subhead.text="RACHA: "+daily.ConsecutiveDays+" DÍAS  ·  7 TRIBUTOS POR CICLO";
            for(int i=0;i<7;i++)
            {
                bool next=i+1==upcoming && reward.IsValid;
                var item=DailyTreasureRules.ForDay(i+1,next?reward.Streak:0);
                bool newCycle=daily.CycleDay==7 && upcoming==1 && reward.IsValid;
                bool collected=!newCycle && daily.CycleDay>0 && i+1<=daily.CycleDay;
                dayHead[i].text="DÍA "+(i+1)+(collected?"  ✓":next?"  ●":"");
                dayHead[i].color=next?Ivory:Gold;
                dayBack[i].color=next?new Color(.42f,.29f,.16f,1f):
                    collected?new Color(.15f,.18f,.15f,1f):
                    i==6?new Color(.28f,.20f,.11f,1f):Oak;
                dayBody[i].text=ShortReward(item);
                dayBody[i].color=collected?Muted:Ivory;
            }
            claimButton.interactable=reward.IsValid;
            claimCaption.text=reward.IsValid?"RECLAMAR DÍA "+reward.CycleDay:
                "TRIBUTO YA RECOGIDO · REGRESA MAÑANA";
        }
        static string ShortReward(DailyTreasureReward reward)
        {
            string text="";
            if(reward.Wood>0)text+="Madera "+reward.Wood+"  ";
            if(reward.Stone>0)text+="Piedra "+reward.Stone+"  ";
            if(reward.Food>0)text+="Comida "+reward.Food+"  ";
            if(reward.SpeedupOneMinute>0)text+="Acel. 1 min ×"+reward.SpeedupOneMinute+"  ";
            if(reward.SpeedupFiveMinutes>0)text+="Acel. 5 min ×"+reward.SpeedupFiveMinutes;
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
            if(!result.Ok){Refresh();return;}
            var after=gateway.Snapshot();
            var current=after.DailyTreasures;
            var reward=DailyTreasureRules.ForDay(current.CycleDay,current.ConsecutiveDays);
            refreshed?.Invoke();
            Present("TRIBUTO DEL DÍA "+reward.CycleDay,
                DailyTreasureRules.Receipt(reward),reward.ShowChest);
        }
        // Safe to reuse for future milestone ceremonies: NEVER mints rewards.
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
            if(originalSfx && chestKnock && showChest)originalSfx.PlayOneShot(chestKnock);
            startTime=Time.unscaledTime;
            chest.gameObject.SetActive(showChest);
            chest.SetOpening(0f);
            glow.color=new Color(1f,.75f,.36f,showChest?0f:.34f);
            skipButton.gameObject.SetActive(showChest);
            if(!showChest)ShowResult();
        }
        void ShowResult()
        {
            playing=false;
            chest.SetOpening(1f);
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
            Place((RectTransform)closeButton.transform,1f,1f,1f,1f,-8f,-8f,42f,42f);
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
                float x=-panelW*.5f+left+column*(cellW+gap)+cellW*.5f;
                float y=gridHeight*.5f-row*(cellH+gap)-cellH*.5f;
                Place(dayBack[i].rectTransform,.5f,.5f,.5f,.5f,x,y,cellW,cellH);
                Place(dayHead[i].rectTransform,.5f,1f,.5f,1f,0f,-5f,cellW-6f,25f);
                Place(dayBody[i].rectTransform,.5f,.5f,.5f,.5f,0f,-11f,cellW-10f,cellH-34f);
            }
            Place((RectTransform)claimButton.transform,.5f,0f,.5f,0f,
                0f,13f,Mathf.Min(panelW-30f,portrait?325f:405f),54f);
            Stretch(ceremony);
            Place(glowRect,.5f,.5f,.5f,.5f,0,10,portrait?290f:355f,portrait?290f:355f);
            Place(chest.rectTransform,.5f,.5f,.5f,.5f,0,portrait?55f:30f,
                portrait?195f:165f,portrait?150f:125f);
            Place(resultText.rectTransform,.5f,.5f,.5f,.5f,0,
                portrait?-140f:-95f,panelW-45f,portrait?165f:118f);
            Place((RectTransform)skipButton.transform,1f,0f,1f,0f,-10f,10f,90f,35f);
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
            if(!playing)return;
            float elapsed=Time.unscaledTime-startTime;
            if(elapsed>=.82f && !chimePlayed)
            {
                chimePlayed=true;
                if(originalSfx && chestChime)originalSfx.PlayOneShot(chestChime);
            }
            float opening=Mathf.SmoothStep(0f,1f,Mathf.Clamp01((elapsed-.50f)/.72f));
            chest.SetOpening(opening);
            float alpha=Mathf.Sin(Mathf.Clamp01((elapsed-.30f)/2.1f)*Mathf.PI);
            glow.color=new Color(1f,.77f,.35f,alpha*.72f);
            for(int i=0;i<sparks.Length;i++)
            {
                float angle=i*Mathf.PI*2f/sparks.Length;
                float distance=Mathf.Max(0f,elapsed-.68f)*(35f+(i%3)*15f);
                var r=sparks[i].rectTransform;
                r.anchoredPosition=new Vector2(
                    Mathf.Cos(angle)*distance,
                    Mathf.Sin(angle)*distance+(chestReward?16f:0f));
                sparks[i].color=new Color(1f,.68f+.025f*i,.24f,
                    Mathf.Clamp01(1f-Mathf.Abs(elapsed-1.18f)*1.2f));
            }
            if(elapsed>=2.25f)ShowResult();
        }
    }
}

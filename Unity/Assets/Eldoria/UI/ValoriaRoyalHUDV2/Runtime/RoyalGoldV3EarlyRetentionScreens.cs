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
        RectTransform safe, screenRect, panelRoot, heroScene, infoCard;
        RoyalButton vigilantShortcut, scoutsShortcut, back, mainAction, claimXp;
        readonly RoyalButton[] scoutActions=new RoyalButton[3];
        readonly RoyalText[] scoutCards=new RoyalText[3];
        readonly RoyalText[] scoutActionLabels=new RoyalText[3];
        readonly RoyalGoldV3ScoutContractGraphic[] scoutArt=new RoyalGoldV3ScoutContractGraphic[3];
        readonly string[] scoutIds=new string[3];
        RoyalText heading, kicker, introduction, chapter, opponent, formation,
            reward, battleFeedback, passive, footer, mainActionLabel, xpLabel;
        RoyalGoldV3EarlyRetentionArtwork sceneArtwork;
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
            infoCard=Panel(panel,"Progression and expedition information");
            chapter=Text(infoCard,"Stage progression",23,Gold,true);
            opponent=Text(infoCard,"Real enemy and trait",19,Ivory);
            formation=Text(infoCard,"Actual available army",17,Muted);
            reward=Text(infoCard,"Authoritative first victory reward",17,Gold);
            passive=Text(infoCard,"Offline hero experience",18,Ivory);
            battleFeedback=Text(infoCard,"Last action result",17,Ivory);
            mainAction=Button(infoCard,"Battle against next unique stage",
                "COMBATIR",ExecuteStage,out mainActionLabel,true);
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
                scoutCards[i]=Text(cell,"Real scout order description",17,Ink);
                scoutActions[i]=Button(cell,"Dispatch or claim scout "+(i+1),
                    "ENVIAR",()=>RunScout(index),out scoutActionLabels[i],true);
                scoutActions[i].transform.SetParent(cell,false);
                cell.name="Scout / live contract "+(i+1);
            }
            footer=Text(panel,"No artificial notifications",15,Muted);
            back=Button(panel,"Return to actual kingdom","VOLVER A VALORIA",
                Close,out var backLabel);
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
            if(!ready||Time.unscaledTime<nextUpdate)return;
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
            TL(heading.rectTransform,20f,18f,pw-210f,44f);
            heading.fontSize=portrait?27:34;
            TL(kicker.rectTransform,20f,61f,pw-35f,27f);
            TL((RectTransform)back.transform,pw-(portrait?150f:184f)-14f,
                23f,portrait?150f:184f,46f);
            float imageTop=portrait?105f:108f;
            TL(heroScene,portrait?18f:20f,imageTop,
                portrait?pw-36f:pw*.44f,portrait?Mathf.Clamp(ph*.235f,155,224):ph-168f);
            TL(infoCard,bodyX,portrait?imageTop+Mathf.Clamp(ph*.235f,155,224)+10f:108f,
                bodyWidth,portrait?ph-(imageTop+Mathf.Clamp(ph*.235f,155,224)+22f):ph-165f);
            float iw=infoCard.rect.width,ih=infoCard.rect.height;
            if(iw<=0||ih<=0)return;
            TL(chapter.rectTransform,14f,13f,iw-28f,36f);
            TL(opponent.rectTransform,14f,52f,iw-28f,47f);
            TL(formation.rectTransform,14f,101f,iw-28f,34f);
            TL(reward.rectTransform,14f,138f,iw-28f,35f);
            TL(passive.rectTransform,14f,182f,iw-28f,50f);
            TL((RectTransform)mainAction.transform,14f,Mathf.Max(236f,ih-114f),iw-28f,49f);
            TL((RectTransform)claimXp.transform,14f,Mathf.Max(288f,ih-62f),iw-28f,44f);
            TL(battleFeedback.rectTransform,14f,Mathf.Max(182f,ih-184f),iw-28f,48f);
            // The approved quest-board cards occupy the actual manuscript,
            // not three tiny 95-point rows floating at the top of a phone.
            // Scale all three naturally with their destination's usable height.
            float head=57f; // Preserve the "ENCARGOS EN CURSO" royal heading.
            float spacing=Mathf.Clamp((ih-head-12f)/3f,95f,330f);
            for(int i=0;i<3;i++)
            {
                var cell=scoutCards[i].transform.parent as RectTransform;
                TL(cell,12f,head+i*spacing,iw-24f,spacing-5f);
                float tab=Mathf.Clamp((iw-24f)*.14f,35f,65f);
                float captionX=tab+22f;
                TL(scoutCards[i].rectTransform,captionX,9f,
                    Mathf.Max(75f,iw-(portrait?115f:153f)-captionX),spacing-19f);
                TL((RectTransform)scoutActions[i].transform,
                    iw-(portrait?112f:150f),spacing*.5f-19f,
                    portrait?87f:120f,40f);
                scoutCards[i].fontSize=portrait?14:17;
                scoutActionLabels[i].fontSize=portrait?13:16;
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
            int chapterProgress=p.LastClearedStage%25;
            if(sceneArtwork.ChapterProgress!=chapterProgress || explicitRefresh)
            {
                sceneArtwork.ChapterProgress=chapterProgress;
                sceneArtwork.SetVerticesDirty();
            }
            heading.Put(vig?"CAMINO DE LOS VIGILANTES":"MESA DE EXPLORADORES");
            kicker.Put(vig?"PRUEBAS DEL REINO · COMBATE AUTOMÁTICO":
                "ENCARGOS REALES · REGRESOS PERSISTENTES");
            introduction.gameObject.SetActive(false);
            chapter.gameObject.SetActive(vig); // Scout board uses its own three distinct mission cards.
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
                if(sceneArtwork.BossEncounter!=boss)
                {
                    sceneArtwork.BossEncounter=boss;
                    sceneArtwork.SetVerticesDirty();
                }
                kicker.Put(info.Scene.ToUpperInvariant()+" · "+
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
                mainActionLabel.Put(mainAction.interactable?"COMBATIR AUTOMÁTICAMENTE":
                    "DESBLOQUEO PENDIENTE");
                claimXp.interactable=xp>0;
                xpLabel.Put(xp>0?"RECOGER "+xp+" XP":"EXPERIENCIA EN PROGRESO");
                battleFeedback.Put(notice);
                footer.Put("Una sola victoria por etapa · El resultado no necesita supervisión.");
            }
            else
            {
                chapter.Put("ENCARGOS EN CURSO");
                var list=p.Orders.Where(x=>x!=null&&!x.Claimed)
                    .OrderBy(x=>x.StartedUtcTicks>0?0:1)
                    .ThenBy(x=>x.Period).ThenBy(x=>x.Slot).Take(3).ToArray();
                for(int i=0;i<3;i++)
                {
                    var t=scoutCards[i];var a=scoutActions[i];var label=scoutActionLabels[i];
                    if(i>=list.Length)
                    {
                        t.Put("Los exploradores no tienen nuevos encargos.");
                        a.interactable=false;label.Put("ESPERAR");scoutIds[i]="";
                        continue;
                    }
                    var order=list[i];
                    scoutIds[i]=order.Id;
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
                    t.Put(EarlyRetentionRules.ScoutTitle(order.Kind)+"\n"+
                        EarlyRetentionRules.ScoutRewardText(order.Kind)+"\n"+state);
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

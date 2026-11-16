// ASHEN CITADEL · author's second interaction layer.
// All actions resolve through the existing LocalGateway, not a parallel gameplay model.
using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Eldoria.ExperimentalAshenCitadel
{
    public sealed partial class AshenCitadelExperience
    {
        private InputField playerNickname;
        private Text timeText;
        private Button dailyButton;
        private string openedArea="";

        private void BuildAdditionalAccesses()
        {
            // A secondary functional navigation tier. Same original button family.
            var toolbar=Node(safe,"Account navigation",
                new Vector2(0,1),new Vector2(1,1),
                new Vector2(8,-169),new Vector2(-8,-122));
            accountHud=toolbar;
            var background=Plate(toolbar,frame,Color.white);
            background.raycastTarget=false;
            var commands=new (string name,Action activate)[]{
                ("CORREO",OpenMailbox),("BOLSA",OpenBag),
                ("HÉROES",OpenHeroes),("RETOS",OpenChallenge)
            };
            for(int i=0;i<commands.Length;i++)
            {
                var n=i;var m=commands.Length;
                Button(toolbar,"Access · "+commands[n].name,commands[n].name,secondary,
                    new Vector2(n/(float)m,.07f),new Vector2((n+1)/(float)m,.94f),
                    new Vector2(3,0),new Vector2(-3,0),commands[n].activate,13);
            }
            dailyButton=Button(safe,"Daily treasury","TRIBUTO",primary,
                new Vector2(0,.0f),new Vector2(.33f,0),
                new Vector2(12,115),new Vector2(-4,153),OpenDaily,13);
            timeText=Label(safe,"Timekeeper",14,TextAnchor.MiddleRight,Muted,
                new Vector2(.4f,0),new Vector2(1,0),
                new Vector2(0,113),new Vector2(-12,153));
            RefreshClockLabel();
        }

        private void RefreshClockLabel()
        {
            var state=gateway?.Snapshot();
            if(state==null || timeText==null)return;
            var now=DateTime.UtcNow.Ticks;
            long next=state.BastionUpgradeCompletesUtcTicks;
            if(next<=0)next=state.BuildingCompletesUtcTicks;
            if(next<=0)next=state.RecruitmentCompletesUtcTicks;
            string work=next>now ? "OBRA "+FormatRemaining(next-now) :
                next>0 ? "COMPLETANDO" : "REINO ACTIVO";
            int unread=state.Mailbox?.Unread()??0;
            timeText.text=work+(unread>0 ? " · "+unread+" cartas" : "");
        }

        private static string FormatRemaining(long ticks)
        {
            var ts=TimeSpan.FromTicks(Math.Max(0,ticks));
            return ts.TotalHours>=1
                ? ((int)ts.TotalHours).ToString("00")+":"+ts.Minutes.ToString("00")
                : ts.Minutes.ToString("00")+":"+ts.Seconds.ToString("00");
        }

        private void OpenMailbox()
        {
            var s=gateway.Snapshot();
            var actions=new List<(string,Action,bool)>();
            var letters=s.Mailbox?.List(null,0,25);
            if(letters!=null)
                foreach(var letter in letters)
                {
                    var id=letter.EventId;
                    var label=(letter.Read?"":"● ")+letter.Title;
                    Add(actions,label,()=>OpenMailDetail(id),true);
                }
            Open("BUZÓN REAL",
                letters==null||letters.Count==0
                    ? "Todavía no han llegado informes ni recompensas. Las marchas y misiones enviarán cartas reales."
                    : "Informes de batalla, exploraciones y premios conservados en el guardado. " +
                      "Pendientes: "+(s.Mailbox?.Unread()??0),actions);
            openedArea="mail";
        }

        private void OpenMailDetail(string eventId)
        {
            var s=gateway.Snapshot();
            var item=s.Mailbox?.Messages?.Find(x=>x!=null&&x.EventId==eventId);
            if(item==null){OpenMailbox();return;}
            var actions=new List<(string,Action,bool)>();
            string battle="";
            if(item.Battle!=null)
            {
                var report=item.Battle;
                battle="\n"+(report.Won?"VICTORIA":"RESULTADO: "+report.EffectiveOutcome)+
                    " · "+report.Rounds+" rondas · poder "+report.PlayerPower+
                    " contra "+report.EnemyPower+
                    "\n"+report.Reason;
            }
            if(!item.Read)
                Add(actions,"MARCAR COMO LEÍDO",()=>{Send("MailRead",eventId);OpenMailDetail(eventId);});
            bool hasMailPackets=s.ResourceRewards?.Packets?.Any(p=>p!=null&&
                p.MailEventId==eventId&&!p.Collected)??false;
            if(hasMailPackets)
                Add(actions,"RECLAMAR PAQUETES",()=>{
                    Send(ResourceRewardCommands.ClaimMail,eventId);
                    OpenMailDetail(eventId);
                });
            Add(actions,"VOLVER AL BUZÓN",OpenMailbox);
            if(!hasMailPackets)
                Add(actions,"ELIMINAR CARTA",()=>{Send("MailDelete",eventId);OpenMailbox();});
            Open(item.Title, (item.Subtitle??"")+(item.DiscoveryText??"")+battle,actions);
            openedArea="mail-detail";
        }

        private void OpenBag()
        {
            var s=gateway.Snapshot();
            var actions=new List<(string,Action,bool)>();
            var packets=s.ResourceRewards?.Packets?.Where(x=>x!=null&&x.Collected&&!x.Used).ToList();
            if(packets!=null)
                foreach(var packet in packets)
                {
                    var receipt=packet.ReceiptId;
                    string kind=packet.Kind=="wood"?"MADERA":packet.Kind=="stone"?"PIEDRA":"COMIDA";
                    string text=kind+" · "+packet.Amount+" · "+(packet.SeenInBag?"USAR":"NUEVO");
                    Add(actions,text,()=>{
                        // First exposure marks the packet as seen; a separate explicit tap uses it.
                        if(!packet.SeenInBag)Send(ResourceRewardCommands.MarkBagSeen,receipt);
                        else Send(ResourceRewardCommands.UsePacket,receipt);
                        OpenBag();
                    });
                }
            Open("BOLSA DE RECURSOS",
                packets==null||packets.Count==0
                    ? "Sin paquetes disponibles. Los premios llegarán por los sistemas reales."
                    : "Cada objeto ocupa una casilla lógica. Toca NUEVO para inspeccionarlo y después USAR " +
                      "para acreditar la cantidad exacta. Objetos pendientes: "+packets.Count,
                actions);
            openedArea="bag";
        }

        private void OpenHeroes()
        {
            var s=gateway.Snapshot();
            var actions=new List<(string,Action,bool)>();
            bool lyra=s.ChapterThree!=null&&s.ChapterThree.LyraRecruited;
            int xpA=s.Retention?.AldricExperience??0,xpL=s.Retention?.LyraExperience??0;
            Add(actions,"SIR ALDRIC · PODER "+SliceRules.TotalPower(s).Total,()=>{
                Open("SIR ALDRIC",
                    "Comandante del reino. Experiencia ganada: "+xpA+
                    "\nLas habilidades, composición y combate pertenecen al sistema existente.",
                    new List<(string,Action,bool)>{("VOLVER A HÉROES",OpenHeroes,true)});
            });
            if(lyra)
                Add(actions,"LYRA · EXPERIENCIA "+xpL,()=>{
                    Open("LYRA","Desbloqueada por la historia de la Brecha. Experiencia: "+xpL,
                        new List<(string,Action,bool)>{("VOLVER A HÉROES",OpenHeroes,true)});
                });
            Open("SALA DE HÉROES",
                "Héroes disponibles: Aldric"+(lyra?" y Lyra":". Lyra se revela durante el capítulo III.")+
                "\nNo existen héroes ficticios ni tres personajes antes de desbloquearlos.",
                actions);
            openedArea="heroes";
        }

        private void OpenDaily()
        {
            var s=gateway.Snapshot();
            var actions=new List<(string,Action,bool)>();
            bool can=s.DailyTreasures!=null&&DailyTreasureRules.CanClaim(s.DailyTreasures,DateTime.UtcNow.Ticks);
            var preview=can?DailyTreasureRules.Preview(s.DailyTreasures,DateTime.UtcNow.Ticks):default(DailyTreasureReward);
            if(can)
                Add(actions,"RECLAMAR TRIBUTO "+preview.CycleDay+"/7",()=>{
                    Send("ClaimDailyTreasure","daily");
                    OpenMailbox();
                });
            Add(actions,"VER CORREO DE RECOMPENSAS",OpenMailbox);
            Open("TRIBUTO DIARIO",
                can?"Tu premio del día "+preview.CycleDay+
                    " está disponible. Al reclamarlo, se crea un correo real.":
                    "El tributo ya está reclamado. Se renueva al siguiente día UTC.",
                actions);
            openedArea="daily";
        }

        private void OpenChallenge()
        {
            var s=gateway.Snapshot();
            bool vigilants=EarlyRetentionRules.VigilantsUnlocked(s);
            bool scouts=EarlyRetentionRules.ScoutsUnlocked(s);
            var actions=new List<(string,Action,bool)>();
            if(vigilants)
            {
                int next=(s.Retention?.LastClearedStage??0)+1;
                bool enabled=EarlyRetentionRules.StageUnlocked(s,next)&&s.Available.Total>0;
                Add(actions,"VIGILANTES · ETAPA "+next,()=>{
                    Send("VigilantBattle",next.ToString());
                    OpenChallenge();
                },enabled);
                Add(actions,"RECLAMAR EXPERIENCIA PASIVA",()=>{
                    Send("ClaimVigilantXp","aldric");
                    OpenChallenge();
                },s.Retention!=null&&s.Retention.LastClearedStage>0);
            }
            if(scouts && s.Retention?.Orders!=null)
            {
                foreach(var order in s.Retention.Orders.Where(x=>x!=null&&!x.Claimed).Take(6))
                {
                    string id=order.Id;
                    bool dispatched=order.StartedUtcTicks>0;
                    bool ended=dispatched&&order.EndsUtcTicks<=DateTime.UtcNow.Ticks;
                    Add(actions,(dispatched?(ended?"RECOGER ":"EN MARCHA "):"ENVIAR ")+id,()=>{
                        Send(dispatched?"ScoutClaim":"ScoutDispatch",id);
                        OpenChallenge();
                    },!dispatched||ended);
                }
            }
            Open("VIGILANTES Y EXPLORADORES",
                "Vigilantes: "+(vigilants?"desbloqueado":"bloqueado por la historia")+
                " · Exploradores: "+(scouts?"desbloqueado":"bloqueado")+
                "\nEstos desafíos utilizan las tropas, requisitos y recompensas del motor existente.",
                actions);
            openedArea="challenge";
        }

        private void ShowNicknamePrompt()
        {
            if(gateway.Snapshot().PlayerName?.Length>0)return;
            Open("NOMBRE DE TU REINO",
                "Elige cómo te reconocerán los demás jugadores. No cambia el identificador de tu partida.",
                new List<(string,Action,bool)>{
                    ("CONFIRMAR NOMBRE",()=>{
                        var name=playerNickname?.text??"";
                        Send("ChoosePlayerName",name);
                        if(!string.IsNullOrWhiteSpace(gateway.Snapshot().PlayerName))
                        {popup.gameObject.SetActive(false);playerNickname=null;}
                    },true)
                });
            // Reserve a dedicated input row ABOVE the confirm button.
            // Direct RectTransform positioning avoids the previous squeezed
            // one-pixel field caused by competing LayoutGroup constraints.
            var confirm=popupActionArea.Cast<Transform>().FirstOrDefault(x=>x.name=="CONFIRMAR NOMBRE");
            if(confirm!=null)
            {
                var actionRect=(RectTransform)confirm;
                actionRect.offsetMin+=new Vector2(0,-58);
                actionRect.offsetMax+=new Vector2(0,-58);
            }
            var r=Node(popupActionArea,"Player name field",
                new Vector2(0,1),new Vector2(1,1),
                new Vector2(0,-52),new Vector2(0,0));
            r.SetAsFirstSibling();
            var rowImage=Plate(r,secondary,Color.white);
            var input=r.gameObject.AddComponent<InputField>();
            input.targetGraphic=rowImage;
            input.characterLimit=20;
            var value=Label(r,"Value",17,TextAnchor.MiddleLeft,Ivory,
                Vector2.zero,Vector2.one,new Vector2(18,3),new Vector2(-18,-3));
            input.textComponent=value;
            var hint=Label(r,"Placeholder",14,TextAnchor.MiddleLeft,Muted,
                Vector2.zero,Vector2.one,new Vector2(18,3),new Vector2(-18,-3));
            hint.text="Escribe tu nombre";
            input.placeholder=hint;
            playerNickname=input;
            SizeSheet(2);
            Canvas.ForceUpdateCanvases();
        }

        private void OpenPickedTarget(AshenCitadelPickTarget target)
        {
            if(target==null || popup.gameObject.activeInHierarchy)return;
            if(target.Kind=="building")
            {
                var s=gateway.Snapshot();
                var id=target.TargetId;
                int level=id=="bastion"?s.BastionLevel:
                    id=="sawmill"?s.SawmillLevel:
                    id=="barracks"?s.BarracksLevel:
                    id=="granary"?(s.ChapterThree?.GranaryLevel??0):
                    id=="stoneworks"?(s.ChapterThree?.StoneworksLevel??0):0;
                var actions=new List<(string,Action,bool)>();
                if(id=="bastion"){OpenSection(0);return;}
                if(id=="barracks")
                    Add(actions,"ENTRENAR ARQUEROS",()=>OpenSection(2),level>0);
                if(level>0&&level<s.BastionLevel)
                    Add(actions,"MEJORAR "+target.DisplayName,()=>{
                        Send("UpgradeBuilding",id);
                        OpenPickedTarget(target);
                    });
                Add(actions,"VER EDIFICIOS",()=>OpenSection(1));
                Open(target.DisplayName+" · NIV. "+level,
                    "Edificio real: "+target.DisplayName+". La disponibilidad de mejoras y la producción " +
                    "se verifican mediante las reglas del Bastión.",actions);
                return;
            }
            if(target.Kind=="gather")
            {
                Open(target.DisplayName,
                    "La expedición reservará tropas y recursos siguiendo las reglas existentes.",
                    new List<(string,Action,bool)>{
                        ("ENVIAR MARCHA",()=>Send("Gather",target.TargetId),true),
                        ("VOLVER",()=>OpenSection(3),true)
                    });
                return;
            }
            if(target.Kind=="fight")
            {
                Open(target.DisplayName,
                    "Los resultados dependen de la composición, enemigos y reglas reales. No es una animación falsa.",
                    new List<(string,Action,bool)>{
                        ("ATACAR",()=>Send("Fight",target.TargetId),true),
                        ("VOLVER",()=>OpenSection(3),true)
                    });
                return;
            }
            if(target.Kind=="fissure"){OpenSection(3);return;}
        }

        private void ProcessSceneTap()
        {
            if(playerCamera==null||popup==null||popup.gameObject.activeInHierarchy)return;
            Vector2 position;
            var touch=Touchscreen.current;
            var mouse=Mouse.current;
            if(touch!=null&&touch.primaryTouch.press.wasReleasedThisFrame)
                position=touch.primaryTouch.position.ReadValue();
            else if(mouse!=null&&mouse.leftButton.wasReleasedThisFrame)
                position=mouse.position.ReadValue();
            else return;

            if(EventSystem.current!=null)
            {
                var eventData=new PointerEventData(EventSystem.current){position=position};
                var hits=new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData,hits);
                if(hits.Count>0)return;
            }
            var ray=playerCamera.ScreenPointToRay(position);
            if(!Physics.Raycast(ray,out var hit,300f,~0,QueryTriggerInteraction.Collide))return;
            var target=hit.collider.GetComponentInParent<AshenCitadelPickTarget>();
            if(target!=null)OpenPickedTarget(target);
        }
    }
}

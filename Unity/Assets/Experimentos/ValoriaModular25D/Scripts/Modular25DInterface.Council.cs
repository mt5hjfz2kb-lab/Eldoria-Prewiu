using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    public sealed partial class Modular25DInterface
    {
        RectTransform orderList;
        readonly List<Button> orderRows = new List<Button>();
        readonly List<(string caption, string command, string target)> orders =
            new List<(string, string, string)>();

        void InitializeCouncil()
        {
            var header = stateSummary.rectTransform;
            header.anchorMin = header.anchorMax = new Vector2(.5f, 1f);
            header.pivot = new Vector2(.5f, 1f);
            header.anchoredPosition = new Vector2(0f, -39f);
            header.sizeDelta = new Vector2(268f, 43f);
            stateSummary.alignment = TextAnchor.MiddleCenter;
            var view = Plate("Órdenes disponibles", statusSheet, null, Vector2.zero,
                Vector2.zero, Vector2.zero, Vector2.zero);
            view.anchorMax = Vector2.one;
            view.offsetMin = new Vector2(18f, 17f);
            view.offsetMax = new Vector2(-18f, -88f);
            view.GetComponent<Image>().color = Color.clear;
            view.GetComponent<Image>().raycastTarget = true;
            view.gameObject.AddComponent<RectMask2D>();
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.viewport = view;
            orderList = new GameObject("Órdenes", typeof(RectTransform)).GetComponent<RectTransform>();
            orderList.SetParent(view, false);
            orderList.anchorMin = new Vector2(0, 1);
            orderList.anchorMax = Vector2.one;
            orderList.pivot = new Vector2(.5f, 1);
            orderList.anchoredPosition = Vector2.zero;
            scroll.content = orderList;
        }

        void RefreshCouncil(PlayerState s)
        {
            orders.Clear();
            var c = s.ChapterThree;
            int level = s.BastionLevel;
            long now = DateTime.UtcNow.Ticks;
            bool workFree = s.BuildingCompletesUtcTicks == 0 && s.BastionUpgradeCompletesUtcTicks == 0;
            bool canMarch = MarchCapacityRules.CanStart(s, now) && s.Available.Total > 0;
            bool Away(string target)
            {
                foreach (var m in MarchCapacityRules.WorldMarches(s))
                    if (m.Phase != "idle" && m.TargetId == target) return true;
                return false;
            }
            void Order(string caption, string command, string target) => orders.Add((caption, command, target));
            void Building(string name, string target, int built, int unlock)
            {
                if (!workFree || level < unlock || built >= level || built >= 5) return;
                string selection = target == "barracks" && built > 0 ? "upgrade-barracks" : target;
                var offer = gateway.BuildingOffer(selection,s.Revision);
                if (offer?.Action == null || !offer.Action.Enabled) return;
                string price = offer.Costs.Replace("Coste de la acción\n", "");
                Order((built == 0 ? "Construir " : "Mejorar ") + name + " · " + (built + 1) +
                    "\n" + price, offer.Action.Kind, offer.Action.Target);
            }
            string summary = "Bastión " + level;
            string objective = SliceRules.CurrentObjectiveKey(s);
            var chapter = s.ChapterProgress;
            if (level == 1 && !s.JourneyComplete)
            {
                if (s.SawmillLevel == 0) summary = "Construye el aserradero";
                else if (objective == "b1.gather-wood") summary = "Madera recogida · " + chapter.GatheredWood + " / " + SliceContentProfiles.Active.Chapter1GatherWood;
                else if (objective == "b1.gather-stone") summary = "Piedra recogida · " + chapter.GatheredStone + " / " + SliceContentProfiles.Active.Chapter1GatherStone;
                else summary = "Despeja la ruta y espera el regreso de Aldric";
            }
            if (level < 5)
            {
                var next = BastionProgressionCatalog.ForLevel(level + 1);
                string missing = BastionProgressionCatalog.MissingSummary(s, next);
                if (level > 1 || s.JourneyComplete) summary = missing.Length > 0 ? missing : "Ascenso disponible";
                if (BastionProgressionCatalog.RequirementsMet(s, next))
                    Order("Ascender a Bastión " + next.Level, "AdvanceBastion", "bastion");
            }
            long deadline = Math.Max(s.BuildingCompletesUtcTicks, s.BastionUpgradeCompletesUtcTicks);
            if (deadline > now) summary = "Obra en curso · " + TimeSpan.FromTicks(deadline - now).ToString(@"hh\:mm\:ss");
            stateSummary.text = summary;
            Building("aserradero", "sawmill", s.SawmillLevel, 1);
            Building("cuartel", "barracks", s.BarracksLevel, 2);
            Building("granero", "granary", c.GranaryLevel, 3);
            Building("cantera", "stoneworks", c.StoneworksLevel, 5);
            if (level >= 2 && s.BarracksLevel > 0)
            {
                int count = Math.Min(SliceRules.RecruitArchers, SliceRules.MaxAffordableArchers(s));
                if (count > 0 && s.RecruitmentCompletesUtcTicks == 0)
                    Order("Entrenar " + count + " arqueros", "Recruit", "archer:t1:" + count);
                if (canMarch && MarchCapacityRules.HeroAvailable(s, "aldric"))
                    Order("Preparar marcha de Aldric", "ConfigureMarch", "march-main");
            }
            if (canMarch)
            {
                if (s.ForestRemaining > 0) Order("Recoger madera", "Gather", "forest-valoria");
                if (s.QuarryRemaining > 0) Order("Recoger piedra", "Gather", "quarry-valoria");
                if (!s.ScoutDefeated && !Away("corrupt-scout")) Order("Despejar la ruta", "Fight", "corrupt-scout");
                if (s.MarchConfigured && !s.EngendroDefeated && !Away("engendro-valoria") &&
                    s.ChapterProgress.TrainedArchers >= SliceContentProfiles.Active.Chapter2TrainArchers &&
                    SliceRules.Expedition(s.PreparedTroops,s.PreparedHeroId).Power >= SliceContentProfiles.Active.Chapter2ExpeditionPower)
                    Order("Combatir al Engendro", "Fight", "engendro-valoria");
                if (level >= 3)
                {
                    if (c.MeatRemaining > 0 && !Away("meat-valoria")) Order("Abastecer de comida", "Gather", "meat-valoria");
                    if (c.WolfRespawnUtcTicks <= now && !Away("hunt-wolf")) Order("Cazar lobos", "Fight", "hunt-wolf");
                    if (c.BoarRespawnUtcTicks <= now && !Away("hunt-boar")) Order("Cazar jabalí", "Fight", "hunt-boar");
                    if (!c.FissureExplored && !c.FissureClosed && c.InvestigationEndsUtcTicks == 0)
                        Order("Investigar la fisura", "InvestigateFissure", "fissure-valoria");
                    if (c.LyraContacted && !c.FissureClosed && !Away("fissure-valoria"))
                        Order("Asaltar la fisura", "Fight", "fissure-valoria");
                }
            }
            if (c.FissureExplored && !c.LyraContacted) Order("Hablar con Lyra", "ContactLyra", "fissure-valoria");
            if (level >= 4 && string.IsNullOrEmpty(c.DevelopmentPriority))
            {
                Order("Priorizar producción", "ChooseDevelopment", "production");
                Order("Priorizar defensa", "ChooseDevelopment", "defense");
                Order("Priorizar desarrollo civil", "ChooseDevelopment", "civil");
            }
            if (s.ResourceRewards != null)
            {
                var mail = new HashSet<string>();
                foreach (var p in s.ResourceRewards.Packets)
                {
                    if (p.Used) continue;
                    if (!p.Collected)
                    {
                        if (mail.Add(p.MailEventId)) Order("Recoger botín del buzón", ResourceRewardCommands.ClaimMail, p.MailEventId);
                    }
                    else Order("Usar " + p.Amount + " de " + (p.Kind == "wood" ? "madera" : p.Kind == "stone" ? "piedra" : "comida"),
                        ResourceRewardCommands.UsePacket, p.ReceiptId);
                }
            }
            orderList.sizeDelta = new Vector2(0f, orders.Count * 50f);
            for (int i = 0; i < orders.Count; i++)
            {
                if (i == orderRows.Count)
                {
                    int slot = i;
                    var row = ButtonPlate("Orden " + i, orderList, "", slatePlate,
                        new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -50f*i),
                        new Vector2(245f, 46f), () => ExecuteOrder(slot));
                    orderRows.Add(row);
                }
                orderRows[i].gameObject.SetActive(true);
                orderRows[i].GetComponentInChildren<Text>().text = orders[i].caption;
            }
            for (int i = orders.Count; i < orderRows.Count; i++) orderRows[i].gameObject.SetActive(false);
        }

        public void OpenDistrict(string target)
        {
            OpenStatus();
            int row = orders.FindIndex(order => order.target == target);
            if (row < 0 && target == "barracks")
                row = orders.FindIndex(order => order.command == "Recruit");
            if (row < 0) return;
            var scroll = orderList.GetComponentInParent<ScrollRect>();
            Canvas.ForceUpdateCanvases();
            scroll.StopMovement();
            orderList.anchoredPosition = new Vector2(0, Mathf.Clamp(row * 50f, 0,
                Mathf.Max(0, orderList.rect.height - scroll.viewport.rect.height)));
        }

        void ExecuteOrder(int slot)
        {
            if (slot >= orders.Count) return;
            var order = orders[slot];
            gateway.Issue(order.command, order.target);
        }
    }
}

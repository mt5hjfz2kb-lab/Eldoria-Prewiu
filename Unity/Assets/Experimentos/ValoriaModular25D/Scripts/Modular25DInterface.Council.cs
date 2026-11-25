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

        Text councilTitle;
        string selectedBuilding;

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
            if (selectedBuilding == null) AddGuideOrders(s);
            else AddBuildingOrders(s);
            RefreshOrderRows();
        }

        void AddBuildingOrders(PlayerState s)
        {
            string selection = selectedBuilding == "barracks" && s.BarracksLevel > 0
                ? "upgrade-barracks" : selectedBuilding;
            var offer = gateway.BuildingOffer(selection, s.Revision);
            if (offer == null) return;
            councilTitle.text = offer.Name.ToUpperInvariant() + " · " + offer.Level;
            bool bastion = selectedBuilding == "bastion";
            var action = offer.Action;
            stateSummary.text = offer.OperationActive ? offer.State + " · " + offer.Time :
                !string.IsNullOrEmpty(offer.Requirements) ? offer.Requirements :
                bastion && offer.Level >= 5 ? "Nivel máximo" : offer.State;
            string label = offer.Level > 0 ? "Mejorar" : "Construir";
            if (bastion && offer.Level < 5) label += " a Bastión " + (offer.Level + 1);
            if (!string.IsNullOrEmpty(offer.Costs) && !offer.OperationActive)
                label += "\n" + offer.Costs.Replace("Coste de la acción\n", "");
            orders.Add((label, action != null && action.Enabled ? action.Kind : "", selectedBuilding));
            if (selectedBuilding == "barracks" && s.BarracksLevel > 0 &&
                Eldoria.ReferenceClean.ValoriaTravelSession.CombinedPlayer)
                orders.Add(("Entrenar tropas", "OpenSystem", "training"));
        }

        void AddGuideOrders(PlayerState s)
        {
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
                Order((built == 0 ? "Construir " : "Mejorar ") + name + " ›", "VisitBuilding", target);
            }
            string summary = gateway.Session.Read(null, false).Objective;
            if (level < 5 && BastionProgressionCatalog.RequirementsMet(s, BastionProgressionCatalog.ForLevel(level + 1)))
                Order("Mejorar el Bastión ›", "VisitBuilding", "bastion");
            long deadline = Math.Max(s.BuildingCompletesUtcTicks, s.BastionUpgradeCompletesUtcTicks);
            if (deadline > now) summary = "Obra en curso · " + TimeSpan.FromTicks(deadline - now).ToString(@"hh\:mm\:ss");
            councilTitle.text = "CONSEJO DE VALORIA";
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
            if (Eldoria.ReferenceClean.ValoriaTravelSession.CombinedPlayer)
            {
                if (s.BarracksLevel > 0 && level >= 2)
                {
                    Order("Gestionar entrenamiento", "OpenSystem", "training");
                    if (NarrativeCoherenceRules.Seen(s,"march-preparation"))
                        Order("Componer marcha", "OpenSystem", "march");
                }
                if (c.LyraRecruited) Order("Salón de héroes", "OpenSystem", "heroes");
                if (EarlyRetentionRules.VigilantsUnlocked(s)) Order("Vigilantes", "OpenSystem", "vigilants");
                if (EarlyRetentionRules.ScoutsUnlocked(s)) Order("Exploradores", "OpenSystem", "scouts");
                if ((s.Mailbox?.ReceivedEventIds?.Count ?? 0) > 0 || (s.Mailbox?.Messages?.Count ?? 0) > 0)
                    Order("Abrir buzón", "OpenSystem", "mail");
                if ((s.ResourceRewards?.Packets?.Count ?? 0) > 0) Order("Abrir bolsa", "OpenSystem", "bag");
                if ((c.Inventory?.Count ?? 0) > 0) Order("Materiales y manuscritos", "Inventory", "inventory");
                Order("Tesoros de Valoria", "OpenSystem", "daily");
            }
        }

        void RefreshOrderRows()
        {
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
                orderRows[i].interactable = orders[i].command.Length > 0;
                orderRows[i].GetComponentInChildren<Text>().text = orders[i].caption;
            }
            for (int i = orders.Count; i < orderRows.Count; i++) orderRows[i].gameObject.SetActive(false);
        }

        // World building taps keep their established contextual entry point.
        // The bottom navigation itself stays identical to main.
        public void OpenDistrict(string target)
        {
            if (target == "sawmill") OpenSawmill();
            else ShowCityPanel(target);
        }

        void ExecuteOrder(int slot)
        {
            if (slot >= orders.Count) return;
            var order = orders[slot];
            if (order.command == "VisitBuilding") OpenDistrict(order.target);
            else if (order.command == "Inventory") OpenCanonicalRoom("bag");
            else if (order.command.Length == 0) return;
            else if (order.command == "OpenSystem") OpenCanonicalRoom(order.target);
            else gateway.Issue(order.command, order.target);
        }
    }
}


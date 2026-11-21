using System;
using Eldoria.DailyTreasures;
using Eldoria.Domain;
using Eldoria.HeroesUI;
using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    public sealed partial class Modular25DInterface
    {
        RoyalHUD roomHost;
        Func<bool> roomIsOpen;
        string activeRoom;

        void OpenCanonicalRoom(string destination)
        {
            if (roomHost || !world.ManagementHudPrefab || gateway.Commands == null) return;
            activeRoom = destination;
            try
            {
                roomHost = Instantiate(world.ManagementHudPrefab).GetComponent<RoyalHUD>();
                var canvas = roomHost.GetComponent<Canvas>();
                canvas.sortingOrder = hudCanvas.sortingOrder + 1;
                var safe = (RectTransform)roomHost.GetComponentInChildren<RoyalHudLayout>(true).transform;
                roomHost.Bind(gateway.Session);
                // The prefab provides the real controls, scaling and modal dependencies.
                // Corona remains the navigation owner and the room draws above it.
                roomHost.enabled = false;
                foreach (Transform child in safe) child.gameObject.SetActive(false);
                Action changed = () => Refresh(gateway.Snapshot());
                Func<long> now = () => DateTime.UtcNow.Ticks;
                ClosePanels();
                hudCanvas.enabled = false;
                GetComponent<Modular25DCameraInput>().enabled = false;
                switch (destination)
                {
                    case "heroes":
                        var heroes = roomHost.GetComponent<HeroesApprovedDestination>() ?? roomHost.gameObject.AddComponent<HeroesApprovedDestination>();
                        heroes.Initialize(safe,() => {
                            var s = gateway.Snapshot();
                            return new[]{ ValoriaIntegratedWorld.ReadCanonicalHero(s,"aldric"),
                                ValoriaIntegratedWorld.ReadCanonicalHero(s,"lyra") };
                        });
                        heroes.CanOpenDestination = () => gateway.Snapshot().ChapterThree.LyraRecruited;
                        heroes.CanOpenHero = id => id=="aldric" || id=="lyra" && heroes.CanOpenDestination();
                        var lyra = roomHost.GetComponent<HeroesLyraModel>() ?? roomHost.gameObject.AddComponent<HeroesLyraModel>();
                        lyra.Loaded = heroes.Refresh;
                        heroes.ReadModelRoot = id => {
                            if (id!="lyra") return null;
                            lyra.Request(); return lyra.Ready ? lyra.ModelRoot : null;
                        };
                        heroes.Open();
                        roomIsOpen = () => heroes.IsOpen;
                        break;
                    case "daily":
                        var chest = DailyTreasurePanel.Install(safe);
                        chest.Bind(gateway.Commands,now,changed);
                        chest.Open();
                        roomIsOpen = () => chest.IsOpen;
                        break;
                    case "scouts":
                    case "vigilants":
                        var retention = roomHost.GetComponent<RoyalGoldV3EarlyRetentionScreens>() ?? roomHost.gameObject.AddComponent<RoyalGoldV3EarlyRetentionScreens>();
                        retention.Initialize(roomHost,gateway.Commands,now,changed);
                        retention.Open(destination=="scouts" ? RoyalGoldV3EarlyRetentionScreens.Destination.Scouts :
                            RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants);
                        roomIsOpen = () => retention.IsOpen;
                        break;
                    case "mail":
                    case "bag":
                        var mail = roomHost.GetComponent<RoyalGoldV41MailboxScreen>() ?? roomHost.gameObject.AddComponent<RoyalGoldV41MailboxScreen>();
                        mail.Initialize(roomHost,gateway.Commands);
                        var bag = roomHost.GetComponent<Issue186BagOverlay>();
                        if (destination=="mail") mail.Open();
                        else { roomHost.Navigation.ChronicleLayout=false; bag.Open(); }
                        roomIsOpen = () => mail.IsOpen || bag.IsOpen;
                        break;
                    case "training":
                    case "march":
                        var military = roomHost.GetComponent<RoyalGoldV3ManagementScreens>() ?? roomHost.gameObject.AddComponent<RoyalGoldV3ManagementScreens>();
                        military.Initialize(roomHost,gateway.Commands,now,changed);
                        military.Open(destination=="training" ? RoyalGoldV3ManagementScreens.Destination.Training :
                            RoyalGoldV3ManagementScreens.Destination.March);
                        roomIsOpen = () => military.IsOpen;
                        break;
                }
            }
            finally
            {
                if (roomIsOpen == null || !roomIsOpen()) CloseCanonicalRoom();
            }
        }

        void CloseCanonicalRoom()
        {
            activeRoom = null;
            roomIsOpen = null;
            if (roomHost) { roomHost.gameObject.SetActive(false); Destroy(roomHost.gameObject); }
            roomHost = null;
            hudCanvas.enabled = true;
            GetComponent<Modular25DCameraInput>().enabled = true;
            Refresh(gateway.Snapshot());
        }
    }
}

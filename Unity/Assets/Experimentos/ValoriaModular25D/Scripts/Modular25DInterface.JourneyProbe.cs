#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    // Read-only, opt-in telemetry for real pointer tests on localhost. No test
    // commands, timer controls, state injection or JavaScript bridge in the game.
    public sealed partial class Modular25DInterface
    {
        bool localJourneyProbe;
        [Serializable] sealed class Tap { public string name; public float x, y; }
        [Serializable] sealed class JourneyView
        {
            public int level, sawmill, barracks, granary, stoneworks, wood, stone, food, army;
            public long revision;
            public int forestRemaining,quarryRemaining;
            public string room, activeScene;
            public bool travelCurtainVisible, handVisible, sawmillClockVisible;
            public bool journey, lyra, fissure, council, sawmillPanel, drawer, busy;
            public List<Tap> taps = new List<Tap>();
        }

        void ReportJourneyControls()
        {
            var s = gateway.Snapshot();
            if (s == null) return;
            var view = new JourneyView {
                room=activeRoom,
                handVisible=firstBuildGuide && firstBuildGuide.gameObject.activeInHierarchy,
                sawmillClockVisible=worldProgressTrack && worldProgressTrack.gameObject.activeInHierarchy && sawmillTagText.text.StartsWith("OBRA"),
                activeScene=UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                travelCurtainVisible=GameObject.Find("Paso entre Valoria y la Región") != null,
                level=s.BastionLevel, sawmill=s.SawmillLevel, barracks=s.BarracksLevel,
                granary=s.ChapterThree.GranaryLevel, stoneworks=s.ChapterThree.StoneworksLevel,
                wood=s.Resources.Wood, stone=s.Resources.Stone, food=s.Resources.Food,
                forestRemaining=s.ForestRemaining,quarryRemaining=s.QuarryRemaining,
                army=s.Available.Total, revision=s.Revision, journey=s.JourneyComplete,
                lyra=s.ChapterThree.LyraRecruited, fissure=s.ChapterThree.FissureClosed,
                busy=s.BuildingCompletesUtcTicks>0 || s.BastionUpgradeCompletesUtcTicks>0 ||
                     s.RecruitmentCompletesUtcTicks>0 || MarchCapacityRules.ActiveCount(s,DateTime.UtcNow.Ticks)>0,
                council=statusSheet.gameObject.activeSelf, sawmillPanel=buildingSheet.activeSelf, drawer=activityExpanded
            };
            var buttons = roomHost ? roomHost.GetComponentsInChildren<Button>() : hudCanvas.GetComponentsInChildren<Button>();
            foreach (var b in buttons)
            {
                if (!b.interactable || !b.gameObject.activeInHierarchy) continue;
                var r=(RectTransform)b.transform;
                var point=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));
                var mask=b.GetComponentInParent<RectMask2D>();
                if (mask && !RectTransformUtility.RectangleContainsScreenPoint(mask.rectTransform,point)) continue;
                string name=b.name;
                if (r.parent == orderList) {
                    var order=orders[r.GetSiblingIndex()];
                    name=order.command+"/"+order.target;
                }
                view.taps.Add(new Tap {name=name, x=point.x/Screen.width, y=1f-point.y/Screen.height});
            }
            Debug.Log("VALORIA_JOURNEY "+JsonUtility.ToJson(view));
        }
    }
}
#endif


using System;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.HeroesUI
{
    /// <summary>
    /// Integration boundary for the owner-approved Heroes interface. Mount on the existing
    /// Royal HUD safe area, never a second canvas. The host supplies authoritative data.
    /// No local save, gameplay state, currency, or simulated upgrades are created here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HeroesApprovedDestination : MonoBehaviour
    {
        [Serializable] public struct HeroState
        {
            public string Id, DisplayName, Role, AbilityName, AbilityDescription;
            public int Level, Health, Attack, Defense, Break, HeroPower;
            public bool Unlocked;
            public Sprite Portrait;
        }
        public enum Page { Salon, Resumen, Habilidades, Talentos, Equipamiento }
        public Func<HeroState[]> ReadHeroes;
        public Func<string, bool> CanOpenHero;
        public Action<string> HeroViewed;
        public Action Closed;
        public RectTransform ExistingHudSafeArea;
        public Sprite[] ApprovedReferenceScreens = new Sprite[6];
        public GameObject ApprovedAldricModel;
        public GameObject ApprovedLyraModel;
        public Page CurrentPage { get; private set; } = Page.Salon;
        public string CurrentHeroId { get; private set; }
        public bool IsOpen => _root != null && _root.gameObject.activeSelf;
        RectTransform _root;
        Image _image;
        public void Initialize(RectTransform safeArea, Func<HeroState[]> readHeroes)
        {
            if (safeArea == null || readHeroes == null)
                throw new ArgumentNullException(safeArea == null ? nameof(safeArea) : nameof(readHeroes));
            ExistingHudSafeArea = safeArea;
            ReadHeroes = readHeroes;
            if (_root != null) return;
            var go = new GameObject("Heroes Approved UI Destination", typeof(RectTransform), typeof(Image));
            _root = go.GetComponent<RectTransform>();
            _root.SetParent(safeArea, false);
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.one;
            _root.offsetMin = _root.offsetMax = Vector2.zero;
            _image = go.GetComponent<Image>();
            _image.preserveAspect = true;
            _image.raycastTarget = false;
            go.SetActive(false);
        }
        public void Open()
        {
            if (_root == null) throw new InvalidOperationException("Initialize on canonical Royal HUD first");
            CurrentPage = Page.Salon;
            CurrentHeroId = null;
            _root.gameObject.SetActive(true);
            RefreshReference();
        }
        public bool OpenHero(string id)
        {
            if (!IsOpen || string.IsNullOrEmpty(id)) return false;
            var heroes = ReadHeroes?.Invoke();
            if (heroes == null) return false;
            bool found = false;
            foreach (var hero in heroes)
                if (hero.Id == id && hero.Unlocked) { found = true; break; }
            if (!found || (CanOpenHero != null && !CanOpenHero(id))) return false;
            CurrentHeroId = id;
            CurrentPage = Page.Resumen;
            HeroViewed?.Invoke(id);
            RefreshReference();
            return true;
        }
        public void Select(Page page)
        {
            if (!IsOpen || (page != Page.Salon && string.IsNullOrEmpty(CurrentHeroId))) return;
            CurrentPage = page;
            RefreshReference();
        }
        public void Close()
        {
            if (!IsOpen) return;
            _root.gameObject.SetActive(false);
            Closed?.Invoke();
        }
        public HeroState? CurrentAuthoritativeHero()
        {
            foreach (var hero in ReadHeroes?.Invoke() ?? Array.Empty<HeroState>())
                if (hero.Id == CurrentHeroId) return hero;
            return null;
        }
        void RefreshReference()
        {
            // This reference layer is deliberately NOT a finished interactive UI:
            // mockup values and illustrations must be replaced by live Unity widgets.
            int i = CurrentPage == Page.Salon ? 0 :
                CurrentHeroId == "lyra" ? 5 :
                CurrentPage == Page.Resumen ? 1 :
                CurrentPage == Page.Habilidades ? 2 :
                CurrentPage == Page.Talentos ? 3 : 4;
            _image.sprite = ApprovedReferenceScreens != null && i < ApprovedReferenceScreens.Length
                ? ApprovedReferenceScreens[i] : null;
            _image.color = _image.sprite == null ? Color.clear : Color.white;
        }
    }
}

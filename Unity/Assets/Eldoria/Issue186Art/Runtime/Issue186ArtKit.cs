using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Issue186Art
{
    // Independent, opt-in visual adapter. Never creates a second HUD or EventSystem.
    // It does NOT credit resources: only LocalGateway may eventually mutate PlayerState.
    public static class Issue186ArtKit
    {
        const string Prefix = "Issue186ResourceArt/";
        public static Sprite PacketSprite(string kind, int amount)
        {
            var key = ResourceRewardRules.SpriteKey(kind, amount);
            return key == null ? null : Resources.Load<Sprite>(key);
        }

        public static bool BindPacket(Image icon, Text quantity, string kind, int amount)
        {
            if (!icon || !ResourceRewardRules.IsStandardTier(amount))
                return false;
            Sprite sprite = PacketSprite(kind, amount);
            if (!sprite) return false; // Missing art is NOT an excuse for fake items.
            icon.sprite = sprite;
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            if (quantity)
                quantity.text = ResourceRewardRules.TierLabel(amount);
            return true;
        }

        // New = information to read; ActionReady = genuinely claimable action.
        // Never clear a claimable alert merely because the panel was opened.
        public static bool BindAttention(Image indicator, AttentionLevel level)
        {
            if (!indicator) return false;
            if (level == AttentionLevel.None)
            {
                indicator.gameObject.SetActive(false);
                return true;
            }
            string key = level == AttentionLevel.New ? "badge_new" :
                         level == AttentionLevel.ActionReady ? "badge_claimable" : null;
            var sprite = key == null ? null : Resources.Load<Sprite>(Prefix + key);
            if (!sprite)
            {
                indicator.gameObject.SetActive(false);
                return false;
            }
            indicator.sprite = sprite;
            indicator.type = Image.Type.Simple;
            indicator.preserveAspect = true;
            indicator.raycastTarget = false;
            indicator.gameObject.SetActive(true);
            return true;
        }

        // Both pointer and halo must be Image components already belonging to
        // an existing Canvas. This method does not create a Canvas/EventSystem.
        // The control is resolved by the actual UI owner, not hardcoded coords.
        public static bool BindFirstUsePointer(
            GuidedTutorialProgress progress, string stepId, string controlId,
            bool featureUnlocked, Canvas canvas, RectTransform actualControl,
            Image pointer, Image targetHalo)
        {
            bool visible = actualControl && actualControl.gameObject.activeInHierarchy
                && actualControl.rect.width > 0 && actualControl.rect.height > 0;
            bool shouldPoint = GuidedTutorialRules.ShouldPoint(
                progress, stepId, controlId, featureUnlocked, visible);
            if (!shouldPoint || !canvas || !canvas.isActiveAndEnabled ||
                !pointer || !pointer.rectTransform.parent)
            {
                if (pointer) pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }

            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : canvas.worldCamera;
            Vector3 centerWorld = actualControl.TransformPoint(actualControl.rect.center);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, centerWorld);
            // Camera render-target dimensions are authoritative during
            // disposable Unity captures; Screen.width can be the host editor
            // window rather than this actual mobile-sized render texture.
            int viewWidth = camera && camera.targetTexture
                ? camera.targetTexture.width : Screen.width;
            int viewHeight = camera && camera.targetTexture
                ? camera.targetTexture.height : Screen.height;
            if (screenPoint.x < 0 || screenPoint.x > viewWidth ||
                screenPoint.y < 0 || screenPoint.y > viewHeight)
            {
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            // Do not place a fake tap marker over a non-interactive component.
            if (!actualControl.GetComponentInParent<Selectable>() ||
                !actualControl.GetComponentInParent<Selectable>().IsInteractable())
            {
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            var parent = pointer.rectTransform.parent as RectTransform;
            if (!parent || !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, screenPoint, camera, out var local))
            {
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            var sprite = Resources.Load<Sprite>(Prefix + "tutorial_pointing_hand");
            if (!sprite)
            {
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            var rect = pointer.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            // Fingertip anchor for the authored right-pointing gauntlet.
            rect.pivot = new Vector2(.82f, .59f);
            rect.anchoredPosition = local;
            pointer.sprite = sprite;
            pointer.type = Image.Type.Simple;
            pointer.preserveAspect = true;
            pointer.raycastTarget = false;
            pointer.gameObject.SetActive(true);

            if (targetHalo)
            {
                var halo = Resources.Load<Sprite>(Prefix + "tutorial_target_halo");
                var holder = targetHalo.rectTransform.parent as RectTransform;
                if (halo && holder &&
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        holder, screenPoint, camera, out var haloPoint))
                {
                    var h = targetHalo.rectTransform;
                    h.anchorMin = h.anchorMax = new Vector2(.5f, .5f);
                    h.pivot = new Vector2(.5f, .5f);
                    h.anchoredPosition = haloPoint;
                    targetHalo.sprite = halo;
                    targetHalo.color = new Color(1f, 1f, 1f, .22f);
                    targetHalo.type = Image.Type.Simple;
                    targetHalo.preserveAspect = true;
                    targetHalo.raycastTarget = false;
                    targetHalo.gameObject.SetActive(true);
                }
                else targetHalo.gameObject.SetActive(false);
            }
            return true;
        }
    }
}

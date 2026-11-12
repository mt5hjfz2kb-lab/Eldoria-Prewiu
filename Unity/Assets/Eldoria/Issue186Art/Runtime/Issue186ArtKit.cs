using System;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
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
            // Recycler slots can hold yesterday's icon. A missing/invalid
            // reward must NEVER silently display the previous resource, even
            // if its backing Sprite is valid. Do not disable the GameObject:
            // the same slot may also own its Button and reusable layout.
            Sprite sprite = icon && amount > 0 ? PacketSprite(kind, amount) : null;
            if (!sprite)
            {
                if (icon)
                {
                    icon.sprite = null;
                    icon.enabled = false;
                    icon.raycastTarget = false;
                }
                if (quantity) quantity.text = string.Empty;
                return false;
            }
            icon.sprite = sprite;
            icon.enabled = true;
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            if (quantity)
                quantity.text = ResourceRewardRules.AmountLabel(amount);
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
        // Native batch diagnostic; never bypass a blocking Graphic.
        public static string LastPointerRejectReason { get; private set; } = "";

        public static bool BindFirstUsePointer(
            GuidedTutorialProgress progress, string stepId, string controlId,
            bool featureUnlocked, Canvas canvas, RectTransform actualControl,
            Image pointer, Image targetHalo)
        {
            LastPointerRejectReason = "not-reached";
            bool visible = actualControl && actualControl.gameObject.activeInHierarchy
                && actualControl.rect.width > 0 && actualControl.rect.height > 0;
            bool shouldPoint = GuidedTutorialRules.ShouldPoint(
                progress, stepId, controlId, featureUnlocked, visible);
            if (!shouldPoint || !canvas || !canvas.isActiveAndEnabled ||
                !pointer || !pointer.rectTransform.parent)
            {
                LastPointerRejectReason = "step-hidden-or-canvas-missing";
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
                LastPointerRejectReason = "control-outside-render-target:" + screenPoint;
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            // Only the exact target may receive the tap: a selectable parent
            // does not make an arbitrary child a valid tutorial button.
            var selectable = actualControl.GetComponent<Selectable>();
            if (!selectable || !selectable.IsInteractable())
            {
                LastPointerRejectReason = "control-is-not-interactable";
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            // A visible/interactable Selectable can still sit BEHIND a modal
            // overlay. In that state a tutorial pointer is misleading: the
            // action cannot be performed. Raycast through the active EventSystem
            // (all canvases, not just this canvas) and only point when the
            // first actual click handler is the specified control.
            var eventSystem = EventSystem.current;
            if (!eventSystem || !eventSystem.isActiveAndEnabled)
            {
                LastPointerRejectReason = "event-system-missing-or-disabled";
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            var pointerData = new PointerEventData(eventSystem) { position = screenPoint };
            var pointerHits = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, pointerHits);
            if (pointerHits.Count == 0 ||
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    pointerHits[0].gameObject) != actualControl.gameObject)
            {
                LastPointerRejectReason = "raycast-first=" + (pointerHits.Count==0 ? "none" : pointerHits[0].gameObject.name) + ";target=" + actualControl.name + ";screen=" + screenPoint;
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            // The actual PNG points LEFT (fingertip in its upper-left).
            // Approach from the RIGHT side of a button, not its label center.
            // The halo independently stays centered on the real target.
            var parent = pointer.rectTransform.parent as RectTransform;
            var sprite = Resources.Load<Sprite>(Prefix + "tutorial_pointing_hand");
            if (!parent || !sprite)
            {
                LastPointerRejectReason = "pointer-parent-or-sprite-missing";
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            var rect = pointer.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            // Preserve the ORIGINAL PNG's native aspect, not a square
            // letterbox. Its actual opaque left fingertip is x62/386,
            // y72..77/245 from the top (alpha>=200), hence this normalized
            // lower-left pivot. No authored pixels are cropped or repainted.
            float pointerWidth=Mathf.Max(1f,rect.sizeDelta.x);
            rect.sizeDelta=new Vector2(pointerWidth,
                pointerWidth*sprite.rect.height/Mathf.Max(1f,sprite.rect.width));
            rect.pivot = new Vector2(.16f, .696f);
            rect.localRotation = Quaternion.identity;
            float originalScaleX = Mathf.Abs(rect.localScale.x);
            if (originalScaleX <= .0001f)
            {
                LastPointerRejectReason = "pointer-scale-invalid";
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
            // The game device safe area, not just the entire screen, limits
            // tutorial art. In offscreen RenderTexture QA its own dimensions
            // are authoritative; the host Editor safe area would be bogus.
            Rect safe = camera && camera.targetTexture ?
                new Rect(3f,3f,viewWidth-6f,viewHeight-6f) : Screen.safeArea;
            if (safe.width < 6f || safe.height < 6f)
                safe = new Rect(3f,3f,viewWidth-6f,viewHeight-6f);
            safe.xMin += 3f; safe.yMin += 3f;
            safe.xMax -= 3f; safe.yMax -= 3f;
            // Do not assume buttons are centered: the upper-right claim CTA
            // in 390/844 mobile left only ~30px to the screen edge. A hand
            // extending right would be cut off. Mirror it from the LEFT when
            // the real render geometry does not fit on the right.
            bool Place(bool mirror, int contact)
            {
                // A top-edge header CTA may have space to the left, but
                // not ABOVE the fingertip. Try its center first, then its
                // lower/upper edge. Never squeeze or clip the painted hand.
                float contactY = actualControl.rect.center.y;
                float insetY = Mathf.Min(4f, actualControl.rect.height*.12f);
                if (contact == 1)
                    contactY = actualControl.rect.yMin + insetY;
                else if (contact == 2)
                    contactY = actualControl.rect.yMax - insetY;
                var tip = actualControl.TransformPoint(new Vector3(
                    mirror ? actualControl.rect.xMin - 3f :
                             actualControl.rect.xMax + 3f,
                    contactY, 0f));
                var target = RectTransformUtility.WorldToScreenPoint(camera,tip);
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent,target,camera,out var local))return false;
                rect.localScale = new Vector3(
                    (mirror?-1f:1f)*originalScaleX,
                    rect.localScale.y,rect.localScale.z);
                rect.anchoredPosition = local;
                var corners = new Vector3[4];
                rect.GetWorldCorners(corners);
                foreach(var corner in corners)
                {
                    var screen = RectTransformUtility.WorldToScreenPoint(camera,corner);
                    if (!safe.Contains(screen))return false;
                }
                return true;
            }
            bool fits = false;
            for(int vertical = 0; vertical < 3 && !fits; vertical++)
            {
                fits = Place(false, vertical) || Place(true, vertical);
            }
            if (!fits)
            {
                LastPointerRejectReason = "hand-outside-safe-area";
                pointer.gameObject.SetActive(false);
                if (targetHalo) targetHalo.gameObject.SetActive(false);
                return false;
            }
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

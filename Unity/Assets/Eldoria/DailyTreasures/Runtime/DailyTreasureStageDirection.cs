using System;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Four authored milestone compositions, each using authentic Eldoria
    /// imagery. No imagined hero portrait, fake future Bastion architecture,
    /// currencies, or static full-screen screenshot-as-UI.
    /// Geometry is pure data so all phone/tablet layouts can be verified
    /// WITHOUT claiming a native Unity render has occurred.
    /// </summary>
    public enum DailyTreasureStageKind
    {
        Daily, Lyra, Explorers, Vigilants, Bastion
    }

    public readonly struct DailyTreasureStageLayout
    {
        public readonly DailyTreasureStageKind Kind;
        public readonly float SceneX,SceneY,SceneW,SceneH;
        public readonly float ChestX,ChestY,ChestW,ChestH;
        public readonly float ReceiptX,ReceiptY,ReceiptW,ReceiptH;
        public readonly float BackdropOpacity,SceneryOpacity,SceneVeilOpacity;
        public readonly bool SideBySide;

        public DailyTreasureStageLayout(DailyTreasureStageKind kind,
            float sceneX,float sceneY,float sceneW,float sceneH,
            float chestX,float chestY,float chestW,float chestH,
            float receiptX,float receiptY,float receiptW,float receiptH,
            float backdropOpacity,float sceneryOpacity,float veilOpacity,
            bool sideBySide)
        {
            Kind=kind;SceneX=sceneX;SceneY=sceneY;
            SceneW=sceneW;SceneH=sceneH;
            ChestX=chestX;ChestY=chestY;ChestW=chestW;ChestH=chestH;
            ReceiptX=receiptX;ReceiptY=receiptY;
            ReceiptW=receiptW;ReceiptH=receiptH;
            BackdropOpacity=backdropOpacity;
            SceneryOpacity=sceneryOpacity;SceneVeilOpacity=veilOpacity;
            SideBySide=sideBySide;
        }
    }

    // Pure deterministic stage entrance — no Unity objects, allocations,
    // new rewards, new actors or re-rendered paintings. The four ceremonies
    // each unveil an EXISTING original in their OWN way instead of playing
    // a second chest animation under a different heading.
    public readonly struct DailyTreasureStageEntrance
    {
        public readonly float Opacity,Scale,ShiftX,ShiftY;
        public DailyTreasureStageEntrance(float opacity,float scale,
            float shiftX,float shiftY)
        {
            Opacity=opacity;Scale=scale;ShiftX=shiftX;ShiftY=shiftY;
        }
    }

    // Raw Image.preserveAspect only shrinks the VERTICES; it does not
    // shrink child RectTransforms. Anchoring genuine mission artwork to a
    // letterboxed parent made the four real illustrations float and stretch
    // vertically across the bronze parchment in a phone portrait.
    // Parent and its child must occupy the SAME unletterboxed source quad.
    public readonly struct DailyTreasureOriginalArtSize
    {
        public readonly float Width,Height;
        public DailyTreasureOriginalArtSize(float width,float height)
        {Width=width;Height=height;}
    }

    public readonly struct DailyTreasureNarrativeFrame
    {
        public readonly float Width,Height;
        public DailyTreasureNarrativeFrame(float width,float height)
        {Width=width;Height=height;}
    }

    public static class DailyTreasureStageDirection
    {
        // True premium cinema: actual authored landscapes should fill the
        // desktop/tablet, while 390x844 and 844x390 retain safe layout.
        public static DailyTreasureNarrativeFrame NarrativeTheatreFrame(
            float width,float height)
        {
            if(float.IsNaN(width)||float.IsInfinity(width)||
               float.IsNaN(height)||float.IsInfinity(height)||
               width<275f||height<300f)
                throw new ArgumentOutOfRangeException(nameof(width));
            bool portrait=height>width;
            if(portrait&&width>=650f&&height>=900f)
                return new DailyTreasureNarrativeFrame(
                    Math.Min(width-24f,620f),
                    Math.Min(height-20f,1060f));
            if(!portrait&&width>=950f&&height>=620f)
                return new DailyTreasureNarrativeFrame(
                    Math.Min(width-32f,Math.Min(1120f,width*.84f)),
                    Math.Min(height-26f,Math.Min(640f,height*.76f)));
            return new DailyTreasureNarrativeFrame(
                Math.Min(width-24f,portrait?440f:740f),
                Math.Min(height-20f,portrait?700f:370f));
        }
        public static DailyTreasureOriginalArtSize ContainOriginalArtwork(
            float availableWidth,float availableHeight,
            float spritePixelWidth,float spritePixelHeight)
        {
            if(float.IsNaN(availableWidth)||float.IsInfinity(availableWidth)||
               float.IsNaN(availableHeight)||float.IsInfinity(availableHeight)||
               float.IsNaN(spritePixelWidth)||float.IsInfinity(spritePixelWidth)||
               float.IsNaN(spritePixelHeight)||float.IsInfinity(spritePixelHeight)||
               availableWidth<=0f||availableHeight<=0f||
               spritePixelWidth<=0f||spritePixelHeight<=0f)
                throw new ArgumentOutOfRangeException(nameof(availableWidth));
            float scale=Math.Min(
                availableWidth/spritePixelWidth,
                availableHeight/spritePixelHeight);
            return new DailyTreasureOriginalArtSize(
                spritePixelWidth*scale,spritePixelHeight*scale);
        }

        public static DailyTreasureStageEntrance SampleEntrance(
            DailyTreasureStageKind kind,float elapsed)
        {
            if(float.IsNaN(elapsed)||float.IsInfinity(elapsed))
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            if(kind==DailyTreasureStageKind.Daily)
                return new DailyTreasureStageEntrance(1f,1f,0f,0f);
            float duration=kind==DailyTreasureStageKind.Explorers?1.28f:
                kind==DailyTreasureStageKind.Vigilants?1.05f:
                kind==DailyTreasureStageKind.Lyra?1.16f:1.12f;
            float progress=Math.Min(1f,Math.Max(0f,elapsed/duration));
            // Genuine bronze/stone has weight; ease out without bouncing.
            float eased=progress*progress*(3f-2f*progress);
            float waiting=1f-eased;
            switch(kind)
            {
                case DailyTreasureStageKind.Lyra:
                    return new DailyTreasureStageEntrance(
                        1f-.24f*waiting,1f-.025f*waiting,
                        0f,-5f*waiting);
                case DailyTreasureStageKind.Explorers:
                    // The TRUE surveyed background remains still while the
                    // original EMPTY parchment enters beside it.
                    return new DailyTreasureStageEntrance(
                        1f-.48f*waiting,1f-.095f*waiting,
                        10f*waiting,-3f*waiting);
                case DailyTreasureStageKind.Vigilants:
                    // Weighty forged shield; actual 128px insignia stays bound
                    // to its own existing overlay, never redrawn as a symbol.
                    return new DailyTreasureStageEntrance(
                        1f-.42f*waiting,1f-.075f*waiting,
                        0f,-9f*waiting);
                case DailyTreasureStageKind.Bastion:
                    // Only the edge decoration moves; the real world camera
                    // and finished building NEVER move or change ownership.
                    return new DailyTreasureStageEntrance(
                        1f-.48f*waiting,1f-.015f*waiting,
                        -5f*waiting,0f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
        // A Bastion milestone celebrates the player's ACTUAL finished city.
        // Prior two stacked layers transmitted only ~35% of its light. Open
        // them to ~41% while preserving a dark forged frame and a separate
        // .82 basalt ink patch behind the real recorded story text.
        public const float BastionPlateOpacity=.34f;
        // When the true building camera has not become available, keep its
        // actual city visible THROUGH the frame rather than substitute an
        // empty black picture. The .82-alpha ink protects factual text.
        // Source-city light retained: (1-.19)*(1-.10)=72.9%.
        public const float BastionWorldThroughPlateOpacity=.19f;
        public const float BastionWorldThroughBackdropOpacity=.10f;
        public static DailyTreasureStageKind FromEvent(string eventId)
        {
            if(string.IsNullOrWhiteSpace(eventId))
                return DailyTreasureStageKind.Daily;
            if(eventId.StartsWith("milestone:hero:lyra",StringComparison.Ordinal))
                return DailyTreasureStageKind.Lyra;
            if(eventId.StartsWith("scout:",StringComparison.Ordinal) ||
                eventId.StartsWith("exploration:",StringComparison.Ordinal))
                return DailyTreasureStageKind.Explorers;
            if(eventId.StartsWith("vigilant:",StringComparison.Ordinal))
                return DailyTreasureStageKind.Vigilants;
            if(eventId.StartsWith("milestone:bastion:",StringComparison.Ordinal))
                return DailyTreasureStageKind.Bastion;
            return DailyTreasureStageKind.Daily;
        }

        // Gentle depth motion through already-cropped real artwork.
        // Never exceed a small part of the available source-image margin:
        // no UV repetition, black bars, jump cuts or invented scene content.
        public static float SafePainterlyDrift(float seconds,float availableUv,
            bool vertical)
        {
            if(float.IsNaN(seconds)||float.IsInfinity(seconds)||
               float.IsNaN(availableUv)||float.IsInfinity(availableUv)||
               availableUv<0f||availableUv>1f)
                throw new ArgumentOutOfRangeException(nameof(availableUv));
            if(availableUv<.0001f)return 0f;
            float phase=vertical?1.2f:.5f;
            return (float)Math.Sin(seconds*(vertical?.38f:.53f)+phase)*
                availableUv*.13f;
        }

        public static DailyTreasureStageLayout Compose(string eventId,
            float width,float height)
        {
            if(float.IsNaN(width)||float.IsNaN(height)||
               float.IsInfinity(width)||float.IsInfinity(height)||
               width<275f||height<300f)
                throw new ArgumentOutOfRangeException(nameof(width),
                    "Need a real portrait/landscape canonical screen size");
            var kind=FromEvent(eventId);
            bool portrait=height>width;
            // Daily loot retains its own verified compact frame. Milestones
            // use the authored full-world theatre, NOT a small flat modal.
            var frame=NarrativeTheatreFrame(width,height);
            float w=kind==DailyTreasureStageKind.Daily?
                Math.Min(width-24f,portrait?440f:740f):frame.Width;
            float h=kind==DailyTreasureStageKind.Daily?
                Math.Min(height-20f,portrait?700f:370f):frame.Height;

            if(kind==DailyTreasureStageKind.Daily)
                return new DailyTreasureStageLayout(kind,
                    0,0,w-24,h-26,
                    0,portrait?105f:87f,portrait?294f:282f,
                    portrait?265f:175f,
                    0,portrait?-148f:-71f,w-45,
                    portrait?141f:66f,
                    .91f,0,.0f,false);

            // A completed Bastion is shown against the TRUE city view.
            // An invented painted city would risk depicting an incorrect level.
            if(kind==DailyTreasureStageKind.Bastion)
                return new DailyTreasureStageLayout(kind,
                    0,0,w-25,h-28,
                    0,portrait?103f:85f,portrait?302f:296f,
                    portrait?269f:176f,
                    portrait?0f:-.24f*w,
                    portrait?-.28f*h:-.08f*h,
                    portrait?w-48:.40f*w,
                    portrait?.16f*h:.31f*h,
                    .38f,0f,0f,false);

            // Lyra, Scouts and Vigilants each use existing original Eldoria
            // scenery, not an imaginary character/map/building from a
            // generated mockup. The real painting must be the focal image.
            // The daily coffer is absent; the right-hand landscape column
            // belongs to the recorded story and Continue action.
            if(portrait)
            {
                return new DailyTreasureStageLayout(kind,
                    0,.15f*h,w-24,.56f*h,
                    0,.005f*h,Math.Min(w*.64f,272f),
                    Math.Min(h*.33f,223f),
                    0,-.27f*h,w-48,.16f*h,
                    // Genuine Unity portrait Lyra hall lost its carved
                    // masonry in the dark veil: archived native 42818514510
                    // has only ten color bins in the painted aperture.
                    // Brighten the existing hall ONLY, keeping other
                    // categories and Bastion's real city untouched.
                    .90f,
                    .95f,.08f,false);
            }
            return new DailyTreasureStageLayout(kind,
                -.18f*w,.018f*h,.62f*w,h-42f,
                .236f*w,.166f*h,Math.Min(.374f*w,282f),
                Math.Min(.475f*h,175f),
                .29f*w,.07f*h,.34f*w,.34f*h,
                .88f,.92f,.10f,true);
        }
    }
}

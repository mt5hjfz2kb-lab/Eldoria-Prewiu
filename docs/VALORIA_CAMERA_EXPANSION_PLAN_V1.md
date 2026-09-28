# Valoria Camera & Expansion Plan v1

Date: 2026-09-28  
Status: **ACTIVE DESIGN BASELINE — IMPLEMENTATION GATE PENDING**

## Purpose

Extend the certified Valoria camera from a fixed-position review/gameplay camera into a **fixed-orientation, bounded-panning mobile city camera**.

The visual composition remains authored. This is not free 360° exploration.

## 1. Certified baseline retained

Current certified camera reference:
- position: `(18.2, 14.6, -25.8)`
- target: `(0, 3.15, 5.8)`
- orthographic zoom range: `9..19`
- benchmark zooms:
  - strategic: 19
  - city: 12
  - detail: 9

This pose becomes the **home/anchor pose** for composition and regression.

## 2. New camera model

Replace the old mental model “fixed position” with:

**fixed orientation + controlled zoom + bounded world translation**

The player may:
- drag the city with one finger / pointer;
- move primarily left/right;
- use controlled secondary depth movement where the final city envelope requires it;
- zoom only inside the approved orthographic range.

The player may not:
- rotate the city freely;
- orbit 360°;
- tilt to arbitrary angles;
- leave the authored city bounds.

## 3. Why panning is required

The long-term city must be larger than one mobile viewport.

At higher progression:
- more districts become occupied;
- more buildings exist than can be comfortably presented at once;
- the player should physically explore their expanded city;
- click targets can stay large and readable instead of shrinking the entire city to fit one screen.

Panning is therefore a structural requirement, not a convenience feature.

## 4. Pan interaction

### Touch
- one-finger drag pans the camera;
- drag begins only after a small movement threshold so a tap remains a building tap;
- releasing the finger ends pan;
- mild inertia may be tested, but default should favor control over slippery scrolling.

### Mouse/editor
- left/middle drag or an equivalent editor-safe input may mirror touch for testing;
- test behavior must not introduce a second camera model.

### Tap-versus-drag contract
A gesture that exceeds the pan threshold must **not** trigger the building under the release point.

A clean tap must still resolve the real `WorldHotspot`.

This must be automated in PlayMode before production rollout.

## 5. Camera bounds

Camera bounds are authored from the approved master city envelope.

They should:
- prevent showing empty void outside the world;
- keep enough framing terrain/skyline at each edge;
- guarantee that every active plot can be centered or comfortably inspected;
- keep the Bastion recoverable as an orientation reference.

### Progression-aware bounds
Early game should not let the player pan across huge empty late-game territory.

Preferred behavior:
- camera bounds expand when districts unlock;
- locked future areas remain outside the normal pan envelope or are only partially teased;
- expansion itself becomes a visible reward.

Exact bounds must be measured after the long-term graybox exists.

## 6. Home / recenter behavior

Provide a reliable way to return to the city anchor.

Minimum contract:
- entering Valoria starts at a progression-appropriate home position;
- a future explicit “recenter/home” control is recommended if playtesting shows orientation loss;
- system-driven focus on a newly unlocked building may temporarily pan there, but user control returns immediately afterward.

Do not auto-snap constantly while the player is exploring.

## 7. Zoom behavior

Retain the existing orthographic range `9..19` unless mobile testing proves a change is needed.

Intended reads:
- **19 strategic:** district relationships / navigation;
- **12 city:** primary everyday management;
- **9 detail:** individual building/route inspection.

The long-term city does **not** need to fit entirely at zoom 19.

If everything fits at 19 after level 35, the city may be too compressed to deliver the intended growth feeling.

## 8. Composition rules under pan

Every district must be authored for its reachable camera window, not only the original home screenshot.

For every pan extreme:
- no giant foreground prop may cover essential interaction;
- terrain edge must remain finished;
- skyline should still have intentional framing;
- important buildings need readable silhouettes;
- roads/stairs must still explain how spaces connect;
- no hidden seam from the blockout/terrain system may be exposed.

This expands the visual gate from **three zoom screenshots at one position** to a camera-envelope test.

## 9. Mobile safe areas and UI

Panning must be tested with the real HUD/safe areas, not a clean cinematic view.

Critical conditions:
- building tap targets are not permanently under UI rails;
- edge districts can be panned into a comfortable interaction zone;
- camera bounds account for screen aspect ratio and safe-area insets;
- portrait is not assumed unless Eldoria explicitly supports it; validate intended mobile orientation(s).

## 10. Expansion envelope

The current certified kernel remains the home-center reference.

Long-term expansion is expected to be:
- primarily lateral left/right;
- secondarily deeper/higher where terraces are justified;
- visually bounded by terrain/fortification rather than invisible arbitrary edges.

Provisional occupancy targets from the progression map:
- Bastion 1–10: compact kernel / minority of final envelope;
- 11–15: first clear wing expansion;
- 16–20: both lateral sides materially active;
- 21–25: capital-scale majority occupied;
- 26–30: near-full envelope;
- 31–35: full planned prestige footprint.

Exact metre dimensions remain pending the full-envelope Unity graybox.

## 11. Implementation gate

Before final-art expansion, implement an isolated/controlled camera prototype on the certified skeleton + master-envelope graybox and prove:

1. drag pans camera in intended world directions;
2. orientation never changes;
3. zoom 9/12/19 remains correct;
4. bounds clamp correctly on all sides;
5. tap still selects Aserradero/Cuartel/Bastion;
6. drag does not accidentally click a building;
7. all current hotspots still open real panels;
8. camera can reach all reserved district centers;
9. early progression bounds can be smaller than late progression bounds;
10. screenshots from home + left edge + right edge + upper/depth edge are visually coherent;
11. common mobile aspect ratios and safe areas do not make edge plots unreachable.

Only after this gate passes should broad city dressing assume panning as production-safe.

## 12. Required QA surfaces

### Technical
- EditMode input/bounds helpers;
- PlayMode tap-vs-drag;
- PlayMode hotspot selection after pan;
- PlayMode zoom after pan;
- build gate.

### Visual
At minimum for each major progression envelope:
- home at zoom 19/12/9;
- left expansion at 12;
- right expansion at 12;
- far/depth expansion at 12 when applicable;
- at least one mobile-aspect capture.

### UX
A novice player must:
- discover that the city can be dragged;
- understand that dragging moves the city, not a building;
- still understand where the Bastion is;
- return to relevant buildings without confusion.

## 13. Permanent interpretation

From this plan onward, “official Valoria camera” means:

**an authored isometric orientation and zoom family with an approved home pose and bounded translation across the city.**

It no longer means that the camera world position must be identical at all times.

# VALORIA MOBILE NAVIGABLE CITY v1 — RESULT

Status: **CLOSED / PROMOTED**
Date: 2026-10-04

## Final verdict

**TECH PASS / MOBILE NAVIGATION VISUAL PASS / GAMEPLAY PASS**

Authoritative run: **37213794236 — SUCCESS**  
Artifact: **11307920631**  
Planner run: **37213645061 — SUCCESS**  
Credits: **0 Tripo / 0 paid**

## Product question resolved

Valoria does **not** need to fit inside one portrait-mobile viewport.

The previous requirement that Granero + Cuartel + the whole relevant city read simultaneously was unnecessarily constraining the visual presentation. The retained M2 principle works substantially better when treated as a **home/entry composition into a navigable city**, rather than as an attempt to display the whole city.

## Promoted mobile home pose

Portrait rule:
- aspect ratio: **<= 0.72**
- projection: **ORTHOGRAPHIC**
- position: **(19.4, 14.6, -28.6)**
- target: **(1.2, 3.35, 2.8)**
- orthographic size: **12.2**

This is equivalent to the useful composition principle demonstrated by M2, but it is promoted here only together with bounded navigation.

16:9 remains separate and unchanged:
- position: **(18.2, 14.6, -25.8)**
- target: **(0, 3.35, 5.6)**
- orthographic size: **9.1**

## Navigation

Existing one-finger/pointer drag remains the canonical city interaction:
- 12 px threshold separates tap from drag;
- fixed orientation;
- orthographic zoom remains 9..19;
- no free orbit / no perspective;
- movement is bounded and progression-aware.

For Bastion <=10 at the tested portrait aspect 390x844:
- pan half extent X: **16.0 world units**
- pan half extent Z: **7.0 world units**

Later progression keeps the existing expanding envelope.

The runtime now centralizes the pan envelope through `ValoriaMobileNavigableCityV1` so home framing and bounds are one coherent policy.

## Recenter / HOME

The existing recenter behavior is now player-accessible with a small city HOME control in the existing HUD.

Recenter restores:
- exact home position;
- exact mobile/home orthographic size.

No large new navigation UI was introduced.

## Evidence

Artifact captures:
- `HOME-mobile.png`
- `PAN-granero-mobile.png`
- `PAN-intermediate-mobile.png`
- `PAN-cuartel-mobile.png`
- `RETURN-HOME-mobile.png`
- `HORIZONTAL-16x9.png`
- `evidence.json`

Visual review:
- HOME keeps Hero Bastion clearly dominant while exposing a meaningful lower-city slice.
- Panning preserves architectural pixel scale instead of shrinking Valoria into a miniature.
- Granero/left functional area and Cuartel/right functional area are reachable without changing their anchors.
- Intermediate movement preserves visual continuity with Bastion as a spatial landmark.
- RETURN HOME reproduces the original promoted mobile home composition.
- No technical world border is exposed.
- Peripheral pans do reveal under-dressed terrain/transition areas. This is now an **art-production gap**, not a reason to keep zooming out or moving the functional buildings.

## Gameplay verification

The successful run verifies:
- gameplay collider/hotspot signature preserved;
- Granero-side canonical economic hotspot remains reachable after camera translation;
- Cuartel hotspot remains reachable after camera translation;
- Bastion hotspot remains reachable after camera translation;
- focused gameplay PlayMode tests PASS;
- tap-vs-drag architecture remains intact.

The paired production visuals remain visual-only, preserving the existing authoritative gameplay targets.

## Horizontal regression

**PASS.**

The 16:9 branch is explicitly excluded from the portrait home rule and was captured at the existing canonical framing.

No mobile rule changes the 16:9 position, target or orthographic size.

## Required answers

1. **¿M2 o una variante equivalente es una buena HOME POSE aunque no muestre toda Valoria?**  
   **Sí.** As a navigable-city entry composition, the M2-equivalent pose gives a materially better balance between Hero presence and useful city scale.

2. **¿El jugador puede llegar naturalmente a Granero y Cuartel mediante panning?**  
   **Sí.** Both functional sides are reachable inside the bounded early-game envelope and interaction remains valid after translation.

3. **¿Los edificios mantienen una escala visual mejor que intentando meter toda Valoria en pantalla?**  
   **Sí.** This is the main visual win: buildings retain readable detail and presence rather than becoming miniature map markers.

4. **¿La navegación se siente como explorar una ciudad y no como mover un mapa?**  
   **Sí.** Fixed orientation, close architectural scale, continuous terrain/streets and a persistent Bastion landmark make the movement read as city exploration. Peripheral dressing is still incomplete, but the camera model itself does not read as a world map.

5. **¿Podemos dejar de exigir visibilidad simultánea de todos los edificios?**  
   **Sí.** That requirement is removed. Home pose is an authored entry view, not a complete-city fit.

6. **¿Esta solución mejora la PRESENTACIÓN GRÁFICA real de Valoria?**  
   **Sí.** It allows the art to occupy more useful screen space and makes city scale itself part of the presentation. It does not improve source geometry by itself; it removes the viewport constraint that was forcing the art to be shown too small.

## Promotion decision

Promote:
- portrait-specific navigable HOME policy;
- existing bounded pan model;
- progression-aware bounds;
- recenter restoring position + zoom;
- small existing-HUD HOME control;
- separate unchanged 16:9 framing.

Do not:
- move Granero/Cuartel merely to make them co-visible;
- compress the district;
- reopen M1/M2/M3 camera variants;
- change projection;
- continue camera research by inertia.

## Graphical conclusion

This camera/screen-space investigation is complete because it has answered the product question that was blocking visual production.

The next work must return to the actual objective: **make the visible frame look substantially better**.

The strongest next screen-impact block is **terrain / edge / architectural-transition cohesion across the reachable pan envelope**:
- finish the under-dressed ground and peripheral transitions revealed by panning;
- replace obvious temporary/support forms visible in played frames;
- strengthen secondary architecture and lower-city continuity;
- add restrained life/storytelling only after those large forms read coherently.

Forja / Hospital / Cantera should not be produced merely to fill space until this visible-frame cohesion pass is complete.

## Final answer

**YES: VALORIA CAN BE PRESENTED AS A LARGE, VISUALLY RICH NAVIGABLE CITY WITHOUT FITTING THE WHOLE CITY ON THE MOBILE SCREEN AT ONCE.**

The camera is now a means to expose the art at useful scale. It is no longer the visual-production objective.

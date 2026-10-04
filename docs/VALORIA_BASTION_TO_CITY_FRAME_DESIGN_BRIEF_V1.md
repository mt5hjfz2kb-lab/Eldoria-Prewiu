# VALORIA BASTION-TO-CITY FRAME DESIGN BRIEF v1

Date: 2026-10-04  
Status: **CLOSED / DESIGN TARGET LOCKED**  
Cost: **0 Tripo / 0 paid credits**  
Canonical visual reference: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`

## Purpose

This block does **not** author production geometry. It locks the exact played-frame target that a later Blender block must build.

The method is now:

**promoted played frame -> exact screen-space design -> Blender freeform construction -> isolated ART SOURCE review -> Unity A/B**

Blender is not allowed to discover the composition by improvising 3D masses.

## 1. Exact canonical base capture

The locked base is:

- source block: **VALORIA LOWER-CITY URBAN MASSING REAUTHORING v1**
- verdict: **TECH PASS / VISUAL PASS / PROMOTED**
- run: **37221732105 — SUCCESS**
- artifact: **11309549299**
- captured head: `5231a9e208cf99d9cc5d6a350f8de17bbdb6ae62`
- artifact file: `AFTER-HORIZONTAL-16x9.png`
- dimensions: **1600 x 900**
- SHA-256: `ef45233a09199829c83f5cb3d5587753f5b93fa7b1e9ba726b633791b7e343c1`

This is the promoted Valoria state after Lower-City Urban Massing and before the rejected Structural Frame / Bastion-to-City experiments. It is therefore the only valid 16:9 base for this brief.

## 2. Exact zone to replace visually

Replace only the **technical-looking transition immediately below and to both sides of Hero Bastion's central ascent**:

- the large grey stair-flanking support masses;
- their exposed rectangular shoulders / support faces;
- the weak support-to-rock interfaces;
- the visually disconnected retaining band between Bastion and the promoted lower city.

Do **not** replace the Hero Bastion itself.  
Do **not** replace the promoted lower-city groups.  
Do **not** replace the canonical central stair/circulation.  
Do **not** move Granero, Cuartel, camera, gameplay routes, parcels or density.

The canonical screen-space envelope is stored in:
- `docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-spec.json`
- `docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-overlay.svg`

The overlay uses the original **1600 x 900** coordinates and is intended to be placed directly over the locked base capture.

## 3. Target architectural reading

The transition must read as **one continuous inhabited civic retaining architecture**, not as two supports and not as a bridge module.

From the official 16:9 frame the hierarchy must be:

1. **Hero Bastion** — dominant landmark, unchanged.
2. **Bastion-to-city retaining frame** — strong secondary architecture, clearly designed as part of the capital.
3. **Promoted lower city** — tertiary urban fabric receiving the frame below.

The desired silhouette is deliberately asymmetric:

- **west shoulder:** lower, broader and more inhabited/craft-like;
- **east shoulder:** slightly taller and more civic/monumental;
- **central stair:** remains open and visually legible;
- **lower receiving terrace:** visually unifies both shoulders without becoming a flat bridge slab;
- **upper contacts:** step into Bastion rock and walls instead of attaching decorative arch pieces onto a box.

## 4. Required masses

### A. West retaining shoulder

A low, broad stepped mass that descends toward the west lower city.

It needs:
- at least two visible vertical setbacks;
- one real deep arcade/recess family;
- a terrace edge with usable architectural thickness;
- broken parapet/cornice rhythm;
- a rock-integrated lower termination.

It must **not** mirror the east side.

### B. East civic shoulder

A slightly taller and narrower mass that carries more vertical emphasis.

It needs:
- stronger upper silhouette than west;
- fewer but deeper openings;
- a visible return/side wall so it reads as volume rather than facade;
- one restrained slate/blue civic accent;
- a different lower rock transition from the west side.

### C. Central receiving terrace

A continuous architectural base around the lower end of the central ascent.

It must:
- visually receive the stair;
- connect to the promoted lower-city fabric;
- contain a dark undercut / shadow band;
- step in depth rather than presenting one flat front;
- preserve the central circulation corridor.

It must not become a defensive wall, bridge slab or podium pasted in front of the stair.

### D. Upper Bastion contacts

The frame must die into the Bastion/rock with stepped returns, retaining walls and rock interlock.

There must be no isolated giant ceremonial arch placed above the system. The Bastion gateway is already the architectural climax.

## 5. Depth and shadow requirements

The rejected source proved that facade vocabulary alone is insufficient.

Every major visible shoulder must show at least three screen-readable depth levels:

1. foreground retaining edge / parapet;
2. primary wall plane;
3. genuinely recessed bay, passage, arcade or dark return.

Openings must have:
- visible wall thickness;
- side returns;
- dark internal value;
- non-uniform spacing.

The official camera must create readable cast/contact shadow under terraces, inside recesses and where stone meets rock.

## 6. Stone / rock interface

Architecture must appear **built into and out of the cliff**, not placed on top of a rock platform.

Required:
- irregular buried wall ends;
- rock penetrating/interrupting straight masonry lines;
- buttress/retaining logic that follows level changes;
- no clean rectangular foundation outline around the whole piece;
- no wedge-shaped rock props used as seam covers.

The rock interface should be strongest at the outer lower corners and lighter near the central stair so circulation remains clear.

## 7. Material / value target

Stay inside the Hero Bastion family without competing with it.

- primary stone: warm mid-value limestone/sandstone;
- secondary stone: slightly darker/weathered retaining courses;
- trim: limited pale warm stone on selected ledges/opening frames;
- recesses: substantially darker neutral brown/charcoal;
- roofs/caps: restrained slate, sparse;
- heraldry: very limited Valoria blue accents;
- rock: darker/desaturated than architecture, with overlap and contact shadow.

Avoid:
- flat single-value stone;
- bright uniform trim around every opening;
- large clean beige rectangles;
- material changes used to fake depth that geometry does not provide.

## 8. Relation to promoted lower city

The frame must **hand off** into the three promoted urban groups rather than overwrite them.

At the bottom:
- terrace edges should visually align with existing courts/lanes;
- mass should reduce in height as it approaches lower city;
- openings and retaining faces should become more domestic in scale;
- negative space between lower-city groups must remain visible.

The result should make the lower city feel like it grows out of the Bastion's civic structure.

## 9. What the rejected frame did wrong

The rejected `VALORIA BASTION-TO-CITY ARCHITECTURAL FRAME v1` was stopped correctly at ART SOURCE.

Do not repeat:

- dominant rectilinear box/slab masses;
- trims, arches and buttresses attached onto boxes as decoration;
- shallow facade-only depth;
- a giant upper arch reading as an added symbol;
- clean generic geometry disconnected from the cliff;
- wedge-like rock interfaces;
- procedural-looking rhythm even when asymmetric;
- flat/simple material relationships;
- polygon/object/modifier count used as a quality argument.

The failure was **authoring method**, not insufficient detail.

## 10. Blender construction brief for the next block

Blender must construct this already-decided screen-space solution.

Mandatory method:

1. Import/use the locked 16:9 base as camera-plane reference or matched proxy.
2. Reproduce the target overlay silhouette before secondary detail.
3. Build the west and east shoulders as genuinely different authored volumes.
4. Use direct mesh editing, controlled booleans, curves/profiles, manual deformation and sculpt where useful.
5. Give every visible opening and terrace believable thickness/returns.
6. Author stone-rock interlock directly; do not cover seams with detached wedges.
7. Establish material/value hierarchy before Unity.
8. Produce isolated clay, lit 3/4 and official-camera/proxy evidence.
9. Stop before Unity unless ART SOURCE passes.

Presumptive rejection:
- primitive-stack silhouette;
- repeated formulaic bays;
- mirror-generated halves;
- flat placeholder materials presented as final;
- a single facade sheet hiding technical supports;
- added decoration without changing the primary silhouette/depth.

## 11. Screen-space success test

From the locked 16:9 frame, before comparing small detail, ask:

- Do the two grey technical support masses cease to read as supports?
- Does the transition read as one capital architecture from Bastion to lower city?
- Is the central stair still obvious and unobstructed?
- Is there nested depth and real shadow?
- Does stone visibly interlock with cliff/rock?
- Is west/east asymmetry intentional but coherent?
- Does the Hero Bastion remain dominant?
- Does lower city remain preserved and better connected?

If these are not true at first glance, the source has not met the brief.

## Mandatory answers

### 1. What exact capture is fixed as the base?

`AFTER-HORIZONTAL-16x9.png`, run **37221732105**, artifact **11309549299**, captured head `5231a9e208cf99d9cc5d6a350f8de17bbdb6ae62`, 1600x900, SHA-256 `ef45233a09199829c83f5cb3d5587753f5b93fa7b1e9ba726b633791b7e343c1`.

### 2. What concrete part of the frame is replaced next?

Only the grey technical retaining/support transition immediately flanking the central Bastion stair and its weak rock contacts, plus the lower receiving retaining band. Hero Bastion, stair circulation and promoted lower city remain fixed.

### 3. What architectural reading do we want?

One asymmetric, layered, inhabited civic retaining composition that appears structurally grown from the Bastion/cliff and naturally steps down into the lower city.

### 4. What was wrong with the previous rejected frame?

It remained a sophisticated blockout: box-derived masses, shallow articulation, attached arch/trim vocabulary, generic clean surfaces and weak rock interlock. It replaced technical supports with architectural greybox rather than final environment art.

### 5. What must Blender build exactly?

The locked screen-space silhouette and depth hierarchy encoded by the target overlay/spec: low inhabited west shoulder, taller civic east shoulder, open central stair, nested deep bays, stepped receiving terrace and irregular stone-rock interfaces, using genuinely freeform DCC authoring rather than scripted box assembly.

## Final decision

**DESIGN TARGET LOCKED.**

The next 3D block is not permitted to redesign the frame. It may solve construction topology and local detailing, but any material change to the primary silhouette, hierarchy or replacement envelope requires returning to this design brief first.

### Final question

**¿TENEMOS YA DEFINIDA LA IMAGEN OBJETIVO EXACTA QUE BLENDER TENDRÁ QUE CONSTRUIR, EN VEZ DE SEGUIR IMPROVISANDO LA FORMA EN 3D?**

**SÍ.** The exact base, replacement envelope, protected regions, screen-space silhouette, architectural hierarchy, depth/material requirements and rejection conditions are now locked.

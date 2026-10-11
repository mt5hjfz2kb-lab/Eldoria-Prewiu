# Valoria Nation 1 -> Production Art Reset Compatibility Audit

Status: **READ-ONLY AUDIT / DOES NOT TAKE OVER NATION 1**  
Program: **VALORIA PRODUCTION ART SYSTEM RESET v1**  
Audited run: **37193601814 SUCCESS**  
Artifact: **11299573844**  
Captured head: **56b1c34a99cd91182c423984c6869e94244a0ee0**  
Audited: 2026-10-04

## Concurrency

The active owner remains `valoria-nation1-authored-production-v1`. This audit did not modify its branch, request, resources, workflow or evidence.

At audit time the production branch had advanced beyond the captured head, so this document is deliberately a compatibility judgement on the last complete artifact, not a closure judgement on unfinished later commits.

## What the valid artifact proves

Technical:
- workflow SUCCESS;
- Blender 4.0.2 source exists;
- a reproducible `.blend` exists;
- 10 GLB outputs exist;
- zoom 9 and mobile captures exist;
- gameplay signature preserved;
- 0 Tripo credits.

Composition:
- city occupancy is clearly higher than the VQB baseline;
- the Bastion is framed by a coherent defensive ring;
- central plaza/civic axis reads more intentionally;
- Aserradero/Cuartel/Granero context is more occupied;
- mobile framing is substantially closer and stronger;
- the layout is useful production-composition evidence.

## What the artifact does NOT prove

It does not prove that the new secondary architectural family is final production art.

Recorded source metrics:
- Wall: 896 vertices
- Tower: 1,629 vertices
- Gate: 4,098 vertices
- Stair: 1,456 vertices
- Civic House: 714 vertices
- Civic House B: 714 vertices
- Workshop: 602 vertices
- Backdrop ridge: 192 vertices
- Civic monument: 183 vertices
- Market stall: 504 vertices

These counts are not a failure by themselves, but combined with source inspection and real captures they identify the current family as primarily low-complexity procedural construction.

## Source-method audit

The Nation 1 Blender authoring script uses primitive-based construction including:
- cube primitives;
- cylinder primitives;
- low-sided cone primitives;
- minimal generated gable roofs;
- repeated mathematical wall/tower modules.

This is valid for:
- composition;
- modular layout;
- silhouette exploration;
- scale;
- support geometry.

Under `docs/ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md`, that method defaults to:
**GREYBOX / SUPPORT / TEMPORARY**

unless real official-camera evidence demonstrates production-equivalent quality.

## Visual judgement against the new Visual Bible

### Clear wins

- stronger city occupancy;
- much better use of the playable footprint;
- Bastion remains the focal point;
- mobile view is no longer dominated by an empty board;
- wall/gate language is more coherent than the prior grey wall ring;
- useful plaza and civic organization;
- reusable placement/composition decisions.

### Remaining final-art gap

At zoom 9:
- secondary houses remain visibly simplified next to the Hero Bastion;
- large facade surfaces are too plain;
- windows/doors/roof edges do not carry enough architectural depth;
- the city still reads as a mix of Hero-quality assets and simplified supporting buildings;
- ground and wall transitions remain cleaner/prototypical rather than premium;
- the defensive ring repeats strongly;
- final material richness is below the approved external reference.

At mobile:
- the closer framing is a major composition improvement;
- the quality mismatch becomes more visible because simplified secondary forms are larger on screen.

## Compatibility verdict

**COMPOSITION: KEEP / HIGH VALUE**  
**GAMEPLAY SAFETY: KEEP**  
**BLENDER REPRODUCIBILITY: KEEP**  
**CURRENT SECONDARY FAMILY AS FINAL ART: NOT CERTIFIED**  
**PRODUCTION-ART CLASSIFICATION: GREYBOX/TEMPORARY pending exceptional visual proof or authored replacement**

This is not a recommendation to delete Nation 1.

The correct reset behavior is:
1. preserve useful composition/placement;
2. preserve any strong existing Hero/dedicated production assets;
3. retain procedural Nation 1 pieces as layout/support while replacements are authored;
4. replace secondary architecture incrementally with the high-fidelity source family;
5. compare replacement against the same camera/layout rather than rebuilding the city.

## Important anti-loop conclusion

Do not spend another long cycle trying to make 600–900-vertex primitive-authored houses become premium architecture through:
- more tinting;
- more lights;
- more props;
- more procedural braces;
- more small roof decorations.

Those can improve a greybox, but the reset must test a different source-authoring technique.

## Reuse decision for the reset

When Nation 1 closes and releases its workstream, the reset should inherit:
- accepted city footprint;
- useful wall/gate placement;
- plaza location/scale;
- building positions;
- field/yard positioning;
- density lessons;
- lake/world-edge lessons where visually accepted;
- capture/gate infrastructure if technically clean.

It should **not** automatically inherit the primitive family as the final art standard.

## Next stop gate

After ownership is released:
- classify the final Nation 1 closure artifact again;
- if later commits have produced an exceptional visual step-change, compare them fairly;
- otherwise start the high-fidelity starter family without changing the accepted composition;
- first replacement comparison remains zoom 9 + mobile.

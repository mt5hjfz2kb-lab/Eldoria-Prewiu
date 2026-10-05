# VALORIA PREMIUM TARGET GAP REVIEW + VISUAL UPLIFT v1

Date: 2026-10-05
Status: **IN PROGRESS — HERO TEST ZONE**

## Authority and evidence
- Canonical reference: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`.
- Current BEFORE authority: Vegetation + Water/Shore final run **37347735272**, artifact **11361740372**, implementation HEAD `6e1dbe984a76a7fe49a46b98f7950986367365cc`.
- Reviewed directly: canonical reference; AFTER 16:9; mobile landscape; 3:2; portrait home/left/right/entry from the final artifact.
- Immutable: official camera, target composition, gameplay axis, Bridge/Lower Gate/Road/Bastion/Sawmill/Camp positions, global platform, gameplay and hotspots.
- Props + Background, new families, new regions, world map, animations and city expansion remain blocked.
- Tripo: **0 credits**.

## Phase 1 — strict premium gap review

| Category | Verdict | Probable cause | Primary domain | Reauthor? | Unity? | New texture/material? | Expected impact | Cost/risk |
|---|---|---|---|---|---|---|---|---|
| Macro composition | MINOR GAP | Layout/camera axis broadly follows locked target but lacks density/depth reinforcement | composition/presentation | No | Unity | No | Medium | Low |
| Visual scale | MAJOR GAP | Large empty plateaus make architecture read like a small model | presentation/lighting | No | Unity | No | High | Low |
| Architecture | MAJOR GAP | Silhouettes are authored but surface/depth response is flattened by presentation | materials/lighting | Evidence first | Unity first | Possibly | High | Medium |
| Rocks / cliffs | MAJOR GAP | Faceted geometry reads low-poly due flat value blocks and weak occlusion | materials/lighting/shader | No initially | Unity first | Possibly | High | Low-Med |
| Ground / terrain | CRITICAL GAP | Broad uniform beige fields dominate frame with almost no material breakup/contact | materials/shader/presentation | No | Unity | Yes | Very high | Medium |
| Materials | CRITICAL GAP | Current frame is dominated by flat/simple value response | materials/shader | No initially | Unity | Yes | Very high | Medium |
| Textures | MAJOR GAP | Several large screen-space surfaces have insufficient readable texture frequency | materials | No initially | Unity | Yes | High | Medium |
| Roughness / specular | CRITICAL GAP | Surfaces do not separate convincingly under light | shader/materials | No | Unity | Yes | Very high | Low-Med |
| AO / contact shadows | CRITICAL GAP | Ground contacts, wall bases, rocks and bridge lack anchoring | lighting/shader | No | Unity | No | Very high | Low |
| General shadows | CRITICAL GAP | Final capture pipeline currently disables directional shadows | lighting | No | Unity | No | Very high | Low |
| Vegetation | MAJOR GAP | Silhouettes exist but uniform mint values and weak grounding make trees read as stylized placeholders | materials/placement/lighting | No | Unity | Maybe | High | Low-Med |
| Water | MAJOR GAP | Water has authored variation but remains visually flat relative to target | shader/lighting | No | Unity | Maybe | Medium-High | Low-Med |
| Shoreline | MAJOR GAP | Contact bands are present but do not yet create natural wet/rock transition | materials/placement | No | Unity | Maybe | High | Medium |
| Rock ↔ architecture | MAJOR GAP | Geometry contact exists but lighting/material response does not unify it | lighting/materials | Not yet | Unity first | Maybe | High | Medium |
| Terrain ↔ building | CRITICAL GAP | Buildings sit on broad uniform ground with little contact transition | lighting/materials | No | Unity | Maybe | Very high | Medium |
| Water ↔ rock | MAJOR GAP | Water value is separated but contact lacks depth/wetness hierarchy | shader/materials | No | Unity | Maybe | High | Medium |
| Lighting | CRITICAL GAP | Flat ambient 0.7 + 0.7 directional + no shadows collapses depth | lighting | No | Unity | No | Very high | Low |
| Color grading | MAJOR GAP | Cool/grey background and pale ground suppress warm premium hierarchy | presentation | No | Unity | No | High | Low |
| Atmospheric depth | CRITICAL GAP | Fog explicitly disabled; no depth cue between foreground/mid/background | lighting/presentation | No | Unity | No | Very high | Low |
| Background / world continuation | CRITICAL GAP | Large flat grey perimeter reads as unfinished board | presentation/composition | No new content in this block | Unity | No | Very high | Medium |
| Environmental density | CRITICAL GAP | Large negative fields lack layered visual information compared with target | placement/presentation | No new props | Unity first | Maybe later | Very high | Medium |
| Silhouettes | MINOR GAP | Major structural silhouettes read cleanly at official camera | geometry | No | Unity | No | Medium | Low |
| Mid-frequency detail | CRITICAL GAP | Most broad surfaces jump from macro forms directly to flat color | materials/geometry | Maybe later | Unity first | Yes | Very high | Medium |
| Microdetail at gameplay camera | MAJOR GAP | Close mobile views expose plain surfaces and limited texel response | materials | No initially | Unity | Yes | High | Medium |
| Mobile readability | MINOR GAP | Axis and landmark readability survive; richness does not | presentation | No | Unity | No | Medium | Low |
| Visual hierarchy | MAJOR GAP | Uniform pale values compress Bastion/Gate/terrain separation | lighting/materials | No | Unity | No | High | Low |
| Premium perception | CRITICAL GAP | Frame reads as production prototype rather than premium shipped environment | all presentation surfaces | No initial reauthor | Unity first | Likely | Very high | Medium |
| Living-world feeling | CRITICAL GAP | Static sparse board, little atmospheric/environmental layering | presentation/placement | No new content yet | Unity | No | High | Medium |
| Asset-pack / generic perception | MAJOR GAP | Individual authored pieces lose identity under uniform surface treatment | materials/lighting | No | Unity | Maybe | High | Low-Med |
| Low-poly / prototype perception | CRITICAL GAP | Flat shading hierarchy, broad empty ground, no shadows/fog | lighting/materials/presentation | No initially | Unity | No | Very high | Low |

### Root diagnosis
The current frame is structurally coherent but is being presented by an intentionally austere capture path: flat ambient light, fog disabled, a weak directional light and **directional shadows disabled**. This collapses form, makes authored geometry read closer to low-poly, removes contact anchoring, flattens material response and exposes the broad unfinished ground/background fields. The premium gap is therefore primarily **lighting + material response + ground/contact integration + atmospheric depth**, not a first-order geometry shortage.

## Phase 2 — top visual levers
Only these five levers are authorized for the first proof, in priority order:
1. **Shadow/contact hierarchy** — enable mobile-safe directional shadows, stronger key direction and lower ambient fill so Gate/Bridge/Road/rock contacts become legible.
2. **Material response hierarchy** — controlled roughness/specular/albedo separation in the hero zone; preserve existing geometry.
3. **Ground/rock/shore contact integration** — darken/wet contacts and reduce the cut-out/floating read without adding a new content family.
4. **Atmospheric/value depth** — restrained fog/background value shift and warmer/cooler depth separation.
5. **Vegetation grounding/value variation** — only if needed after 1–4; no density expansion.

No Blender reauthor is justified before these Unity-first levers are tested. If the AFTER still fails because silhouettes/interfaces remain intrinsically primitive, the audit may recommend a bounded later Blender uplift with evidence; closed families are not automatically reopened.

## Phase 3 — VALORIA PREMIUM HERO TEST ZONE v1
Primary judged zone: **Bridge → Lower Gate → start of Road**, with only the water/shore, rock, vegetation, architecture, ground, lighting and atmosphere needed to read that transition. Global lighting/atmosphere may change because they are presentation systems, but asset/material edits must remain local to the hero zone.

## Phase 4 — initial metric
Scale: 0 prototype, 1 very weak, 2 acceptable prototype, 3 production-mid, 4 premium-close, 5 reference-class.

| Metric | Reference | BEFORE |
|---|---:|---:|
| reference similarity in macro hierarchy | 5 | 3 |
| material richness | 5 | 1 |
| depth | 5 | 1 |
| contact quality | 5 | 1 |
| natural integration | 5 | 1 |
| lighting depth | 5 | 1 |
| atmospheric depth | 5 | 0 |
| premium perception | 5 | 1 |
| mobile readability | 5 | 3 |

**Gate rule:** the proof cannot close unless materials, lighting, depth, environment integration and premium perception each reach at least **4/5**, and the AFTER materially reduces the reference gap without reading as a greybox/prototype.

## Execution plan
- Unity-first presentation proof.
- Generate new official 16:9, mobile landscape, 3:2 and portrait home/left/right/entry captures.
- Compare **REFERENCE vs BEFORE (artifact 11361740372) vs AFTER** directly.
- Run gameplay regression and performance sanity.
- Iterate on the same proof if visual verdict is FAIL.
- Do not open Props + Background automatically.

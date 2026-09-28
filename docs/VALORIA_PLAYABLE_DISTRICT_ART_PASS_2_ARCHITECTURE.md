# VALORIA PLAYABLE DISTRICT v1 — ART PASS 2: ARCHITECTURE

Status: **ATTEMPT 1 — TECH PASS / INTERACTION PASS / VISUAL FAIL**; dedicated Aserradero v1 is now **TECH PASS / INTERACTION PASS / ARCHITECTURE PASS / MATERIAL-INTEGRATION FAIL**.

## Evidence

- Evaluated code checkpoint: `33244ff9fde78bff6b8e92680b020f0afcbd7da7`
- Unity gate: `36450086683`
- Full Unity checks artifact: `10982833751`
- Official Valoria captures artifact: `10983317051`
- Capture digest: `sha256:b984615a62a2aaecd30e4cb648e041f71895c1d5eedd9d7e5dc705a27d9d5b1d`
- Credits spent: **0**
- New generated external assets: **none**

The certified Art Pass 1 topology, camera, zooms, panning, circulation and interaction volumes remain unchanged.

## Attempt 1 result

### Aserradero
Existing art was reused through `SlavicShed` plus a parcel-specific timber canopy, log stack, firewood, compact hoist and plank rack. The building reads more clearly as a workshop than in Art Pass 1, but the primary mass still reads as a small shed/house with production props attached rather than near-final dedicated wood-production architecture.

**Decision: existing art is insufficient without a forced composition. Prepare a dedicated asset.**

### Cuartel
Existing art was reused through `SlavicHouse` plus a subordinate watch element, rear guard wall, command standard, weapon rack, spears and training cue. Functional differentiation improves, but the dominant read remains residential/cottage-like rather than an unequivocal compact barracks/guardhouse.

**Decision: existing art is insufficient without a forced composition. Prepare a dedicated asset.**

### Bastion
Existing castle pieces were recomposed for greater verticality and asymmetry. Technical integration is valid, but the capture review shows roof masses that dominate and visually detach from the masonry. The skyline regresses versus Art Pass 1 and does not meet the landmark-quality target.

**Decision: this composition is not acceptable final architecture. Prepare a dedicated hero asset or tightly scoped hero pieces rather than continuing to stack generic castle fragments.**

## Gate

Run `36450086683`:
- Source preflight: PASS
- EditMode: PASS
- PlayMode: PASS
- Windows desktop build: PASS
- Official Valoria benchmark render: PASS
- Certified building hotspots / panels: PASS through PlayMode regression
- Gate -> Frontier: preserved
- Official zoom/pan/recenter behavior: preserved

Verdict:
- **TECH PASS**
- **INTERACTION PASS**
- **VISUAL FAIL — ART PASS 2 ATTEMPT 1**

Visual failure is deliberate: successful CI is not sufficient for architectural acceptance.

---

# Dedicated asset briefs

Production order is mandatory: **Aserradero -> Cuartel -> Bastion**. Do not spend Tripo credits without explicit owner authorization.

## 1. Valoria_Aserradero_AP2_v1

**Function**  
Dedicated medieval mountain sawmill / wood-production workshop.

**Exact parcel**  
Centre `(-7.0, 0.31, -2.8)`; certified plot **5.1 wide x 5.2 deep**.

**Asset envelope**  
- preferred footprint: about **3.4-3.6 x 2.8-3.1**
- hard visual footprint target: stay within about **4.0 x 3.4**
- desired visual height: **3.2-3.8 above L0**

**Orientation / principal face**  
South/front face toward the official camera, biased toward the central street.

**Must remain free**  
The east / central-street edge of the plot must remain visually open. Do not invade the certified road corridor or cover the existing click target.

**Desired silhouette**  
Asymmetric compact workshop: strong timber-and-stone production mass, readable gable, subordinate lean-to/work canopy and one clear work-frame/hoist accent.

**Required elements**  
- stone footing / lower masonry;
- substantial timber frame and exposed beams;
- coherent functional roof;
- visible wood-production language integrated into the building;
- log/plank storage or work frame;
- compact pulley/hoist only if it strengthens the silhouette;
- believable medieval mountain-city construction.

**Prohibited**  
- generic cottage as the dominant read;
- fortress/tower;
- giant industrial factory;
- fused mountain, terrain, road or stair;
- full diorama;
- decorative clutter that hides the building form.

**Official-camera acceptance**  
At zoom 19 it must read as a distinct productive workshop silhouette; at 12 as unmistakably wood-production architecture; at 9 the structural timber/work details must be legible without blocking circulation.

## 2. Valoria_Cuartel_AP2_v1

**Function**  
Dedicated compact barracks / guardhouse.

**Exact parcel**  
Centre `(7.0, 0.31, -4.0)`; certified plot **5.1 wide x 5.2 deep**.

**Asset envelope**  
- preferred footprint: about **3.5-3.8 x 2.9-3.2**
- hard visual footprint target: stay within about **4.1 x 3.5**
- desired visual height: **3.4-4.0 above L0**

**Orientation / principal face**  
South/front face toward the official camera.

**Must remain free**  
The west / central-street flank must stay open. Do not create a secondary fortress that competes with the Bastion.

**Desired silhouette**  
Robust compact military mass: stone lower body, asymmetric timber/slate upper silhouette, one subordinate guard/watch cue and a clear banner/command anchor.

**Required elements**  
- defensive/robust stone base;
- coherent roof hierarchy;
- guarded entrance or military facade language;
- restrained weapons/shields/banner cues integrated architecturally;
- optional small training/guard annex only if it fits inside the parcel.

**Prohibited**  
- generic residence/cottage;
- large castle tower or keep;
- fused courtyard, terrain, road or stair;
- oversized walls that close the street flank;
- full diorama.

**Official-camera acceptance**  
At 19 it must be distinct from the Aserradero; at 12 it must read immediately as military; at 9 military cues must remain subordinate to the architecture rather than looking like props pasted onto a house.

## 3. Valoria_Bastion_AP2_v1

**Function**  
Primary hero landmark / fortress keep of the district.

**Exact envelope**  
- functional origin around `(0, 3.0, 7.25)`
- landing **7.0 x 4.2**
- central support **7.2 x 4.4**
- click target remains separate at centre `(0.75, 5.0, 5.95)`, size **5.4 x 4.7 x 1.35**

**Asset envelope**  
- visual footprint target: approximately **6.0-6.8 wide x <=4.2 deep**
- skyline: approximately **6-7 units above L1**

**Orientation / principal face**  
South/front toward the official camera and certified stair axis.

**Must remain free**  
The stair mouth and front approach must remain readable. Tallest mass stays central/rear.

**Desired silhouette**  
Monumental asymmetric keep: one dominant vertical rear/central mass, secondary unequal towers, coherent connected slate roofs, strong front hierarchy, deliberate architecture-rock junction.

**Required elements**  
- readable main keep;
- asymmetric tower hierarchy;
- roof masses physically connected to walls;
- coherent front gate/facade;
- controlled rock plinth integration;
- enough silhouette complexity to carry zoom 19 without becoming noisy at 9.

**Prohibited**  
- detached/floating roof slabs;
- symmetric box-castle;
- giant front curtain that hides the stair;
- fused mountain/city/street/stair;
- complete city/diorama;
- generic castle kit composition as final solution.

**Official-camera acceptance**  
Zoom 19 must gain landmark silhouette; 12 must show tower/roof hierarchy and monumental facade; 9 must show coherent connected architecture, not stacked primitives/modules.

## Dedicated Aserradero execution — 2026-09-28

**Integrated checkpoint:** `d633a5482a321a75cc4cf11a4a0e548e6cf8f1bd`

The owner-approved isolated Aserradero reference completed the real automated path:
Tripo Studio -> GLB export -> runner Downloads -> canonical Blender optimization -> Unity import -> real parcel integration -> official zoom captures.

### Tripo / canonical module evidence
- Credits spent: **55**, explicitly owner-authorized.
- Tripo source file: `Valoria_Aserradero_AP2_v1.glb`.
- Source SHA-256: `874e3344d7c09da153fcfeaf012d3d542fdf4d55ca8eb198078646660c4ad37b`.
- Source bytes: **66,112,576**.
- Canonical pipeline run: **36461987549**.
- Pipeline artifact: **10987829034**.
- Raw geometry: **1,796,171 tris / 1,004,637 verts**.
- Optimized geometry: **49,800 tris / 53,609 verts** in Blender report; Unity imported the reviewed mesh successfully.
- Optimized GLB bytes: **14,492,836**.
- UV0: **present**.
- Normals: **present**.
- Materials: **1**.
- Embedded images/textures reported by Blender: **3**.
- Canonical technical gate: **PASS**.

### Real Valoria integration evidence
- Dedicated asset is tracked at `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb`.
- The certified functional click volume remains separate from the visual mesh.
- PlayMode now asserts the dedicated Aserradero exists in real Valoria, stays inside the parcel envelope and keeps the central-street side clear.
- Final Unity run: **36472860786**.
- Full Unity checks artifact: **10992473730**.
- Official Valoria captures artifact: **10991679902**.
- Capture artifact digest: `sha256:3932f0cca11050a4a28596686a3fbafa8e10e968d2f46bbf0b4b4d611accd8cc`.
- Source preflight: PASS.
- EditMode: PASS.
- PlayMode: PASS.
- Windows build: PASS.
- Real Aserradero interaction/parcelling: PASS.

### Visual review
The dedicated geometry is a real improvement over the reused shed: the silhouette, timber-frame/workshop structure and production identity are materially stronger and justify keeping this generated asset.

However, the final official-camera captures still show the Aserradero **too pale / washed out relative to the district**. Several material experiments were attempted; the Unlit route regressed the result and was reverted. The remaining issue is now primarily **material/lighting integration**, not a reason to regenerate the geometry or spend more Tripo credits.

**Aserradero AP2 v1 verdict**
- TECH: **PASS**
- INTERACTION / PARCEL: **PASS**
- DEDICATED ARCHITECTURE / FUNCTION READ: **PASS**
- INTEGRATED MATERIAL / FINAL VISUAL: **FAIL**
- ART PASS 2 overall: **still VISUAL FAIL**

### Production decision
Do **not** regenerate the Aserradero yet. Preserve this geometry and solve its Unity material/lighting integration first. Continue the larger Art Pass 2 only after recording this as the current baseline; do not lose time re-running already proven Tripo/export/optimization steps.

## Selective certified-kit audit — 2026-09-28

The six certified families remain reference/art donors only; none is authorized to redefine the production topology, certified 12-step route or gameplay parcels.

- **TowerWallRock** — eligible only for isolated defensive skyline, short parapet/wall language and architecture-rock junction references around Bastion; never as circulation or repeated filler.
- **TerraceStairRock** — eligible for terrace edge, short stair/elevation language and stepped urban silhouette where the already-approved topology calls for it; not a primary gate or dominant skyline.
- **GateStreetRiseRock MV1** — eligible only for arch/facade, masonry depth and fortified-entry language. Its fused route geometry is explicitly not reusable as Valoria circulation.
- **ResidentialTerraceRock** — strongest donor for inhabited facade/roof language and secondary residential density after primary buildings read clearly. Its large rock base must not become a new island/topology.
- **StreetLandingTransition** — eligible for small parapet/landing-edge visual fragments only; avoid repetition because its fortification read becomes dominant.
- **RockTerrainSeamFiller** — eligible as buried/overlapped rock integration around supports and building bases; never as a focal exposed platform.

Repository audit: none of these six certified family GLBs is currently tracked under production `Unity/Assets/Eldoria/Resources/Valoria/`; their certification sources remain in prior gate evidence/runner sources. Therefore this audit authorizes selective staging when a concrete production placement needs it, but does **not** falsely record any family as already reused in production Valoria.

Decision: the current dedicated Cuartel requirement and Bastion landmark requirement are not solved by whole-family reuse. Certified-family reuse is therefore subordinate dressing/integration, not a substitute for dedicated architecture.

## Aserradero material-integration pass 2 — 2026-09-28

Unity-only checkpoint: `4327d5778f1056fef04b0b31684298027b6eb459`.

The dedicated Aserradero geometry, scale, position, click volume and parcel remain unchanged. The previous uniform material modulation was replaced by a dedicated URP/Lit integration path that preserves the imported base texture while forcing low smoothness, zero metallic/specular/environment reflections and zero emission, with a controlled warm timber/stone tint. This deliberately avoids the previously rejected Unlit regression while addressing the washed-out Lit response.

Status before new official captures:
- geometry: unchanged / accepted;
- gameplay coordinates and colliders: unchanged;
- TECH / INTERACTION: require the next Unity gate to reconfirm;
- MATERIAL / VISUAL: pending capture review.

## Next action

Do **not** repeat Aserradero generation/export/optimization. Its geometry is accepted. In the next larger execution block:
1. solve Aserradero material/lighting integration with a bounded Unity-only pass;
2. audit the six certified Valoria families for selective fragment reuse without letting them dictate topology;
3. move directly into dedicated Cuartel and Bastion production/integration in the same Art Pass 2 block where practical;
4. close with official zoom 19/12/9 captures and real click/panel regression.

The owner explicitly wants fewer micro-passes: a block should create an obvious city-level visual jump, not only infrastructure or one minor polish step.

## Historical Aserradero Tripo pre-spend stage (2026-09-28)

- Exact owner-provided JPEG: `pipeline/art-inputs/Valoria_Aserradero_AP2_v1.jpeg`, 699467 bytes, 1254 × 1254, SHA-256 `4c00ab8b91c57c1872066a40448a1aba5a152fc940ae6bad494302e55f2aa0be`. Its bytes match the chat attachment.
- Safe bridge run `36459555616` at `9346f7c38d97118825729a734ecaf350189b5363`: runner SHA/size verified; Tripo Studio accepted the image and displayed its completed thumbnail. Artifact `10986504233` contains the post-upload screenshot and JSON report.
- Selected Studio setting: **Modelo HD / H3.1 - Máx. calidad** with **Generar en Partes off**, **Textura 8K off**, **Solo para compartir**. Visible button: **Generar 55** (55 credits).
- `Generate` was not clicked; this run spent **0 credits**. Input remains staged in the owner's browser for approval. Generation/export and Blender/Unity evaluation are pending explicit owner authorization for credit spend.
- A staged image proves the safe UI bridge, not the generation/export or complete Tripo-to-Unity pipeline. Do not infer art acceptance from this checkpoint.

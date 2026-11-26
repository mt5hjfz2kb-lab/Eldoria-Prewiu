# Visual QA checkpoint: actual rendered comparisons (not final agent scoring)

Reviewed real WebGL at 390×844: [13 UI pairs](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45824434222) artifact 15125483003 (original 5ef2aee / candidate 3486754), five supplementary matched-progression [building/council pairs](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45837717094) artifact 15126077699 (same revisions), and six city views from the original and candidate journey artifacts 15037184906 / 15125587841. Reviewed [nine native camera pairs](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45834241279) artifact 15127316953 (ff387633 with original colour vs candidate colour, identical camera and same code), including matched route and lattice bytes. The 13 and five paired screenshots were checked against original source ZIP SHA256 and pane pixels. Scores below are my own subjective visual QA on actual rendered frames, rounded to half points, NOT independent studio agent results and NOT grades for the newer d290273. Later scorecards must retain revision and reviewer identity. Rubric: composition/hierarchy, readability, artistic identity, spatial/material integration, finish, each 0–2; total 0–10. Reference images are from 15113766981 (Whiteout Survival HUD, hero/march, mail, analogous rewards; Call of Dragons training, inventory, scout analogue, hero analogue). Different game content/viewport/state prevents literal matching; no numeric comparator where no defensible analogous view exists. This audit rejects acceptance based solely on technical tests.

## UI and contextual panels (5ef2aee → 3486754)

| Role | Before | Rendered after | Delta | Main visible issue / result |
|---|---:|---:|---:|---|
| hud-b1 | 6.5 | 6.5 | 0 | Resource ribbon slightly polished; dock/context composition not a full commercial HUD. |
| hud-b5 | 7 | 7 | 0 | Mature city stronger, but HUD and labels almost unchanged. |
| tutorial | 6.5 | 6.5 | 0 | Hand and sawmill CTA readable; header ornamental interference remains. |
| construction | 6 | 6.5 | +0.5 | Larger real time text, still limited distinction of construction state. |
| name | 7 | 7.5 | +0.5 | Field now integrated; original narrative art and CTA retained. |
| march | 6.5 | 6 | -0.5 | Smaller metrics and troop art hurt readability. ff387633 corrects text; final visual grade pending. |
| bag | 6.5 | 6.5 | 0 | Quiet actions and clearer caption, but reduced item art and wasted grid space offset gain. ff387633 density correction awaits actual WebGL frame. |
| mail | 7 | 7.5 | +0.5 | Clearer quiet tabs and active state; stronger typography and visual list rhythm. |
| daily | 6.5 | 7 | +0.5 | Consistent resource icons and coffer/CTA; calendar still boxy, CTA narrow. |
| heroes | 7.5 | 8 | +0.5 | Strong portrait art; quieter controls reduce ornamental competition. |
| training | 7 | 7 | 0 | Character artwork preserved; live order data still secondary in composition. |
| scouts | 7.5 | 7.5 | 0 | Beautiful illustrated map, cramped tactical rows; rewards differ between sessions. |
| vigilants | 7.5 | 7.5 | 0 | Strong route art, small actionable stats and reward text. |
| council-b1 | 5.5 | 5.5 | 0 | Repetitive rune list, title conflicts with crest. |
| council-b2 | 5.5 | 5.5 | 0 | Repetitive list and small text, scroll needed. |
| sawmill-built | 7 | 7 | 0 | Good contextual art, little visible change. |
| bastion-upgrade | 5 | 5 | 0 | Single action inside tall empty sheet; title crosses frame. |
| barracks-plot | 5 | 5 | 0 | Same large empty sheet and weak action hierarchy. |

## City landscape (5ef2aee → 3486754, scored as landscape excluding HUD)

| View | Before | Rendered after | Delta | Main issue |
|---|---:|---:|---:|---|
| Bastion I | 7 | 7 | 0 | Approved modular character, deliberately sparse start, preserve. |
| Bastion II | 6.5 | 6.5 | 0 | Clear growth, very large homogeneous surroundings. |
| Bastion III | 6.5 | 6.5 | 0 | New buildings connected, limited material distinction. |
| Bastion IV | 6.5 | 6.5 | 0 | Visible progress, uniform broad ground. |
| Bastion V | 7 | 7 | 0 | Coherent streets/districts; improve contact and ground differentiation. |
| Bastion V panorama | 7 | 7 | 0 | Urban composition improved earlier, no measurable gain from this UI revision. |

## World native colour A/B (ff387633, exact camera, original vs candidate palette)

| View | Before | Rendered after | Delta | Main issue |
|---|---:|---:|---:|---|
| home | 6.5 | 6.5 | 0 | Illustrations are strong, too much uniform meadow; water clearer. |
| wood | 6.5 | 6.5 | 0 | Distinct resource art, same sparse ground. |
| stone | 6.5 | 6.5 | 0 | Clearer water, same object grounding limitations. |
| woodland | 6.5 | 6.5 | 0 | Woodlands distinct, still little geographic relief. |
| river | 6 | 6.5 | +0.5 | More recognizable blue channel; banks still need integration. |
| region | 6 | 6 | 0 | Clearer river offset by excessive green/ochre patches. |
| breach | 6 | 6 | 0 | Landmark silhouette retained, ground saturation distracts. |
| panorama | 5.5 | 5.5 | 0 | Regional shape visible, colour still feels flat and noisy. |
| breach-panorama | 5.5 | 5.5 | 0 | Same problem at wide scale. |

## Commercial gate

These real screenshots do **not** establish a global one-point uplift. WOS and COD reference strengths are dense legible hierarchies, recognizable resource/CTA states, coherent spatial materials, confident contrast, and richer action affordance. The artwork should retain Eldoria's own palette and imagery, not clone those games. Follow-up d290273 explicitly responds to observed failures: readable council/building hierarchy; additional portrait tactical space and order-panel typography; more controlled world palette with slope contrast; differentiated mature district surfaces. Its visual result cannot be scored until captured. The two paid independent agent assessments are source-pinned to prior revisions and remain separate from this manual checkpoint. No new charges are authorized by this document. The follow-up native proof is [run 45839167243](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45839167243). No merge, no deployment, no claim of commercial acceptance. 

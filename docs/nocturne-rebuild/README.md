# NOCTURNE — Bastion I–V: isolated art-first experiment

Status: **Blender art generated; native Unity acceptance pending.**
Date: 15 November 2026.
Branch: experiment/nocturne-rebuild-bastion-i-v-20261115.
Base main SHA: 8a2a541689c0f71e7ff5fe2c9d053943edb5b299.

## Artistic contract

Original mature stylized medieval dark fantasy. Aged copper and bronze, dark
slate, hand-cut stone, oiled oak, restrained violet as a corruption signature,
camera 2.5D. Do not imitate a concept screenshot with overlaid sprites.
Do not import the explicitly owner-prohibited old HeroBastion GLB. Do not
modify main/beta scenes or merge until the new production has passed all QA.
Zero paid generation credits or external art purchases.

## Actual physical art

Produced with Blender on Ubuntu GitHub-hosted runner:
- Bastion 1–5: Models/Bastion_01.glb to Bastion_05.glb.
- Sawmill.glb, Barracks.glb, Granary.glb, Stoneworks.glb.
- ConstructionPlot.glb, CityGround.glb, WorldGround.glb, Pine.glb, Rock.glb.
- Four sliced UI sprites: NocturnePanel, NocturnePrimary,
  NocturneSecondary, NocturneDock (PNG).
- One editable master source: ArtSource/NocturneRebuild/Nocturne_Master.blend.
- Five Blender PNG views: ArtSource/NocturneRebuild/Previews/Bastion_01…05.png.
- Repeatable Blender source: tools/nocturne-rebuild/author_art.py.

All geometry is physical GLB, not procedural geometry built in the shipping
runtime. The editable Blender originals are preserved while physical exports
are batched by material for low draw-call count.

Measured by real GLB inspection: Bastion I=2295, II=3407, III=4871,
IV=6259, V=7473 triangles; 12,12,12,13,13 material surface primitives.
14 physical GLBs in total; each under 24 surface primitives. These metrics
verify file structure and geometry only, NOT Unity performance or art quality.

## Mechanical contract

The separate runtime explicitly invokes the original LocalGateway, GameCommand,
SystemClock, FileStateStore, PlayerState and BastionProgressionCatalog. No fake
upgrades, wallet grants or timers are implemented in a button.

Bastion I: gather, sawmill, chapter objectives;
II: barracks, training, march composition;
III: granary, frontier/Fissure and infrastructure;
IV: upgrade and three distinct irreversible development priorities;
V: stoneworks, hunting and later frontier targets.
All progression decisions remain gated by the domain model.

The experiment uses an independent save filename
eldoria-nocturne-experiment-i-v.json. It must never load the production key.

## Authoring the real Unity scene

Source: Unity/Assets/Eldoria/Experiments/NocturneRebuild/Editor/NocturneSceneBuilder.cs.
Runtime: Unity/Assets/Eldoria/Experiments/NocturneRebuild/Runtime/NocturneRuntime.cs.
In a **separate experimental Unity worktree**, use the Unity Editor menu
Eldoria > Experiments > NOCTURNE > Build independent scene,
or the batchmode method
Eldoria.Experiments.Nocturne.Editor.NocturneSceneBuilder.Build.

Expected authored output is the real scene file:
Assets/Eldoria/Experiments/NocturneRebuild/Scenes/Nocturne_Experiment.unity.
The builder fails if any GLB or sprite cannot be imported. It does not
modify production scenes or build settings. SliceBoot only on this branch
skips the separate scene.

## Real test gate not yet passed

The five GLBs and Blender renders exist, and an Ubuntu Roslyn syntax-only
check validates C# syntax. The scene itself has NOT yet been built/played in
Unity. The following are therefore not claimed to be working or approved:

- Unity semantic compile and full GLB prefab imports.
- Editor scene serialization and output prefabs/meta.
- Genuine end-to-end Bastion I–V progression test through all objectives.
- Gamepad/touch input, mobile icon legibility and clickable placements.
- 390x844 and 1600x900 honest Unity captures and aesthetic A/B.
- iPhone Safari WebGL stability, memory measurements, Android frame pacing.
- Fully finished hero/inventory/treasure/mail UI beyond initial read-only views.

In the original project P0 world/iPhone and Senda-Brecha native work are
active. Never take over their shared Windows Unity runner. To run Unity
native QA first acquire its canonical exclusive resource lease in the main
workstream coordination registry; an idle queue is not proof of availability.
This lease is an operational requirement, not permission to merge.

## Learning log and QA policy

- Concept PNG alone is insufficient evidence of a working Unity UI.
- Source Blender assets are editable, not disposable flattened screenshots.
- Avoid one renderer per individual hand-cut block: export batch by material.
- Blender CLI can exit 0 after Python exceptions: check script sentinels
  and actual GLB/PNG plus geometry QA, not just exit status.
- Concurrent Github commits can move the branch during rendering:
  publish via non-forced fetch/rebase/push with bounded conflict handling.
- Numerical geometry QA is not the same thing as aesthetic acceptance.
- Do not touch main or publish a tester beta from an experimental scene.

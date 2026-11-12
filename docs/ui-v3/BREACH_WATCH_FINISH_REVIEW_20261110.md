# UI finish review — 2026-11-10

Scope: existing PR173, original restored artwork. No merge or publication.
Baseline reviewed: e7e4328b2c14d448242c91574315dbdf20e74ec0.
Native images reviewed are from 1cf3f6d049aedbf2d323594f502bc40adef1a758,
not images of the pending corrections.

## Confirmed source defects and corrections

- Cost heading: Label enables BestFit with bounds12..14. Reflow wrote fontSize10/13
  without resetting bounds; large landscape also omitted an explicit size.
  Disable BestFit for this measured inscription and set14 in large landscape.
  Exact embedded Cormorant measurements: 102.4/132u at10pt in900 portrait,
  133.1/145u at13pt in390 portrait,143.3/194u at14pt in1600 landscape.
- Building names: Aserradero measures65.9u at15pt in a54u900-portrait well;
  Bastion alone fits. Fit actual live preferredWidth from each layout maximum,
  down to12pt on large portrait,14pt on small portrait,16pt on landscape.
  Aserradero at12pt measures52.7u. Reset for every selection and reflow.
  No title truncation, abbreviated gameplay names or alteration to commands.
- Native certification now checks real sawmill/Bastion names and cost font
  through five successive orientation changes, including return to portrait.

## Reviewed without speculative changes

- Resource medallion source coordinate/PPU, amount gap and wider capsules.
- Profile and mission padding; retained earlier pending spacing corrections.
- Original navigation images, filtered sampling, active/dormant destinations,
  touch bounds, decorative linkwork and selected tint.
- CTA painted aspect/hammer clearance, cost and timer separation.
- Close target versus visible medal, card title clearance and real 3D crop.
- Screen safe area handling, two/three resource states, two/three active routes.
- No duplicate backdrop, shadow panel, replacement generated icon or new style.

## Verification boundary and follow-up

Source font measurement and52 geometry/unlock scenarios pass locally.
This is not a new Unity screenshot, artistic acceptance or phone certification.
Windows is reserved by the beta/iPhone owner. Native checks must be run after
explicit release. Review all eight resulting images before closing.

Additional stress cases remain unverified: unusually long custom player names,
long unavailable-action explanations in the fixed toast, and large multi-digit
upgrade prices beyond the first-step examples. Their containers are fixed and
warrant native cases before claiming comprehensive I–V text coverage. Do not
hide real costs or lower physical font minimums to make a test pass.

Learning: preferred font size is not proof of actual BestFit size; a fitting
Bastion label is not evidence that the longer sawmill label fits. Inspect the
real font, actual painted well, and both directions of orientation changes.

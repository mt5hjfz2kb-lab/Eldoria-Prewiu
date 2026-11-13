# Native recovery and visual rejection — 10 November 2026

Owner request: finish pending PR173 native validation and show actual UI; Bastion 3D is not the priority.

## Evidence
- Original run 42873624537 stopped on obsolete sprite-name/9-slice expectations.
- Correction 8659557 updated resource identity; first rerun 42877673604 failed due to an overly strict added check equating hit-area aspect with preserved image aspect. Corrected in a3e57c3; the error belongs to this recovery attempt.
- Run 42878041020, exact source 96b6e79ed56049582985e8ef4160513266f278de, succeeded: eight native PNGs, six real navigation interactions, unchanged save, old HUD hidden. Original artifact 13791513964. All eight PNG hashes independently verified after download.
- Actual screenshots compared with original owner reference (Library reference SHA256 f57f1250951c906777e52d5a2e3524d9a6dfea3b64f283da82ad44a4a50e6e13) and baseline native42866907280 / source ff07ae2d9383cd9192289177f0ce93019faffeb0.

## Visual verdict: reject latest three-surface substitution
The source-derived profile, mission and resources introduce cut-off contours, visible background remnants and loss of relief. Source dimensions: profile257x66, mission303x100, resource135x52, lion56x56. These are low-resolution extracted illustrations, not equivalent production-quality 2K surfaces. The prior cleanroom pieces have cleaner silhouettes and better definition. Neither candidate meets owner target7.5/10.

## Action
Restore the five runtime/capture/source-validation files to their last cleaner baseline, retaining rejected source assets and all Git history. No source image altered, no gameplay/3D model/save/release change. Request one exact-head verification of the restored candidate. Restoring technical gates alongside their actual matching source is not artistic approval.

## Accumulated lesson
Do not replace clean high-resolution surfaces with small crops merely because they originate from the approved reference. Original reference is the target; extraction is only useful if contour, transparency, material and runtime scaling survive. Next artistic attempt must produce clean finished surfaces at actual display resolution, inspect those surfaces before Unity, then compare native images. Do not repeat this crop substitution or claim progress from CI success.

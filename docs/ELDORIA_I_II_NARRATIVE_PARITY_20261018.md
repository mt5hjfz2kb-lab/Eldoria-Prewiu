# I–II narrative and scope reconciliation — current integrated Unity

Canonical comparison: `v0220/index.html`, `SliceContentProfiles.WebContract`, native123 integrated scene. This is a continuation of the same `main` scene, not a new runtime.

- Intro now has the original Breach narrator, original Aldric opening and the existing contextual sawmill instruction as three separate turns. Source: web `opening()`, `aldricIntro()` and `tutorialBeat.start`.
- Forest's black mark is narrator followed by Aldric, never two speakers in a single paragraph; the Arcón-specific third turn is intentionally excluded because that mechanic does not exist in Unity I–II.
- Narrator sigil and the exact embedded original Aldric JPEG are used by a reusable component on the existing Royal Gold ModalWindow; typed text, explicit reveal vs continue, speaker identity, English/Spanish, pagination and blocking overlay.
- The journal ledger is in `ChapterProgress.SeenNarrative` and is committed only after the final page. Old saves without the property normalize to an empty list; historical PlayerPrefs are honored only for old existing saves, never a new player.
- Realm costs and active timings remain `OWNER_I_II`, not `QA_FAST`. Training stays at authorized 20-archer batch, prepared march uses available roster: it does not pretend to implement the full original selection controls. Gathered chapter counters are distinct from spendable Wood and total Power is distinct from Expedition Power.
- End-of-available-content language is `Bastión II asegurado · próximo hito Bastión III`; Bastion III itself remains unavailable, so the chapter is not falsely declared completed.
- World ground has no new painted roads; expedition standard interpolates directly to destination and back. Earlier Qwen/Blender assets, city pathways, original scene and local saves are retained.
- The native I–II integration probe uses TestClock and is not manual pacing/physical iPhone proof. Real Unity UI evidence, normal-clock WebGL browser input, signature-verified Android update and private Sites publication have separate acceptance gates.

Artistic approval, mobile performance and dialogue-motion-video quality remain pending independent review until the exact build has passed its respective checks.

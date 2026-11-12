# Issue #186 — actual combined saved-bag gate, native run 43146245415

At exact source `256117b03987216c4a100bfb607d3113c5e74f96`, native Unity imported 16 sprites and passed four isolated PNG/pointer/modal checks. Actual first frontmost resource-capsule click was dispatched (`realFirstHitClicks=1`) but the saved bag never opened. The combined report was `REAL_VIGILIA_SAVED_BAG_FAIL`; no combined bag image, claim/use proof, ART_APPROVED or mobile certification exists.

## Root cause and repair

The optional PR173 bridge correctly installs a Button on the real capsule and forwards the click to `Issue186BagOverlay.OpenOnCanvas`. That method called `Unobstructed()` **before** setting the independent presentation Canvas, then `Open()` called it a second time. The guard interpreted `hud.Navigation.ChronicleLayout` as an obstruction. But `RoyalGoldV2Hud.EnsureView` sets that flag to true as a *legacy footer layout selection*, even when the V2 canvas is disabled by Vigilia. This is a structural contradiction: the bridge offers an interactable capsule while the bag rejects its own approved independent host. This is a source-grounded causal diagnosis, not yet a successful Unity re-test.

The change now allows only the **independent active Canvas** to bypass that legacy footer-layout restriction. Narrative, modal, management, retention, building-selection and disabled-Canvas restrictions remain intact. The legacy entry retains its previous guard. The actual native probe keeps the topmost EventSystem target requirement and now reports the independent Canvas and modal/selection/Chronicle gates if opening fails. No synthetic `onClick.Invoke` is introduced into the tested player interaction.

## Separate cleanup safety issue

The same run's overlay restore correctly **refused to overwrite** unfamiliar bytes in `Assets/Eldoria/BreachWatch/Resources/VigiliaArt/action_antique_bronze.png.meta` (previous installed SHA256 `601b0eaad080719b72676b27e9f8db2839cce5b7e3e4d6ce13374c17a0658a27`). The 245-file manifest and existing original backups were retained; no exact restore was certified. The changed bytes were not included in the uploaded artifact, so attribution to Unity reimport is a hypothesis, not proof. Before the next exclusive Windows native run, archive that exact current file and hash, compare against pinned UI source and Unity importer changes, and restore only if ownership and byte provenance are established. Never run git clean/forced overwrite against this isolated project while it is unresolved. Other workstreams use separate isolated projects and already received Windows by formal CAS.

The next evidence gate is eight **real** combined images, 20 frontmost UI events, actual mail read/claim commands, voluntary Use once, JSON reload and Back, followed by honest A/B against the approved art reference. Full mailbox row UI remains separately unverified. No merge/publication without owner art approval.

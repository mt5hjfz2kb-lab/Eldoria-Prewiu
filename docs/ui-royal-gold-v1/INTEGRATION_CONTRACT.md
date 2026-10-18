# Royal Gold V1 — bounded integration contract

Read-only baseline inspected: Valoria cb076f982a45ff951793413f1fd4bae819efe2dd. This document does not authorize editing shared files.

## Ownership
All production changes stay inside Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV1. ExistingSliceBinding accepts the existing ICommandGateway and IClock; it never constructs LocalGateway, selects a content profile, calls Advance, writes PlayerState or persists. The existing owner alone advances the simulation and performs save/load. A pure read receives one Snapshot for each UI update.

## Host connection (requires Valoria owner's coordinated change)
Instantiate the RoyalHUD prefab once; call RoyalHUD.Bind(new ExistingSliceBinding(existingGateway,existingClock),navigationHost). Route the host's already-resolved building selection to hud.SelectBuilding(id). IDs currently supported: bastion, sawmill, barracks, march-main. No raycasts into the world and no camera are created by the HUD. Navigation host implements IRoyalNavigation; World/Inventory remain hidden without a real available route. The world owner and city owner decide their actual scene names and transitions; the package does not assume an old scene.

## Files to coordinate, NOT edited here
SlicePresenter.cs currently creates old controls and also advances gameplay, pans, zooms and selects buildings. Do NOT simply disable or delete it to remove the old HUD: that would also stop its other responsibilities. The Valoria owner needs a presentation-only opt-out and selection notification (or equivalent integration with its current IPlayableSceneHost). Preserve its single Advance loop, state application and input handling. Replace old controls only in the one coordinated integration change. SliceBoot.cs retains gateway/save ownership. No changes to either file are made by UI.

## Commands and UI state
Costs: SliceRules and active SliceContentProfiles. Power: SliceRules.TotalPower. Objectives: SliceRules.CurrentObjectiveKey. Build, Recruit, AdvanceBastion and ConfigureMarch retain the real gateway's validation, actor, target and displayed revision. A stale revision fails visibly rather than silently reissuing an action. Every submitted offer gets a stable command ID; repeated clicks are suppressed. Remaining times format the existing deadlines against injected IClock only; UI never completes operations itself.

## Touch and modal contract
World gesture handling must test RoyalInputGate.Blocks(screenPoint) on pointer-down and retain that ownership until pointer-up/cancel; do not start camera drags from UI. Existing world gestures must not deliver a selection to the HUD. RoyalButton rejects a click not owned by its pointer-down and any drag beyond its threshold. Modal overlays block the entire viewport and restore focus when closed. No parallel camera controller or second event system in the prefab.

## Acceptance before integration
Native Unity import/compile, 16:9/20:9/4:3 safe-area captures, real event-system clicks, long text, disabled/error states, external scene switching, single HUD/camera, preserved save hashes and real I-II command paths. An authored serialized prefab, source test or HTML/PNG preview is not native evidence. Physical Android/iOS performance requires separate measurement.

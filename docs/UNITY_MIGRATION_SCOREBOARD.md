# Eldoria — Unity Migration Scoreboard

Status: active operational scoreboard.  
Updated: 2026-09-28.

This is not a percentage-complete estimate. It tracks whether a player capability has actually crossed from the web reference into Unity.

States: REFERENCE_ONLY, CONTRACTED, DOMAIN_GREEN, PLAYABLE, UX_GREEN, UNITY_AUTHORITY.

| Capability | Current state | Evidence / remaining gate |
| --- | --- | --- |
| Save/versioned local persistence | PLAYABLE | Unity slice persists and resumes; broaden migration/version tests before authority. |
| Injected clock / offline tasks | PLAYABLE | Construction/travel persistence exists; expand fixtures to later systems. |
| Local command gateway / revisions / idempotency | DOMAIN_GREEN | LocalGateway exists and core slice uses it; keep expanding commands instead of UI mutation. |
| Derived Power model | PLAYABLE | 2452→2622 slice transition implemented; not yet complete I–X formula. |
| Wallet/building upgrade | PLAYABLE | Aserradero loop is usable end-to-end. |
| World node / travel / return | PLAYABLE | First forest/scout route exists; region/content scale still limited. |
| Canonical march composition | PLAYABLE | Initial Aldric + archer path exists under the Unity contract; needs later roster/hero expansion. |
| Deterministic combat report | PLAYABLE | First encounter path exists; later enemy layers and medical effects remain. |
| Bastion I progression | PLAYABLE | First slice completed in Unity. |
| Bastion II / Cuartel / recruitment | PLAYABLE | Timed Cuartel, +12 Archer T1 and Engendro gate are implemented/green in the reported Unity checkpoint. |
| Mission/chapter orchestration I–X | REFERENCE_ONLY | Web remains authority except migrated slice beats. |
| Heroes beyond initial slice | REFERENCE_ONLY | Migrate by playable need, not bulk roster port. |
| Equipment/Forja | REFERENCE_ONLY | Contract fixture first. |
| Hospital/wounded | REFERENCE_ONLY | Web v0.32 is current semantic reference. |
| Códice | REFERENCE_ONLY | Presentation/system migration later. |
| Relicario / Practice | REFERENCE_ONLY | Preserve independent responsibility from Códice. |
| Full Bastion I–X uninterrupted Unity run | REFERENCE_ONLY | Required before Unity replaces web as Arc-I authority. |
| Mobile installed build + device performance | REFERENCE_ONLY | Windows CI is green; device pipeline/budgets still pending. |

## Next migration closure sequence

1. Turn the existing first slice into a formally frozen Unity scenario fixture.
2. Add a migration fixture for Bastion II/Cuartel/Engendro.
3. Move mission/chapter orchestration for I–II into a data-driven Unity contract instead of adding more one-off presentation logic.
4. Certify save/reload/offline across I–II.
5. Then extend one coherent block at a time: III, IV–VI, VII–VIII, IX–X.

Do not start by porting the full UI/catalogue. Close authority on the already-playable slice first.

## Authority rule

A capability reaches UNITY_AUTHORITY only when it is reachable through real Unity interaction, persistence/offline semantics are covered when relevant, targeted automated tests are green, novice UX is understandable, intentional divergence from web is documented, and the owner accepts Unity behavior as the new source of truth.
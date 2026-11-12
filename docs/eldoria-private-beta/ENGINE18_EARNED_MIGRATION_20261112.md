# Engine 18: real earned-save migration certificate — 2026-11-12

## Result and scope

An isolated Windows Edge profile opened the unchanged official Playwright checkpoint earned in engine 17, using the unpublished engine 18 bundle. The actual Unity state retained Bastion V complete, 60 troops and power 10,389. This is a Windows browser compatibility result, not physical iPhone certification or a tester release.

- Native engine source: `de63660d25bee9a32d7b804d6245744ed1f0dbca`.
- Native/real-scene run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43246088947
- Mobile profile package run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43257126363
- Actual Windows migration run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43260864457
- Migration workflow source: `55e5c364090fe4f2470933c605c74c928578f143`.
- Candidate bundle SHA256: `43e2b883d8667c455b5cc9be37ff3fb5abddc6815776e2c7d01ea47c7f87da5a`.
- Input checkpoint SHA256, unchanged before/after: `6b1df345a48681145ecde90ea8ce94b7cc4f4b8909531c8dfed5196b2c65434d`.
- Proof artifact 13962589902; ZIP SHA256 independently verified: `d28c9a1da8527ca4abdb7a7483a944f827315bc3d0191465b91e7306da6f99aa`.
- Actual screenshot SHA256: `7866452984ac472d7c86516a475daf0d314ab6fd705a26142b915299716f3558`.

The browser reported no errors or context loss during the bounded reopen test. No owner save was accessed, no fake game state was created, and no saved game was edited. The original public origin/path was retained while network responses were routed to the exact unpublished candidate. The public deployment remains engine 16.

## Implemented correction

First deferred world navigation now preserves the latest requested quest target across asynchronous region construction. Returning to the city cancels pending navigation. Canonical and frozen UX19 callers use the same host method.

The real Unity test covers first request, replacement, cancellation, reselection, selected quarry without a second tap, repeated region entry and actual disk reload. Native cleanup verified 115 audited inputs with no byte changes. The UI snapshot from PR173 remains outside Unity: its presence does not mean it has been integrated, visually approved, or certified against this mobile build.

## Reproducible findings

- Job-level GitHub Actions env cannot reference the runner context. Put runner.temp in step env/with. YAML parsing alone missed this; actionlint 1.7.12 caught it and accepted the fixed workflow.
- The local Linux browser attempt failed before game launch because socket creation was denied. This is not a game failure or a reopen pass. An isolated existing Windows Edge installation provided the actual test.
- Mac reopen attempt 43251704856 was cancelled before browser startup. Profile18 Apple GPU job was also cancelled before execution, then explicitly retried. Neither cancellation is a passing Apple result.
- An absent optional telemetry collector must not crash an otherwise valid candidate snapshot. Keep telemetry disabled without inventing receipts.
- Native success and Windows compatibility do not establish physical iPhone memory stability.

## Pending

Actual Apple GPU profile18 execution; physical iPhone sustained play and close/reopen; isolated opt-in UI integration and visual comparison. No external tester distribution is enabled by this certificate.

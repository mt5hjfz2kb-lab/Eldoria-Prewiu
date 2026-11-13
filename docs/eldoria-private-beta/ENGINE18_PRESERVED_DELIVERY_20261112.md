# Engine18 preserved owner delivery — 12 November 2026

Native source: `de63660d25bee9a32d7b804d6245744ed1f0dbca`.
Native bundle: `43e2b883d8667c455b5cc9be37ff3fb5abddc6815776e2c7d01ea47c7f87da5a`.
Delivery preparation source: `d90b2bd28db2fb3b23e5440c876e26df7e05b839`.

## Implemented and verified

[Run 43283946677](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43283946677) passed both exact delivery staging and actual desktop ARM WebKit 26 tests.

- Replaces only `beta-iphone-p0-lazy-world/` inside a new local copy of the complete preceding Pages archive from run 43152671564. Other 146 files match both the pinned archive and their live public HTTP SHA256. No browser saves are accessed.
- Reconstructs all 513,656,003 native data bytes and verifies all 31 native chunk hashes using the browser COPY/ADD implementation. Twenty forward operations require 8 MiB literal data. Browser frames were at most 16,384 bytes; no whole target-chunk buffer was introduced.
- Retains the native existing WASM/framework `?v=` URLs and decoded-size keys. Encoded hashes agree with their versions; decoded hashes match the native identity. Treat the full URL as a manifest key; remove the query only when opening its local filesystem path.
- Complete prepared site: 971,317,925 bytes, below the existing 980 MB budget.
- Desktop WebKit: actual nickname entry, resize stress, five actual story clicks, reload, no reported runtime errors. Personally reviewed `reloaded.png` (Beta QA and introductory narrative) and `story-step-4.png` (Bastion I / build sawmill objective). This is not a full browser world-entry test.

Prepared site artifact: `13974700649`, ZIP SHA256 `dc072e36fd5726c30f7ff7263bfa531733540798e893ed8005ecc6f8783446d2`.
Staged desktop proof artifact: `13973923268`, ZIP SHA256 `8079b39828eb1d318ec8e7716288042cb245264baac81187459abf0739f35742`. Downloaded independently and ZIP hash verified before image review.

## Retained failures and lessons

Initial stage source `3e12c85f280e7d4cce31c83577f54a1312b31688`, run 43282381475: data reconstruction passed; packaging failed because its assertion used a stripped filename as the decoded-size lookup key. Fixed the assertion to preserve the existing native URL instead of altering delivery/cache semantics.

Two old source guards expected the previous deferred-world cancellation syntax. They now require the exact new cancellation and retained-objective behavior while still comparing every unrelated UX19 byte with its frozen original. Generic Node suite: 82/82 passed in run 43283946737. No native gameplay change or recompilation was made during delivery staging.

## Publication and remaining limits

Publication verifier tools pinned at `22aaf65b66ebdd835a26ea3fffe5fcbc6a43bb6f`; branch verification run 43286378941 passed before promoting only the existing Pages workflow to main. The workflow preserves the main-only deploy gate and checks the actual public route afterward in desktop WebKit.

The actual public deployment and desktop fallback are verified below. Its separate publication proof and individual job results are authoritative. Physical iPhone/world-entry stability, physical Android, external tester release, and art approval remain false/pending. The owner route keeps its existing path and save origin. This does not merge the historical PR179 or the new opt-in PR173 interface.

## Actual public deployment and earned continuation

Only the Pages workflow was promoted to main at `1cfd64f5064b12546a7ea504ba382670b80ab189`. Run [43286895185](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43286895185) verified inputs and deployed successfully at 19:10 UTC. Pages artifact `13975981075`, ZIP SHA256 `beeb0964b468b433c7c7d01668d884df43cc3221834fb10ec17c61b557327b50`.

Independent HTTP checks after deployment confirmed all three route identities, the exact compressed native18 WASM/framework hashes and every one of the other 146 public file hashes. City14 and beta10 remain unchanged. Browser saves were not touched.

The additional public Mac job `131424695929` was **cancelled before any runner was allocated**, after fifteen minutes. GitHub reports that no hosted runner acquired it and cites macOS ARM capacity constraints. The overall publication workflow therefore is not an all-jobs SUCCESS, despite verified deployment. This is not a tested game crash. The earlier exact staged WebKit run43283946677 remains a genuine PASS; no public WebKit PASS is claimed.

A real public Edge fallback [43290448094](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43290448094), QA source `1de0f02e049093c908b3fa79f3ada391033f92c5`, **passed**. It loaded the real public URLs with no request routing, local asset fulfilment or personal browser profile. The unchanged officially exported earned17 checkpoint (`6b1df345a48681145ecde90ea8ce94b7cc4f4b8909531c8dfed5196b2c65434d`, 130908 bytes) retained Bastion V complete, 60 troops and power10389. No page errors or context loss were observed. The input checkpoint stayed unchanged.

The two actual taps were checked with read-only `elementFromPoint` to ensure the game Canvas received them. Original `after-world-control.png` shows the world, bridge, Enter Valoria and City control; `after-return-control.png` shows the level5 city again. Both images were inspected independently after downloading and verifying artifact `13975239739`, ZIP SHA256 `c7c495fafe373b02536e176767d498ac2e7498f07837c8531c12c06c9d21c67b`. The machine report intentionally leaves `worldEntryVerified=false` until that separate visual inspection; its original bytes were not modified.

Screenshot hashes: world `a4943933fa0578b310ae9646597272545dcece611f6bb63a18624ff8de8512fd`; return `2f725f0f4ea8eb7d68c2dffe68ac6c16e7a6ea1aac95ed367f5cde881da412ca`.

Retained harness lesson: the first public Edge run43289610287 genuinely proved earned-save continuity, but its y815 taps opened the diagnostic HTML overlay. Those images did not prove world entry. The corrected y790 taps target the observed native icon above that overlay, and the explicit Canvas-hit guard prevents the same false interpretation. No published game code changed during this correction.

Windows lease released after the browser job completed. **Physical iPhone, physical Android, long device sessions, art approval and external tester release remain pending/false.** Public owner trial: `beta-iphone-p0-lazy-world/?eldoria_p0_lazy_world=1&eldoria_diag=1&build=mobile18`.

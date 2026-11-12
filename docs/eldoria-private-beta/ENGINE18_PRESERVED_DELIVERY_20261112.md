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

Publication verifier tools pinned at `22aaf65b66ebdd835a26ea3fffe5fcbc6a43bb6f`; branch verification run 43286378941 must pass before promoting only the existing Pages workflow to main. The workflow preserves the main-only deploy gate and checks the actual public route afterward in desktop WebKit.

At the time of this note, public publication is not yet certified by this document. Its separate publication proof and public workflow result are authoritative. Physical iPhone/world-entry stability, physical Android, external tester release, and art approval remain false/pending. The owner route keeps its existing path and save origin. This does not merge the historical PR179 or the new opt-in PR173 interface.

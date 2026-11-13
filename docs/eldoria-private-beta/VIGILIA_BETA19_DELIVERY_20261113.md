# Integrated Vigilia delivery

The owner authorized integration and beta publication. PR179 merged as
`f1c8dd673ee74072d4bcf646da1413cd551ebaf6`, retaining current main
`874eb50be38a181735414cf629a5fc92ee462b89` and exact candidate
`fb8727d3ac45e31e61ab8ba244fa395477eff16e` as parents. Canonical PR173,
PR181 and PR187 are now merged too.

## Verified source and artifacts

- Integrated native/WebGL/package/WebKit run43329543960 succeeded.
- Full certificate run43329553450 succeeded: 218/218 EditMode, 79/79 PlayMode.
- Terrain replay: two passes of119870 queries,2142 sites and17 cities; geometry,
  queries, routes and saved bytes preserved. Native I–V journey passed.
- Native proof13995963431 SHA256
  `0c4ecb3a05f3aa71f2947affe1e3ad4c3a2a1cc5f06b4f7505bc8d1396f75d87`.
- UI proof13995488869 SHA256
  `05b512d3f43612ab016b4e51873dfc0e0e4d7b01f7b4526b6d5922f8c1cfa11e`.
- Packaged candidate13995794244 SHA256
  `34b1177ff9d98d1feb5c97a041827dc5c337f0ceb15c73049522efc8afb44849`.
- Packaging proof13997156118 SHA256
  `f82c4ec39ee346d430f6452130951b4d1a638e54098393816f5c7ce698be134e`.
- Desktop WebKit13998070186 SHA256
  `67910a05daa2d77f07b452ea205c4213fcd5f81853de7929bcb6e1df17d935a7`.

These five archives were downloaded and their hashes independently checked.
Actual WebKit screenshots show the entered name Beta QA before and after reload.
Vigilia installed twice; no browser errors. Peak WebContent RSS was2710256 KiB
for beta18 and2818464 KiB for the candidate. Desktop evidence is not iPhone
certification. Exploradores and Vigilantes remain visually provisional.

## Publication preparation

The candidate contains531369638 decoded data bytes and its packaged ZIP is
480134834 bytes. Its bundle SHA256 is
`eb9192488a6cc34dd38309f48cdd7fdb3dc3ea5a2d6fb7dc23a023d8e23b0048`.
All57654 remaining objects were verified after exact deduplication and reuse
of157 byte-matched, previously shipped mobile texture transformations.

The original copy/add strategy cannot compact this new bundle adequately:
measurement found516292608 literal bytes,442422519 gzip bytes, despite only
17720962 more decoded bytes than beta14. Keeping all historical builds would
exceed Pages capacity. Do not relax the original980000000-byte staging budget.

The prepared publication instead retains beta14 and beta18 byte for byte,
adds the new `beta-integrated-vigilia` route, and archives beta10 plus the old
clock diagnostic. Their entry URLs become explicit archive notices linking
to the stable and new betas. Before any deployment, the complete original
Pages tar must be uploaded to the draft release
`archive-pages-before-vigilia-20261113`, downloaded again and compared exactly.
The original artifact is13975981075 from run43286895185; ZIP SHA256
`beeb0964b468b433c7c7d01668d884df43cc3221834fb10ec17c61b557327b50`.

The new route has separate URL-based WebGL storage; beta18 and its existing
player storage are preserved. This is an owner test candidate, not an automatic
player-save migration or an external-tester release.

The delivery workflow tests the exact staged route in WebKit before deployment,
requires durable archive verification, checks unchanged public stable files,
then verifies the live identity and repeats WebKit on the actual public route.
Physical iPhone and final art approval remain false. Publication is pending
until those jobs actually finish; this preparation document is not a release.

## Main validation ownership

The same integration workstream moves to main at ownership revision65 for
postmerge validation and delivery. The main UI certificate accepts that exact
postmerge phase and keeps every native test. The old main invocation43337524782
uses the premerge branch-only lease rule and cannot enter Unity. Delivery may
cancel only that exact invocation while queued or in its first lease wait;
it must refuse cancellation if any later step has started. No live Editor
may be cancelled. A fresh main certificate runs with the updated lease rule.

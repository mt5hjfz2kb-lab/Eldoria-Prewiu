# Unity metadata recovery and prevention

Run from the repository root (use --archive-manifest to verify archived source blobs):

```sh
python3 tools/unity-metadata/audit.py
python3 -m unittest discover -s tools/unity-metadata -p 'test_*.py' -v
```

The audit reads the committed Git tree, alongside metadata bytes in the checkout.
The sparse CI workflow materializes every tracked Unity `.meta`, including those
belonging to folders. Missing checkout bytes are errors, not proof that a Git file
was deleted. To inspect a prepared Git tree, use `--ref TREE_SHA`.

It rejects new missing file or folder metadata, invalid/duplicate GUIDs, orphan
file metadata and ignore rules that exclude expected metadata. Empty-folder
identities are retained because Git cannot represent an empty directory.
Contents of bundled plug-ins such as `.androidlib` are treated as one plug-in;
the bundle root still needs its own metadata.

`baseline.json` has no allowed missing metadata or orphan exceptions after this
recovery. The audit therefore requires zero outstanding integrity debt. The
baseline format can pin a historical file exception to its exact asset blob, but
no such exception is currently enabled. Static integrity is separate from native
Unity import and gameplay certification.

`snapshot.py` copies existing metadata into a **new** output directory. It never
opens Unity, generates a GUID, edits a project or installs software. It rejects
linked paths, changing sources, oversized metadata and exhausted hashing budgets.
For files, the exact Git blob must match; only explicit text types allow an exact
Git CRLF-to-LF checkout conversion. Binary assets are never normalized. For
folders, only stable existing folder identity is recovered; equality of all folder
contents is **not** asserted.

The snapshot workflow has no checkout step. It reads bounded runner workspace and
temporary project locations and uses an existing Python interpreter. The ordinary
Windows `python` alias can point to Microsoft Store; interpreter discovery must
exclude that alias and validate the executable. Do not launch Blender just to use
its bundled Python interpreter.

Recovery provenance and exact remaining work:
[2026-11-04 recovery](../../docs/evidence/unity-metadata-20261104/RESULT.md).
Preserve original importer bytes, including whitespace emitted by Unity. Do not
clean those files merely to satisfy a whitespace linter.

Authoritative references:

- [Unity 6000.3 asset metadata](https://docs.unity.com/en-us/engine/6000.3/manual/assets-and-media/import-assets/asset-metadata)
- [Unity plug-in import and bundled folders](https://docs.unity3d.com/cn/2023.2/Manual/PluginInspector.html)

## Native proof modes and checkout bytes

The request must explicitly select `identities_only` or `full`.
`identities_only` compiles/imports and checks original GUIDs, importer bytes and
asset/typed SceneAsset references; it does not open scenes, run game tests or
build a Player. Its certificate is `IDENTITIES_PASS_ONLY`, not a full game PASS.
`full` additionally opens enabled scenes and runs EditMode/PlayMode; it refuses to
start while the expressly rejected legacy Resource remains in the project.
Neither mode publishes or builds a Player.

Pin BOTH `core.autocrlf=false` and `core.eol=lf` before the first checkout.
`* text=auto` still converts checkout bytes to CRLF with Windows/native core.eol
even when autocrlf is disabled. A real Git regression fixture reproduces this.
Every recovered SHA-256 is checked **before** Unity starts. Any subsequent byte
difference is captured as raw metadata plus a path/hash mapping, rather than
assuming a checkout difference was an importer rewrite. Scenes are loaded as
`SceneAsset` Editor references, never as sub-asset arrays or instantiated objects.
Runner temporary files are removed between jobs; do not rely on them as caches.

The existing owner-rejected legacy Resource is forbidden in tracked Unity paths.
The archive ledger can be checked using Git tree blob identities without checking
large model binaries out. Native full validation also refuses that Resource before
opening an Editor. Archived source bytes and GUID are preserved; ordinary asset
reuse and all artistic approval remain blocked. The two existing catalog locators
are updated, not promoted. Historical pending recovery copies are referenced by
commit in the manifest after their exact restored destinations were verified.

Full PlayMode uses the canonical `VALORIA_GAMEPLAY_PROBE=1` disposable save
fixture and a new `ELDORIA_REGION1_CAPTURE_OUT` directory. NUnit acceptance requires
61+ PlayMode cases and every selected case Passed, with no skips. Both suites must
identify assemblies inside the exact project's Library/ScriptAssemblies directory;
zero tests, foreign project output or a Passed root hiding skips are rejected.
The contract has real PowerShell XML/disposable-fixture regression checks.

Full validation also reproduces the existing unity-slice.yml preparation:
unchanged author-frontier.py in the installed Blender, then BastionIIIArtBake.Build
and the five original build-safe prefabs. These are isolated derived test/build
outputs, never a new art source or a committed model. Omitting this preparation
causes BASTION_III_MISSING_ART and invalidates gameplay acceptance.
Tracked state/differences are captured even when a test fails; no source reset is
used to hide a dirty checkout. Only the run's own timed-out processes are stopped.

Windows PowerShell redirected Start-Process can expose a null ExitCode unless its
handle is retained before waiting (PowerShell/PowerShell issue5421). The logged
launcher caches that handle, rejects missing codes and records the real result.
A real child-process contract checks exit0, exit7 and both redirected streams in
CI and again on the Windows host before the expensive clone/Unity preparation.
The Blender export marker alone never authorizes PASS.

The canonical preparation calls SaveAssets, which serializes older existing
TextureImporter stubs and vendor assets. Native41832431346 passed all 345 original
identities/bytes, 57 EditMode and 73 PlayMode cases, but the original blanket
pristine-checkout assertion rejected 61 other baseline files. The exact
source/observed Git blobs are recorded in native-serialization-baseline.json:
27 existing metadata files retain their GUIDs, 32 vendor assets and two project
settings are serialized. Every source blob matches pre-repair main. These files
are neither committed nor artistically approved. Unknown paths, changed source
blobs, changed results or overlap with any recovered entry still fail. The
certificate distinguishes a pristine checkout from these explicit side effects.
All 345 raw hashes are checked again after the tests.

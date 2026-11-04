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

`baseline.json` records precise existing debt protected by concurrent production
work. File exceptions include the asset blob SHA: changing an asset cannot quietly
inherit the exception. Folder exceptions identify existing folders, not permission
to omit metadata in new ones. Passing this check means no new integrity failure;
the output still reports outstanding known debt. It is not Unity certification.

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

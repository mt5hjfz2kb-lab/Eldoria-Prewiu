# Reversible input preservation for the isolated native checkout

The actual R19/R25 investigations showed that Unity can serialize importer metadata and empty Android icon slots while running native evidence. Future treasure source dispatches must preserve these private QA inputs before switching commits.

The new helper archives every raw changed tracked file, records the original HEAD and Git blobs, and refuses source-code/scene/material changes. For importer metadata and PlayerSettings only, it additionally retains and verifies the complete working Git state in a private local preservation ref before restoring the canonical input paths for the next pinned source. It never clears Library, removes untracked notes or changes a source branch. These archived bytes are not classified as harmless or discarded; they remain exactly recoverable.

The subsequent checkout is normal, without --force, so an untracked collision remains refused. A real Git fixture tests original input recovery, the saved working-state ref, byte-identical archives, cache/untracked preservation and refusal of an unfamiliar C# edit. Native art approval still requires actual images.

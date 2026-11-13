# Exact native input recovery

Run43157612284 refused to overwrite two non-importer files before launching Unity. Artifact13916990677 retains every changed byte. CPU audit43158308069/job130983331098 compared the actual files with original Git blobs: package lock reordered existing module records and added builtin Audio1.0 required by the existing native workflow; QualitySettings changed antiAliasing0→4 and customRenderPipeline48e3a09b57f028a4bb2e693d044ea6ca→2107f16f9344623478727c7dc4792d39. This is inspected source state, not artistic evidence.

Recovery accepts these two exact path/original-blob/working-blob/raw-SHA256 quadruples only. It archives original bytes, saves the complete Git working tree under an immutable local preservation ref, verifies every working blob, and restores only the recorded tracked inputs to original Git blobs before normal detached checkout. Any new quality/package/code edit is refused and preserved. No clean or Library/cache removal. Ten actual Git assertions include acceptance of the exact pair, reversible working-state backup, and refusal after one unknown settings edit.

Cinematic art still requires actual new native camera captures, comparison with source2418d3d, and owner review before main. No native pass or art approval follows from source recovery.

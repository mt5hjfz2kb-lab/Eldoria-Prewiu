# Coherent native route comparison and exact restoration

## Observed on Windows
Native 43127449523 (PR179 source7824f307) recovered exactly nine known tracked material differences, preserved the immutable original115dirty inputs, and passed real lazy-region PlayMode. Structural terrain baking completed, then route-baseline failed CS0117: the historical pre-buffer-reuse route source lacks CreateStaged, now required by the staged placement service. The strict wrapper retained that one known intermediate instead of overwriting unknown source. Existing dirty input hashes were unchanged. Evidence13904300414/13904425261; no full-world WebGL or phone certification.

## Implementation
The new comparator uses the current route class with identical staged grid construction, iterator API, query ordering and caller. Only reachability dictionary/queue storage reuse is reversed, isolating the actual allocation comparison. The historical comparator remains immutable as recovery provenance, never as new runtime input. The wrapper accepts the exact new authored comparator as a known intermediate and restores starting bytes after failure as well as success.

A separate guarded recovery function recognizes only the ONE historical route intermediate left by43127449523. It requires the frozenHEAD and unchanged index, all immutable original dirty hashes, the sole expected unstaged route delta and exact Git-normalized comparison-source bytes. It archives those bytes before restoring that one tracked file; unknown, staged, mixed or dirty-baseline changes fail before mutation. No clean/reset or adopted baseline.

## Verified off Windows
Actual Git plus portable PowerShell:26route recovery checks across exact/CRLF/no-op/unknown/foreign/staged/dirty/wrongHEAD. Existing58material checks remain. PowerShell parsing passes. These checks verify guards and cleanup, not Unity APIs or physical-device stability. The next exclusive native batch must execute actual parity, full I–V journey, WebGL build and strict starting-input restoration. Windows was released to181 while this source work proceeded;187 follows181.

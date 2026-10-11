# Historical visual residue cleanup v1

This pass protects the current Valoria production state while quarantining superseded visual experimentation.

## Result

- Current Valoria production scene, correct visual authority, SHARP production lineage, real-game-state evidence and closed production families are **protected / untouched**.
- Repository inventory found **244 paths** matching clearly historical visual families (Camera-First/2.5D, Tripo-era tooling and retired generator capability probes).
- Camera-First / semantic 2.5D is already explicitly rejected by `AGENTS.md` as a production pipeline and must remain historical/manual-only.
- Historical evidence is therefore **classified and quarantined logically**, not destructively deleted. This follows the repository's own instruction to preserve rejected-method evidence.
- The active mobile WebGL playtest workstream is outside this cleanup scope and is untouched.

## Permanent rule

Anything matching `pipeline/historical-visual-residue-v1.json` is historical-only. It cannot be treated as current Valoria visual authority, production input, or a production workflow unless the owner explicitly reauthorizes that method.

## Destructive deletion gate

A historical file may be physically removed only when all three are true:
1. it is not evidence required by repository governance;
2. it has no current production/runtime/tooling reference;
3. removal passes the relevant build/test validation.

Ambiguity means keep.

This pass deliberately prefers safety over repository-size reduction.

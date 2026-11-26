# Eldoria Studio V3 — autonomous execution pilot

Separate scope: tools/studio-v3 only. Frozen V2 source: 1657b2caa6b1a955319538ce3a8b2906398599b3. The active Corona V2 missions retain their branch, budgets and Windows reservation. No spend, no Unity, no merge.

## Implemented
- Bounded offline mission engine, exact source snapshot and trusted external role adapters.
- The producer supplies target, strategy and declarative exact text edits, never shell commands. Candidates are applied in memory from the original baseline; ambiguous, duplicate, unsafe or out-of-scope changes fail closed. Never execute proposed code in the host.
- Required checks are bound to candidate digest, then a separate reviewer audits. Technical pass does not approve art. The engine returns a candidate only; no promotion or production write.
- Failed checks feed session evidence back. After two failures with the same strategy, require a pivot. Reject repeated candidates; bound attempts. Persist checkpoints through a trusted callback.
- Paired benchmark requires same source, target, objective, protocol, constraints and blind reviewer. Unknown cost or quality remains null.
- The trusted host must verify Git provenance and sandbox any later code execution. The role callbacks are trusted application code, not arbitrary tools. No paid calls in this version.

## Verification
`node --test tools/studio-v3/core.test.mjs` (Node SHA-256). See evidence/core-fixture-20261126.json for actual core execution with fixture adapters in a JS isolate; it is not a Node CI or live-agent benchmark. First run failed on mutable retry state; corrected and retested.

## Pinned-input replay
Run `node tools/studio-v3/replay.mjs` in a checkout that has the pinned V2 commit. This read-only fixture uses the actual V2 lessons and quality decision, deliberately proposes two invalid changes, pivots, and writes only an append-only candidate and journal to a fresh temporary directory. It does not update V2 learning automatically. The separate offline workflow runs Node tests and replay without secrets or Windows.

## Pending
Attach V2 role/economics and explicit budget gates, with no automatic paid retries. Use a paired real product task against a single agent, identical validation and blind quality review. Unity/Blender adapters require their own isolated execution and resource lease. Permanent lessons require evidence and independent curation. No measured productivity or artistic gains yet.

## Verified evidence and offline role transport
The [first actual Node CI run](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45887151636) passed 19 tests and replayed two deliberate defects followed by a pivot on actual pinned V2 files, preserving six existing lessons. The exact proof is archived in `evidence/node-replay-20261126.json`. The new role bridge binds typed external output to a request digest, role identity, target, source and attempt, then sends it to the same executor. It accepts only offline fixture/archived role output and retains a receipt trail; it cannot call the API or authenticate a model by itself. A trusted V2 adapter with an explicit budget authorization is still required for live calls. New regression coverage rejects previously exhausted strategies and contradictory quality audits.

## Actual V2 client compatibility (offline)
`v2-role-adapter.py` reuses the pinned V2 `studio.make_payload`, `studio.report`, routing and economics modules, binds typed proposals to the exact request and receipt, and refuses text-only visual approval. Cross-runtime Node/Python tests exercise rejection, correction and independent audit with scripted, explicitly labelled fixture responses; no network or secret access. No live entrypoint. A separate immutable spending authorization and trusted runner must be designed/approved before live use; a previous V2 prose report cannot be retroactively called a typed V3 response. The accepted fixture is not a real model-quality benchmark.

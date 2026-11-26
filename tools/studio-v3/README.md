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

## Pending
Add a trusted scratch/persistence adapter and prove it on actual pinned V2 inputs; then attach V2 role/economics and explicit budget gates, with no automatic paid retries. Use a paired real product task against a single agent, identical validation and blind quality review. Unity/Blender adapters require their own isolated execution and resource lease. Permanent lessons require evidence and independent curation. No measured productivity or artistic gains yet.

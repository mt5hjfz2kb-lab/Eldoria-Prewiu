# Eldoria Parallel Workstream Protocol v1

Status: **CANONICAL PROCESS**  
Effective: 2026-10-02

## Purpose

Prevent two ChatGPT/agent sessions from silently converging onto the same Eldoria work, duplicating experiments, overwriting canonical state, competing for the Windows runner, or spending Tripo credits twice.

The repository remains the source of truth. This protocol adds a small coordination layer; it does not replace project-state documents or certified evidence.

## Canonical registry

Live ownership is stored in:

`pipeline/active-workstreams.json`

Every substantive session reads it before mutation and claims exactly one coherent workstream before beginning execution.

## Claim procedure

1. Read `AGENTS.md`, `SESSION_HANDOFF.md`, `PROJECT_STATE.md`, the live registry, and current `main` HEAD.
2. Define a narrow workstream ID and scope.
3. Compare the proposed scope/resources against every entry in `active`.
4. If there is no conflict, update the registry using the blob SHA just read.
5. If GitHub rejects the write because the SHA is stale, treat that as concurrent activity. Re-read the registry and reassess before retrying.
6. Only after the claim succeeds may the session perform substantive mutations or dispatch shared expensive/heavy resources.

Example active entry:

```json
{
  "id": "valoria-world-composition-v3",
  "title": "Valoria World Composition v3",
  "owner": "chat-valoria-composition",
  "scope": [
    "Unity/Assets/Eldoria/Runtime/Valoria/VisualWorld.cs",
    "docs/VALORIA_*"
  ],
  "resources": [
    "valoria-production-scene",
    "windows-runner-heavy"
  ],
  "status": "active",
  "claimed_at": "2026-10-02T10:40:00Z",
  "base_main_sha": "<sha>"
}
```

## Conflict rules

A conflict exists when two active workstreams would mutate or control the same logical production surface. Exact filename overlap is sufficient but not required.

Treat these as exclusive unless explicitly narrowed:

- the same canonical source file or production request;
- the same Valoria district or asset family;
- the same promotion target;
- Tripo generation/credit spend for the same asset or reference;
- Windows-runner heavy Unity certification when concurrent execution would cause contention or invalidate evidence;
- recovery/hotfix work targeting the same failure;
- project-state closeout for a block still owned by another active session.

Read-only research does not need a claim if it cannot mutate project state, occupy shared runners, or spend credits. The moment it becomes implementation, it must claim.

## Dependencies without collision

One workstream may use outputs already committed/promoted by another. It may also record a dependency on an active workstream. It must not take over unfinished scope.

If work A discovers that the correct next step belongs to active work B:

- do not execute B;
- record the dependency in A if useful;
- continue A only where scopes remain independent;
- consume B's result after B releases/completes it.

## Shared runner rule

The Windows self-hosted runner is not globally locked for all work. Only jobs that would materially contend or invalidate one another need the exclusive resource `windows-runner-heavy`.

Short zero-credit/read-only probes may coexist when safe. Tripo spend remains governed by the separate explicit owner-authorization gate regardless of workstream ownership.

## Main reconciliation

A workstream claim does not freeze `main`. Before each substantial promotion/merge/closeout:

1. re-read live `main` HEAD;
2. re-read the registry;
3. ensure another workstream has not changed a dependency;
4. adapt to the newer canonical state rather than restoring an older snapshot.

The rule `repo wins over chat` remains unchanged.

## Release and handoff

When the workstream is done, move it out of `active` into `history` and record, when applicable:

- final status;
- resulting commit SHA;
- workflow run/artifact IDs;
- blocker or handoff target;
- completion timestamp.

Allowed terminal states: `completed`, `blocked`, `handoff`, `cancelled`.

Do not leave stale claims merely because a chat ended. A resumed session must inspect and either reclaim/continue its matching entry or clean it up based on real repo/workflow state.

## Scope naming guidance

Prefer stable purpose-based IDs:

- `asset-library-reprocessing-v2`
- `valoria-world-composition-v3`
- `toolchain-automation-v3`
- `hero-bastion-surface-pass-v2`

Avoid generic IDs such as `work`, `test`, `continue`, or IDs based only on a chat number.

## Failure mode this prevents

Without this protocol, two sessions can start from the same `main`, independently decide that the same bottleneck is next, and eventually both modify the same assets/pipeline. Because both correctly obey "repo wins", they then converge further and may duplicate execution.

With the registry, convergence becomes an explicit dependency instead of duplicated ownership.

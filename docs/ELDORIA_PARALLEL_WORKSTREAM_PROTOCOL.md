# Eldoria Parallel Workstream Protocol v1

Status: **CANONICAL PROCESS**  
Effective: 2026-10-02

## Purpose

Prevent two ChatGPT/agent sessions from silently converging onto the same Eldoria work, duplicating experiments, overwriting canonical state, competing for the Windows runner, or spending Tripo credits twice.

The repository remains the source of truth. This protocol adds a small coordination layer; it does not replace project-state documents or certified evidence.

## Permanent continuous-execution rule

Once an agent successfully claims a workstream, it must continue autonomously through diagnosis, correction, rerun, validation and closeout. **Progress updates are informational only and MUST NOT suspend execution.** The agent must not wait for the owner to say `continúa`, `sigue`, `reanuda` or equivalent when the next action is already determined by the repository, existing evidence, canonical planner/routing, tests or zero-cost tooling.

A failed gate is the beginning of the next bounded diagnostic/correction cycle; it is **not** a valid reason to end the work session. Completing a sub-step, producing an artifact, dispatching a run, reaching a new phase, needing to inspect captures, or needing to reroute through the canonical planner are also not valid stopping points.

An owned workstream may stop only when one of these terminal conditions is true:

1. **COMPLETED / CLOSED** — the requested work is actually finished and all required technical, gameplay, visual and/or release gates for that workstream have passed with the required evidence.
2. **GENUINE HUMAN BLOCKER** — progress requires a decision or input that cannot be derived safely from the repository, existing evidence, canonical tooling or an available zero-cost route. Examples include fresh authorization to spend money/credits, credentials the agent cannot access, an irreducible owner-only creative choice between incompatible directions, or an external dependency that has no viable alternative path.
3. **SESSION/PLATFORM INTERRUPTION** — the execution environment itself is no longer available. This is not a project-state completion and must never be recorded as one. On the next live session, the matching workstream must be reconstructed from `main` and resumed without waiting for the owner to restate prior instructions.

Not valid reasons to stop include:

- a workflow/run is still executing when its result can be checked in the same live session;
- a test or gate failed but the failure is diagnosable/correctable;
- one phase or artifact completed while the workstream remains open;
- captures still need visual inspection;
- the planner/fallback router needs to be rerun;
- a known zero-cost alternative remains available;
- the agent already sent a progress message;
- the owner is offline/asleep;
- the task is long or has required many tool calls.

If a run must finish before the next action is known, the agent should continue checking it while the session remains live and proceed immediately when the result is available. A progress message must never be phrased as a request for permission to continue unless a genuine human blocker exists.

This rule is permanent project governance. Any future prompt, chat habit or local workflow convention that asks an agent to stop at intermediate milestones is subordinate to this rule unless the owner explicitly instructs that specific workstream to pause.

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

# Native Windows proof safety (no PC allocated to this branch yet)

This Unity 6.3 workflow intentionally **skips the Windows job unless the live
`main/pipeline/active-workstreams.json` registry contains exactly one
`windows-runner-heavy` holder, whose **claimed branch is
`art/valoria-spatial-coherence-20261118`** and whose ID equals the
`resource_rotation.current_workstream`. It validates again on the runner.

As of this authoring, the PC is **not assigned to this branch**; this workflow
is a safe source-only preflight and is NOT an executed Unity test. The owner
of `art/valoria-25d-profundidad-piloto` retains its registered claim.
Never edit that workstream from here, force its release, duplicate its PR, or
infer PC availability merely because no job is running.

When the recorded owner legitimately hands over an exclusive turn, re-run this
workflow from the exact candidate SHA. It uses an isolated shallow sparse
checkout in a unique RUNNER_TEMP directory, does not change main or the
functional work, checks for Unity/Blender processes, renders 104 actual PNG
samples in RenderTexture with SHA manifest, archives them, and deletes **only**
its own disposable checkout with a guarded long-path-safe cleanup helper.
No capture/PlayMode/mobile/artistic pass may be claimed until original Unity
evidence is inspected. Do not publish a WebGL beta automatically.

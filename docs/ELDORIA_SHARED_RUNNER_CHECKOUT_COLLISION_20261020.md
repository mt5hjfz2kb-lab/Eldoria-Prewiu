# Shared runner checkout collision — 20 October 2026

Confirmed in completed Tripo run39495619494, job118879787295: checkout logged removal of valoria-layered-source/* and deletion of the workspace contents around20:53 UTC. This discarded the cached Valoria project, not its committed sources. Native238 run39495755210 consequently needs a fresh checkout/import.

Minimal reversible protection: Tripo's existing checkout now sets clean:false. Generation, request, budget, Unity checks, artifact gates and promotion remain unchanged. The current root repository already has a valid checkout; preserving untracked nested worktrees prevents the destructive recursive git-clean on subsequent runs. No new workflow, branch or project was introduced. Use the updated main workflow for the next request; rerunning an old commit uses the old destructive checkout configuration.

The Tripo run ended with its own isolated Unity timeout; that task remains its owner's responsibility. Do not cancel it or treat its failure as a Valoria art result. Valoria source/evidence and current request remain in the single main workstream.

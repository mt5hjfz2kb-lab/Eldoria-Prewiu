// Coordination only. This never grants spend, publication, or process access.
export function requireLiveLease(snapshot, live, id, resource = 'windows-runner-heavy') {
  const find = (registry, label) => {
    if (!Array.isArray(registry?.active)) throw Error(`${label}: active registry missing`);
    const matches = registry.active.filter(w => w.id === id);
    if (matches.length !== 1) throw Error(`${label}: expected one ${id} workstream`);
    const w = matches[0];
    if (w.status !== 'active' || w.branch !== 'main' ||
        typeof w.owner !== 'string' || !w.owner.trim() ||
        w.ownership_state === 'available' || w.write_scope_released === true)
      throw Error(`${label}: claim the available workstream before dispatching a heavy job`);
    if (!Array.isArray(w.resources) || !w.resources.includes(resource))
      throw Error(`${label}: ${resource} reservation missing`);
    const revision = w.ownership_revision ?? 0;
    if (!Number.isSafeInteger(revision) || revision < 0)
      throw Error(`${label}: invalid ownership revision`);
    const holders = registry.active.filter(x => Array.isArray(x.resources) && x.resources.includes(resource));
    if (holders.length !== 1) throw Error(`${label}: conflicting ${resource} reservations`);
    return w;
  };
  const queued = find(snapshot, 'Run snapshot'), current = find(live, 'Live main');
  if (queued.owner !== current.owner ||
      (queued.ownership_revision ?? 0) !== (current.ownership_revision ?? 0) ||
      JSON.stringify(queued.scope ?? []) !== JSON.stringify(current.scope ?? []))
    throw Error('Run predates an ownership/scope handoff; dispatch from the new claimed main commit');
  return { id, owner: current.owner, revision: current.ownership_revision ?? 0, resource };
}


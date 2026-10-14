// Read the live canonical registry. This cannot grant or take a shared runner lease.
import fs from 'node:fs';
const registry = JSON.parse(fs.readFileSync(process.argv[2], 'utf8'));
const id = 'world4x-zone1-isolated-reference-prototype-20261013';
const active = registry.active || [];
const own = active.find(w => w.id === id);
const other = active.filter(w => w.id !== id && (w.resources || []).includes('windows-runner-heavy'));
if (other.length || !own || !(own.resources || []).includes('windows-runner-heavy')) {
  console.error(JSON.stringify({state: 'ZONE1_NATIVE_RESOURCE_BLOCKED',
    ownWindowsLease: !!own?.resources?.includes('windows-runner-heavy'),
    holders: other.map(w => ({id: w.id, owner: w.owner})),
    unityStarted: false, reason: 'A coordinated live lease is required; idle jobs do not release a retained claim.'}));
  process.exit(1);
}
console.log('ZONE1_EXCLUSIVE_LIVE_NATIVE_WINDOW_VERIFIED');

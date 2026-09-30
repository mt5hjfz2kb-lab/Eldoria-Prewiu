// v0.24 compatibility migration only.
// Core dialogue and gameplay now live in v0220/index.html.
(() => {
  const migrationKey = 'eldoria-hotfix-quarry-4200-v1';
  if (localStorage.getItem(migrationKey)) return;
  const api = window.ELDORIA_V023;
  if (!api || typeof api.state !== 'function' || typeof api.setQA !== 'function') return;
  const state = api.state();
  if (!state || state.graniteQuarry || state.bastionLevel > 5) {
    localStorage.setItem(migrationKey, 'skipped');
    return;
  }
  api.setQA({quarryRemain: Number(state.quarryRemain || 0) + 700});
  localStorage.setItem(migrationKey, 'applied');
})();

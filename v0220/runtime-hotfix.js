// Runtime bridge for the active v0.24 playtest milestone.
// The late Arc I runtime calls dialogue() from its own script scope; expose the
// Aldric cinematic helper globally until the canonical scripts are consolidated.
window.dialogue = function(name, copy, done) {
  if (name !== 'Sir Aldric') {
    if (typeof done === 'function') done();
    return;
  }
  document.querySelectorAll('.aldric-cinematic').forEach(x => x.remove());
  const full = String(copy || '').replace(/<[^>]+>/g, ' ');
  const o = document.createElement('div');
  o.className = 'aldric-cinematic';
  o.dataset.scene = 'aldric-state-dialogue';
  o.setAttribute('role', 'dialog');
  o.setAttribute('aria-modal', 'true');
  o.innerHTML = '<div class="aldric-dialogue"><div class="aldric-head"><div class="aldric-portrait"><img src="data:image/jpeg;base64,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" alt="Sir Aldric"></div><div><div class="aldric-name">SIR ALDRIC</div><div class="aldric-role">Guardián de Valoria</div></div></div><div class="aldric-copy"><div class="aldric-ghost"></div><div class="aldric-typed"></div></div><div class="aldric-wait">ESCUCHA…</div><button class="aldric-continue primary" type="button">Continuar</button></div>';
  document.body.appendChild(o);
  document.documentElement.style.overflow = 'hidden';
  const box = o.querySelector('.aldric-dialogue');
  const ghost = o.querySelector('.aldric-ghost');
  const typed = o.querySelector('.aldric-typed');
  const btn = o.querySelector('.aldric-continue');
  ghost.textContent = full;
  let n = 0;
  (function type() {
    if (n < full.length) {
      typed.textContent = full.slice(0, ++n);
      setTimeout(type, full[n - 1] === '.' ? 130 : full[n - 1] === ',' ? 65 : 22);
    } else {
      typed.textContent = full;
      box.classList.add('done');
      btn.focus();
    }
  })();
  btn.onclick = () => {
    if (!box.classList.contains('done')) return;
    o.remove();
    document.documentElement.style.overflow = '';
    if (typeof done === 'function') done();
  };
};

// One-time economy migration: the pre-Stoneworks quarry reserve introduced in
// the current baseline still leaves a fresh save short of the Stoneworks build.
// Add one extra 700-stone load once per save/browser without creating a
// repeatable reload exploit. This preserves the current gameplay direction and
// only removes the hard progression deadlock found by the uninterrupted QA run.
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
  const current = Number(state.quarryRemain || 0);
  api.setQA({quarryRemain: current + 700});
  localStorage.setItem(migrationKey, 'applied');
})();

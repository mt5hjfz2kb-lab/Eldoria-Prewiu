// Runtime bridge for the active v0.22 slice.
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
  o.innerHTML = '<div class="aldric-dialogue"><div class="aldric-head"><div class="aldric-portrait"><span style="font-size:48px">♜</span></div><div><div class="aldric-name">SIR ALDRIC</div><div class="aldric-role">Guardián de Valoria</div></div></div><div class="aldric-copy"><div class="aldric-ghost"></div><div class="aldric-typed"></div></div><div class="aldric-wait">ESCUCHA…</div><button class="aldric-continue primary" type="button">Continuar</button></div>';
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

import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';

// Zero-network lifecycle probe: no real game save or personal information.
const script = readFileSync(new URL('./optional-iphone-lifecycle-diagnostics.js', import.meta.url), 'utf8');
function mockNode() {
  return {
    style: {}, children: [], textContent: '', type: '', readyState: '',
    appendChild(child) { this.children.push(child); },
    addEventListener(name, fn) { (this.listeners ||= {})[name] = fn; }
  };
}
function browser(query, store) {
  const events = {}, documentEvents = {};
  const storage = {
    getItem: key => store.has(key) ? store.get(key) : null,
    setItem: (key, value) => store.set(key, value),
    removeItem: key => store.delete(key)
  };
  const window = {
    location: { search: query, hash: '' }, localStorage: storage,
    innerWidth: 390, innerHeight: 844, __eldoriaDiagnosticRevision: 'native281',
    addEventListener(name, fn) { events[name] = fn; }
  };
  const document = {
    readyState: 'complete', body: mockNode(), visibilityState: 'visible',
    createElement: () => mockNode(),
    addEventListener(name, fn) { documentEvents[name] = fn; }
  };
  const performance = { now: () => 40, getEntriesByType: () => [{ type: 'reload' }] };
  vm.runInNewContext(script, { window, document, performance, Date, JSON, String, RegExp, Array, Math, Object });
  return { window, events, documentEvents, document };
}

const store = new Map();
let inactive = browser('', store);
assert.equal(inactive.window.EldoriaLocalDiagnostic, undefined, 'default must be dormant');
assert.equal(store.size, 0, 'default must not write localStorage');

const active = browser('?eldoria_diag=1', store);
assert.equal(active.window.EldoriaLocalDiagnostic.enabled, true);
const api = active.window.EldoriaLocalDiagnostic;
assert.equal(api.read()[0].kind, 'boot');
assert.equal(api.read()[0].detail, 'reload');
active.events.error({ message: 'out of memory; private https://example.invalid/token' });
assert.equal(api.read().at(-1).detail, 'memory');
assert.doesNotMatch(JSON.stringify(api.read()), /private|example\.invalid|token/);
active.documentEvents.webglcontextlost({});
active.events.pagehide({ persisted: false });
assert.equal(api.read().at(-1).kind, 'pagehide');
for (let i = 0; i < 100; i++) active.events.resize({});
assert.equal(api.read().length, 48, 'bounded history');

const recovered = browser('', store);
assert.equal(recovered.window.EldoriaLocalDiagnostic.enabled, true, 'opt-in should survive a reload');
assert.equal(recovered.window.EldoriaLocalDiagnostic.read().length, 48);
const off = browser('?eldoria_diag=0', store);
assert.equal(off.window.EldoriaLocalDiagnostic, undefined, 'opt-out must not attach listeners');
assert.equal(store.get('eldoria.lifecycle.enabled.v1'), '0');
console.log('PASS: default dormant; reload persistence; bounded 48 events; no raw error data; explicit opt-out');

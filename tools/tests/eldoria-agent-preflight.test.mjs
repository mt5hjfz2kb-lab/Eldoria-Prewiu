import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {execFileSync} from 'node:child_process';

const script = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', 'eldoria-agent-preflight.mjs');
function run(root) {
  const result = execFileSync(process.execPath, [script, '--root', root, '--no-probe'], {encoding: 'utf8', timeout: 5000});
  return JSON.parse(result);
}
function fixture() {
  const root = fs.mkdtempSync(path.join(os.tmpdir(), 'eldoria-preflight-'));
  fs.mkdirSync(path.join(root, 'pipeline'), {recursive: true});
  fs.mkdirSync(path.join(root, '.github', 'workflows'), {recursive: true});
  fs.writeFileSync(path.join(root, 'AGENTS.md'), '# agent rules\n');
  return root;
}
test('declares a configured workflow without asserting a live runner or local tools', t => {
  const root = fixture();
  t.after(() => fs.rmSync(root, {recursive: true, force: true}));
  fs.writeFileSync(path.join(root, '.github', 'workflows', 'unity.yml'), 'runs-on: [self-hosted, windows, unity-6000-3-23f1]\n');
  fs.writeFileSync(path.join(root, '.github', 'workflows', 'artist.yml'), 'name: Qwen Blender Ollama\nruns-on: ubuntu-latest\n');
  fs.writeFileSync(path.join(root, 'pipeline', 'active-workstreams.json'),
    JSON.stringify({owner_production_pause: {active: true}}));
  const result = run(root);
  assert.equal(result.checkout.agents_md_present, true);
  assert.equal(result.routes.windows_unity.status, 'declared_not_verified_live');
  assert.equal(result.routes.ai_artist_blender.status, 'declared_not_verified_live');
  assert.equal(result.local_tools.unity.status, 'not_tested');
  assert.equal(result.local_tools.ollama_http.status, 'not_tested');
  assert.equal(result.owner_production_pause, 'active');
  assert.match(result.conclusion, /NOT proof/);
});
test('unknown evidence stays unknown; no phantom Windows or remote capability', t => {
  const root = fixture();
  t.after(() => fs.rmSync(root, {recursive: true, force: true}));
  const result = run(root);
  assert.equal(result.routes.windows_unity.status, 'not_found');
  assert.equal(result.routes.ai_artist_blender.status, 'not_found');
  assert.equal(result.owner_production_pause, 'unknown');
});

import fs from 'node:fs';
import crypto from 'node:crypto';

const cfgPath = 'pipeline/golden-generator-bakeoff-v1.json';
const manifestPath = 'pipeline/exact-inputs/valoria-golden-generator-bakeoff-v1/manifest.json';

const fail = (m) => { console.error('GOLDEN BAKE-OFF PREFLIGHT FAIL:', m); process.exitCode = 1; };
const cfg = JSON.parse(fs.readFileSync(cfgPath,'utf8'));
const manifest = JSON.parse(fs.readFileSync(manifestPath,'utf8'));

if (cfg.status !== 'prepared_zero_spend') fail('config status must be prepared_zero_spend');
if (cfg.execution_authorized !== false) fail('execution_authorized must remain false during zero-spend prep');
if (cfg.max_authorized_credits !== 0) fail('max_authorized_credits must remain 0 during zero-spend prep');
if (!cfg.safety?.no_vendor_calls_in_preflight) fail('preflight must prohibit vendor calls');
if ((cfg.safety?.tripo_credits_currently_authorized ?? -1) !== 0) fail('Tripo authorization must be 0');
if ((cfg.safety?.meshy_credits_currently_authorized ?? -1) !== 0) fail('Meshy authorization must be 0');

const ids = new Set((cfg.candidates||[]).map(x=>x.id));
for (const required of ['tripo-p1','meshy-7.1-2k']) if (!ids.has(required)) fail('missing candidate '+required);

const roles = ['front','left','back','right'];
const byRole = new Map((manifest.views||[]).map(v=>[v.role,v]));
for (const role of roles) if (!byRole.has(role)) fail('manifest missing role '+role);

let exactReady = true;
for (const role of roles) {
  const v = byRole.get(role);
  if (!v.path || !v.sha256 || !v.bytes || !v.width || !v.height || !v.format) {
    exactReady = false;
    continue;
  }
  if (!fs.existsSync(v.path)) { fail(role+': file path does not exist: '+v.path); exactReady=false; continue; }
  const buf=fs.readFileSync(v.path);
  const sha=crypto.createHash('sha256').update(buf).digest('hex');
  if (sha !== v.sha256) fail(role+': sha256 mismatch');
  if (buf.length !== v.bytes) fail(role+': byte-size mismatch');
}

if (manifest.execution_authorized !== false) fail('exact-input manifest execution_authorized must remain false during prep');

const summary = {
  preflight: process.exitCode ? 'FAIL' : 'PASS',
  zero_spend_guard: cfg.execution_authorized === false && cfg.max_authorized_credits === 0,
  exact_multiview_ready: exactReady,
  input_status: manifest.status,
  candidates: cfg.candidates.map(x=>({id:x.id,expected_credits:x.expected_credits})),
  expected_total_credits: cfg.candidates.reduce((a,x)=>a+(x.expected_credits||0),0),
  next: exactReady
    ? 'OWNER REVIEW OF EXACT FOUR-VIEW PACKAGE + FRESH CREDIT AUTHORIZATION'
    : 'CREATE/PERSIST CONSISTENT EXACT FOUR-VIEW PACKAGE; NO VENDOR CALLS'
};

fs.mkdirSync('docs/evidence/valoria-golden-lookdev-slice-v1/generator-bakeoff-v1',{recursive:true});
fs.writeFileSync('docs/evidence/valoria-golden-lookdev-slice-v1/generator-bakeoff-v1/preflight.json',JSON.stringify(summary,null,2)+'\n');
console.log(JSON.stringify(summary,null,2));

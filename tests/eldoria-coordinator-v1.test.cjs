const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const {spawnSync} = require('node:child_process');
const coordinator = path.resolve(__dirname, '../tools/eldoria-coordinator-v1.cjs');
function evaluate(record, active=true) {
  const cwd = fs.mkdtempSync(path.join(os.tmpdir(), 'eldoria-coordinator-'));
  try {
    fs.mkdirSync(path.join(cwd, 'pipeline'));
    const registry = {active: active ? [record] : [], history: active ? [] : [record]};
    fs.writeFileSync(path.join(cwd, 'pipeline/active-workstreams.json'), JSON.stringify(registry));
    const run = spawnSync(process.execPath,[coordinator],{cwd,encoding:'utf8'});
    assert.equal(run.status,0,run.stderr);
    const persisted = JSON.parse(fs.readFileSync(path.join(cwd,'pipeline/reports/coordinator-v1-latest.json'),'utf8'));
    assert.deepEqual(persisted,JSON.parse(run.stdout));
    return persisted;
  } finally { fs.rmSync(cwd,{recursive:true,force:true}); }
}
const complete = () => ({id:'m07-r1-sawmill-construction-visual-correction',status:'completed',runner_released:true,result:{post_patch_capture_reviewed:true,post_patch_capture_artifact_id:123,post_patch_capture_source_sha:'abc',independent_visual_pass:true,visual_review_evidence:'review',published_webgl_verified:true,published_probe_run_id:456,published_source_sha:'abc'}});
test('realistic blocked M07 stays blocked',()=>{
  const o=evaluate({...complete(),status:'blocked',result:{}});
  assert.equal(o.verdict,'BLOCKED_NO_HANDOFF');
  assert.equal(o.checks.filter(x=>x.passed).length,2);
});
test('complete with all explicit evidence allows only review',()=>{
  const o=evaluate(complete(),false);
  assert.equal(o.verdict,'READY_FOR_QA_HANDOFF_REVIEW');
  assert.equal(o.checks.filter(x=>x.passed).length,6);
});
test('no visual certification blocks',()=>{
  const r=complete();r.result.independent_visual_pass=false;
  assert.equal(evaluate(r,false).verdict,'BLOCKED_NO_HANDOFF');
});
test('still active despite completed label blocks',()=>assert.equal(evaluate(complete(),true).verdict,'BLOCKED_NO_HANDOFF'));
test('missing WebGL proof blocks',()=>{
  const r=complete();delete r.result.published_probe_run_id;
  assert.equal(evaluate(r,false).verdict,'BLOCKED_NO_HANDOFF');
});
test('missing record fails closed',()=>{
  const cwd=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-coordinator-'));
  try {
    fs.mkdirSync(path.join(cwd,'pipeline'));
    fs.writeFileSync(path.join(cwd,'pipeline/active-workstreams.json'),JSON.stringify({active:[],history:[]}));
    const run=spawnSync(process.execPath,[coordinator],{cwd,encoding:'utf8'});
    assert.equal(run.status,0,run.stderr);
    assert.equal(JSON.parse(run.stdout).verdict,'BLOCKED_NO_HANDOFF');
  } finally {fs.rmSync(cwd,{recursive:true,force:true});}
});

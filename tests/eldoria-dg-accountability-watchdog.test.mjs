import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {spawnSync} from 'node:child_process';
const script=path.resolve('tools/eldoria-workstream-watchdog.mjs');
const base={schema_version:1,directorate_accountability:{owner:'D01-Direccion-General'},active:[{id:'production',owner:'existing-department-executor',department_executor_owner:'existing-department-executor',accountable_director:'D01-Direccion-General',status:'active',claimed_at:'2026-10-09T09:00:00Z',resources:['unity-runner']}]};
function run(data){const dir=fs.mkdtempSync(path.join(os.tmpdir(),'eldoria-dg-'));try{const file=path.join(dir,'registry.json');fs.writeFileSync(file,JSON.stringify(data));const r=spawnSync(process.execPath,[script,file],{env:{...process.env,ELDORIA_CHECK_TIME:'2026-10-09T10:00:00Z'},encoding:'utf8'});return {code:r.status,stderr:r.stderr,report:r.status===0?JSON.parse(r.stdout):null};}finally{fs.rmSync(dir,{recursive:true,force:true});}}
test('accountability retained with original executor and resources',()=>{const r=run(base);assert.equal(r.code,0,r.stderr);assert.deepEqual(r.report.findings,[]);});
test('missing DG claim emits explicit actionable finding without stealing department',()=>{const x=structuredClone(base);delete x.active[0].accountable_director;const r=run(x);assert.equal(r.code,0,r.stderr);assert.equal(r.report.findings[0].status,'DIRECTORATE_UNASSIGNED');assert.equal(r.report.findings[0].owner,'existing-department-executor');});
test('shared runner collision is not silently tolerated',()=>{const x=structuredClone(base);x.active.push({...x.active[0],id:'another',owner:'other-executor',department_executor_owner:'other-executor'});const r=run(x);assert.equal(r.code,0,r.stderr);assert(r.report.findings.some(f=>f.status==='RESOURCE_CONFLICT'));});
test('absent top-level director governance fails closed',()=>{const x=structuredClone(base);delete x.directorate_accountability;const r=run(x);assert.notEqual(r.code,0);assert.match(r.stderr,/DIRECTORATE_ACCOUNTABILITY_MISSING/);});

test('Unity player production cannot pass on source commits or WebGL baseline alone',()=>{const x=structuredClone(base);x.active[0].id='r2-b-strategic-choice';x.active[0].delivery_kind='unity_player_facing';x.active[0].result={webgl_candidate_pass:true,unity_tech_pass:true};const r=run(x);assert.equal(r.code,0,r.stderr);assert(r.report.findings.some(f=>f.status==='UNITY_PLAYER_PRODUCTION_OPEN'));});
test('verified Unity and independent experience remove incomplete production finding',()=>{const x=structuredClone(base);x.active[0].id='r2-b-strategic-choice';x.active[0].delivery_kind='unity_player_facing';x.active[0].result={unity_source_sha:'a'.repeat(40),unity_integrated_build_run_id:123,independent_gameplay_accepted:true,visual_experience_accepted:true,regression_accepted:true};const r=run(x);assert.equal(r.code,0,r.stderr);assert(!r.report.findings.some(f=>f.status==='UNITY_PLAYER_PRODUCTION_OPEN'));});

test('support-only job does not falsely require a Unity executable',()=>{const x=structuredClone(base);x.active[0].delivery_kind='support_only';const r=run(x);assert.equal(r.code,0,r.stderr);assert(!r.report.findings.some(f=>f.status==='UNITY_PLAYER_PRODUCTION_OPEN'));});

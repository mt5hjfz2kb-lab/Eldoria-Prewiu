import './cycle-hooks.test.mjs';
import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {validate,appendPending,comparisonGroups,report,brief} from './learning.mjs';
const dir=path.dirname(fileURLToPath(import.meta.url));
const catalog=JSON.parse(fs.readFileSync(path.join(dir,'observations.json'),'utf8'));
const fresh=()=>structuredClone(catalog);
test('live historical source records internally consistent',()=>{
 assert.deepEqual(validate(catalog),[]);
 assert.equal(catalog.cases.length,6);
 assert.ok(catalog.cases.every(x=>x.art_approved===false));
});
test('duplicate case and forged visual pass rejected',()=>{
 const c=fresh();c.cases.push(structuredClone(c.cases[0]));
 assert.match(validate(c).join(' | '),/duplicate case/);
 const d=fresh();d.cases[0].art_approved=true;
 assert.match(validate(d).join(' | '),/false ART PASS/);
});
test('pixel and mesh claims must match real measurement tier',()=>{
 const d=fresh();d.cases[0].visibility_gate='REVIEW_REQUIRED';
 assert.match(validate(d).join(' | '),/visibility contradicts/);
 const e=fresh();e.cases[5].visibility_gate='REVIEW_REQUIRED';
 assert.match(validate(e).join(' | '),/mesh cannot certify visibility/);
});
test('new event is only pending verification and append-only',()=>{
 const d=fresh(),e=structuredClone(d.cases[0]);
 e.id='eldoria-bastion1-provisional';e.source.evidence_level='pending_verification';
 e.art_verdict='ART_UNREVIEWED';
 const n=appendPending(d,e);
 assert.equal(n.cases.length,7);assert.equal(d.cases.length,6);
 assert.throws(()=>appendPending(n,e),/Duplicate/);
 e.id='eldoria-bastion1-forged';e.source.evidence_level='run_log_reviewed';
 assert.throws(()=>appendPending(d,e),/pending_verification/);
});
test('no accidental promotion of unreviewed case',()=>{
 const d=fresh();d.cases[4].art_verdict='ART_REJECTED';
 assert.match(validate(d).join(' | '),/cannot reject aesthetic/);
 const e=fresh();e.cases[0].source.evidence_level='pending_verification';
 assert.match(validate(e).join(' | '),/unverified record/);
});
test('report is source-cited, distinguishes technical change from aesthetic quality',()=>{
 const r=report(catalog);
 assert.match(r,/38358105789/);
 assert.match(r,/38405267853/);
 assert.match(r,/Aesthetic trend: UNDETERMINED/);
});
test('learning brief retains original failures even beyond the latest four cases',()=>{
 const t=brief(catalog);
 assert.match(t,/small-sculpt-repeated/);
 assert.match(t,/frustum-parity/);
 assert.match(t,/preserve-playable-protected-terrain/);
 assert.match(t,/not model-weight training/);
});
test('never compare different camera frustum or resolution as a quality trend',()=>{
 const g=comparisonGroups(catalog);
 assert.equal(g.reduce((n,x)=>n+x.length,0),5);
 assert.ok(g.some(x=>x.length===3&&x[0].run_id===38358105789));
 assert.ok(g.some(x=>x.length===1&&x[0].run_id===38405267853));
});
test('pin reference across all records and require real run URL',()=>{
 const d=fresh();d.cases[0].reference_sha256='a'.repeat(64);
 assert.match(validate(d).join(' | '),/mismatched reference/);
 const e=fresh();e.cases[0].source.url='https://example.com';
 assert.match(validate(e).join(' | '),/URL/);
});

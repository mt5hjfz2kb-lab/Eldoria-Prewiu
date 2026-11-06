import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {documentTriage,markdown} from './report.mjs';

const here=path.dirname(fileURLToPath(import.meta.url));
const root=path.resolve(here,'../..');
test('real catalog produces an advisory-only, complete priority queue',()=>{
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'docs/documentation-catalog.json')));
  const report=documentTriage(catalog);
  assert.equal(report.read_only,true);
  assert.equal(report.total,report.by_status.current+report.by_status.reference+
    report.by_status.review_pending+report.by_status.historical);
  assert.equal(report.pending_count,report.priority_review_queue.length);
  const pendingPaths=catalog.entries.filter(e=>e.status==='review_pending').map(e=>e.path).sort();
  assert.equal(report.pending_count,pendingPaths.length);
  assert.deepEqual(report.priority_review_queue.map(e=>e.path).sort(),pendingPaths);
  assert.equal(new Set(report.priority_review_queue.map(e=>e.path)).size,report.pending_count);
  assert.match(markdown(report),/sin re-clasificarlos automáticamente/);
});
test('a fully classified catalog is valid, rather than failing an outdated minimum count',()=>{
  const catalog={schema_version:1,entries:[
    {path:'docs/current.md',domain:'quality',status:'current'},
    {path:'docs/reference.md',domain:'art',status:'reference'},
    {path:'docs/historical.md',domain:'history',status:'historical'}
  ]};
  const report=documentTriage(catalog);
  assert.equal(report.read_only,true);
  assert.equal(report.total,3);
  assert.equal(report.pending_count,0);
  assert.deepEqual(report.priority_review_queue,[]);
  assert.match(markdown(report),/pendientes 0/);
  assert.equal(catalog.entries[0].status,'current');
});

test('operational unknowns first, preserving historical sources and all pending entries',()=>{
  const c={schema_version:1,entries:[
    {path:'docs/art.md',domain:'art',status:'review_pending'},
    {path:'docs/automation.md',domain:'automation',status:'review_pending'},
    {path:'docs/history.md',domain:'history',status:'historical'}]};
  const r=documentTriage(c);
  assert.equal(r.priority_review_queue[0].path,'docs/automation.md');
  assert.equal(r.by_status.historical,1);
  assert.equal(r.total,3);
  assert.equal(c.entries[0].status,'review_pending');
});
test('invalid or duplicate entries fail instead of acquiring authority',()=>{
  const seed={schema_version:1,entries:[{path:'docs/ok.md',domain:'art',status:'current'}]};
  assert.throws(()=>documentTriage({...seed,entries:[...seed.entries,...seed.entries]}),/Invalid\/duplicate/);
  assert.throws(()=>documentTriage({...seed,entries:[{...seed.entries[0],status:'art_approved'}]}),/Invalid\/duplicate/);
  assert.throws(()=>documentTriage({...seed,entries:[{...seed.entries[0],path:'../secrets'}]}),/Invalid\/duplicate/);
});
